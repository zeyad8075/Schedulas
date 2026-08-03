/// Reads build-time configuration injected via `--dart-define`, e.g.:
///
/// ```
/// flutter run \
///   --dart-define=API_BASE_URL=https://api.schedulas.example/api/v1
/// ```
///
/// Never hardcode real values here — this mirrors the backend's
/// environment-variable/user-secrets discipline (Constitution §16).
/// Empty defaults fail loudly at startup (see main.dart) rather than
/// silently pointing at nothing.
class EnvConfig {
  EnvConfig._();

  static const String apiBaseUrl = String.fromEnvironment('API_BASE_URL');

  static bool get isConfigured => apiBaseUrl.isNotEmpty;
}
