# Request an ID Token from AAD

## Here is what you'll learn

- How to register an application in AAD
- How to use the OpenID Connect protocol for authenticating an user
- How to receive an `id_token` with basic profile information about the authenticated user

Here is an high-level overview of the authentication process:

![alt-text](images/oidc-id-token.png)

In short:

1. We show the user a sign-in button
1. The sign-in button forwards to the `.../oauth2/v2.0/authorize` URL, including the ID of our application and the ID of our AAD tenant
1. The user logs in and consents to letting us access his or her profile
1. Our AAD tenants forwards us to the callback URL and includes an `id_token`, which contains basic profile information of the user in form of a JWT (JSON Web Token)
1. Lastly, we could validate the returned `id_token` using its signature (not shown here, most libraries do this for us)

## Create an AAD application

Before we can authenticate an user we have to register an application in our AAD tenant. We can either use the PowerShell Module Az or the Azure CLI.

### PowerShell

```powershell
New-AzADApplication -DisplayName ChallengeIdToken -IdentifierUris https://challengeidtoken -ReplyUrls http://localhost:5001/api/tokenecho
```

**Note:** The `IdentifierUris` needs to be unique within an instance of AAD.

Now, retrieve the ID of your current AAD tenant via:

```powershell
Get-AzContext
```

### Azure CLI

```shell
az ad app create --display-name challengeidtokencli --reply-urls http://localhost:5001/api/tokenecho --identifier-uris https://challengeidtoken
```
**Note:** The `IdentifierUris` needs to be unique within an instance of AAD.

Retrieve the ID of your current AAD tenant via:

```shell
az account show 
```

### Viewing your ApplicationId

Note the `appId` value in the response - this is the id under which your AAD application has been registered. In the Azure Portal, we can see your new app registration under `AAD --> App Registrations --> Owned applications`:

![alt-text](images/aad_app_registration.png)

## Run the Token Echo Server

Open another shell and run the Token Echo Server from [`apps/token-echo-server`](apps/token-echo-server) in this repository. This helper ASP.NET Core tool is used to echo the token issued by your AAD and "simulates" our website or server backend that would receive the `id_token`.

The tool is listening on port 5001 on your local machine. Tokens are accepted on the route `http://localhost:5001/api/tokenecho`. This is why we initially registered an AAD application with a reply url pointing to `http://localhost:5001/api/tokenecho`.

```
dotnet run
```

## Create an authentication request

Replace `TENANT_ID` with your AAD Tenant Id and `APPLICATION_ID` with your Application Id. Open a browser and paste the modified request.

```
https://login.microsoftonline.com/TENANT_ID/oauth2/v2.0/authorize?
client_id=APPLICATION_ID
&response_type=id_token
&redirect_uri=http%3A%2F%2Flocalhost%3A5001%2Fapi%2Ftokenecho
&response_mode=form_post
&scope=openid%20profile
&nonce=1234
```

For explanation, `openid` scope allows the user to sign in, and `profile` scope allows us to read the basic profile information of the user.

Copy the `id_token` value from your browser output, go to [https://jwt.ms](https://jwt.ms) and paste the token. Take a minute and have a look at the decoded token.

If you need further information about the issued claims take a look [here](https://docs.microsoft.com/en-us/azure/active-directory/develop/id-tokens#header-claims).

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

This challenge showed how to create a new application in AAD and how user can be authenticated using the Open ID Connect protocol. The full process is described [here](https://docs.microsoft.com/en-us/azure/active-directory/develop/v2-protocols-oidc).