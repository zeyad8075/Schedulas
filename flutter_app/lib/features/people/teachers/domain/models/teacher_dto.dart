class TeacherDto {
  final String id;
  final String profileId;
  final String institutionId;
  final String? departmentId;
  final String fullName;
  final String email;
  final bool isActive;

  TeacherDto({
    required this.id,
    required this.profileId,
    required this.institutionId,
    this.departmentId,
    required this.fullName,
    required this.email,
    required this.isActive,
  });

  factory TeacherDto.fromJson(Map<String, dynamic> json) {
    return TeacherDto(
      id: json['id'] as String,
      profileId: json['profileId'] as String,
      institutionId: json['institutionId'] as String,
      departmentId: json['departmentId'] as String?,
      fullName: json['fullName'] as String,
      email: json['email'] as String,
      isActive: json['isActive'] as bool? ?? false,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'profileId': profileId,
      'institutionId': institutionId,
      'departmentId': departmentId,
      'fullName': fullName,
      'email': email,
      'isActive': isActive,
    };
  }
}
