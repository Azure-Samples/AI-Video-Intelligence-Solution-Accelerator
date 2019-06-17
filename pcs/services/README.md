Remote Monitoring Microservices for AI Video Intelligence Solution Accelerator
========
This section contains a .NET-only version of the 
[Azure Remote Monitoring Microservices](https://github.com/Azure/remote-monitoring-services-dotnet/tree/a864a3ce0fcb3d378635b9f5d1ef90822e3a383f) 
that has had the [Device Telemetry Microservice](device-telemetry/README.md) modified to add two features
needed to support the 
[AI Video Intelligence Solution Accelerator](https://github.com/Azure-Samples/AI-Video-Intelligence-Solution-Accelerator):
* Addition of a `GetSasUrlForBlobAccess` call. This call provides a way for the Web UI to retrieve a
SAS-tokenized URL to allow secure access to BLOB data.
* The `MessageSchema` property is now propagated through for all telemetry messages. 
This is the value set as the message schema in IoT Hub client components.

The code in this section is based on commit `37cf899` of the
[Azure Remote Monitoring Microservices](https://github.com/Azure/remote-monitoring-services-dotnet/tree/a864a3ce0fcb3d378635b9f5d1ef90822e3a383f).