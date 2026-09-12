import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface AccessReview {
  id: string;
  memberName: string;
  roleName: string;
  requestedAt: string;
  status: string;
  reviewedByName: string;
}

@Injectable({ providedIn: 'root' })
export class AccessReviewService {
  private readonly http = inject(HttpClient);
  listItems(): Observable<AccessReview[]> {
    return this.http.get<AccessReview[]>('/api/v1/access-reviews');
  }
}