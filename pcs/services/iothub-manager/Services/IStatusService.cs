// Copyright (c) Microsoft. All rights reserved.

using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Models;
using System.Threading.Tasks;

namespace Microsoft.Azure.IoTSolutions.IotHubManager.Services
{
    public interface IStatusService
    {
        Task<StatusServiceModel> GetStatusAsync(bool authRequired);
    }
}
