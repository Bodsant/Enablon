import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface HealthProfile {
  id: string;
  personName: string;
  bloodType: string;
  allergies: string;
  conditions: string;
  status: string;
}

export interface FitnessStatus {
  id: string;
  personName: string;
  fitnessClass: string;
  assessmentDate: string;
  nextAssessmentDate: string;
  status: string;
}

@Injectable({ providedIn: 'root' })
export class HealthService {
  private readonly http = inject(HttpClient);

  listProfiles(): Observable<HealthProfile[]> {
    return this.http.get<HealthProfile[]>('/api/v1/health/profiles');
  }

  listFitnessStatuses(): Observable<FitnessStatus[]> {
    return this.http.get<FitnessStatus[]>('/api/v1/health/fitness-statuses');
  }
}
