// Copyright (c) Microsoft. All rights reserved.
/* This file contains default values useful for creating CompactGrid */
import Config from 'app.config';

export const ROW_HEIGHT = 33; // has to match $rowHeight in compactGrid.scss

/** The default value for CompactGrid cells that are empty */
export const EMPTY_FIELD_VAL = Config.emptyFieldValue;

/** A collection of reusable value formatter methods */
export const gridValueFormatters = {
  checkForEmpty: (value, emptyValue = EMPTY_FIELD_VAL) => value || emptyValue
};

/** A the class name for the first row in a grid (used for soft and hard selection ) */
export const FIRST_COLUMN_CLASS = 'first-child-column';
export const CHECKBOX_COLUMN_CLASS = 'checkbox-column';

export const checkboxColumn = {
  lockPosition: true,
  cellClass: FIRST_COLUMN_CLASS,
  headerClass: CHECKBOX_COLUMN_CLASS,
  suppressResize: true,
  checkboxSelection: true,
  headerCheckboxSelection: true,
  headerCheckboxSelectionFilteredOnly: true,
  suppressMovable: true,
  width: 25
};
