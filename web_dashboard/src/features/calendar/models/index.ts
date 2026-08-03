import { ActivityType, ActivityStatus } from '../../activities/models';

export interface CalendarEntryDto {
  activityId: string;
  title: string;
  description?: string;
  date: string;
  startTime?: string;
  endTime?: string;
  duration?: string;
  status: ActivityStatus;
  priority: number;
  activityType: ActivityType;

  classId: string;
  className: string;
  courseName: string;

  teacherId?: string;
  teacherName?: string;
  roomId?: string;
  roomName?: string;

  colorHex: string;
}

export interface DailyCalendarDto {
  date: string;
  entries: CalendarEntryDto[];
}

export interface WeeklyCalendarDto {
  startDate: string;
  endDate: string;
  days: DailyCalendarDto[];
}

export interface MonthlyCalendarDto {
  year: number;
  month: number;
  weeks: WeeklyCalendarDto[];
}
