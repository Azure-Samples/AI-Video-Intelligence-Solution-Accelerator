// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Exceptions;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Models.Actions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Converters
{
    public class ActionConverter : JsonConverter
    {
        private const string ACTION_TYPE_KEY = "Type";
        private const string PARAMETERS_KEY = "Parameters";

        public override bool CanWrite => false;
        public override bool CanRead => true;

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(IAction);
        }

        public override object ReadJson(JsonReader reader,
            Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            JObject jsonObject = JObject.Load(reader);

            var actionType = Enum.Parse(
                typeof(ActionType),
                jsonObject.GetValue(ACTION_TYPE_KEY).Value<string>(),
                true);

            var parameters = jsonObject.GetValue(PARAMETERS_KEY).ToString();

            switch (actionType)
            {
                case ActionType.Email:
                    Dictionary<string, object> emailParameters =
                        JsonConvert.DeserializeObject<Dictionary<string, object>>(
                            parameters,
                            new EmailParametersConverter());
                    return new EmailAction(emailParameters);
            }

            // If could not deserialize, throw exception
            throw new InvalidInputException($"Could not deseriailize action with type {actionType}");
        }

        public override void WriteJson(JsonWriter writer,
            object value, JsonSerializer serializer)
        {
            throw new NotImplementedException("Use default implementation for writing to the field.");
        }
    }
}
