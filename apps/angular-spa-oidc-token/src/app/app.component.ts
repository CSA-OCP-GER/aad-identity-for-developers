import { Component, OnInit } from '@angular/core';
import { MsalService } from '@azure/msal-angular';
import {BroadcastService} from "@azure/msal-angular";
import { CommonModule } from '@angular/common';  

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit
{
  title = 'angular-spa-oidc-token';
  loggedIn: boolean = false;
  username: string = "";

  constructor (private broadcastService : BroadcastService, private authService: MsalService)
  {

  }

  ngOnInit(): void 
  {
    this.broadcastService.subscribe("msal:loginFailure", (payload) => 
    {
      console.log("login failure " + JSON.stringify(payload));
      this.loggedIn = false;

    });

    this.broadcastService.subscribe("msal:loginSuccess", (payload) => 
    {
      console.log("login success " + JSON.stringify(payload));
      this.loggedIn = true;
    });

    if (!this.authService.getUser())
    {
      this.loggedIn = false;
    }
    else
    {
      this.loggedIn = true;
      this.username = this.authService.getUser().name;
    }
  }

  onSignin()
  {
    this.authService.loginRedirect();
  }

  onSignout()
  {
    this.authService.logout();
  }
}
