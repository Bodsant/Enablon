import { Routes } from '@angular/router';
export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'architecture' },
  { path: 'architecture', loadComponent: () => import('./features/architecture/architecture-page.component').then(m => m.ArchitecturePageComponent) },
  { path: '**', redirectTo: 'architecture' }
];
