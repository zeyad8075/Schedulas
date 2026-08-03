import 'package:flutter/material.dart';
import 'package:flutter_localizations/flutter_localizations.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import 'l10n/app_localizations.dart';

import 'package:path_provider/path_provider.dart';

import 'core/config/env_config.dart';
import 'core/routing/app_router.dart';
import 'core/theme/app_theme.dart';
import 'features/auth/presentation/providers/auth_providers.dart';
import 'features/settings/presentation/providers/settings_providers.dart';

Future<void> main() async {
  WidgetsFlutterBinding.ensureInitialized();

  final tempDir = await getTemporaryDirectory();

  if (!EnvConfig.isConfigured) {
    // Fail loudly and in Arabic-app-appropriate fashion rather than
    // silently pointing Dio/Supabase at empty strings and producing
    // confusing downstream errors — mirrors the backend's fail-fast
    // pattern for missing configuration (Program.cs's connection-string
    // check).
    runApp(const _MissingConfigApp());
    return;
  }

  // Removed Supabase initialization

  runApp(ProviderScope(
    overrides: [
      cacheStorePathProvider.overrideWithValue(tempDir.path),
    ],
    child: const SchedulasApp(),
  ));
}

class SchedulasApp extends ConsumerWidget {
  const SchedulasApp({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final router = ref.watch(routerProvider);
    final themeMode = ref.watch(themeModeProvider);

    return MaterialApp.router(
      title: 'شيدولاس',
      debugShowCheckedModeBanner: false,
      theme: AppTheme.light(),
      darkTheme: AppTheme.dark(),
      themeMode: themeMode,
      locale: const Locale('ar'),
      supportedLocales: const [
        Locale('ar'),
        Locale('en'),
      ],
      localizationsDelegates: const [
        AppLocalizations.delegate,
        GlobalMaterialLocalizations.delegate,
        GlobalWidgetsLocalizations.delegate,
        GlobalCupertinoLocalizations.delegate,
      ],
      builder: (context, child) => Directionality(
        textDirection: TextDirection.rtl,
        child: child!,
      ),
      routerConfig: router,
    );
  }
}

/// Shown instead of the real app if --dart-define values are missing —
/// deliberately does not attempt to run the app in a half-configured
/// state.
class _MissingConfigApp extends StatelessWidget {
  const _MissingConfigApp();

  @override
  Widget build(BuildContext context) {
    return const MaterialApp(
      debugShowCheckedModeBanner: false,
      home: Directionality(
        textDirection: TextDirection.rtl,
        child: Scaffold(
          body: Center(
            child: Padding(
              padding: EdgeInsets.all(24),
              child: Column(
                mainAxisSize: MainAxisSize.min,
                children: [
                  Icon(Icons.settings_outlined, size: 48),
                  SizedBox(height: 16),
                  Text(
                    'إعدادات التطبيق غير مكتملة.\nيرجى تمرير API_BASE_URL عبر --dart-define.',
                    textAlign: TextAlign.center,
                  ),
                ],
              ),
            ),
          ),
        ),
      ),
    );
  }
}
