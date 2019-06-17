// Copyright (c) Microsoft. All rights reserved.

import { connect } from 'react-redux';
import { withNamespaces } from 'react-i18next';
import {
  epics as appEpics
} from 'store/reducers/appReducer';

import {
  redux as deviceRedux,
  getActiveCameraId
} from 'store/reducers/devicesReducer';
import { DeviceListDropdown } from './deviceListDropdown';

const mapStateToProps = state => ({
  activeCameraId: getActiveCameraId(state)
});

// Wrap the dispatch method
const mapDispatchToProps = dispatch => ({
  changeSelectedCamera: (id) => dispatch(deviceRedux.actions.updateActiveCamera(id)),
  logEvent: diagnosticsModel => dispatch(appEpics.actions.logEvent(diagnosticsModel))
});

export const DeviceListDropdownContainer = withNamespaces()(connect(mapStateToProps, mapDispatchToProps)(DeviceListDropdown));
