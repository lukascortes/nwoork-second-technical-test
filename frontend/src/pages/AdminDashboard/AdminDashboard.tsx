import { useState } from 'react';
import { useAdminDashboard } from '../AdminDashboard/useAdminDashboard';
import RequestsTable from '../../components/requests/RequestsTable';
import MetricsPanel from '../../components/admin/MetricsPanel';
import TeamCalendar from '../../components/requests/TeamCalendar';
import Navbar from '../../components/layout/AdminNavbar';
import LoadingSpinner from '../../components/common/LoadingSpinner';
import ErrorMessage from '../../components/common/ErrorMessage';
import { ClipboardDocumentListIcon, ChartBarIcon, CalendarIcon } from '@heroicons/react/24/outline';

type Tab = 'requests' | 'metrics' | 'calendar';

export default function AdminDashboard() {
  const { requests, stats, loading, error, handleStatusChange } = useAdminDashboard();
  const [tab, setTab] = useState<Tab>('requests');

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

  const tabs: { id: Tab; label: string; icon: React.ReactNode }[] = [
    { id: 'requests', label: 'Requests', icon: <ClipboardDocumentListIcon className="h-5 w-5 mr-2" /> },
    { id: 'metrics', label: 'Metrics', icon: <ChartBarIcon className="h-5 w-5 mr-2" /> },
    { id: 'calendar', label: 'Team Calendar', icon: <CalendarIcon className="h-5 w-5 mr-2" /> },
  ];

  return (
    <div className="min-h-screen bg-gradient-to-b from-purple-50 to-gray-50">
      <Navbar />

      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <div className="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden mb-8">
          <div className="border-b border-gray-200">
            <nav className="flex -mb-px">
              {tabs.map((t) => (
                <button
                  key={t.id}
                  onClick={() => setTab(t.id)}
                  className={`py-4 px-6 text-center border-b-2 font-medium text-sm flex items-center ${
                    tab === t.id
                      ? 'border-purple-500 text-purple-600'
                      : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
                  }`}
                >
                  {t.icon}
                  {t.label}
                </button>
              ))}
            </nav>
          </div>
        </div>

        {tab === 'requests' && (
          <RequestsTable
            requests={requests}
            onStatusChange={handleStatusChange}
            isAdmin
            loading={loading}
            error={error}
          />
        )}

        {tab === 'metrics' && stats && <MetricsPanel stats={stats} />}

        {tab === 'calendar' && <TeamCalendar requests={requests} />}
      </div>
    </div>
  );
}
