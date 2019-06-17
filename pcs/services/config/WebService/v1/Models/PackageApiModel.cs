// Copyright (c) Microsoft. All rights reserved.

using System;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Microsoft.Azure.IoTSolutions.UIConfig.WebService.v1.Models
{
    public class PackageApiModel
    {
        [JsonProperty("Id")]
        public string Id;

        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("PackageType")]
        [JsonConverter(typeof(StringEnumConverter))]
        public PackageType packageType { get; set; }

        [JsonProperty("ConfigType")]
        public string ConfigType { get; set; }

        [JsonProperty(PropertyName = "DateCreated")]
        public string DateCreated { get; set; }

        [JsonProperty("Content")]
        public string Content { get; set; }

        public PackageApiModel(PackageServiceModel model)
        {
            this.Id = model.Id;
            this.Name = model.Name;
            this.packageType = model.PackageType;
            this.DateCreated = model.DateCreated;
            this.Content = model.Content;
            this.ConfigType = model.ConfigType;
        }

        public PackageApiModel(
                string Content,
                string Name,
                PackageType Type,
                string ConfigType)
        {
            this.Content = Content;
            this.Name = Name;
            this.packageType = Type;
            this.ConfigType = ConfigType;
        }

        public PackageServiceModel ToServiceModel()
        {
            return new PackageServiceModel()
            {
                Content = this.Content,
                Name = this.Name,
                PackageType = this.packageType,
                ConfigType = this.ConfigType
            };
        }
    }
}