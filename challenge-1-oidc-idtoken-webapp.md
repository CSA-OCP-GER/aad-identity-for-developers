# Authenticate users in a web app using Azure AD

## Here is what you learn

- register an AAD application
- authenticate users using [MSAL](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet)

## Description

In this challenge we create a simple ASP.NET Core Web Application that uses Azure AD to authenticate users.
The sample application can be found [here](apps/simplewebapplication) (apps/simplewebapplication).

The following picture demonstrates the authentication flow.

![alt-text](images/simple-webapp-auth-flow.png)

When the id token is received by the web application, a session cookie is created that contains the id token.
Each additional call sends the cookie to the server so that the request can check the existence of a valid id token.
Each ASP.NET Core controller uses the [Authorize](https://docs.microsoft.com/en-us/aspnet/core/security/authorization/simple?view=aspnetcore-2.2) attribute to indicate that the controller method's can only be invoked by an authenticated user.