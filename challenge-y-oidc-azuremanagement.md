# Create an ASP.NET Core WebApplication that accesses the Azure Resource Manager API using a Service Principal. 

## Here is what you will learn

- How to register an Azure AD application
- How to gain consent to access Azure Resource Manager API on-behalf-of signed-in user
- How to gain consent to access Microsoft Graph to create a Service Principal
- How to assign `Owner` role to an Azure subscription for a Service Principal
- How to list all Azure subscriptions using Azure Resource Manager API

In this challenge we use an ASP.NET Core WebApplication that is already implemented.

## Create an Azure AD application

Before you can authenticate an user you have to register an application in your AAD tenant.
You can either use the PowerShell Module Az or Azure CLI.

### PowerShell

To create an Azure AD application we use the PowerShell Cmdlet AzureAD.

```PowerShell
Import-Module AzureAD
Connect-AzureAd
$app = New-AzureADApplication -DisplayName challengeaspnetarm -IdentifierUris https://challengeaspnetarm -ReplyUrls http://localhost:5003/signin-oidc
```

Retrieve and note the ID of your current AAD tenant via:

```powershell
Get-AzContext
```

### Azure CLI

```shell
az ad app create --display-name challengeaspnetarm --reply-urls http://localhost:5003/signin-oidc --identifier-uris https://challengeaspnetarm
```

Retrieve and note the ID of your current AAD tenant via:

```shell
az account show 
```

// TODO...
- create app ...