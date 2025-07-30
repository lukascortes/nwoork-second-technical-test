import type { TimeOffType, RequestStatus } from '../types/requestTypes';


export const convertTypeToNumber = (type: TimeOffType): number => {
  const map: Record<TimeOffType, number> = {
    'Vacation': 0,
    'Sick': 1,
    'Other': 2
  };
  return map[type];
};

export const convertStatusToNumber = (status: RequestStatus): number => {
  const map: Record<RequestStatus, number> = {
    'Pending': 0,
    'Approved': 1,
    'Rejected': 2
  };
  return map[status];
};


export const convertNumberToType = (typeNumber: number): TimeOffType => {
  const map: Record<number, TimeOffType> = {
    0: 'Vacation',
    1: 'Sick',
    2: 'Other'
  };
  return map[typeNumber] || 'Other';
};

export const convertNumberToStatus = (statusNumber: number): RequestStatus => {
  const map: Record<number, RequestStatus> = {
    0: 'Pending',
    1: 'Approved',
    2: 'Rejected'
  };
  return map[statusNumber] || 'Pending'; 
};