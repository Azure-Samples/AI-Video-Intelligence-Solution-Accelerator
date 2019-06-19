// Copyright (c) Microsoft. All rights reserved.

import Config from 'app.config';
import { SoftSelectLinkRenderer, TimeRenderer } from 'components/shared/cellRenderers';
import { getPackageTypeTranslation, getConfigTypeTranslation } from 'utilities';
import { gridValueFormatters } from 'components/shared/pcsGrid/pcsGridConfig';

const { checkForEmpty } = gridValueFormatters;

export const deploymentsColumnDefs = {
  name: {
    headerName: 'deployments.grid.name',
    field: 'name',
    sort: 'asc',
    valueFormatter: ({ value }) => checkForEmpty(value),
    cellRendererFramework: SoftSelectLinkRenderer
  },
  package: {
    headerName: 'deployments.grid.package',
    field: 'packageName',
    valueFormatter: ({ value }) => checkForEmpty(value)
  },
  deviceGroup: {
    headerName: 'deployments.grid.deviceGroup',
    field: 'deviceGroupName',
    valueFormatter: ({ value }) => checkForEmpty(value)
  },
  priority: {
    headerName: 'deployments.grid.priority',
    field: 'priority',
    valueFormatter: ({ value }) => checkForEmpty(value)
  },
  packageType: {
    headerName: 'deployments.grid.packageType',
    field: 'packageType',
    valueFormatter: ({ value, context: { t } }) => getPackageTypeTranslation(checkForEmpty(value), t)
  },
  configType: {
    headerName: 'deployments.grid.configType',
    field: 'configType',
    valueFormatter: ({ value, context: { t } }) => getConfigTypeTranslation(checkForEmpty(value), t)
  },
  targeted: {
    headerName: 'deployments.grid.targeted',
    headerTooltip: 'deployments.grid.targetedTooltip',
    field: 'targetedCount',
    valueFormatter: ({ value }) => checkForEmpty(value)
  },
  applied: {
    headerName: 'deployments.grid.applied',
    headerTooltip: 'deployments.grid.appliedTooltip',
    field: 'appliedCount',
    valueFormatter: ({ value }) => checkForEmpty(value)
  },
  failed: {
    headerName: 'deployments.grid.failed',
    field: 'failedCount',
    valueFormatter: ({ value }) => checkForEmpty(value)
  },
  succeeded: {
    headerName: 'deployments.grid.succeeded',
    field: 'succeededCount',
    valueFormatter: ({ value }) => checkForEmpty(value)
  },
  dateCreated: {
    headerName: 'deployments.grid.dateCreated',
    field: 'createdDateTimeUtc',
    cellRendererFramework: TimeRenderer
  }
};

export const defaultDeploymentsGridProps = {
  enableColResize: true,
  pagination: true,
  paginationPageSize: Config.paginationPageSize,
  enableSorting: true,
  unSortIcon: true,
  sizeColumnsToFit: true,
  deltaRowDataMode: true
};
