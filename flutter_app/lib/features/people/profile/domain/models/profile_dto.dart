import '../../../domain/models/user_role.dart';
import '../../../domain/models/theme_enum.dart' as model_theme;

class ProfileDto {
  final String id;
  final String fullName;
  final String email;
  final String? phoneNumber;
  final UserRole role;
  final String? institutionId;
  final String? departmentId;
  final bool isActive;
  final model_theme.Theme preferredTheme;

  ProfileDto({
    required this.id,
    required this.fullName,
    required this.email,
    this.phoneNumber,
    required this.role,
    this.institutionId,
    this.departmentId,
    required this.isActive,
    required this.preferredTheme,
  });

  factory ProfileDto.fromJson(Map<String, dynamic> json) {
    return ProfileDto(
      id: json['id'] as String,
      fullName: json['fullName'] as String,
      email: json['email'] as String,
      phoneNumber: json['phoneNumber'] as String?,
      role: json['role'] is String
          ? UserRole.values.firstWhere(
              (e) =>
                  e.name.toLowerCase() ==
                  (json['role'] as String).toLowerCase(),
              orElse: () => UserRole.student)
          : UserRoleExtension.fromInt(json['role'] as int? ?? 0),
      institutionId: json['institutionId'] as String?,
      departmentId: json['departmentId'] as String?,
      isActive: json['isActive'] as bool? ?? false,
      preferredTheme: json['preferredTheme'] is String
          ? model_theme.Theme.values.firstWhere(
              (e) =>
                  e.name.toLowerCase() ==
                  (json['preferredTheme'] as String).toLowerCase(),
              orElse: () => model_theme.Theme.system)
          : model_theme.ThemeExtension.fromInt(
              json['preferredTheme'] as int? ?? 0),
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'fullName': fullName,
      'email': email,
      'phoneNumber': phoneNumber,
      'role': role.index,
      'institutionId': institutionId,
      'departmentId': departmentId,
      'isActive': isActive,
      'preferredTheme': preferredTheme.index,
    };
  }
}
