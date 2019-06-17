// Copyright (c) Microsoft. All rights reserved.

using System;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Exceptions
{
    /// <summary>
    /// This exception is thrown when an error occurs while
    /// parsing response from Time Series Insights.
    /// </summary>
    public class TimeSeriesParseException : Exception
    {
        public TimeSeriesParseException() : base()
        {
        }

        public TimeSeriesParseException(string message) : base(message)
        {
        }

        public TimeSeriesParseException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
