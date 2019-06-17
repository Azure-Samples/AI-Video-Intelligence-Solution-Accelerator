// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Models.Actions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Services.Test.helpers;
using Xunit;

namespace Services.Test
{
    public class ActionConverterTest
    {
        private const string PARAM_NOTES = "Chiller pressure is at 250 which is high";
        private const string PARAM_SUBJECT = "Alert Notification";
        private const string PARAM_RECIPIENTS = "sampleEmail@gmail.com";
        private const string PARAM_NOTES_KEY = "Notes";
        private const string PARAM_RECIPIENTS_KEY = "Recipients";

        public ActionConverterTest() { }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void ItReturnsEmailAction_WhenEmailActionJsonPassed()
        {
            // Arrange

            const string SAMPLE_JSON = "[{\"Type\":\"Email\"," + 
                                 "\"Parameters\":{\"Notes\":\"" + PARAM_NOTES +
                                 "\",\"Subject\":\"" + PARAM_SUBJECT +
                                 "\",\"Recipients\":[\"" + PARAM_RECIPIENTS + "\"]}}]";

            // Act 
            var rulesList = JsonConvert.DeserializeObject<List<IAction>>(SAMPLE_JSON);

            // Assert 
            Assert.NotEmpty(rulesList);
            Assert.Equal(ActionType.Email, rulesList[0].Type);
            Assert.Equal(PARAM_NOTES, rulesList[0].Parameters[PARAM_NOTES_KEY]);
            Assert.Equal(new JArray { PARAM_RECIPIENTS }, rulesList[0].Parameters[PARAM_RECIPIENTS_KEY]);
        }
    }
}
