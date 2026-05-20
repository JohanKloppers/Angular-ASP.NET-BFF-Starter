import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface DashboardStats {
    totalUsers: number;
    registeredToday: number;
}

@Injectable({ providedIn: 'root' })
export class DashboardService {
    constructor(private http: HttpClient) {}

    getStats(): Observable<DashboardStats> {
        return this.http.get<DashboardStats>('/api/dashboard/stats');
    }
}
