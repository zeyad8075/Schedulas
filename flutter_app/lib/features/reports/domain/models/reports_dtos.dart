class WorkloadReportDto {
  final String entityId;
  final double totalWorkloadMinutes;
  final DateTime startDate;
  final DateTime endDate;

  const WorkloadReportDto({
    required this.entityId,
    required this.totalWorkloadMinutes,
    required this.startDate,
    required this.endDate,
  });

  factory WorkloadReportDto.fromJson(Map<String, dynamic> json) {
    return WorkloadReportDto(
      entityId: json['entityId'] as String,
      totalWorkloadMinutes: (json['totalWorkloadMinutes'] as num).toDouble(),
      startDate: DateTime.parse(json['startDate'] as String),
      endDate: DateTime.parse(json['endDate'] as String),
    );
  }
}

class ActivityDistributionItemDto {
  final String activityType;
  final int count;

  const ActivityDistributionItemDto({
    required this.activityType,
    required this.count,
  });

  factory ActivityDistributionItemDto.fromJson(Map<String, dynamic> json) {
    // Handling the enum from C# (it might be serialized as integer or string depending on backend config)
    // We parse it to string for now.
    return ActivityDistributionItemDto(
      activityType: json['activityType'].toString(),
      count: json['count'] as int,
    );
  }
}

class ActivityDistributionReportDto {
  final String entityId;
  final DateTime startDate;
  final DateTime endDate;
  final List<ActivityDistributionItemDto> distribution;

  const ActivityDistributionReportDto({
    required this.entityId,
    required this.startDate,
    required this.endDate,
    required this.distribution,
  });

  factory ActivityDistributionReportDto.fromJson(Map<String, dynamic> json) {
    return ActivityDistributionReportDto(
      entityId: json['entityId'] as String,
      startDate: DateTime.parse(json['startDate'] as String),
      endDate: DateTime.parse(json['endDate'] as String),
      distribution: (json['distribution'] as List<dynamic>?)
              ?.map((e) => ActivityDistributionItemDto.fromJson(
                  e as Map<String, dynamic>))
              .toList() ??
          [],
    );
  }
}
