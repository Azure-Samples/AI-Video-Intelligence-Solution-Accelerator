// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Exceptions;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Models;
using Microsoft.Azure.IoTSolutions.IotHubManager.WebService.v1.Controllers;
using Microsoft.Azure.IoTSolutions.IotHubManager.WebService.v1.Models;
using Moq;
using WebService.Test.helpers;
using Xunit;

namespace WebService.Test.v1.Controllers
{
    public class DeploymentsControllerTest
    {
        private readonly DeploymentsController deploymentsController;
        private readonly Mock<IDeployments> deploymentsMock;
        private const string DEPLOYMENT_NAME = "depname";
        private const string DEVICE_GROUP_ID = "dvcGroupId";
        private const string DEVICE_GROUP_NAME = "dvcGroupName";
        private const string DEVICE_GROUP_QUERY = "dvcGroupQuery";
        private const string PACKAGE_CONTENT = "{}";
        private const string PACKAGE_NAME = "packageName";
        private const string DEPLOYMENT_ID = "dvcGroupId-packageId";
        private const int PRIORITY = 10;
        private const string CONFIG_TYPE = "Edge";

        public DeploymentsControllerTest()
        {
            this.deploymentsMock = new Mock<IDeployments>();
            this.deploymentsController = new DeploymentsController(this.deploymentsMock.Object);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async Task GetDeploymentTest()
        {
            // Arrange
            this.deploymentsMock.Setup(x => x.GetAsync(DEPLOYMENT_ID, false)).ReturnsAsync(new DeploymentServiceModel()
            {
                Name = DEPLOYMENT_NAME,
                DeviceGroupId = DEVICE_GROUP_ID,
                DeviceGroupName = DEVICE_GROUP_NAME,
                DeviceGroupQuery = DEVICE_GROUP_QUERY,
                PackageContent = PACKAGE_CONTENT,
                PackageName = PACKAGE_NAME,
                Priority = PRIORITY,
                Id = DEPLOYMENT_ID,
                PackageType = PackageType.EdgeManifest,
                ConfigType = CONFIG_TYPE,
                CreatedDateTimeUtc = DateTime.UtcNow
            });

            // Act
            var result = await this.deploymentsController.GetAsync(DEPLOYMENT_ID);

            // Assert
            Assert.Equal(DEPLOYMENT_ID, result.DeploymentId);
            Assert.Equal(DEPLOYMENT_NAME, result.Name);
            Assert.Equal(PACKAGE_CONTENT, result.PackageContent);
            Assert.Equal(PACKAGE_NAME, result.PackageName);
            Assert.Equal(DEVICE_GROUP_ID, result.DeviceGroupId);
            Assert.Equal(DEVICE_GROUP_NAME, result.DeviceGroupName);
            Assert.Equal(PRIORITY, result.Priority);
            Assert.Equal(PackageType.EdgeManifest, result.PackageType);
            Assert.Equal(CONFIG_TYPE, result.ConfigType);
            Assert.True((DateTimeOffset.UtcNow - result.CreatedDateTimeUtc).TotalSeconds < 5);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async Task VerifyGroupAndPackageNameLabelsTest()
        {
            // Arrange
            this.deploymentsMock.Setup(x => x.GetAsync(DEPLOYMENT_ID, false)).ReturnsAsync(new DeploymentServiceModel()
            {
                Name = DEPLOYMENT_NAME,
                DeviceGroupId = DEVICE_GROUP_ID,
                DeviceGroupName = DEVICE_GROUP_NAME,
                DeviceGroupQuery = DEVICE_GROUP_QUERY,
                PackageContent = PACKAGE_CONTENT,
                Priority = PRIORITY,
                Id = DEPLOYMENT_ID,
                PackageType = PackageType.EdgeManifest,
                ConfigType = CONFIG_TYPE,
                CreatedDateTimeUtc = DateTime.UtcNow
            });

            // Act
            var result = await this.deploymentsController.GetAsync(DEPLOYMENT_ID);

            // Assert
            Assert.Equal(DEPLOYMENT_ID, result.DeploymentId);
            Assert.Equal(DEPLOYMENT_NAME, result.Name);
            Assert.Equal(PACKAGE_CONTENT, result.PackageContent);
            Assert.Equal(DEVICE_GROUP_ID, result.DeviceGroupId);
            Assert.Equal(PRIORITY, result.Priority);
            Assert.Equal(PackageType.EdgeManifest, result.PackageType);
            Assert.Equal(CONFIG_TYPE, result.ConfigType);
            Assert.True((DateTimeOffset.UtcNow - result.CreatedDateTimeUtc).TotalSeconds < 5);
        }

        [Theory, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(5)]
        public async Task GetDeploymentsTest(int numDeployments)
        {
            // Arrange
            var deploymentsList = new List<DeploymentServiceModel>();
            var deploymentMetrics = new DeploymentMetricsServiceModel(null, null)
            {
                DeviceMetrics = new Dictionary<DeploymentStatus, long>()
                {
                    { DeploymentStatus.Succeeded, 0},
                    { DeploymentStatus.Pending, 0},
                    { DeploymentStatus.Failed, 0}
                }
            };

            for (var i = 0; i < numDeployments; i++)
            {
                deploymentsList.Add(new DeploymentServiceModel()
                {
                    Name = DEPLOYMENT_NAME + i,
                    DeviceGroupId = DEVICE_GROUP_ID + i,
                    DeviceGroupQuery = DEVICE_GROUP_QUERY + i,
                    PackageContent = PACKAGE_CONTENT + i,
                    Priority = PRIORITY + i,
                    Id = DEPLOYMENT_ID + i,
                    PackageType = PackageType.EdgeManifest,
                    ConfigType = CONFIG_TYPE,
                    CreatedDateTimeUtc = DateTime.UtcNow,
                    DeploymentMetrics = deploymentMetrics
                });
            }

            this.deploymentsMock.Setup(x => x.ListAsync()).ReturnsAsync(
                new DeploymentServiceListModel(deploymentsList)
            );

            // Act
            var results = await this.deploymentsController.GetAsync();

            // Assert
            Assert.Equal(numDeployments, results.Items.Count);
            for (var i = 0; i < numDeployments; i++)
            {
                var result = results.Items[i];
                Assert.Equal(DEPLOYMENT_ID + i, result.DeploymentId);
                Assert.Equal(DEPLOYMENT_NAME + i, result.Name);
                Assert.Equal(DEVICE_GROUP_QUERY + i, result.DeviceGroupQuery);
                Assert.Equal(DEVICE_GROUP_ID + i, result.DeviceGroupId);
                Assert.Equal(PACKAGE_CONTENT + i, result.PackageContent);
                Assert.Equal(PRIORITY + i, result.Priority);
                Assert.Equal(PackageType.EdgeManifest, result.PackageType);
                Assert.Equal(CONFIG_TYPE, result.ConfigType);
                Assert.True((DateTimeOffset.UtcNow - result.CreatedDateTimeUtc).TotalSeconds < 5);
                Assert.Equal(5, result.Metrics.SystemMetrics.Count());
            }
        }

        [Theory, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        [InlineData("depName", "dvcGroupId", "dvcQuery", "pkgContent", 10, false)]
        [InlineData("", "dvcGroupId", "dvcQuery", "pkgContent", 10, true)]
        [InlineData("depName", "", "dvcQuery", "pkgContent", 10, true)]
        [InlineData("depName", "dvcGroupId", "", "pkgContent", 10, true)]
        [InlineData("depName", "dvcGroupId", "dvcQuery", "", 10, true)]
        [InlineData("depName", "dvcGroupId", "dvcQuery", "pkgContent", -1, true)]
        public async Task PostDeploymentTest(string name, string deviceGroupId,
                                             string deviceGroupQuery, string packageContent,
                                             int priority, bool throwsException)
        {
            // Arrange
            var deploymentId = "test-deployment";
            const string deviceGroupName = "DeviceGroup";
            this.deploymentsMock.Setup(x => x.CreateAsync(Match.Create<DeploymentServiceModel>(model =>
                    model.DeviceGroupId == deviceGroupId &&
                    model.PackageContent == packageContent &&
                    model.Priority == priority &&
                    model.DeviceGroupName == deviceGroupName &&
                    model.Name == name &&
                    model.PackageType == PackageType.EdgeManifest &&
                    model.ConfigType == CONFIG_TYPE)))
                .ReturnsAsync(new DeploymentServiceModel()
                {
                    Name = name,
                    DeviceGroupId = deviceGroupId,
                    DeviceGroupName = deviceGroupName,
                    DeviceGroupQuery = deviceGroupQuery,
                    PackageContent = packageContent,
                    Priority = priority,
                    Id = deploymentId,
                    PackageType = PackageType.EdgeManifest,
                    ConfigType = CONFIG_TYPE,
                    CreatedDateTimeUtc = DateTime.UtcNow
                });

            var depApiModel = new DeploymentApiModel()
            {
                Name = name,
                DeviceGroupId = deviceGroupId,
                DeviceGroupQuery = deviceGroupQuery,
                DeviceGroupName = deviceGroupName,
                PackageContent = packageContent,
                PackageType = PackageType.EdgeManifest,
                ConfigType = CONFIG_TYPE,
                Priority = priority
            };

            // Act
            if (throwsException)
            {
                await Assert.ThrowsAsync<InvalidInputException>(async () => await this.deploymentsController.PostAsync(depApiModel));
            }
            else
            {
                var result = await this.deploymentsController.PostAsync(depApiModel);

                // Assert
                Assert.Equal(deploymentId, result.DeploymentId);
                Assert.Equal(name, result.Name);
                Assert.Equal(deviceGroupId, result.DeviceGroupId);
                Assert.Equal(deviceGroupQuery, result.DeviceGroupQuery);
                Assert.Equal(packageContent, result.PackageContent);
                Assert.Equal(priority, result.Priority);
                Assert.Equal(PackageType.EdgeManifest, result.PackageType);
                Assert.Equal(CONFIG_TYPE, result.ConfigType);
                Assert.True((DateTimeOffset.UtcNow - result.CreatedDateTimeUtc).TotalSeconds < 5);
            }
        }

        [Theory, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        [InlineData("depName", "dvcGroupId", "dvcQuery", "pkgContent", -1)]
        public async Task PostInvalidDeploymentTest(string name, string deviceGroupId,
                                             string deviceGroupQuery, string packageContent,
                                             int priority)
        {
            // Arrange
            var depApiModel = new DeploymentApiModel()
            {
                Name = name,
                DeviceGroupId = deviceGroupId,
                DeviceGroupQuery = deviceGroupQuery,
                PackageContent = packageContent,
                PackageType = PackageType.DeviceConfiguration,
                ConfigType = string.Empty,
                Priority = priority
            };

            // Act
            await Assert.ThrowsAsync<InvalidInputException>(async () => await this.deploymentsController.PostAsync(depApiModel));    
        }
    }
}
