#!/bin/bash
# Copyright (c) Microsoft. All rights reserved.

APP_HOME="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && cd ../../../ && pwd )"

set -e

cd $APP_HOME/device-simulation/scripts/docker
./run

set +e