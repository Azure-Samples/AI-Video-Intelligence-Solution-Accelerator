@ECHO off & setlocal enableextensions enabledelayedexpansion

IF "%PCS_STORAGEADAPTER_WEBSERVICE_URL%" == "" (
    echo Error: the PCS_STORAGEADAPTER_WEBSERVICE_URL environment variable is not defined.
    exit /B 1
)

IF "%PCS_DEVICESIMULATION_WEBSERVICE_URL%" == "" (
    echo Error: the PCS_DEVICESIMULATION_WEBSERVICE_URL environment variable is not defined.
    exit /B 1
)

IF "%PCS_TELEMETRY_WEBSERVICE_URL%" == "" (
    echo Error: the PCS_TELEMETRY_WEBSERVICE_URL environment variable is not defined.
    exit /B 1
)

IF "%PCS_AZUREMAPS_KEY%" == "" (
    echo Error: the PCS_AZUREMAPS_KEY environment variable is not defined.
    exit /B 1
)

IF "%PCS_AUTH_WEBSERVICE_URL%" == "" (
    echo Error: the PCS_AUTH_WEBSERVICE_URL environment variable is not defined.
    exit /B 1
)

:: Optional environment variables
IF "%PCS_OFFICE365_CONNECTION_URL%" == "" (
    echo Warning: the PCS_OFFICE365_CONNECTION_URL environment variable is not defined.
)

IF "%PCS_SOLUTION_NAME%" == "" (
    echo Warning: the $PCS_SOLUTION_NAME environment variable is not defined.
)

IF "%PCS_SUBSCRIPTION_ID%" == "" (
    echo Warning: the PCS_SUBSCRIPTION_ID environment variable is not defined.
)

IF "%PCS_ARM_ENDPOINT_URL%" == "" (
    echo Warning: the PCS_ARM_ENDPOINT_URL environment variable is not defined.
)

endlocal
