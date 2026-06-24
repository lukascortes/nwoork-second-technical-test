import apiClient from './apiClient';
import { getApiErrorMessage } from './errors';
import type { User, UserCreateDto, UserUpdateDto } from '../types/userTypes';

export const userService = {
  getAll: async (): Promise<User[]> => {
    const { data } = await apiClient.get<User[]>('/users');
    return data;
  },

  getById: async (id: string): Promise<User> => {
    const { data } = await apiClient.get<User>(`/users/${id}`);
    return data;
  },

  create: async (payload: UserCreateDto): Promise<User> => {
    try {
      const { data } = await apiClient.post<User>('/users', payload);
      return data;
    } catch (error) {
      throw new Error(getApiErrorMessage(error, 'Failed to create user'));
    }
  },

  update: async (id: string, payload: UserUpdateDto): Promise<User> => {
    try {
      const { data } = await apiClient.put<User>(`/users/${id}`, payload);
      return data;
    } catch (error) {
      throw new Error(getApiErrorMessage(error, 'Failed to update user'));
    }
  },

  delete: async (id: string): Promise<void> => {
    try {
      await apiClient.delete(`/users/${id}`);
    } catch (error) {
      throw new Error(getApiErrorMessage(error, 'Failed to delete user'));
    }
  },
};
