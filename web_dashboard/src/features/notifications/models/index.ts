export enum NotificationCategory {
  NewActivity = 'NewActivity',
  ActivityEdited = 'ActivityEdited',
  ActivityCancelled = 'ActivityCancelled',
  DeadlineReminder = 'DeadlineReminder',
  RuleViolation = 'RuleViolation',
  RuleOverride = 'RuleOverride',
}

export interface NotificationDto {
  id: string;
  category: NotificationCategory;
  title: string;
  body: string;
  relatedActivityId?: string;
  isRead: boolean;
  createdAt: string;
}

export interface PaginatedNotificationsResponse {
  items: NotificationDto[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}
