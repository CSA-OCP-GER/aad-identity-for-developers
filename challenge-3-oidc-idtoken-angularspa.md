# Integrate an Angular SPA into Azure AD

## Here is what you learn

- register an Azure AD application
- authenticate Azure AD users in an Angular SPA

There is already a simple [Angular SPA demo](apps/angular-spa-oidc-token) implemented in this repository.
The demo application uses [MSAL for Angular](https://www.npmjs.com/package/@azure/msal-angular). MSAL is a library that gives your app the ability to integrate in Azure AD. 
[MSAL for Angular](https://www.npmjs.com/package/@azure/msal-angular) is an Angular wrapper for [MSAL.js](https://github.com/AzureAD/microsoft-authentication-library-for-js).

## Create an Azure AD application

Before you can authenticate an user you have to register an application in your Azure AD tenant.
You can either use the Powershell Module Az or Azure CLI.

### Powershell

``` Powershell
New-AzADApplication -DisplayName ChallengeIdTokenAngularSPA -IdentifierUris https://challengeidtokenangularspa -ReplyUrls http://localhost:5003
```
Get the ID of your current Azure AD tenant.

``` Powershell
Get-AzContext
```
### Azure CLI

```Shell
az ad app create --display-name challengeidtokenangularspa --reply-urls http://localhost:5003
```

Get the ID of your current Azure AD tenant

```Shell
az account show 
```

## Run the Angular SPA demo

Before we can run the demo application, we have to replace some values in [app.module.ts](apps/angular-spa-oidc-token/src/app/app.module.ts). Open the file [app.module.ts](apps/angular-spa-oidc-token/src/app/app.module.ts) and replace the following values:
- clientID -> your application id
- authority -> your tenant id

After you have replaced the values open a shell and navigate to the demo application's root folder [angular-spa-oidc-token](apps/angular-spa-oidc-token) an run the following command:

```Shell
ng serve --port 5003
```
This command brings up the demo application listening on port 5003.
Open a browser and navigate to http://localhost:5003 .

## Cleanup resources

### Powershell

```Powershell
Remove-AzAdApplication -ApplicationId <applicationid> -Force
```

### Azure CLI

```Shell
az ad app delete --id <applicationid>
```