/// Mirrors Schedulas.Domain.Enums.UserRole exactly — string values match
/// the backend's JsonStringEnumConverter output so parsing needs no
/// translation table.
enum UserRole {
  platformAdmin,
  institutionAdmin,
  departmentAdmin,
  teacher,
  student,
  parent;

  static UserRole fromJson(String value) => switch (value) {
        'PlatformAdmin' => UserRole.platformAdmin,
        'InstitutionAdmin' => UserRole.institutionAdmin,
        'DepartmentAdmin' => UserRole.departmentAdmin,
        'Teacher' => UserRole.teacher,
        'Student' => UserRole.student,
        'Parent' => UserRole.parent,
        _ => throw ArgumentError('Unknown UserRole: $value'),
      };
}

enum AppTheme { light, dark }
