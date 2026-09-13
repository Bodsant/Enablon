import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface AuditLogEntry {
  id: string;
  actionCode: string;
  occurredAt: string;
  tenantMemberName: string;
  ipAddress: string;
  recordTitle: string;
  moduleCode: string;
}

@Injectable({ providedIn: 'root' })
export class AuditTrailService {
  private readonly http = inject(HttpClient);
  listItems(): Observable<AuditLogEntry[]> {
    return this.http.get<AuditLogEntry[]>('/api/v1/audit-logs');
  }
}