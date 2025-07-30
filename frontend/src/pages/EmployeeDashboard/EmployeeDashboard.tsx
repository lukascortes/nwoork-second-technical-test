import { useState, useEffect } from 'react';
import { timeOffService } from '../../api/timeOffService';
import type { TimeOffRequest } from '../../types/requestTypes';
import EmployeeNavbar from '../../components/layout/Navbar';
import RequestsTable from '../../components/requests/RequestsTable';
import LoadingSpinner from '../../components/common/LoadingSpinner';
import ErrorMessage from '../../components/common/ErrorMessage';
import RequestFormContainer from '../../components/requests/RequestForm/RequestFormContainer';
import { convertTypeToNumber } from '../../utils/requestConverters';
import type { TimeOffType } from '../../types/requestTypes';

import {
  ClockIcon,
  CalendarIcon,
  PlusCircleIcon,
  CheckCircleIcon,
  XCircleIcon
} from '@heroicons/react/24/outline';


import {
  HomeIcon,
  UserIcon
} from '@heroicons/react/24/solid';

export default function EmployeeDashboard() {
  const [requests, setRequests] = useState<TimeOffRequest[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [activeTab, setActiveTab] = useState<'requests' | 'new'>('requests');
  const [successMessage, setSuccessMessage] = useState('');

  useEffect(() => {
    const loadRequests = async () => {
      try {
        const data = await timeOffService.getMyRequests();
        setRequests(data);
      } catch (err) {
        setError('Failed to load your requests');
      } finally {
        setLoading(false);
      }
    };



    loadRequests();
  }, []);

  const handleNewRequest = async (requestData: {
    startDate: string;
    endDate: string;
    type: TimeOffType;
    reason?: string;
  }) => {
    setError(''); 
    setSuccessMessage(''); 
    const hasOverlappingRequest = requests.some(request => {
      const newStart = new Date(requestData.startDate);
      const newEnd = new Date(requestData.endDate);
      const existingStart = new Date(request.startDate);
      const existingEnd = new Date(request.endDate);

      return (
        (request.status === 'Pending' || request.status === 'Approved') &&
        ((newStart >= existingStart && newStart <= existingEnd) ||
          (newEnd >= existingStart && newEnd <= existingEnd) ||
          (newStart <= existingStart && newEnd >= existingEnd))
      );
    });

    if (hasOverlappingRequest) {
      setError('Ya tienes una solicitud aprobada/pendiente para estas fechas');
      return;
    }
    try {
      const userId = Number(localStorage.getItem('userId'));
      if (!userId) throw new Error('User not authenticated');

      const backendRequest = {
        userId,
        startDate: new Date(requestData.startDate).toISOString(),
        endDate: new Date(requestData.endDate).toISOString(),
        type: convertTypeToNumber(requestData.type),
        reason: requestData.reason || null,
        status: 1 
      };

      const newRequest = await timeOffService.createRequest(backendRequest);
      setRequests([...requests, newRequest]);
      setSuccessMessage('Request created successfully!');

    } catch (error: unknown) {
      let errorMessage = 'Error al crear la solicitud';

      
      if (typeof error === 'object' && error !== null && 'response' in error) {
        const axiosError = error as {
          response?: {
            data?: string | { message?: string };
            status?: number;
          }
        };

        
        if (typeof axiosError.response?.data === 'string') {
          errorMessage = axiosError.response.data;
        }
       
        else if (axiosError.response?.data && typeof axiosError.response.data === 'object') {
          errorMessage = axiosError.response.data.message || errorMessage;
        }
      }
    
      else if (error instanceof Error) {
        errorMessage = error.message;
      }

      setError(errorMessage);

    
      console.error('Detalles del error:', error);
    }
  };
  if (loading) return <LoadingSpinner />;


  return (
    <div className="min-h-screen bg-gradient-to-b from-purple-50 to-gray-50">
      <EmployeeNavbar />

      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">


        
        {successMessage && (
          <div className="mb-6 p-4 bg-green-50 rounded-lg border border-green-200 flex items-start">
            <CheckCircleIcon className="h-5 w-5 text-green-500 mr-3 mt-0.5" />
            <p className="text-green-800">{successMessage}</p>
          </div>
        )}

      
        <div className="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden mb-8">
          <div className="border-b border-gray-200">
            <nav className="flex -mb-px">
              <button
                onClick={() => setActiveTab('requests')}
                className={`py-4 px-6 text-center border-b-2 font-medium text-sm flex items-center ${activeTab === 'requests' ? 'border-purple-500 text-purple-600' : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'}`}
              >
                <CalendarIcon className="h-5 w-5 mr-2" />
                My Time Off Requests
                {requests.length > 0 && (
                  <span className="ml-2 bg-purple-100 text-purple-800 text-xs font-semibold px-2 py-0.5 rounded-full">
                    {requests.length}
                  </span>
                )}
              </button>
              <button
                onClick={() => setActiveTab('new')}
                className={`py-4 px-6 text-center border-b-2 font-medium text-sm flex items-center ${activeTab === 'new' ? 'border-purple-500 text-purple-600' : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'}`}
              >
                <PlusCircleIcon className="h-5 w-5 mr-2" />
                New Request
              </button>
            </nav>
          </div>
        </div>

       
        {activeTab === 'requests' ? (
          requests.length > 0 ? (
            <div className="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden">
              <div className="p-6">
                <div className="flex items-center justify-between mb-6">
                  <h2 className="text-xl font-semibold text-gray-800 flex items-center">
                    <ClockIcon className="h-6 w-6 text-purple-500 mr-2" />
                    My Requests History
                  </h2>
                  <span className="text-sm text-gray-500">
                    Showing {requests.length} request{requests.length !== 1 ? 's' : ''}
                  </span>
                </div>
                <RequestsTable requests={requests} />
              </div>
            </div>
          ) : (
            <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-12 text-center">
              <CalendarIcon className="mx-auto h-12 w-12 text-gray-400" />
              <h3 className="mt-2 text-lg font-medium text-gray-900">No requests yet</h3>
              <p className="mt-1 text-sm text-gray-500">
                You haven't submitted any time off requests yet.
              </p>
              <div className="mt-6">
                <button
                  onClick={() => setActiveTab('new')}
                  className="inline-flex items-center px-4 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-gradient-to-r from-purple-600 to-purple-700 hover:from-purple-700 hover:to-purple-800 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-purple-500"
                >
                  <PlusCircleIcon className="-ml-1 mr-2 h-5 w-5" />
                  Create New Request
                </button>
              </div>
            </div>
          )
        ) : (
          <div className="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden">
            <div className="p-6">
              <h2 className="text-xl font-semibold text-gray-800 flex items-center mb-6">
                <PlusCircleIcon className="h-6 w-6 text-purple-500 mr-2" />
                Create New Request
              </h2>
              {error && (
                <div className="mb-6 p-4 bg-red-50 rounded-lg border border-red-200 flex items-start">
                  <XCircleIcon className="h-5 w-5 text-red-500 mr-3 mt-0.5" />
                  <p className="text-red-800">{error}</p>
                </div>
              )}
              <RequestFormContainer onSubmit={handleNewRequest} />
            </div>
          </div>
        )}
      </div>
    </div>
  );
}