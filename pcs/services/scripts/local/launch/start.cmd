:: Copyright (c) Microsoft. All rights reserved.
:: This script will install pcs-cli on this machine.
:: For more info on pcs-cli visit ().

@ECHO OFF
echo  Have you created required Azure resources ([Y]es/[N]o)?
set /p answer=Enter selection:
if "%answer%" == "Y" (
    echo Please set the env variables on your machine. You need not run this script again. 
) else if "%answer%" == "N" (
    echo Setting up Azure resources... 
    call helpers\"create-azure-resources"
) else (
    echo "Incorrect option. Please re-run the script."
)