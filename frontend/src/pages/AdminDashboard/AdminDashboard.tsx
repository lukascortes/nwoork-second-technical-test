import { useAdminDashboard } from '../AdminDashboard/useAdminDashboard';
import RequestsTable from '../../components/requests/RequestsTable';
import Navbar from '../../components/layout/AdminNavbar';
import LoadingSpinner from '../../components/common/LoadingSpinner';
import ErrorMessage from '../../components/common/ErrorMessage';
import { ClipboardDocumentListIcon, UsersIcon, CalendarIcon } from '@heroicons/react/24/outline';

export default function AdminDashboard() {
  const { 
    requests, 
    loading, 
    error,
    handleStatusChange 
    
  } = useAdminDashboard();

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
       
        <div className="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden mb-8">
          <div className="p-6">
            <div className="flex items-center justify-between">
              <div>
                <h2 className="text-2xl font-semibold text-gray-800 flex items-center">
                  <ClipboardDocumentListIcon className="h-6 w-6 text-purple-500 mr-2" />
                  Admin Dashboard
                </h2>
                <p className="mt-1 text-sm text-gray-500">
                  View all time off requests
                </p>
              </div>
              
            </div>
          </div>
        </div>

       
        <div className="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden">
          <div className="p-6">
            <div className="flex items-center justify-between mb-6">
              <h3 className="text-lg font-medium text-gray-900 flex items-center">
                <CalendarIcon className="h-5 w-5 text-purple-500 mr-2" />
                All Time Off Requests
              </h3>
            </div>

            <RequestsTable 
              requests={requests}
              onStatusChange={handleStatusChange} 
              isAdmin={true}
              loading={loading}
              error={error}
            />
          </div>
        </div>
      </div>
    </div>
  );
}