import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SecurityHeadersService, SecurityInfo } from './security-headers.service';

@Component({
  selector: 'app-security-headers-page',
  standalone: true,
  imports: [CommonModule],
  providers: [SecurityHeadersService],
  template: `
    <section class="security-headers-page">
      <p class="eyebrow">PRODUCTION READINESS</p>
      <h1>Security headers</h1>
      <p>Headers applied by the API middleware to every response.</p>

      <h2>Applied headers</h2>
      <ul class="list">
        <li><strong>X-Content-Type-Options</strong> — <span class="badge">nosniff</span></li>
        <li><strong>X-Frame-Options</strong> — <span class="badge">DENY</span></li>
        <li><strong>Referrer-Policy</strong> — <span class="badge">no-referrer</span></li>
        <li><strong>Content-Security-Policy</strong> — <span class="badge">default-src 'none'</span></li>
      </ul>

      <h2>Application info</h2>
      <ul class="list">
        @if (info(); as i) {
          <li><strong>{{ i.application }}</strong> v{{ i.version }} <span class="badge">{{ i.environment }}</span></li>
        } @else {
          <li class="muted">No application info loaded.</li>
        }
      </ul>
    </section>
  `,
})
export class SecurityHeadersPageComponent implements OnInit {
  private readonly svc = inject(SecurityHeadersService);
  readonly info = signal<SecurityInfo | null>(null);

  ngOnInit(): void {
    this.svc.getInfo().subscribe((x) => this.info.set(x));
  }
}