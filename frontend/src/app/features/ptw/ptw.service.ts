import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface WorkRequest {
  id: string;
  recordNumber: string;
  workDescription: string;
  workType: string;
}

export interface Permit {
  id: string;
  recordNumber: string;
  validFrom?: string | null;
  validUntil?: string | null;
}

export interface IsolationPlan {
  id: string;
  recordNumber: string;
  status: string;
}

@Injectable({ providedIn: 'root' })
export class PtwService {
  private readonly http = inject(HttpClient);

  listWorkRequests(): Observable<WorkRequest[]> {
    return this.http.get<WorkRequest[]>('/api/v1/work-requests');
  }

  listPermits(): Observable<Permit[]> {
    return this.http.get<Permit[]>('/api/v1/permits');
  }

  listIsolationPlans(): Observable<IsolationPlan[]> {
    return this.http.get<IsolationPlan[]>('/api/v1/isolation-plans');
  }
}
