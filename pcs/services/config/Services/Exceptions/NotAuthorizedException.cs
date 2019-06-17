// Copyright (c) Microsoft. All rights reserved.

using System;

namespace Microsoft.Azure.IoTSolutions.UIConfig.Services.Exceptions
{
    /// <summary>
    /// This exception is thrown when the user or the application
    /// is not authorized to perform the action.
    /// </summary>
    public class NotAuthorizedException : Exception
    {
        public NotAuthorizedException() : base()
        {
        }

        public NotAuthorizedException(string message) : base(message)
        {
        }

        public NotAuthorizedException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
