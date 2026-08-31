import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface LegalSource {
  id: string;
  title: string;
  jurisdiction: string;
  sourceType: string;
  effectiveDate: string;
  status: string;
}

export interface LegalObligation {
  id: string;
  legalSourceId: string;
  title: string;
  description: string;
  dueDate: string;
  complianceStatus: string;
}

@Injectable({ providedIn: 'root' })
export class LegalService {
  private readonly http = inject(HttpClient);

  listSources(): Observable<LegalSource[]> {
    return this.http.get<LegalSource[]>('/api/v1/legal/sources');
  }

  listObligations(): Observable<LegalObligation[]> {
    return this.http.get<LegalObligation[]>('/api/v1/legal/obligations');
  }
}
