import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface AccessScopeItem {
  id: string;
  memberName: string;
  scopeType: string;
  siteName?: string | null;
  companyName?: string | null;
  validFrom?: string | null;
  validUntil?: string | null;
  status: string;
}

export interface TempGrantItem {
  id: string;
  memberName: string;
  roleName: string;
  validFrom?: string | null;
  validUntil?: string | null;
  reason: string;
  status: string;
}

@Injectable({ providedIn: 'root' })
export class AccessScopeService {
  private readonly http = inject(HttpClient);
  listScopes(): Observable<AccessScopeItem[]> {
    return this.http.get<AccessScopeItem[]>('/api/v1/identities/scopes');
  }
  listTempGrants(): Observable<TempGrantItem[]> {
    return this.http.get<TempGrantItem[]>('/api/v1/identities/temporary-grants');
  }
}