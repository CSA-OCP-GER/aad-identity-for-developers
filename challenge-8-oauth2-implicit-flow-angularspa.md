# OAuth2 Implicit Flow in an Angular SPA (Single Page Application)

## Here is what you'll learn

- How to register an Azure AD application and allow the OAuth2 Implicit Grant Flow
- How to run an Angular SPA demo application that calls the Microsoft Graph API to read an user's profile

## Create an Azure AD application and enable Implicit grant flow

Before you can authenticate an user and acquire an access token for `microsoft.graph.com` you have to register an application in your Azure AD tenant. By default the implicit grant flow is disabled.

### PowerShell

To allow the OAuth2 implicit flow the PowerShell module `AzureAD` must be used. The `Azure Shell` within the Azure Portal already has it pre-installed. If you want to run the code on local machine and haven't already installed the Azure AD module do the following:

Open a shell and run it as an administrator and run the command `Install-Module`:

```powershell
Install-Module AzureAD -Force
```

Then create a new Azure AD application:

```powershell
Import-Module AzureAD
Connect-AzureAD
New-AzureADApplication -DisplayName challengeimplicitgrantangularspa -IdentifierUris https://challengeimplicitgrantflowangularspa -ReplyUrls http://localhost:5003 -Oauth2AllowImplicitFlow $true
```

### Azure CLI

Firstly, create a new Azure AD Application, this time with `oauth2-allow-implicit-flow` enabled:

```shell
az ad app create --display-name challengeimplicitgrantangularspa --reply-urls http://localhost:5003 --identifier-uris https://challengeimplicitgrantflowangularspa --oauth2-allow-implicit-flow true
```

As before, note down the `appId`. Next, retrieve and note the ID of your current AAD tenant via:

```shell
az account show 
```

## Run the demo Angular SPA

In your shell navigate to the folder of the `Angular SPA demo` under [`apps/angular-spa-oauth2-implicit-flow`](apps/angular-spa-oauth2-implicit-flow/).

Open and edit the file [`app.modules.ts`](apps/angular-spa-oauth2-implicit-flow/src/app/app.module.ts) and replace `ApplicationId` and `TenantId`.

Install the dependencies of the app and then start it on port 5003:

```shell
npm install
ng serve --port 5003
```

Open your browser and navigate to [`http://localhost:5003`](http://localhost:5003). After you have logged in you can get your profile details by clicking `Get Profile`.

In [`app.component.ts`](/apps/angular-spa-oauth2-implicit-flow/src/app/app.component.ts) you can see the event handler `onGetProfile()`:

```Typescript
public onGetUserProfile() : void
{
this.graphService.getUserProfile()
    .then(result => result.subscribe(profile => 
    {
        this.profile = profile;
    }));
}
```

This handler calls the method `getUserProfile()` from the service [`graph.service.ts`](/apps/angular-spa-oauth2-implicit-flow/src/app/services/graph.service.ts). Take a look at the implementation - here MSAL is used to acquire an access token for Microsoft Graph API.

```Typescript
public getUserProfile(): Promise<Observable<string>>
{
    var _this = this;
    return this.authService.acquireTokenSilent(["User.Read"])
        .then((token) => 
        {
            var options = {
                headers: new HttpHeaders({'Authorization': 'Bearer ' + token})
            };

            return this.http.get(_this.endpoint + "me", options)
                .map((response: Response) => 
                {
                    return JSON.stringify(response);
                })
                .pipe(catchError(_this.handleError));
        });
}
```

## Cleanup resources

### PowerShell

```powershell
Remove-AzAdApplication -ApplicationId <applicationid> -Force
```

### Azure CLI

```shell
az ad app delete --id <applicationid>
```
