import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface SecurityInfo {
  application: string;
  version: string;
  environment: string;
}

@Injectable({ providedIn: 'root' })
export class SecurityHeadersService {
  private readonly http = inject(HttpClient);
  getInfo(): Observable<SecurityInfo> {
    return this.http.get<SecurityInfo>('/api/v1/architecture/info');
  }
}