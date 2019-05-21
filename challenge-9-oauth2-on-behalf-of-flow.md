# Create an API that is protected by Azure AD

## Here is what you learn
- Create a REST API that is protected by Azure AD
- Create an Azure AD application for your REST API
- Gain consent for the middle-tier application
- Integrate authorization in Swagger for your API 
- Use the OAuth2 auth code grant to acquire an access token from a client to call your REST API

## Protocol diagram
At a high level, the entire authentication flow for a native/mobile/webapp looks a bit like this:

![alt-text](images/protocols-oauth-on-behalf-of-flow.png)

To protect an API with Azure AD an Azure AD application for the API must be created.
An API does not sign in users, therefore no reply url must be created when the Azure AD application is registered.

## Sample scenario
A sample scenario is already implemnted for this challenge. The sample contains a web API that is running on ASP.NET Core which is protected by Azure AD and calls Microsoft-Graph API on behlaf of signed in user to read user's profile. The web API is accessed by an ASP.NET Core web application on behalf of the signed-in user. The ASP.NET Core Web application uses the OpenID Connect middleware and MSAL for .NET to acquire an access token for the API on behalf of the signed-in user.
The sources for the sample application is located here:
[aspnetcore-protect-api](/apps/aspnetcore-on-behalf-of-flow)

## Create the Azure AD applications

To create the Azure AD applications we use Powershell and the AzureAD module.
The API is running on port 5002 and the Web application is running on port 5003.

Connect to AzureAd Instance:

```Powershell
Connect-AzureAD
```

### Step 1: Register an Azure AD Application for the API
To expose an API application in AzureAD OAuth 2.0 permission scopes must be exposed.
When creating an Azure AD application using New-AzureADApplication Cmdlet a scope named "user_impersonation" is created automatically.
If further scopes must be created, use the following:

```Powerhsell
$exposedScopes = New-Object -TypeName Microsoft.Open.AzureAD.Model.OAuth2Permission
$exposedScopes.Type = "User"
$exposedScopes.AdminConsentDisplayName = "AccessEcho Claims API"
$exposedScopes.AdminConsentDescription = "Access Echo Claims API on behalf of signed-in users to echo claims."
$exposedScopes.Id = $(New-Guid)
$exposedScopes.IsEnabled = $true
$exposedScopes.Value = "myscope"
$exposedScopes.UserConsentDisplayName = $null
$exposedScopes.UserConsentDescription = $null
```

Create the Azure AD application for the API.

```Powershell
$api = New-AzureADApplication -DisplayName "EchoClaimsAPI" -IdentifierUris "https://echoclaimsapi"
```

Do the following when further scopes must be created.

```Powershell
$api = New-AzureADApplication -DisplayName "EchoClaimsAPI" -IdentifierUris "https://echoclaimsapi" -Oauth2Permissions $exposedScopes
```

Create a ServicePrincipal for the application

```Powershell
New-AzureADServicePrincipal -AppId $api.AppId
```


### Step 2: Register an Azure AD application for the Web application

To access the API in the web application an AzureAD application must be registered with required resource access right to access the API.

```Powershell
# required resource access (the API)
$requiredResourceAccess = New-Object -TypeName Microsoft.Open.AzureAD.Model.RequiredResourceAccess
$requiredResourceAccess.ResourceAppId = $api.AppId
$requiredResourceAccess.ResourceAccess = New-Object System.Collections.Generic.List[Microsoft.Open.AzureAD.Model.ResourceAccess]
$resourceAccess = New-Object Microsoft.Open.AzureAD.Model.ResourceAccess
$resourceAccess.Id = $api.Oauth2Permissions[0].Id
$resourceAccess.Type = "Scope"                                                            
$requiredResourceAccess.ResourceAccess.Add($resourceAccess) 
# Create the Azure AdApplication
$app = New-AzureADApplication -DisplayName EchoClaimsWebApp -IdentifierUris "https://echoclaimswebapp" -ReplyUrls "http://localhost:5003/signin-oidc" -RequiredResourceAccess $requiredResourceAccess
```

To acquire an access token for the API we use the OAuth2 code grant flow, therefore we need a client secret.

```Powershell
$secret = New-Guid
New-AzureADApplicationPasswordCredential -ObjectId $app.ObjectId -CustomKeyIdentifier "ClientSecret" -Value $secret
```

Create a ServicePrincipal for the application

```Powershell
New-AzureADServicePrincipal -AppId $app.AppId
```


### Step 3: Known Client Application

This setting is used for bundling consent if you have a solution that contains two parts: a client app and a custom web API app. If you enter the appID of the client app into this value, the user will only have to consent once to the client app. Azure AD will know that consenting to the client means implicitly consenting to the web API and will automatically provision service principals for both the client and web API at the same time. Both the client and the web API app must be registered in the same tenant.

```Powershell
$api.KnownClientApplications = New-Object System.Collections.Generic.List[System.String]
$api.KnownClientApplications.Add($app.AppId)
Set-AzureADApplication -ObjectId $api.ObjectId -KnownClientApplications $api.KnownClientApplications
```

## Run the demo application

In the demo application the ASP.NET Core web application calls the ASP.NET Core API which just returns the claims of the calling user.
The demo is located [here](/apps/aspnetcore-protect-api)
To run the demo application two shells must be opened, one for the web application and one for the API.

### Run the API
Edit the file [appsettings.json](apps/aspnetcore-protect-api/WebApi/appsettings.json) and replace Domain, TenantId, and ClientId.

In the shell navigate to the folder /apps/aspnetcore-protect-api/WebApi and run the following dotnet command

```Shell
dotnet run
```
The API is listening on port 5002.

### Run the Web application
Edit the file [appsettings.json](apps/aspnetcore-protect-api/WebApplication/appsettings.json) and replace Domain, TenantId, ClientId and ClientSecret.

In another shell navigate to the folder /apps/apsnetcore-protect-api/WebApplication and run the following dotnet command.

```shell
dotnet run
```
The web application is listening on port 5003.