// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Models;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.WebService.v1.Controllers.Helpers;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.WebService.v1.Exceptions;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.WebService.v1.Filters;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.WebService.v1.Models;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.WebService.v1.Controllers
{
    [Route(Version.PATH + "/[controller]"), TypeFilter(typeof(ExceptionsFilterAttribute))]
    public class AlarmsByRuleController : Controller
    {
        private const int DEVICE_LIMIT = 1000;

        private readonly IAlarms alarmService;
        private readonly IRules ruleService;
        private readonly ILogger log;

        public AlarmsByRuleController(
            IAlarms alarmService,
            IRules ruleService,
            ILogger logger)
        {
            this.alarmService = alarmService;
            this.ruleService = ruleService;
            this.log = logger;
        }

        [HttpGet]
        [Authorize("ReadAll")]
        public async Task<AlarmByRuleListApiModel> GetAsync(
            [FromQuery] string from,
            [FromQuery] string to,
            [FromQuery] string order,
            [FromQuery] int? skip,
            [FromQuery] int? limit,
            [FromQuery] string devices)
        {
            string[] deviceIds = new string[0];
            if (!string.IsNullOrEmpty(devices))
            {
                deviceIds = devices.Split(',');
            }

            return await this.GetAlarmCountByRuleHelper(from, to, order, skip, limit, deviceIds);
        }

        [HttpPost]
        [Authorize("ReadAll")]
        public async Task<AlarmByRuleListApiModel> PostAsync([FromBody] QueryApiModel body)
        {
            string[] deviceIds = body.Devices == null
                ? new string[0]
                : body.Devices.ToArray();

            return await this.GetAlarmCountByRuleHelper(
                body.From,
                body.To,
                body.Order,
                body.Skip,
                body.Limit,
                deviceIds);
        }

        [HttpGet("{id}")]
        [Authorize("ReadAll")]
        public AlarmListByRuleApiModel Get(
            [FromRoute] string id,
            [FromQuery] string from,
            [FromQuery] string to,
            [FromQuery] string order,
            [FromQuery] int? skip,
            [FromQuery] int? limit,
            [FromQuery] string devices)
        {
            string[] deviceIds = new string[0];
            if (!string.IsNullOrEmpty(devices))
            {
                deviceIds = devices.Split(',');
            }

            return this.GetAlarmListByRuleHelper(id, from, to, order, skip, limit, deviceIds);
        }

        [HttpPost("{id}")]
        [Authorize("ReadAll")]
        public AlarmListByRuleApiModel Post(
            [FromRoute] string id,
            [FromBody] QueryApiModel body)
        {
            string[] deviceIds = body.Devices == null
                ? new string[0]
                : body.Devices.ToArray();

            return this.GetAlarmListByRuleHelper(
                id,
                body.From,
                body.To,
                body.Order,
                body.Skip,
                body.Limit,
                deviceIds);
        }

        private async Task<AlarmByRuleListApiModel> GetAlarmCountByRuleHelper(
            string from,
            string to,
            string order,
            int? skip,
            int? limit,
            string[] deviceIds)
        {
            DateTimeOffset? fromDate = DateHelper.ParseDate(from);
            DateTimeOffset? toDate = DateHelper.ParseDate(to);

            if (order == null) order = "asc";
            if (skip == null) skip = 0;
            if (limit == null) limit = 1000;

            /* TODO: move this logic to the storage engine, depending on the
             * storage type the limit will be different. DEVICE_LIMIT is CosmosDb
             * limit for the IN clause.
             */
            if (deviceIds.Length > DEVICE_LIMIT)
            {
                this.log.Warn("The client requested too many devices", () => new { deviceIds.Length });
                throw new BadRequestException("The number of devices cannot exceed " + DEVICE_LIMIT);
            }

            List<AlarmCountByRule> alarmsList
                = await this.ruleService.GetAlarmCountForListAsync(
                    fromDate,
                    toDate,
                    order,
                    skip.Value,
                    limit.Value,
                    deviceIds);

            return new AlarmByRuleListApiModel(alarmsList);
        }

        private AlarmListByRuleApiModel GetAlarmListByRuleHelper(
            string id,
            string from,
            string to,
            string order,
            int? skip,
            int? limit,
            string[] deviceIds)
        {
            DateTimeOffset? fromDate = DateHelper.ParseDate(from);
            DateTimeOffset? toDate = DateHelper.ParseDate(to);

            if (order == null) order = "asc";
            if (skip == null) skip = 0;
            if (limit == null) limit = 1000;

            /* TODO: move this logic to the storage engine, depending on the
             * storage type the limit will be different. DEVICE_LIMIT is CosmosDb
             * limit for the IN clause.
             */
            if (deviceIds.Length > DEVICE_LIMIT)
            {
                this.log.Warn("The client requested too many devices", () => new { deviceIds.Length });
                throw new BadRequestException("The number of devices cannot exceed " + DEVICE_LIMIT);
            }

            List<Alarm> alarmsList = this.alarmService.ListByRule(
                id,
                fromDate,
                toDate,
                order,
                skip.Value,
                limit.Value,
                deviceIds);

            return new AlarmListByRuleApiModel(alarmsList);
        }
    }
}
