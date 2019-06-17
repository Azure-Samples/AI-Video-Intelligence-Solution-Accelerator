// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Models;

namespace Microsoft.Azure.IoTSolutions.UIConfig.Services.Helpers.PackageValidation
{

    public static class PackageValidatorFactory
    {
        private static Dictionary<ConfigType, IPackageValidator> validatorMapping =
            new Dictionary<ConfigType, IPackageValidator>()
        {
            { ConfigType.Firmware, new FirmwareValidator() }
        };

        private static EdgePackageValidator edgePackageValidator = new EdgePackageValidator();

        // TODO:Return double checked singleton objects
        public static IPackageValidator GetValidator(PackageType packageType, string configType)
        {
            if (packageType.Equals(PackageType.EdgeManifest))
            {
                return edgePackageValidator;
            }

            Enum.TryParse<ConfigType>(configType, out ConfigType type);

            return validatorMapping.TryGetValue(type, out IPackageValidator value) ? value : null;
        }
    }
}
