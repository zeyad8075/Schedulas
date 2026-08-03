class StudentDto {
  final String id;
  final String profileId;
  final String institutionId;
  final String? studentNumber;
  final String fullName;
  final String email;
  final bool isActive;

  StudentDto({
    required this.id,
    required this.profileId,
    required this.institutionId,
    this.studentNumber,
    required this.fullName,
    required this.email,
    required this.isActive,
  });

  factory StudentDto.fromJson(Map<String, dynamic> json) {
    return StudentDto(
      id: json['id'] as String,
      profileId: json['profileId'] as String,
      institutionId: json['institutionId'] as String,
      studentNumber: json['studentNumber'] as String?,
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
      'studentNumber': studentNumber,
      'fullName': fullName,
      'email': email,
      'isActive': isActive,
    };
  }
}
