// Copyright (c) Microsoft. All rights reserved.

import React, { Component } from 'react';
import { AgGridReact } from 'ag-grid-react';
import Rx from 'rxjs';
import Config from 'app.config';
import { isFunc } from 'utilities';
import { Indicator } from '../indicator/indicator';
import { ROW_HEIGHT } from './compactGridConfig';

import '../../../../node_modules/ag-grid-community/src/styles/ag-grid.scss';
import '../../../../node_modules/ag-grid-community/src/styles/ag-theme-dark.scss';
import './compactGrid.scss';

/**
 * CompactGrid is a helper wrapper around AgGrid. The primary functionality of this wrapper
 * is to allow easy reuse of the pcs dark grid theme. To see params, read the AgGrid docs.
 *
 * Props:
 *  getSoftSelectId: A method that when provided with the a row data object returns an id for that object
 *  softSelectId: The ID of the row data to be soft selected
 *  onHardSelectChange: Fires when rows are hard selected
 *  onSoftSelectChange: Fires when a row is soft selected
 * TODO (stpryor): Add design pagination
 */
export class CompactGrid extends Component {

  constructor(props) {
    super(props);
    this.state = {
      currentSoftSelectId: undefined
    };

    this.defaultCompactGridProps = {
      suppressDragLeaveHidesColumns: true,
      suppressCellSelection: true,
      suppressClickEdit: true,
      suppressRowClickSelection: true, // Suppress so that a row is only selectable by checking the checkbox
      suppressLoadingOverlay: true,
      suppressNoRowsOverlay: true
    };

    this.subscriptions = [];
    this.resizeEvents = new Rx.Subject();
  }

  componentDidMount() {
    this.subscriptions.push(
      this.resizeEvents
        .debounceTime(Config.gridResizeDebounceTime)
        .filter(() => !!this.gridApi && !!this.props.sizeColumnsToFit && window.outerWidth >= Config.gridMinResize)
        .subscribe(() => this.gridApi.sizeColumnsToFit())
    );
    window.addEventListener('resize', this.registerResizeEvent);
  }

  componentWillUnmount() {
    window.removeEventListener('resize', this.registerResizeEvent);
    this.subscriptions.forEach(sub => sub.unsubscribe());
  }

  registerResizeEvent = () => this.resizeEvents.next('r');

  /** When new props are passed in, check if the soft select state needs to be updated */
  componentWillReceiveProps(nextProps) {
    if (this.state.currentSoftSelectId !== nextProps.softSelectId) {
      this.setState({ currentSoftSelectId: nextProps.softSelectId }, this.refreshRows);
    }
    // Resize the grid if updating from 0 row data to 1+ rowData
    if (
      nextProps.rowData
      && nextProps.rowData.length
      && (!this.props.rowData || !this.props.rowData.length)
    ) this.resizeEvents.next('r');
  }

  /** Save the gridApi locally on load */
  onGridReady = gridReadyEvent => {
    this.gridApi = gridReadyEvent.api;
    if (this.props.sizeColumnsToFit) {
      this.resizeEvents.next('r');
    }
    if (isFunc(this.props.onGridReady)) {
      this.props.onGridReady(gridReadyEvent);
    }
  }

  /**
   * Refreshes the grid to update soft select CSS states
   * Forces and update event
   */
  refreshRows = () => {
    if (this.gridApi && isFunc(this.gridApi.updateRowData))
      this.gridApi.updateRowData({ update: [] });
  }

  /** When a row is hard selected, try to fire a hard select event, plus any props callbacks */
  onSelectionChanged = () => {
    const { onHardSelectChange, onSelectionChanged } = this.props;
    if (isFunc(onHardSelectChange)) {
      onHardSelectChange(this.gridApi.getSelectedRows());
    }
    if (isFunc(onSelectionChanged)) {
      onSelectionChanged();
    }
  };

  /** When a row is clicked, select the row unless a soft select link was clicked */
  onRowClicked = rowEvent => {
    const className = rowEvent.event.target.className;
    if (className.indexOf && className.indexOf('soft-select-link') === -1) {
      const { onRowClicked } = this.props;
      if (isFunc(onRowClicked)) onRowClicked(rowEvent);
    }
  };

  render() {
    const {
      onSoftSelectChange,
      getSoftSelectId,
      softSelectId,
      context = {},
      style,
      ...restProps
    } = this.props;
    const gridParams = {
      ...this.defaultCompactGridProps,
      ...restProps,
      headerHeight: ROW_HEIGHT,
      rowHeight: ROW_HEIGHT,
      onGridReady: this.onGridReady,
      onSelectionChanged: this.onSelectionChanged,
      onRowClicked: this.onRowClicked,
      domLayout: 'print',
      rowClassRules: {
        'compact-row-soft-selected': ({ data }) =>
          isFunc(getSoftSelectId)
            ? getSoftSelectId(data) === softSelectId
            : false
      },
      enableSorting: true,
      unSortIcon: true,
      context: {
        ...context,
        onSoftSelectChange, // Pass soft select logic to cell renderers
        getSoftSelectId // Pass soft select id logic to cell renderers
      }
    };
    const { rowData, compactLoadingTemplate } = this.props;

    const loadingContainer =
      <div className="compact-grid-loading-container">
        { !compactLoadingTemplate ? <Indicator /> : compactLoadingTemplate }
      </div>;

    return (
      <div className={`compact-grid-container ag-theme-dark ${gridParams.suppressMovableColumns ? '' : 'movable-columns'}`} style={style}>
        { !rowData ? loadingContainer : '' }
        <AgGridReact {...gridParams} />
      </div>
    );
  }
}
