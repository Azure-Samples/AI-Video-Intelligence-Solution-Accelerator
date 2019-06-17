// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Storage.TimeSeries
{
    public class SchemaModel
    {
        private const string DEVICE_ID_KEY = "iothub-connection-device-id";
        private const string MESSAGE_SCHEMA_KEY = "iothub-message-schema";
        private readonly HashSet<string> excludeProperties;

        public SchemaModel()
        {
            // List of properties from Time Series that should be
            // excluded in conversion to message model
            this.excludeProperties = new HashSet<string>
            {
                "$$ContentType",
                "$$CreationTimeUtc",
                "$$MessageSchema",
                "content-encoding",
                "content-type",
                "iothub-connection-auth-generation-id",
                "iothub-connection-auth-method",
                "iothub-connection-device-id",
                "iothub-creation-time-utc",
                "iothub-enqueuedtime",
                "iothub-message-schema",
                "iothub-message-source"
            };
        }

        [JsonProperty("rid")]
        public long RowId { get; set; }

        [JsonProperty("$esn")]
        public string EventSourceName { get; set; }

        [JsonProperty("properties")]
        public List<PropertyModel> Properties { get; set; }

        /// <summary>
        /// Returns the properties needed to convert to message model
        /// with lookup by index, excludes iothub properties.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, int> PropertiesByIndex()
        {
            var result = new Dictionary<string, int> ();

            for (int i = 0; i < this.Properties.Count; i++)
            {
                var property = this.Properties[i];

                if (!this.excludeProperties.Contains(property.Name))
                {
                    result.Add(property.Name, i);
                }
            }

            return result;
        }

        public int GetDeviceIdIndex()
        {
            for (int i = 0; i < this.Properties.Count; i++)
            {
                if (this.Properties[i].Name.Equals(DEVICE_ID_KEY, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }

            throw new TimeSeriesParseException("No device id found in message schema from Time Series Insights. " +
                                            $"Device id property '{DEVICE_ID_KEY}' is missing.");
        }

        public int GetMessageSchemaIndex()
        {
            for (int i = 0; i < this.Properties.Count; i++)
            {
                if (this.Properties[i].Name.Equals(MESSAGE_SCHEMA_KEY, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
