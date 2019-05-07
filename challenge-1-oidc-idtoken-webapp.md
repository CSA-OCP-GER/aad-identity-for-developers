# Authenticate users in a web app using Azure AD

## Here is what you learn

- register an AAD application
- authenticate users in a ASP.NET Core MVC application

## Description

In this challenge we create a simple ASP.NET Core MVC Application that uses Azure AD to authenticate users.
The sample application can be found [here](apps/aspnetcore-mvc-oidc-idtoken) (apps/aspnetcore-mvc-oidc-idtoken).

The following picture demonstrates the authentication flow.

![alt-text](images/simple-webapp-auth-flow.png)

When the id token is received by the web application, a session cookie is created that contains the id token.
Each additional call sends the cookie to the server so that the request can check the existence of a valid id token.
Each ASP.NET Core controller uses the [Authorize](https://docs.microsoft.com/en-us/aspnet/core/security/authorization/simple?view=aspnetcore-2.2) attribute to indicate that the controller method's can only be invoked by an authenticated user.

In the previous [challenge](challenge-0-oidc-idtoken.md) a token echo server was used to receive the id token from Azure AD. In ASP.NET Core a special route ```host/signin-oidc``` is created to handle the post back from Azure AD containing the id token. 
We have to use this route as reply url when we create the Azure AD application for the demo ASP.NET Core MVC application.
The demo is listening on port 5002.

## Create the Azure AD application

### Powershell

```Powershell
New-AzADApplication -DisplayName ChallengeIdTokenAspNet -IdentifierUris https://challengeidtoken -ReplyUrls http://localhost:5002/signin-oidc
```

Save the ApplicationId.

### Azure CLI

```Shell
az ad app create --display-name ChallengeIdTokenAspNet --reply-urls http://localhost:5002/signin-oidc
```
Save the ApplicationId.

## Configure appsettings.json

Open the [appsettings.json](apps/aspnetcore-mvc-oidc-idtoken) and replace values:
- ```Domain``` -> your domain e.g. mycompany.onmicrosoft.com
- ```TenantId``` -> your Azure AD directory id
- ```ClientId``` -> ApplicationId of the newly created application

## Run the application

Open a shell and navigate to apps/apsnetcore-mvc-oidc-idtoken folder.
Run the dotnet command.

```Shell
dotnet run
```

Open a browser and navigate to http://localhost:5002

## Cleanup resources

### Powershell

```Powershell
Remove-AzAdApplication -ApplicationId <applicationid> -Force
```

### Azure CLI

```Shell
az ad app delete --id <applicationid>
```