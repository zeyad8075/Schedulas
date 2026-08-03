/// A failure surfaced from the API (or from the network layer itself),
/// carrying the Arabic message the backend already resolved (Architecture
/// §7 — Domain/Application produce reason codes, the API resolves them to
/// Arabic; Flutter just displays what it's given, never re-implements
/// that resolution logic on the client).
class ApiException implements Exception {
  final String message;
  final int? statusCode;
  final List<String>? fieldMessages;

  const ApiException(this.message, {this.statusCode, this.fieldMessages});

  /// True for 401 — the caller should redirect to login.
  bool get isUnauthenticated => statusCode == 401;

  /// True for 403 — the caller should show an Arabic permission-denied
  /// message but NOT redirect to login (the user is authenticated, just
  /// not allowed to do this specific thing).
  bool get isForbidden => statusCode == 403;

  @override
  String toString() => message;
}
