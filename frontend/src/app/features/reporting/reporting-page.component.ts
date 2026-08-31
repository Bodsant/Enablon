import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReportingService, Kpi, KpiVersion } from './reporting.service';

@Component({
  selector: 'app-reporting-page',
  standalone: true,
  imports: [CommonModule],
  providers: [ReportingService],
  template: `
    <section class="reporting-page">
      <p class="eyebrow">REPORTING &amp; KPI</p>
      <h1>Reporting &amp; KPI management</h1>
      <p>Key performance indicators and versioned targets for EHS performance.</p>

      <h2>KPIs <span class="count">{{ kpis().length }}</span></h2>
      <ul class="list">
        @for (k of kpis(); track k.id) {
          <li>
            <strong>{{ k.name }}</strong> — {{ k.code }}
            <span class="badge">{{ k.status }}</span>
            <span class="muted">{{ k.unit }}</span>
          </li>
        } @empty {
          <li class="muted">No KPIs loaded.</li>
        }
      </ul>

      <h2>KPI versions</h2>
      <ul class="list">
        @for (v of versions(); track v.id) {
          <li>
            <strong>{{ v.kpiId }}</strong> → {{ v.period }}
            <span class="badge">{{ v.status }}</span>
            <span class="muted">{{ v.target }}</span>
          </li>
        } @empty {
          <li class="muted">No KPI versions loaded.</li>
        }
      </ul>
    </section>
  `,
})
export class ReportingPageComponent implements OnInit {
  private readonly reporting = inject(ReportingService);
  readonly kpis = signal<Kpi[]>([]);
  readonly versions = signal<KpiVersion[]>([]);

  ngOnInit(): void {
    this.reporting.listKpis().subscribe((k) => this.kpis.set(k));
    this.reporting.listKpiVersions().subscribe((v) => this.versions.set(v));
  }
}
