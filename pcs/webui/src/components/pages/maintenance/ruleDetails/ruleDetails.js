// Copyright (c) Microsoft. All rights reserved.

import React, { Component } from 'react';
import { Trans } from 'react-i18next';
import { Observable, Subject } from 'rxjs';

import Config from 'app.config';
import { permissions, toDiagnosticsModel } from 'services/models';
import { RulesGrid } from 'components/pages/rules/rulesGrid';
import {
  AjaxError,
  Btn,
  ComponentArray,
  ContextMenu,
  ContextMenuAlign,
  Indicator,
  PageContent,
  PageTitle,
  Protected,
  RefreshBarContainer as RefreshBar
} from 'components/shared';
import { svgs, joinClasses, renderUndefined } from 'utilities';
import { DevicesGridContainer } from 'components/pages/devices/devicesGrid/devicesGrid.container';
import { DeviceGroupDropdownContainer as DeviceGroupDropdown } from 'components/shell/deviceGroupDropdown';
import { ManageDeviceGroupsBtnContainer as ManageDeviceGroupsBtn } from 'components/shell/manageDeviceGroupsBtn';
import { TimeIntervalDropdownContainer as TimeIntervalDropdown } from 'components/shell/timeIntervalDropdown';
import { TelemetryChartContainer as TelemetryChart, transformTelemetryResponse, chartColorObjects } from 'components/pages/dashboard/panels/telemetry';
import { TelemetryService } from 'services';
import { TimeRenderer, SeverityRenderer } from 'components/shared/cellRenderers';
import { AlertOccurrencesGrid } from 'components/pages/maintenance/grids';
import { ROW_HEIGHT } from 'components/shared/pcsGrid/pcsGridConfig';

import './ruleDetails.scss';

const tabIds = {
  all: 'all',
  devices: 'devices',
  telemetry: 'telemetry'
};

const idDelimiter = ' ';

// TODO: For the PcsGrid, fix bug causing the selection to be lost when the grid data updates.
// TODO: Related, fix bug where the context buttons don't update when the on grid prop changes
export class RuleDetails extends Component {

  constructor(props) {
    super(props);

    this.state = {
      updatingAlertStatus: false,

      selectedAlerts: [],
      selectedRule: undefined,

      telemetryIsPending: true,
      telemetry: {},
      telemetryError: undefined,

      devices: [],
      deviceIds: '',
      occurrences: [],
      selectedTab: tabIds.all,

      ruleContextBtns: undefined,
      alertContextBtns: undefined,
      deviceContextBtns: undefined
    };

    this.restartTelemetry$ = new Subject();
    this.telemetryRefresh$ = new Subject();

    this.subscriptions = [];
  }

  componentDidMount() {
    // Telemetry stream - START
    const onPendingStart = () => this.setState({ telemetryIsPending: true });

    this.subscriptions.push(
      this.restartTelemetry$
        .distinctUntilChanged()
        .map(deviceIds => deviceIds.split(idDelimiter).filter(id => id))
        .do(() => this.setState({ telemetry: {}, telemetryIsPending: false }))
        .switchMap(deviceIds => {
          if (deviceIds.length > 0) {
            return TelemetryService.getTelemetryByDeviceIdP15M(deviceIds)
              .merge(
                this.telemetryRefresh$ // Previous request complete
                  .delay(Config.telemetryRefreshInterval) // Wait to refresh
                  .do(onPendingStart)
                  .flatMap(_ => TelemetryService.getTelemetryByDeviceIdP1M(deviceIds))
              )
              .flatMap(transformTelemetryResponse(() => this.state.telemetry))
              .map(telemetry => ({ telemetry, telemetryIsPending: false }))
          } else {
            return Observable.empty();
          }
        }
        )
        .subscribe(
          telemetryState => this.setState(
            telemetryState,
            () => this.telemetryRefresh$.next('r')
          ),
          telemetryError => this.setState({ telemetryError, telemetryIsPending: false })
        )
    );

    this.handleProps(this.props);
    this.props.logEvent(toDiagnosticsModel('AlertDetails_Click', {}));
  }

