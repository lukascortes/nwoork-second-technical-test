import { useEffect, useState } from 'react';
import { timeOffService } from '../../api/timeOffService';
import { getApiErrorMessage } from '../../api/errors';
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
        setError(getApiErrorMessage(err, 'Failed to load requests'));
      } finally {
        setLoading(false);
      }
    };

    loadRequests();
  }, []);

  const handleStatusChange = async (id: string, status: RequestStatus) => {
    const previous = requests;
    // optimistic update
    setRequests((prev) => prev.map((req) => (req.id === id ? { ...req, status } : req)));

    try {
      const updated = await timeOffService.updateStatus(id, status);
      setRequests((prev) => prev.map((req) => (req.id === id ? updated : req)));
    } catch (err) {
      setError(getApiErrorMessage(err, 'Failed to update status'));
      setRequests(previous); // rollback
    }
  };

  return { requests, loading, error, handleStatusChange };
};
