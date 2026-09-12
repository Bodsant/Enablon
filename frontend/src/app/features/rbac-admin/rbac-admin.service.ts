import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface RoleItem {
  id: string;
  code: string;
  name: string;
  scopeType: string;
  isSystem: boolean;
}

export interface PermissionItem {
  id: string;
  code: string;
  module: string;
  action: string;
  description: string;
}

@Injectable({ providedIn: 'root' })
export class RbacAdminService {
  private readonly http = inject(HttpClient);
  listRoles(): Observable<RoleItem[]> {
    return this.http.get<RoleItem[]>('/api/v1/identities/roles');
  }
  listPermissions(): Observable<PermissionItem[]> {
    return this.http.get<PermissionItem[]>('/api/v1/identities/permissions');
  }
}