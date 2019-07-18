// Copyright (c) Microsoft. All rights reserved.

import React, { Component } from 'react';
import { Observable, Subject } from 'rxjs';
import moment from 'moment';

import Config from 'app.config';
import { TelemetryService } from 'services';
import { permissions } from 'services/models';
import { compareByProperty, getIntervalParams, retryHandler } from 'utilities';
import { Grid, Cell } from './grid';
import { DeviceGroupDropdownContainer as DeviceGroupDropdown } from 'components/shell/deviceGroupDropdown';
import { ManageDeviceGroupsBtnContainer as ManageDeviceGroupsBtn } from 'components/shell/manageDeviceGroupsBtn';
import { TimeIntervalDropdownContainer as TimeIntervalDropdown } from 'components/shell/timeIntervalDropdown';
import {
  EventsPanelContainer as EventsPanel,
  InsightsPanelContainer as InsightsPanel,
  RulesPanelContainer as RulesPanel,
} from './panels';
import {
  ComponentArray,
  ContextMenu,
  ContextMenuAlign,
  PageContent,
  Protected,
  RefreshBarContainer as RefreshBar
} from 'components/shared';

import './dashboard.scss';

const initialState = {
  // Telemetry data
  telemetry: [],
  telemetryIsPending: true,
  telemetryError: null,

  telemetryImageReportCurrent: null,
  telemetryBlobUrlRetrievalPending: false,

  // Analytics data
  analyticsVersion: 0,
  currentActiveAlerts: [],
  topAlerts: [],
  alertsPerDeviceId: {},
  criticalAlertsChange: 0,
  analyticsIsPending: true,
  analyticsError: null,

  // Summary data
  openWarningCount: undefined,
  openCriticalCount: undefined,

  // Map data
  devicesInAlert: {},

  lastRefreshed: undefined
};

const refreshEvent = (deviceIds = [], timeInterval) => ({ deviceIds, timeInterval });

const { retryWaitTime, maxRetryAttempts } = Config;

export class Dashboard extends Component {

  constructor(props) {
    super(props);

    this.state = initialState;

    this.subscriptions = [];
    this.dashboardRefresh$ = new Subject(); // Restarts all streams
    this.telemetryRefresh$ = new Subject();
    this.panelsRefresh$ = new Subject();

    this.props.updateCurrentWindow('Dashboard');
  }

  mergeAndTrimTelemetryMessages = (messages, updates) => {
    // messages and updates arrive in descending order
    // There is a mismatch in how Azure returns time strings and moment.toJSON(),
    // but the mismatch is only in the time zone spec, so it won't break the comparison.
    const oldAgeLimit = moment().subtract(15, 'm').toJSON();
    if (updates.length > 0) {
        // Remove all of the old messages that are newer than the oldest update message
      const updateTimeLimit = updates[updates.length - 1].time;
      const timeFilteredMessages = messages.filter(
        item => item.time < updateTimeLimit && item.time > oldAgeLimit);
      const result = [...updates, ...timeFilteredMessages];
      return result;
    }
    // If there are no updates, just filter old messages out of the original array
    return messages.filter(item => item.time > oldAgeLimit);
  }

  // Early versions of the message schemas ('image-upload;v1') erroneously had ':' as
  // separators instead of ';' so the preferred full string comparison would fail.
  matchesSchema = (message, schema) => {
    return message.messageSchema.startsWith(schema);
  }

  // TSI query results can be up to 30 or 40 seconds out-of-date, which would cause
  // some images to be skipped. This function ensures that each reported image
  // gets a chance to be displayed in the order it was reported.
  // Returns an "imageReport", which is an object containing the image message
  // and its associated recognition images.
  getNextImageReportToDisplay = (cameraId, allTelemetry, currentReport) => {
    const foundReports = [];
    if (!!allTelemetry && !!cameraId) {
      const telemetry = allTelemetry.filter(message => message.data.cameraId === cameraId);
      let i, j;
      for (i = 0; i < telemetry.length; i++) {
        const message = telemetry[i];
        if (this.matchesSchema(message, 'image-upload')) {
          // If this message matches the currentReport, then there's no new report available
          if (!!currentReport && currentReport.image.data.time >= message.data.time) {
            break;
          }
          // Attempt to find the associated recognition messages
          const recognitionMessages = [];
          for (j = 0; j < telemetry.length; j++) {
            if (this.matchesSchema(telemetry[j], 'recognition') && message.data.time === telemetry[j].data.time) {
              recognitionMessages.push(telemetry[j]);
              if (recognitionMessages.length === message.data.featureCount) {
                break;
              }
            }
          }
          // Did we find a complete record?
          if (message.data.featureCount === recognitionMessages.length) {
            const report = {
              image: message,
              recognitions: recognitionMessages
            };
            foundReports.push(report);
            // If there's no currentReport then the report we just pushed
            // is the latest, which is what we want.
            if (!currentReport) {
              console.log("Initial report");
              break;
            }
          }
        }
      }
    }
    // When there's no currentReport, we're returning the most recent match. Otherwise,
    // we return the report that comes next in time after the currentReport (if it
    // exists yet.)
    return foundReports.length === 0 ? null : foundReports[foundReports.length - 1];
  }

