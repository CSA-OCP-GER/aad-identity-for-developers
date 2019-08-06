# OAuth2 Code Grant

## Here is what you'll learn

- How to register an Azure AD application
- How to authenticate an user and start an OAuth2 code grant flow 
- How to use an authorization code to acquire an access token to call the Microsoft Graph API
- How to use the `refresh_token` for obtaining new `access_token`

The Code Grant Flow is the most secure option for accessing resources on behalf of an user. However, this flow requires our application to have a server backend. It is not suited for web apps like SPAs that run 100% in the browser and do not have a backend.

## Create an AAD application

Before you can authenticate an user and acquire an access token for `microsoft.graph.com` you have to register an application in your Azure AD tenant.

This time we'll also set a password for the application. This is used during communication between your application's backend and the Graph API, but more on that later.

You can either use the PowerShell Module Az or Azure CLI.

### PowerShell

```powershell
# Import needed Az resource
Import-Module Az.Resources
# Create a new credential object
$credentials = New-Object Microsoft.Azure.Commands.ActiveDirectory.PSADPasswordCredential -Property @{ StartDate=Get-Date; EndDate=Get-Date -Year 2020; Password="supersupersupersecret123!"}
# Create the Azure AD application
$app = New-AzADApplication -DisplayName ChallengeIdTokenCode -IdentifierUris https://challengeidtokencode -ReplyUrls http://localhost:5001/api/tokenechocode -PasswordCredential $credentials
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

## Run the Token Echo Server

Open another shell and run the `Token Echo Server` from [`apps/token-echo-server`](apps/token-echo-server) in this repository. This helper ASP.NET Core tool is used to echo the token issued by your AAD. The tool is listening on port 5001 on your local machine. Tokens are accepted on the route `http://localhost:5001/api/tokenechocode`. That's the reason why we initially registered an AAD application with a reply url pointing to `http://localhost:5001/api/tokenechocode`.

```shell
dotnet run
``` 

## Create the authentication request with grant type `code`

Replace `TENANT_ID` with your AAD Tenant Id and `APPLICATION_ID` with your Application Id. Open a browser and paste the request:

```
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

Note down the `code`. If you have a close look at the request, you'll see what we are also asking for an `id_token` in `response_type`. Behind the scenes, this triggers not only an OAuth 2.0 Code Grant Flow, but also the implicit flow for requesting an `id_token` via OIDC. If we would want a plain OAuth 2.0 Code Grant Flow, we would set `response_type=code%20offline_access` and remove `openid profile` from `scope`. However, AAD will still request consent for both.

## Acquire an access token using the authorization code

Open [Visual Studio Code](https://code.visualstudio.com/) and run the following POST request using the [REST Client](https://marketplace.visualstudio.com/items?itemName=humao.rest-client) plugin by creating a new file called `requests.http`, save it, and add the HTTP request in it. Make sure to replace the following parameters:

* `TENANT_ID` - Your AAD tenant Id
* `APPLICATION_ID` - Your AAD application Id (from the create step at the top)
* `AUTHORIZATION_CODE` - The authorization code you just received during the step above

```HTTP
POST https://login.microsoftonline.com/TENANT_ID/oauth2/v2.0/token
Content-Type: application/x-www-form-urlencoded

client_id=APPLICATION_ID
&scope=https%3A%2F%2Fgraph.microsoft.com%2Fuser.read
&code=AUTHORIZATION_CODE
&redirect_uri=http%3A%2F%2Flocalhost%3A5001%2Fapi%2Ftokenechocode
&grant_type=authorization_code
&client_secret=supersupersupersecret123!
```

For real-world scenarios it is important to note that the `client_secret` is stored in your application's backend (e.g., in an Azure Key Vault) and that is being sent on the back-channel from your backend to AAD. This allows that **only our application** can convert the `code` into an `access_token` for accessing the Graph API - even if an attacker would get hold of the `code`.

The response should look something like this:

```json
{
    "token_type": "Bearer",
    "scope": "openid profile email https://graph.microsoft.com/User.Read",
    "expires_in": 3600,
    "ext_expires_in": 3600,
    "access_token": "eyJ0eXAiOiJKV1Qi...",
    "refresh_token": "OAQABAAAAAADCoMpjJXrxTq9VG9...",
    "id_token": "eyJ0eXAiOiJKV1QiL..."
}
```

Now you can copy the `access_token` from the response.

## Query Microsoft Graph API

Finally you can query your full profile from the Graph API using your `requests.http` by setting the `Authorization` header and passing in your newly generated `access_token`. As noted in the response, this token will expire after 3600 seconds.

```HTTP
###

GET https://graph.microsoft.com/v1.0/me
Authorization: Bearer <access_token>
```

## Acquire an access token using the refresh token

An `access_token` does not live forever - In fact, our initial `access_token` expires per default after 3600 seconds. The lifetime of access tokens is usually in the range of minutes to hours.

Once the initial `access_token` has expired, we can use the `refresh_token` to acquire a new `access_token`. This is possible as long as the signed-in user has granted consent or until the `refresh_token` times out. Typical life times for refresh tokens are in the range of days (e.g., 30 or 90 days). These two links will be helpful for further understanding the use and lifetime of these tokens:

* [Token Types](https://docs.microsoft.com/en-us/azure/active-directory/develop/active-directory-configurable-token-lifetimes#token-types) - What is an `access_token` and `refresh_token` 
* [Token Lifetime properties](https://docs.microsoft.com/en-us/azure/active-directory/develop/active-directory-configurable-token-lifetimes#configurable-token-lifetime-properties).

Now back to how to generate an `access_token` from the `refresh_token`. Add the following snippet to your `requests.http` and make sure to replace the following parameters:

* `TENANT_ID` - Your AAD tenant Id
* `APPLICATION_ID` - Your AAD application Id (from the create step at the top)
* `REFRESH_TOKEN` - The refresh token you just received during the step above

```HTTP
###

POST https://login.microsoftonline.com/TENANT_ID/oauth2/v2.0/token
Content-Type: application/x-www-form-urlencoded

client_id=APPLICATION_ID
&scope=https%3A%2F%2Fgraph.microsoft.com%2Fuser.read
&refresh_token=REFRESH_TOKEN
&grant_type=refresh_token
&client_secret=supersupersupersecret123!
```

The response should look very similar to what we saw before:

```json
{
    "token_type": "Bearer",
    "scope": "openid profile email https://graph.microsoft.com/User.Read",
    "expires_in": 3600,
    "ext_expires_in": 3600,
    "access_token": "eyJ0eXAiOiJKV1QiLCJub2...",
    "refresh_token": "OAQABAAAAAADCoMpjJXrxTq...",
    "id_token": "eyJ0eXAiOiJKV1QiLCJh..."
}
```

Now you can copy the `access_token` and call the Microsoft Graph API again.

## Cleanup resources

### PowerShell

```powershell
Remove-AzAdApplication -ApplicationId <applicationid> -Force
```

### Azure CLI

```shell
az ad app delete --id <applicationid>
```

## Summary

This challenge showed how to create a new application in AAD and use the OAuth 2.0 Code Grant Flow to request token for accessing the Graph API. The full process is described [here](https://docs.microsoft.com/en-us/azure/active-directory/develop/v2-oauth2-auth-code-flow).