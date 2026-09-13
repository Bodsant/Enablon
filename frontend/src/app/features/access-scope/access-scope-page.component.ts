import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AccessScopeService, AccessScopeItem, TempGrantItem } from './access-scope.service';

@Component({
  selector: 'app-access-scope-page',
  standalone: true,
  imports: [CommonModule],
  providers: [AccessScopeService],
  template: `
    <section class="access-scope-page">
      <p class="eyebrow">IDENTITY &amp; ACCESS</p>
      <h1>Access scopes</h1>
      <p>Where members can act, and temporary overrides in force.</p>

      <h2>Scopes <span class="count">{{ scopes().length }}</span></h2>
      <ul class="list">
        @for (s of scopes(); track s.id) {
          <li><strong>{{ s.memberName }}</strong> — {{ s.scopeType }} <span class="badge">{{ s.status }}</span><br />
            <span class="muted">{{ s.siteName ?? '—' }} / {{ s.companyName ?? '—' }}
            valid {{ s.validFrom ?? 'now' }} → {{ s.validUntil ?? 'open' }}</span></li>
        } @empty {
          <li class="muted">No scopes loaded.</li>
        }
      </ul>

      <h2>Temporary grants <span class="count">{{ tempGrants().length }}</span></h2>
      <ul class="list">
        @for (g of tempGrants(); track g.id) {
          <li><strong>{{ g.memberName }}</strong> → {{ g.roleName }} <span class="badge">{{ g.status }}</span><br />
            <span class="muted">{{ g.validFrom ?? 'now' }} → {{ g.validUntil ?? 'open' }} · {{ g.reason }}</span></li>
        } @empty {
          <li class="muted">No temporary grants loaded.</li>
        }
      </ul>
    </section>
  `,
})
export class AccessScopePageComponent implements OnInit {
  private readonly svc = inject(AccessScopeService);
  readonly scopes = signal<AccessScopeItem[]>([]);
  readonly tempGrants = signal<TempGrantItem[]>([]);

  ngOnInit(): void {
    this.svc.listScopes().subscribe((x) => this.scopes.set(x));
    this.svc.listTempGrants().subscribe((x) => this.tempGrants.set(x));
  }
}