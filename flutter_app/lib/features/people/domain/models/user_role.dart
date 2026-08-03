enum UserRole {
  platformAdmin,
  institutionAdmin,
  teacher,
  student,
  parent,
  staff
}

extension UserRoleExtension on UserRole {
  static UserRole fromInt(int value) {
    return UserRole.values.firstWhere(
      (e) => e.index == value,
      orElse: () => UserRole.student,
    );
  }
}
