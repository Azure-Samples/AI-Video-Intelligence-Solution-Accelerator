// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Exceptions;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Models;
using Microsoft.Azure.IoTSolutions.IotHubManager.WebService.v1.Controllers;
using Microsoft.Extensions.Primitives;
using Moq;
using Newtonsoft.Json.Linq;
using WebService.Test.helpers;
using Xunit;

namespace WebService.Test.v1.Controllers
{
    public class ModulesControllerTest
    {
        private readonly ModulesController modulesController;
        private readonly Mock<IDevices> devicesMock;
        private readonly HttpContext httpContext;
        private const string CONTINUATION_TOKEN_NAME = "x-ms-continuation";

        public ModulesControllerTest()
        {
            this.devicesMock = new Mock<IDevices>();
            this.httpContext = new DefaultHttpContext();
            this.modulesController = new ModulesController(this.devicesMock.Object)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = this.httpContext
                }
            };
        }

        [Theory, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        [InlineData("", "", true)]
        [InlineData("deviceId", "", true)]
        [InlineData("", "moduleId", true)]
        [InlineData("deviceId", "moduleId", false)]
        public async Task GetSingleModuleTwinTest(string deviceId, string moduleId, bool throwsException)
        {
            if (throwsException)
            {
                await Assert.ThrowsAsync<InvalidInputException>(async () =>
                    await this.modulesController.GetModuleTwinAsync(deviceId, moduleId));
            }
            else
            {
                // Arrange
                var twinResult = ModulesControllerTest.CreateTestTwin(deviceId, moduleId);
                this.devicesMock.Setup(x => x.GetModuleTwinAsync(deviceId, moduleId))
                    .ReturnsAsync(twinResult);

                // Act
                var module = await this.modulesController.GetModuleTwinAsync(deviceId, moduleId);

                // Assert
                Assert.Equal(moduleId, module.ModuleId);
                Assert.Equal(deviceId, module.DeviceId);
                Assert.Equal("v2", module.Desired["version"]);
                Assert.Equal("v1", module.Reported["version"]);
            }
        }

        [Theory, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        [InlineData("", "")]
        [InlineData("my module query", "continuationToken")]
        public async Task GetModuleTwinsTest(string query, string continuationToken)
        {
            const string resultToken = "nextToken";

            var twinList = new List<TwinServiceModel>() {ModulesControllerTest.CreateTestTwin("d", "m")};
            var twins = new TwinServiceListModel(twinList, resultToken);

            this.devicesMock.Setup(x => x.GetModuleTwinsByQueryAsync(query, continuationToken))
                .ReturnsAsync(twins);
            this.httpContext.Request.Headers.Add(CONTINUATION_TOKEN_NAME,
                                                 new StringValues(continuationToken));

            // Act
            var moduleTwins = await this.modulesController.GetModuleTwinsAsync(query);

            // Assert
            var moduleTwin = moduleTwins.Items[0];
            Assert.Equal("d", moduleTwin.DeviceId);
            Assert.Equal("m", moduleTwin.ModuleId);
            Assert.Equal(resultToken, moduleTwins.ContinuationToken);
            Assert.Equal("v2", moduleTwin.Desired["version"]);
            Assert.Equal("v1", moduleTwin.Reported["version"]);
        }

        private static TwinServiceModel CreateTestTwin(string deviceId, string moduleId)
        {
            return new TwinServiceModel()
            {
                DeviceId = deviceId,
                ModuleId = moduleId,
                DesiredProperties = new Dictionary<string, JToken>()
                {
                    { "version", JToken.Parse("'v2'") }
                },
                ReportedProperties = new Dictionary<string, JToken>()
                {
                    { "version", JToken.Parse("'v1'") }
                }
            };
        }
    }
}