  componentDidMount() {
    // Ensure the rules are loaded
    this.refreshRules();

    // Telemetry stream - START
    const onPendingStart = () => this.setState({ telemetryIsPending: true });

    const getTelemetryStream = ({ deviceIds = [] }) => TelemetryService.getTelemetryByDeviceIdP2M(deviceIds)
      .map(response => ({ initial: response }) )
      .merge(
        this.telemetryRefresh$ // Previous request complete
          .delay(Config.telemetryRefreshInterval) // Wait to refresh
          .do(onPendingStart)
          .flatMap(_ => TelemetryService.getTelemetryByDeviceIdP2M(deviceIds))
          .map(response => ({ update: response }) )
          )
      //.flatMap(transformTelemetryResponse(() => this.state.telemetry))
      .map(telemetry => ({ telemetry, telemetryIsPending: false })) // Stream emits new state
      // Retry any retryable errors
      .retryWhen(retryHandler(maxRetryAttempts, retryWaitTime));
    // Telemetry stream - END

    // Analytics stream - START

    // TODO: Add device ids to params - START
    const getAnalyticsStream = ({ deviceIds = [], timeInterval }) => this.panelsRefresh$
      .delay(Config.dashboardRefreshInterval)
      .startWith(0)
      .do(_ => this.setState({ analyticsIsPending: true }))
      .flatMap(_ => {
        const devices = deviceIds.length ? deviceIds.join(',') : undefined;
        const [currentIntervalParams, previousIntervalParams] = getIntervalParams(timeInterval);

        const currentParams = { ...currentIntervalParams, devices };
        const previousParams = { ...previousIntervalParams, devices };

        return Observable.forkJoin(
          TelemetryService.getActiveAlerts(currentParams),
          TelemetryService.getActiveAlerts(previousParams),

          TelemetryService.getAlerts(currentParams),
          TelemetryService.getAlerts(previousParams)
        )
      }).map(([
        currentActiveAlerts,
        previousActiveAlerts,

        currentAlerts,
        previousAlerts
      ]) => {
        // Process all the data out of the currentAlerts list
        const currentAlertsStats = currentAlerts.reduce((acc, alert) => {
          const isOpen = alert.status === Config.alertStatus.open;
          const isWarning = alert.severity === Config.ruleSeverity.warning;
          const isCritical = alert.severity === Config.ruleSeverity.critical;
          let updatedAlertsPerDeviceId = acc.alertsPerDeviceId;
          if (alert.deviceId) {
            updatedAlertsPerDeviceId = {
              ...updatedAlertsPerDeviceId,
              [alert.deviceId]: (updatedAlertsPerDeviceId[alert.deviceId] || 0) + 1
            };
          }
          return {
            openWarningCount: (acc.openWarningCount || 0) + (isWarning && isOpen ? 1 : 0),
            openCriticalCount: (acc.openCriticalCount || 0) + (isCritical && isOpen ? 1 : 0),
            totalCriticalCount: (acc.totalCriticalCount || 0) + (isCritical ? 1 : 0),
            alertsPerDeviceId: updatedAlertsPerDeviceId
          };
        },
          { alertsPerDeviceId: {} }
        );

        // ================== Critical Alerts Count - START
        const currentCriticalAlerts = currentAlertsStats.totalCriticalCount;
        const previousCriticalAlerts = previousAlerts.reduce(
          (cnt, { severity }) => severity === Config.ruleSeverity.critical ? cnt + 1 : cnt,
          0
        );
        const criticalAlertsChange = ((currentCriticalAlerts - previousCriticalAlerts) / currentCriticalAlerts * 100).toFixed(2);
        // ================== Critical Alerts Count - END

        // ================== Top Alerts - START
        const currentTopAlerts = currentActiveAlerts
          .sort(compareByProperty('count'))
          .slice(0, Config.maxTopAlerts);

        // Find the previous counts for the current top analytics
        const previousTopAlertsMap = previousActiveAlerts.reduce(
          (acc, { ruleId, count }) =>
            (ruleId in acc)
              ? { ...acc, [ruleId]: count }
              : acc
          ,
          currentTopAlerts.reduce((acc, { ruleId }) => ({ ...acc, [ruleId]: 0 }), {})
        );

        const topAlerts = currentTopAlerts.map(({ ruleId, count }) => ({
          ruleId,
          count,
          previousCount: previousTopAlertsMap[ruleId] || 0
        }));
        // ================== Top Alerts - END

        const devicesInAlert = currentAlerts
          .filter(({ status }) => status === Config.alertStatus.open)
          .reduce((acc, { deviceId, severity, ruleId }) => {
            return {
              ...acc,
              [deviceId]: { severity, ruleId }
            };
          }, {});

        return ({
          analyticsIsPending: false,
          analyticsVersion: this.state.analyticsVersion + 1,

          // Analytics data
          currentActiveAlerts,
          topAlerts,
          criticalAlertsChange,
          alertsPerDeviceId: currentAlertsStats.alertsPerDeviceId,

          // Summary data
          openWarningCount: currentAlertsStats.openWarningCount,
          openCriticalCount: currentAlertsStats.openCriticalCount,

          // Map data
          devicesInAlert
        });
      })
      // Retry any retryable errors
      .retryWhen(retryHandler(maxRetryAttempts, retryWaitTime));
    // Analytics stream - END

    this.subscriptions.push(
      this.dashboardRefresh$
        .subscribe(() => this.setState(initialState))
    );

    this.subscriptions.push(
      this.dashboardRefresh$
        .switchMap(getTelemetryStream)
        .subscribe(
          telemetryState => {
            const telemetry = telemetryState.telemetry.initial
              // On the initial request we just set the local state
              // to the returned array of telemetry messages
              ? telemetryState.telemetry.initial
              // On the 2 Minute updates replace the local state with the updates
              // with the newer ones.
              : telemetryState.telemetry.update;

            // Don't worry about updating when a Blob Url retrieval is pending
            if (!this.state.telemetryBlobUrlRetrievalPending) {
              // Ignore the telemetryImageReportCurrent if it doesn't match the current cameraId
              let currentReport = this.state.telemetryImageReportCurrent;
              if (!!currentReport && currentReport.image.data.cameraId !== this.props.activeCameraId) {
                this.setState( { telemetryImageReportCurrent: null} );
                currentReport = null;
              }
              const candidateReport = this.getNextImageReportToDisplay(this.props.activeCameraId, telemetry, currentReport);
              if (!!candidateReport) {
                // Always update if there's no current report
                let doImageUpdate = !currentReport;
                if (!!currentReport) {
                  // Display each image for at least a good fraction (80%) of the time between images
                  // Keep the image dwell time to 10 seconds or less
                  const imageDwellTimeMilliseconds = Math.min(10000, 0.8 * (Date.parse(candidateReport.image.data.time)
                    - Date.parse(currentReport.image.data.time)));
                  // Don't update the current report until the old one has been
                  // displayed for at least imageDwellTimeMilliseconds
                  if (Date.now().valueOf() > currentReport.displayStartMsecEpoch + imageDwellTimeMilliseconds) {
                    doImageUpdate = true;
                  }
                }
                if (doImageUpdate) {
                  candidateReport.displayStartMsecEpoch = Date.now().valueOf();
                  this.setState( {
                    telemetryBlobUrlRetrievalPending: true
                  } );
                  const imageUrl = candidateReport.image.data.cameraId + "/"
                    + candidateReport.image.data.time + "."
                    + candidateReport.image.data.type;
                  const getImageUrl = TelemetryService.getBlobAccessUrl(imageUrl);
                  getImageUrl.subscribe(
                    (result) => {
                      candidateReport.image.url = result;
                      console.log("Setting telemetryImageReportCurrent " + JSON.stringify(candidateReport));
                      this.setState( {
                        telemetryImageReportCurrent: candidateReport,
                        telemetryBlobUrlRetrievalPending: false
                      } );
                    },
                    error => {
                      console.log("Error fetching Blob access url: " + error);
                      console.log("Setting telemetryImageReportCurrent " + JSON.stringify(candidateReport));
                      candidateReport.image.url = null;
                      this.setState( {
                        telemetryImageReportCurrent: candidateReport,
                        telemetryBlobUrlRetrievalPending: false
                      } );
                    }
                  );
                }
              }
            }

            this.setState(
              {
                telemetry: telemetry,
                telemetryIsPending: telemetryState.telemetryIsPending,
                lastRefreshed: moment() },
              () => {
                // After the state update is complete, trigger a telemetry update call
                this.telemetryRefresh$.next('r')
              }
            )
          },
          telemetryError => this.setState({ telemetryError, telemetryIsPending: false })
        )
    );

    this.subscriptions.push(
      this.dashboardRefresh$
        .switchMap(getAnalyticsStream)
        .subscribe(
          analyticsState => this.setState(
            { ...analyticsState, lastRefreshed: moment() },
            () => this.panelsRefresh$.next('r')
          ),
          analyticsError => this.setState({ analyticsError, analyticsIsPending: false })
        )
    );

    // Start polling all panels
    if (this.props.deviceLastUpdated) {
      this.dashboardRefresh$.next(
        refreshEvent(
          Object.keys(this.props.devices || {}),
          this.props.timeInterval
        )
      );
    }
  }

