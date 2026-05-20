import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { tap } from 'rxjs/operators';

export interface UserClaim {
    type: string;
    value: string;
}

export interface UserInfo {
    claims: UserClaim[];
}

@Injectable({ providedIn: 'root' })
export class AuthService {
    private isAuthenticatedSubject = new BehaviorSubject<boolean>(false);
    private isLoadingSubject = new BehaviorSubject<boolean>(true);

    public isAuthenticated$ = this.isAuthenticatedSubject.asObservable();
    public isLoading$ = this.isLoadingSubject.asObservable();

    constructor(private http: HttpClient) {
        this.checkAuthStatus();
    }

    login(email: string, password: string): Observable<unknown> {
        return this.http.post('/login', { email, password }).pipe(
            tap(() => this.isAuthenticatedSubject.next(true))
        );
    }

    register(email: string, password: string, firstName: string, lastName: string): Observable<unknown> {
        return this.http.post('/register', { email, password, firstName, lastName }).pipe(
            tap(() => this.isAuthenticatedSubject.next(true))
        );
    }

    logout(): Observable<unknown> {
        return this.http.get('/logout').pipe(
            tap(() => this.isAuthenticatedSubject.next(false))
        );
    }

    getUser(): Observable<UserInfo> {
        return this.http.get<UserInfo>('/user');
    }

    checkAuthStatus(): void {
        this.isLoadingSubject.next(true);
        this.getUser().subscribe({
            next: () => {
                this.isAuthenticatedSubject.next(true);
                this.isLoadingSubject.next(false);
            },
            error: () => {
                this.isAuthenticatedSubject.next(false);
                this.isLoadingSubject.next(false);
            }
        });
    }

    getClaim(user: UserInfo, type: string): string {
        return user.claims.find(c => c.type === type || c.type.endsWith('/' + type))?.value ?? '';
    }
}
