// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;

namespace Microsoft.Azure.IoTSolutions.IotHubManager.Services.Models
{
    public class DeploymentServiceListModel
    {
        public List<DeploymentServiceModel> Items { get; set; }

        public DeploymentServiceListModel(List<DeploymentServiceModel> items)
        {
            this.Items = items;
        }
    }
}
