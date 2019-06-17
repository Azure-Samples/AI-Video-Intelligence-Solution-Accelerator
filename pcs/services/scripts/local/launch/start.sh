#!/bin/bash
# Copyright (c) Microsoft. All rights reserved.
# This script will install pcs-cli on this machine.
# For more info on pcs-cli visit (https://github.com/Azure/pcs-cli).
APP_HOME="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && cd ../../../ && pwd )" 2> /dev/null
read -p "Have you created required Azure resources (Y/N)?" yn
case $yn in
"Y"|"y"|"Yes"|"yes") 
	echo -e "Please set the env variables on your machine. You need not run this script again.";  
	exit 0
;;
"N"|"n"|"No"|"no") 
	echo "Setting up Azure resources."
	$APP_HOME/scripts/local/launch/helpers/create-azure-resources.sh;
;;
*)
	echo "Incorrect option. Please re-run the script."
	exit 0
;;
esac
