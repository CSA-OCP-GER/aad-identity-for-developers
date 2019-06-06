# Azure AD Application Roles

## Here is what you will learn
- How to register an application in Azure AD
- How to add app roles in your application and receive them in the token
- How to target roles for users and applications
- How to assign roles to an Azure AD user or group

Role-based access control is a mechanism to enforce authorization in applications. An administrator can assign roles to different users and groups to control who has access to what content and functionality.

The application roles for an Azure AD application can either be declared in the application manifest in the Azure portal, via PowerShell or Azure-CLI.

An application role is defined by the following structure:

```Json
"appId": "8763f1c4-f988-489c-a51e-158e9ef97d6a",
"appRoles": [
    {
      "allowedMemberTypes": [
        "User"
      ],
      "displayName": "Writer",
      "id": "d1c2ade8-98f8-45fd-aa4a-6d06b947c66f",
      "isEnabled": true,
      "description": "Writers Have the ability to create tasks.",
      "value": "Writer"
    }
  ],
"availableToOtherTenants": false,
```

### Note
>Each app role definition must have a unique valid GUID for the `id` property. 
>The `value` property of each app role definition should exactly match the strings that are used in code.
>No spaces are allowed for the `value` property.
>The `allowedMemberType` can either be `User`, `Application` or both. For the `allowedMemberType` of type `Application`, the app role appears as application permission in the Required Permissions of your application.

Here is an example of an app role for an application.

```Json
"appId": "8763f1c4-f988-489c-a51e-158e9ef97d6a",
"appRoles": [
    {
      "allowedMemberTypes": [
        "Application"
      ],
      "displayName": "ConsumerApps",
      "id": "47fbb575-859a-4941-89c9-0f7a6c30beac",
      "isEnabled": true,
      "description": "Consumer apps have access to the consumer data.",
      "value": "Consumer"
    }
  ],
"availableToOtherTenants": false,
```

In this challenge we use an application with two different roles:
* Admin: A role for administrators that have full access to content and functionality
* User: A role for users that have only access to a subset of content and functionality

## Register an Azure AD application and add app roles

To register an Azure AD application you can either use PowerShell or Azure-CLI

### PowerShell

```PowerShell
Import-Module AzureAD
Connect-AzureAD
# create the admin role
$adminrole = New-Object -TypeName Microsoft.Open.AzureAD.Model.AppRole
$adminrole.Description = "Administrator role: Admins have full access to content and functionality"
$adminrole.Id = New-Guid
$adminrole.IsEnabled = $true
$adminrole.Value = "Administrators"
$adminrole.DisplayName = "Administrators"
# create the user role
$userrole = New-Object -TypeName Microsoft.Open.AzureAD.Model.AppRole
$userrole.AllowedMemberTypes = "User"
$userrole.Description = "Users role: Users have access to a subset of content and functionality"
$userrole.Id = New-Guid
$userrole.IsEnabled = $true
$userrole.Value = "Users"
$userrole.DisplayName = "Users"
# create the application
$app = New-AzureADApplication -DisplayName "challengeapproles" -IdentifierUris "https://challengeapproles" -ReplyUrls "http://localhost:5001/api/tokenecho" -AppRoles $adminrole,$userrole
```

### Azure-CLI
Todo

## Run the Token Echo Server

Open another shell and run the Token Echo Server from [`apps/token-echo-server`](apps/token-echo-server) in this repository. This helper ASP.NET Core tool is used to echo the token issued by your AAD and "simulates" our website or server backend that would receive the `id_token`.
The tool is listening on port 5001 on your local machine. Tokens are accepted on the route `http://localhost:5001/api/tokenecho`. this is why we initially registered an AAD application with a reply url pointing to `http://localhost:5001/api/tokenecho`.

```
dotnet run
```

## Create an authentication request

Replace `TENANT_ID` with your TenantId and `APPLICATION_ID` with your ApplicationId. Open a browser and paste the modified request.

```
// Line breaks are for readability only

GET
https://login.microsoftonline.com/2a151364-d43b-4192-b727-ab106e85ccdd/oauth2/v2.0/authorize?
client_id=f0ce9a58-0a6a-41e6-bf05-921fbf684710
&response_type=id_token
&redirect_uri=http%3A%2F%2Flocalhost%3A5001%2Fapi%2Ftokenecho
&response_mode=form_post
&scope=openid%20profile
&nonce=1234
```

Copy the `id_token` value from your browser output, go to [https://jwt.ms](https://jwt.ms) and paste the token. Take a minute and have a look at the decoded token.
You will not find any information about an assigned app role!!

## Assign an application role

Now we assign the app role `Administrator` to your Azure AD user. 

### PowerShell

```PowerShell
# Get the ServicePrincipal
$sp = Get-AzureADServicePrincipal -SearchString $app.DisplayName
$user = Get-AzureADUser -ObjectId "<your user's UPN e.g. max.mustermann@muster.onmicrosoft.com>"
New-AzureADUserAppRoleAssignment -ObjectId $user.ObjectId -PrincipalId $user.ObjectId -ResourceId $sp.ObjectId -Id $adminrole.Id
```

### Azure CLI
Todo

## Create the authentication request again

Create the authentication request again and take look at the token.
You will see that there is a new claim `roles` issued that contains your app roles that you were assigned to.