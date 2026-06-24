import { useEffect, useState } from 'react';
import { timeOffService } from '../../api/timeOffService';
import { getApiErrorMessage } from '../../api/errors';
import type { TimeOffRequest, RequestStatus, RequestStats } from '../../types/requestTypes';

export const useAdminDashboard = () => {
  const [requests, setRequests] = useState<TimeOffRequest[]>([]);
  const [stats, setStats] = useState<RequestStats | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  const refreshStats = async () => {
    try {
      setStats(await timeOffService.getStats());
    } catch {
      // metrics are non-critical; ignore transient errors
    }
  };

  useEffect(() => {
    const load = async () => {
      try {
        const [allRequests] = await Promise.all([
          timeOffService.getAllRequests(),
          refreshStats(),
        ]);
        setRequests(allRequests);
      } catch (err) {
        setError(getApiErrorMessage(err, 'Failed to load requests'));
      } finally {
        setLoading(false);
      }
    };

    load();
  }, []);

  const handleStatusChange = async (id: string, status: RequestStatus) => {
    const previous = requests;
    setRequests((prev) => prev.map((req) => (req.id === id ? { ...req, status } : req)));

    try {
      const updated = await timeOffService.updateStatus(id, status);
      setRequests((prev) => prev.map((req) => (req.id === id ? updated : req)));
      refreshStats();
    } catch (err) {
      setError(getApiErrorMessage(err, 'Failed to update status'));
      setRequests(previous); // rollback
    }
  };

  return { requests, stats, loading, error, handleStatusChange };
};
