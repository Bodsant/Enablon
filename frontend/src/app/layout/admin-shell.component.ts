import { Component, inject } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from '../core/auth.service';

@Component({
  selector: 'app-admin-shell',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  template: `
    <div class="admin">
      <aside class="sidebar">
        <div class="brand">ENABLON <span>EHSMS</span></div>
        <nav>
          <a routerLink="/dashboard" routerLinkActive="active">Dashboard</a>
          <a routerLink="/records" routerLinkActive="active">Records</a>
          <a routerLink="/tasks" routerLinkActive="active">My Tasks</a>
          <a routerLink="/architecture" routerLinkActive="active">Architecture</a>
        </nav>
        <div class="foot">
          <span class="who">{{ email }}</span>
          <button (click)="logout()">Sign out</button>
        </div>
      </aside>
      <main class="content"><router-outlet /></main>
    </div>
  `,
  styles: [`
    .admin { display: grid; grid-template-columns: 220px 1fr; min-height: 100vh; }
    .sidebar { background: #0f172a; color: #e2e8f0; display: flex; flex-direction: column; padding: 16px 12px; }
    .brand { font-weight: 800; letter-spacing: .5px; padding: 8px 12px 20px; }
    .brand span { color: #3b82f6; }
    nav { display: grid; gap: 2px; }
    nav a { color: #94a3b8; text-decoration: none; padding: 8px 12px; border-radius: 6px; font-size: 14px; }
    nav a:hover { background: #1e293b; color: #e2e8f0; }
    nav a.active { background: #1e293b; color: white; font-weight: 600; }
    .foot { margin-top: auto; display: grid; gap: 8px; padding: 8px 12px; }
    .who { font-size: 12px; color: #64748b; overflow: hidden; text-overflow: ellipsis; }
    .foot button { background: #475569; color: white; border: 0; border-radius: 6px; padding: 6px 10px; cursor: pointer; }
    .content { background: #f1f5f9; padding: 24px; }
  `],
})
export class AdminShellComponent {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  get email(): string {
    return this.auth.user()?.email ?? '';
  }

  logout(): void {
    this.auth.logout();
    void this.router.navigateByUrl('/login');
  }
}