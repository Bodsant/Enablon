import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../core/auth.service';

/** Route guard: redirects to /login when the user is not authenticated. */
export function authGuard() {
  const auth = inject(AuthService);
  const router = inject(Router);
  if (auth.isAuthenticated) return true;
  return router.createUrlTree(['/login']);
}