import 'package:fpdart/fpdart.dart';
import '../../../../core/network/api_client.dart';
import '../../../../core/network/api_exception.dart';
import '../../../../core/errors/failures.dart';
import '../../../../core/network/paginated_list.dart';
import '../../domain/models/department_dto.dart';
import '../../domain/repositories/department_repository.dart';

class DepartmentRepositoryImpl implements DepartmentRepository {
  final ApiClient _apiClient;

  DepartmentRepositoryImpl(this._apiClient);

  @override
  Future<Either<Failure, PaginatedList<DepartmentDto>>> getDepartments({
    String? institutionId,
    String? searchTerm,
    bool? isActive,
    String? sortBy,
    bool sortDescending = false,
    int pageNumber = 1,
    int pageSize = 20,
  }) async {
    try {
      final queryParameters = {
        if (institutionId != null && institutionId.isNotEmpty)
          'InstitutionId': institutionId,
        if (searchTerm != null && searchTerm.isNotEmpty)
          'SearchTerm': searchTerm,
        if (isActive != null) 'IsActive': isActive,
        if (sortBy != null && sortBy.isNotEmpty) 'SortBy': sortBy,
        'SortDescending': sortDescending,
        'PageNumber': pageNumber,
        'PageSize': pageSize,
      };

      final result = await _apiClient.get<PaginatedList<DepartmentDto>>(
        '/departments',
        queryParameters: queryParameters,
        fromData: (json) => PaginatedList.fromJson(
          json,
          (itemJson) => DepartmentDto.fromJson(itemJson),
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
  Future<Either<Failure, DepartmentDto>> getDepartmentById(String id) async {
    try {
      final result = await _apiClient.get<DepartmentDto>(
        '/departments/$id',
        fromData: (json) =>
            DepartmentDto.fromJson(json as Map<String, dynamic>),
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
  Future<Either<Failure, DepartmentDto>> createDepartment(
    String institutionId,
    String name,
  ) async {
    try {
      final result = await _apiClient.post<DepartmentDto>(
        '/departments',
        body: {
          'institutionId': institutionId,
          'name': name,
        },
        fromData: (json) =>
            DepartmentDto.fromJson(json as Map<String, dynamic>),
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
  Future<Either<Failure, DepartmentDto>> updateDepartment(
    String id,
    String name,
    bool isActive,
  ) async {
    try {
      final result = await _apiClient.put<DepartmentDto>(
        '/departments/$id',
        body: {
          'name': name,
          'isActive': isActive,
        },
        fromData: (json) =>
            DepartmentDto.fromJson(json as Map<String, dynamic>),
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
  Future<Either<Failure, void>> deleteDepartment(String id) async {
    try {
      await _apiClient.delete<void>('/departments/$id', fromData: (_) {});
      return right(null);
    } on ApiException catch (e) {
      return left(ServerFailure(e.message));
    } catch (e) {
      return left(UnknownFailure(e.toString()));
    }
  }

  @override
  Future<Either<Failure, void>> restoreDepartment(String id) async {
    try {
      await _apiClient.post<void>('/departments/$id/restore', fromData: (_) {});
      return right(null);
    } on ApiException catch (e) {
      return left(ServerFailure(e.message));
    } catch (e) {
      return left(UnknownFailure(e.toString()));
    }
  }
}
