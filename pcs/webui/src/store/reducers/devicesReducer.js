// Copyright (c) Microsoft. All rights reserved.

import 'rxjs';
import { Observable } from 'rxjs';
import moment from 'moment';
import { schema, normalize } from 'normalizr';
import update from 'immutability-helper';
import { createSelector } from 'reselect';
import { redux as appRedux, getActiveDeviceGroupConditions } from './appReducer';
import { IoTHubManagerService } from 'services';
import {
  createReducerScenario,
  createEpicScenario,
  resetPendingAndErrorReducer,
  errorPendingInitialState,
  pendingReducer,
  errorReducer,
  setPending,
  toActionCreator,
  getPending,
  getError
} from 'store/utilities';

// ========================= Epics - START
const handleError = fromAction => error =>
  Observable.of(redux.actions.registerError(fromAction.type, { error, fromAction }));

export const epics = createEpicScenario({
  /** Loads the devices */
  fetchDevices: {
    type: 'DEVICES_FETCH',
    epic: (fromAction, store) => {
      const conditions = getActiveDeviceGroupConditions(store.getState());
      return IoTHubManagerService.getDevices(conditions)
        .map(toActionCreator(redux.actions.updateDevices, fromAction))
        .catch(handleError(fromAction))
    }
  },

  /** Loads the devices by condition provided in payload*/
  fetchDevicesByCondition: {
    type: 'DEVICES_FETCH_BY_CONDITION',
    epic: fromAction => {
      return IoTHubManagerService.getDevices(fromAction.payload)
        .map(toActionCreator(redux.actions.updateDevicesByCondition, fromAction))
        .catch(handleError(fromAction))
    }
  },

  /** Loads EdgeAgent json from device modules */
  fetchEdgeAgent: {
    type: 'DEVICES_FETCH_EDGE_AGENT',
    epic: fromAction => IoTHubManagerService
      .getModulesByQuery(`"deviceId IN ['${fromAction.payload}'] AND moduleId = '$edgeAgent'"`)
      .map(([edgeAgent]) => edgeAgent)
      .map(toActionCreator(redux.actions.updateModuleStatus, fromAction))
      .catch(handleError(fromAction))

  },

  /* Update the devices if the selected device group changes */
  refreshDevices: {
    type: 'DEVICES_REFRESH',
    rawEpic: ($actions) =>
      $actions.ofType(appRedux.actionTypes.updateActiveDeviceGroup)
        .map(({ payload }) => payload)
        .distinctUntilChanged()
        .map(_ => epics.actions.fetchDevices())
  }
});
// ========================= Epics - END

// ========================= Schemas - START
const deviceSchema = new schema.Entity('devices');
const deviceListSchema = new schema.Array(deviceSchema);
// ========================= Schemas - END

// ========================= Reducers - START
const initialState = { ...errorPendingInitialState, entities: {}, items: [], lastUpdated: '', activeCameraId: undefined };

const updateActiveCameraReducer = (state, { payload }) => update(state,
  { activeCameraId: { $set: payload } }
);

const updateDevicesReducer = (state, { payload, fromAction }) => {
  const { entities: { devices }, result } = normalize(payload, deviceListSchema);
  return update(state, {
    entities: { $set: devices },
    items: { $set: result },
    lastUpdated: { $set: moment() },
    ...setPending(fromAction.type, false)
  });
};

const updateDevicesByConditionReducer = (state, { payload, fromAction }) => {
  const { entities: { devices } } = normalize(payload, deviceListSchema);
  return update(state, {
    devicesByCondition: { $set: devices },
    ...setPending(fromAction.type, false)
  });
};

const deleteDevicesReducer = (state, { payload }) => {
  const spliceArr = payload.reduce((idxAcc, payloadItem) => {
    const idx = state.items.indexOf(payloadItem);
    if (idx !== -1) {
      idxAcc.push([idx, 1]);
    }
    return idxAcc;
  }, []);
  return update(state, {
    entities: { $unset: payload },
    items: { $splice: spliceArr }
  });
};

const insertDevicesReducer = (state, { payload }) => {
  const inserted = payload.map(device => ({ ...device, isNew: true }));
  const { entities: { devices }, result } = normalize(inserted, deviceListSchema);
  if (state.entities) {
    return update(state, {
      entities: { $merge: devices },
      items: { $splice: [[0, 0, ...result]] }
    });
  }
  return update(state, {
    entities: { $set: devices },
    items: { $set: result }
  });
};

