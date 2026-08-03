import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../../core/network/api_exception.dart';
import '../../../../l10n/app_localizations.dart';
import '../../../auth/presentation/providers/auth_providers.dart';
import '../../domain/user_settings.dart';
import '../../domain/institution_settings.dart';
import '../providers/settings_providers.dart';

class SettingsScreen extends ConsumerStatefulWidget {
  const SettingsScreen({super.key});

  @override
  ConsumerState<SettingsScreen> createState() => _SettingsScreenState();
}

class _SettingsScreenState extends ConsumerState<SettingsScreen> {
  final _personalFormKey = GlobalKey<FormState>();
  final _institutionFormKey = GlobalKey<FormState>();

  late final TextEditingController _fullNameController;
  late final TextEditingController _phoneController;
  PreferredTheme? _selectedTheme;

  late final TextEditingController _institutionNameController;
  late final TextEditingController _timezoneController;

  bool _isSavingPersonal = false;
  String? _personalErrorMessage;
  bool _personalInitialized = false;

  bool _isSavingInstitution = false;
  String? _institutionErrorMessage;
  bool _institutionInitialized = false;

  void _initializePersonal(UserSettings settings) {
    if (_personalInitialized) return;
    _fullNameController = TextEditingController(text: settings.fullName);
    _phoneController = TextEditingController(text: settings.phoneNumber ?? '');
    _selectedTheme = settings.preferredTheme;
    _personalInitialized = true;
  }

  void _initializeInstitution(InstitutionSettingsDto settings) {
    if (_institutionInitialized) return;
    _institutionNameController = TextEditingController(text: settings.name);
    _timezoneController = TextEditingController(text: settings.timezone);
    _institutionInitialized = true;
  }

  @override
  void dispose() {
    if (_personalInitialized) {
      _fullNameController.dispose();
      _phoneController.dispose();
    }
    if (_institutionInitialized) {
      _institutionNameController.dispose();
      _timezoneController.dispose();
    }
    super.dispose();
  }

