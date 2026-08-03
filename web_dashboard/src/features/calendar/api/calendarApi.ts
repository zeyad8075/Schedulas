import { apiClient } from '../../../core/api/apiClient';
import { DailyCalendarDto, WeeklyCalendarDto, MonthlyCalendarDto } from '../models';

export const calendarApi = {
  getDailyCalendar: async (date: string): Promise<DailyCalendarDto> => {
    const response = await apiClient.get<DailyCalendarDto>(`/api/v1/calendar/daily`, { params: { date } });
    return response.data;
  },

  getWeeklyCalendar: async (startDate: string): Promise<WeeklyCalendarDto> => {
    const response = await apiClient.get<WeeklyCalendarDto>(`/api/v1/calendar/weekly`, { params: { startDate } });
    return response.data;
  },

  getMonthlyCalendar: async (year: number, month: number): Promise<MonthlyCalendarDto> => {
    const response = await apiClient.get<MonthlyCalendarDto>(`/api/v1/calendar/monthly`, { params: { year, month } });
    return response.data;
  },
};