const updateTagsReducer = (state, { payload }) => {
  const updatedTagData = {};
  payload.updatedTags.forEach(({ name, value }) => (updatedTagData[name] = value));

  const updatedDevices = payload.deviceIds
    .map((id) => update(state.entities[id], {
      tags: {
        $merge: updatedTagData,
        $unset: payload.deletedTags
      }
    }));

  const { entities: { devices } } = normalize(updatedDevices, deviceListSchema);
  return update(state, {
    entities: { $merge: devices }
  });
};

const updateModuleStatusReducer = (state, { payload, fromAction }) => {
  const updateAction = payload
    ? { deviceModuleStatus: { $set: payload } }
    : { $unset: ['deviceModuleStatus'] };

  return update(state, {
    ...updateAction,
    ...setPending(fromAction.type, false)
  });
};

const updatePropertiesReducer = (state, { payload }) => {
  const updatedPropertyData = {};
  payload.updatedProperties.forEach(({ name, value }) => (updatedPropertyData[name] = value));

  const updatedDevices = payload.deviceIds
    .map((id) => update(state.entities[id], {
      desiredProperties: {
        $merge: updatedPropertyData,
        $unset: payload.deletedProperties
      }
    }));

  const { entities: { devices } } = normalize(updatedDevices, deviceListSchema);
  return update(state, {
    entities: { $merge: devices }
  });
};

/* Action types that cause a pending flag */
const fetchableTypes = [
  epics.actionTypes.fetchDevices,
  epics.actionTypes.fetchDevicesByCondition,
  epics.actionTypes.fetchEdgeAgent
];

export const redux = createReducerScenario({
  updateDevices: { type: 'DEVICES_UPDATE', reducer: updateDevicesReducer },
  updateDevicesByCondition: { type: 'DEVICES_UPDATE_BY_CONDITION', reducer: updateDevicesByConditionReducer },
  registerError: { type: 'DEVICES_REDUCER_ERROR', reducer: errorReducer },
  isFetching: { multiType: fetchableTypes, reducer: pendingReducer },
  deleteDevices: { type: 'DEVICE_DELETE', reducer: deleteDevicesReducer },
  insertDevices: { type: 'DEVICE_INSERT', reducer: insertDevicesReducer },
  updateActiveCamera: { type: 'APP_ACTIVE_DEVICE_UPDATE', reducer: updateActiveCameraReducer },
  updateTags: { type: 'DEVICE_UPDATE_TAGS', reducer: updateTagsReducer },
  updateProperties: { type: 'DEVICE_UPDATE_PROPERTIES', reducer: updatePropertiesReducer },
  updateModuleStatus: { type: 'DEVICE_MODULE_STATUS', reducer: updateModuleStatusReducer },
  resetPendingAndError: { type: 'DEVICE_REDUCER_RESET_ERROR_PENDING', reducer: resetPendingAndErrorReducer }
});

export const reducer = { devices: redux.getReducer(initialState) };
// ========================= Reducers - END

// ========================= Selectors - START
export const getActiveCameraId = state => getDevicesReducer(state).activeCameraId;
export const getDevicesReducer = state => state.devices;
export const getEntities = state => getDevicesReducer(state).entities || {};
export const getItems = state => getDevicesReducer(state).items || [];
export const getDevicesLastUpdated = state => getDevicesReducer(state).lastUpdated;
export const getDevicesError = state =>
  getError(getDevicesReducer(state), epics.actionTypes.fetchDevices);
export const getDevicesPendingStatus = state =>
  getPending(getDevicesReducer(state), epics.actionTypes.fetchDevices);
export const getDevicesByCondition = state => getDevicesReducer(state).devicesByCondition || {};
export const getDevicesByConditionError = state =>
  getError(getDevicesReducer(state), epics.actionTypes.fetchDevicesByCondition);
export const getDevicesByConditionPendingStatus = state =>
  getPending(getDevicesReducer(state), epics.actionTypes.fetchDevicesByCondition);
export const getDevices = createSelector(
  getEntities, getItems,
  (entities, items) => items.map(id => entities[id])
);
export const getDeviceById = (state, id) =>
  getEntities(state)[id];
export const getDeviceModuleStatus = state => {
  const deviceModuleStatus = getDevicesReducer(state).deviceModuleStatus
  return deviceModuleStatus
    ? {
      code: deviceModuleStatus.code,
      description: deviceModuleStatus.description
    }
    : undefined
};
export const getDeviceModuleStatusPendingStatus = state =>
  getPending(getDevicesReducer(state), epics.actionTypes.fetchEdgeAgent);
export const getDeviceModuleStatusError = state =>
  getError(getDevicesReducer(state), epics.actionTypes.fetchEdgeAgent);
// ========================= Selectors - END
