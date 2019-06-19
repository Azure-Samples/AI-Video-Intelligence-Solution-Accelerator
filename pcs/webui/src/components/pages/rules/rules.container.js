// Copyright (c) Microsoft. All rights reserved.

import { connect } from 'react-redux';
import { withNamespaces } from 'react-i18next';
import { Rules } from './rules';
import {
  epics as rulesEpics,
  getRules,
  getEntities,
  getRulesError,
  getRulesLastUpdated,
  getRulesPendingStatus
} from 'store/reducers/rulesReducer';
import {
  epics as appEpics,
  redux as appRedux,
  getDeviceGroups,
  getApplicationPermissionsAssigned
} from 'store/reducers/appReducer';

// Pass the devices status
const mapStateToProps = state => ({
  rules: getRules(state),
  entities: getEntities(state),
  error: getRulesError(state),
  isPending: getRulesPendingStatus(state),
  deviceGroups: getDeviceGroups(state),
  lastUpdated: getRulesLastUpdated(state),
  applicationPermissionsAssigned: getApplicationPermissionsAssigned(state)
});

// Wrap the dispatch method
const mapDispatchToProps = dispatch => ({
  fetchRules: () => dispatch(rulesEpics.actions.fetchRules()),
  updateCurrentWindow: (currentWindow) => dispatch(appRedux.actions.updateCurrentWindow(currentWindow)),
  logEvent: diagnosticsModel => dispatch(appEpics.actions.logEvent(diagnosticsModel))
});

export const RulesContainer = withNamespaces()(connect(mapStateToProps, mapDispatchToProps)(Rules));
