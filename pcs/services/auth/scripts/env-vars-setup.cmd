:: Prepare the environment variables used by the application.

:: The OpenId tokens issuer URL, e.g. https://sts.windows.net/12000000-3400-5600-0000-780000000000/
SETX PCS_AUTH_ISSUER "{enter the token issuer URL here}"

:: The intended audience of the tokens, e.g. your Client Id
SETX PCS_AUTH_AUDIENCE "{enter the tokens audience here}"

# Azure Active Directory endpoint url, e.g. https://login.microsoftonline.com/
SETX PCS_AAD_ENDPOINT_URL "{enter the AAD endpoint URL here}"

# The tenant id of Azure Active Directory
SETX PCS_AAD_TENANT "{enter the tenant id of AAD here}"

# The secret of intended application audience
SETX PCS_AAD_APPSECRET "{enter the secret of AAD application here}"

# Azure Resource Manager endpoint url, e.g. https://management.azure.com/
SETX PCS_ARM_ENDPOINT_URL "{enter the endpoint URL of Azure Resource Manager here}"
