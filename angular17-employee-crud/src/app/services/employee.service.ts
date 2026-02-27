import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, delay, map, of } from 'rxjs';
import { Employee, EmployeeForm } from '../models/employee.model';

@Injectable({ providedIn: 'root' })
export class EmployeeService {
  // Kept to demonstrate HttpClient injection and where real APIs are called.
  private readonly apiUrl = '/api/employees';

  private employees: Employee[] = [
    { id: 1, name: 'Anita Rao', email: 'anita@corp.com', role: 'Manager', salary: 120000, active: true, joinedOn: '2022-05-12' },
    { id: 2, name: 'John Peter', email: 'john@corp.com', role: 'Developer', salary: 90000, active: true, joinedOn: '2023-02-17' }
  ];

  constructor(private readonly http: HttpClient) {
    void this.http;
    void this.apiUrl;
  }

  getEmployees(): Observable<Employee[]> {
    return of(this.employees).pipe(delay(250));
  }

  createEmployee(form: EmployeeForm): Observable<Employee> {
    const employee: Employee = {
      ...form,
      id: Date.now(),
      joinedOn: new Date().toISOString().slice(0, 10)
    };
    this.employees = [...this.employees, employee];
    return of(employee).pipe(delay(200));
  }

  updateEmployee(id: number, form: EmployeeForm): Observable<Employee | undefined> {
    this.employees = this.employees.map((employee) =>
      employee.id === id ? { ...employee, ...form } : employee
    );
    return this.getEmployees().pipe(map((items) => items.find((item) => item.id === id)));
  }

  deleteEmployee(id: number): Observable<boolean> {
    this.employees = this.employees.filter((employee) => employee.id !== id);
    return of(true).pipe(delay(150));
  }
}
