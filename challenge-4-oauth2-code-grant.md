# OAuth2 code grant

## Here is what you learn
- register an Azure AD application
- authenticate a user and start an OAuth2 code grant flow 
- use an authorization code to acquire an access token to call Microsoft Graph API

## Create an AAD application

Before you can authenticate an user and acquire an access token for microsoft.graph.com you have to register an application in your Azure AD tenant. You also have to create 
You can either use the Powershell Module Az or Azure CLI.

### Powershell

``` Powershell
# Import needed Az resource
Import-Module Az.Resources
# Create a new credential object
$credentials = New-Object Microsoft.Azure.Commands.ActiveDirectory.PSADPasswordCredential -Property @{ StartDate=Get-Date; EndDate=Get-Date -Year 2020; Password="<your password>"}
# Create the Azure AD application
$app = New-AzADApplication -DisplayName ChallengeIdTokenCode -IdentifierUris https://challengeidtokencode -ReplyUrls http://localhost:5001/api/tokenechocode
# Create a Service Principal for your application
$sp = New-AzADServicePrincipal -ApplicationId $app.ApplicationId -PasswordCredential $credentials
```

Get the ID of your current AAD tenant.

``` Powershell
Get-AzContext
```

## Run the Token Echo Server

Open another shell and run the [Token Echo Server](apps/token-echo-server) (/apps/token-echo-server).
This helper ASP.NET Core tool is used to echo the token issued by your AAD. The tool is listening on port 5001 on your local machine. Tokens are accepted on route http://localhost:5001/api/tokenechocode . That's why we registered an AAD application with reply url http://localhost:5001/api/tokenechocode .

```
dotnet run
``` 

## Create the authentciation request with grant type ```code```

Replace tenant id and application id. Open browser und run the request.

```
GET
https://login.microsoftonline.com/<tenant id>/oauth2/v2.0/authorize?
client_id=<application id>
&response_type=id_token%20code
&redirect_uri=http%3A%2F%2Flocalhost%3A5001%2Fapi%2Ftokenechocode
&response_mode=form_post
&scope=openid%20profile%20https%3A%2F%2Fgraph.microsoft.com%2Fuser.read
&nonce=1234
```

## Acquire an access token using the authorization code

Copy the authorization code from your browser's output.
Open Postman or Insomnia and run the following POST request (replace tenant id, application id, authorization code and use the password that you created in the step above for your Service Principal).

``` HTTP
POST /<tenant id>/oauth2/v2.0/token HTTP/1.1
Host: https://login.microsoftonline.com
Content-Type: application/x-www-form-urlencoded

client_id=<application id>
&scope=https%3A%2F%2Fgraph.microsoft.com%2Fuser.read
&code=<authorization code>
&redirect_uri=http%3A%2F%2Flocalhost%3A5001%2Fapi%2Ftokenechocode
&grant_type=<authorization_code>&client_secret=<password>
```
Now you can copy the access_token from your browser's output.

If you don't want to use Postman or Insomnia you can invoke a web request using Powershell.

```Powershell
$result = Invoke-RestMethod -Uri https://login.microsoftonline.com/<tenant id>/oauth2/token? -Method Post -Body @{"grant_type" = "authorization_code";  "client_id" = "<application id>"; "client_secret" = "<password>"; "scope" = "https://graph.microsoft.com/User.Read"; "code" = "<authorization code>"; "redirect_uri" = "http://localhost:5001/api/tokenechocode"}
$result.access_token
```


## Query Microsoft Graph API

Now query your full profile using Postman or Insomnia to set authorization header.

```HTTP
GET https://graph.microsoft.com/v1.0/me
Header: authorization: Bearer <access_token>
```

If you don't want to use Postman or Insomnia, you can invoke a web request with Powershell

```Powershell
# Create the authorization header
$headers = @{'authorization'="Bearer $($result.access_token)"}
Invoke-WebRequest -Uri https://graph.microsoft.com/v1.0/me -Headers $headers -Method Get
```