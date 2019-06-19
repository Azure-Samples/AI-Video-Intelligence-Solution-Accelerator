// Copyright (c) Microsoft. All rights reserved.

import React from "react";

import Config from 'app.config';

import { compareByProperty } from 'utilities';
import {
  SeverityRenderer,
  RuleStatusRenderer,
  CountRenderer,
  LastTriggerRenderer,
  LinkRenderer,
  SoftSelectLinkRenderer
} from 'components/shared/cellRenderers';
export const LAST_TRIGGER_DEFAULT_WIDTH = 310;

export const rulesColumnDefs = {
  ruleName: {
    headerName: 'rules.grid.ruleName',
    field: 'name',
    sort: 'asc',
    filter: 'text',
    cellRendererFramework: SoftSelectLinkRenderer
  },
  description: {
    headerName: 'rules.grid.description',
    field: 'description',
    filter: 'text'
  },
  severity: {
    headerName: 'rules.grid.severity',
    field: 'severity',
    filter: 'text',
    cellRendererFramework: SeverityRenderer
  },
  severityIconOnly: {
    headerName: 'rules.grid.severity',
    field: 'severity',
    filter: 'text',
    cellRendererFramework: props => <SeverityRenderer {...props} iconOnly={true} />
  },
  filter: {
    headerName: 'rules.grid.deviceGroup',
    field: 'groupId',
    filter: 'text',
    valueFormatter: ({ value, context: { deviceGroups } }) => {
      if (!deviceGroups) return value;

      const deviceGroup = deviceGroups.find(group => group.id === value);
      return (deviceGroup || {}).displayName || value;
    }
  },
  trigger: {
    headerName: 'rules.grid.trigger',
    field: 'sortableConditions',
    filter: 'text',
    cellClass: 'capitalize-cell'
  },
  notificationType: {
    headerName: 'rules.grid.notificationType',
    field: 'type',
    filter: 'text',
    valueFormatter: ({ value, context: { t } }) => value || t('rules.grid.maintenanceLog')
  },
  status: {
    headerName: 'rules.grid.status',
    field: 'status',
    filter: 'text',
    cellRendererFramework: RuleStatusRenderer
  },
  alertStatus: {
    headerName: 'rules.grid.status',
    field: 'status',
    filter: 'text',
    cellClass: 'capitalize-cell'
  },
  count: {
    headerName: 'rules.grid.count',
    field: 'count',
    cellRendererFramework: CountRenderer,
    comparator: compareByProperty('response', true)
  },
  lastTrigger: {
    headerName: 'rules.grid.lastTrigger',
    field: 'lastTrigger',
    cellRendererFramework: LastTriggerRenderer,
    comparator: compareByProperty('response', true),
    width: LAST_TRIGGER_DEFAULT_WIDTH
  },
  explore: {
    headerName: 'rules.grid.explore',
    field: 'ruleId',
    cellRendererFramework: props => <LinkRenderer {...props} to={`/maintenance/rule/${props.value}`} />
  }
};

export const defaultRulesGridProps = {
  enableColResize: true,
  multiSelect: true,
  pagination: true,
  paginationPageSize: Config.paginationPageSize,
  rowSelection: 'multiple'
};
