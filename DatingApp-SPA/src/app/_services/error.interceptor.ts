import { Injectable } from '@angular/core';
import {
  HttpInterceptor,
  HttpEvent,
  HttpHandler,
  HttpRequest,
  HttpErrorResponse,
  HTTP_INTERCEPTORS,
} from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';

@Injectable()
export class ErrorInterceptor implements HttpInterceptor {
  intercept(
    req: HttpRequest<any>,
    next: HttpHandler
  ): Observable<HttpEvent<any>> {
    return next.handle(req).pipe(
      catchError((response) => {
        if (response.status === 401) {
          return throwError(response.statusText);
        }
        if (response instanceof HttpErrorResponse) {
          const applicationError = response.headers.get('Application-Error');
          if (applicationError) {
            return throwError(applicationError);
          }

          const serverError = response.error;
          let modelStateErrors = '';
          if (serverError.errors && typeof serverError.errors === 'object') {
            for (const key in serverError.errors) {
              if (serverError.errors[key]) {
                modelStateErrors += serverError.errors[key] + '\n';
              }
            }
          }

          return throwError(modelStateErrors || serverError || 'Unknown Server Error');
        }
      })
    );
  }
}

export const ErrorInterceptorProvider = {
    provide: HTTP_INTERCEPTORS,
    useClass: ErrorInterceptor,
    multi: true
}
