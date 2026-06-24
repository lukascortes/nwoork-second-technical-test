import { useState } from 'react';
import {
  startOfMonth,
  endOfMonth,
  eachDayOfInterval,
  startOfWeek,
  endOfWeek,
  format,
  isWithinInterval,
  parseISO,
  isSameMonth,
  isToday,
  addMonths,
  subMonths,
} from 'date-fns';
import { ChevronLeftIcon, ChevronRightIcon } from '@heroicons/react/24/outline';
import type { TimeOffRequest, TimeOffType } from '../../types/requestTypes';

const typeChip: Record<TimeOffType, string> = {
  Vacation: 'bg-purple-100 text-purple-700',
  Sick: 'bg-blue-100 text-blue-700',
  Other: 'bg-gray-100 text-gray-700',
};

const weekDays = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];

export default function TeamCalendar({ requests }: { requests: TimeOffRequest[] }) {
  const [month, setMonth] = useState(new Date());

  const days = eachDayOfInterval({
    start: startOfWeek(startOfMonth(month)),
    end: endOfWeek(endOfMonth(month)),
  });

  const approved = requests.filter((r) => r.status === 'Approved');
  const absencesOn = (day: Date) =>
    approved.filter((r) =>
      isWithinInterval(day, { start: parseISO(r.startDate), end: parseISO(r.endDate) })
    );

  return (
    <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
      <div className="flex items-center justify-between mb-4">
        <button onClick={() => setMonth(subMonths(month, 1))} className="p-2 rounded hover:bg-gray-100" aria-label="Previous month">
          <ChevronLeftIcon className="h-5 w-5 text-gray-600" />
        </button>
        <h3 className="text-lg font-semibold text-gray-800">{format(month, 'MMMM yyyy')}</h3>
        <button onClick={() => setMonth(addMonths(month, 1))} className="p-2 rounded hover:bg-gray-100" aria-label="Next month">
          <ChevronRightIcon className="h-5 w-5 text-gray-600" />
        </button>
      </div>

      <div className="grid grid-cols-7 gap-1 text-center text-xs font-medium text-gray-500 mb-1">
        {weekDays.map((d) => <div key={d} className="py-1">{d}</div>)}
      </div>

      <div className="grid grid-cols-7 gap-1">
        {days.map((day) => {
          const inMonth = isSameMonth(day, month);
          const items = absencesOn(day);
          return (
            <div
              key={day.toISOString()}
              className={`min-h-[88px] rounded-lg border p-1 ${inMonth ? 'bg-white border-gray-100' : 'bg-gray-50 border-gray-50'} ${isToday(day) ? 'ring-2 ring-purple-300' : ''}`}
            >
              <div className={`text-xs ${inMonth ? 'text-gray-400' : 'text-gray-300'}`}>{format(day, 'd')}</div>
              <div className="space-y-0.5 mt-0.5">
                {items.slice(0, 3).map((r) => (
                  <div
                    key={r.id}
                    className={`truncate rounded px-1 py-0.5 text-[10px] ${typeChip[r.type]}`}
                    title={`${r.user?.fullName ?? 'Employee'} — ${r.type}`}
                  >
                    {r.user?.fullName?.split(' ')[0] ?? 'Off'}
                  </div>
                ))}
                {items.length > 3 && <div className="text-[10px] text-gray-400">+{items.length - 3} more</div>}
              </div>
            </div>
          );
        })}
      </div>

      <div className="flex gap-4 mt-4 text-xs text-gray-500">
        <span className="flex items-center"><span className="w-3 h-3 rounded bg-purple-200 mr-1" />Vacation</span>
        <span className="flex items-center"><span className="w-3 h-3 rounded bg-blue-200 mr-1" />Sick</span>
        <span className="flex items-center"><span className="w-3 h-3 rounded bg-gray-200 mr-1" />Other</span>
      </div>
    </div>
  );
}
