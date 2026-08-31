import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Asset {
  id: string;
  name: string;
  assetCode: string;
  assetType: string;
  location: string;
  status: string;
}

export interface EmergencyPlan {
  id: string;
  title: string;
  planType: string;
  version: string;
  effectiveDate: string;
  status: string;
}

@Injectable({ providedIn: 'root' })
export class AssetEmergencyService {
  private readonly http = inject(HttpClient);

  listAssets(): Observable<Asset[]> {
    return this.http.get<Asset[]>('/api/v1/assets');
  }

  listEmergencyPlans(): Observable<EmergencyPlan[]> {
    return this.http.get<EmergencyPlan[]>('/api/v1/emergency/plans');
  }
}
