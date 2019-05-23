# OAuth2 code grant

## Here is what you'll learn

- How to register an Azure AD application
- How to authenticate an user and start an OAuth2 code grant flow 
- How to use an authorization code to acquire an access token to call the Microsoft Graph API
- How to use refresh tokens

## Create an AAD application

Before you can authenticate an user and acquire an access token for `microsoft.graph.com` you have to register an application in your Azure AD tenant. You also have to create 

You can either use the PowerShell Module Az or Azure CLI.

### PowerShell

TODO: Make sure the password is properly set!

```powershell
# Import needed Az resource
Import-Module Az.Resources
# Create a new credential object
$credentials = New-Object Microsoft.Azure.Commands.ActiveDirectory.PSADPasswordCredential -Property @{ StartDate=Get-Date; EndDate=Get-Date -Year 2020; Password="<your password>"}
# Create the Azure AD application
$app = New-AzADApplication -DisplayName ChallengeIdTokenCode -IdentifierUris https://challengeidtokencode -ReplyUrls http://localhost:5001/api/tokenechocode
# Create a Service Principal for your application
$sp = New-AzADServicePrincipal -ApplicationId $app.ApplicationId -PasswordCredential $credentials
```

Retrieve and note the ID of your current AAD tenant via:

```powershell
Get-AzContext
```

### Azure CLI

First, create a new application, but this time we need to specify a password (we won't generate a secure password in this exercise):

```shell
az ad app create --display-name challengeidtokencode --reply-urls http://localhost:5001/api/tokenechocode --identifier-uris https://challengeidtokencode --password supersupersupersecret123!
```

Now, let's create a `Service Principal` with the same id:

```shell
az ad sp create --id <APP_ID>
```

TODO: Explain why we do this.

## Run the Token Echo Server

Open another shell and run the `Token Echo Server` from [`apps/token-echo-server`](apps/token-echo-server) in this repository. This helper ASP.NET Core tool is used to echo the token issued by your AAD. The tool is listening on port 5001 on your local machine. Tokens are accepted on the route `http://localhost:5001/api/tokenechocode`. this is why we initially registered an AAD application with a reply url pointing to `http://localhost:5001/api/tokenechocode`.

```
dotnet run
``` 

## Create the authentication request with grant type `code` and add the scope `offline_access`

To retrieve a refresh token we must gain consent from the signed-in user, therefore we have to add an additional scope named `offline_access`.

Replace `TENANT_ID` with your AAD Tenant Id and `APPLICATION_ID` with your Application Id. Open a browser and paste the request:

```
GET
https://login.microsoftonline.com/TENANT_ID/oauth2/v2.0/authorize?
client_id=APPLICATION_ID
&response_type=id_token%20code
&redirect_uri=http%3A%2F%2Flocalhost%3A5001%2Fapi%2Ftokenechocode
&response_mode=form_post
&scope=openid%20profile%20offline_access%20https%3A%2F%2Fgraph.microsoft.com%2Fuser.read
&nonce=1234
```

The response should look like this:

```json
{
  "code": "OAQABAAIAAADCoMpjJ....",
  "id_token": "eyJ0eXAiOiJK...",
  "session_state": "0f76c823-..."
}
```

Note down the `code`.

## Acquire an access token and refresh token using the authorization code

Open Postman or Insomnia and run the following POST request - make sure to replace the following parameters:

* `TENANT_ID` - Your AAD tenant Id
* `APPLICATION_ID` - Your AAD application Id (from the create step at the top)
* `AUTHORIZATION_CODE` - The authorization code you just received during the step above
* `PASSWORD` - Your AAD application's password (specified during the create step at the top)

```HTTP
POST /TENANT_ID/oauth2/v2.0/token HTTP/1.1
Host: https://login.microsoftonline.com
Content-Type: application/x-www-form-urlencoded

client_id=APPLICATION_ID
&scope=offline_access%20https%3A%2F%2Fgraph.microsoft.com%2Fuser.read
&code=AUTHORIZATION_CODE
&redirect_uri=http%3A%2F%2Flocalhost%3A5001%2Fapi%2Ftokenechocode
&grant_type=authorization_code
&client_secret=PASSWORD // NOTE: Only required for web apps
```

Now you can copy the `access_token` and `refresh_token`from the response.

If you don't want to use Postman or Insomnia you can invoke a web request using PowerShell.

```powershell
$result = Invoke-RestMethod -Uri https://login.microsoftonline.com/<tenant id>/oauth2/token? -Method Post -Body @{"grant_type" = "authorization_code";  "client_id" = "<application id>"; "client_secret" = "<password>"; "scope" = "https://graph.microsoft.com/User.Read"; "code" = "<authorization code>"; "redirect_uri" = "http://localhost:5001/api/tokenechocode"}
$result.access_token
```

## Query Microsoft Graph API

Finally you can query your full profile using Postman or Insomnia by properly setting the `Authorization` header and passing in your newly generated `access_token`.

```HTTP
GET https://graph.microsoft.com/v1.0/me
Header: Authorization: Bearer <access_token>
```

If you don't want to use Postman or Insomnia, you can invoke a web request with PowerShell:

```powershell
# Create the authorization header
$headers = @{'authorization'="Bearer $($result.access_token)"}
Invoke-WebRequest -Uri https://graph.microsoft.com/v1.0/me -Headers $headers -Method Get
```

## Acquire an access token using the refresh token.

With a refresh token you can acquire access token for all protected resources as lon as the signed-in user has granted consent.

Open Postman or Insomnia and run the following POST request - make sure to replace the following parameters:

* `TENANT_ID` - Your AAD tenant Id
* `APPLICATION_ID` - Your AAD application Id (from the create step at the top)
* `REFRESH_TOKEN` - The refresh token you just received during the step above
* `PASSWORD` - Your AAD application's password (specified during the create step at the top)

```HTTP
POST /TENANT_ID/oauth2/v2.0/token HTTP/1.1
Host: https://login.microsoftonline.com
Content-Type: application/x-www-form-urlencoded

client_id=APPLICATION_ID
&scope=https%3A%2F%2Fgraph.microsoft.com%2Fuser.read
&refresh_token=REFRESH_TOKEN
&grant_type=refresh_token
&client_secret=PASSWORD      // NOTE: Only required for web apps
```

If you don't want to use Postman or Insomnia you can invoke a web request using PowerShell.

```powershell
$result = Invoke-RestMethod -Uri https://login.microsoftonline.com/<tenant id>/oauth2/token? -Method Post -Body @{"grant_type" = "refresh_token";  "client_id" = "<application id>"; "client_secret" = "<password>"; "scope" = "https://graph.microsoft.com/User.Read"; "refresh_token" = "<refresh_token>"}
$result.access_token
```

Now you can copy the `access_token` and call Microsoft Graph API again.

## Cleanup resources

The `Service Principal` should be automatically deleted when we delete the application.

### PowerShell

```powershell
Remove-AzAdApplication -ApplicationId <applicationid> -Force
```

### Azure CLI

```shell
az ad app delete --id <applicationid>
```
