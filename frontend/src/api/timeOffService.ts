import apiClient from './apiClient';
import type { TimeOffRequest, RequestStatus, CreateTimeOffRequest } from '../types/requestTypes';
import {
  convertNumberToType,
  convertNumberToStatus, convertStatusToNumber
} from '../utils/requestConverters';

export const timeOffService = {
  getAllRequests: async (): Promise<TimeOffRequest[]> => {
    const response = await apiClient.get('/timeoffrequests/all');
    console.log('Response from getAllRequests:', response);
    return response.data;
  },
  getMyRequests: async (): Promise<TimeOffRequest[]> => {
    const response = await apiClient.get('/timeoffrequests/my');
    return response.data;
  },
  createRequest: async (request: CreateTimeOffRequest): Promise<TimeOffRequest> => {
    const response = await apiClient.post('/timeoffrequests', request);
    return {
      ...response.data,
      type: convertNumberToType(response.data.type),
      status: convertNumberToStatus(response.data.status)
    };
  },
  updateStatus: async (id: number, status: RequestStatus): Promise<TimeOffRequest> => {
    try {
      
      const numericStatus = convertStatusToNumber(status);

      const response = await apiClient.put(`/timeoffrequests/${id}/status`, {
        status: numericStatus 
      });

      return {
        ...response.data,
        status: convertNumberToStatus(response.data.status) 
      };
    } catch (error) {
      console.error('Error updating status:', {
        request: { id, status },
        response: (error as any).response?.data
      });
      throw new Error((error as any).response?.data?.message || 'Failed to update request status');
    }
  },
};