# Create an ASP.NET Core WebApplication that accesses the Azure Resource Manager API using a Service Principal. 

## Here is what you will learn

- How to register an Azure AD application
- How to gain consent to access Azure Resource Manager API on-behalf-of signed-in user
- How to gain consent to access Microsoft Graph to create a Service Principal
- How to assign `Contributor` role to an Azure subscription for a Service Principal
- How to list all Azure subscriptions using Azure Resource Manager API
- How to use [Microsoft Graph .NET Client library](https://github.com/microsoftgraph/msgraph-sdk-dotnet). (Currently using Microsoft.Graph.Beta)

In this challenge we use an ASP.NET Core WebApplication that is already implemented.
This challenge shows you how you can create an application that creates a Service Principal using Microsoft Graph.
Sometimes your application needs access to Azure resources in a background service or daemon. A backround service or daemon does not have an active user signed-in, therefore we can not acquire an access token on behalf of a user. A Service Principal is an Azure Active Directory Security Principal that can be used to acquire an access token on behalf of the Service Principal. 
In Azure AD an application has to be created, before a Service Principal can be created for that application.

## `Note`

```
An Azure AD application is defined by its one and only application object, which resides in the Azure AD tenant where the application was registered, known as the application's "home" tenant. 
To access resources that are secured by an Azure AD tenant, the entity that requires access must be represented by a security principal. This is true for both users (user principal) and applications (service principal).

```

To grant access to Azure resources for a Service Principal role-based access control (RBAC) for Azure resources must be used. You can find more details about RBAC for Azure [here](https://docs.microsoft.com/en-us/azure/role-based-access-control/overview). In this example we assign the role `Contributor` to the Service Principal at the subscription scope. 

To do all the stuff a user that has adminsitrative rights in the Azure AD Tenant must sign in to grant consent for the needed permissions. The creation of the Service Principal and the role assignment at the subscription scope is done on behalf-of the signed-in user. After the Service Principal is created and the `Contributor` role is assigned, an access token can be acquired using the Service Principal to call the Azure Resource Manager API. In a real world scenario the `ClientId` and `ClientSecret` of the Service Principal should be stored in an Azure key vault to access it for acquireing tokens during the lifetime of the application.  

Consider the following example in PowerShell. Here a Service Principal is created and the role `Contributor` is assigned at the subscription scope. That's exactly what the sample application does.

```PowerShell
Import-Module AzureAD
Connect-AzureAD
# Create the AzureAD application
$app = New-AzureADApplication -DisplayName dempsprabac
# We use a simple guid for the password
$secret = New-Guid
# create the client secret
New-AzureADApplicationPasswordCredential -CustomKeyIdentifier ClientSecret -Value $secret -ObjectId $app.ObjectId
# Now, create the Service Principal
$sp = New-AzureADServicePrincipal -AppId $app.AppId
# Assign the `Contributor` role at subscription scope
New-AzRoleAssignment -ApplicationId $sp.AppId -Scope "/subscriptions/00000000-0000-0000-0000-000000000000" -RoleDefinitionName "Contributor"
```

Now an access token can be acquired to call the Azure Resource Manager API.

```PowerShell
$applicationId = $sp.ApplicationId
$tenantId = "2a151364-d43b-4192-b727-ab106e85ccdd"
$result=Invoke-RestMethod -Uri https://login.microsoftonline.com/$tenantId/oauth2/v2.0/token?api-version=1.0 -Method Post -Body @{"grant_type" = "client_credentials"; "scope" = "https://management.azure.com/.default"; "client_id" = "$applicationId"; "client_secret" = "$secret" } | ConvertFrom-Json
```

## Create an Azure AD application

Before you can authenticate an user you have to register an application in your AAD tenant.
You can either use the PowerShell Module Az or Azure CLI.

### PowerShell

To create an Azure AD application we use the PowerShell Cmdlet AzureAD.

```PowerShell
Import-Module AzureAD
Connect-AzureAd
$app = New-AzureADApplication -DisplayName challengeaspnetarm -IdentifierUris https://challengeaspnetarm -ReplyUrls http://localhost:5003/signin-oidc
$secret = "<your secret>"
New-AzureADApplicationPasswordCredential -ObjectId $app.ObjectId -Value $secret
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
## Configure the application

Open the `appsettings.json` in [`apps/aspnetcore-oidc-azuremanagement`](apps/aspnetcore-oidc-azuremanagement) and replace values:
* `Domain` -> your domain e.g., `mycompany.onmicrosoft.com`
* `TenantId` -> your Azure AD Directory Id (also known as Tenant Id)
* `ClientId` -> `ApplicationId` of the newly created application
* `ClientSecret` -> `Secret` the secret you specified

## Run and inspect the application

Before you can run the application 

The demo application for this challenge can be found in this repository. Open a shell and run the application from [apps/aspnetcore-oidc-azuremanagement] apps/aspnetcore-oidc-azuremanagement.

```shell
dotnet run
```

Open a browser and navigate to http://localhost:5003. When you sign in you have to grant consent for the default permissions.

### Create Service Principal
Under the menu `Service Principal` you can create a new Service Principal by clicking the button `Create`. The controller action `Create` from the controller `ServicePrincipalController` is toggled. In this action an access token for accessing the APIs `Azure Management Service` and `Microsoft Graph` on behalf of the signed-in users is requested. As the current signed-in user didn't garnt consent to the required permissions an exception of type `MsalUiRequiredException` is thrown. In the catch-block an autentication challenge is initialized that redirects the user to the Azure AD Tenant to gain consent for the required permissions `Azure Management Service` and `Microsoft Graph - Directory.AccessAsUser.All`. With the permission `AccessAsUser.All` the signed-in user can access the Azure AD of the tenant with his Azure AD roles. The signed-in user must have at least the role `Application Developer` to create a Service Principal.
When permissions are granted, a new view is displayed that shows you all available Azure subscriptions for the signe-in user. Please select a subscription for which you want to create a Service Principal and a `Contrinutor` role assignment and click the button `Create`.
After the Service Principal is created, al list of available Azure resource groups is displayed.  