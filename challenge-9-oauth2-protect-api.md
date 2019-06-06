# Create an API that is protected by Azure AD

## Here is what you'll learn
- Create a REST API that is protected by Azure AD
- Create an Azure AD application for your REST API
- Integrate authorization in Swagger for your API 
- Use the OAuth2 auth code grant to acquire an access token from a client to call your REST API
- Dynamically grant consent to read the signed-in user's profile

## Protocol diagram
At a high level, the entire authentication flow for a native/mobile/webapp looks a bit like this:

![alt-text](images/code-grant-flow.png)

To protect an API with Azure AD an Azure AD application for the API must be created.
An API does not sign in users, therefore no reply url must be created when the Azure AD application is registered.

## Sample scenario

A sample scenario is already implemented for this challenge. The sample contains a web API that is running on ASP.NET Core which is protected by Azure AD. The web API is accessed by an ASP.NET Core web application on behalf of the signed-in user. The ASP.NET Core Web application uses the OpenID Connect middleware and MSAL for .NET to acquire an access token for the API on behalf of the signed-in user.
The source for the sample application is located under [`apps/aspnetcore-protect-api/`](/apps/aspnetcore-protect-api).

## Create the Azure AD applications

To create the Azure AD application we will use PowerShell and the AzureAD module.
The API will be running on port 5002 and the Web application is running on port 5003.

Connect to Azure AD Instance:

```powershell
Connect-AzureAD
```

### Step 1: Register an Azure AD Application for the API

To expose an API in Azure AD, that can be accessed by a client in the user's context, OAuth 2.0 Permission scopes must be exposed. These permissions are called `Delegated Permissions`.
Consider you want to implement an API that manages a Todo list for users, and you want to allow a consuming client app to access the Todo list of the signed-in user, you can define a permission `Todo.Read`. When the consuming client app wants to access the API in the user's context, the user must consent to `Todo.Read`. The consuming client can then acquire an access token that contains information about the user and the consuming client app.
As the access token must be used in the Http authorization header for each API call, the API can query information about the calling user to show only Todo items that belongs to the calling user. 
When creating an Azure AD application using the `New-AzureADApplication` Cmdlet, a scope named `user_impersonation` is created automatically. 

If you want to create additional scopes, you can use following PowerShell code:

```powershell
$exposedScopes = New-Object -TypeName Microsoft.Open.AzureAD.Model.OAuth2Permission
## With Type `User`, users can consent. With Type `Admin`, an Azure AD admin can consent only
$exposedScopes.Type = "User"
$exposedScopes.AdminConsentDisplayName = "AccessEcho Claims API"
$exposedScopes.AdminConsentDescription = "AccessEcho Claims API on behalf of signed-in users to echo claims."
$exposedScopes.Id = $(New-Guid)
$exposedScopes.IsEnabled = $true
$exposedScopes.Value = "myscope"
$exposedScopes.UserConsentDisplayName = $null
$exposedScopes.UserConsentDescription = $null
```

Next, create the Azure AD application for the API (without adding additional scopes):

```powershell
$api = New-AzureADApplication -DisplayName "EchoClaimsAPI" -IdentifierUris "https://echoclaimsapi"
```

Do the following if you want to add one or more scopes:

```powershell
$api = New-AzureADApplication -DisplayName "EchoClaimsAPI" -IdentifierUris "https://echoclaimsapi" -Oauth2Permissions $exposedScopes
```

Lastly, create a Service Principal for the application:

```powershell
New-AzureADServicePrincipal -AppId $api.AppId
```

### Step 2: Register an Azure AD application for the Web application

Firstly, we'll create a new application in AAD for our Web app:

```powershell
$app = New-AzureADApplication -DisplayName EchoClaimsWebApp -IdentifierUris "https://echoclaimswebapp" -ReplyUrls "http://localhost:5003/signin-oidc"
```

To acquire an access token for the API we'll use the OAuth2 code grant flow, therefore we need a client secret:

```powershell
$secret = New-Guid
New-AzureADApplicationPasswordCredential -ObjectId $app.ObjectId -CustomKeyIdentifier "ClientSecret" -Value $secret
```

Lastly, create a Service Principal for the application:

```powershell
New-AzureADServicePrincipal -AppId $app.AppId
```

## Run the demo application

In the demo application the ASP.NET Core web application calls the ASP.NET Core API which just returns the claims of the calling user.
The demo is located under [`apps/aspnetcore-protect-api/`](/apps/aspnetcore-protect-api/).

For running the demo application two shells need to be opened: one for the web application and one for the API.

### Run the API

Edit the file [`appsettings.json`](apps/aspnetcore-protect-api/WebApi/appsettings.json) for the API and replace `Domain`, `TenantId`, and `ClientId`.

In the first shell navigate to `apps/aspnetcore-protect-api/WebApi/` and run the following `dotnet` command

```shell
dotnet run
```
The API is listening on port 5002.

### Run the Web application

Edit the file [`appsettings.json`](apps/aspnetcore-protect-api/WebApplication/appsettings.json) for the Web App and replace `Domain`, `TenantId`, `ClientId` and `ClientSecret`.

In the second shell navigate to  `apps/aspnetcore-protect-api/WebApplication/` and run the following `dotnet` command.

```shell
dotnet run
```

The web application will be listening on port 5003.

### Testing everything together

Open a browser and navigate to [`http://localhost:5003`](http://localhost:5003) and sign in. Azure AD will ask you if you want to grant consent to the application permissions. After you have granted consent to access the EchoClaims API click the link "GetClaims". The claims for the signed-in user will be displayed.

Next, try to click the link "GetUserProfile". As you didn't grant consent to access full user's profile, the application needs to be redirected to Azure AD to gain consent to access the full profile of the user.

Take a look at the implementation of the [HomeController](apps/aspnetcore-protect-api/WebApplication/Controllers/HomeController.cs).
The method `GetUserProfile` tries to get the full profile of the signed-in user. In the catch statement the controller redirects to `Home/ConsentRequried` if an exception of type `MsalUiRequiredException` is thrown.
The View [ConsentRequired](apps/aspnetcore-protect-api/WebApplication/Views/Home/ConsentRequired.cshtml) informs the user that he or she must be redirected to Azure AD to grant consent to access the profile of the signed-in user. In the [AccountController](apps/aspnetcore-protect-api/WebApplication/Controllers/AccountController.cs) the method `GrantConsent` redirects the user to Azure AD to grant consent.


## Cleanup resources

The `Service Principal` should be automatically deleted when we delete the application. Be sure to delete both the Web App and the Web API!

### PowerShell

```powershell
Remove-AzAdApplication -ApplicationId $api.AppId -Force
Remove-AzAdApplication -ApplicationId $app.AppId -Force
```

### Azure CLI

```shell
az ad app delete --id <applicationid_webapi>
az ad app delete --id <applicationid_webapp>
```
