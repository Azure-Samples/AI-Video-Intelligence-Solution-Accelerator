@ECHO off & setlocal enableextensions enabledelayedexpansion

IF "%PCS_AUTH_ISSUER%" == "" (
    echo Error: the PCS_AUTH_ISSUER environment variable is not defined.
    exit /B 1
)

IF "%PCS_AUTH_AUDIENCE%" == "" (
    echo Error: the PCS_AUTH_AUDIENCE environment variable is not defined.
    exit /B 1
)

IF "%PCS_AAD_ENDPOINT_URL%" == "" (
    echo Error: the PCS_AAD_ENDPOINT_URL environment variable is not defined.
    exit /B 1
)

IF "%PCS_AAD_TENANT%" == "" (
    echo Error: the PCS_AAD_TENANT environment variable is not defined.
    exit /B 1
)

IF "%PCS_AAD_APPSECRET%" == "" (
    echo Error: the PCS_AAD_APPSECRET environment variable is not defined.
    exit /B 1
)

IF "%PCS_ARM_ENDPOINT_URL%" == "" (
    echo Error: the PCS_ARM_ENDPOINT_URL environment variable is not defined.
    exit /B 1
)

endlocal
