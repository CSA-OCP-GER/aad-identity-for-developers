# Receive an ID Token from AAD

## Here is what you learn

- register an AAD application
- create an OpenIdConnect request to authenticate an user
- receive an ID token to get informations about the authenticated user

## Create an AAD application

Before you can authenticate an user you have to register an application in your AAD tenant.
You can either use the Powershell Module Az or Azure CLI.

### Powershell

``` Powershell
New-AzADApplication -DisplayName ChallengeIdToken -IdentifierUris https://challengeidtoken -ReplyUrls http://localhost:5001/api/tokenecho
```
Get the ID of your current AAD tenant.

``` Powershell
Get-AzContext
```
### Azure CLI

```Shell
az ad app create --display-name challengeidtokencli --reply-urls http://localhost:5001/api/tokenecho
```

Get the ID of your current AAD tenant

```Shell
az account show 
```

## Run the Token Echo Server

Open another shell and run the [Token Echo Server](apps/token-echo-server) (/apps/token-echo-server).
This helper ASP.NET Core tool is used to echo the token issued by your AAD. The tool is listening on port 5001 on your local machine. Tokens are accepted on route http://localhost/api/tokenecho . That's why we registered an AAD application with reply url http://localhost/api/tokenecho .

```
dotnet run
```

## Create authentication request

Replace ```tenant``` with your TenantId and ```applicationid``` with your ApplicatinId. Open a browser and paste the modified request.

```
// Line breaks are for legibility only.

GET
https://login.microsoftonline.com/{tenant}/oauth2/v2.0/authorize?
client_id={applicationid}
&response_type=id_token
&redirect_uri=http%3A%2F%2Flocalhost%3A5001%2Fapi%2Ftokenecho
&response_mode=form_post
&scope=openid%20profile
&nonce=1234
```
Copy the id_token value from your browser output, go to https://jwt.ms and paste the token.
Take a look at the decoded token.
If you need further informations about the issued claims take a look here: 
https://docs.microsoft.com/en-us/azure/active-directory/develop/id-tokens#header-claims


## Cleanup resources

### Powershell

```Powershell
Remove-AzAdApplication -ApplicationId <applicationid> -Force
```

### Azure CLI

```Shell
az ad app delete --id <applicationid>
```

## Summary

This challenge showed how to create an Application in AAD and how an OpenIdConnect request is created to authenticate a user.