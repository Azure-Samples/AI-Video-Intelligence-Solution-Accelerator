﻿// Copyright (c) Microsoft. All rights reserved.

using System;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Models
{
    public class Message
    {
        public string DeviceId { get; set; }
        public string MessageSchema { get; set; }
        public DateTimeOffset Time { get; set; }
        public JObject Data { get; set; }

        public Message()
        {
            this.DeviceId = string.Empty;
            this.Time = DateTimeOffset.UtcNow;
            this.Data = null;
        }

        public Message(
            string deviceId,
            string messageSchema,
            long time,
            JObject data)
        {
            this.DeviceId = deviceId;
            this.MessageSchema = messageSchema;
            this.Time = DateTimeOffset.FromUnixTimeMilliseconds(time);
            this.Data = data;
        }
    }
}
