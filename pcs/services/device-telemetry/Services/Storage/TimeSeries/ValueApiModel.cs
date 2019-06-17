// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Storage.TimeSeries
{
    public class ValueApiModel
    {
        [JsonProperty("schemaRid")]
        public long ?SchemaRowId { get; set; }

        [JsonProperty("schema")]
        public SchemaModel Schema { get; set; }

        [JsonProperty("$ts")]
        public string Timestamp { get; set; }

        [JsonProperty("values")]
        public List<JValue> Values { get; set; }
    }
}
