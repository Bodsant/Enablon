import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AssetEmergencyService, Asset, EmergencyPlan } from './asset-emergency.service';

@Component({
  selector: 'app-asset-emergency-page',
  standalone: true,
  imports: [CommonModule],
  providers: [AssetEmergencyService],
  template: `
    <section class="asset-emergency-page">
      <p class="eyebrow">ASSETS &amp; EMERGENCY</p>
      <h1>Asset &amp; emergency management</h1>
      <p>Asset inventory and emergency response plans for operational readiness.</p>

      <h2>Assets <span class="count">{{ assets().length }}</span></h2>
      <ul class="list">
        @for (a of assets(); track a.id) {
          <li>
            <strong>{{ a.name }}</strong> — {{ a.assetCode }}
            <span class="badge">{{ a.status }}</span>
            <span class="muted">{{ a.location }}</span>
          </li>
        } @empty {
          <li class="muted">No assets loaded.</li>
        }
      </ul>

      <h2>Emergency plans</h2>
      <ul class="list">
        @for (p of plans(); track p.id) {
          <li>
            <strong>{{ p.title }}</strong> → {{ p.planType }}
            <span class="badge">{{ p.status }}</span>
            <span class="muted">{{ p.version }}</span>
          </li>
        } @empty {
          <li class="muted">No emergency plans loaded.</li>
        }
      </ul>
    </section>
  `,
})
export class AssetEmergencyPageComponent implements OnInit {
  private readonly assetEmergency = inject(AssetEmergencyService);
  readonly assets = signal<Asset[]>([]);
  readonly plans = signal<EmergencyPlan[]>([]);

  ngOnInit(): void {
    this.assetEmergency.listAssets().subscribe((a) => this.assets.set(a));
    this.assetEmergency.listEmergencyPlans().subscribe((p) => this.plans.set(p));
  }
}
