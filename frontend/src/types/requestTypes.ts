export type TimeOffType = 'Vacation' | 'Sick' | 'Other';
export type RequestStatus = 'Pending' | 'Approved' | 'Rejected';

export interface TimeOffRequestUser {
  id: string;
  email: string;
  fullName: string;
  role: 'Admin' | 'Employee';
}

// Front-end representation of a time-off request (enums come as strings from the API).
export interface TimeOffRequest {
  id: string;
  startDate: string; // ISO date, e.g. "2026-07-01"
  endDate: string;
  type: TimeOffType;
  reason?: string | null;
  status: RequestStatus;
  totalDays: number;
  createdAt: string;
  reviewedAt?: string | null;
  user?: TimeOffRequestUser;
}

// Payload for POST /api/timeoffrequests — the server assigns owner & status.
export interface CreateTimeOffRequest {
  startDate: string;
  endDate: string;
  type: TimeOffType;
  reason?: string | null;
}

export interface VacationBalance {
  annualAllowance: number;
  usedDays: number;
  pendingDays: number;
  remainingDays: number;
  projectedRemainingDays: number;
}
