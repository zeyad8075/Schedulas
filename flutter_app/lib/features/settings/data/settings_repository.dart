import '../../../core/network/api_client.dart';
import '../domain/user_settings.dart';
import '../domain/institution_settings.dart';

/// GET/PUT /api/v1/settings/user — deliberately takes no user-id
/// parameter anywhere, matching the backend's own design
/// (UpdateUserSettingsCommand/GetUserSettingsQuery resolve the caller's
/// own Profile from ICurrentUserService, never from a client-supplied
/// id — Phase 8 Verification pass called this out as the safest possible
/// pattern for "give me MY OWN X").
class SettingsRepository {
  final ApiClient _apiClient;

  SettingsRepository(this._apiClient);

  Future<UserSettings> getUserSettings() => _apiClient.get(
        '/settings/user',
        fromData: (json) => UserSettings.fromJson(json as Map<String, dynamic>),
      );

  Future<UserSettings> updateUserSettings({
    required String fullName,
    String? phoneNumber,
    required PreferredTheme preferredTheme,
  }) {
    return _apiClient.put(
      '/settings/user',
      body: {
        'fullName': fullName,
        'phoneNumber': phoneNumber,
        'preferredTheme': preferredTheme.toJson(),
      },
      fromData: (json) => UserSettings.fromJson(json as Map<String, dynamic>),
    );
  }

  Future<InstitutionSettingsDto> getInstitutionSettings() => _apiClient.get(
        '/settings/institution',
        fromData: (json) =>
            InstitutionSettingsDto.fromJson(json as Map<String, dynamic>),
      );

  Future<void> updateInstitutionSettings({
    required String name,
    required String timezone,
  }) {
    return _apiClient.put(
      '/settings/institution',
      body: {
        'name': name,
        'timezone': timezone,
      },
      fromData: (_) {},
    );
  }
}
