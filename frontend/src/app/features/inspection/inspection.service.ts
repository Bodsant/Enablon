import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Audit {
  id: string;
  recordNumber: string;
  auditType: string;
  scopeText: string;
}

export interface Inspection {
  id: string;
  recordNumber: string;
  compliancePercentage?: number | null;
}

@Injectable({ providedIn: 'root' })
export class InspectionService {
  private readonly http = inject(HttpClient);

  listAudits(): Observable<Audit[]> {
    return this.http.get<Audit[]>('/api/v1/audits');
  }

  listInspections(): Observable<Inspection[]> {
    return this.http.get<Inspection[]>('/api/v1/inspections');
  }
}
