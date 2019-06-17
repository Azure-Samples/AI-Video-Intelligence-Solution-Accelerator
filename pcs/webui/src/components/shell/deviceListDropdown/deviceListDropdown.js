// Copyright (c) Microsoft. All rights reserved.

import React, { Component } from 'react';
import { SelectInput } from '@microsoft/azure-iot-ux-fluent-controls/lib/components/Input/SelectInput';

//import { toDiagnosticsModel } from 'services/models';

import './deviceListDropdown.scss';

export class DeviceListDropdown extends Component {

  onChange = (cameraIds) => (value) => {
    //this.props.logEvent(toDiagnosticsModel('DeviceListFilter_Select', {}));
    // Don't try to update the camera list if the camera id doesn't exist
    if (cameraIds.indexOf(value) > -1) {
      this.props.changeSelectedCamera(value);
    }
    //this.props.logEvent(toDiagnosticsModel('DeviceFilter_Select', {}));
  }

  camerasToOptions = camIds => camIds
    .map(( value ) => ({ label: value, value: value }));


  render() {
    const { activeCameraId, selectCameraPrompt, cameras } = this.props;

    // if we don't have an activeCamera, show "Select a camera" as the first option
    const cameraIds = activeCameraId ? cameras : [selectCameraPrompt].concat(cameras);

    return (
      <SelectInput
        name="device-list-dropdown"
        className="device-list-dropdown"
        attr={{
          select: {
            className: "device-list-dropdown-select",
            'aria-label': this.props.t(`deviceGroupDropDown.ariaLabel`)
          },
          chevron: {
            className: "device-list-dropdown-chevron",
          },
        }}
        options={this.camerasToOptions(cameraIds)}
        value={activeCameraId}
        onChange={this.onChange(cameraIds)} />
    );
  }
}
