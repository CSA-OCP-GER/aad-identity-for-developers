import { Injectable } from "@angular/core"
import { HttpClient, HttpErrorResponse, HttpHeaders } from "@angular/common/http"
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { MsalService } from "@azure/msal-angular";

@Injectable()
export class GraphService
{
    private endpoint = "https://graph.microsoft.com/v1.0/";

    constructor(private http: HttpClient, private authService: MsalService)
    {

    }

    public getUserProfile(): Promise<Observable<string>>
    {
        var _this = this;
        return this.authService.acquireTokenSilent(["User.Read"])
            .then((token) => 
            {
                var options = {
                    headers: new HttpHeaders({'Authorization': 'Bearer ' + token})
                };

                return this.http.get(_this.endpoint + "me", options)
                    .map((response: Response) => 
                    {
                        alert(response);
                        return JSON.stringify(response);
                    })
                    .pipe(catchError(_this.handleError));
            });
    }

    private handleError(error: HttpErrorResponse) 
    {
        if (error.error instanceof ErrorEvent) 
        {
          // A client-side or network error occurred. Handle it accordingly.
          console.error('An error occurred:', error.error.message);
        } 
        else 
        {
          // The backend returned an unsuccessful response code.
          // The response body may contain clues as to what went wrong,
          console.error(
            `Backend returned code ${error.status}, ` +
            `body was: ${error.error}`);
        }
        // return an observable with a user-facing error message
        return throwError(
          'Something bad happened; please try again later.');
      };
}