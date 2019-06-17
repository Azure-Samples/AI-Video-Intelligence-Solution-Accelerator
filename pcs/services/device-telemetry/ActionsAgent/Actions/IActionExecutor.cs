// Copyright (c) Microsoft. All rights reserved.

using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Models.Actions;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.ActionsAgent.Actions
{
    public interface IActionExecutor
    {
        Task Execute(IAction action, object metadata);
    }
}
