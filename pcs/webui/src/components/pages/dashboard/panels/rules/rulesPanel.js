// Copyright (c) Microsoft. All rights reserved.

import React, { Component } from 'react';

import { AjaxError, Indicator } from 'components/shared';
import {
  Panel,
  PanelContent,
  PanelError,
  PanelHeader,
  PanelHeaderLabel,
  PanelMsg,
  PanelOverlay
} from 'components/pages/dashboard/panel';
import { CompactGrid } from 'components/shared';
import { toDiagnosticsModel } from 'services/models';
import { translateColumnDefs } from 'utilities';
import { formatTime } from 'utilities';

import './rulesPanel.scss';

export class RulesPanel extends Component {

  constructor(props) {
    super(props);

    this.columnDefs = [
      {
        headerName: 'rules.grid.ruleName',
        field: 'name',
        sort: 'asc',
        filter: 'text',
        cellRendererFramework: undefined
      },
      {
        headerName: 'rules.grid.severity',
        field: 'severity',
        filter: 'text',
      },
      {
        headerName: 'rules.grid.eventTime',
        field: 'lastTrigger.response',
        cellRendererFramework: ({value}) => formatTime(value)
      }
    ];
  }

  logExploreClick = () => {
    this.props.logEvent(toDiagnosticsModel('RulesPanel_ExploreClick', {}));
  }

  render() {
    const { t, rules, isPending, error, fetchRules } = this.props;
    const gridProps = {
      columnDefs: translateColumnDefs(t, this.columnDefs),
      onGridReady: this.onGridReady,
      rowData: isPending ? undefined : rules || [],
      onContextMenuChange: this.onContextMenuChange,
      t: this.props.t,
      deviceGroups: this.props.deviceGroups,
      refresh: fetchRules,
      logEvent: this.props.logEvent,
      pagination: true,
      paginationPageSize: 5

    };
    const showOverlay = isPending && !rules.length;

    return (
      <Panel className="rules-panel-container">
        <PanelHeader>
          <PanelHeaderLabel>{t('dashboard.panels.rules.header')}</PanelHeaderLabel>
        </PanelHeader>
        <PanelContent>
        <CompactGrid {...gridProps} />
          {
            (!showOverlay && rules.length === 0)
              && <PanelMsg>{t('dashboard.noData')}</PanelMsg>
          }
        </PanelContent>
        { showOverlay && <PanelOverlay><Indicator /></PanelOverlay> }
        { error && <PanelError><AjaxError t={t} error={error} /></PanelError> }
      </Panel>
    );
  }
}
