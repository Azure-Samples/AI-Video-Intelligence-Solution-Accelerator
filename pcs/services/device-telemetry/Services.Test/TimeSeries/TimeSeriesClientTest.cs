// Copyright (c) Microsoft. All rights reserved.

using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Exceptions;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Http;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Runtime;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Storage.TimeSeries;
using Moq;
using Services.Test.helpers;
using Xunit;

namespace Services.Test.TimeSeries
{
    public class TimeSeriesClientTest
    {
        private readonly Mock<ILogger> logger;
        private readonly Mock<IHttpClient> httpClient;
        private Mock<IServicesConfig> servicesConfig;
        private  TimeSeriesClient client;

        public TimeSeriesClientTest()
        {
            this.logger = new Mock<ILogger>();
            this.servicesConfig = new Mock<IServicesConfig>();
            this.httpClient = new Mock<IHttpClient>();
            this.client = new TimeSeriesClient(
                this.httpClient.Object,
                this.servicesConfig.Object,
                this.logger.Object);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async Task QueryThrowsInvalidConfiguration_WhenConfigValuesAreNull()
        {
            // Arrange 
            this.SetupClientWithNullConfigValues();

            // Act & Assert
            await Assert.ThrowsAsync<InvalidConfigurationException>(() =>
                 this.client.QueryEventsAsync(null, null, "desc", 0, 1000, new string[0]));
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async Task PingReturnsFalse_WhenConfigValuesAreNull()
        {
            // Arrange
            this.SetupClientWithNullConfigValues();

            // Act
            var result = await this.client.PingAsync();

            // Assert
            Assert.False(result.IsHealthy);
            Assert.Contains("TimeSeries check failed", result.Message);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async Task QueryThrows_IfInvalidAuthParams()
        {
            // Arrange
           this.SetupClientWithConfigValues();

            // Act & Assert
            await Assert.ThrowsAsync<InvalidConfigurationException>(() =>
                this.client.QueryEventsAsync(null, null, "desc", 0, 1000, new string[0]));
        }

        private void SetupClientWithNullConfigValues()
        {
            this.servicesConfig = new Mock<IServicesConfig>();
            this.client = new TimeSeriesClient(
                this.httpClient.Object,
                this.servicesConfig.Object,
                this.logger.Object);
        }

        private void SetupClientWithConfigValues()
        {
            this.servicesConfig.Setup(f => f.TimeSeriesFqdn).Returns("test123");
            this.servicesConfig.Setup(f => f.TimeSeriesAudience).Returns("test123");
            this.servicesConfig.Setup(f => f.TimeSertiesApiVersion).Returns("2016-12-12-test");
            this.servicesConfig.Setup(f => f.TimeSeriesTimeout).Returns("PT20S");
            this.servicesConfig.Setup(f => f.ActiveDirectoryTenant).Returns("test123");
            this.servicesConfig.Setup(f => f.ActiveDirectoryAppId).Returns("test123");
            this.servicesConfig.Setup(f => f.ActiveDirectoryAppSecret).Returns("test123");
            this.servicesConfig.Setup(f => f.TimeSeriesAuthority).Returns("https://login.testing.net/");

            this.client = new TimeSeriesClient(
                this.httpClient.Object,
                this.servicesConfig.Object,
                this.logger.Object);
        }
    }
}
