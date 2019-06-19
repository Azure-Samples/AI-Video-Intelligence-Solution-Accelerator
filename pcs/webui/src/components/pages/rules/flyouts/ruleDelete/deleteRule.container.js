// Copyright (c) Microsoft. All rights reserved.

import { connect } from 'react-redux';
import { withNamespaces } from 'react-i18next';
import { DeleteRule } from './deleteRule';
import { redux as rulesRedux } from 'store/reducers/rulesReducer';

// Wrap the dispatch method
const mapDispatchToProps = dispatch => ({
  modifyRules: rule => dispatch(rulesRedux.actions.modifyRules(rule))
});

export const DeleteRuleContainer = withNamespaces()(connect(null, mapDispatchToProps)(DeleteRule));
