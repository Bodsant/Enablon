import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IncidentService, Incident, CapaAction } from './incident.service';

@Component({
  selector: 'app-incident-page',
  standalone: true,
  imports: [CommonModule],
  providers: [IncidentService],
  template: `
    <section class="incident-page">
      <p class="eyebrow">INCIDENT &amp; CAPA</p>
      <h1>Incident management</h1>
      <p>Report incidents, run investigations, and track corrective actions.</p>

      <h2>Incidents</h2>
      <ul class="list">
        @for (i of incidents(); track i.id) {
          <li>
            <strong>{{ i.recordNumber }}</strong> — {{ i.description }}
            <span class="badge">{{ i.classificationStatus }}</span>
          </li>
        } @empty {
          <li class="muted">No incidents loaded.</li>
        }
      </ul>

      <h2>CAPA actions</h2>
      <ul class="list">
        @for (a of actions(); track a.id) {
          <li>
            <strong>{{ a.actionType }}</strong> — {{ a.description }}
            <span class="badge">{{ a.priority }}</span>
          </li>
        } @empty {
          <li class="muted">No CAPA actions loaded.</li>
        }
      </ul>
    </section>
  `,
})
export class IncidentPageComponent implements OnInit {
  private readonly svc = inject(IncidentService);
  readonly incidents = signal<Incident[]>([]);
  readonly actions = signal<CapaAction[]>([]);

  ngOnInit(): void {
    this.svc.listIncidents().subscribe((x) => this.incidents.set(x));
    this.svc.listCapaActions().subscribe((x) => this.actions.set(x));
  }
}
