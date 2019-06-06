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
