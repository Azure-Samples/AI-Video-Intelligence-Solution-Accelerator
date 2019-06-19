// Copyright (c) Microsoft. All rights reserved.

import React, { Component } from 'react';

import { permissions, toDiagnosticsModel } from 'services/models';
import { RulesGrid } from './rulesGrid';
import { DeviceGroupDropdownContainer as DeviceGroupDropdown } from 'components/shell/deviceGroupDropdown';
import { ManageDeviceGroupsBtnContainer as ManageDeviceGroupsBtn } from 'components/shell/manageDeviceGroupsBtn';
import {
  AjaxError,
  Btn,
  ComponentArray,
  ContextMenu,
  ContextMenuAlign,
  PageContent,
  PageTitle,
  Protected,
  RefreshBarContainer as RefreshBar,
  SearchInput
} from 'components/shared';
import { NewRuleFlyout } from './flyouts';
import { svgs } from 'utilities';
import { toSinglePropertyDiagnosticsModel } from  'services/models';

import './rules.scss';

const closedFlyoutState = {
  openFlyoutName: '',
  selectedRuleId: undefined
};

export class Rules extends Component {
  constructor(props) {
    super(props);

    this.state = {
      ...closedFlyoutState,
      contextBtns: null
    };

    if (!this.props.lastUpdated && !this.props.error) {
      this.props.fetchRules();
    }

    this.props.updateCurrentWindow('Rules');

    if (this.props.applicationPermissionsAssigned !== undefined) {
      this.logApplicationPermissions(this.props.applicationPermissionsAssigned);
    }
  }

  componentWillReceiveProps(nextProps) {
    if (nextProps.isPending && nextProps.isPending !== this.props.isPending) {
      // If the grid data refreshes, hide the flyout and deselect soft selections
      this.setState(closedFlyoutState);
    }
    if (nextProps.applicationPermissionsAssigned !== undefined
      && nextProps.applicationPermissionsAssigned !== this.props.applicationPermissionsAssigned) {
      this.logApplicationPermissions(nextProps.applicationPermissionsAssigned);
    }
  }

  closeFlyout = () => this.setState(closedFlyoutState);

  openNewRuleFlyout = () => {
    const { logEvent } = this.props;
    this.setState({
      openFlyoutName: 'newRule',
      selectedRuleId: ''
    });
    logEvent(toDiagnosticsModel('Rule_NewClick', {}));
  }

  onGridReady = gridReadyEvent => this.rulesGridApi = gridReadyEvent.api;

  searchOnChange = ({ target: { value } }) => {
    if (this.rulesGridApi) this.rulesGridApi.setQuickFilter(value);
  };

  onContextMenuChange = contextBtns => this.setState({ contextBtns });

  logApplicationPermissions(applicationPermissionsAssigned) {
    if (applicationPermissionsAssigned !== undefined) {
      this.props.logEvent(toSinglePropertyDiagnosticsModel(
        'ApplicationPermissions',
        'AssignedPermissions',
        applicationPermissionsAssigned));
    }
  }

  render() {
    const {
      t,
      rules,
      error,
      isPending,
      lastUpdated,
      fetchRules,
      logEvent
    } = this.props;
    const gridProps = {
      onGridReady: this.onGridReady,
      rowData: isPending ? undefined : rules || [],
      onContextMenuChange: this.onContextMenuChange,
      t: this.props.t,
      deviceGroups: this.props.deviceGroups,
      refresh: fetchRules,
      logEvent: this.props.logEvent
    };
    return (
      <ComponentArray>
        <ContextMenu>
          <ContextMenuAlign left={true}>
            <DeviceGroupDropdown />
            <Protected permission={permissions.updateDeviceGroups}>
              <ManageDeviceGroupsBtn />
            </Protected>
          </ContextMenuAlign>
          <ContextMenuAlign>
            <SearchInput
            onChange={this.searchOnChange}
            placeholder={t('rules.searchPlaceholder')}
            aria-label={t('rules.ariaLabel')}/>
            {this.state.contextBtns}
            <Protected permission={permissions.createRules}>
              <Btn svg={svgs.plus} onClick={this.openNewRuleFlyout}>{t('rules.flyouts.newRule')}</Btn>
            </Protected>
            <RefreshBar refresh={fetchRules} time={lastUpdated} isPending={isPending} t={t} />
          </ContextMenuAlign>
        </ContextMenu>
        <PageContent className="rules-container">
          <PageTitle titleValue={t('rules.title')} />
          {!!error && <AjaxError t={t} error={error} />}
          {!error && <RulesGrid {...gridProps} />}
          {this.state.openFlyoutName === 'newRule' && <NewRuleFlyout t={t} onClose={this.closeFlyout} logEvent={logEvent} />}
        </PageContent>
      </ComponentArray>
    );
  }
}
