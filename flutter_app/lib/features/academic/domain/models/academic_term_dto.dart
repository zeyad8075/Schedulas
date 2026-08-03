class AcademicTermDto {
  final String id;
  final String institutionId;
  final String name;
  final DateTime startDate;
  final DateTime endDate;
  final bool isActive;

  const AcademicTermDto({
    required this.id,
    required this.institutionId,
    required this.name,
    required this.startDate,
    required this.endDate,
    required this.isActive,
  });

  factory AcademicTermDto.fromJson(Map<String, dynamic> json) =>
      AcademicTermDto(
        id: json['id'] as String,
        institutionId: json['institutionId'] as String,
        name: json['name'] as String,
        startDate: DateTime.parse(json['startDate'] as String),
        endDate: DateTime.parse(json['endDate'] as String),
        isActive: json['isActive'] as bool,
      );

  Map<String, dynamic> toJson() => {
        'id': id,
        'institutionId': institutionId,
        'name': name,
        'startDate': startDate.toIso8601String().split('T')[0],
        'endDate': endDate.toIso8601String().split('T')[0],
        'isActive': isActive,
      };
}