  Future<void> _savePersonal() async {
    if (!_personalFormKey.currentState!.validate() || _selectedTheme == null) {
      return;
    }
    final l10n = AppLocalizations.of(context);

    setState(() {
      _isSavingPersonal = true;
      _personalErrorMessage = null;
    });

    try {
      await updateUserSettings(
        ref,
        fullName: _fullNameController.text.trim(),
        phoneNumber: _phoneController.text.trim().isEmpty
            ? null
            : _phoneController.text.trim(),
        preferredTheme: _selectedTheme!,
      );
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text(l10n.settingsSuccess)),
        );
      }
    } on ApiException catch (e) {
      setState(() => _personalErrorMessage = e.message);
    } catch (_) {
      setState(() => _personalErrorMessage = l10n.settingsUnexpectedError);
    } finally {
      if (mounted) setState(() => _isSavingPersonal = false);
    }
  }

  Future<void> _saveInstitution() async {
    if (!_institutionFormKey.currentState!.validate()) return;
    final l10n = AppLocalizations.of(context);

    setState(() {
      _isSavingInstitution = true;
      _institutionErrorMessage = null;
    });

    try {
      await updateInstitutionSettings(
        ref,
        name: _institutionNameController.text.trim(),
        timezone: _timezoneController.text.trim(),
      );
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text(l10n.settingsSuccess)),
        );
      }
    } on ApiException catch (e) {
      setState(() => _institutionErrorMessage = e.message);
    } catch (_) {
      setState(() => _institutionErrorMessage = l10n.settingsUnexpectedError);
    } finally {
      if (mounted) setState(() => _isSavingInstitution = false);
    }
  }

  Widget _buildPersonalForm(
      UserSettings settings, AppLocalizations l10n, ThemeData theme) {
    _initializePersonal(settings);

    return SingleChildScrollView(
      padding: const EdgeInsets.all(20),
      child: Form(
        key: _personalFormKey,
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            if (_personalErrorMessage != null) ...[
              Container(
                padding: const EdgeInsets.all(12),
                decoration: BoxDecoration(
                  color: theme.colorScheme.errorContainer,
                  borderRadius: BorderRadius.circular(12),
                ),
                child: Text(_personalErrorMessage!,
                    style:
                        TextStyle(color: theme.colorScheme.onErrorContainer)),
              ),
              const SizedBox(height: 16),
            ],
            Text(l10n.settingsAppearance, style: theme.textTheme.labelLarge),
            const SizedBox(height: 8),
            SegmentedButton<PreferredTheme>(
              segments: [
                ButtonSegment(
                    value: PreferredTheme.light,
                    label: Text(l10n.themeLight),
                    icon: const Icon(Icons.light_mode_outlined)),
                ButtonSegment(
                    value: PreferredTheme.dark,
                    label: Text(l10n.themeDark),
                    icon: const Icon(Icons.dark_mode_outlined)),
              ],
              selected: {_selectedTheme ?? settings.preferredTheme},
              onSelectionChanged: (selection) =>
                  setState(() => _selectedTheme = selection.first),
            ),
            const SizedBox(height: 24),
            Text(l10n.settingsPersonalInfo, style: theme.textTheme.labelLarge),
            const SizedBox(height: 8),
            TextFormField(
              controller: _fullNameController,
              decoration: InputDecoration(labelText: l10n.fullName),
              validator: (value) => (value == null || value.trim().isEmpty)
                  ? l10n.settingsNameRequired
                  : null,
            ),
            const SizedBox(height: 16),
            TextFormField(
              controller: _phoneController,
              keyboardType: TextInputType.phone,
              decoration:
                  InputDecoration(labelText: l10n.settingsPhoneOptional),
            ),
            const SizedBox(height: 32),
            ElevatedButton(
              onPressed: _isSavingPersonal ? null : _savePersonal,
              child: _isSavingPersonal
                  ? const SizedBox(
                      height: 22,
                      width: 22,
                      child: CircularProgressIndicator(
                          strokeWidth: 2.5, color: Colors.white),
                    )
                  : Text(l10n.save),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildInstitutionForm(
      InstitutionSettingsDto settings, AppLocalizations l10n, ThemeData theme) {
    _initializeInstitution(settings);

    return SingleChildScrollView(
      padding: const EdgeInsets.all(20),
      child: Form(
        key: _institutionFormKey,
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            if (_institutionErrorMessage != null) ...[
              Container(
                padding: const EdgeInsets.all(12),
                decoration: BoxDecoration(
                  color: theme.colorScheme.errorContainer,
                  borderRadius: BorderRadius.circular(12),
                ),
                child: Text(_institutionErrorMessage!,
                    style:
                        TextStyle(color: theme.colorScheme.onErrorContainer)),
              ),
              const SizedBox(height: 16),
            ],
            TextFormField(
              controller: _institutionNameController,
              decoration: InputDecoration(labelText: l10n.academicName),
              validator: (value) => (value == null || value.trim().isEmpty)
                  ? l10n.validationRequired
                  : null,
            ),
            const SizedBox(height: 16),
            TextFormField(
              controller: _timezoneController,
              decoration: InputDecoration(labelText: l10n.academicTimezone),
              validator: (value) => (value == null || value.trim().isEmpty)
                  ? l10n.validationRequired
                  : null,
            ),
            const SizedBox(height: 32),
            ElevatedButton(
              onPressed: _isSavingInstitution ? null : _saveInstitution,
              child: _isSavingInstitution
                  ? const SizedBox(
                      height: 22,
                      width: 22,
                      child: CircularProgressIndicator(
                          strokeWidth: 2.5, color: Colors.white),
                    )
                  : Text(l10n.save),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildError(
      ThemeData theme, AppLocalizations l10n, void Function() onRetry) {
    return Center(
      child: Padding(
        padding: const EdgeInsets.all(24),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            Icon(Icons.error_outline, size: 48, color: theme.colorScheme.error),
            const SizedBox(height: 12),
            Text(l10n.settingsLoadError, textAlign: TextAlign.center),
            const SizedBox(height: 16),
            FilledButton(
              onPressed: onRetry,
              child: Text(l10n.commonRetry),
            ),
          ],
        ),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context);
    final theme = Theme.of(context);
    final user = ref.watch(currentProfileProvider);
    final isInstitutionAdmin = user?.role.name == 'institutionAdmin';

    final settingsAsync = ref.watch(userSettingsProvider);

    if (!isInstitutionAdmin) {
      return Scaffold(
        appBar: AppBar(title: Text(l10n.settings)),
        body: settingsAsync.when(
          loading: () => const Center(child: CircularProgressIndicator()),
          error: (error, _) => _buildError(
              theme, l10n, () => ref.invalidate(userSettingsProvider)),
          data: (settings) {
            if (settings == null) return const SizedBox.shrink();
            return _buildPersonalForm(settings, l10n, theme);
          },
        ),
      );
    }

    final institutionSettingsAsync = ref.watch(institutionSettingsProvider);

    return DefaultTabController(
      length: 2,
      child: Scaffold(
        appBar: AppBar(
          title: Text(l10n.settings),
          bottom: TabBar(
            tabs: [
              Tab(text: l10n.settingsPersonal),
              Tab(text: l10n.settingsInstitution),
            ],
          ),
        ),
        body: TabBarView(
          children: [
            settingsAsync.when(
              loading: () => const Center(child: CircularProgressIndicator()),
              error: (error, _) => _buildError(
                  theme, l10n, () => ref.invalidate(userSettingsProvider)),
              data: (settings) {
                if (settings == null) return const SizedBox.shrink();
                return _buildPersonalForm(settings, l10n, theme);
              },
            ),
            institutionSettingsAsync.when(
              loading: () => const Center(child: CircularProgressIndicator()),
              error: (error, _) => _buildError(theme, l10n,
                  () => ref.invalidate(institutionSettingsProvider)),
              data: (settings) {
                if (settings == null) return const SizedBox.shrink();
                return _buildInstitutionForm(settings, l10n, theme);
              },
            ),
          ],
        ),
      ),
    );
  }
}
