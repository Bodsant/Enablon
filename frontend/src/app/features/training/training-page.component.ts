import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TrainingService, TrainingSession, TrainingParticipant } from './training.service';

@Component({
  selector: 'app-training-page',
  standalone: true,
  imports: [CommonModule],
  providers: [TrainingService],
  template: `
    <section class="training-page">
      <p class="eyebrow">LEARNING &amp; DEVELOPMENT</p>
      <h1>Training management</h1>
      <p>Training sessions and participant records for workforce competency.</p>

      <h2>Sessions <span class="count">{{ sessions().length }}</span></h2>
      <ul class="list">
        @for (t of sessions(); track t.id) {
          <li>
            <strong>{{ t.title }}</strong> — {{ t.description }}
            <span class="badge">{{ t.status }}</span>
            <span class="muted">{{ t.location }}</span>
          </li>
        } @empty {
          <li class="muted">No sessions loaded.</li>
        }
      </ul>

      <h2>Participants</h2>
      <ul class="list">
        @for (p of participants(); track p.id) {
          <li>
            <strong>{{ p.memberName }}</strong> → {{ p.trainingSessionId }}
            <span class="badge">{{ p.attended }}</span>
            <span class="muted">{{ p.score }}</span>
          </li>
        } @empty {
          <li class="muted">No participants loaded.</li>
        }
      </ul>
    </section>
  `,
})
export class TrainingPageComponent implements OnInit {
  private readonly training = inject(TrainingService);
  readonly sessions = signal<TrainingSession[]>([]);
  readonly participants = signal<TrainingParticipant[]>([]);

  ngOnInit(): void {
    this.training.listSessions().subscribe((s) => this.sessions.set(s));
    this.training.listParticipants('1').subscribe((p) => this.participants.set(p));
  }
}
