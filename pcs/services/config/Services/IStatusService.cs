// Copyright (c) Microsoft. All rights reserved.

using Microsoft.Azure.IoTSolutions.UIConfig.Services.Models;
using System.Threading.Tasks;

namespace Microsoft.Azure.IoTSolutions.UIConfig.Services
{
    public interface IStatusService
    {
        Task<StatusServiceModel> GetStatusAsync();
    }
}
