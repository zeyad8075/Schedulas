import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../auth/presentation/providers/auth_providers.dart';
import '../../domain/user_settings.dart';
import '../../domain/institution_settings.dart';
import '../../data/settings_repository.dart';

final settingsRepositoryProvider = Provider<SettingsRepository>(
  (ref) => SettingsRepository(ref.watch(apiClientProvider)),
);

final userSettingsProvider =
    FutureProvider.autoDispose<UserSettings?>((ref) async {
  final authenticated = ref.watch(isAuthenticatedProvider);
  if (!authenticated) return null;

  final repository = ref.watch(settingsRepositoryProvider);
  return repository.getUserSettings();
});

/// Drives MaterialApp.themeMode directly from the signed-in user's saved
/// preference (Constitution §19: light/dark both required, and it should
/// be an actual per-user setting, not just whatever the OS happens to be
/// set to). Falls back to ThemeMode.system while settings are still
/// loading or for a signed-out user, rather than flashing a wrong theme
/// then correcting it.
final themeModeProvider = Provider<ThemeMode>((ref) {
  final settings = ref.watch(userSettingsProvider).valueOrNull;
  if (settings == null) return ThemeMode.system;

  return switch (settings.preferredTheme) {
    PreferredTheme.light => ThemeMode.light,
    PreferredTheme.dark => ThemeMode.dark,
  };
});

Future<UserSettings> updateUserSettings(
  WidgetRef ref, {
  required String fullName,
  String? phoneNumber,
  required PreferredTheme preferredTheme,
}) async {
  final updated = await ref.read(settingsRepositoryProvider).updateUserSettings(
        fullName: fullName,
        phoneNumber: phoneNumber,
        preferredTheme: preferredTheme,
      );
  ref.invalidate(userSettingsProvider);
  return updated;
}

final institutionSettingsProvider =
    FutureProvider.autoDispose<InstitutionSettingsDto?>((ref) async {
  final user = ref.watch(currentProfileProvider);
  if (user == null || user.role.name != 'institutionAdmin') return null;

  final repository = ref.watch(settingsRepositoryProvider);
  return repository.getInstitutionSettings();
});

Future<void> updateInstitutionSettings(
  WidgetRef ref, {
  required String name,
  required String timezone,
}) async {
  await ref.read(settingsRepositoryProvider).updateInstitutionSettings(
        name: name,
        timezone: timezone,
      );
  ref.invalidate(institutionSettingsProvider);
}
