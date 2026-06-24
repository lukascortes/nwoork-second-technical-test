import apiClient from './apiClient';
import { getApiErrorMessage } from './errors';
import type { TimeOffRequest, RequestStatus, CreateTimeOffRequest } from '../types/requestTypes';

export const timeOffService = {
  getAllRequests: async (): Promise<TimeOffRequest[]> => {
    const { data } = await apiClient.get<TimeOffRequest[]>('/timeoffrequests');
    return data;
  },

  getMyRequests: async (): Promise<TimeOffRequest[]> => {
    const { data } = await apiClient.get<TimeOffRequest[]>('/timeoffrequests/me');
    return data;
  },

  createRequest: async (request: CreateTimeOffRequest): Promise<TimeOffRequest> => {
    try {
      const { data } = await apiClient.post<TimeOffRequest>('/timeoffrequests', request);
      return data;
    } catch (error) {
      throw new Error(getApiErrorMessage(error, 'Failed to create request'));
    }
  },

  updateStatus: async (id: string, status: RequestStatus): Promise<TimeOffRequest> => {
    try {
      const { data } = await apiClient.put<TimeOffRequest>(`/timeoffrequests/${id}/status`, { status });
      return data;
    } catch (error) {
      throw new Error(getApiErrorMessage(error, 'Failed to update request status'));
    }
  },
};
