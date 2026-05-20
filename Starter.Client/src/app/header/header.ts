import { Component, Input, Output, EventEmitter, HostListener, inject, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { RouterModule, Router } from '@angular/router';
import { AuthService } from '../auth.service';

@Component({
  selector: 'app-header',
  imports: [RouterModule],
  templateUrl: './header.html',
  styleUrl: './header.scss',
})
export class Header {
  @Input() sidebarOpen = false;
  @Output() toggleSidebarEvent = new EventEmitter<void>();

  private authService = inject(AuthService);
  private router = inject(Router);

  isAuthenticated = toSignal(this.authService.isAuthenticated$, { initialValue: false });
  isUserMenuOpen = signal(false);

  toggleSidebar() {
    this.toggleSidebarEvent.emit();
  }

  toggleUserMenu() {
    this.isUserMenuOpen.update(v => !v);
  }

  signIn() {
    this.router.navigate(['/login']);
  }

  signOut() {
    this.authService.logout().subscribe({
      next: () => this.router.navigate(['/login']),
      error: () => this.router.navigate(['/login']),
    });
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: Event) {
    const target = event.target as HTMLElement;
    if (!target.closest('.user-menu-container')) {
      this.isUserMenuOpen.set(false);
    }
  }
}
