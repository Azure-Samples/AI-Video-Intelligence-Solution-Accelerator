// Copyright (c) Microsoft. All rights reserved.

using System;

namespace Microsoft.Azure.IoTSolutions.IotHubManager.Services.Exceptions
{
    /// <summary>
    /// This exception is thrown when a client is requesting a resource that
    /// was created outside of remote monitoring specifically by id.
    /// </summary>
    public class ResourceNotSupportedException : Exception
    {
        public ResourceNotSupportedException()
        {
        }

        public ResourceNotSupportedException(string message) : base(message)
        {
        }

        public ResourceNotSupportedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
