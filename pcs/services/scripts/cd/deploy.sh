#!/bin/bash
source .env
####### Install helm charts
install() {
#	helm install --name storageadapter storageadapter/charts/storageadapter/ --set secrets.storageadapter.PCS_STORAGEADAPTER_DOCUMENTDB_CONNSTRING=${PCS_STORAGEADAPTER_DOCUMENTDB_CONNSTRING} --set-string secrets.storageadapter.PCS_AUTH_REQUIRED=${PCS_AUTH_REQUIRED}

	helm install --name telemetry telemetry/charts/devicetelemetry/ --set secrets.devicetelemetry.PCS_TELEMETRY_DOCUMENTDB_CONNSTRING=${PCS_TELEMETRY_DOCUMENTDB_CONNSTRING} --set secrets.devicetelemetry.PCS_STORAGEADAPTER_WEBSERVICE_URL=${PCS_STORAGEADAPTER_WEBSERVICE_URL} --set secrets.devicetelemetry.PCS_STORAGEADAPTER_WEBSERVICE_URL=${PCS_STORAGEADAPTER_WEBSERVICE_URL} --set secrets.devicetelemetry.PCS_AUTH_WEBSERVICE_URL=${PCS_AUTH_WEBSERVICE_URL} --set secrets.devicetelemetry.PCS_AAD_TENANT=${PCS_AAD_TENANT} --set secrets.devicetelemetry.PCS_AAD_APPID=${PCS_AAD_APPID} --set secrets.devicetelemetry.PCS_AAD_APPSECRET=${PCS_AAD_APPSECRET} --set secrets.devicetelemetry.PCS_TELEMETRY_STORAGE_TYPE=${PCS_TELEMETRY_STORAGE_TYPE} --set secrets.devicetelemetry.PCS_TSI_FQDN=${PCS_TSI_FQDN} --set-string secrets.devicetelemetry.PCS_AUTH_REQUIRED=${PCS_AUTH_REQUIRED}

	helm install --name iothubmanager iothubmanager/charts/iothubmanager/ --set secrets.iothubmanager.PCS_IOTHUB_CONNSTRING=${PCS_IOTHUB_CONNSTRING} --set secrets.iothubmanager.PCS_AUTH_WEBSERVICE_URL=${PCS_AUTH_WEBSERVICE_URL} --set secrets.iothubmanager.PCS_STORAGEADAPTER_WEBSERVICE_URL=${PCS_STORAGEADAPTER_WEBSERVICE_URL} --set-string secrets.iothubmanager.PCS_AUTH_REQUIRED=${PCS_AUTH_REQUIRED}

	helm install --name simulation simulation/charts/simulation/ --set secrets.simulation.PCS_IOTHUB_CONNSTRING=${PCS_IOTHUB_CONNSTRING} --set secrets.simulation.PCS_STORAGEADAPTER_WEBSERVICE_URL=${PCS_STORAGEADAPTER_WEBSERVICE_URL} --set-string secrets.simulation.PCS_AUTH_REQUIRED=${PCS_AUTH_REQUIRED} 

	helm install --name config config/charts/config --set secrets.config.PCS_DEVICESIMULATION_WEBSERVICE_URL=${PCS_DEVICESIMULATION_WEBSERVICE_URL} --set secrets.config.PCS_STORAGEADAPTER_WEBSERVICE_URL=${PCS_STORAGEADAPTER_WEBSERVICE_URL} --set secrets.config.PCS_TELEMETRY_WEBSERVICE_URL=${PCS_TELEMETRY_WEBSERVICE_URL} --set secrets.config.PCS_AZUREMAPS_KEY=${PCS_AZUREMAPS_KEY} --set secrets.config.PCS_AUTH_WEBSERVICE_URL=${PCS_AUTH_WEBSERVICE_URL} --set-string secrets.config.PCS_AUTH_REQUIRED=${PCS_AUTH_REQUIRED} --debug

	helm install --name asamanager asamanager/charts/asamanager/ --set secrets.asamanager.PCS_ASA_DATA_AZUREBLOB_ACCOUNT=${PCS_ASA_DATA_AZUREBLOB_ACCOUNT} --set secrets.asamanager.PCS_ASA_DATA_AZUREBLOB_KEY=${PCS_ASA_DATA_AZUREBLOB_KEY} --set secrets.asamanager.PCS_ASA_DATA_AZUREBLOB_ENDPOINT_SUFFIX=${PCS_ASA_DATA_AZUREBLOB_ENDPOINT_SUFFIX} --set secrets.asamanager.PCS_EVENTHUB_CONNSTRING=${PCS_EVENTHUB_CONNSTRING} --set secrets.asamanager.PCS_EVENTHUB_NAME=${PCS_EVENTHUB_NAME} --set secrets.asamanager.PCS_TELEMETRY_DOCUMENTDB_CONNSTRING=${PCS_TELEMETRY_DOCUMENTDB_CONNSTRING} --set secrets.asamanager.PCS_TELEMETRY_WEBSERVICE_URL=${PCS_TELEMETRY_WEBSERVICE_URL} --set secrets.asamanager.PCS_CONFIG_WEBSERVICE_URL=${PCS_CONFIG_WEBSERVICE_URL} --set secrets.asamanager.PCS_IOTHUBMANAGER_WEBSERVICE_URL=${PCS_IOTHUBMANAGER_WEBSERVICE_URL} --set secrets.asamanager.PCS_TELEMETRY_STORAGE_TYPE=${PCS_TELEMETRY_STORAGE_TYPE} --set-string secrets.asamanager.PCS_AUTH_REQUIRED=${PCS_AUTH_REQUIRED} --debug

	helm install --name auth auth/charts/auth/ --set secrets.auth.PCS_AUTH_AUDIENCE=${PCS_AUTH_AUDIENCE} --set secrets.auth.PCS_AUTH_ISSUER=${PCS_AUTH_ISSUER}

	helm install --name webui webui/charts/webui/ --set REACT_APP_BASE_SERVICE_URL=${REACT_APP_BASE_SERVICE_URL}

	helm install --name reverse-proxy reverse-proxy/charts/reverse-proxy/
}
##########################

upgrade() {
	helm upgrade --install --force storageadapter storageadapter/ --set secrets.storageadapter.PCS_STORAGEADAPTER_DOCUMENTDB_CONNSTRING=${PCS_STORAGEADAPTER_DOCUMENTDB_CONNSTRING} --set-string secrets.storageadapter.PCS_AUTH_REQUIRED=${PCS_AUTH_REQUIRED} --debug
}


must_run_once_more(){
   #### Thisstep is required due to bug in helm https://github.com/helm/helm/issues/1479
    helm install --name storageadapter storageadapter/ --set secrets.storageadapter.PCS_STORAGEADAPTER_DOCUMENTDB_CONNSTRING=${PCS_STORAGEADAPTER_DOCUMENTDB_CONNSTRING} --set-string secrets.storageadapter.PCS_AUTH_REQUIRED=${PCS_AUTH_REQUIRED}
    exit 0
}


must_run_once_more

install
