import apiClient from './apiClient';

interface LoginData {
  email: string;
  password: string;
}

interface LoginResponse {
  token: string;
  role: string;
  id: number;
}

export const authService = {
  async login(credentials: { email: string; password: string }) {
    try {
      const response = await apiClient.post('/auth/login', credentials);

      
      if (!response.data.token || !response.data.role) {
        throw new Error('Respuesta de autenticación inválida');
      }
      
      return {
        token: response.data.token,
        role: response.data.role,
        id: response.data.userId
      };
    } catch (error) {
      console.error('Error en authService.login:', error);
      throw error;
    }
  },
  logout: () => {
    localStorage.removeItem('token');
    localStorage.removeItem('userRole');
    localStorage.removeItem('userId');
  },
};