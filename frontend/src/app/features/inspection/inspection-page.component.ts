import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { InspectionService, Audit, Inspection } from './inspection.service';

@Component({
  selector: 'app-inspection-page',
  standalone: true,
  imports: [CommonModule],
  providers: [InspectionService],
  template: `
    <section class="inspection-page">
      <p class="eyebrow">INSPECTION &amp; AUDIT</p>
      <h1>Inspection &amp; audit</h1>
      <p>Track audits, inspections, and compliance outcomes.</p>

      <h2>Audits</h2>
      <ul class="list">
        @for (a of audits(); track a.id) {
          <li>
            <strong>{{ a.recordNumber }}</strong> · {{ a.auditType }} — {{ a.scopeText }}
          </li>
        } @empty {
          <li class="muted">No audits loaded.</li>
        }
      </ul>

      <h2>Inspections</h2>
      <ul class="list">
        @for (i of inspections(); track i.id) {
          <li>
            <strong>{{ i.recordNumber }}</strong>
            @if (i.compliancePercentage !== null && i.compliancePercentage !== undefined) {
              <span class="badge">{{ i.compliancePercentage }}% compliant</span>
            }
          </li>
        } @empty {
          <li class="muted">No inspections loaded.</li>
        }
      </ul>
    </section>
  `,
})
export class InspectionPageComponent implements OnInit {
  private readonly svc = inject(InspectionService);
  readonly audits = signal<Audit[]>([]);
  readonly inspections = signal<Inspection[]>([]);

  ngOnInit(): void {
    this.svc.listAudits().subscribe((x) => this.audits.set(x));
    this.svc.listInspections().subscribe((x) => this.inspections.set(x));
  }
}
