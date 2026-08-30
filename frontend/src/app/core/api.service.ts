import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';

/** Thin wrapper around fetch for API calls with auth token + JSON handling. */
@Injectable({ providedIn: 'root' })
export class ApiService {
  private readonly baseUrl = environment.apiBaseUrl;

  async get<T>(path: string): Promise<T> {
    const res = await fetch(this.baseUrl + path, {
      headers: this.headers(),
    });
    return this.handle<T>(res);
  }

  async post<T>(path: string, body: unknown): Promise<T> {
    const res = await fetch(this.baseUrl + path, {
      method: 'POST',
      headers: { ...this.headers(), 'Content-Type': 'application/json' },
      body: JSON.stringify(body),
    });
    return this.handle<T>(res);
  }

  private headers(): Record<string, string> {
    const token = localStorage.getItem('ehsms_token');
    return token ? { Authorization: `Bearer ${token}` } : {};
  }

  private async handle<T>(res: Response): Promise<T> {
    if (!res.ok) {
      const text = await res.text().catch(() => '');
      throw new Error(`API ${res.status}: ${text.slice(0, 200)}`);
    }
    if (res.status === 204) return undefined as T;
    return (await res.json()) as T;
  }
}