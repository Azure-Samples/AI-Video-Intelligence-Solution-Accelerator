:: Copyright (c) Microsoft. All rights reserved.

@ECHO off & setlocal enableextensions enabledelayedexpansion

:: Some settings are used to connect to an external dependency, e.g. Azure IoT Hub and IoT Hub Manager API
:: Depending on which settings and which dependencies are needed, edit the list of variables checked

IF "%PCS_TELEMETRY_DOCUMENTDB_CONNSTRING%" == "" (
    echo Error: the PCS_TELEMETRY_DOCUMENTDB_CONNSTRING environment variable is not defined.
    exit /B 1
)

IF "%PCS_STORAGEADAPTER_WEBSERVICE_URL%" == "" (
    echo Error: the PCS_STORAGEADAPTER_WEBSERVICE_URL environment variable is not defined.
    exit /B 1
)

IF "%PCS_AUTH_WEBSERVICE_URL%" == "" (
    echo Error: the PCS_AUTH_WEBSERVICE_URL environment variable is not defined.
    exit /B 1
)

IF "%PCS_AUTH_ISSUER%" == "" (
    echo Error: the PCS_AUTH_ISSUER environment variable is not defined.
    exit /B 1
)

IF "%PCS_AUTH_AUDIENCE%" == "" (
    echo Error: the PCS_AUTH_AUDIENCE environment variable is not defined.
    exit /B 1
)

IF "%PCS_TELEMETRY_STORAGE_TYPE%" == "" (
    echo Error: the PCS_TELEMETRY_STORAGE_TYPE environment variable is not defined.
    exit /B 1
)

:: The settings below are for Time Series Insights. If your deployment does not use
:: Time Series Insights they are safe to remove.

IF "%PCS_AAD_TENANT%" == "" (
    echo Error: the PCS_AAD_TENANT environment variable is not defined.
    exit /B 1
)

IF "%PCS_AAD_APPID%" == "" (
    echo Error: the PCS_AAD_APPID environment variable is not defined.
    exit /B 1
)

IF "%PCS_AAD_APPSECRET%" == "" (
    echo Error: the PCS_AAD_APPSECRET environment variable is not defined.
    exit /B 1
)

IF "%PCS_TSI_FQDN%" == "" (
    echo Error: the PCS_TSI_FQDN environment variable is not defined.
    exit /B 1
)

:: Settings for actions

IF "%PCS_ACTION_EVENTHUB_NAME%" == "" (
    echo Error: the PCS_ACTION_EVENTHUB_NAME environment variable is not defined.
    exit /B 1
)

IF "%PCS_ACTION_EVENTHUB_CONNSTRING%" == "" (
    echo Error: the PCS_ACTION_EVENTHUB_CONNSTRING environment variable is not defined.
    exit /B 1
)

IF "%PCS_LOGICAPP_ENDPOINT_URL%" == "" (
    echo Error: the PCS_LOGICAPP_ENDPOINT_URL environment variable is not defined.
    exit /B 1
)

IF "%PCS_AZUREBLOB_CONNSTRING%" == "" (
    echo Error: the PCS_AZUREBLOB_CONNSTRING environment variable is not defined.
    exit /B 1
)

IF "%PCS_SOLUTION_WEBSITE_URL%" == "" (
    echo Error: the PCS_SOLUTION_WEBSITE_URL environment variable is not defined.
    exit /B 1
)

endlocal
