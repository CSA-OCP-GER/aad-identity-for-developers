# OAuth2 code grant flow in an ASP.NET Core MVC web application

## Here is what you learn
- register an Azure AD application
- authenticate an user and start an OAuth2 code grant flow
- acquire an access token to call Microsoft Graph API using MSAL.NET

In this challenge we use an already implemented ASP.NET Core MVC web application that authenticates users and acquires an access token for Microsoft Graph API on behalf of the authenticated user. The access token is used to query detailed profile information about the authenticated user. [MSAL.NET](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet) is used to acquire and manage access tokens.

## Create an AAD application

Before you can authenticate an user and acquire an access token for microsoft.graph.com you have to register an application in your Azure AD tenant. You also have to create 
You can either use the Powershell Module Az or Azure CLI.

### Powershell

``` Powershell
# Create the Azure AD application
$app = New-AzADApplication -DisplayName ChallengeIdTokenCodeWebApp -IdentifierUris https://challengeidtokencodewebapp -ReplyUrls http://localhost:5004
#Create an application credential
New-AzADAppCredential -ObjectId $app.ObjectId -Password $(ConvertTo-SecureString -String "<Password>" -AsPlainText -Force)
# Create a Service Principal for your application
$sp = New-AzADServicePrincipal -ApplicationId $app.ApplicationId
```

Get the ID of your current AAD tenant.

``` Powershell
Get-AzContext
```