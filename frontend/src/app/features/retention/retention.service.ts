import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface RetentionPolicyItem {
  id: string;
  name: string;
  moduleCode: string;
  recordType: string;
  retentionDays: number;
  status: string;
}

export interface PurgeCandidateItem {
  id: string;
  moduleCode: string;
  recordType: string;
  title: string;
  scheduledPurgeDate: string;
}

@Injectable({ providedIn: 'root' })
export class RetentionService {
  private readonly http = inject(HttpClient);
  listPolicies(): Observable<RetentionPolicyItem[]> {
    return this.http.get<RetentionPolicyItem[]>('/api/v1/retention-policies');
  }
  listPurgeCandidates(): Observable<PurgeCandidateItem[]> {
    return this.http.get<PurgeCandidateItem[]>('/api/v1/retention-policies/purge-candidates');
  }
}