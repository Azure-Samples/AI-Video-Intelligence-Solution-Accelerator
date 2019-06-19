// Copyright (c) Microsoft. All rights reserved.

import AckAlertIconPath from 'assets/icons/ackAlert.svg';
import ApplyIconPath from 'assets/icons/apply.svg';
import CancelXIconPath from 'assets/icons/cancelX.svg';
import CaratIconPath from 'assets/icons/carat.svg';
import ChangeStatusPath from 'assets/icons/changeStatus.svg';
import CheckmarkIconPath from 'assets/icons/checkmark.svg';
import ChevronIconPath from 'assets/icons/chevron.svg';
import ChevronRightIconPath from 'assets/icons/chevronRight.svg';
import CloseAlertIconPath from 'assets/icons/closeAlert.svg';
import ColonIconPath from 'assets/icons/colon.svg';
import ContosoIconPath from 'assets/icons/contoso.svg';
import CopyIconPath from 'assets/icons/copy.svg';
import CriticalIconPath from 'assets/icons/critical.svg';
import DeviceChillerIconPath from 'assets/icons/deviceChiller.svg';
import DeviceElevatorIconPath from 'assets/icons/deviceElevator.svg';
import DeviceEngineIconPath from 'assets/icons/deviceEngine.svg';
import DeviceGenericIconPath from 'assets/icons/deviceIcon.svg';
import DevicePrototypingIconPath from 'assets/icons/devicePrototyping.svg';
import DeviceTruckIconPath from 'assets/icons/deviceTruck.svg';
import DisableToggleIconPath from 'assets/icons/disableToggle.svg';
import DisabledIconPath from 'assets/icons/disabled.svg';
import EditIconPath from 'assets/icons/edit.svg';
import EllipsisIconPath from 'assets/icons/ellipsis.svg';
import EnableToggleIconPath from 'assets/icons/enableToggle.svg';
import ErrorIconPath from 'assets/icons/errorAsterisk.svg';
import FailedIconPath from 'assets/icons/failed.svg';
import GlimmerIconPath from 'assets/icons/glimmer.svg';
import HamburgerIconPath from 'assets/icons/hamburger.svg';
import InfoBubbleIconPath from 'assets/icons/infoBubble.svg';
import InfoIconPath from 'assets/icons/info.svg';
import LinkToPath from 'assets/icons/linkTo.svg';
import LoadingToggleIconPath from 'assets/icons/loadingToggle.svg';
import ManageFiltersIconPath from 'assets/icons/manageFilters.svg';
import TabPackagesIconPath from 'assets/icons/packages.svg';
import PhysicalDeviceIconPath from 'assets/icons/physicalDevice.svg';
import PlusIconPath from 'assets/icons/plus.svg';
import QuestionMarkIconPath from 'assets/icons/questionMark.svg';
import RadioSelectedIconPath from 'assets/icons/radioSelected.svg';
import RadioUnselectedIconPath from 'assets/icons/radioUnselected.svg';
import ReconfigureIconPath from 'assets/icons/reconfigure.svg';
import RefreshIconPath from 'assets/icons/refresh.svg';
import RuleDisabledIconPath from 'assets/icons/ruleDisabled.svg';
import RuleEnabledIconPath from 'assets/icons/ruleEnabled.svg';
import SIMManagementIconPath from 'assets/icons/SIMManagement.svg';
import SearchIconPath from 'assets/icons/searchIcon.svg';
import SettingsIconPath from 'assets/icons/settings.svg';
import SimulatedDeviceIconPath from 'assets/icons/simulatedDevice.svg';
import TabDashboardIconPath from 'assets/icons/tabDashboard.svg';
import TabDevicesIconPath from 'assets/icons/tabDevices.svg';
import TabDeploymentsIconPath from 'assets/icons/tabDeployments.svg';
import TabMaintenanceIconPath from 'assets/icons/tabMaintenance.svg';
import TabRulesIconPath from 'assets/icons/tabRules.svg';
import TrashIconPath from 'assets/icons/trash.svg';
import UploadIconPath from 'assets/icons/upload.svg';
import WarningIconPath from 'assets/icons/warning.svg';
import XIconPath from 'assets/icons/x.svg';

/** A helper object mapping svg names to their paths */
export const svgs = {
  tabs: {
    dashboard: TabDashboardIconPath,
    devices: TabDevicesIconPath,
    deployments: TabDeploymentsIconPath,
    maintenance: TabMaintenanceIconPath,
    packages: TabPackagesIconPath,
    rules: TabRulesIconPath,
    example: InfoBubbleIconPath
  },
  devices: {
    generic: DeviceGenericIconPath,
    chiller: DeviceChillerIconPath,
    elevator: DeviceElevatorIconPath,
    engine: DeviceEngineIconPath,
    prototyping: DevicePrototypingIconPath,
    truck: DeviceTruckIconPath
  },
  ackAlert: AckAlertIconPath,
  apply: ApplyIconPath,
  cancelX: CancelXIconPath,
  carat: CaratIconPath,
  changeStatus: ChangeStatusPath,
  checkmark: CheckmarkIconPath,
  chevron: ChevronIconPath,
  chevronRight: ChevronRightIconPath,
  closeAlert: CloseAlertIconPath,
  colon: ColonIconPath,
  contoso: ContosoIconPath,
  copy: CopyIconPath,
  critical: CriticalIconPath,
  disableToggle: DisableToggleIconPath,
  disabled: DisabledIconPath,
  edit: EditIconPath,
  ellipsis: EllipsisIconPath,
  enableToggle: EnableToggleIconPath,
  error: ErrorIconPath,
  failed: FailedIconPath,
  glimmer: GlimmerIconPath,
  hamburger: HamburgerIconPath,
  info: InfoIconPath,
  infoBubble: InfoBubbleIconPath,
  linkTo: LinkToPath,
  loadingToggle: LoadingToggleIconPath,
  manageFilters: ManageFiltersIconPath,
  physicalDevice: PhysicalDeviceIconPath,
  plus: PlusIconPath,
  questionMark: QuestionMarkIconPath,
  radioSelected: RadioSelectedIconPath,
  radioUnselected: RadioUnselectedIconPath,
  reconfigure: ReconfigureIconPath,
  refresh: RefreshIconPath,
  ruleDisabled: RuleDisabledIconPath,
  ruleEnabled: RuleEnabledIconPath,
  search: SearchIconPath,
  settings: SettingsIconPath,
  simmanagement: SIMManagementIconPath,
  simulatedDevice: SimulatedDeviceIconPath,
  trash: TrashIconPath,
  upload: UploadIconPath,
  warning: WarningIconPath,
  x: XIconPath
};
