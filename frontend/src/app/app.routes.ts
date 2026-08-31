import { Routes } from '@angular/router';
import { authGuard } from './core/auth.guard';
import { AdminShellComponent } from './layout/admin-shell.component';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
  {
    path: 'login',
    loadComponent: () => import('./features/login/login-page.component').then(m => m.LoginPageComponent),
  },
  {
    path: '',
    component: AdminShellComponent,
    canActivate: [authGuard],
    children: [
      {
        path: 'dashboard',
        loadComponent: () => import('./features/dashboard/dashboard-page.component').then(m => m.DashboardPageComponent),
      },
      {
        path: 'records',
        loadComponent: () => import('./features/records/records-page.component').then(m => m.RecordsPageComponent),
      },
      {
        path: 'tasks',
        loadComponent: () => import('./features/tasks/tasks-page.component').then(m => m.TasksPageComponent),
      },
      {
        path: 'architecture',
        loadComponent: () => import('./features/architecture/architecture-page.component').then(m => m.ArchitecturePageComponent),
      },
      {
        path: 'risk',
        loadComponent: () => import('./features/risk/risk-page.component').then(m => m.RiskPageComponent),
      },
    ],
  },
  { path: '**', redirectTo: 'dashboard' },
];