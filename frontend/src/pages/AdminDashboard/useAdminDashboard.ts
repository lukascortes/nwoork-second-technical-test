import { useEffect, useState } from 'react';
import { timeOffService } from '../../api/timeOffService';
import type { TimeOffRequest, RequestStatus } from '../../types/requestTypes';

export const useAdminDashboard = () => {
  const [requests, setRequests] = useState<TimeOffRequest[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    const loadRequests = async () => {
      try {
        const data = await timeOffService.getAllRequests();
        setRequests(data);
      } catch (err) {
        setError(err instanceof Error ? err.message : 'Failed to load requests');
      } finally {
        setLoading(false);
      }
    };

    loadRequests();
  }, []);

  const handleStatusChange = async (id: number, status: RequestStatus) => {
    try {
      
      setRequests(requests.map(req =>
        req.id === id ? { ...req, status } : req
      ));

      const updatedRequest = await timeOffService.updateStatus(id, status);

    
      setRequests(requests.map(req =>
        req.id === id ? updatedRequest : req
      ));
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to update status');
     
      setRequests(requests);
    }
  };

  return {
    requests,
    loading,
    error,
    handleStatusChange,
  };
};