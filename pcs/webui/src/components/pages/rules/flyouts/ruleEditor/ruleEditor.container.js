// Copyright (c) Microsoft. All rights reserved.

import { connect } from 'react-redux';
import { withNamespaces } from 'react-i18next';
import { RuleEditor } from './ruleEditor';
import { getDeviceGroups } from 'store/reducers/appReducer';
import { redux as rulesRedux, getRuleById } from 'store/reducers/rulesReducer';
import { epics as appEpics } from 'store/reducers/appReducer';

// Pass device groups
const mapStateToProps = (state, props) => ({
  rule: getRuleById(state, props.ruleId),
  deviceGroups: getDeviceGroups(state)
});

// Wrap the dispatch method
const mapDispatchToProps = dispatch => ({
  insertRules: rules => dispatch(rulesRedux.actions.insertRules(rules)),
  modifyRules: rules => dispatch(rulesRedux.actions.modifyRules(rules)),
  logEvent: diagnosticsModel => dispatch(appEpics.actions.logEvent(diagnosticsModel))
});

export const RuleEditorContainer = withNamespaces()(connect(mapStateToProps, mapDispatchToProps)(RuleEditor));
