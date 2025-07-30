import * as Yup from 'yup';
import type { TimeOffType } from '../../../types/requestTypes';

export const validationSchema = Yup.object().shape({
  startDate: Yup.date()
    .required('Start date is required')
    .min(new Date(), 'Start date cannot be in the past'),
  endDate: Yup.date()
    .required('End date is required')
    .min(Yup.ref('startDate'), 'End date must be after start date'),
  type: Yup.string()
    .oneOf<TimeOffType>(['Vacation', 'Sick', 'Other'])
    .required('Type is required'),
  reason: Yup.string().optional()
});