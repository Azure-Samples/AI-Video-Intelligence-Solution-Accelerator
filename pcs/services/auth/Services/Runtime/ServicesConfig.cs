// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.IO;

namespace Microsoft.Azure.IoTSolutions.Auth.Services.Runtime
{
    public interface IServicesConfig
    {
        IEnumerable<string> JwtUserIdFrom { get; set; }
        IEnumerable<string> JwtNameFrom { get; set; }
        IEnumerable<string> JwtEmailFrom { get; set; }
        string JwtRolesFrom { get; set; }
        string PoliciesFolder { get; }
        string AadEndpointUrl { get; set; }
        string AadTenantId { get; set; }
        string AadApplicationId { get; set; }
        string AadApplicationSecret { get; set; }
        string ArmEndpointUrl { get; }
    }

    public class ServicesConfig : IServicesConfig
    {
        private string policiesFolder;

        public IEnumerable<string> JwtUserIdFrom { get; set; }
        public IEnumerable<string> JwtNameFrom { get; set; }
        public IEnumerable<string> JwtEmailFrom { get; set; }
        public string JwtRolesFrom { get; set; }
        public string AadEndpointUrl { get; set; }
        public string AadTenantId { get; set; }
        public string AadApplicationId { get; set; }
        public string AadApplicationSecret { get; set; }
        public string ArmEndpointUrl { get; set; }

        public ServicesConfig()
        {
            this.policiesFolder = string.Empty;
        }

        public string PoliciesFolder
        {
            get => this.policiesFolder;
            set => this.policiesFolder = this.NormalizePath(value);
        }

        private string NormalizePath(string path)
        {
            return path
                       .TrimEnd(Path.DirectorySeparatorChar)
                       .Replace(
                           Path.DirectorySeparatorChar + "." + Path.DirectorySeparatorChar,
                           Path.DirectorySeparatorChar.ToString())
                   + Path.DirectorySeparatorChar;
        }
    }
}
