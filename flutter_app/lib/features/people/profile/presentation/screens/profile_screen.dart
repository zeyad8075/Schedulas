import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_form_builder/flutter_form_builder.dart';
import '../../../../../l10n/app_localizations.dart';
import '../../../../../core/widgets/error_view.dart';
import '../../../../../core/widgets/loading_view.dart';
import '../../../../../features/people/domain/models/theme_enum.dart'
    as model_theme;
import '../providers/profile_providers.dart';
import '../widgets/profile_form.dart';

class ProfileScreen extends ConsumerWidget {
  const ProfileScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final loc = AppLocalizations.of(context);
    final profileAsync = ref.watch(myProfileProvider);

    return Scaffold(
      appBar: AppBar(
        title: Text(loc.myProfile),
      ),
      body: profileAsync.when(
        loading: () => const LoadingView(),
        error: (error, stack) => ErrorView(
          message: error.toString(),
          onRetry: () => ref.invalidate(myProfileProvider),
        ),
        data: (profile) {
          return SingleChildScrollView(
            padding: const EdgeInsets.all(16),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                Card(
                  child: Padding(
                    padding: const EdgeInsets.all(16),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        ListTile(
                          leading:
                              const CircleAvatar(child: Icon(Icons.person)),
                          title: Text(profile.fullName,
                              style: Theme.of(context).textTheme.titleLarge),
                          subtitle: Text(profile.role.name.toUpperCase()),
                          trailing: IconButton(
                            icon: const Icon(Icons.edit),
                            onPressed: () =>
                                _showEditProfileDialog(context, ref, profile),
                          ),
                        ),
                        const Divider(),
                        _buildInfoRow(
                            context, Icons.email, loc.email, profile.email),
                        if (profile.phoneNumber != null)
                          _buildInfoRow(context, Icons.phone, loc.phoneNumber,
                              profile.phoneNumber!),
                      ],
                    ),
                  ),
                ),
                const SizedBox(height: 16),
                Card(
                  child: Padding(
                    padding: const EdgeInsets.all(16),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(loc.settings,
                            style: Theme.of(context).textTheme.titleMedium),
                        const SizedBox(height: 16),
                        ListTile(
                          leading: const Icon(Icons.palette),
                          title: Text(loc.theme),
                          subtitle:
                              Text(profile.preferredTheme.name.toUpperCase()),
                          trailing: PopupMenuButton<model_theme.Theme>(
                            initialValue: profile.preferredTheme,
                            onSelected: (theme) =>
                                _updateTheme(context, ref, theme),
                            itemBuilder: (context) => [
                              PopupMenuItem(
                                value: model_theme.Theme.light,
                                child: Text(loc.themeLight),
                              ),
                              PopupMenuItem(
                                value: model_theme.Theme.dark,
                                child: Text(loc.themeDark),
                              ),
                              PopupMenuItem(
                                value: model_theme.Theme.system,
                                child: Text(loc.themeSystem),
                              ),
                            ],
                          ),
                        ),
                      ],
                    ),
                  ),
                )
              ],
            ),
          );
        },
      ),
    );
  }

  Widget _buildInfoRow(
      BuildContext context, IconData icon, String label, String value) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 8),
      child: Row(
        children: [
          Icon(icon, size: 20, color: Theme.of(context).colorScheme.outline),
          const SizedBox(width: 16),
          Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(label,
                  style: Theme.of(context)
                      .textTheme
                      .bodySmall
                      ?.copyWith(color: Theme.of(context).colorScheme.outline)),
              Text(value, style: Theme.of(context).textTheme.bodyLarge),
            ],
          ),
        ],
      ),
    );
  }

  void _showEditProfileDialog(BuildContext context, WidgetRef ref, profile) {
    final formKey = GlobalKey<FormBuilderState>();
    final loc = AppLocalizations.of(context);

    showDialog(
      context: context,
      builder: (ctx) => AlertDialog(
        title: Text(loc.editProfile),
        content: SizedBox(
          width: 400,
          child: ProfileForm(
            formKey: formKey,
            initialData: profile,
            onSubmit: () async {
              if (formKey.currentState?.saveAndValidate() ?? false) {
                final values = formKey.currentState!.value;
                final repository = ref.read(profileRepositoryProvider);
                final result = await repository.updateProfile(
                  values['fullName'] as String,
                  values['phoneNumber'] as String?,
                );

                if (result.isRight() && context.mounted) {
                  ref.invalidate(myProfileProvider);
                } else if (result.isLeft() && context.mounted) {
                  result.fold(
                    (failure) => ScaffoldMessenger.of(ctx)
                        .showSnackBar(SnackBar(content: Text(failure.message))),
                    (_) {},
                  );
                }
              }
            },
            onCancel: () => Navigator.of(ctx).pop(),
          ),
        ),
      ),
    );
  }

  void _updateTheme(
      BuildContext context, WidgetRef ref, model_theme.Theme theme) async {
    final repository = ref.read(profileRepositoryProvider);
    final result = await repository.updateTheme(theme);
    if (result.isRight() && context.mounted) {
      ref.invalidate(myProfileProvider);
    } else if (result.isLeft() && context.mounted) {
      result.fold(
        (failure) => ScaffoldMessenger.of(context)
            .showSnackBar(SnackBar(content: Text(failure.message))),
        (_) {},
      );
    }
  }
}
