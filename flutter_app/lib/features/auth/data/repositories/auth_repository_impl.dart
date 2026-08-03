import 'package:fpdart/fpdart.dart';
import '../../../../core/errors/failures.dart';
import '../../../../core/network/api_client.dart';
import '../../../../core/network/api_exception.dart';
import '../../../../core/storage/secure_storage_service.dart';
import '../../domain/repositories/auth_repository.dart';
import '../../domain/user_profile.dart';

class AuthRepositoryImpl implements AuthRepository {
  final ApiClient _apiClient;
  final SecureStorageService _storageService;

  AuthRepositoryImpl(this._apiClient, this._storageService);

  @override
  Future<Either<Failure, Unit>> login(String email, String password) async {
    try {
      final result = await _apiClient.post(
        '/auth/login',
        body: {'email': email, 'password': password},
        fromData: (json) => json as Map<String, dynamic>,
      );

      final accessToken = result['accessToken'] as String?;
      final refreshToken = result['refreshToken'] as String?;

      if (accessToken != null && refreshToken != null) {
        await _storageService.saveAccessToken(accessToken);
        await _storageService.saveRefreshToken(refreshToken);
        return right(unit);
      } else {
        return left(
            const ServerFailure('لم يتم العثور على رموز الوصول في الاستجابة'));
      }
    } on ApiException catch (e) {
      return left(_mapApiException(e));
    } catch (e) {
      return left(UnknownFailure(e.toString()));
    }
  }

  @override
  Future<Either<Failure, Unit>> register(
      String email,
      String password,
      String fullName,
      int role,
      String institutionId,
      String? departmentId) async {
    try {
      await _apiClient.post(
        '/auth/register',
        body: {
          'email': email,
          'password': password,
          'fullName': fullName,
          'role': role,
          'institutionId': institutionId,
          'departmentId': departmentId,
        },
        fromData: (json) => json,
      );
      return right(unit);
    } on ApiException catch (e) {
      return left(_mapApiException(e));
    } catch (e) {
      return left(UnknownFailure(e.toString()));
    }
  }

  @override
  Future<Either<Failure, Unit>> forgotPassword(String email) async {
    try {
      await _apiClient.post(
        '/auth/forgot-password',
        body: {'email': email},
        fromData: (json) => json,
      );
      return right(unit);
    } on ApiException catch (e) {
      return left(_mapApiException(e));
    } catch (e) {
      return left(UnknownFailure(e.toString()));
    }
  }

  @override
  Future<Either<Failure, Unit>> refresh() async {
    try {
      final refreshToken = await _storageService.readRefreshToken();
      if (refreshToken == null) {
        return left(const UnauthorizedFailure('لا يوجد رمز تحديث متوفر'));
      }

      final result = await _apiClient.post(
        '/auth/refresh',
        body: {'refreshToken': refreshToken},
        fromData: (json) => json as Map<String, dynamic>,
      );

      final newAccessToken = result['accessToken'] as String?;
      final newRefreshToken = result['refreshToken'] as String?;

      if (newAccessToken != null && newRefreshToken != null) {
        await _storageService.saveAccessToken(newAccessToken);
        await _storageService.saveRefreshToken(newRefreshToken);
        return right(unit);
      } else {
        return left(const ServerFailure('فشل في تجديد الجلسة'));
      }
    } on ApiException catch (e) {
      return left(_mapApiException(e));
    } catch (e) {
      return left(UnknownFailure(e.toString()));
    }
  }

  @override
  Future<Either<Failure, Unit>> logout() async {
    try {
      await _storageService.clear();
      return right(unit);
    } catch (e) {
      return left(UnknownFailure(e.toString()));
    }
  }

  @override
  Future<Either<Failure, UserProfile>> me() async {
    try {
      final profile = await _apiClient.get(
        '/auth/me',
        fromData: (json) => UserProfile.fromJson(json as Map<String, dynamic>),
      );
      return right(profile);
    } on ApiException catch (e) {
      return left(_mapApiException(e));
    } catch (e) {
      return left(UnknownFailure(e.toString()));
    }
  }

  Failure _mapApiException(ApiException e) {
    if (e.statusCode == 401) {
      return UnauthorizedFailure(e.message);
    }
    if (e.fieldMessages != null && e.fieldMessages!.isNotEmpty) {
      return ValidationFailure(e.message, e.fieldMessages!);
    }
    if (e.message.contains('تعذّر الاتصال بالخادم')) {
      return NetworkFailure(e.message);
    }
    if (e.statusCode != null && e.statusCode! >= 500) {
      return ServerFailure(e.message);
    }
    return ServerFailure(e.message);
  }
}
