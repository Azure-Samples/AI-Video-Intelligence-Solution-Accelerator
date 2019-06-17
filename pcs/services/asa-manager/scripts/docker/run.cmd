:: Copyright (c) Microsoft. All rights reserved.

@ECHO off & setlocal enableextensions enabledelayedexpansion

:: Note: use lowercase names for the Docker images
SET DOCKER_IMAGE="azureiotpcs/asa-manager-dotnet"

:: strlen("\scripts\docker\") => 16
SET APP_HOME=%~dp0
SET APP_HOME=%APP_HOME:~0,-16%
cd %APP_HOME%

:: Check dependencies
docker version > NUL 2>&1
IF %ERRORLEVEL% NEQ 0 GOTO MISSING_DOCKER

:: Check settings
call .\scripts\env-vars-check.cmd
IF %ERRORLEVEL% NEQ 0 GOTO FAIL

:: Start the application
echo Starting ASA manager ...
docker run -it -p 9024:9024 ^
    -e PCS_TELEMETRY_WEBSERVICE_URL ^
    -e PCS_CONFIG_WEBSERVICE_URL ^
    -e PCS_IOTHUBMANAGER_WEBSERVICE_URL ^
    -e PCS_ASA_DATA_AZUREBLOB_ACCOUNT ^
    -e PCS_ASA_DATA_AZUREBLOB_KEY ^
    -e PCS_ASA_DATA_AZUREBLOB_ENDPOINT_SUFFIX ^
    -e PCS_EVENTHUB_CONNSTRING ^
    -e PCS_EVENTHUB_NAME ^
    -e PCS_AUTH_REQUIRED ^
    -e PCS_CORS_WHITELIST ^
    -e PCS_AUTH_ISSUER ^
    -e PCS_AUTH_AUDIENCE ^
    -e PCS_TWIN_READ_WRITE_ENABLED ^
    -e PCS_TELEMETRY_DOCUMENTDB_CONNSTRING ^
    -e PCS_TELEMETRY_STORAGE_TYPE ^
    %DOCKER_IMAGE%:testing

:: - - - - - - - - - - - - - -
goto :END

:FAIL
    echo Command failed
    endlocal
    exit /B 1

:MISSING_DOCKER
    echo ERROR: 'docker' command not found.
    echo Install Docker and make sure the 'docker' command is in the PATH.
    echo Docker installation: https://www.docker.com/community-edition#/download
    exit /B 1

:END
endlocal
