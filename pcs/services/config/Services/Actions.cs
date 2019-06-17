// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.External;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Models.Actions;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Runtime;

namespace Microsoft.Azure.IoTSolutions.UIConfig.Services
{
    public interface IActions
    {
        Task<List<IActionSettings>> GetListAsync();
    }

    public class Actions : IActions
    {
        private readonly IAzureResourceManagerClient resourceManagerClient;
        private readonly IServicesConfig servicesConfig;
        private readonly ILogger log;

        public Actions(
            IAzureResourceManagerClient resourceManagerClient,
            IServicesConfig servicesConfig,
            ILogger log)
        {
            this.resourceManagerClient = resourceManagerClient;
            this.servicesConfig = servicesConfig;
            this.log = log;
        }

        public async Task <List<IActionSettings>> GetListAsync()
        {
            var result = new List<IActionSettings>();

            // Add Email Action Settings
            var emailActionSettings = new EmailActionSettings(
                this.resourceManagerClient,
                this.servicesConfig,
                this.log);
            await emailActionSettings.InitializeAsync();
            result.Add(emailActionSettings);

            return result;
        }
    }
}
