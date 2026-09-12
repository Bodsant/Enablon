import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DataClassificationService, DataClassificationItem } from './data-classification.service';

@Component({
  selector: 'app-data-classification-page',
  standalone: true,
  imports: [CommonModule],
  providers: [DataClassificationService],
  template: `
    <section class="data-classification-page">
      <p class="eyebrow">PLATFORM SECURITY</p>
      <h1>Data classification</h1>
      <p>Classification levels applied to records and fields.</p>
      <h2>Classifications <span class="count">{{ items().length }}</span></h2>
      <ul class="list">
        @for (i of items(); track i.id) {
          <li><strong>{{ i.code }}</strong> — {{ i.name }} (rank {{ i.rank }})
            @if (i.isRestricted) { <span class="badge">restricted</span> }</li>
        } @empty {
          <li class="muted">No classifications loaded.</li>
        }
      </ul>
    </section>
  `,
})
export class DataClassificationPageComponent implements OnInit {
  private readonly svc = inject(DataClassificationService);
  readonly items = signal<DataClassificationItem[]>([]);

  ngOnInit(): void {
    this.svc.listItems().subscribe((x) => this.items.set(x));
  }
}