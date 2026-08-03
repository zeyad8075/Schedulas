class HolidayDto {
  final String id;
  final String institutionId;
  final String? academicTermId;
  final String name;
  final DateTime holidayDate;

  const HolidayDto({
    required this.id,
    required this.institutionId,
    this.academicTermId,
    required this.name,
    required this.holidayDate,
  });

  factory HolidayDto.fromJson(Map<String, dynamic> json) => HolidayDto(
        id: json['id'] as String,
        institutionId: json['institutionId'] as String,
        academicTermId: json['academicTermId'] as String?,
        name: json['name'] as String,
        holidayDate: DateTime.parse(json['holidayDate'] as String),
      );

  Map<String, dynamic> toJson() => {
        'id': id,
        'institutionId': institutionId,
        'academicTermId': academicTermId,
        'name': name,
        'holidayDate': holidayDate.toIso8601String().split('T')[0],
      };
}
