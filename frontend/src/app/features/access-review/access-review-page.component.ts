import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AccessReviewService, AccessReview } from './access-review.service';

@Component({
  selector: 'app-access-review-page',
  standalone: true,
  imports: [CommonModule],
  providers: [AccessReviewService],
  template: `
    <section class="access-review-page">
      <p class="eyebrow">IDENTITY &amp; ACCESS</p>
      <h1>Access reviews</h1>
      <p>Periodic recertification of member access to roles.</p>
      <h2>Reviews <span class="count">{{ items().length }}</span></h2>
      <ul class="list">
        @for (i of items(); track i.id) {
          <li><strong>{{ i.memberName }}</strong> → {{ i.roleName }} <span class="badge">{{ i.status }}</span><br />
            <span class="muted">requested {{ i.requestedAt }} · reviewed by {{ i.reviewedByName }}</span></li>
        } @empty {
          <li class="muted">No access reviews loaded.</li>
        }
      </ul>
    </section>
  `,
})
export class AccessReviewPageComponent implements OnInit {
  private readonly svc = inject(AccessReviewService);
  readonly items = signal<AccessReview[]>([]);

  ngOnInit(): void {
    this.svc.listItems().subscribe((x) => this.items.set(x));
  }
}