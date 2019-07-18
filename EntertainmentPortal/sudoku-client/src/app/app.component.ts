import { Router } from '@angular/router';
import { Component } from '@angular/core';
import { OAuthService, JwksValidationHandler } from 'angular-oauth2-oidc';
import { AuthConfig } from 'angular-oauth2-oidc';

export const authConfig: AuthConfig = {

  // Url of the Identity Provider
  issuer: 'http://localhost:5000',

  // URL of the SPA to redirect the user to after login
  redirectUri: window.location.origin + '/home',

  // The SPA's id. The SPA is registerd with this id at the auth-server
  clientId: 'spa',

  // set the scope for the permissions the client should request
  // The first three are defined by OIDC. The 4th is a usecase-specific one
  scope: 'openid profile sudoku_api',
}

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent {
  title = 'sudoku-client';
  userName: string = this.getValueFromIdToken('sub');

  constructor(private authService: OAuthService, private router: Router) {
     this.authService.configure(authConfig);
     this.authService.tokenValidationHandler = new JwksValidationHandler();
     this.authService.loadDiscoveryDocumentAndTryLogin();

  }

  login() {
    this.authService.initImplicitFlow();
  }

  logout() {
    // true - redirect user after logout
    console.log(sessionStorage);
    this.authService.logOut();
    console.log(sessionStorage);
    this.router.navigateByUrl('/');

  }

  getValueFromIdToken(claim: string) {
    const jwt = sessionStorage.getItem('id_token');
    if ( jwt == null) {
      return null;
    }

    const jwtData = jwt.split('.')[1];
    const decodedJwtJsonData = window.atob(jwtData);
    let value: any;
    JSON.parse(decodedJwtJsonData, function findKey(k, v) {
      if (k === claim) {
        value = v;
      }
    });
    console.log(value);
    return value;
  }
}
