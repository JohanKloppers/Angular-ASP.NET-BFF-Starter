import { Injectable } from '@angular/core';
import { CanActivate, Router } from '@angular/router';
import { Observable, combineLatest } from 'rxjs';
import { filter, map, take } from 'rxjs/operators';
import { AuthService } from './auth.service';

@Injectable({
    providedIn: 'root'
})
export class AuthGuard implements CanActivate {
    constructor(private authService: AuthService, private router: Router) { }

    canActivate(): Observable<boolean> {
        return combineLatest([
            this.authService.isAuthenticated$,
            this.authService.isLoading$
        ]).pipe(
            filter(([_, loading]) => !loading),
            take(1),
            map(([isAuthenticated]) => {
                if (isAuthenticated) return true;
                this.router.navigate(['/login']);
                return false;
            })
        );
    }
}
