import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RetentionService, RetentionPolicyItem, PurgeCandidateItem } from './retention.service';

@Component({
  selector: 'app-retention-page',
  standalone: true,
  imports: [CommonModule],
  providers: [RetentionService],
  template: `
    <section class="retention-page">
      <p class="eyebrow">PLATFORM SECURITY</p>
      <h1>Retention policies</h1>
      <p>How long records are kept, and what is queued for purge.</p>

      <h2>Policies <span class="count">{{ policies().length }}</span></h2>
      <ul class="list">
        @for (p of policies(); track p.id) {
          <li><strong>{{ p.name }}</strong> — {{ p.moduleCode }} / {{ p.recordType }}
            <span class="badge">{{ p.status }}</span> · {{ p.retentionDays }} days</li>
        } @empty {
          <li class="muted">No retention policies loaded.</li>
        }
      </ul>

      <h2>Purge candidates <span class="count">{{ candidates().length }}</span></h2>
      <ul class="list">
        @for (c of candidates(); track c.id) {
          <li><strong>{{ c.title }}</strong> — {{ c.moduleCode }} / {{ c.recordType }}
            <span class="muted">scheduled {{ c.scheduledPurgeDate }}</span></li>
        } @empty {
          <li class="muted">No purge candidates loaded.</li>
        }
      </ul>
    </section>
  `,
})
export class RetentionPageComponent implements OnInit {
  private readonly svc = inject(RetentionService);
  readonly policies = signal<RetentionPolicyItem[]>([]);
  readonly candidates = signal<PurgeCandidateItem[]>([]);

  ngOnInit(): void {
    this.svc.listPolicies().subscribe((x) => this.policies.set(x));
    this.svc.listPurgeCandidates().subscribe((x) => this.candidates.set(x));
  }
}