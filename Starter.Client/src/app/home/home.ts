import { Component, inject, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { catchError, EMPTY } from 'rxjs';
import { Header } from '../header/header';
import { Sidebar } from '../sidebar/sidebar';
import { AuthService } from '../auth.service';
import { DashboardService } from '../dashboard/dashboard.service';

@Component({
  selector: 'app-home',
  imports: [Header, Sidebar],
  templateUrl: './home.html',
  styleUrl: './home.scss',
})
export class Home {
  private authService = inject(AuthService);

  sidebarOpen = signal(false);

  user = toSignal(this.authService.getUser());

  private readonly _statsError = signal(false);
  readonly statsError = this._statsError.asReadonly();

  stats = toSignal(
    inject(DashboardService).getStats().pipe(
      catchError(() => {
        this._statsError.set(true);
        return EMPTY;
      })
    )
  );

  getClaim(type: string): string {
    const u = this.user();
    return u ? this.authService.getClaim(u, type) : '';
  }

  toggleSidebar() {
    this.sidebarOpen.update(v => !v);
  }
}
