# Oauth2 implicit flow in an Angular SPA

## Here is what your learn

- register an Azure AD application and allow the oauth2 implicit flow
- run an Angular SPA demo application the calls Microsft Graph API to read an user's profile

## Create an Azure AD application and enable implict grant flow

Before you can authenticate an user and acquire an access token for microsoft.graph.com you have to register an application in your Azure AD tenant. By default the implicit grant flow is disabled.

### Powershell

To allow the oauth2 implicit flow the Powershell module ```AzureAD``` must be used. If you haven't already installed the Azure AD module do the following:

Open a shell and run it as an administrator and run the command Install-Module

```Powershell
Install-Module AzureAD -Force
```

Create the AzureAD application:

``` Powershell
Impprt-Module AzureAD
Connect-AzureAD
New-AzureADApplication -DisplayName challengeimplicitgrantangularspa -IdentifierUris https://challengeimplicitgrantflowangularspa -ReplyUrls http://localhost:5003 -Oauth2AllowImplicitFlow $true
```

## Run the demo Angular SPA

In your shell navigate to the folder of the [Angular SPA demo](/apps/angular-spa-oauth2-implicit-flow) /apps/angular-spa-oauth2-implicit-flow.
Open and edit the file [app.modules.ts](/apps/angular-spa-oauth2-implicit-flow/src/app/app.module.ts).
Replace ApplicationId and TenantId.
Run the ```ng serve``` command and start the application on port 5003.

```Shell
ng serve --port 5003
```

Open your browser and navigate to http://localhost:5003.
After you have logged in you can get your profile details by clicking ```GetProfile```.

In [app.component.ts](/apps/angular-spa-oauth2-implicit-flow/src/app/app.component.ts) you can see the event handler onGetProfile():

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

This handler calls the method getUserProfile() from the service [graph.service.ts](/apps/angular-spa-oauth2-implicit-flow/src/app/services/graph.service.ts). Take a look at the implementation.
Here MSAL is used to acquire an access token for Microsoft Graph API.

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
                    alert(response);
                    return JSON.stringify(response);
                })
                .pipe(catchError(_this.handleError));
        });
}
```