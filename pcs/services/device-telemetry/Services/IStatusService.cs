// Copyright (c) Microsoft. All rights reserved.

using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Models;
using System.Threading.Tasks;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services
{
    public interface IStatusService
    {
        Task<StatusServiceModel> GetStatusAsync(bool authRequired);
    }
}
