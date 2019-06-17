using System;
using Microsoft.Azure.Devices;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Models;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.UIConfig.WebService.v1.Helpers
{
    public class PackagesHelper
    {
        /**
         * This function is used to verify if the package type and package contents are 
         * compatible. for eg:- if package type is DeviceConfiguration it should contain
         * "devicesContent" object.
         */ 
        public static bool VerifyPackageType(string packageContent, PackageType packageType)
        {
            if (packageType == PackageType.EdgeManifest &&
                IsEdgePackage(packageContent))
            {
                return true;
            }
            else if (packageType == PackageType.DeviceConfiguration &&
                !(IsEdgePackage(packageContent)))
            {
                return true;
            }

            return false;
        }

        public static Boolean IsEdgePackage(string packageContent)
        {
            var package = JsonConvert.DeserializeObject<Configuration>(packageContent);

            if (package.Content?.ModulesContent != null &&
                package.Content?.DeviceContent == null )
            {
                return true;
            }

            return false;
        }
    }
}
