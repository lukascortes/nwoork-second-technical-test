import { useState, useEffect } from 'react';
import { userService } from '../api/userService';
import { getApiErrorMessage } from '../api/errors';
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
      setUsers(await userService.getAll());
    } catch (err) {
      setError(getApiErrorMessage(err, 'Failed to fetch users'));
      setUsers([]);
    } finally {
      setLoading(false);
    }
  };

  const createUser = async (userData: UserCreateDto) => {
    setLoading(true);
    try {
      const created = await userService.create(userData);
      setUsers((prev) => [...prev, created]);
      return created;
    } catch (err) {
      setError(getApiErrorMessage(err, 'Failed to create user'));
      throw err;
    } finally {
      setLoading(false);
    }
  };

  const updateUser = async (id: string, userData: UserUpdateDto) => {
    setLoading(true);
    try {
      const updated = await userService.update(id, userData);
      setUsers((prev) => prev.map((user) => (user.id === id ? updated : user)));
      return updated;
    } catch (err) {
      setError(getApiErrorMessage(err, 'Failed to update user'));
      throw err;
    } finally {
      setLoading(false);
    }
  };

  const deleteUser = async (id: string) => {
    setLoading(true);
    try {
      await userService.delete(id);
      setUsers((prev) => prev.filter((user) => user.id !== id));
    } catch (err) {
      setError(getApiErrorMessage(err, 'Failed to delete user'));
      throw err;
    } finally {
      setLoading(false);
    }
  };

  return { users, loading, error, fetchUsers, createUser, updateUser, deleteUser };
};
