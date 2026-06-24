export type UserRole = 'Admin' | 'Employee';

// Shape returned by POST /api/auth/login and /api/auth/register
export interface AuthResponse {
  accessToken: string;
  expiresAtUtc: string;
  userId: string;
  email: string;
  fullName: string;
  role: UserRole;
}

export interface AuthContextType {
  isAuthenticated: boolean;
  role: UserRole | null;
  userId: string | null;
  fullName: string | null;
  login: (email: string, password: string) => Promise<UserRole | null>;
  logout: () => void;
}
