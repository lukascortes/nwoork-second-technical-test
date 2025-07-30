export type TimeOffType = 'Vacation' | 'Sick' | 'Other';
export type RequestStatus = 'Pending' | 'Approved' | 'Rejected';

// Specific type for time off requests (front end representation)
export interface TimeOffRequest {
  id: number;
  userId: number;
  startDate: string;
  endDate: string;
  type: TimeOffType; 
  reason?: string;
  status: RequestStatus; 
  createdAt: string;
  user?: {
    email: string;
    role: 'Admin' | 'Employee';
  };
}

// Specific type for creating a time off request (back end representation)
export interface CreateTimeOffRequest {
  userId: number;
  startDate: string;
  endDate: string;
  type: number; 
  reason?: string | null;
  status: number; 
}