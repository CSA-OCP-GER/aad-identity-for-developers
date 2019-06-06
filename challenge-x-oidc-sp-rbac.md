# Use a Service Principal to call Azure Resource Manager API

## Here is what you will learn

- create a Service Principal in AAD
- use RBAC to authorize access to an Azure subscription for the Service Principal
- acquire a token for your Service Principal
- list all resource groups in your Azure subscription using Azure Resource Manager REST API

## Description

This challenge can be done using either Powershell Az Module or Azure CLI.
If you haven't already installed Powershell Az Module or Azure CLI take a look here:
- Powershell Az Module: https://docs.microsoft.com/de-de/powershell/azure/install-az-ps?view=azps-2.0.0
- Azure CLI: https://docs.microsoft.com/de-de/cli/azure/install-azure-cli?view=azure-cli-latest

### Powershell

#### Create a Service Principal

Open PowerShell and login to Azure:

```powershell
Connect-AzAccount
```

If you want to change to another Azure subscription do the following:

```powershell
Set-AzContext -SubscriptionId <subcriptionid>
```

To list available Azure subscription do the following:

```powershell
Get-AzSubscription
```

After you have logged in and set the right context, create a Service Principal with credentials and assign the Contributor role to it for your subscription.

```powershell
# Import needed Az resource
Import-Module Az.Resources
# Create a new credential object
$credentials = New-Object Microsoft.Azure.Commands.ActiveDirectory.PSADPasswordCredential -Property @{ StartDate=Get-Date; EndDate=Get-Date -Year 2020; Password="<your password>"}
# Create the Service principal
$sp = New-AzADServicePrincipal -DisplayName <your sp name> -PasswordCredential $credentials
# Assign role
New-AzRoleAssignment -ApplicationId $sp.ApplicationId -Scope /subscriptions/<your subscription id> -RoleDefinitionName Contributor
```

#### Acquire an access token

Now a new Service Principal is created that has contributor access to your subscription. The Service Principal can be used to acquire a token that is used to call the Azure Resource Manager REST API. 
To acquire a token a call to the Azure AD token endpoint must be done using the grant type client-credentials, because we want the token endpoint to issue an access token in the name of an application.

We need the following parameters:
- TenantId (use ```powershell Get-AzContext``` to get your Azure AD tenant id)
- ApplicationId
- Password 

```powershell
$applicationId = $sp.ApplicationId
$tenantId = "<your tenant id>"
$result=Invoke-RestMethod -Uri https://login.microsoftonline.com/$tenantId/oauth2/token?api-version=1.0 -Method Post -Body @{"grant_type" = "client_credentials"; "resource" = "https://management.core.windows.net/"; "client_id" = "$applicationId"; "client_secret" = "<your password>" } | ConvertFrom-Json
```

The result contains a valid access token that can be used to call the Azure Resource Manager REST API.
```powershell
$result.access_token
```

#### List all resource groups

Now we can create a REST call to list all Azure resource groups.

```powershell
# Create the authorization header
$headers = @{'authorization'="Bearer $($result.access_token)"}
# SubscriptionId
$subscriptionId = "<your subscription id>"
# Make the request
$requestResult = Invoke-WebRequest -Uri https://management.azure.com/subscriptions/$subscriptionId/resourcegroups?api-version=2018-05-01 -Headers $headers -Method Get
# Create a PS object from JSOn stream
$requestResultObject = ConvertFrom-Json -InputObject $requestResult
# show resource groups
$requestResultObject.value
```

#### Cleanup

```powershell
Remove-AzADServicePrincipal -ApplicationId $sp.ApplicationId
```

### Azure CLI
Todo ... or coming soon ;-)