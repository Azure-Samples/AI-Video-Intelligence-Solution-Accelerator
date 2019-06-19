// Copyright (c) Microsoft. All rights reserved.

import update from 'immutability-helper';
import dot from 'dot-object';
import { camelCaseReshape, getItems, float } from 'utilities';
import uuid from 'uuid/v4';

// Contains methods for converting service response
// object to UI friendly objects

export const toDevicesModel = (response = {}) => getItems(response)
  .map(toDeviceModel);

export const toDeviceModel = (device = {}) => {
  const modelData = camelCaseReshape(device, {
    'id': 'id',
    'lastActivity': 'lastActivity',
    'connected': 'connected',
    'isSimulated': 'isSimulated',
    'properties.reported.supportedMethods': 'methods',
    'properties.reported.telemetry': 'telemetry',
    'properties.reported.type': 'type',
    'properties.reported.firmware.currentFwVersion': 'currentFwVersion',
    'properties.reported.firmware.lastFwUpdateStartTime': 'lastFwUpdateStartTime',
    'properties.reported.firmware.lastFwUpdateEndTime': 'lastFwUpdateEndTime',
    'c2DMessageCount': 'c2DMessageCount',
    'enabled': 'enabled',
    'lastStatusUpdated': 'lastStatusUpdated',
    'ioTHubHostName': 'iotHubHostName',
    'eTag': 'eTag',
    'authentication': 'authentication'
  });
  // TODO: Remove this once device simulation has removed FirmwareUpdate from supportedMethods of devices
  const methods = (modelData.methods ? modelData.methods.split(',') : [])
    .filter((methodName) => methodName !== "FirmwareUpdate");
  return update(modelData, {
    methods: { $set: methods },
    tags: { $set: device.Tags || {} },
    // TODO: Rename properties to reportedProperties
    properties: {
      $set: update(dot.pick('Properties.Reported', device), {
        $unset: ['Telemetry', 'SupportedMethods', 'firmware']
      })
    },
    desiredProperties: {
      $set: dot.pick('Properties.Desired', device)
    },
    firmware: {
      $set: modelData.currentFwVersion ? modelData.currentFwVersion : dot.pick('Properties.Reported.Firmware', device)
    }
  });
}

export const toModuleFieldsModel = (response = {}) => getItems(response)
  .map(toModuleFieldModel);

export const toModuleFieldModel = (module = {}) => camelCaseReshape(module, {
  /* Expected schema for modules
    "reported": {
      "Telemetry": {
        "MessageSchema": {
          "Fields": {
            "temperature": "Double",
            ...
          }
        }
      }
    }
    */
  'reported.telemetry.messageSchema.fields': 'moduleFields',
});

export const toJobsModel = (response = []) => response.map(job => camelCaseReshape(job, {
  'jobId': 'jobId',
  'createdTimeUtc': 'createdTimeUtc',
  'endTimeUtc': 'endTimeUtc',
  'maxExecutionTimeInSeconds': 'maxExecutionTimeInSeconds',
  'methodParameter.name': 'methodName',
  'queryCondition': 'queryCondition',
  'resultStatistics.deviceCount': 'stats.deviceCount',
  'resultStatistics.failedCount': 'stats.failedCount',
  'resultStatistics.pendingCount': 'stats.pendingCount',
  'resultStatistics.runningCount': 'stats.runningCount',
  'resultStatistics.succeededCount': 'stats.succeededCount',
  'startTimeUtc': 'startTimeUtc',
  'status': 'status',
  'type': 'type'
}));

export const toSubmitTagsJobRequestModel = (devices, { jobName, updatedTags, deletedTags }) => {
  const jobId = jobName ? jobName + '-' + uuid() : uuid();
  const deviceIds = devices.map(({ id }) => `'${id}'`).join(',');
  const Tags = {};
  // Ensure type passed to server is correct as specified: number or text.
  // The toString call is necessary when a number should be saved as text.
  updatedTags.forEach(({ name, value, type }) =>
    (Tags[name] = type === 'Number' ? float(value) : value.toString()));
  deletedTags.forEach((name) => (Tags[name] = null));
  const request = {
    JobId: jobId,
    QueryCondition: `deviceId in [${deviceIds}]`,
    MaxExecutionTimeInSeconds: 0,
    UpdateTwin: {
      Tags
    }
  };
  return request;
};

export const toSubmitPropertiesJobRequestModel = (devices, { jobName, updatedProperties, deletedProperties }) => {
  const jobId = jobName ? jobName + '-' + uuid() : uuid();
  const deviceIds = devices.map(({ id }) => `'${id}'`).join(',');
  const Desired = {};
  // Ensure type passed to server is correct as specified: number or text.
  // The toString call is necessary when a number should be saved as text.
  updatedProperties.forEach(({ name, value, type }) =>
    (Desired[name] = type === 'Number' ? float(value) : value.toString()));
  deletedProperties.forEach((name) => (Desired[name] = null));
  const request = {
    JobId: jobId,
    QueryCondition: `deviceId in [${deviceIds}]`,
    MaxExecutionTimeInSeconds: 0,
    UpdateTwin: {
      Properties: {
        Desired
      }
    }
  };
  return request;
};

