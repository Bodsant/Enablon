import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface TrainingSession {
  id: string;
  title: string;
  description: string;
  sessionDate: string;
  location: string;
  instructorName: string;
  status: string;
}

export interface TrainingParticipant {
  id: string;
  trainingSessionId: string;
  memberName: string;
  attended: boolean;
  score: number;
}

@Injectable({ providedIn: 'root' })
export class TrainingService {
  private readonly http = inject(HttpClient);

  listSessions(): Observable<TrainingSession[]> {
    return this.http.get<TrainingSession[]>('/api/v1/training-sessions');
  }

  listParticipants(sessionId: string): Observable<TrainingParticipant[]> {
    return this.http.get<TrainingParticipant[]>(`/api/v1/training-sessions/${sessionId}/participants`);
  }
}
