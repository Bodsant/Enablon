import { Component, inject, OnInit } from '@angular/core';
import { DatePipe } from '@angular/common';
import { ApiService } from '../../core/api.service';

interface TaskItem {
  id: string;
  title: string;
  status: string;
  priority: string;
  dueAt: string | null;
  workflowCode?: string;
  recordId?: string;
}

@Component({
  selector: 'app-tasks-page',
  standalone: true,
  imports: [DatePipe],
  template: `
    <h1>My Tasks</h1>
    <p class="eyebrow">Open workflow tasks assigned to you</p>
    @if (rows.length > 0) {
      <table>
        <thead>
          <tr><th>Title</th><th>Status</th><th>Priority</th><th>Due</th></tr>
        </thead>
        <tbody>
          @for (t of rows; track t.id) {
            <tr>
              <td>{{ t.title }}</td>
              <td><span class="badge">{{ t.status }}</span></td>
              <td><span class="prio" [class.critical]="t.priority === 'Critical'">{{ t.priority }}</span></td>
              <td>{{ t.dueAt ? (t.dueAt | date:'short') : '—' }}</td>
            </tr>
          }
        </tbody>
      </table>
    } @else {
      <p class="muted">No open tasks.</p>
    }
    @if (error) {
      <p class="error">{{ error }}</p>
    }
  `,
  styles: [`
    h1 { margin: 0 0 4px; }
    table { width: 100%; border-collapse: collapse; background: white; border-radius: 10px; overflow: hidden; box-shadow: 0 1px 2px rgba(0,0,0,.06); margin-top: 16px; }
    th, td { text-align: left; padding: 10px 12px; font-size: 13px; border-bottom: 1px solid #e2e8f0; }
    th { background: #f8fafc; color: #64748b; font-weight: 600; }
    .badge { background: #dbeafe; color: #1d4ed8; border-radius: 999px; padding: 2px 8px; font-size: 11px; }
    .prio.critical { color: #dc2626; font-weight: 700; }
    .muted { color: #64748b; }
    .error { color: #dc2626; }
  `],
})
export class TasksPageComponent implements OnInit {
  private readonly api = inject(ApiService);

  rows: TaskItem[] = [];
  error = '';

  async ngOnInit(): Promise<void> {
    try {
      const res = await this.api.get<{ tasks: TaskItem[] }>('/workflow/my-tasks');
      this.rows = res.tasks ?? [];
    } catch (e) {
      this.error = e instanceof Error ? e.message : 'Failed to load tasks';
    }
  }
}