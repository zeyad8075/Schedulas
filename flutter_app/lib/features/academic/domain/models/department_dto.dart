class DepartmentDto {
  final String id;
  final String institutionId;
  final String name;
  final bool isActive;

  const DepartmentDto({
    required this.id,
    required this.institutionId,
    required this.name,
    required this.isActive,
  });

  factory DepartmentDto.fromJson(Map<String, dynamic> json) => DepartmentDto(
        id: json['id'] as String,
        institutionId: json['institutionId'] as String,
        name: json['name'] as String,
        isActive: json['isActive'] as bool,
      );

  Map<String, dynamic> toJson() => {
        'id': id,
        'institutionId': institutionId,
        'name': name,
        'isActive': isActive,
      };
}
