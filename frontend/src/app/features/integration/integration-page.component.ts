import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IntegrationService, IntegrationInterface, IntegrationRun } from './integration.service';

@Component({
  selector: 'app-integration-page',
  standalone: true,
  imports: [CommonModule],
  providers: [IntegrationService],
  template: `
    <section class="integration-page">
      <p class="eyebrow">SYSTEM INTEGRATION</p>
      <h1>Integration management</h1>
      <p>Interface definitions and execution runs for system interoperability.</p>

      <h2>Interfaces <span class="count">{{ interfaces().length }}</span></h2>
      <ul class="list">
        @for (i of interfaces(); track i.id) {
          <li>
            <strong>{{ i.name }}</strong> — {{ i.interfaceType }}
            <span class="badge">{{ i.status }}</span>
            <span class="muted">{{ i.protocol }}</span>
          </li>
        } @empty {
          <li class="muted">No interfaces loaded.</li>
        }
      </ul>

      <h2>Runs</h2>
      <ul class="list">
        @for (r of runs(); track r.id) {
          <li>
            <strong>{{ r.interfaceId }}</strong> → {{ r.direction }}
            <span class="badge">{{ r.status }}</span>
            <span class="muted">{{ r.messageCount }}</span>
          </li>
        } @empty {
          <li class="muted">No runs loaded.</li>
        }
      </ul>
    </section>
  `,
})
export class IntegrationPageComponent implements OnInit {
  private readonly integration = inject(IntegrationService);
  readonly interfaces = signal<IntegrationInterface[]>([]);
  readonly runs = signal<IntegrationRun[]>([]);

  ngOnInit(): void {
    this.integration.listInterfaces().subscribe((i) => this.interfaces.set(i));
    this.integration.listRuns().subscribe((r) => this.runs.set(r));
  }
}
