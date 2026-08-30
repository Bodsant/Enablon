import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/auth.service';

@Component({
  selector: 'app-login-page',
  standalone: true,
  imports: [FormsModule],
  template: `
    <div class="login-wrap">
      <form class="login-card" (ngSubmit)="submit()">
        <h1>ENABLON EHSMS</h1>
        <p class="eyebrow">Sign in to continue</p>
        <label>
          Email
          <input type="email" name="email" [(ngModel)]="email" required autocomplete="username" />
        </label>
        <label>
          Password
          <input type="password" name="password" [(ngModel)]="password" required autocomplete="current-password" />
        </label>
        <button type="submit" [disabled]="busy">{{ busy ? 'Signing in…' : 'Sign in' }}</button>
        @if (error) {
          <p class="error">{{ error }}</p>
        }
        <p class="hint">Dev seed: <code>admin@ehsms.local</code> / <code>EhsmsDev!123</code></p>
      </form>
    </div>
  `,
  styles: [`
    .login-wrap { min-height: 100vh; display: grid; place-items: center; background: #0f172a; color: #e2e8f0; }
    .login-card { width: 320px; display: grid; gap: 12px; background: #1e293b; padding: 28px; border-radius: 12px; }
    .login-card h1 { margin: 0; font-size: 22px; }
    .login-card label { display: grid; gap: 4px; font-size: 13px; color: #94a3b8; }
    .login-card input { padding: 8px 10px; border-radius: 6px; border: 1px solid #334155; background: #0f172a; color: #e2e8f0; }
    .login-card button { padding: 10px; border: 0; border-radius: 6px; background: #3b82f6; color: white; font-weight: 600; cursor: pointer; }
    .login-card button:disabled { opacity: .6; cursor: default; }
    .error { color: #f87171; font-size: 13px; margin: 0; }
    .hint { color: #64748b; font-size: 12px; margin: 0; }
  `],
})
export class LoginPageComponent {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  email = 'admin@ehsms.local';
  password = '';
  busy = false;
  error = '';

  async submit(): Promise<void> {
    this.busy = true;
    this.error = '';
    try {
      await this.auth.login(this.email, this.password);
      await this.router.navigateByUrl('/dashboard');
    } catch (e) {
      this.error = e instanceof Error ? e.message : 'Login failed';
    } finally {
      this.busy = false;
    }
  }
}