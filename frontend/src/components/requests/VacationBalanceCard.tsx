import type { VacationBalance } from '../../types/requestTypes';
import {
  SparklesIcon,
  CheckCircleIcon,
  ClockIcon,
  CalendarIcon,
} from '@heroicons/react/24/outline';

interface Props {
  balance: VacationBalance;
}

export default function VacationBalanceCard({ balance }: Props) {
  const stats = [
    { label: 'Available', value: balance.remainingDays, color: 'text-green-600', icon: <SparklesIcon className="h-5 w-5 text-green-500" /> },
    { label: 'Used', value: balance.usedDays, color: 'text-purple-600', icon: <CheckCircleIcon className="h-5 w-5 text-purple-500" /> },
    { label: 'Pending', value: balance.pendingDays, color: 'text-yellow-600', icon: <ClockIcon className="h-5 w-5 text-yellow-500" /> },
    { label: 'Annual allowance', value: balance.annualAllowance, color: 'text-gray-700', icon: <CalendarIcon className="h-5 w-5 text-gray-400" /> },
  ];

  return (
    <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-6 mb-8">
      <h2 className="text-lg font-semibold text-gray-800 mb-4 flex items-center">
        <SparklesIcon className="h-5 w-5 text-purple-500 mr-2" />
        Vacation Balance
      </h2>
      <div className="grid grid-cols-2 sm:grid-cols-4 gap-4">
        {stats.map((stat) => (
          <div key={stat.label} className="bg-gray-50 rounded-lg p-4">
            <div className="flex items-center justify-between">
              <span className="text-sm text-gray-500">{stat.label}</span>
              {stat.icon}
            </div>
            <p className={`mt-2 text-2xl font-bold ${stat.color}`}>
              {stat.value}
              <span className="text-sm font-normal text-gray-400"> days</span>
            </p>
          </div>
        ))}
      </div>
      {balance.remainingDays <= 0 && (
        <p className="mt-4 text-sm text-red-600">You have no remaining vacation days for this year.</p>
      )}
    </div>
  );
}
