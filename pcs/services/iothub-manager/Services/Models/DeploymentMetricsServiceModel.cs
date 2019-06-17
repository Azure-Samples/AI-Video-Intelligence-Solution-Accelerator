// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using Microsoft.Azure.Devices;

namespace Microsoft.Azure.IoTSolutions.IotHubManager.Services.Models
{
    /// <summary>
    /// Statistics exposed by configuration queries
    /// </summary>
    public class DeploymentMetricsServiceModel
    {
        public IDictionary<string, long> SystemMetrics { get; set; }
        public IDictionary<string, long> CustomMetrics { get; set; }
        public IDictionary<DeploymentStatus, long> DeviceMetrics { get; set; }
        public IDictionary<string, DeploymentStatus> DeviceStatuses { get; set; }

        public DeploymentMetricsServiceModel(
            ConfigurationMetrics systemMetrics,
            ConfigurationMetrics customMetrics)
        {
            this.SystemMetrics = systemMetrics?.Results;
            this.CustomMetrics = customMetrics?.Results;
        }
    }
}
