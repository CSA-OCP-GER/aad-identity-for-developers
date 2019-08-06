# aad-identity-for-developers

This repository gives several examples on how to integrate your your applications with Azure Active Directory.

## Prerequisites

* Azure CLI or PowerShell Az module
* ASP.NET Core 2.2
* Angular CLI
* [Visual Studio Code](https://code.visualstudio.com/)
* [REST Client](https://marketplace.visualstudio.com/items?itemName=humao.rest-client) plugin for VSCode
* Node.js

## Challenges

* [00 - OIDC - Registering an application in AAD and receiving an ID Token](challenge-0-oidc-idtoken.md)
* [01 - OIDC - Authenticating users in a web app using Azure AD](challenge-1-oidc-idtoken-webapp.md)
* [02 - OIDC - Registering an application in AAD and receiving an ID Token via a URL Fragment](challenge-2-oidc-idtoken-fragment.md)
* [03 - OIDC - Integrating an Angular SPA (Single page application) with Azure AD](challenge-3-oidc-idtoken-angularspa.md)
* [04 - OAuth2 - Code Grant Flow](challenge-4-oauth2-code-grant.md)
* 06 - TODO
* [07 - OAuth2 - Implicit Grant Flow](challenge-7-oauth2-implicit-flow.md)
* [08 - OAuth2 - Implicit Grant Flow in a SPA (Single Page Application)](challenge-8-oauth2-implicit-flow-angularspa.md)
* [09 - OAuth2 - Create an API that is protected by Azure AD](challenge-9-oauth2-protect-api.md)

## Quick Terminology Refreshers

* OAuth 2.0 - an open standard for access delegation with a focus on **authorization** - *"Allows apps or services to make (restricted) API calls to other services without needing the password of the user"*
* OpenID Connect (OIDC) - An identity layer built on top of OAuth 2.0 for **authentication** - *"Allows apps or services to know who the user is and authenticate him/her, including basic profile information"*
* Claims - TODO
* Scopes - TODO
* Consent - TODO
* Resource Server - TODO
* Authorization Server - TODO
* ID Token - TODO
* Access Token - TODO
* Refresh Token - TODO

## Authors

* Andreas Mock - [@andreasM009](https://twitter.com/andreasm009)
* Clemens Siebler - [@clemenssiebler](https://twitter.com/clemenssiebler)