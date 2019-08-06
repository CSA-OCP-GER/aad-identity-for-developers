# Integrate an Angular SPA into Azure AD

## Here is what you'll learn

- How to register an AAD application
- How to authenticate Azure AD users in an Angular SPA (Single Page Application)

This challenge is similar to [challenge 2](challenge-2-oidc-idtoken-fragment.md), but uses an Angular instead of .NET Core and a Fragment URL for receiving the `id_token`.

There is already a simple [Angular SPA demo](apps/angular-spa-oidc-token) implemented in this repository.
The demo application uses the [MSAL for Angular](https://www.npmjs.com/package/@azure/msal-angular) library. MSAL is a library that gives your app the ability to integrate with Azure AD.
[MSAL for Angular](https://www.npmjs.com/package/@azure/msal-angular) is an Angular wrapper for [MSAL.js](https://github.com/AzureAD/microsoft-authentication-library-for-js).

## Create an AAD application

Before you can authenticate an user you have to register an application in your AAD tenant.
You can either use the PowerShell Module Az or Azure CLI.

### PowerShell

```powershell
New-AzADApplication -DisplayName ChallengeIdTokenAngularSPA -IdentifierUris https://challengeidtokenangularspa -ReplyUrls http://localhost:5003
```

Retrieve and note the ID of your current AAD tenant via:

```powershell
Get-AzContext
```

### Azure CLI

```shell
az ad app create --display-name challengeidtokenangularspa --reply-urls http://localhost:5003 --identifier-uris https://challengeidtokenangularspa
```

Retrieve and note the ID of your current AAD tenant via:

```shell
az account show 
```

## Run the Angular SPA demo

Before we can run the demo application, we have to replace some values in [`app.module.ts`](apps/angular-spa-oidc-token/src/app/app.module.ts). Open the file [`app.module.ts`](apps/angular-spa-oidc-token/src/app/app.module.ts) and replace the following values:

- `clientID` -> your application id
- `authority` -> your tenant id

After you have replaced the values open a shell and navigate to the demo application's root folder [`apps/angular-spa-oidc-token`](apps/angular-spa-oidc-token) an run the following command:

```shell
npm install
ng serve --port 5003
```

This command brings up the demo application listening on port 5003. Open a browser and navigate to [`http://localhost:5003`](http://localhost:5003) and use the app.

## Cleanup resources

### PowerShell

```powershell
Remove-AzAdApplication -ApplicationId <applicationid> -Force
```

### Azure CLI

```shell
az ad app delete --id <applicationid>
```