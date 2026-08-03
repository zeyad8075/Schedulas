import 'package:fpdart/fpdart.dart';
import '../../../../../core/network/api_client.dart';
import '../../../../../core/network/api_exception.dart';
import '../../../../../core/errors/failures.dart';
import '../../../../../core/network/paginated_list.dart';
import '../../domain/models/teacher_dto.dart';
import '../../domain/repositories/teacher_repository.dart';

class TeacherRepositoryImpl implements TeacherRepository {
  final ApiClient _apiClient;

  TeacherRepositoryImpl(this._apiClient);

  @override
  Future<Either<Failure, TeacherDto>> getTeacherById(String id) async {
    try {
      final result = await _apiClient.get<TeacherDto>(
        '/teachers/$id',
        fromData: (json) => TeacherDto.fromJson(json as Map<String, dynamic>),
      );
      return right(result);
    } on ApiException catch (e) {
      if (e.statusCode == 404) {
                return left(const ServerFailure('Teacher not found'));
              }
      return left(ServerFailure(e.message));
    } catch (e) {
      return left(UnknownFailure(e.toString()));
    }
  }

  @override
  Future<Either<Failure, PaginatedList<TeacherDto>>> getTeachers(
    String institutionId, {
    String? departmentId,
    String? searchTerm,
    String? sortBy,
    bool sortDescending = false,
    int pageNumber = 1,
    int pageSize = 20,
  }) async {
    try {
      final queryParameters = {
        'InstitutionId': institutionId,
        if (departmentId != null && departmentId.isNotEmpty)
          'DepartmentId': departmentId,
        if (searchTerm != null && searchTerm.isNotEmpty)
          'SearchTerm': searchTerm,
        if (sortBy != null && sortBy.isNotEmpty) 'SortBy': sortBy,
        'SortDescending': sortDescending,
        'PageNumber': pageNumber,
        'PageSize': pageSize,
      };

      final result = await _apiClient.get<PaginatedList<TeacherDto>>(
        '/teachers',
        queryParameters: queryParameters,
        fromData: (json) => PaginatedList.fromJson(
          json,
          (itemJson) => TeacherDto.fromJson(itemJson),
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
  Future<Either<Failure, TeacherDto>> createTeacher(
    String profileId,
    String? departmentId,
  ) async {
    try {
      final result = await _apiClient.post<TeacherDto>(
        '/teachers',
        body: {
          'profileId': profileId,
          'departmentId': departmentId,
        },
        fromData: (json) => TeacherDto.fromJson(json as Map<String, dynamic>),
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
  Future<Either<Failure, TeacherDto>> updateTeacher(
    String id,
    String? departmentId,
  ) async {
    try {
      final result = await _apiClient.put<TeacherDto>(
        '/teachers/$id',
        body: {
          'departmentId': departmentId,
        },
        fromData: (json) => TeacherDto.fromJson(json as Map<String, dynamic>),
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
  Future<Either<Failure, void>> deleteTeacher(String id) async {
    try {
      await _apiClient.delete<void>('/teachers/$id', fromData: (_) {});
      return right(null);
    } on ApiException catch (e) {
      return left(ServerFailure(e.message));
    } catch (e) {
      return left(UnknownFailure(e.toString()));
    }
  }

  @override
  Future<Either<Failure, void>> restoreTeacher(String id) async {
    try {
      await _apiClient.post<void>('/teachers/$id/restore', fromData: (_) {});
      return right(null);
    } on ApiException catch (e) {
      return left(ServerFailure(e.message));
    } catch (e) {
      return left(UnknownFailure(e.toString()));
    }
  }
}
