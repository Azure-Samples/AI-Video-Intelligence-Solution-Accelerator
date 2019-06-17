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

import './eventsPanel.scss';

export class EventsPanel extends Component {

  constructor(props) {
    super(props);

    this.columnDefs = [
      {
        headerName: 'rules.grid.eventName',
        field: 'data.recognition',
        cellRendererFramework: ({value}) => {  return value ? value : "image update" },
        filter: 'text',
      },
      {
        sort: 'desc',
        headerName: 'rules.grid.eventTime',
        field: 'data.time',
        cellRendererFramework: ({value}) => formatTime(value)
      }
    ];
  }

  logExploreClick = () => {
    this.props.logEvent(toDiagnosticsModel('EventsPanel_ExploreClick', {}));
  }


  render() {
    const { t, events, isPending, error } = this.props;

    const gridProps = {
      columnDefs: translateColumnDefs(t, this.columnDefs),
      rowData: events.map((value, index) => { return { ...value, id: index};}),
      suppressFlyouts: true,
      domLayout: 'autoHeight',
      deltaRowDataMode: false,
      pagination: true,
      paginationPageSize: 16,
      t
    };
    const showOverlay = isPending && !events.length;

    return (
      <Panel className="events-panel-container">
        <PanelHeader>
          <PanelHeaderLabel>{t('dashboard.panels.events.header')}</PanelHeaderLabel>
        </PanelHeader>
        <PanelContent>
          <CompactGrid {...gridProps} />
          {
            (!showOverlay && events.length === 0)
              && <PanelMsg>{t('dashboard.noData')}</PanelMsg>
          }
        </PanelContent>
        { showOverlay && <PanelOverlay><Indicator /></PanelOverlay> }
        { error && <PanelError><AjaxError t={t} error={error} /></PanelError> }
      </Panel>
    );
  }
}
