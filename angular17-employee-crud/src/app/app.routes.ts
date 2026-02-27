import { Routes } from '@angular/router';
import { EmployeeDashboardComponent } from './components/employee-dashboard.component';
import { TopicCoverageComponent } from './components/topic-coverage.component';

export const appRoutes: Routes = [
  { path: '', redirectTo: 'employees', pathMatch: 'full' },
  { path: 'employees', component: EmployeeDashboardComponent },
  { path: 'topics', component: TopicCoverageComponent },
  { path: '**', redirectTo: 'employees' }
];
