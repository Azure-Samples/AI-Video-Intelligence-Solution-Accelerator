// Copyright (c) Microsoft. All rights reserved.

import { connect } from 'react-redux';
import { epics as appEpics } from 'store/reducers/appReducer';
import { InsightsPanel } from './insightsPanel';

const mapDispatchToProps = dispatch => ({
  logEvent: diagnosticsModel => dispatch(appEpics.actions.logEvent(diagnosticsModel))
});

export const InsightsPanelContainer = connect(null, mapDispatchToProps)(InsightsPanel);
