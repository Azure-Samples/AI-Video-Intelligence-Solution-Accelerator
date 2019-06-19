@ECHO off & setlocal enableextensions enabledelayedexpansion

:: Usage:
:: scripts\docker\run         : Starts the stable version
:: scripts\docker\run testing : Starts the testing version

:: Note: use lowercase names for the Docker images
SET DOCKER_IMAGE=azureaivideo/pcs-remote-monitoring-webui
SET STABLE_VERSION=3.0.0

IF "%1"=="" goto :STABLE
IF "%1"=="testing" goto :TESTING

:STABLE
  echo Starting Remote Monitoring Web UI [%STABLE_VERSION%] ...
  docker run -it -p 10080:10080 -p 10443:10443 %DOCKER_IMAGE%:%STABLE_VERSION%
  goto :END

:TESTING
  echo Starting Remote Monitoring Web UI [testing version] ...
  docker run -it -p 10080:10080 -p 10443:10443 %DOCKER_IMAGE%:testing
  goto :END


:END

endlocal
