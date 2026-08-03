import 'user_role.dart';

/// Mirrors Schedulas.Application.Features.Auth.Queries.CurrentUserDto —
/// the shape returned by GET /api/v1/auth/me.
class UserProfile {
  final String id;
  final String fullName;
  final String email;
  final UserRole role;
  final String? institutionId;
  final String? departmentId;

  const UserProfile({
    required this.id,
    required this.fullName,
    required this.email,
    required this.role,
    this.institutionId,
    this.departmentId,
  });

  factory UserProfile.fromJson(Map<String, dynamic> json) => UserProfile(
        id: json['id'] as String,
        fullName: json['fullName'] as String,
        email: json['email'] as String,
        role: UserRole.fromJson(json['role'] as String),
        institutionId: json['institutionId'] as String?,
        departmentId: json['departmentId'] as String?,
      );
}
