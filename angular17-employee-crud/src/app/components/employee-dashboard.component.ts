import { AsyncPipe, CommonModule, DatePipe, JsonPipe, NgClass, NgIf, NgStyle } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Observable } from 'rxjs';
import { Employee, EmployeeForm } from '../models/employee.model';
import { EmployeeService } from '../services/employee.service';
import { SalaryInrPipe } from '../pipes/salary-inr.pipe';
import { HighlightDirective } from '../directives/highlight.directive';
import { EmployeeFormComponent } from './employee-form.component';

@Component({
  selector: 'app-employee-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    AsyncPipe,
    NgIf,
    NgClass,
    NgStyle,
    DatePipe,
    JsonPipe,
    SalaryInrPipe,
    HighlightDirective,
    EmployeeFormComponent
  ],
  template: `
    <h2>Employee Management (Angular 17 CRUD)</h2>

    <app-employee-form
      [selectedEmployee]="selectedEmployee"
      (saveEmployee)="onSave($event)"
      (cancelEdit)="selectedEmployee = undefined"
    />

    <div class="card">
      <h3>Employees</h3>

      <label>
        Search by name (two-way binding):
        <input [(ngModel)]="searchText" placeholder="type to filter..." />
      </label>

      <table>
        <thead>
          <tr>
            <th>#</th>
            <th>Name</th>
            <th>Email</th>
            <th>Role</th>
            <th>Salary</th>
            <th>Joined</th>
            <th>Status</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          <tr *ngFor="let employee of filterEmployees((employees$ | async) ?? []); let i = index" [appHighlight]="employee.active">
            <td>{{ i + 1 }}</td>
            <td>{{ employee.name }}</td>
            <td>{{ employee.email }}</td>
            <td [ngStyle]="{ fontWeight: employee.role === 'Manager' ? '700' : '400' }">{{ employee.role }}</td>
            <td>{{ employee.salary | salaryInr }}</td>
            <td>{{ employee.joinedOn | date:'mediumDate' }}</td>
            <td>
              <span class="badge" [ngClass]="employee.active ? 'primary' : 'danger'" [style.background]="employee.active ? '#86efac' : '#fca5a5'">
                {{ employee.active ? 'Active' : 'Inactive' }}
              </span>
            </td>
            <td style="display:flex; gap:.35rem;">
              <button class="warn" (click)="edit(employee)">Edit</button>
              <button class="danger" (click)="remove(employee.id)">Delete</button>
            </td>
          </tr>
        </tbody>
      </table>

      <div class="card" *ngIf="selectedEmployee">
        <strong>Selected Employee (json pipe):</strong>
        <pre>{{ selectedEmployee | json }}</pre>
      </div>
    </div>
  `
})
export class EmployeeDashboardComponent implements OnInit {
  employees$!: Observable<Employee[]>;
  selectedEmployee?: Employee;
  searchText = '';

  constructor(private readonly employeeService: EmployeeService) {}

  ngOnInit(): void {
    this.loadEmployees();
  }

  onSave(form: EmployeeForm): void {
    if (this.selectedEmployee) {
      this.employeeService.updateEmployee(this.selectedEmployee.id, form).subscribe(() => {
        this.selectedEmployee = undefined;
        this.loadEmployees();
      });
      return;
    }

    this.employeeService.createEmployee(form).subscribe(() => this.loadEmployees());
  }

  edit(employee: Employee): void {
    this.selectedEmployee = employee;
  }

  remove(id: number): void {
    this.employeeService.deleteEmployee(id).subscribe(() => this.loadEmployees());
  }

  filterEmployees(employees: Employee[]): Employee[] {
    const term = this.searchText.trim().toLowerCase();
    if (!term) {
      return employees;
    }
    return employees.filter((employee) => employee.name.toLowerCase().includes(term));
  }

  private loadEmployees(): void {
    this.employees$ = this.employeeService.getEmployees();
  }
}
