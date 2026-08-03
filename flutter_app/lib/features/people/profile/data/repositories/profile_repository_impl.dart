import 'package:fpdart/fpdart.dart';
import '../../../../../core/network/api_client.dart';
import '../../../../../core/network/api_exception.dart';
import '../../../../../core/errors/failures.dart';
import '../../../../../core/network/paginated_list.dart';
import '../../../domain/models/user_role.dart';
import '../../../domain/models/theme_enum.dart' as model_theme;
import '../../domain/models/profile_dto.dart';
import '../../domain/repositories/profile_repository.dart';

class ProfileRepositoryImpl implements ProfileRepository {
  final ApiClient _apiClient;

  ProfileRepositoryImpl(this._apiClient);

  @override
  Future<Either<Failure, ProfileDto>> getMyProfile() async {
    try {
      final result = await _apiClient.get<ProfileDto>(
        '/profiles/me',
        fromData: (json) => ProfileDto.fromJson(json as Map<String, dynamic>),
      );
      return right(result);
    } on ApiException catch (e) {
      if (e.statusCode == 404) {
                return left(const ServerFailure('Profile not found'));
              }
      return left(ServerFailure(e.message));
    } catch (e) {
      return left(UnknownFailure(e.toString()));
    }
  }

  @override
  Future<Either<Failure, ProfileDto>> getProfileById(String id) async {
    try {
      final result = await _apiClient.get<ProfileDto>(
        '/profiles/$id',
        fromData: (json) => ProfileDto.fromJson(json as Map<String, dynamic>),
      );
      return right(result);
    } on ApiException catch (e) {
      if (e.statusCode == 404) {
                return left(const ServerFailure('Profile not found'));
              }
      return left(ServerFailure(e.message));
    } catch (e) {
      return left(UnknownFailure(e.toString()));
    }
  }

  @override
  Future<Either<Failure, PaginatedList<ProfileDto>>> getProfiles(
    String institutionId, {
    String? searchTerm,
    UserRole? role,
    String? sortBy,
    bool sortDescending = false,
    int pageNumber = 1,
    int pageSize = 20,
  }) async {
    try {
      final queryParameters = {
        'InstitutionId': institutionId,
        if (searchTerm != null && searchTerm.isNotEmpty)
          'SearchTerm': searchTerm,
        if (role != null) 'Role': role.index,
        if (sortBy != null && sortBy.isNotEmpty) 'SortBy': sortBy,
        'SortDescending': sortDescending,
        'PageNumber': pageNumber,
        'PageSize': pageSize,
      };

      final result = await _apiClient.get<PaginatedList<ProfileDto>>(
        '/profiles',
        queryParameters: queryParameters,
        fromData: (json) => PaginatedList.fromJson(
          json,
          (itemJson) => ProfileDto.fromJson(itemJson),
        ),
      );
      return right(result);
    } on ApiException catch (e) {
      if (e.statusCode == 401 || e.statusCode == 403) {
        return left(const UnauthorizedFailure('Not authorized'));
      }
      return left(ServerFailure(e.message));
    } catch (e) {
      return left(UnknownFailure(e.toString()));
    }
  }

  @override
  Future<Either<Failure, ProfileDto>> updateProfile(
    String fullName,
    String? phoneNumber,
  ) async {
    try {
      final result = await _apiClient.put<ProfileDto>(
        '/profiles/me',
        body: {
          'fullName': fullName,
          'phoneNumber': phoneNumber,
        },
        fromData: (json) => ProfileDto.fromJson(json as Map<String, dynamic>),
      );
      return right(result);
    } on ApiException catch (e) {
      if (e.statusCode == 400 && e.fieldMessages != null) {
        return left(ValidationFailure('Validation error', e.fieldMessages!));
      }
      return left(ServerFailure(e.message));
    } catch (e) {
      return left(UnknownFailure(e.toString()));
    }
  }

  @override
  Future<Either<Failure, void>> updateTheme(
      model_theme.Theme preferredTheme) async {
    try {
      await _apiClient.put<void>(
        '/profiles/me/theme',
        body: {
          'preferredTheme': preferredTheme.index,
        },
        fromData: (_) {},
      );
      return right(null);
    } on ApiException catch (e) {
      return left(ServerFailure(e.message));
    } catch (e) {
      return left(UnknownFailure(e.toString()));
    }
  }

  @override
  Future<Either<Failure, void>> suspendProfile(String id) async {
    try {
      await _apiClient.post<void>('/profiles/$id/suspend', fromData: (_) {});
      return right(null);
    } on ApiException catch (e) {
      return left(ServerFailure(e.message));
    } catch (e) {
      return left(UnknownFailure(e.toString()));
    }
  }

  @override
  Future<Either<Failure, void>> activateProfile(String id) async {
    try {
      await _apiClient.post<void>('/profiles/$id/activate', fromData: (_) {});
      return right(null);
    } on ApiException catch (e) {
      return left(ServerFailure(e.message));
    } catch (e) {
      return left(UnknownFailure(e.toString()));
    }
  }
}
