import { Component, OnInit } from '@angular/core';
import { Header } from '../header/header';
import { Sidebar } from '../sidebar/sidebar';
import { AuthService, UserInfo } from '../auth.service';
import { DashboardService, DashboardStats } from '../dashboard/dashboard.service';

@Component({
  selector: 'app-home',
  imports: [Header, Sidebar],
  templateUrl: './home.html',
  styleUrl: './home.scss',
})
export class Home implements OnInit {
  sidebarOpen = false;
  user: UserInfo | null = null;
  stats: DashboardStats | null = null;
  statsError = false;

  constructor(
    private authService: AuthService,
    private dashboardService: DashboardService
  ) {}

  ngOnInit() {
    this.authService.getUser().subscribe({ next: u => this.user = u });
    this.dashboardService.getStats().subscribe({
      next: s => this.stats = s,
      error: () => this.statsError = true,
    });
  }

  getClaim(type: string): string {
    return this.user ? this.authService.getClaim(this.user, type) : '';
  }

  toggleSidebar() {
    this.sidebarOpen = !this.sidebarOpen;
  }
}
