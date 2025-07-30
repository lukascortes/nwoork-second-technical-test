import { useState, useEffect, useContext, createContext } from 'react';
import { authService } from '../api/authService';
import type { AuthContextType } from '../types/authTypes';

const AuthContext = createContext<AuthContextType | null>(null);

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [isAuthenticated, setIsAuthenticated] = useState(() => {
    return !!localStorage.getItem('token');
  });
  
  const [role, setRole] = useState<'Admin' | 'Employee' | null>(() => {
    const storedRole = localStorage.getItem('userRole');
    return storedRole as 'Admin' | 'Employee' | null;
  });

  
  const [userId, setUserId] = useState<number | null>(() => {
    const storedUserId = localStorage.getItem('userId');
    return storedUserId ? parseInt(storedUserId) : null;
  });

  const login = async (email: string, password: string) => {
    try {
      
      const { token, role, id } = await authService.login({ email, password });
      
      
      localStorage.setItem('token', token);
      localStorage.setItem('userRole', role);
      localStorage.setItem('userId', id.toString()); 
      
      setIsAuthenticated(true);
      setRole(role as 'Admin' | 'Employee');
      setUserId(id); 
      
      return true;
    } catch (error) {
      console.error('Login failed:', error);
      return false;
    }
  };

  const logout = () => {
    localStorage.removeItem('token');
    localStorage.removeItem('userRole');
    localStorage.removeItem('userId'); 
    setIsAuthenticated(false);
    setRole(null);
    setUserId(null); 
  };

  return (
    <AuthContext.Provider 
      value={{ 
        isAuthenticated, 
        role, 
        userId, 
        login, 
        logout 
      }}
    >
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