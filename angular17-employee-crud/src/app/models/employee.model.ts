export type EmployeeRole = 'Developer' | 'QA' | 'Manager' | 'HR';

export interface Employee {
  id: number;
  name: string;
  email: string;
  role: EmployeeRole;
  salary: number;
  active: boolean;
  joinedOn: string;
}

export interface EmployeeForm {
  name: string;
  email: string;
  role: EmployeeRole;
  salary: number;
  active: boolean;
}
