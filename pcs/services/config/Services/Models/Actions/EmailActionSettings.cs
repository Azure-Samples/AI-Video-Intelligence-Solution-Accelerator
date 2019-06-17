// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Exceptions;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.External;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Runtime;

namespace Microsoft.Azure.IoTSolutions.UIConfig.Services.Models.Actions
{
    public class EmailActionSettings : IActionSettings
    {
        private const string IS_ENABLED_KEY = "IsEnabled";
        private const string OFFICE365_CONNECTOR_URL_KEY = "Office365ConnectorUrl";
        private const string APP_PERMISSIONS_KEY = "ApplicationPermissionsAssigned";

        private readonly IAzureResourceManagerClient resourceManagerClient;
        private readonly IServicesConfig servicesConfig;
        private readonly ILogger log;

        public ActionType Type { get; }

        public IDictionary<string, object> Settings { get; set; }

        // In order to initialize all settings, call InitializeAsync
        // to retrieve all settings due to async call to logic app
        public EmailActionSettings(
            IAzureResourceManagerClient resourceManagerClient,
            IServicesConfig servicesConfig,
            ILogger log)
        {
            this.resourceManagerClient = resourceManagerClient;
            this.servicesConfig = servicesConfig;
            this.log = log;

            this.Type = ActionType.Email;
            this.Settings = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        }

        public async Task InitializeAsync()
        {
            // Check signin status of Office 365 Logic App Connector
            var office365IsEnabled = false;
            var applicationPermissionsAssigned = true;
            try
            {
                office365IsEnabled = await this.resourceManagerClient.IsOffice365EnabledAsync();
            }
            catch (NotAuthorizedException notAuthorizedException)
            {
                // If there is a 403 Not Authorized exception, it means the application has not
                // been given owner permissions to make the isEnabled check. This can be configured
                // by an owner in the Azure Portal.
                applicationPermissionsAssigned = false;
                this.log.Debug("The application is not authorized and has not been " +
                               "assigned owner permissions for the subscription. Go to the Azure portal and " +
                               "assign the application as an owner in order to retrieve the token.", () => new { notAuthorizedException });
            }
            this.Settings.Add(IS_ENABLED_KEY, office365IsEnabled);
            this.Settings.Add(APP_PERMISSIONS_KEY, applicationPermissionsAssigned);

            // Get Url for Office 365 Logic App Connector setup in portal
            // for display on the webui for one-time setup.
            this.Settings.Add(OFFICE365_CONNECTOR_URL_KEY, this.servicesConfig.Office365LogicAppUrl);

            this.log.Debug("Email Action Settings Retrieved. Email setup status: " + office365IsEnabled, () => new { this.Settings });
        }
    }
}
