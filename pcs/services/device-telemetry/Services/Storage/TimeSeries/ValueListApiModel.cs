// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Exceptions;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Storage.TimeSeries
{
    public class ValueListApiModel
    {
        [JsonProperty("events")]
        public List<ValueApiModel> Events { get; set; }

        public ValueListApiModel()
        {
            this.Events = new List<ValueApiModel>();
        }

        /// <summary>
        /// Converts Time Series Events to service MessageList model.
        /// Takes in a skip value to return messages starting from skip.
        /// </summary>
        public MessageList ToMessageList(int skip)
        {
            var messages = new List<Message>();
            var properties = new HashSet<string>();
            var schemas = new Dictionary<long, SchemaModel>();
            var schemaIdsInRange = new HashSet<long>();

            for (int i = 0; i < this.Events.Count; i++)
            {
                var tsiEvent = this.Events[i];

                try
                {
                    // Track each new message schema type.
                    // The first message of the new schema message type
                    // contains the TSI schema info.
                    if (!tsiEvent.SchemaRowId.HasValue)
                    {
                        schemas.Add(tsiEvent.Schema.RowId, tsiEvent.Schema);
                        tsiEvent.SchemaRowId = tsiEvent.Schema.RowId;
                    }

                    // Note: Time Series does not have a skip parameter.
                    // Must query for all values up to skip + limit and
                    // return messages starting from skip.
                    // Time Series has a query limit of 10,000 events.
                    if (i >= skip)
                    {
                        // Add message from event
                        var schema = schemas[tsiEvent.SchemaRowId.Value];

                        // Keep track of schemas needed for properties
                        schemaIdsInRange.Add(schema.RowId);
                        int messageSchemaPropIndex = schema.GetMessageSchemaIndex();

                        var message = new Message
                        {
                            DeviceId = tsiEvent.Values[schema.GetDeviceIdIndex()].ToString(),
                            MessageSchema = messageSchemaPropIndex > 0 ? tsiEvent.Values[messageSchemaPropIndex].ToString() : null,
                            Time = DateTimeOffset.Parse(tsiEvent.Timestamp),
                            Data = this.GetEventAsJson(tsiEvent.Values, schema)
                        };
                        messages.Add(message);
                    }
                }
                catch (Exception e)
                {
                    throw new TimeSeriesParseException("Failed to parse message from Time Series Insights.", e);
                }
            }

            // Add properties from schemas
            foreach (var id in schemaIdsInRange)
            {
                var schemaProperties = schemas[id].PropertiesByIndex();
                foreach (var property in schemaProperties)
                {
                    properties.Add(property.Key);
                }
            }

            return new MessageList(messages, new List<string>(properties));
        }

        /// <summary>
        /// Converts the tsi paylod for 'values' to the 'data' JObject payload for the message model.
        /// </summary>
        private JObject GetEventAsJson(List<JValue> values, SchemaModel schema)
        {
            // Get dictionary of properties and index e.g. < propertyname, index > from schema
            var propertiesByIndex = schema.PropertiesByIndex();

            var result = new JObject();

            foreach (var property in propertiesByIndex)
            {
                result.Add(property.Key, values[property.Value]);
            }

            return result;
        }
    }
}
