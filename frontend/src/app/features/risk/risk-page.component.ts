import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RiskService, RiskHazard, RiskRegister } from './risk.service';

@Component({
  selector: 'app-risk-page',
  standalone: true,
  imports: [CommonModule],
  providers: [RiskService],
  template: `
    <section class="risk-page">
      <p class="eyebrow">HAZARD &amp; RISK</p>
      <h1>Risk management</h1>
      <p>Hazard registers and risk assessments for safe operations.</p>

      <h2>Hazards <span class="count">{{ hazards().length }}</span></h2>
      <ul class="list">
        @for (h of hazards(); track h.id) {
          <li>
            <strong>{{ h.code }}</strong> — {{ h.name }}
            <span class="muted">{{ h.description }}</span>
          </li>
        } @empty {
          <li class="muted">No hazards loaded.</li>
        }
      </ul>

      <h2>Risk registers</h2>
      <ul class="list">
        @for (r of registers(); track r.id) {
          <li>
            <strong>{{ r.activityName }}</strong> → {{ r.riskEvent }}
            <span class="badge">{{ r.status }}</span>
          </li>
        } @empty {
          <li class="muted">No registers loaded.</li>
        }
      </ul>
    </section>
  `,
})
export class RiskPageComponent implements OnInit {
  private readonly risk = inject(RiskService);
  readonly hazards = signal<RiskHazard[]>([]);
  readonly registers = signal<RiskRegister[]>([]);

  ngOnInit(): void {
    this.risk.listHazards().subscribe((h) => this.hazards.set(h));
    this.risk.listRegisters().subscribe((r) => this.registers.set(r));
  }
}
