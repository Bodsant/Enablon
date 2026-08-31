import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LegalService, LegalSource, LegalObligation } from './legal.service';

@Component({
  selector: 'app-legal-page',
  standalone: true,
  imports: [CommonModule],
  providers: [LegalService],
  template: `
    <section class="legal-page">
      <p class="eyebrow">LEGAL &amp; COMPLIANCE</p>
      <h1>Legal management</h1>
      <p>Legal sources and compliance obligations for regulatory adherence.</p>

      <h2>Legal sources <span class="count">{{ sources().length }}</span></h2>
      <ul class="list">
        @for (s of sources(); track s.id) {
          <li>
            <strong>{{ s.title }}</strong> — {{ s.jurisdiction }}
            <span class="badge">{{ s.status }}</span>
            <span class="muted">{{ s.sourceType }}</span>
          </li>
        } @empty {
          <li class="muted">No sources loaded.</li>
        }
      </ul>

      <h2>Obligations</h2>
      <ul class="list">
        @for (o of obligations(); track o.id) {
          <li>
            <strong>{{ o.title }}</strong> → {{ o.description }}
            <span class="badge">{{ o.complianceStatus }}</span>
            <span class="muted">{{ o.dueDate }}</span>
          </li>
        } @empty {
          <li class="muted">No obligations loaded.</li>
        }
      </ul>
    </section>
  `,
})
export class LegalPageComponent implements OnInit {
  private readonly legal = inject(LegalService);
  readonly sources = signal<LegalSource[]>([]);
  readonly obligations = signal<LegalObligation[]>([]);

  ngOnInit(): void {
    this.legal.listSources().subscribe((s) => this.sources.set(s));
    this.legal.listObligations().subscribe((o) => this.obligations.set(o));
  }
}
