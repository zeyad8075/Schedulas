import 'package:fpdart/fpdart.dart';
import '../../../../core/network/api_client.dart';
import '../../../../core/network/api_exception.dart';
import '../../../../core/errors/failures.dart';
import '../../../../core/network/paginated_list.dart';
import '../../domain/models/class_dto.dart';
import '../../domain/repositories/class_repository.dart';

class ClassRepositoryImpl implements ClassRepository {
  final ApiClient _apiClient;

  ClassRepositoryImpl(this._apiClient);

  @override
  Future<Either<Failure, PaginatedList<ClassDto>>> getClasses({
    String? courseId,
    String? academicTermId,
    String? searchTerm,
    bool? isActive,
    String? sortBy,
    bool sortDescending = false,
    int pageNumber = 1,
    int pageSize = 20,
  }) async {
    try {
      final queryParameters = {
        if (courseId != null && courseId.isNotEmpty) 'CourseId': courseId,
        if (academicTermId != null && academicTermId.isNotEmpty)
          'AcademicTermId': academicTermId,
        if (searchTerm != null && searchTerm.isNotEmpty)
          'SearchTerm': searchTerm,
        if (isActive != null) 'IsActive': isActive,
        if (sortBy != null && sortBy.isNotEmpty) 'SortBy': sortBy,
        'SortDescending': sortDescending,
        'PageNumber': pageNumber,
        'PageSize': pageSize,
      };

      final result = await _apiClient.get<PaginatedList<ClassDto>>(
        '/classes',
        queryParameters: queryParameters,
        fromData: (json) => PaginatedList.fromJson(
          json,
          (itemJson) => ClassDto.fromJson(itemJson),
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
  Future<Either<Failure, ClassDto>> getClassById(String id) async {
    try {
      final result = await _apiClient.get<ClassDto>(
        '/classes/$id',
        fromData: (json) => ClassDto.fromJson(json as Map<String, dynamic>),
      );
      return right(result);
    } on ApiException catch (e) {
      if (e.statusCode == 404) return left(const ServerFailure('Not found'));
      return left(ServerFailure(e.message));
    } catch (e) {
      return left(UnknownFailure(e.toString()));
    }
  }

  @override
  Future<Either<Failure, ClassDto>> createClass(
    String courseId,
    String academicTermId,
    String name,
  ) async {
    try {
      final result = await _apiClient.post<ClassDto>(
        '/classes',
        body: {
          'courseId': courseId,
          'academicTermId': academicTermId,
          'name': name,
        },
        fromData: (json) => ClassDto.fromJson(json as Map<String, dynamic>),
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
  Future<Either<Failure, ClassDto>> updateClass(
    String id,
    String name,
    bool isActive,
  ) async {
    try {
      final result = await _apiClient.put<ClassDto>(
        '/classes/$id',
        body: {
          'name': name,
          'isActive': isActive,
        },
        fromData: (json) => ClassDto.fromJson(json as Map<String, dynamic>),
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
  Future<Either<Failure, void>> deleteClass(String id) async {
    try {
      await _apiClient.delete<void>('/classes/$id', fromData: (_) {});
      return right(null);
    } on ApiException catch (e) {
      return left(ServerFailure(e.message));
    } catch (e) {
      return left(UnknownFailure(e.toString()));
    }
  }

  @override
  Future<Either<Failure, void>> restoreClass(String id) async {
    try {
      await _apiClient.post<void>('/classes/$id/restore', fromData: (_) {});
      return right(null);
    } on ApiException catch (e) {
      return left(ServerFailure(e.message));
    } catch (e) {
      return left(UnknownFailure(e.toString()));
    }
  }
}
