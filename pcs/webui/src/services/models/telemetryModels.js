// Copyright (c) Microsoft. All rights reserved.

import { camelCaseReshape, getItems, reshape } from 'utilities';
import update from 'immutability-helper';

export const ruleCalculations = ['Average', 'Instant'];

// Represented in milliSeconds (label is minutes, value is milliseconds)
export const ruleTimePeriods = [
  { label: '1', value: '60000' },
  { label: '5', value: '300000' },
  { label: '10', value: '600000' }
];

export const getRuleTimePeriodLabel = (value) => {
  const timePeriod = ruleTimePeriods.find(item => item.value === value);
  return (timePeriod || {}).label || value;

};

export const ruleOperators = ['Equals', 'GreaterThan', 'LessThan', 'GreaterThanOrEqual', 'LessThanOrEqual'];

export const toRulesModel = (response = {}) => getItems(response)
  .map(toRuleModel);

export const toRuleModel = (response = {}) => {
  const model = camelCaseReshape(response, {
    'id': 'id',
    'conditions': 'conditions',
    'dateCreated': 'dateCreated',
    'dateModified': 'dateModified',
    'description': 'description',
    'eTag': 'eTag',
    'enabled': 'enabled',
    'groupId': 'groupId',
    'name': 'name',
    'severity': 'severity',
    'calculation': 'calculation',
    'timePeriod': 'timePeriod',
    'action.type': 'type',
    'deleted': 'deleted',
    'actions': 'actions'
  });
  return update(model, {
    severity: { $set: (model.severity || '').toLowerCase() },
    status: { $set: (model.deleted ? 'Deleted' : model.enabled ? 'Enabled' : 'Disabled') }
  });
};

// TODO: Double check the response from alertsByRule and alerts, might only need one model
export const toAlertsModel = (response = {}) => getItems(response)
  .map((alert = {}) => {
    const model = camelCaseReshape(alert, {
      'rule.id': 'ruleId',
      'created': 'created',
      'status': 'status',
      'rule.severity': 'severity',
      'rule.description': 'ruleDescription',
      'deviceId': 'deviceId',
      'dateCreated': 'dateCreated',
      'dateModified': 'dateModified',
      'description': 'description',
      'id': 'id',
      'groupId': 'groupId'
    });
    return update(model, {
      severity: { $set: (model.severity || '').toLowerCase() },
      status: { $set: (model.status || '').toLowerCase() }
    });
  });

export const toActiveAlertsModel = (response = {}) => getItems(response)
  .map((alert = {}) => {
    const model = camelCaseReshape(alert, {
      'rule.id': 'ruleId',
      'count': 'count',
      'created': 'created',
      'status': 'status',
      'rule.severity': 'severity',
      'rule.description': 'description'
    });
    return update(model, {
      severity: { $set: (model.severity || '').toLowerCase() },
      status: { $set: (model.status || '').toLowerCase() }
    });
  });

export const toAlertsForRuleModel = (response = {}) => getItems(response)
  .map(toAlertForRuleModel);

export const toAlertForRuleModel = (alert = {}) => {
  const model = camelCaseReshape(alert, {
    'id': 'id',
    'dateCreated': 'dateCreated',
    'dateModified': 'dateModified',
    'description': 'description',
    'deviceId': 'deviceId',
    'eTag': 'eTag',
    'enabled': 'enabled',
    'groupId': 'groupId',
    'status': 'status',
    'rule.id': 'ruleId'
  });
  return update(model, {
    status: { $set: (model.status || '').toLowerCase() }
  });
};

// TSI has a bug that causes it to drop trailing zero character in
// the data.time field, so we fix that here. Note that this fix
// assumes the AIVideo time format 2019-05-16T21:02:56.870Z, with
// exactly three decimal places of seconds.
export const toMessagesModel = (response = {}) => getItems(response)
  .map((message = {}) => {
    const result = camelCaseReshape(message, {
      'data': 'data',
      'deviceId': 'deviceId',
      'messageSchema': 'messageSchema',
      'time': 'time'
      });
    // Check if 'time' is present
    const correctLength = 24;
    if (result.data.time && result.data.time.length < correctLength) {
      const missingChars = correctLength - result.data.time.length;
      // Trim the trailing 'Z'
      var adjustedValue = result.data.time.slice(0, -1);
      switch (missingChars) {
        case 1:
        adjustedValue += '0Z';
        break;
        case 2:
        adjustedValue += '00Z';
        break;
        case 4:
        adjustedValue += '.000Z'
        break;
        default: break;
      }
      result.data.time = adjustedValue;
    }
  return result;
});

export const toStatusModel = (response = {}) => camelCaseReshape(response, {
  'properties': 'properties'
});

export const toEditRuleRequestModel = ({
  id,
  name,
  description,
  groupId,
  conditions,
  actions,
  severity,
  enabled,
  calculation,
  timePeriod,
  eTag
}) => {
  const Conditions = conditions.map(condition => ({
    Field: condition.field,
    Operator: condition.operator,
    Value: condition.value
  }));

  const Actions =  (actions || []).map(act => ({
    Type: act.type,
    Parameters: {
      Notes: act.parameters.notes,
      Recipients: act.parameters.recipients,
      Subject: act.parameters.subject
    }
  }));

  return {
    Id: id,
    Name: name,
    Description: description,
    GroupId: groupId,
    Severity: severity,
    Enabled: enabled,
    Calculation: calculation,
    TimePeriod: timePeriod,
    ETag: eTag,
    Conditions,
    Actions
  };
}

export const toTelemetryRequestModel = (alarmQueryModel = {}) => reshape(alarmQueryModel, {
  'from': 'From',
  'to': 'To',
  'order': 'Order',
  'limit': 'Limit',
  'devices': 'Devices'
});
