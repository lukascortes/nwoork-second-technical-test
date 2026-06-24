export type Role = 'Admin' | 'Employee';

export interface User {
  id: string;
  email: string;
  fullName: string;
  role: Role;
  annualVacationDays: number;
  createdAt: string;
}

export interface UserCreateDto {
  email: string;
  password: string;
  fullName: string;
  role: Role;
  annualVacationDays?: number;
}

export interface UserUpdateDto {
  email?: string;
  password?: string;
  fullName?: string;
  role?: Role;
  annualVacationDays?: number;
}
