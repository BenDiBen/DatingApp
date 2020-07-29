import { Injectable } from '@angular/core';
import { User } from '../_models/user';
import {
  Resolve,
  ActivatedRouteSnapshot,
  RouterStateSnapshot,
  Router,
} from '@angular/router';
import { UserService } from '../_services/user.service';
import { AlertifyService } from '../_services/alertify.service';
import { Observable, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { AuthService } from '../_services/auth.service';

@Injectable()
export class MemberEditResolver implements Resolve<User> {
  constructor(
    private userService: UserService,
    private router: Router,
    private alertify: AlertifyService,
    private authService: AuthService
  ) {}
  resolve(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot
  ): User | Observable<User> | Promise<User> {
    return this.userService.getUser(this.authService.currentUser.id).pipe(
      catchError((error) => {
        this.alertify.error('Problem retrieving your data');
        this.router.navigate(['/members']);
        return of(null);
      })
    );
  }
}
