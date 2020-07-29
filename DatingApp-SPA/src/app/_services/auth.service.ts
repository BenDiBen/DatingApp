import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { map } from 'rxjs/operators';
import { JwtHelperService } from '@auth0/angular-jwt';
import { environment } from '../../environments/environment';
import { User } from '../_models/user';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  defaultUserPhotoUrl = '../../assest/user.png';
  baseUrl = environment.apiUrl + 'auth/';
  jwtHelper = new JwtHelperService();
  decodedToken: any;
  currentUser: User;
  photoUrl = new BehaviorSubject<string>(this.defaultUserPhotoUrl);
  currentPhotoUrl = this.photoUrl.asObservable();

  constructor(private http: HttpClient) {}

  changeMemberPhoto(photoUrl: string) {
    this.photoUrl.next(photoUrl);
    this.currentUser.photoUrl = photoUrl;
    localStorage.setItem('user', JSON.stringify(this.currentUser));
  }

  login(model: any): Observable<void> {
    return this.http.post(this.baseUrl + 'login', model).pipe(
      map((response: any) => {
        const userResponse = response;
        if (userResponse) {
          localStorage.setItem('token', userResponse.token);
          localStorage.setItem('user', JSON.stringify(userResponse.user));
          this.load();
          console.log(this.decodedToken);
        }
      })
    );
  }

  load(): void {
    const token = localStorage.getItem('token');
    if (token) {
      this.decodedToken = this.jwtHelper.decodeToken(token);
    }
    const user: User = JSON.parse(localStorage.getItem('user'));
    if (user) {
      this.currentUser = user;
    }

    this.changeMemberPhoto(this.currentUser.photoUrl);
  }

  clear(): void {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    this.changeMemberPhoto(this.defaultUserPhotoUrl);
    this.decodedToken = null;
    this.currentUser = null;
  }

  register(model: any): Observable<object> {
    return this.http.post(this.baseUrl + 'register', model);
  }

  loggedIn(): boolean {
    const token = localStorage.getItem('token');
    return !this.jwtHelper.isTokenExpired(token);
  }

  getUserId(): number {
    return this.currentUser.id;
  }
}
