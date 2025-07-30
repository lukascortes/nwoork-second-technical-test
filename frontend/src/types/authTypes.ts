export type UserRole = 'Admin' | 'Employee';

export interface AuthContextType {
  isAuthenticated: boolean;
  role: 'Admin' | 'Employee' | null;
  userId: number | null;
  login: (email: string, password: string) => Promise<boolean>;
  logout: () => void;
}