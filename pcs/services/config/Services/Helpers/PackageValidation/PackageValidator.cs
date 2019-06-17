// Copyright (c) Microsoft. All rights reserved.

using System;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.IoTSolutions.UIConfig.Services.Helpers.PackageValidation
{
    public interface IPackageValidator
    {
        JObject ParsePackageContent(string package);
        Boolean Validate();
    }

    public abstract class PackageValidator : IPackageValidator
    {
        JObject IPackageValidator.ParsePackageContent(string package)
        {
            try
            {
                return JObject.Parse(package);
            }
            catch (JsonReaderException e)
            {
                throw new InvalidInputException($"Provided package is not a valid json. Error: {e.Message}.");
            }
        }

        public abstract Boolean Validate();
    }
}
