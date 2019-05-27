# OAuth2 implicit flow

## Here is what you learn

- register an Azure AD application and allow the oauth2 implicit flow
- authenticate a user and start an OAuth2 implicit flow to acquire an access token to call Microsoft Graph API

## Create an Azure AD application and enable implict grant flow

Before you can authenticate an user and acquire an access token for microsoft.graph.com you have to register an application in your Azure AD tenant. By default the implicit grant flow is disabled.

### Powershell

To allow the oauth2 implicit flow the Powershell module ```AzureAD``` must be used. If you haven't already installed the Azure AD module do the following:

Open a shell and run it as an administrator and run the command Install-Module

```powershell
Install-Module AzureAD -Force
```

Create the AzureAD application:

```powershell
Import-Module AzureAD
Connect-AzureAD
New-AzureADApplication -DisplayName challengeimplicitgrant -IdentifierUris https://challengeimplicitgrantflow -ReplyUrls http://localhost:5001/api/tokenechofragment -Oauth2AllowImplicitFlow $true
```

## Run the Token Echo Server

Open another shell and run the [Token Echo Server](apps/token-echo-server) (/apps/token-echo-server).
This helper ASP.NET Core tool is used to echo the token issued by your AAD. The tool is listening on port 5001 on your local machine. Tokens are accepted on route http://localhost:5001/api/tokenechofragment . That's why we registered an AAD application with reply url http://localhost:5001/api/tokenechofragment .

```
dotnet run
```

## Create authentication request

Replace ```tenant``` with your TenantId and ```applicationid``` with your ApplicatinId. Open a browser and paste the modified request.

```
// Line breaks are for legibility only.

GET
https://login.microsoftonline.com/2a151364-d43b-4192-b727-ab106e85ccdd/oauth2/v2.0/authorize?
client_id=4f037430-b1bd-4ebc-a215-38ae6803d1e1
&response_type=id_token
&redirect_uri=http%3A%2F%2Flocalhost%3A5001%2Fapi%2Ftokenechofragment
&response_mode=fragment
&scope=openid%20profile
&nonce=1234
```

## Create the request to acquire an access token for Microsoft Graph API

```
// Line breaks are for legibility only.

GET
https://login.microsoftonline.com/2a151364-d43b-4192-b727-ab106e85ccdd/oauth2/v2.0/authorize?
client_id=4f037430-b1bd-4ebc-a215-38ae6803d1e1
&response_type=id_token%20token
&redirect_uri=http%3A%2F%2Flocalhost%3A5001%2Fapi%2Ftokenechofragment
&response_mode=fragment
&scope=openid%20profile%20User.Read
&nonce=1234
```

Take a look at your browser address bar. The access token is in the fragment of the url.

## Query Microsoft Graph API

Now query your full profile using Postman or Insomnia to set authorization header.

```HTTP
GET https://graph.microsoft.com/v1.0/me
Header: authorization: Bearer <access_token>
```

If you don't want to use Postman or Insomnia, you can invoke a web request with Powershell

```powershell
# Create the authorization header
$headers = @{'authorization'="Bearer {acess_token}"}
Invoke-WebRequest -Uri https://graph.microsoft.com/v1.0/me -Headers $headers -Method Get
```
