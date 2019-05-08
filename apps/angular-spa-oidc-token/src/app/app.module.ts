import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { MsalModule } from '@azure/msal-angular';

import { CommonModule } from '@angular/common';  

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';

@NgModule({
  declarations: [
    AppComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    CommonModule,
    MsalModule.forRoot({
      clientID: "<application id>",
      authority: "https://login.microsoftonline.com/<tenant id>/",
      redirectUri: "http://localhost:5003",
      validateAuthority : true,
      cacheLocation : "localStorage",
      postLogoutRedirectUri: "http://localhost:5003",
      navigateToLoginRequestUrl : true,
      popUp: false,
      consentScopes: [],
      unprotectedResources: ["https://angularjs.org/"],
      correlationId: '1234',
      piiLoggingEnabled: true,
    })
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
