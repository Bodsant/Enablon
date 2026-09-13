import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface SessionItem {
  id: string;
  userEmail: string;
  ipAddress: string;
  userAgent: string;
  createdAt: string;
  expiresAt: string;
  isCurrent: boolean;
}

@Injectable({ providedIn: 'root' })
export class SessionMgmtService {
  private readonly http = inject(HttpClient);
  listItems(): Observable<SessionItem[]> {
    return this.http.get<SessionItem[]>('/api/v1/identities/sessions');
  }
}