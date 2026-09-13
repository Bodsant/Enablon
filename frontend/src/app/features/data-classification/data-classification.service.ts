import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface DataClassificationItem {
  id: string;
  code: string;
  name: string;
  rank: number;
  isRestricted: boolean;
  description: string;
}

@Injectable({ providedIn: 'root' })
export class DataClassificationService {
  private readonly http = inject(HttpClient);
  listItems(): Observable<DataClassificationItem[]> {
    return this.http.get<DataClassificationItem[]>('/api/v1/data-classifications');
  }
}