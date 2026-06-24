import { useState, useContext, createContext } from 'react';
import { authService } from '../api/authService';
import type { AuthContextType, UserRole } from '../types/authTypes';

const AuthContext = createContext<AuthContextType | null>(null);

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [isAuthenticated, setIsAuthenticated] = useState(() => !!localStorage.getItem('token'));
  const [role, setRole] = useState<UserRole | null>(
    () => localStorage.getItem('userRole') as UserRole | null
  );
  const [userId, setUserId] = useState<string | null>(() => localStorage.getItem('userId'));
  const [fullName, setFullName] = useState<string | null>(() => localStorage.getItem('fullName'));

  const login = async (email: string, password: string): Promise<UserRole | null> => {
    try {
      const res = await authService.login({ email, password });

      localStorage.setItem('token', res.accessToken);
      localStorage.setItem('userRole', res.role);
      localStorage.setItem('userId', res.userId);
      localStorage.setItem('fullName', res.fullName);

      setIsAuthenticated(true);
      setRole(res.role);
      setUserId(res.userId);
      setFullName(res.fullName);

      return res.role;
    } catch (error) {
      console.error('Login failed:', error);
      return null;
    }
  };

  const logout = () => {
    authService.logout();
    setIsAuthenticated(false);
    setRole(null);
    setUserId(null);
    setFullName(null);
  };

  return (
    <AuthContext.Provider value={{ isAuthenticated, role, userId, fullName, login, logout }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
}
