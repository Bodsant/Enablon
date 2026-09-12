import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HealthService, HealthProfile, FitnessStatus } from './health.service';

@Component({
  selector: 'app-health-page',
  standalone: true,
  imports: [CommonModule],
  providers: [HealthService],
  template: `
    <section class="health-page">
      <p class="eyebrow">HEALTH &amp; WELLNESS</p>
      <h1>Health management</h1>
      <p>Health profiles and fitness assessments for personnel.</p>

      <h2>Health profiles <span class="count">{{ profiles().length }}</span></h2>
      <ul class="list">
        @for (p of profiles(); track p.id) {
          <li>
            <strong>{{ p.personName }}</strong> — {{ p.bloodType }}
            <span class="badge">{{ p.status }}</span>
            <span class="muted">{{ p.allergies }}</span>
          </li>
        } @empty {
          <li class="muted">No profiles loaded.</li>
        }
      </ul>

      <h2>Fitness statuses</h2>
      <ul class="list">
        @for (f of fitnessStatuses(); track f.id) {
          <li>
            <strong>{{ f.personName }}</strong> → {{ f.fitnessClass }}
            <span class="badge">{{ f.status }}</span>
            <span class="muted">{{ f.assessmentDate }}</span>
          </li>
        } @empty {
          <li class="muted">No fitness statuses loaded.</li>
        }
      </ul>
    </section>
  `,
})
export class HealthPageComponent implements OnInit {
  private readonly health = inject(HealthService);
  readonly profiles = signal<HealthProfile[]>([]);
  readonly fitnessStatuses = signal<FitnessStatus[]>([]);

  ngOnInit(): void {
    this.health.listProfiles().subscribe((p) => this.profiles.set(p));
    this.health.listFitnessStatuses().subscribe((f) => this.fitnessStatuses.set(f));
  }
}
