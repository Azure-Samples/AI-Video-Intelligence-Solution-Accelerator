// Copyright (c) Microsoft. All rights reserved.

import { connect } from 'react-redux';
import { withNamespaces } from 'react-i18next';
import {
  epics as appEpics,
  getDeviceGroups,
  getApplicationPermissionsAssigned
} from 'store/reducers/appReducer';
import {
  epics as rulesEpics,
  getRules,
  getEntities,
  getRulesError,
  getRulesLastUpdated,
  getRulesPendingStatus
} from 'store/reducers/rulesReducer';
import { RulesPanel } from './rulesPanel';

const mapDispatchToProps = dispatch => ({
  fetchRules: () => dispatch(rulesEpics.actions.fetchRules()),
  logEvent: diagnosticsModel => dispatch(appEpics.actions.logEvent(diagnosticsModel))
});

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

export const RulesPanelContainer = withNamespaces()(connect(mapStateToProps, mapDispatchToProps)(RulesPanel));
