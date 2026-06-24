import type { RequestStats } from '../../types/requestTypes';
import {
  ClipboardDocumentListIcon,
  ClockIcon,
  CheckCircleIcon,
  XCircleIcon,
} from '@heroicons/react/24/outline';

interface BarItem {
  label: string;
  value: number;
  color: string;
}

function BarGroup({ title, data }: { title: string; data: BarItem[] }) {
  const max = Math.max(1, ...data.map((d) => d.value));
  return (
    <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
      <h3 className="text-sm font-semibold text-gray-700 mb-4">{title}</h3>
      <div className="space-y-3">
        {data.map((d) => (
          <div key={d.label}>
            <div className="flex justify-between text-xs text-gray-500 mb-1">
              <span>{d.label}</span>
              <span className="font-medium">{d.value}</span>
            </div>
            <div className="h-2 bg-gray-100 rounded-full overflow-hidden">
              <div className={`h-full ${d.color} rounded-full transition-all`} style={{ width: `${(d.value / max) * 100}%` }} />
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}

export default function MetricsPanel({ stats }: { stats: RequestStats }) {
  const cards = [
    { label: 'Total', value: stats.total, icon: <ClipboardDocumentListIcon className="h-6 w-6 text-purple-500" />, color: 'text-gray-800' },
    { label: 'Pending', value: stats.pending, icon: <ClockIcon className="h-6 w-6 text-yellow-500" />, color: 'text-yellow-600' },
    { label: 'Approved', value: stats.approved, icon: <CheckCircleIcon className="h-6 w-6 text-green-500" />, color: 'text-green-600' },
    { label: 'Rejected', value: stats.rejected, icon: <XCircleIcon className="h-6 w-6 text-red-500" />, color: 'text-red-600' },
  ];

  const byType: BarItem[] = [
    { label: 'Vacation', value: stats.vacation, color: 'bg-purple-500' },
    { label: 'Sick', value: stats.sick, color: 'bg-blue-500' },
    { label: 'Other', value: stats.other, color: 'bg-gray-400' },
  ];

  const byStatus: BarItem[] = [
    { label: 'Pending', value: stats.pending, color: 'bg-yellow-400' },
    { label: 'Approved', value: stats.approved, color: 'bg-green-500' },
    { label: 'Rejected', value: stats.rejected, color: 'bg-red-500' },
  ];

  return (
    <div className="space-y-6">
      <div className="grid grid-cols-2 sm:grid-cols-4 gap-4">
        {cards.map((c) => (
          <div key={c.label} className="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
            <div className="flex items-center justify-between">
              <span className="text-sm text-gray-500">{c.label}</span>
              {c.icon}
            </div>
            <p className={`mt-2 text-3xl font-bold ${c.color}`}>{c.value}</p>
          </div>
        ))}
      </div>
      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        <BarGroup title="By type" data={byType} />
        <BarGroup title="By status" data={byStatus} />
      </div>
    </div>
  );
}
