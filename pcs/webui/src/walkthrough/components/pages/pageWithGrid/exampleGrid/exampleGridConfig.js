// Copyright (c) Microsoft. All rights reserved.

// <gridconfig>

import Config from 'app.config';
import { SoftSelectLinkRenderer } from 'components/shared/cellRenderers';

/** A collection of column definitions for the example grid */
export const exampleColumnDefs = {
  id: {
    headerName: 'walkthrough.pageWithGrid.grid.name',
    field: 'id',
    sort: 'asc',
    cellRendererFramework: SoftSelectLinkRenderer
  },
  description: {
    headerName: 'walkthrough.pageWithGrid.grid.description',
    field: 'descr'
  }
};

/** Given an example object, extract and return the device Id */
export const getSoftSelectId = ({ id }) => id;

/** Shared example grid AgGrid properties */
export const defaultExampleGridProps = {
  enableColResize: true,
  multiSelect: true,
  pagination: false,
  paginationPageSize: Config.paginationPageSize,
  rowSelection: 'multiple'
};

// </gridconfig>
