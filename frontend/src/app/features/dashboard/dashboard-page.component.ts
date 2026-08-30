import { Component, inject, OnInit } from '@angular/core';
import { ApiService } from '../../core/api.service';

@Component({
  selector: 'app-dashboard-page',
  standalone: true,
  template: `
    <h1>Dashboard</h1>
    <p class="eyebrow">Tenant overview</p>
    @if (stats; as s) {
      <div class="cards">
        <div class="card">
          <strong>{{ s.records ?? '—' }}</strong>
          <span>Records</span>
        </div>
        <div class="card">
          <strong>{{ s.openTasks ?? '—' }}</strong>
          <span>Open tasks</span>
        </div>
        <div class="card">
          <strong>{{ s.unreadNotifications ?? '—' }}</strong>
          <span>Unread notifications</span>
        </div>
      </div>
    }
    @if (error) {
      <p class="error">{{ error }}</p>
    }
  `,
  styles: [`
    h1 { margin: 0 0 4px; }
    .cards { display: grid; grid-template-columns: repeat(auto-fit, minmax(160px, 1fr)); gap: 12px; margin-top: 16px; }
    .card { background: white; border-radius: 10px; padding: 20px; box-shadow: 0 1px 2px rgba(0,0,0,.06); display: grid; gap: 2px; }
    .card strong { font-size: 28px; }
    .card span { color: #64748b; font-size: 13px; }
    .error { color: #dc2626; }
  `],
})
export class DashboardPageComponent implements OnInit {
  private readonly api = inject(ApiService);

  stats: { records?: number; openTasks?: number; unreadNotifications?: number } | null = null;
  error = '';

  async ngOnInit(): Promise<void> {
    try {
      const [count, me] = await Promise.all([
        this.api.get<{ count: number }>('/platform/records/count'),
        this.api.get<{ openTasks: number; unreadNotifications: number }>('/workflow/me'),
      ]);
      this.stats = {
        records: count.count,
        openTasks: me.openTasks,
        unreadNotifications: me.unreadNotifications,
      };
    } catch (e) {
      this.error = e instanceof Error ? e.message : 'Failed to load dashboard';
    }
  }
}