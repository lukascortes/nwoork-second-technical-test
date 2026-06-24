import { useEffect, useState } from 'react';
import { useUserManagement } from '../../../hooks/useUserManagement';
import UserActions from './UserActions';
import type { User } from '../../../types/userTypes';

interface UserListProps {
  onEdit: (user: User) => void;
  users?: User[];
  loading: boolean;
  error: string | null;
  onDelete: (id: string) => void;
}
export default function UserList({ 
  onEdit, 
  users = [], 
  loading, 
  error, 
  onDelete 
}: UserListProps) {
  const [currentPage, setCurrentPage] = useState(1);
  const usersPerPage = 10;

  const getRoleDisplay = (roleValue: number | string): string => {
    if (typeof roleValue === 'number') {
      return roleValue === 0 ? 'Admin' : 'Employee';
    }
    return roleValue;
  };

  const getRoleValue = (role: number | string): number => {
    if (typeof role === 'number') {
      return role;
    }
    return role === 'Admin' ? 0 : 1;
  };

  if (loading) return <div className="text-center py-8">Loading users...</div>;
  if (error) return <div className="text-red-500 py-8">Error: {error}</div>;

  const indexOfLastUser = currentPage * usersPerPage;
  const indexOfFirstUser = indexOfLastUser - usersPerPage;
  const currentUsers = users.slice(indexOfFirstUser, indexOfLastUser);

  return (
    <div className="bg-white shadow rounded-lg overflow-hidden">
      <div className="overflow-x-auto">
        <table className="min-w-full divide-y divide-gray-200">
          <thead className="bg-purple-50">
            <tr>
              <th className="px-6 py-3 text-left text-xs font-medium text-purple-800 uppercase tracking-wider">Name</th>
              <th className="px-6 py-3 text-left text-xs font-medium text-purple-800 uppercase tracking-wider">Email</th>
              <th className="px-6 py-3 text-left text-xs font-medium text-purple-800 uppercase tracking-wider">Role</th>
              <th className="px-6 py-3 text-left text-xs font-medium text-purple-800 uppercase tracking-wider">Actions</th>
            </tr>
          </thead>
          <tbody className="bg-white divide-y divide-gray-200">
            {currentUsers.map((user) => (
              <tr key={user.id} className="hover:bg-purple-50">
                <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">{user.fullName}</td>
                <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">{user.email}</td>
                <td className="px-6 py-4 whitespace-nowrap text-sm">
                  <span className={`px-2 inline-flex text-xs leading-5 font-semibold rounded-full 
                      ${getRoleValue(user.role) === 0 ? 'bg-purple-100 text-purple-800' : 'bg-red-100 text-red-800'}`}>
                      {getRoleDisplay(user.role)}
                    </span>
                </td>
                <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                  <UserActions
                    user={user}
                    onEdit={() => onEdit(user)}
                    onDelete={() => onDelete(user.id)}
                  />
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

     
      <div className="bg-white px-4 py-3 flex items-center justify-between border-t border-gray-200 sm:px-6">
        <div className="flex-1 flex justify-between sm:hidden">
          <button
            onClick={() => setCurrentPage(prev => Math.max(prev - 1, 1))}
            disabled={currentPage === 1}
            className="relative inline-flex items-center px-4 py-2 border border-gray-300 text-sm font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50"
          >
            Previous
          </button>
          <button
            onClick={() => setCurrentPage(prev => prev + 1)}
            disabled={indexOfLastUser >= users.length}
            className="ml-3 relative inline-flex items-center px-4 py-2 border border-gray-300 text-sm font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50"
          >
            Next
          </button>
        </div>
      </div>
    </div>
  );
}