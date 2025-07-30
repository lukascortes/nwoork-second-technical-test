import apiClient from './apiClient';
import type { User, UserCreateDto, UserUpdateDto } from '../types/userTypes';

export const userService = {
  getAll: async (): Promise<User[]> => {
    try {
      const response = await apiClient.get('/users');
      console.log('Response from getAllUsers:', response); // Log para debugging
      return response.data;
    } catch (error) {
      console.error('Error fetching users:', {
        error: (error as any).response?.data
      });
      throw new Error((error as any).response?.data?.message || 'Failed to fetch users');
    }
  },

  getById: async (id: number): Promise<User> => {
    try {
      const response = await apiClient.get(`/users/${id}`);
      return response.data;
    } catch (error) {
      console.error('Error fetching user:', {
        id,
        error: (error as any).response?.data
      });
      throw new Error((error as any).response?.data?.message || 'Failed to fetch user');
    }
  },

  create: async (userData: UserCreateDto): Promise<User> => {
    try {
      
      const payload = {
        email: userData.email,
        password: userData.password,
        role: userData.role === 'Admin' ? 0 : 1 
      };

      const response = await apiClient.post('/users', payload);
      return {
        ...response.data,
        role: response.data.role === 0 ? 'Admin' : 'Employee' 
      };
    } catch (error) {
      console.error('Error creating user:', {
        request: userData,
        error: (error as any).response?.data
      });
      throw new Error(
        (error as any).response?.data?.title ||
        'Validation failed. Please check your data.'
      );
    }
  },

  update: async (id: number, userData: UserUpdateDto): Promise<User> => {
    try {
      console.log('Updating user with ID:', id, 'Data:', userData); // Log para debugging
      const response = await apiClient.put(`/users/${id}`, userData);
      console.log('Response from updateUser:', response); // Log para debugging
      return response.data;
    } catch (error) {
      console.error('Error updating user:', {
        id,
        request: userData,
        error: (error as any).response?.data
      });
      throw new Error((error as any).response?.data?.message || 'Failed to update user');
    }
  },

  delete: async (id: number): Promise<void> => {
    try {
      await apiClient.delete(`/users/${id}`);
    } catch (error) {
      console.error('Error deleting user:', {
        id,
        error: (error as any).response?.data
      });
      throw new Error((error as any).response?.data?.message || 'Failed to delete user');
    }
  },

  
  getByRole: async (role: string): Promise<User[]> => {
    try {
      const response = await apiClient.get(`/users/role/${role}`);
      return response.data;
    } catch (error) {
      console.error('Error fetching users by role:', {
        role,
        error: (error as any).response?.data
      });
      throw new Error((error as any).response?.data?.message || 'Failed to fetch users by role');
    }
  }
};