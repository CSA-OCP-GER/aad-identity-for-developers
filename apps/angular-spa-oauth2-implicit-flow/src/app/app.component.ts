import { Component, OnInit } from '@angular/core';
import { MsalService } from "@azure/msal-angular";
import {BroadcastService} from "@azure/msal-angular";
import {HttpClient} from "@angular/common/http";
import { HttpHeaders } from '@angular/common/http';
import { GraphService } from "./services/graph.service";

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  title = 'angular-spa-oauth2-implicit-flow';
  loggedin: boolean = false;
  username: string = "";
  profile: string = "";

  constructor(
    private broadcastService: BroadcastService, 
    private authService: MsalService, 
    private graphService: GraphService)
  {
  }

  ngOnInit(): void 
  {
    this.broadcastService.subscribe("msal:loginFailure", (payload) => 
    {
      console.log("login failure " + JSON.stringify(payload));
      this.loggedin = false;

    });

    this.broadcastService.subscribe("msal:loginSuccess", (payload) => 
    {
      console.log("login success " + JSON.stringify(payload));
      this.loggedin = true;
    });

    if (!this.authService.getUser())
    {
      this.loggedin = false;
    }
    else
    {
      this.loggedin = true;
      this.username = this.authService.getUser().name;
    }
  }

  public onLogin(): void
  {
    // specify your scopes to consent
    this.authService.loginRedirect(["User.Read"]);
  }

  public onLogout(): void
  {
    this.authService.logout();
  }

  public onGetUserProfile() : void
  {
    this.profile = "Loading..."
    this.graphService.getUserProfile()
      .then(result => result.subscribe(profile => 
        {
          this.profile = profile;
        }))
        .catch(reason => 
        {
          this.profile = reason;  
        });
  }
}
