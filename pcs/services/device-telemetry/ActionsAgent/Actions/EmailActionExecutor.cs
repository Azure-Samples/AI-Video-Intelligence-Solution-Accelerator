// Copyright (c) Microsoft. All rights reserved.

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.ActionsAgent.Models;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Http;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Models.Actions;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Runtime;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.ActionsAgent.Actions
{
    public class EmailActionExecutor : IActionExecutor
    {
        private readonly IServicesConfig servicesConfig;
        private readonly IHttpClient httpClient;
        private readonly ILogger logger;

        private const string EMAIL_TEMPLATE_FILE_NAME = "EmailTemplate.html";
        private const string DATE_FORMAT_STRING = "r";

        public EmailActionExecutor(
            IServicesConfig servicesConfig,
            IHttpClient httpClient,
            ILogger logger)
        {
            this.servicesConfig = servicesConfig;
            this.httpClient = httpClient;
            this.logger = logger;
        }

        /// <summary>
        /// Execute the given email action for the given alarm.
        /// Sends a post request to Logic App with alarm information
        /// </summary>
        public async Task Execute(IAction action, object metadata)
        {
            if (metadata.GetType() != typeof(AsaAlarmApiModel)
                || action.GetType() != typeof(EmailAction))
            {
                string errorMessage = "Email action expects metadata to be alarm and action" + 
                                      " to be EmailAction, will not send email";
                this.logger.Error(errorMessage, () => { });
                return;
            }

            try
            {
                AsaAlarmApiModel alarm = (AsaAlarmApiModel)metadata;
                EmailAction emailAction = (EmailAction)action;
                string payload = this.GeneratePayload(emailAction, alarm);
                HttpRequest httpRequest = new HttpRequest(this.servicesConfig.LogicAppEndpointUrl);
                httpRequest.SetContent(payload);
                IHttpResponse response = await this.httpClient.PostAsync(httpRequest);
                if (!response.IsSuccess)
                {
                    this.logger.Error("Could not execute email action against logic app", () => { });
                }
            }
            catch (JsonException e)
            {
                this.logger.Error("Could not create email payload to send to logic app,", () => new { e });
            }
            catch (Exception e)
            {
                this.logger.Error("Could not execute email action against logic app", () => new { e });
            }
        }

        /**
         * Generate email payload for given alarm and email action.
         * Creates subject, recipients, and body based on action and alarm
         */
        private string GeneratePayload(EmailAction emailAction, AsaAlarmApiModel alarm)
        {
            string emailTemplate = File.ReadAllText(this.servicesConfig.TemplateFolder + EMAIL_TEMPLATE_FILE_NAME);
            string alarmDate = DateTimeOffset.FromUnixTimeMilliseconds(alarm.DateCreated).ToString(DATE_FORMAT_STRING);
            emailTemplate = emailTemplate.Replace("${subject}", emailAction.GetSubject());
            emailTemplate = emailTemplate.Replace(
                "${alarmDate}", 
                DateTimeOffset.FromUnixTimeMilliseconds(alarm.DateCreated).ToString(DATE_FORMAT_STRING));
            emailTemplate = emailTemplate.Replace("${ruleId}", alarm.RuleId);
            emailTemplate = emailTemplate.Replace("${ruleDescription}", alarm.RuleDescription);
            emailTemplate = emailTemplate.Replace("${ruleSeverity}", alarm.RuleSeverity);
            emailTemplate = emailTemplate.Replace("${deviceId}", alarm.DeviceId);
            emailTemplate = emailTemplate.Replace("${notes}", emailAction.GetNotes());
            emailTemplate = emailTemplate.Replace("${alarmUrl}", this.GenerateRuleDetailUrl(alarm.RuleId));

            EmailActionPayload payload = new EmailActionPayload
            {
                Recipients = emailAction.GetRecipients(),
                Subject = emailAction.GetSubject(),
                Body = emailTemplate
            };

            return JsonConvert.SerializeObject(payload);
        }

        /**
         * Generate URL to direct to maintenance dashboard for specific rule
         */
        private string GenerateRuleDetailUrl(string ruleId)
        {
            return this.servicesConfig.SolutionUrl + "/maintenance/rule/" + ruleId;
        }
    }
}
