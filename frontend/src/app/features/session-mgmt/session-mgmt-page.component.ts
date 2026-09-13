import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SessionMgmtService, SessionItem } from './session-mgmt.service';

@Component({
  selector: 'app-session-mgmt-page',
  standalone: true,
  imports: [CommonModule],
  providers: [SessionMgmtService],
  template: `
    <section class="session-mgmt-page">
      <p class="eyebrow">IDENTITY &amp; ACCESS</p>
      <h1>Active sessions</h1>
      <p>Live sign-in sessions across devices and browsers.</p>
      <h2>Sessions <span class="count">{{ items().length }}</span></h2>
      <ul class="list">
        @for (s of items(); track s.id) {
          <li><strong>{{ s.userEmail }}</strong> <span class="badge">{{ s.isCurrent ? 'current' : 'other' }}</span><br />
            <span class="muted">{{ s.ipAddress }} · {{ s.userAgent }} · created {{ s.createdAt }}</span></li>
        } @empty {
          <li class="muted">No sessions loaded.</li>
        }
      </ul>
    </section>
  `,
})
export class SessionMgmtPageComponent implements OnInit {
  private readonly svc = inject(SessionMgmtService);
  readonly items = signal<SessionItem[]>([]);

  ngOnInit(): void {
    this.svc.listItems().subscribe((x) => this.items.set(x));
  }
}