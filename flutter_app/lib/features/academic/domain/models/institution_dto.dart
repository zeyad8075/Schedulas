enum InstitutionType {
  school,
  university,
  trainingCenter,
  other;

  static InstitutionType fromJson(String value) => switch (value) {
        'School' => InstitutionType.school,
        'University' => InstitutionType.university,
        'TrainingCenter' => InstitutionType.trainingCenter,
        _ => InstitutionType.other,
      };

  String toJson() => switch (this) {
        InstitutionType.school => 'School',
        InstitutionType.university => 'University',
        InstitutionType.trainingCenter => 'TrainingCenter',
        InstitutionType.other => 'Other',
      };
}

class InstitutionDto {
  final String id;
  final String name;
  final InstitutionType type;
  final String timezone;
  final String? logoUrl;
  final bool isSuspended;

  const InstitutionDto({
    required this.id,
    required this.name,
    required this.type,
    required this.timezone,
    this.logoUrl,
    required this.isSuspended,
  });

  factory InstitutionDto.fromJson(Map<String, dynamic> json) => InstitutionDto(
        id: json['id'] as String,
        name: json['name'] as String,
        type: InstitutionType.fromJson(json['type'] as String),
        timezone: json['timezone'] as String,
        logoUrl: json['logoUrl'] as String?,
        isSuspended: json['isSuspended'] as bool,
      );

  Map<String, dynamic> toJson() => {
        'id': id,
        'name': name,
        'type': type.toJson(),
        'timezone': timezone,
        'logoUrl': logoUrl,
        'isSuspended': isSuspended,
      };
}