export const toSubmitMethodJobRequestModel = (devices, { jobName, methodName }) => {
  const jobId = jobName ? jobName + '-' + uuid() : uuid();
  const deviceIds = devices.map(({ id }) => `'${id}'`).join(',');
  const request = {
    JobId: jobId,
    QueryCondition: `deviceId in [${deviceIds}]`,
    MaxExecutionTimeInSeconds: 0,
    MethodParameter: {
      Name: methodName,
      JsonPayload: ''
    }
  };
  return request;
};

export const toJobStatusModel = (response = {}) => camelCaseReshape(response, {
  'createdTimeUtc': 'createdTimeUtc',
  'devices': 'devices',
  'endTimeUtc': 'endTimeUtc',
  'jobId': 'jobId',
  'maxExecutionTimeInSeconds': 'maxExecutionTimeInSeconds',
  'methodParameter.name': 'methodName',
  'queryCondition': 'queryCondition',
  'startTimeUtc': 'startTimeUtc',
  'status': 'status',
  'type': 'type'
});

export const authenticationTypeOptions = {
  symmetric: 0,
  x509: 1
};

export const toNewDeviceRequestModel = ({
  deviceId,
  isGenerateId,
  isEdgeDevice,
  isSimulated,
  authenticationType,
  isGenerateKeys,
  primaryKey,
  secondaryKey
}) => {
  const isX509 = authenticationType === authenticationTypeOptions.x509;

  return {
    Id: isGenerateId ? '' : deviceId,
    IsEdgeDevice: isEdgeDevice,
    IsSimulated: isSimulated,
    Enabled: true,
    Authentication:
      isGenerateKeys
        ? {}
        : {
          AuthenticationType: authenticationType,
          PrimaryKey: isX509 ? null : primaryKey,
          SecondaryKey: isX509 ? null : secondaryKey,
          PrimaryThumbprint: isX509 ? primaryKey : null,
          SecondaryThumbprint: isX509 ? secondaryKey : null
        }
  };
}

export const toDevicePropertiesModel = (iotResponse, dsResponse) => {
  const propertySet = new Set([...getItems(iotResponse), ...getItems(dsResponse)]);
  return [...propertySet];
};

export const toDeploymentModel = (deployment = {}) => {
  const modelData = camelCaseReshape(deployment, {
    'id': 'id',
    'name': 'name',
    'deviceGroupId': 'deviceGroupId',
    'deviceGroupQuery': 'deviceGroupQuery',
    'deviceGroupName': 'deviceGroupName',
    'packageName': 'packageName',
    'priority': 'priority',
    'packageType': 'packageType',
    'configType': 'configType',
    'createdDateTimeUtc': 'createdDateTimeUtc',
    'metrics.systemMetrics.appliedCount': 'appliedCount',
    'metrics.systemMetrics.reportedFailedCount': 'failedCount',
    'metrics.systemMetrics.reportedSuccessfulCount': 'succeededCount',
    'metrics.systemMetrics.targetedCount': 'targetedCount',
    'metrics.systemMetrics.pendingCount': 'pendingCount'
  });
  return update(modelData, {
    deviceStatuses: { $set: dot.pick('Metrics.DeviceStatuses', deployment) },
    customMetrics: { $set: dot.pick('Metrics.CustomMetrics', deployment) }
  });
}

export const toDeploymentsModel = (response = {}) => getItems(response)
  .map(toDeploymentModel);

export const toDeploymentRequestModel = (deploymentModel = {}) => ({
  DeviceGroupId: deploymentModel.deviceGroupId,
  DeviceGroupName: deploymentModel.deviceGroupName,
  DeviceGroupQuery: deploymentModel.deviceGroupQuery,
  Name: deploymentModel.name,
  PackageId: deploymentModel.packageId,
  PackageName: deploymentModel.packageName,
  PackageContent: deploymentModel.packageContent,
  Priority: deploymentModel.priority,
  PackageType: deploymentModel.packageType,
  ConfigType: deploymentModel.configType
});

export const toEdgeAgentModel = (edgeAgent = {}) => camelCaseReshape(edgeAgent, {
  'deviceId': 'id',
  'reported.lastDesiredStatus.description': 'description',
  'reported.lastDesiredStatus.code': 'code',
  'reported.systemModules.edgeAgent.lastStartTimeUtc': 'start',
  'reported.systemModules.edgeAgent.lastExitTimeUtc': 'end'
});

export const toEdgeAgentsModel = (response = []) => getItems(response)
  .map(toEdgeAgentModel);
