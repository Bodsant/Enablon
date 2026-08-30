import { Injectable, signal } from '@angular/core';
import { ApiService } from './api.service';

export interface AuthUser {
  sub: string;
  email: string;
  tenantId: string;
}

export interface LoginResponse {
  accessToken: string;
  refreshToken?: string;
  expiresInSeconds?: number;
}

const TOKEN_KEY = 'ehsms_token';
const USER_KEY = 'ehsms_user';

@Injectable({ providedIn: 'root' })
export class AuthService {
  readonly user = signal<AuthUser | null>(this.loadUser());

  constructor(private readonly api: ApiService) {}

  get token(): string | null {
    return localStorage.getItem(TOKEN_KEY);
  }

  get isAuthenticated(): boolean {
    return !!this.token;
  }

  async login(email: string, password: string): Promise<AuthUser> {
    const res = await this.api.post<LoginResponse>('/auth/login', { email, password });
    localStorage.setItem(TOKEN_KEY, res.accessToken);
    const user = this.decodeUser(res.accessToken);
    this.user.set(user);
    return user;
  }

  logout(): void {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
    this.user.set(null);
  }

  private decodeUser(token: string): AuthUser {
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      return {
        sub: payload.sub ?? '',
        email: payload.email ?? '',
        tenantId: payload.tenant ?? '',
      };
    } catch {
      return { sub: '', email: '', tenantId: '' };
    }
  }

  private loadUser(): AuthUser | null {
    const raw = localStorage.getItem(USER_KEY);
    if (!raw) return null;
    try {
      return JSON.parse(raw) as AuthUser;
    } catch {
      return null;
    }
  }
}