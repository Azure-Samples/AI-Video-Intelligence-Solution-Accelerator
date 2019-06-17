
> This folder references the *scripts* required for starting simulation micro-service in docker. The code of this micro-service resides [here](https://github.com/Azure/device-simulation-dotnet). 

How to use the microservice
===========================

## Quickstart - Running the service with Docker

1. Create an instance of [Azure IoT Hub][iothub-url]
1. Follow the [Storage quickstart instructions][storageadapter-url]
   for setting up the storage used by Storage Adapter microservice.
1. Find your Iot Hub connection string. See
   [Understanding IoTHub Connection Strings][iothubconnstring-url] if you
   need help finding it.
1. Store the "IoT Hub Connection string" in the [env-vars-setup](scripts)
   script, then run the script. When using MacOS/Linux, the environment
   variables need to be set in the same terminal session where Docker is
   executed, every time a new session is created.
1. [Install Docker Compose][docker-compose-install-url]
1. Start the Simulation service using docker compose:
   ```
   cd scripts
   cd docker
   docker-compose up
   ```
1. Use an HTTP client such as [Postman][postman-url], to exercise the
   [RESTful API][wiki-createsim-url] to create a simulation.


## Configuration and Environment variables

The service configuration is accessed via ASP.NET Core configuration
adapters, and stored in [appsettings.ini](https://github.com/Azure/device-simulation-dotnet/blob/master/WebService/appsettings.ini).
The INI format allows to store values in a readable format, with comments.

The configuration also supports references to environment variables, e.g. to
import credentials and network details. Environment variables are not
mandatory though, you can for example edit appsettings.ini and write
credentials directly in the file. Just be careful not sharing the changes,
e.g. sending a Pull Request or checking in the changes in git.

The configuration file in the repository references some environment
variables that need to be defined. Depending on the OS and the IDE used,
there are several ways to manage environment variables.

1. When running the service **with Docker** or **from the command line**, the
   application will inherit environment variables values from the system. 
   * [This page][windows-envvars-howto-url] describes how to setup env vars
     in Windows. We suggest to edit and execute once the
     [env-vars-setup.cmd](scripts/env-vars-setup.cmd) script included in the
     repository. The settings will persist across terminal sessions and reboots.
   * For Linux and MacOS, we suggest to edit and execute
     [env-vars-setup](scripts/env-vars-setup) each time, before starting the
     service. Depending on OS and terminal, there are ways to persist values
     globally, for more information these pages should help:
     * https://stackoverflow.com/questions/13046624/how-to-permanently-export-a-variable-in-linux
     * https://stackoverflow.com/questions/135688/setting-environment-variables-in-os-x
     * https://help.ubuntu.com/community/EnvironmentVariables

[iothub-url]: https://azure.microsoft.com/services/iot-hub
[storageadapter-url]: https://github.com/Azure/remote-monitoring-services-dotnet/tree/master/storage-adapter
[iothubconnstring-url]: https://blogs.msdn.microsoft.com/iotdev/2017/05/09/understand-different-connection-strings-in-azure-iot-hub
[docker-compose-install-url]: https://docs.docker.com/compose/install
[windows-envvars-howto-url]: https://superuser.com/questions/949560/how-do-i-set-system-environment-variables-in-windows-10
[postman-url]: https://www.getpostman.com
[wiki-createsim-url]: https://github.com/Azure/device-simulation-dotnet/wiki/%5BAPI-Specifications%5D-Simulations#create-default-simulation
