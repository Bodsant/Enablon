import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AuditTrailService, AuditLogEntry } from './audit-trail.service';

@Component({
  selector: 'app-audit-trail-page',
  standalone: true,
  imports: [CommonModule],
  providers: [AuditTrailService],
  template: `
    <section class="audit-trail-page">
      <p class="eyebrow">PLATFORM SECURITY</p>
      <h1>Audit trail</h1>
      <p>Immutable log of who did what, when, and from where.</p>
      <h2>Events <span class="count">{{ items().length }}</span></h2>
      <ul class="list">
        @for (i of items(); track i.id) {
          <li><strong>{{ i.actionCode }}</strong> — {{ i.recordTitle }} <span class="badge">{{ i.moduleCode }}</span><br />
            <span class="muted">{{ i.occurredAt }} by {{ i.tenantMemberName }} ({{ i.ipAddress }})</span></li>
        } @empty {
          <li class="muted">No audit events loaded.</li>
        }
      </ul>
    </section>
  `,
})
export class AuditTrailPageComponent implements OnInit {
  private readonly svc = inject(AuditTrailService);
  readonly items = signal<AuditLogEntry[]>([]);

  ngOnInit(): void {
    this.svc.listItems().subscribe((x) => this.items.set(x));
  }
}