  componentWillReceiveProps(nextProps) {
    this.handleProps(nextProps);
  }

  handleProps(nextProps) {
    const {
      alerts,
      alertEntities,
      deviceEntities,
      match,
      rulesEntities
    } = nextProps;
    const selectedId = match.params.id;
    const selectedRule = rulesEntities[selectedId];
    const selectedAlert = alerts.filter(({ ruleId }) => ruleId === selectedId)[0] || {};
    const occurrences = (selectedAlert.alerts || [])
      .map(alertId => ({ ...alertEntities[alertId], name: selectedRule.name, severity: selectedRule.severity }));

    const deviceObjects = (occurrences || []).reduce(
      (acc, { deviceId }) => ({
        ...acc,
        [deviceId]: acc[deviceId] || deviceEntities[deviceId]
      }),
      {}
    );

    const deviceIds = Object.keys(deviceObjects);
    const devices = deviceIds.map(deviceId => deviceObjects[deviceId]);
    const deviceIdString = deviceIds.sort().join(idDelimiter);
    this.setState({
      deviceIds: deviceIdString,
      devices,
      occurrences,
      selectedAlert,
      selectedRule
    },
      () => this.restartTelemetry$.next(deviceIdString)
    );
  }

  componentWillUnmount() {
    this.subscriptions.forEach(sub => sub.unsubscribe());
  }

  updateAlertStatus = (selectedAlerts, status) =>
    this.subscriptions.push(
      Observable.of(selectedAlerts)
        .do(() => this.setState({ updatingAlertStatus: true }))
        .flatMap(alerts => alerts)
        .flatMap(({ id }) => TelemetryService.updateAlertStatus(id, status))
        .toArray() // Use toArray to wait for all calls to succeed
        .subscribe(
          () => {
            this.props.setAlertStatus(selectedAlerts, status);
            this.onAlertGridHardSelectChange([]);
          },
          undefined, // TODO: Handle error
          () => this.setState({ updatingAlertStatus: false })
        )
    );

  deleteAlerts = () => {
    this.setState({ updatingAlertStatus: true });
    const ids = this.state.selectedAlerts.map(x => x.id);
    this.subscriptions.push(
      TelemetryService.deleteAlerts(ids)
        .subscribe(
          () => { this.refreshData(); },
          undefined, // TODO: Handle error
          () => this.setState({ updatingAlertStatus: false })
        )
    )
    this.props.logEvent(toDiagnosticsModel('AlertDelete_Click', {}));
  }

  // TODO: Move constant values to central location
  closeAlerts = () => {
    this.props.logEvent(toDiagnosticsModel('AlertClose_Click', {}));
    return this.updateAlertStatus(this.state.selectedAlerts, Config.alertStatus.closed);
  }

  ackAlerts = () => {
    this.props.logEvent(toDiagnosticsModel('AlertAcknowledge_Click', {}));
    return this.updateAlertStatus(this.state.selectedAlerts, Config.alertStatus.acknowledged);
  }

  setTab = selectedTab => () => this.setState({ selectedTab })

  onRuleGridReady = gridReadyEvent => this.ruleGridApi = gridReadyEvent.api;
  onAlertGridReady = gridReadyEvent => this.alertGridApi = gridReadyEvent.api;
  onDeviceGridReady = gridReadyEvent => this.deviceGridApi = gridReadyEvent.api;

  onContextMenuChange = stateKey => contextBtns => this.setState({ [stateKey]: contextBtns });

  onHardSelectChange = gridName =>
    selectedRows => {
      if (selectedRows.length > 0) this.deselectOtherGrids(gridName);
    };

