import { useQuery } from '@tanstack/react-query';
import { calendarApi } from '../api/calendarApi';

export const useDailyCalendar = (date: string) => {
  return useQuery({
    queryKey: ['calendar', 'daily', date],
    queryFn: () => calendarApi.getDailyCalendar(date),
    enabled: !!date,
  });
};

export const useWeeklyCalendar = (startDate: string) => {
  return useQuery({
    queryKey: ['calendar', 'weekly', startDate],
    queryFn: () => calendarApi.getWeeklyCalendar(startDate),
    enabled: !!startDate,
  });
};

export const useMonthlyCalendar = (year: number, month: number) => {
  return useQuery({
    queryKey: ['calendar', 'monthly', year, month],
    queryFn: () => calendarApi.getMonthlyCalendar(year, month),
    enabled: !!year && !!month,
  });
};
