import { useState } from 'react';
import UserList from '../../components/admin/users/UserList';
import UserForm from '../../components/admin/users/UserForm';
import type { User } from '../../types/userTypes';
import Navbar from '../../components/layout/AdminNavbar';
import LoadingSpinner from '../../components/common/LoadingSpinner';
import ErrorMessage from '../../components/common/ErrorMessage';
import { useUserManagement } from '../../hooks/useUserManagement';
import {
  UserIcon,
  PlusCircleIcon,
  CheckCircleIcon,
  XCircleIcon
} from '@heroicons/react/24/outline';

export default function UsersPage() {
  const [isFormOpen, setIsFormOpen] = useState(false);
  const [currentUser, setCurrentUser] = useState<User | null>(null);
  const [successMessage, setSuccessMessage] = useState('');
  const userManagement = useUserManagement();
  const { users = [], loading, error, fetchUsers, deleteUser } = userManagement;

  const handleSuccess = () => {
    setIsFormOpen(false);
    fetchUsers();
    setSuccessMessage('User operation completed successfully!');
    setTimeout(() => setSuccessMessage(''), 3000);
  };

  const handleEdit = (user: User) => {
    setCurrentUser(user);
    setIsFormOpen(true);
  };

  if (loading) return (
    <div className="min-h-screen bg-gradient-to-b from-purple-50 to-gray-50">
      <Navbar />
      <LoadingSpinner />
    </div>
  );

  if (error) return (
    <div className="min-h-screen bg-gradient-to-b from-purple-50 to-gray-50">
      <Navbar />
      <ErrorMessage message={error} />
    </div>
  );

  return (
    <div className="min-h-screen bg-gradient-to-b from-purple-50 to-gray-50">
      <Navbar />

      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
       
        {successMessage && (
          <div className="mb-6 p-4 bg-green-50 rounded-lg border border-green-200 flex items-start">
            <CheckCircleIcon className="h-5 w-5 text-green-500 mr-3 mt-0.5" />
            <p className="text-green-800">{successMessage}</p>
          </div>
        )}

       
        <div className="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden mb-8">
          <div className="p-6">
            <div className="flex items-center justify-between">
              <div>
                <h2 className="text-2xl font-semibold text-gray-800 flex items-center">
                  <UserIcon className="h-6 w-6 text-purple-500 mr-2" />
                  User Management
                </h2>
                <p className="mt-1 text-sm text-gray-500">
                  Manage all system users and their permissions
                </p>
              </div>
              <div className="flex space-x-4">
                <div className="bg-purple-50 px-4 py-3 rounded-lg">
                  <p className="text-sm font-medium text-gray-500">Total Users</p>
                  <p className="text-2xl font-semibold text-purple-600">{users.length}</p>
                </div>
              
              </div>
            </div>
          </div>
        </div>

        
        <div className="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden">
          <div className="p-6">
            <div className="flex items-center justify-between mb-6">
              <h3 className="text-lg font-medium text-gray-900">
                User List
              </h3>
              <button
                onClick={() => {
                  setCurrentUser(null);
                  setIsFormOpen(true);
                }}
                className="inline-flex items-center px-4 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-gradient-to-r from-purple-600 to-purple-700 hover:from-purple-700 hover:to-purple-800 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-purple-500"
              >
                <PlusCircleIcon className="-ml-1 mr-2 h-5 w-5" />
                Add New User
              </button>
            </div>

            {users.length > 0 ? (
              <UserList
                onEdit={handleEdit}
                users={users}
                loading={loading}
                error={error}
                onDelete={(id) => {
                  if (window.confirm('Are you sure you want to delete this user?')) {
                    deleteUser(id);
                  }
                }}
              />
            ) : (
              <div className="text-center py-12">
                <UserIcon className="mx-auto h-12 w-12 text-gray-400" />
                <h3 className="mt-2 text-lg font-medium text-gray-900">No users found</h3>
                <p className="mt-1 text-sm text-gray-500">
                  There are no users in the system yet.
                </p>
                <div className="mt-6">
                  <button
                    onClick={() => {
                      setCurrentUser(null);
                      setIsFormOpen(true);
                    }}
                    className="inline-flex items-center px-4 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-gradient-to-r from-purple-600 to-purple-700 hover:from-purple-700 hover:to-purple-800 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-purple-500"
                  >
                    <PlusCircleIcon className="-ml-1 mr-2 h-5 w-5" />
                    Create First User
                  </button>
                </div>
              </div>
            )}
          </div>
        </div>

        {isFormOpen && (
          <UserForm
            user={currentUser}
            onClose={() => setIsFormOpen(false)}
            onSuccess={handleSuccess}
          />
        )}
      </div>
    </div>
  );
}