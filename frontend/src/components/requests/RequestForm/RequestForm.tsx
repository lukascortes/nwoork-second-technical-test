import { useFormik } from 'formik';
import { useState } from 'react';
import { validationSchema } from './validationSchema';
import type { TimeOffType } from '../../../types/requestTypes';
import {
  FiCalendar as CalendarDaysIcon,
  FiAlertCircle as AlertCircleIcon
} from 'react-icons/fi';
import {
  FaUmbrellaBeach as VacationIcon,
  FaProcedures as SickIcon,
  FaQuestionCircle as OtherIcon
} from 'react-icons/fa';

interface RequestFormProps {
  onSubmit: (values: {
    startDate: string;
    endDate: string;
    type: 'Vacation' | 'Sick' | 'Other';
    reason?: string;
  }) => void;
    onTypeChange?: (type: TimeOffType) => void;
}




export default function RequestForm({ onSubmit, onTypeChange }: RequestFormProps) {
  const [showIllustration, setShowIllustration] = useState(true);
  const formik = useFormik({
    initialValues: {
      startDate: '',
      endDate: '',
      type: 'Vacation' as TimeOffType,
      reason: ''
    },
    validationSchema,
    onSubmit: (values) => {
      onSubmit(values);
    }
  });

  const typeOptions = [
    { value: 'Vacation', label: 'Vacation', icon: <VacationIcon className="h-5 w-5 mr-2" /> },
    { value: 'Sick', label: 'Sick Leave', icon: <SickIcon className="h-5 w-5 mr-2" /> },
    { value: 'Other', label: 'Other', icon: <OtherIcon className="h-5 w-5 mr-2" /> }
  ];
    const handleTypeSelection = (e: React.ChangeEvent<HTMLInputElement>) => {
    const type = e.target.value as TimeOffType;
    formik.handleChange(e); 
    onTypeChange?.(type);   
  };
  return (

    <form onSubmit={formik.handleSubmit} className="space-y-6">
      <div className="grid grid-cols-1 gap-6 sm:grid-cols-2">
        <div className="relative">
          <label htmlFor="startDate" className="block text-sm font-medium text-gray-700 mb-1 flex items-center">
            <CalendarDaysIcon className="h-4 w-4 text-purple-500 mr-1" />
            Start Date
          </label>
          <input
            type="date"
            id="startDate"
            name="startDate"
            onChange={formik.handleChange}
            onBlur={formik.handleBlur}
            value={formik.values.startDate}
            className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-purple-500 focus:ring-purple-500 py-2 px-3 border pl-10"
          />
          {formik.touched.startDate && formik.errors.startDate && (
            <p className="mt-1 text-sm text-red-600 flex items-center">
              <AlertCircleIcon className="h-4 w-4 mr-1" />
              {formik.errors.startDate}
            </p>
          )}
        </div>

        <div className="relative">
          <label htmlFor="endDate" className="block text-sm font-medium text-gray-700 mb-1 flex items-center">
            <CalendarDaysIcon className="h-4 w-4 text-purple-500 mr-1" />
            End Date
          </label>
          <input
            type="date"
            id="endDate"
            name="endDate"
            onChange={formik.handleChange}
            onBlur={formik.handleBlur}
            value={formik.values.endDate}
            className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-purple-500 focus:ring-purple-500 py-2 px-3 border pl-10"
          />
          {formik.touched.endDate && formik.errors.endDate && (
            <p className="mt-1 text-sm text-red-600 flex items-center">
              <AlertCircleIcon className="h-4 w-4 mr-1" />
              {formik.errors.endDate}
            </p>
          )}
        </div>
      </div>

      <div>
        <label htmlFor="type" className="block text-sm font-medium text-gray-700 mb-1">
          Request Type
        </label>
        <div className="grid grid-cols-1 gap-3 sm:grid-cols-3">
          {typeOptions.map((option) => (
            <label
              key={option.value}
              className={`flex items-center p-3 border rounded-lg cursor-pointer transition-colors ${formik.values.type === option.value
                ? 'border-purple-500 bg-purple-50'
                : 'border-gray-300 hover:border-purple-300'
                }`}
              onClick={() => setShowIllustration(true)}
            >
              <input
                type="radio"
                id={`type-${option.value}`}
                name="type"
                value={option.value}
                checked={formik.values.type === option.value}
                onChange={handleTypeSelection}
                onBlur={formik.handleBlur}
                className="h-4 w-4 text-purple-600 focus:ring-purple-500 border-gray-300"
              />
              <div className="ml-3 flex items-center">
                {option.icon}
                <span className="block text-sm text-gray-700">{option.label}</span>
              </div>
            </label>
          ))}
        </div>
      </div>

      <div>
        <label htmlFor="reason" className="block text-sm font-medium text-gray-700 mb-1">
          Reason (Optional)
        </label>
        <textarea
          id="reason"
          name="reason"
          rows={4}
          onChange={formik.handleChange}
          onBlur={formik.handleBlur}
          value={formik.values.reason}
          className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-purple-500 focus:ring-purple-500 py-2 px-3 border"
          placeholder="✍️ Please provide details about your request..."
        />
      </div>

      <div className="flex justify-end space-x-3">
        <button
          type="button"
          onClick={() => formik.resetForm()}
          className="inline-flex items-center px-4 py-2 border border-gray-300 rounded-md shadow-sm text-sm font-medium text-gray-700 bg-white hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-purple-500"
        >
          Reset
        </button>
        <button
          type="submit"
          className="inline-flex items-center px-4 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-gradient-to-r from-purple-600 to-purple-700 hover:from-purple-700 hover:to-purple-800 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-purple-500 transition-all duration-150 transform hover:scale-[1.02]"
        >
          Submit Request
          <svg className="ml-2 -mr-1 w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M5 12h14M12 5l7 7-7 7" />
          </svg>
        </button>
      </div>
    </form>

  );
}

