import type { User } from '../../../types/userTypes';
import { useState, useEffect } from 'react';

interface UserActionsProps {
  user: User;
  onEdit: () => void;
  onDelete: () => void;
}

export default function UserActions({ user, onEdit, onDelete }: UserActionsProps) {
  const [confirmDelete, setConfirmDelete] = useState(false);

  return (
    <div className="flex space-x-2">
      <button
        onClick={onEdit}
        className="text-purple-600 hover:text-purple-900"
        title="Edit"
      >
        <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
        </svg>
      </button>

      {confirmDelete ? (
        <>
          <button
            onClick={onDelete}
            className="text-red-600 hover:text-red-900"
            title="Confirm Delete"
          >
            <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M5 13l4 4L19 7" />
            </svg>
          </button>
          <button
            onClick={() => setConfirmDelete(false)}
            className="text-gray-600 hover:text-gray-900"
            title="Cancel"
          >
            <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        </>
      ) : (
        <button
          onClick={() => setConfirmDelete(true)}
          className="text-red-600 hover:text-red-900"
          title="Delete"
        >
          <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
          </svg>
        </button>
      )}
    </div>
  );
}