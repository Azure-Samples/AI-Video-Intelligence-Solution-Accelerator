// Copyright (c) Microsoft. All rights reserved.

using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Exceptions;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.External;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Http;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Runtime;
using Moq;
using Services.Test.helpers;
using Xunit;
using IUserManagementClient = Microsoft.Azure.IoTSolutions.UIConfig.Services.External.IUserManagementClient;

namespace Services.Test
{
    public class AzureResourceManagerClientTest
    {
        private const string MOCK_SUBSCRIPTION_ID = @"123456abcd";
        private const string MOCK_RESOURCE_GROUP = @"example-name";
        private const string MOCK_ARM_ENDPOINT_URL = @"https://management.azure.com";
        private const string MOCK_API_VERSION = @"2016-06-01";

        private readonly string logicAppTestConnectionUrl;

        private readonly Mock<IHttpClient> mockHttpClient;
        private readonly Mock<IUserManagementClient> mockUserManagementClient;

        private readonly AzureResourceManagerClient client;

        public AzureResourceManagerClientTest()
        {
            this.mockHttpClient = new Mock<IHttpClient>();
            this.mockUserManagementClient = new Mock<IUserManagementClient>();
            this.client = new AzureResourceManagerClient(
                this.mockHttpClient.Object,
                new ServicesConfig
                {
                    SubscriptionId = MOCK_SUBSCRIPTION_ID,
                    ResourceGroup = MOCK_RESOURCE_GROUP,
                    ArmEndpointUrl = MOCK_ARM_ENDPOINT_URL,
                    ManagementApiVersion = MOCK_API_VERSION
                },
                this.mockUserManagementClient.Object);

            this.logicAppTestConnectionUrl = $"{MOCK_ARM_ENDPOINT_URL}" +
                                        $"/subscriptions/{MOCK_SUBSCRIPTION_ID}/" +
                                        $"resourceGroups/{MOCK_RESOURCE_GROUP}/" +
                                        "providers/Microsoft.Web/connections/" +
                                        "office365-connector/extensions/proxy/testconnection?" +
                                        $"api-version={MOCK_API_VERSION}";
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async Task GetOffice365IsEnabled_ReturnsTrueIfEnabled()
        {
            // Arrange
            var response = new HttpResponse
            {
                StatusCode = HttpStatusCode.OK,
                IsSuccessStatusCode = true
            };

            this.mockHttpClient
                .Setup(x => x.GetAsync(It.IsAny<IHttpRequest>()))
                .ReturnsAsync(response);

            // Act
            var result = await this.client.IsOffice365EnabledAsync();
            
            // Assert
            this.mockHttpClient
                .Verify(x => x.GetAsync(
                    It.Is<IHttpRequest>(r => r.Check(
                    this.logicAppTestConnectionUrl))), Times.Once);

            Assert.True(result);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async Task GetOffice365IsEnabled_ReturnsFalseIfDisabled()
        {
            // Arrange
            var response = new HttpResponse
            {
                StatusCode = HttpStatusCode.NotFound,
                IsSuccessStatusCode = false
            };

            this.mockHttpClient
                .Setup(x => x.GetAsync(It.IsAny<IHttpRequest>()))
                .ReturnsAsync(response);

            // Act
            var result = await this.client.IsOffice365EnabledAsync();

            // Assert
            this.mockHttpClient
                .Verify(x => x.GetAsync(
                    It.Is<IHttpRequest>(r => r.Check(
                    this.logicAppTestConnectionUrl))), Times.Once);

            Assert.False(result);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async Task GetOffice365IsEnabled_ThrowsIfNotAuthorizd()
        {
            // Arrange
            this.mockUserManagementClient
                .Setup(x => x.GetTokenAsync())
                .ThrowsAsync(new NotAuthorizedException());

            // Act & Assert
            await Assert.ThrowsAsync<NotAuthorizedException>(async () => await this.client.IsOffice365EnabledAsync());
        }
    }
}
