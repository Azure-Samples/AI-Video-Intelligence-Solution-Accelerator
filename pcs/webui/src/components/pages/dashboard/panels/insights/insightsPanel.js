// Copyright (c) Microsoft. All rights reserved.

import React, { Component } from 'react';

import { AjaxError } from 'components/shared';
import {
  Panel,
  PanelContent,
  PanelError,
  PanelHeader,
  PanelHeaderLabel
} from 'components/pages/dashboard/panel';
import { toDiagnosticsModel } from 'services/models';
import { DeviceListDropdownContainer as DeviceListDropdown } from 'components/shell/deviceListDropdown';
import {InsightsImage} from './insightsImage';
import './insightsPanel.scss';
import { formatTime } from 'utilities';

export class InsightsPanel extends Component {


  logExploreClick = () => {
    this.props.logEvent(toDiagnosticsModel('InsightsPanel_ExploreClick', {}));
  }

  render() {
    const { t, error, cameras, imageReport } = this.props;
    let image = null;
    let boundingBoxes = null;
    let headerTime = "";
    if (!!imageReport) {
      image = imageReport.image;
      headerTime = " " + formatTime(image.data.time);
      boundingBoxes = imageReport.recognitions;
    }

    return (
      <Panel className="insights-panel-container">
        <PanelHeader>
          <PanelHeaderLabel>{t('dashboard.panels.insights.header') + headerTime}</PanelHeaderLabel>
          <DeviceListDropdown
            selectCameraPrompt={t('dashboard.panels.insights.selectCameraPrompt')}
            cameras={ cameras } />
        </PanelHeader>
        <PanelContent>
          <InsightsImage
            image={ image }
            boundingBoxes={ boundingBoxes }
            t={t} />
        </PanelContent>
        { error && <PanelError><AjaxError t={t} error={error} /></PanelError> }
      </Panel>
    );
  }
}
