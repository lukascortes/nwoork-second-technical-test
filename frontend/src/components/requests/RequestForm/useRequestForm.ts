import { useState } from 'react';
import type { TimeOffType } from '../../../types/requestTypes';

interface RequestFormValues {
  startDate: string;
  endDate: string;
  type: TimeOffType;
  reason: string;
}

export const useRequestForm = (onSubmit: (values: RequestFormValues) => void) => {
  const [values, setValues] = useState<RequestFormValues>({
    startDate: '',
    endDate: '',
    type: 'Vacation',
    reason: ''
  });

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement>) => {
    const { name, value } = e.target;
    setValues(prev => ({ ...prev, [name]: value }));
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    onSubmit(values);
  };

  return {
    values,
    handleChange,
    handleSubmit,
  };
};