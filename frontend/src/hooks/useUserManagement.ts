import { useState, useEffect } from 'react';
import { userService } from '../api/userService';
import type { User, UserCreateDto, UserUpdateDto } from '../types/userTypes';

export const useUserManagement = () => {
  const [users, setUsers] = useState<User[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  useEffect(() => {
    fetchUsers(); 
  }, []);
  const fetchUsers = async () => {
    setLoading(true);
    setError(null);
    try {
      const usersData = await userService.getAll(); 
      console.log('Users data received:', usersData); 
      setUsers(usersData);
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Failed to fetch users';
      setError(errorMessage);
      console.error('Fetch users error:', err);
      setUsers([]);
    } finally {
      setLoading(false);
    }
  };

  const createUser = async (userData: UserCreateDto) => {
    setLoading(true);
    try {
      const newUser = await userService.create(userData);
      setUsers(prev => [...prev, newUser]);
      return newUser;
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to create user');
      throw err;
    } finally {
      setLoading(false);
    }
  };

  const updateUser = async (id: number, userData: UserUpdateDto) => {
    setLoading(true);
    try {
      await userService.update(id, userData);
      setUsers(prev =>
        prev.map(user => (user.id === id ? { ...user, ...userData } : user))
      );
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to update user');
      throw err;
    } finally {
      setLoading(false);
    }
  };

  const deleteUser = async (id: number) => {
    setLoading(true);
    try {
      await userService.delete(id);
      setUsers(prev => prev.filter(user => user.id !== id));
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to delete user');
      throw err;
    } finally {
      setLoading(false);
    }
  };

  return {
    users,
    loading,
    error,
    fetchUsers,
    createUser,
    updateUser,
    deleteUser,
  };
};