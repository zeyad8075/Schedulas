class ParentDto {
  final String id;
  final String profileId;
  final String fullName;
  final String email;
  final bool isActive;

  ParentDto({
    required this.id,
    required this.profileId,
    required this.fullName,
    required this.email,
    required this.isActive,
  });

  factory ParentDto.fromJson(Map<String, dynamic> json) {
    return ParentDto(
      id: json['id'] as String,
      profileId: json['profileId'] as String,
      fullName: json['fullName'] as String,
      email: json['email'] as String,
      isActive: json['isActive'] as bool? ?? false,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'profileId': profileId,
      'fullName': fullName,
      'email': email,
      'isActive': isActive,
    };
  }
}
