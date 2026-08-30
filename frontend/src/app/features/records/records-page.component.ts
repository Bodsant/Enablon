import { Component, inject, OnInit } from '@angular/core';
import { DatePipe } from '@angular/common';
import { ApiService } from '../../core/api.service';

interface RecordItem {
  id: string;
  recordNumber: string;
  moduleCode: string;
  recordType: string;
  title: string | null;
  status: string;
  createdAt: string;
}

@Component({
  selector: 'app-records-page',
  standalone: true,
  imports: [DatePipe],
  template: `
    <h1>Records</h1>
    <p class="eyebrow">Latest tenant records</p>
    @if (rows.length > 0) {
      <table>
        <thead>
          <tr><th>Number</th><th>Module</th><th>Type</th><th>Title</th><th>Status</th><th>Created</th></tr>
        </thead>
        <tbody>
          @for (r of rows; track r.id) {
            <tr>
              <td>{{ r.recordNumber }}</td>
              <td>{{ r.moduleCode }}</td>
              <td>{{ r.recordType }}</td>
              <td>{{ r.title ?? '—' }}</td>
              <td><span class="badge">{{ r.status }}</span></td>
              <td>{{ r.createdAt | date:'short' }}</td>
            </tr>
          }
        </tbody>
      </table>
    } @else {
      <p class="muted">No records yet.</p>
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
    .muted { color: #64748b; }
    .error { color: #dc2626; }
  `],
})
export class RecordsPageComponent implements OnInit {
  private readonly api = inject(ApiService);

  rows: RecordItem[] = [];
  error = '';

  async ngOnInit(): Promise<void> {
    try {
      this.rows = await this.api.get<RecordItem[]>('/platform/records');
    } catch (e) {
      this.error = e instanceof Error ? e.message : 'Failed to load records';
    }
  }
}