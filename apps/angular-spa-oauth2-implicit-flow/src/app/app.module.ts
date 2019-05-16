import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';

import { MsalModule } from '@azure/msal-angular';
import { CommonModule } from '@angular/common';
import { HttpClientModule } from '@angular/common/http';

import { GraphService } from "./services/graph.service";

@NgModule({
  declarations: [
    AppComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    CommonModule,
    HttpClientModule,
    MsalModule.forRoot({
      clientID: "<ApplicationId>",
      authority: "https://login.microsoftonline.com/<TenantId>/",
      redirectUri: "http://localhost:5003",
      validateAuthority : true,
      cacheLocation : "localStorage",
      postLogoutRedirectUri: "http://localhost:5003/",
      navigateToLoginRequestUrl : true,
      popUp: false,
      consentScopes: [ "user.read" ],
      unprotectedResources: ["https://angularjs.org/"],
      correlationId: '1234',
      piiLoggingEnabled: true
    })
  ],
  providers: [GraphService],
  bootstrap: [AppComponent]
})
export class AppModule { }
