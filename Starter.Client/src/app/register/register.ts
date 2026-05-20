import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../auth.service';

@Component({
  selector: 'app-register',
  imports: [FormsModule, RouterModule],
  templateUrl: './register.html',
  styleUrl: './register.scss',
})
export class Register {
  private authService = inject(AuthService);
  private router = inject(Router);

  firstName = '';
  lastName = '';
  email = '';
  password = '';
  confirmPassword = '';
  isLoading = signal(false);
  error = signal('');

  register() {
    if (this.password !== this.confirmPassword) {
      this.error.set('Passwords do not match.');
      return;
    }

    this.isLoading.set(true);
    this.error.set('');

    this.authService.register(this.email, this.password, this.firstName, this.lastName).subscribe({
      next: () => this.router.navigate(['/dashboard']),
      error: () => {
        this.error.set('Registration failed. The email may already be in use.');
        this.isLoading.set(false);
      }
    });
  }
}
