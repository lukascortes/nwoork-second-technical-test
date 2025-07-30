
export const Role = {
  Admin: 'Admin',
  Employee: 'Employee'
} as const;

export type Role = keyof typeof Role;


export interface User {
  id: number;
  email: string;
  role: Role;
}

export interface UserCreateDto {
  email: string;
  password: string;
  role: Role;
}

export interface UserUpdateDto {
  email?: string;
  password?: string;
  role?: Role;
}

export type UserUpdateDtoRole = {
  email?: string;
  password?: string;
  role: 0 | 1;
};