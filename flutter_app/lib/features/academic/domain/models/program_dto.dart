class ProgramDto {
  final String id;
  final String departmentId;
  final String name;
  final bool isActive;

  const ProgramDto({
    required this.id,
    required this.departmentId,
    required this.name,
    required this.isActive,
  });

  factory ProgramDto.fromJson(Map<String, dynamic> json) => ProgramDto(
        id: json['id'] as String,
        departmentId: json['departmentId'] as String,
        name: json['name'] as String,
        isActive: json['isActive'] as bool,
      );

  Map<String, dynamic> toJson() => {
        'id': id,
        'departmentId': departmentId,
        'name': name,
        'isActive': isActive,
      };
}
