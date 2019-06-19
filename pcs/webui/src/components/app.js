// Copyright (c) Microsoft. All rights reserved.

import React, { Component } from 'react';

import { permissions } from 'services/models';
import { svgs } from 'utilities';
import { Protected } from 'components/shared';
import Shell from "components/shell/shell";
import { ManageDeviceGroupsContainer, SettingsContainer, HelpContainer, ProfileContainer } from 'components/shell/flyouts';
import {
  DashboardContainer,
  DevicesContainer,
  RulesContainer,
  MaintenanceContainer,
  PackagesContainer,
  DeploymentsRouter
} from './pages';

class App extends Component {

  constructor(props) {
    super(props);

    this.state = { openFlyout: '' };
  }

  closeFlyout = () => this.setState({ openFlyout: '' });

  openSystemSettings = () => this.setState({ openFlyout: 'settings' });
  openHelpFlyout = () => this.setState({ openFlyout: 'help' });
  openUserProfile = () => this.setState({ openFlyout: 'profile' });

  render() {
    const { deviceGroupFlyoutIsOpen } = this.props;
    const { openFlyout } = this.state;

    const pagesConfig = [
      {
        to: '/dashboard',
        exact: true,
        svg: svgs.tabs.dashboard,
        labelId: 'tabs.dashboard',
        component: DashboardContainer
      },
      {
        to: '/devices',
        exact: true,
        svg: svgs.tabs.devices,
        labelId: 'tabs.devices',
        component: DevicesContainer
      },
      {
        to: '/rules',
        exact: true,
        svg: svgs.tabs.rules,
        labelId: 'tabs.rules',
        component: RulesContainer
      },
      {
        to: '/packages',
        exact: true,
        svg: svgs.tabs.packages,
        labelId: 'tabs.packages',
        component: PackagesContainer
      },
      {
        to: '/deployments',
        exact: false,
        svg: svgs.tabs.deployments,
        labelId: 'tabs.deployments',
        component: DeploymentsRouter
      },
      {
        to: '/maintenance',
        exact: false,
        svg: svgs.tabs.maintenance,
        labelId: 'tabs.maintenance',
        component: MaintenanceContainer
      }
    ];

    const crumbsConfig = [
      {
        path: '/dashboard', crumbs: [
          { to: '/dashboard', labelId: 'tabs.dashboard' }
        ]
      },
      {
        path: '/devices', crumbs: [
          { to: '/devices', labelId: 'tabs.devices' }
        ]
      },
      {
        path: '/rules', crumbs: [
          { to: '/rules', labelId: 'tabs.rules' }
        ]
      },
      {
        path: '/packages', crumbs: [
          { to: '/packages', labelId: 'tabs.packages' }
        ]
      },
      {
        path: '/deployments', crumbs: [
          { to: '/deployments', labelId: 'tabs.deployments' }
        ]
      },
      {
        path: '/deployments/:id', crumbs: [
          { to: '/deployments', labelId: 'tabs.deployments' },
          { to: '/deployments/:id', matchParam: 'id' },
        ]
      },
      {
        path: '/maintenance', crumbs: [
          { to: '/maintenance', labelId: 'tabs.maintenance' }
        ]
      },
      {
        path: '/maintenance/notifications', crumbs: [
          { to: '/maintenance', labelId: 'tabs.maintenance' },
          { to: '/maintenance/notifications', labelId: 'maintenance.notifications' }
        ]
      },
      {
        path: '/maintenance/rule/:id', crumbs: [
          { to: '/maintenance', labelId: 'tabs.maintenance' },
          { to: '/maintenance/notifications', labelId: 'maintenance.notifications' },
          { to: '/maintenance/rule/:id', matchParam: 'id' },
        ]
      },
      {
        path: '/maintenance/jobs', crumbs: [
          { to: '/maintenance', labelId: 'tabs.maintenance' },
          { to: '/maintenance/jobs', labelId: 'maintenance.jobs' }
        ]
      },
      {
        path: '/maintenance/job/:id', crumbs: [
          { to: '/maintenance', labelId: 'tabs.maintenance' },
          { to: '/maintenance/jobs', labelId: 'maintenance.jobs' },
          { to: '/maintenance/rule/:id', matchParam: 'id' }
        ]
      }
    ];

    const shellProps = {
      pagesConfig,
      crumbsConfig,
      openSystemSettings: this.openSystemSettings,
      openHelpFlyout: this.openHelpFlyout,
      openUserProfile: this.openUserProfile,
      openFlyout: this.state.openFlyout,
      ...this.props
    };

    return (
      <Protected permission={permissions.readAll}>
        {
          (hasPermission) =>
            <Shell denyAccess={!hasPermission} {...shellProps}>
              {deviceGroupFlyoutIsOpen && <ManageDeviceGroupsContainer />}
              {openFlyout === 'settings' && <SettingsContainer onClose={this.closeFlyout} />}
              {openFlyout === 'help' && <HelpContainer onClose={this.closeFlyout} />}
              {openFlyout === 'profile' && <ProfileContainer onClose={this.closeFlyout} />}
            </Shell>
        }
      </Protected>
    );
  }
}

export default App;
