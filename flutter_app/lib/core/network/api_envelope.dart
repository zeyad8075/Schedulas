/// Mirrors Schedulas.API.Common.ApiResponse&lt;T&gt; exactly (Constitution §12).
/// Every successful or failed call against the Schedulas API deserializes
/// into this shape — callers never hand-parse raw JSON per endpoint.
class ApiEnvelope<T> {
  final bool success;
  final T? data;
  final String? message;
  final List<ApiFieldError>? errors;

  const ApiEnvelope({
    required this.success,
    this.data,
    this.message,
    this.errors,
  });

  factory ApiEnvelope.fromJson(
    Map<String, dynamic> json,
    T Function(dynamic json) fromData,
  ) {
    final rawData = json['data'];
    return ApiEnvelope<T>(
      success: json['success'] as bool? ?? false,
      data: rawData == null ? null : fromData(rawData),
      message: json['message'] as String?,
      errors: (json['errors'] as List<dynamic>?)
          ?.map((e) => ApiFieldError.fromJson(e as Map<String, dynamic>))
          .toList(),
    );
  }
}

class ApiFieldError {
  final String field;
  final String message;

  const ApiFieldError({required this.field, required this.message});

  factory ApiFieldError.fromJson(Map<String, dynamic> json) => ApiFieldError(
        field: json['field'] as String? ?? '',
        message: json['message'] as String? ?? '',
      );
}