  onAlertGridHardSelectChange = selectedRows => {
    const alertContextBtns =
      selectedRows.length > 0
        ?
        <ComponentArray>
          <Protected permission={permissions.updateAlarms}>
            <Btn svg={svgs.closeAlert} onClick={this.closeAlerts}>
              <Trans i18nKey="maintenance.close">Close</Trans>
            </Btn>
          </Protected>
          <Protected permission={permissions.updateAlarms}>
            <Btn svg={svgs.ackAlert} onClick={this.ackAlerts}>
              <Trans i18nKey="maintenance.acknowledge">Acknowledge</Trans>
            </Btn>
          </Protected>
          <Protected permission={permissions.deleteAlarms}>
            <Btn svg={svgs.trash} onClick={this.deleteAlerts}>
              <Trans i18nKey="maintenance.delete">Delete</Trans>
            </Btn>
          </Protected>
        </ComponentArray>
        : null;
    this.setState({
      selectedAlerts: selectedRows,
      alertContextBtns
    });
    this.onHardSelectChange('alerts')(selectedRows);
  }

  deselectOtherGrids = gridName => {
    if (gridName !== 'rules' && this.ruleGridApi.getSelectedNodes().length > 0) {
      this.ruleGridApi.deselectAll();
    }
    if (gridName !== 'alerts' && this.alertGridApi.getSelectedNodes().length > 0) {
      this.alertGridApi.deselectAll();
    }
    if (gridName !== 'devices' && this.deviceGridApi.getSelectedNodes().length > 0) {
      this.deviceGridApi.deselectAll();
    }
  }

  refreshData = () => this.setState({
    ruleContextBtns: undefined,
    alertContextBtns: undefined,
    deviceContextBtns: undefined
  }, this.props.refreshData);

