import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Incident {
  id: string;
  recordNumber: string;
  description: string;
  classificationStatus: string;
}

export interface CapaAction {
  id: string;
  actionType: string;
  description: string;
  priority: string;
}

@Injectable({ providedIn: 'root' })
export class IncidentService {
  private readonly http = inject(HttpClient);

  listIncidents(): Observable<Incident[]> {
    return this.http.get<Incident[]>('/api/v1/incidents');
  }

  listCapaActions(): Observable<CapaAction[]> {
    return this.http.get<CapaAction[]>('/api/v1/capa/actions');
  }
}
