// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.ActionsAgent.Models;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Http;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Models.Actions;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Runtime;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.ActionsAgent.Actions
{
    public interface IActionManager
    {
        Task ExecuteAlarmActions(IEnumerable<AsaAlarmApiModel> alarms);
    }

    public class ActionManager : IActionManager
    {
        private readonly IActionExecutor emailActionExecutor;

        public ActionManager(ILogger logger, IServicesConfig servicesConfig, IHttpClient httpClient)
        {
            this.emailActionExecutor = new EmailActionExecutor(
                servicesConfig,
                httpClient,
                logger);
        }

        /**
         * Given a string of alarms in format {AsaAlarmApiModel1}...{AsaAlarmApiModelN}
         * For each alarm with an action, execute that action
         */
        public async Task ExecuteAlarmActions(IEnumerable<AsaAlarmApiModel> alarms)
        {
            IEnumerable<AsaAlarmApiModel> alarmList = alarms.Where(x => x.Actions != null && x.Actions.Count > 0);
            List<Task> actionList = new List<Task>();
            foreach (var alarm in alarmList)
            {
                foreach (var action in alarm.Actions)
                {
                    switch (action.Type)
                    {
                        case ActionType.Email:
                            actionList.Add(this.emailActionExecutor.Execute((EmailAction)action, alarm));
                            break;
                    }
                }
            }

            await Task.WhenAll(actionList);
        }
    }
}
