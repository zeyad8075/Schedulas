import 'package:fpdart/fpdart.dart';
import '../../../../core/network/api_client.dart';
import '../../../../core/network/api_exception.dart';
import '../../../../core/errors/failures.dart';
import '../../../../core/network/paginated_list.dart';
import '../../domain/models/program_dto.dart';
import '../../domain/repositories/program_repository.dart';

class ProgramRepositoryImpl implements ProgramRepository {
  final ApiClient _apiClient;

  ProgramRepositoryImpl(this._apiClient);

  @override
  Future<Either<Failure, PaginatedList<ProgramDto>>> getPrograms({
    String? departmentId,
    String? searchTerm,
    bool? isActive,
    String? sortBy,
    bool sortDescending = false,
    int pageNumber = 1,
    int pageSize = 20,
  }) async {
    try {
      final queryParameters = {
        if (departmentId != null && departmentId.isNotEmpty)
          'DepartmentId': departmentId,
        if (searchTerm != null && searchTerm.isNotEmpty)
          'SearchTerm': searchTerm,
        if (isActive != null) 'IsActive': isActive,
        if (sortBy != null && sortBy.isNotEmpty) 'SortBy': sortBy,
        'SortDescending': sortDescending,
        'PageNumber': pageNumber,
        'PageSize': pageSize,
      };

      final result = await _apiClient.get<PaginatedList<ProgramDto>>(
        '/programs',
        queryParameters: queryParameters,
        fromData: (json) => PaginatedList.fromJson(
          json,
          (itemJson) => ProgramDto.fromJson(itemJson),
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
  Future<Either<Failure, ProgramDto>> getProgramById(String id) async {
    try {
      final result = await _apiClient.get<ProgramDto>(
        '/programs/$id',
        fromData: (json) => ProgramDto.fromJson(json as Map<String, dynamic>),
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
  Future<Either<Failure, ProgramDto>> createProgram(
    String departmentId,
    String name,
  ) async {
    try {
      final result = await _apiClient.post<ProgramDto>(
        '/programs',
        body: {
          'departmentId': departmentId,
          'name': name,
        },
        fromData: (json) => ProgramDto.fromJson(json as Map<String, dynamic>),
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
  Future<Either<Failure, ProgramDto>> updateProgram(
    String id,
    String name,
    bool isActive,
  ) async {
    try {
      final result = await _apiClient.put<ProgramDto>(
        '/programs/$id',
        body: {
          'name': name,
          'isActive': isActive,
        },
        fromData: (json) => ProgramDto.fromJson(json as Map<String, dynamic>),
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
  Future<Either<Failure, void>> deleteProgram(String id) async {
    try {
      await _apiClient.delete<void>('/programs/$id', fromData: (_) {});
      return right(null);
    } on ApiException catch (e) {
      return left(ServerFailure(e.message));
    } catch (e) {
      return left(UnknownFailure(e.toString()));
    }
  }

  @override
  Future<Either<Failure, void>> restoreProgram(String id) async {
    try {
      await _apiClient.post<void>('/programs/$id/restore', fromData: (_) {});
      return right(null);
    } on ApiException catch (e) {
      return left(ServerFailure(e.message));
    } catch (e) {
      return left(UnknownFailure(e.toString()));
    }
  }
}
