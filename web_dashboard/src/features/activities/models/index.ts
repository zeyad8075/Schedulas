export enum ActivityType {
  StandardClass = 0,
  Exam = 1,
  Assignment = 2,
  Project = 3,
  Presentation = 4,
  Meeting = 5,
  Event = 6,
  Other = 7,
}

export enum ActivityStatus {
  Scheduled = 0,
  InProgress = 1,
  Completed = 2,
  Cancelled = 3,
  RequiresOverrideApproved = 4,
}

export interface ActivityDto {
  id: string;
  classId: string;
  activityType: ActivityType;
  title: string;
  description?: string;
  scheduledDate: string; // YYYY-MM-DD
  scheduledTime?: string; // HH:mm:ss
  endTime?: string; // HH:mm:ss
  duration?: string; // HH:mm:ss
  priority: number;
  estimatedWeight?: number;
  status: ActivityStatus;
  metadataJson?: string;
}

export interface ActivitySubmissionResult {
  success: boolean;
  activity?: ActivityDto;
  requiresOverride: boolean;
  reasonCode?: string;
  triggeredRuleId?: string;
}
