import 'package:dio/dio.dart';
import '../config/env_config.dart';
import '../storage/secure_storage_service.dart';
import 'package:dio_cache_interceptor/dio_cache_interceptor.dart';
import 'package:dio_cache_interceptor_hive_store/dio_cache_interceptor_hive_store.dart';
import 'api_envelope.dart';
import 'api_exception.dart';

class ApiClient {
  final Dio _dio;
  final SecureStorageService _storageService;

  ApiClient(this._storageService, String cachePath)
      : _dio = Dio(BaseOptions(
            baseUrl: EnvConfig.apiBaseUrl,
            connectTimeout: const Duration(seconds: 15),
            receiveTimeout: const Duration(seconds: 15))) {
    _dio.interceptors.add(AuthInterceptor(_dio, _storageService));

    final cacheOptions = CacheOptions(
      store: HiveCacheStore(cachePath),
      policy: CachePolicy.request,
      hitCacheOnErrorExcept: [401, 403],
      maxStale: const Duration(days: 7),
      priority: CachePriority.normal,
      cipher: null,
      keyBuilder: CacheOptions.defaultCacheKeyBuilder,
      allowPostMethod: false,
    );
    _dio.interceptors.add(DioCacheInterceptor(options: cacheOptions));
  }

  Future<T> get<T>(
    String path, {
    Map<String, dynamic>? queryParameters,
    required T Function(dynamic json) fromData,
  }) =>
      _send(() => _dio.get(path, queryParameters: queryParameters), fromData);

  Future<T> post<T>(
    String path, {
    Object? body,
    required T Function(dynamic json) fromData,
  }) =>
      _send(() => _dio.post(path, data: body), fromData);

  Future<T> put<T>(
    String path, {
    Object? body,
    required T Function(dynamic json) fromData,
  }) =>
      _send(() => _dio.put(path, data: body), fromData);

  Future<T> delete<T>(
    String path, {
    required T Function(dynamic json) fromData,
  }) =>
      _send(() => _dio.delete(path), fromData);

  Future<ApiEnvelope<T>> postAllowingFailureData<T>(
    String path, {
    Object? body,
    required T Function(dynamic json) fromData,
  }) async {
    try {
      final response = await _dio.post(path, data: body);
      return ApiEnvelope<T>.fromJson(
          response.data as Map<String, dynamic>, fromData);
    } on DioException catch (e) {
      if (e.response?.data is Map<String, dynamic>) {
        try {
          return ApiEnvelope<T>.fromJson(
              e.response!.data as Map<String, dynamic>, fromData);
        } catch (_) {}
      }
      throw _mapDioException(e);
    }
  }

  Future<ApiEnvelope<T>> putAllowingFailureData<T>(
    String path, {
    Object? body,
    required T Function(dynamic json) fromData,
  }) async {
    try {
      final response = await _dio.put(path, data: body);
      return ApiEnvelope<T>.fromJson(
          response.data as Map<String, dynamic>, fromData);
    } on DioException catch (e) {
      if (e.response?.data is Map<String, dynamic>) {
        try {
          return ApiEnvelope<T>.fromJson(
              e.response!.data as Map<String, dynamic>, fromData);
        } catch (_) {}
      }
      throw _mapDioException(e);
    }
  }

  Future<T> _send<T>(
    Future<Response> Function() request,
    T Function(dynamic json) fromData,
  ) async {
    try {
      final response = await request();
      final envelope = ApiEnvelope<T>.fromJson(
        response.data as Map<String, dynamic>,
        fromData,
      );

      if (!envelope.success) {
        throw ApiException(
          envelope.message ?? 'حدث خطأ غير متوقع، الرجاء المحاولة لاحقاً',
          statusCode: response.statusCode,
          fieldMessages: envelope.errors?.map((e) => e.message).toList(),
        );
      }

      return envelope.data as T;
    } on DioException catch (e) {
      throw _mapDioException(e);
    }
  }

  ApiException _mapDioException(DioException e) {
    final responseData = e.response?.data;
    final statusCode = e.response?.statusCode;

    if (responseData is Map<String, dynamic> &&
        responseData.containsKey('message')) {
      final message = responseData['message'] as String? ??
          'حدث خطأ غير متوقع، الرجاء المحاولة لاحقاً';
      final errors = (responseData['errors'] as List<dynamic>?)
          ?.map((e) => (e as Map<String, dynamic>)['message'] as String? ?? '')
          .toList();
      return ApiException(message,
          statusCode: statusCode, fieldMessages: errors);
    }

    if (e.type == DioExceptionType.connectionTimeout ||
        e.type == DioExceptionType.receiveTimeout ||
        e.type == DioExceptionType.connectionError) {
      return const ApiException(
          'تعذّر الاتصال بالخادم، تحقق من اتصالك بالإنترنت');
    }

    if (statusCode == 401) {
      return const ApiException('انتهت الجلسة، الرجاء تسجيل الدخول مجدداً',
          statusCode: 401);
    }

    return ApiException('حدث خطأ غير متوقع، الرجاء المحاولة لاحقاً',
        statusCode: statusCode);
  }
}

class AuthInterceptor extends Interceptor {
  final Dio _dio;
  final SecureStorageService _storageService;

  AuthInterceptor(this._dio, this._storageService);

  @override
  void onRequest(
      RequestOptions options, RequestInterceptorHandler handler) async {
    final token = await _storageService.readAccessToken();
    if (token != null) {
      options.headers['Authorization'] = 'Bearer $token';
    }
    super.onRequest(options, handler);
  }

  @override
  void onError(DioException err, ErrorInterceptorHandler handler) async {
    if (err.response?.statusCode == 401 &&
        !err.requestOptions.path.contains('/auth/refresh') &&
        !err.requestOptions.path.contains('/auth/login')) {
      final refreshToken = await _storageService.readRefreshToken();
      if (refreshToken != null) {
        try {
          final refreshResponse = await _dio
              .post('/auth/refresh', data: {'refreshToken': refreshToken});
          if (refreshResponse.statusCode == 200 &&
              refreshResponse.data['success'] == true) {
            final data = refreshResponse.data['data'];
            await _storageService.saveAccessToken(data['accessToken']);
            await _storageService.saveRefreshToken(data['refreshToken']);

            err.requestOptions.headers['Authorization'] =
                'Bearer ${data['accessToken']}';
            final retryResponse = await _dio.fetch(err.requestOptions);
            return handler.resolve(retryResponse);
          }
        } catch (_) {
          // Fallback to error handling if refresh fails
        }
      }
      await _storageService.clear();
      // Notify state to logout - we'll handle this in the AuthNotifier by catching 401
    }
    super.onError(err, handler);
  }
}
