import { Routes } from '@angular/router';
import { authGuard } from './core/auth.guard';
import { Login } from './features/auth/login';
import { TaskDashboard } from './features/tasks/task-dashboard';

export const routes: Routes = [
  { path: 'login', component: Login },
  { path: 'tasks', component: TaskDashboard, canActivate: [authGuard] },
  { path: '', pathMatch: 'full', redirectTo: 'tasks' },
  { path: '**', redirectTo: 'login' }
];
