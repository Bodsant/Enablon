import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Kpi {
  id: string;
  name: string;
  code: string;
  description: string;
  formula: string;
  unit: string;
  status: string;
}

export interface KpiVersion {
  id: string;
  kpiId: string;
  period: string;
  target: number;
  actual: number;
  status: string;
}

@Injectable({ providedIn: 'root' })
export class ReportingService {
  private readonly http = inject(HttpClient);

  listKpis(): Observable<Kpi[]> {
    return this.http.get<Kpi[]>('/api/v1/kpis');
  }

  listKpiVersions(): Observable<KpiVersion[]> {
    return this.http.get<KpiVersion[]>('/api/v1/kpis/versions');
  }
}
