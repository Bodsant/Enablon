import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface IntegrationInterface {
  id: string;
  name: string;
  interfaceType: string;
  protocol: string;
  status: string;
}

export interface IntegrationRun {
  id: string;
  interfaceId: string;
  direction: string;
  status: string;
  messageCount: number;
  startedAt: string;
}

@Injectable({ providedIn: 'root' })
export class IntegrationService {
  private readonly http = inject(HttpClient);

  listInterfaces(): Observable<IntegrationInterface[]> {
    return this.http.get<IntegrationInterface[]>('/api/v1/integration/interfaces');
  }

  listRuns(): Observable<IntegrationRun[]> {
    return this.http.get<IntegrationRun[]>('/api/v1/integration/runs');
  }
}
