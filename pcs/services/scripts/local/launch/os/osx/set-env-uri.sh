#!/bin/bash
# Copyright (c) Microsoft. All rights reserved.
launchctl setenv PCS_TELEMETRY_WEBSERVICE_URL "http://localhost:9004/v1"
launchctl setenv PCS_CONFIG_WEBSERVICE_URL "http://localhost:9005/v1"
launchctl setenv PCS_IOTHUBMANAGER_WEBSERVICE_URL "http://localhost:9002/v1"
launchctl setenv PCS_STORAGEADAPTER_WEBSERVICE_URL "http://localhost:9022/v1"
launchctl setenv PCS_AUTH_WEBSERVICE_URL "http://localhost:9001/v1"
