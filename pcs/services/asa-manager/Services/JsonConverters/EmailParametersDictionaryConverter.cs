// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.IoTSolutions.AsaManager.Services.JsonConverters
{
    public class EmailParametersDictionaryConverter : JsonConverter
    {
        public override bool CanWrite => false;
        public override bool CanRead => true;

        private const string RECIPIENTS_KEY = "Recipients";

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Dictionary<string, object>);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jsonObject = JObject.Load(reader);

            // Convert to a case-insensitive dictionary for case insensitive look up.
            Dictionary<string, object> returnDictionary =
                new Dictionary<string, object>(jsonObject.ToObject<Dictionary<string, object>>(), StringComparer.OrdinalIgnoreCase);

            if (returnDictionary.ContainsKey(RECIPIENTS_KEY) && returnDictionary[RECIPIENTS_KEY] != null)
            {
                returnDictionary[RECIPIENTS_KEY] = ((JArray)returnDictionary[RECIPIENTS_KEY]).ToObject<List<string>>();
            }

            return returnDictionary;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException("Use default implementation for writing to the field.");
        }
    }
}
