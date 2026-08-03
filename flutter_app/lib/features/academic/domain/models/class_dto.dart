class ClassDto {
  final String id;
  final String courseId;
  final String academicTermId;
  final String name;
  final bool isActive;

  const ClassDto({
    required this.id,
    required this.courseId,
    required this.academicTermId,
    required this.name,
    required this.isActive,
  });

  factory ClassDto.fromJson(Map<String, dynamic> json) => ClassDto(
        id: json['id'] as String,
        courseId: json['courseId'] as String,
        academicTermId: json['academicTermId'] as String,
        name: json['name'] as String,
        isActive: json['isActive'] as bool,
      );

  Map<String, dynamic> toJson() => {
        'id': id,
        'courseId': courseId,
        'academicTermId': academicTermId,
        'name': name,
        'isActive': isActive,
      };
}
