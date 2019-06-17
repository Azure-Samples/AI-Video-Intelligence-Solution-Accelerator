// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Linq;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.ActionsAgent.Actions;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.ActionsAgent.Models;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Models.Actions;
using Moq;
using Xunit;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.ActionsAgent.Test
{
    public class AlarmParserTest
    {
        private readonly Mock<ILogger> loggerMock;

        public AlarmParserTest()
        {
            this.loggerMock = new Mock<ILogger>();
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void CanParse_ProperlyFormattedJson()
        {
            // Arrange
            string data = "{\"created\":1539035437937,\"modified\":1539035437937,\"rule.description\":\"description\",\"rule.severity\":\"Warning\",\"rule.id\":\"TestRuleId\",\"rule.actions\":[{\"Type\":\"Email\",\"Parameters\":{\"Notes\":\"Test Note\",\"Subject\":\"Test Subject\",\"Recipients\":[\"sampleEmail@gmail.com\"]}}],\"device.id\":\"Test Device Id\",\"device.msg.received\":1539035437937}" +
                          "{\"created\":1539035437940,\"modified\":1539035437940,\"rule.description\":\"description2\",\"rule.severity\":\"Info\",\"rule.id\":\"1234\",\"device.id\":\"Device Id\",\"device.msg.received\":1539035437940}";

            // Act
            var result = AlarmParser.ParseAlarmList(data, this.loggerMock.Object);

            // Assert
            AsaAlarmApiModel[] resultArray = result.ToArray();
            Assert.Equal(2, resultArray.Length);
            Assert.Equal("description", resultArray[0].RuleDescription);
            Assert.Equal("description2", resultArray[1].RuleDescription);
            Assert.Equal(1, resultArray[0].Actions.Count);
            var action = resultArray[0].Actions[0];
            Assert.Equal(ActionType.Email, action.Type);
            var recipients = ((EmailAction)action).GetRecipients();
            Assert.Single(recipients);
            Assert.Equal("sampleEmail@gmail.com", recipients[0]);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void LogsError_OnImproperlyFormattedJson()
        {
            // Arrange
            // no commas 
            string data = "{\"created\":1539035437937\"modified\":1539035437937\"rule.description\":\"description\"\"rule.severity\":\"Warning\"}";

            // Act
            var result = AlarmParser.ParseAlarmList(data, this.loggerMock.Object);

            // Assert
            this.loggerMock.Verify(x => x.Error(It.IsAny<string>(), It.IsAny<Func<object>>()));
            Assert.Empty(result);
        }
    }
}
