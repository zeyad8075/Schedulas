/// Mirrors Schedulas.Domain.Enums.Theme.
enum PreferredTheme {
  light,
  dark;

  static PreferredTheme fromJson(String value) => switch (value) {
        'Light' => PreferredTheme.light,
        'Dark' => PreferredTheme.dark,
        _ => throw ArgumentError('Unknown Theme: $value'),
      };

  String toJson() => switch (this) {
        PreferredTheme.light => 'Light',
        PreferredTheme.dark => 'Dark',
      };
}

/// Mirrors Schedulas.Application.Features.Settings.UserSettingsDto.
class UserSettings {
  final String id;
  final String fullName;
  final String? phoneNumber;
  final PreferredTheme preferredTheme;

  const UserSettings({
    required this.id,
    required this.fullName,
    this.phoneNumber,
    required this.preferredTheme,
  });

  factory UserSettings.fromJson(Map<String, dynamic> json) => UserSettings(
        id: json['id'] as String,
        fullName: json['fullName'] as String,
        phoneNumber: json['phoneNumber'] as String?,
        preferredTheme:
            PreferredTheme.fromJson(json['preferredTheme'] as String),
      );
}
