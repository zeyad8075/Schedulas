enum ActivityType {
  standardClass,
  exam,
  assignment,
  project,
  presentation,
  meeting,
  event,
  other
}

enum ActivityStatus {
  scheduled,
  inProgress,
  completed,
  cancelled,
  requiresOverrideApproved
}

extension ActivityTypeExtension on ActivityType {
  int get value {
    switch (this) {
      case ActivityType.standardClass:
        return 0;
      case ActivityType.exam:
        return 1;
      case ActivityType.assignment:
        return 2;
      case ActivityType.project:
        return 3;
      case ActivityType.presentation:
        return 4;
      case ActivityType.meeting:
        return 5;
      case ActivityType.event:
        return 6;
      case ActivityType.other:
        return 7;
    }
  }

  static ActivityType fromValue(int value) {
    switch (value) {
      case 0:
        return ActivityType.standardClass;
      case 1:
        return ActivityType.exam;
      case 2:
        return ActivityType.assignment;
      case 3:
        return ActivityType.project;
      case 4:
        return ActivityType.presentation;
      case 5:
        return ActivityType.meeting;
      case 6:
        return ActivityType.event;
      case 7:
        return ActivityType.other;
      default:
        return ActivityType.other;
    }
  }
}

extension ActivityStatusExtension on ActivityStatus {
  int get value {
    switch (this) {
      case ActivityStatus.scheduled:
        return 0;
      case ActivityStatus.inProgress:
        return 1;
      case ActivityStatus.completed:
        return 2;
      case ActivityStatus.cancelled:
        return 3;
      case ActivityStatus.requiresOverrideApproved:
        return 4;
    }
  }

  static ActivityStatus fromValue(int value) {
    switch (value) {
      case 0:
        return ActivityStatus.scheduled;
      case 1:
        return ActivityStatus.inProgress;
      case 2:
        return ActivityStatus.completed;
      case 3:
        return ActivityStatus.cancelled;
      case 4:
        return ActivityStatus.requiresOverrideApproved;
      default:
        return ActivityStatus.scheduled;
    }
  }
}
