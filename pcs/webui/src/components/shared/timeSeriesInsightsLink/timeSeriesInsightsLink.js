// Copyright (c) Microsoft. All rights reserved.

import React, { Component } from 'react';
import { Trans } from 'react-i18next';


import Config from 'app.config';
import { toDiagnosticsModel } from 'services/models';
import { themedPaths } from 'utilities';
import { Hyperlink, ThemedSvgContainer } from 'components/shared';
import { Balloon, BalloonAlignment, BalloonPosition } from '@microsoft/azure-iot-ux-fluent-controls/lib/components/Balloon/Balloon';

import './timeSeriesInsightsLink.scss';

export class TimeSeriesInsightsLink extends Component {
  onClick = () => {
    this.props.logEvent(toDiagnosticsModel('TimeSeriesInsights_Click', {}));
  }

  render() {
    const { t, href } = this.props;

    return (
      <div className="time-series-explorer-container">
        <Hyperlink href={href} onClick={this.onClick} target="_blank">{t('timeSeriesInsights.explore')}</Hyperlink>
        <Balloon
          position={BalloonPosition.Top}
          align={BalloonAlignment.End}
          tooltip={
          <Trans i18nKey={'timeSeriesInsights.exploreTooltip'}>
            To view in TSI, get permissions from the solution owner.
            <Hyperlink href={Config.contextHelpUrls.exploreTimeSeries} target="_blank">{t('timeSeriesInsights.learnMore')}</Hyperlink>
          </Trans>
          }
        >
          <ThemedSvgContainer paths={themedPaths.questionBubble} />
        </Balloon>
      </div>
    );
  }
}