  render() {
    const {
      error,
      isPending,
      lastUpdated,
      match,
      theme,
      t,
      deviceGroups,
      onTimeIntervalChange,
      timeInterval
    } = this.props;
    const selectedId = match.params.id;
    const rule = isPending || !this.state.selectedRule ? undefined : [this.state.selectedRule];
    const alertName = (this.state.selectedRule || {}).name || selectedId;

    const alertsGridProps = {
      domLayout: 'autoHeight',
      rowSelection: 'multiple',
      deltaRowDataMode: true,
      getRowNodeId: ({ id }) => id,
      rowData: isPending ? undefined : this.state.occurrences,
      sizeColumnsToFit: true,
      pagination: true,
      paginationPageSize: Config.smallGridPageSize,
      onHardSelectChange: this.onAlertGridHardSelectChange,
      onGridReady: this.onAlertGridReady,
      onColumnMoved: this.props.onColumnMoved,
      onRowClicked: ({ node }) => {
        this.props.logEvent(toDiagnosticsModel('Alert_Click', {}));
        return node.setSelected(!node.isSelected());
      },
      t,
      deviceGroups
    };

    const { selectedTab, selectedAlert = {} } = this.state;
    const { counts = {} } = selectedAlert;
    return (
      <ComponentArray>
        <ContextMenu className="rule-details-context-menu-container">
          <ContextMenuAlign left={true}>
            <DeviceGroupDropdown />
            <Protected permission={permissions.updateDeviceGroups}>
              <ManageDeviceGroupsBtn />
            </Protected>
          </ContextMenuAlign>
          <ContextMenuAlign>
            {
              this.state.updatingAlertStatus &&
              <div className="alert-indicator-container">
                <Indicator />
              </div>
            }
            {
              this.state.ruleContextBtns
              || this.state.alertContextBtns
              || this.state.deviceContextBtns
            }
            <TimeIntervalDropdown
              onChange={onTimeIntervalChange}
              value={timeInterval}
              t={t} />
            <RefreshBar
              refresh={this.refreshData}
              time={lastUpdated}
              isPending={isPending}
              t={t} />
          </ContextMenuAlign>
        </ContextMenu>
        <PageContent className="maintenance-container rule-details-container">

          <PageTitle titleValue={alertName} />
          {
            !this.props.error
              ?
              <div>
                <div className="header-container">
                  <div className="rule-stat-container">
                    <div className="rule-stat-cell">
                      <div className="rule-stat-header">{t('maintenance.total')}</div>
                      <div className="rule-stat-value">{renderUndefined(counts.total)}</div>
                    </div>
                    <div className="rule-stat-cell">
                      <div className="rule-stat-header">{t('maintenance.open')}</div>
                      <div className="rule-stat-value">{renderUndefined(counts.open)}</div>
                    </div>
                    <div className="rule-stat-cell">
                      <div className="rule-stat-header">{t('maintenance.acknowledged')}</div>
                      <div className="rule-stat-value">{renderUndefined(counts.acknowledged)}</div>
                    </div>
                    <div className="rule-stat-cell">
                      <div className="rule-stat-header">{t('maintenance.closed')}</div>
                      <div className="rule-stat-value">{renderUndefined(counts.closed)}</div>
                    </div>
                    <div className="rule-stat-cell">
                      <div className="rule-stat-header">{t('maintenance.lastEvent')}</div>
                      <div className="rule-stat-value">
                        {
                          selectedAlert.lastOccurrence
                            ? <TimeRenderer value={selectedAlert.lastOccurrence} />
                            : Config.emptyValue
                        }
                      </div>
                    </div>
                    <div className="rule-stat-cell">
                      <div className="rule-stat-header">{t('maintenance.severity')}</div>
                      <div className="rule-stat-value">
                        {
                          selectedAlert.severity
                            ? <SeverityRenderer context={({ t: this.props.t })} value={selectedAlert.severity} />
                            : Config.emptyValue
                        }
                      </div>
                    </div>
                  </div>
                </div>
                <div className="details-description">
                  {t('maintenance.ruleDetailsDesc')}
                </div>
                <h4 className="sub-heading">{t('maintenance.ruleDetail')}</h4>
                <RulesGrid
                  t={t}
                  deviceGroups={deviceGroups}
                  style={{ height: 2 * ROW_HEIGHT + 2 }}
                  onGridReady={this.onRuleGridReady}
                  onContextMenuChange={this.onContextMenuChange('ruleContextBtns')}
                  onHardSelectChange={this.onHardSelectChange('rules')}
                  rowData={rule}
                  pagination={false}
                  refresh={this.props.fetchRules}
                  logEvent={this.props.logEvent} />

                <h4 className="sub-heading">{t('maintenance.alertOccurrences')}</h4>
                <AlertOccurrencesGrid {...alertsGridProps} />

                <h4 className="sub-heading">{t('maintenance.relatedInfo')}</h4>
                <div className="tab-container">
                  <button className={joinClasses('tab', selectedTab === tabIds.all ? 'active' : '')}
                    onClick={this.setTab(tabIds.all)}>{t('maintenance.all')}</button>
                  <button className={joinClasses('tab', selectedTab === tabIds.devices ? 'active' : '')}
                    onClick={this.setTab(tabIds.devices)}>{t('maintenance.devices')}</button>
                  <button className={joinClasses('tab', selectedTab === tabIds.telemetry ? 'active' : '')}
                    onClick={this.setTab(tabIds.telemetry)}>{t('maintenance.telemetry')}</button>
                </div>
                {
                  (selectedTab === tabIds.all || selectedTab === tabIds.devices) &&
                  <ComponentArray>
                    <h4 className="sub-heading">{t('maintenance.alertedDevices')}</h4>
                    <DevicesGridContainer
                      t={t}
                      domLayout="autoHeight"
                      onGridReady={this.onDeviceGridReady}
                      rowData={isPending ? undefined : this.state.devices}
                      onContextMenuChange={this.onContextMenuChange('deviceContextBtns')}
                      onHardSelectChange={this.onHardSelectChange('devices')} />
                  </ComponentArray>
                }
                {
                  !isPending && (selectedTab === tabIds.all || selectedTab === tabIds.telemetry) && Object.keys(this.state.telemetry).length > 0 &&
                  <ComponentArray>
                    <h4 className="sub-heading">{t('maintenance.alertedDeviceTelemetry')}</h4>
                    <div className="details-chart-container">
                      <TelemetryChart telemetry={this.state.telemetry} theme={theme} colors={chartColorObjects} />
                    </div>
                  </ComponentArray>
                }
              </div>
              : <AjaxError t={t} error={error} />
          }
        </PageContent>
      </ComponentArray>
    );
  }
}
