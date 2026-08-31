import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PtwService, WorkRequest, Permit, IsolationPlan } from './ptw.service';

@Component({
  selector: 'app-ptw-page',
  standalone: true,
  imports: [CommonModule],
  providers: [PtwService],
  template: `
    <section class="ptw-page">
      <p class="eyebrow">PTW / JSA / LOTO</p>
      <h1>Permit to work</h1>
      <p>Work requests, permits, and LOTO isolation plans.</p>

      <h2>Work requests</h2>
      <ul class="list">
        @for (w of workRequests(); track w.id) {
          <li><strong>{{ w.recordNumber }}</strong> — {{ w.workDescription }} <span class="badge">{{ w.workType }}</span></li>
        } @empty {
          <li class="muted">No work requests loaded.</li>
        }
      </ul>

      <h2>Permits</h2>
      <ul class="list">
        @for (p of permits(); track p.id) {
          <li><strong>{{ p.recordNumber }}</strong> — valid {{ p.validFrom ?? 'now' }} → {{ p.validUntil ?? 'open' }}</li>
        } @empty {
          <li class="muted">No permits loaded.</li>
        }
      </ul>

      <h2>LOTO isolation plans</h2>
      <ul class="list">
        @for (i of plans(); track i.id) {
          <li><strong>{{ i.recordNumber }}</strong> <span class="badge">{{ i.status }}</span></li>
        } @empty {
          <li class="muted">No isolation plans loaded.</li>
        }
      </ul>
    </section>
  `,
})
export class PtwPageComponent implements OnInit {
  private readonly svc = inject(PtwService);
  readonly workRequests = signal<WorkRequest[]>([]);
  readonly permits = signal<Permit[]>([]);
  readonly plans = signal<IsolationPlan[]>([]);

  ngOnInit(): void {
    this.svc.listWorkRequests().subscribe((x) => this.workRequests.set(x));
    this.svc.listPermits().subscribe((x) => this.permits.set(x));
    this.svc.listIsolationPlans().subscribe((x) => this.plans.set(x));
  }
}
