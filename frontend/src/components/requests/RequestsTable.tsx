import type { TimeOffRequest, RequestStatus } from '../../types/requestTypes';
import { CheckCircleIcon, XCircleIcon, ClockIcon } from '@heroicons/react/24/outline';

interface RequestsTableProps {
  requests: TimeOffRequest[];
  onStatusChange?: (id: string, status: RequestStatus) => void;
  isAdmin?: boolean;
  loading?: boolean;
  error?: string | null;
}

export default function RequestsTable({
  requests = [],
  onStatusChange,
  isAdmin = false,
  loading = false,
  error = null,
}: RequestsTableProps) {
  if (loading) return (
    <div className="flex justify-center py-8">
      <div className="animate-spin rounded-full h-12 w-12 border-t-2 border-b-2 border-purple-500"></div>
    </div>
  );

  if (error) return (
    <div className="bg-red-50 rounded-lg p-4 border border-red-200 text-red-800">
      {error}
    </div>
  );

  return (
    <div className="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden">
      <div className="overflow-x-auto">
        <table className="min-w-full divide-y divide-gray-200">
          <thead className="bg-purple-50">
            <tr>
              <th className="px-6 py-3 text-left text-xs font-medium text-purple-800 uppercase tracking-wider">Dates</th>
              <th className="px-6 py-3 text-left text-xs font-medium text-purple-800 uppercase tracking-wider">Days</th>
              <th className="px-6 py-3 text-left text-xs font-medium text-purple-800 uppercase tracking-wider">Type</th>
              <th className="px-6 py-3 text-left text-xs font-medium text-purple-800 uppercase tracking-wider">Reason</th>
              <th className="px-6 py-3 text-left text-xs font-medium text-purple-800 uppercase tracking-wider">Status</th>
              {isAdmin && (
                <th className="px-6 py-3 text-left text-xs font-medium text-purple-800 uppercase tracking-wider">Employee</th>
              )}
              {isAdmin && onStatusChange && (
                <th className="px-6 py-3 text-left text-xs font-medium text-purple-800 uppercase tracking-wider">Actions</th>
              )}
            </tr>
          </thead>
          <tbody className="bg-white divide-y divide-gray-200">
            {requests.length > 0 ? (
              requests.map((request) => (
                <tr key={request.id} className="hover:bg-purple-50 transition-colors">
                  <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
                    {new Date(request.startDate).toLocaleDateString()} - {new Date(request.endDate).toLocaleDateString()}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                    {request.totalDays}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                    {request.type}
                  </td>
                  <td className="px-6 py-4 text-sm text-gray-900 max-w-xs">
                    <div className="line-clamp-2 hover:line-clamp-none">
                      {request.reason || 'No reason provided'}
                    </div>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <div className="flex items-center">
                      {request.status === 'Approved' && <CheckCircleIcon className="h-4 w-4 text-green-500 mr-1" />}
                      {request.status === 'Rejected' && <XCircleIcon className="h-4 w-4 text-red-500 mr-1" />}
                      {request.status === 'Pending' && <ClockIcon className="h-4 w-4 text-yellow-500 mr-1" />}
                      <span className={`px-2 inline-flex text-xs leading-5 font-semibold rounded-full
                        ${request.status === 'Approved' ? 'bg-green-100 text-green-800' :
                          request.status === 'Rejected' ? 'bg-red-100 text-red-800' :
                          'bg-yellow-100 text-yellow-800'}`}>
                        {request.status}
                      </span>
                    </div>
                  </td>
                  {isAdmin && (
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                      <div className="font-medium text-gray-900">{request.user?.fullName}</div>
                      <div className="text-xs text-gray-500">{request.user?.email}</div>
                    </td>
                  )}
                  {isAdmin && onStatusChange && request.status === 'Pending' && (
                    <td className="px-6 py-4 whitespace-nowrap space-x-2">
                      <button
                        onClick={() => onStatusChange(request.id, 'Approved')}
                        className="inline-flex items-center px-3 py-1 border border-transparent rounded-md shadow-sm text-xs font-medium text-white bg-gradient-to-r from-green-600 to-green-700 hover:from-green-700 hover:to-green-800 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-green-500"
                      >
                        Approve
                      </button>
                      <button
                        onClick={() => onStatusChange(request.id, 'Rejected')}
                        className="inline-flex items-center px-3 py-1 border border-transparent rounded-md shadow-sm text-xs font-medium text-white bg-gradient-to-r from-red-600 to-red-700 hover:from-red-700 hover:to-red-800 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-red-500"
                      >
                        Reject
                      </button>
                    </td>
                  )}
                </tr>
              ))
            ) : (
              <tr>
                <td colSpan={isAdmin ? 7 : 5} className="px-6 py-8 text-center">
                  <div className="text-gray-500">No time off requests found</div>
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}
