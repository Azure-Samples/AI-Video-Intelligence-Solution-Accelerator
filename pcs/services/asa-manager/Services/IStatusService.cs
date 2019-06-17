// Copyright (c) Microsoft. All rights reserved.

using Microsoft.Azure.IoTSolutions.AsaManager.Services.Models;
using System.Threading.Tasks;

namespace Microsoft.Azure.IoTSolutions.AsaManager.Services
{
    public interface IStatusService
    {
        Task<StatusServiceModel> GetStatusAsync();
    }
}