  componentWillUnmount() {
    this.subscriptions.forEach(sub => sub.unsubscribe());
  }

  componentWillReceiveProps(nextProps) {
    if (nextProps.deviceLastUpdated !== this.props.deviceLastUpdated || nextProps.timeInterval !== this.props.timeInterval) {
      this.dashboardRefresh$.next(
        refreshEvent(
          Object.keys(nextProps.devices),
          nextProps.timeInterval
        ),
      );
    }
  }

  refreshDashboard = () => this.dashboardRefresh$.next(
    refreshEvent(
      Object.keys(this.props.devices),
      this.props.timeInterval
    )
  );

  refreshRules = () => {
    if (!this.props.rulesError && !this.props.rulesIsPending) this.props.fetchRules();
  }

  render() {
    const {
      activeCameraId,
      timeInterval,

      devicesIsPending,

      deviceGroups,

      rules,
      rulesError,
      rulesIsPending,
      t
    } = this.props;
    const {
      currentActiveAlerts,
      topAlerts,
      analyticsIsPending,
      analyticsError,

      telemetry,
      telemetryImageReportCurrent,

      lastRefreshed
    } = this.state;

    // Determine if the rules for all of the alerts are actually loaded.
    const unloadedRules =
      topAlerts.filter(alert => !rules[alert.ruleId]).length
      + currentActiveAlerts.filter(alert => !rules[alert.ruleId]).length;
    if (unloadedRules > 0) {
      // Fetch the rules since at least one alert doesn't know the name for its rule
      this.refreshRules();
    }

    const cameraList = telemetry.reduce(function(allItems, currentItem) {
      if(!allItems.includes(currentItem.data.cameraId))
        allItems.push(currentItem.data.cameraId);
      return allItems;
    }, []);

    return (
      <ComponentArray>
        <ContextMenu>
          <ContextMenuAlign left={true}>
            <DeviceGroupDropdown />
            <Protected permission={permissions.updateDeviceGroups}>
              <ManageDeviceGroupsBtn />
            </Protected>
          </ContextMenuAlign>
          <ContextMenuAlign>
            <TimeIntervalDropdown
              onChange={this.props.updateTimeInterval}
              value={timeInterval}
              t={t} />
            <RefreshBar
              refresh={this.refreshDashboard}
              time={lastRefreshed}
              isPending={analyticsIsPending || devicesIsPending}
              t={t} />
          </ContextMenuAlign>
        </ContextMenu>
        <PageContent className="dashboard-container">
          <Grid>
            <Cell className="col-5">
              <div className="innerGrid">
                <Grid>
                  <Cell  className="col-8">
                    <InsightsPanel
                    imageReport = { telemetryImageReportCurrent }
                    cameras= { cameraList }
                    error= {rulesError || analyticsError}
                    t={t} />
                  </Cell>
                  <Cell className="col-8">
                    <RulesPanel
                    error={rulesError || analyticsError}
                    t={t} />
                </Cell>
                </Grid>
              </div>
            </Cell>
            <Cell className="col-3">
              <EventsPanel
                events={activeCameraId && telemetry.length > 0 ? telemetry.filter(x => x.data.cameraId === activeCameraId): [] }
                isPending={analyticsIsPending || rulesIsPending}
                error={rulesError || analyticsError}
                t={t}
                deviceGroups={deviceGroups} />
            </Cell>
          </Grid>
        </PageContent>
      </ComponentArray>
    );
  }
}
