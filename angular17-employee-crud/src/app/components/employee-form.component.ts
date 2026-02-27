import { Component, EventEmitter, Input, OnChanges, OnInit, Output, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Employee, EmployeeForm, EmployeeRole } from '../models/employee.model';

@Component({
  selector: 'app-employee-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="card">
      <h3>{{ selectedEmployee ? 'Update Employee' : 'Add Employee' }}</h3>
      <form [formGroup]="employeeForm" (ngSubmit)="submit()" class="grid grid-2">
        <div>
          <label>Name</label>
          <input formControlName="name" placeholder="Enter name" />
          <div class="error" *ngIf="employeeForm.controls.name.touched && employeeForm.controls.name.invalid">
            Name is required.
          </div>
        </div>

        <div>
          <label>Email</label>
          <input formControlName="email" placeholder="Enter email" />
          <div class="error" *ngIf="employeeForm.controls.email.touched && employeeForm.controls.email.invalid">
            Valid email is required.
          </div>
        </div>

        <div>
          <label>Role</label>
          <select formControlName="role">
            <option *ngFor="let role of roles" [value]="role">{{ role }}</option>
          </select>
        </div>

        <div>
          <label>Salary</label>
          <input type="number" formControlName="salary" />
        </div>

        <div>
          <label>
            <input type="checkbox" formControlName="active" /> Active employee
          </label>
        </div>

        <div style="display:flex; gap:.5rem; align-items:end;">
          <button class="primary" type="submit" [disabled]="employeeForm.invalid">
            {{ selectedEmployee ? 'Update' : 'Create' }}
          </button>
          <button class="ghost" type="button" (click)="reset()">Reset</button>
        </div>
      </form>
    </div>
  `
})
export class EmployeeFormComponent implements OnInit, OnChanges {
  @Input() selectedEmployee?: Employee;
  @Output() saveEmployee = new EventEmitter<EmployeeForm>();
  @Output() cancelEdit = new EventEmitter<void>();

  readonly roles: EmployeeRole[] = ['Developer', 'QA', 'Manager', 'HR'];

  readonly employeeForm = this.formBuilder.nonNullable.group({
    name: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    role: 'Developer' as EmployeeRole,
    salary: [50000, [Validators.required, Validators.min(10000)]],
    active: true
  });

  constructor(private readonly formBuilder: FormBuilder) {}

  ngOnInit(): void {
    // Lifecycle hook for demo.
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['selectedEmployee']?.currentValue) {
      const item = changes['selectedEmployee'].currentValue as Employee;
      this.employeeForm.patchValue({
        name: item.name,
        email: item.email,
        role: item.role,
        salary: item.salary,
        active: item.active
      });
    }
  }

  submit(): void {
    if (this.employeeForm.valid) {
      this.saveEmployee.emit(this.employeeForm.getRawValue());
      if (!this.selectedEmployee) {
        this.employeeForm.reset({ role: 'Developer', salary: 50000, active: true, name: '', email: '' });
      }
    }
  }

  reset(): void {
    this.employeeForm.reset({ role: 'Developer', salary: 50000, active: true, name: '', email: '' });
    this.cancelEdit.emit();
  }
}
