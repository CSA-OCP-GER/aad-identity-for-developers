# OAuth2 Implicit Flow

## Here is what you'll learn

- How to register an Azure AD application and allow the OAuth2 Implicit Grant Flow
- How to authenticate an user and start an OAuth2 implicit flow to acquire an access token to call Microsoft Graph API

*Important Note:*
The Implicit Grant Flow is less secure than the Code Grant Flow we looked at earlier. This is because the generation of the `access_token` for accessing the user's data on a resource server (e.g., the Graph API), is completely happening on the front channel. However, as the flow completely happens in the browser, it is suited for applications that do not have a server backend (e.g., 100% JS-based static web app). 

Nowadays, it is recommended to use the [authorization code flow with the PKCE extension](https://docs.microsoft.com/en-us/azure/active-directory/develop/v2-oauth2-auth-code-flow). We'll look into that later, but it helps to understand the implicit flow first.


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
New-AzureADApplication -DisplayName challengeimplicitgrant -IdentifierUris https://challengeimplicitgrantflow -ReplyUrls http://localhost:5001/api/tokenechofragment -Oauth2AllowImplicitFlow $true
```

### Azure CLI

Firstly, create a new Azure AD Application, this time with `oauth2-allow-implicit-flow` enabled:

```shell
az ad app create --display-name challengeimplicitgrant --reply-urls http://localhost:5001/api/tokenechofragment --identifier-uris https://challengeimplicitgrantflow --oauth2-allow-implicit-flow true
```

As before, note down the `appId`. Next, retrieve and note the ID of your current AAD tenant via:

```shell
az account show 
```

## Run the Token Echo Server

Open another shell and run the `Token Echo Server` from [`apps/token-echo-server`](apps/token-echo-server) in this repository. This helper ASP.NET Core tool is used to echo the token issued by your AAD. The tool is listening on port 5001 on your local machine. Tokens are accepted on the route `http://localhost:5001/api/tokenechofragment`. That's the reason why we initially registered an AAD application with a reply url pointing to `http://localhost:5001/api/tokenechofragment`.

Run the application via:

```
dotnet run
```

## Create authentication request for receiving an `id_token`

Replace `TENANT_ID` with your AAD Tenant Id and `APPLICATION_ID` with your Application Id. Open a browser and paste the request:

```http
GET
https://login.microsoftonline.com/TENANT_ID/oauth2/v2.0/authorize?
client_id=APPLICATION_ID
&response_type=id_token
&redirect_uri=http%3A%2F%2Flocalhost%3A5001%2Fapi%2Ftokenechofragment
&response_mode=fragment
&scope=openid%20profile
&nonce=1234
```

This will return an `id_token`, which the basic profile information for the user.

## Create the request to directly acquire an `access_token` for Microsoft Graph API

We can also directly request an `access_token` by specifying `token` in the `response_type` field. Adding `token` will allow our app to receive an `access_token` immediately from the authorize endpoint without having to make a second request to the token endpoint. If you use the token `response_type`, the scope parameter must contain a scope indicating which resource to issue the token for.

Replace `TENANT_ID` with your AAD Tenant Id and `APPLICATION_ID` with your Application Id. Open a browser and paste the request:

```http
GET
https://login.microsoftonline.com/TENANT_ID/oauth2/v2.0/authorize?
client_id=APPLICATION_ID
&response_type=id_token+token
&redirect_uri=http%3A%2F%2Flocalhost%3A5001%2Fapi%2Ftokenechofragment
&response_mode=fragment
&scope=openid%20profile%20User.Read
&nonce=1234
```

After executing the request, have a look at your browser address bar. The `access_token` is in the fragment of the url and should look something like this:

```
http://localhost:5001/api/tokenechofragment#access_token=eyJ0eX...&token_type=Bearer&expires_in=3599&scope=openid+profile+User.Read+email&id_token=eyJ0eXAiOi...&session_state=0f76c823-eac5-4ec0-9d4a-edf40b783583
```

Make sure to only copy the `access_token`, not the full remainder of the string!

More details on how the OAuth2 Implicit Grant Flow request can be used is documented [here](https://docs.microsoft.com/en-us/azure/active-directory/develop/v2-oauth2-implicit-grant-flow#send-the-sign-in-request).

## Query Microsoft Graph API

Finally we can query the full profile using Postman or Insomnia by setting the authorization header to our `access_token`:

```HTTP
GET https://graph.microsoft.com/v1.0/me
Header: authorization: Bearer <access_token>
```

If you don't want to use Postman or Insomnia, you can also invoke a web request with PowerShell:

```powershell
# Create the authorization header
$headers = @{'authorization'="Bearer {acess_token}"}
Invoke-WebRequest -Uri https://graph.microsoft.com/v1.0/me -Headers $headers -Method Get
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
