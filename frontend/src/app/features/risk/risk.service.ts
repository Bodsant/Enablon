import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface RiskHazard {
  id: string;
  code: string;
  name: string;
  description?: string;
}

export interface RiskRegister {
  id: string;
  hazardId: string;
  activityName: string;
  riskEvent: string;
  status: string;
}

@Injectable({ providedIn: 'root' })
export class RiskService {
  private readonly http = inject(HttpClient);

  listHazards(): Observable<RiskHazard[]> {
    return this.http.get<RiskHazard[]>('/api/v1/risk/hazards');
  }

  listRegisters(): Observable<RiskRegister[]> {
    return this.http.get<RiskRegister[]>('/api/v1/risk/registers');
  }
}
