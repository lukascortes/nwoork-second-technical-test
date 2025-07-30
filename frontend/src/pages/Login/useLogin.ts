import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { authService } from '../../api/authService';

export const useLogin = () => {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const navigate = useNavigate();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError('');

    try {
      console.log('Iniciando petición de login...');
      const response = await authService.login({ email, password });
      console.log('Tipo de respuesta:', typeof response);
      console.log('Respuesta completa:', response);
      console.log('¿Response es objeto?', response instanceof Object);
      if (!response) {
        throw new Error('Respuesta vacía del servidor');
      }

      if (typeof response !== 'object') {
        throw new Error(`Respuesta no es objeto, es: ${typeof response}`);
      }

      if (!response.token) {
        throw new Error('Token no encontrado en respuesta');
      }

      if (!response.role) {
        throw new Error('Rol no encontrado en respuesta');
      }

      console.log('Token recibido:', response.token);
      console.log('Rol recibido:', response.role);

      localStorage.setItem('token', response.token);
      localStorage.setItem('userRole', response.role);
      localStorage.setItem('userId', response.id.toString()); 

      console.log('Token almacenado:', localStorage.getItem('token'));
      console.log('Rol almacenado:', localStorage.getItem('userRole'));
      console.log('ID de usuario almacenado:', localStorage.getItem('userId'));
      const targetPath = response.role === 'Admin'
        ? '/dashboard/admin'
        : '/dashboard/employee';

      console.log('Redirigiendo a:', targetPath);
      window.location.href = targetPath;
    } catch (err) {
      console.error('Error en login:', err);
      setError(err instanceof Error ? err.message : 'Login failed');
      
      localStorage.removeItem('token');
      localStorage.removeItem('userRole');
    } finally {
      setLoading(false);
    }
  };

  return {
    email,
    setEmail,
    password,
    setPassword,
    error,
    loading,
    handleSubmit,
  };
};