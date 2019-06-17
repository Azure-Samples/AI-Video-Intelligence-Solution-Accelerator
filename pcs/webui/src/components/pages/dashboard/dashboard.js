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
  ExamplePanel,
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
  telemetryImage: null,

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

  componentDidMount() {
    // Ensure the rules are loaded
    this.refreshRules();

    // Telemetry stream - START
    const onPendingStart = () => this.setState({ telemetryIsPending: true });

    const getTelemetryStream = ({ deviceIds = [] }) => TelemetryService.getTelemetryByDeviceIdP15M(deviceIds)
      .map(response => ({ initial: response }) )
      .merge(
        this.telemetryRefresh$ // Previous request complete
          .delay(Config.telemetryRefreshInterval) // Wait to refresh
          .do(onPendingStart)
          .flatMap(_ => TelemetryService.getTelemetryByDeviceIdP1M(deviceIds))
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
              // On the 1Minute updates we merge the existing messages
              // with the newer ones.
              : this.mergeAndTrimTelemetryMessages(
                  this.state.telemetry, telemetryState.telemetry.update);
            const imageMessage = telemetry.length > 0
              ? telemetry.find(x => x.data.hasOwnProperty('cameraId') && x.data.cameraId === this.props.activeCameraId && x.messageSchema === 'image-upload:v1')
              : null;
            if (imageMessage) {
              // ignore poorly formed messages
              if (imageMessage.data.cameraId && imageMessage.data.time && imageMessage.data.type) {
                const imageUrl = imageMessage.data.cameraId + "/"
                  + imageMessage.data.time + "."
                  + imageMessage.data.type;
                const getImageUrl = TelemetryService.getBlobAccessUrl(imageUrl);
                getImageUrl.subscribe(
                  (result) => {
                    const newImage = {
                      'url': result,
                      'cameraId': imageMessage.data.cameraId,
                      'time': imageMessage.data.time,
                      'type': imageMessage.data.type
                    }
                    this.setState( { telemetryImage: newImage } );
                    // filter out telemetry messages of schema recognition:v1 with the same camera and time
                    const boundingBoxes = telemetry.filter(x => x.messageSchema === 'recognition:v1' && x.data.cameraId === imageMessage.data.cameraId && x.data.time === imageMessage.data.time);
                    this.setState( { telemetryBoundingBoxes: boundingBoxes } );
                  },
                  error => {
                    this.setState( { telemetryImage: null } );
                    this.setState( { telemetryBoundingBoxes: null } );
                  }
                );
                          } else {
                this.setState( { telemetryImage: null } );
                this.setState( { telemetryBoundingBoxes: null } );
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
      telemetryImage,
      telemetryBoundingBoxes,

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
                    image= { telemetryImage }
                    boundingBoxes= { telemetryBoundingBoxes }
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
            {
              Config.showWalkthroughExamples &&
              <Cell className="col-4">
                <ExamplePanel t={t} />
              </Cell>
            }
          </Grid>
        </PageContent>
      </ComponentArray>
    );
  }
}
