class CourseDto {
  final String id;
  final String programId;
  final String name;
  final String? code;
  final bool isActive;

  const CourseDto({
    required this.id,
    required this.programId,
    required this.name,
    this.code,
    required this.isActive,
  });

  factory CourseDto.fromJson(Map<String, dynamic> json) => CourseDto(
        id: json['id'] as String,
        programId: json['programId'] as String,
        name: json['name'] as String,
        code: json['code'] as String?,
        isActive: json['isActive'] as bool,
      );

  Map<String, dynamic> toJson() => {
        'id': id,
        'programId': programId,
        'name': name,
        'code': code,
        'isActive': isActive,
      };
}
