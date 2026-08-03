import 'package:fpdart/fpdart.dart';
import '../../../../core/network/api_client.dart';
import '../../../../core/network/api_exception.dart';
import '../../../../core/errors/failures.dart';
import '../../../../core/network/paginated_list.dart';
import '../../domain/models/institution_dto.dart';
import '../../domain/repositories/institution_repository.dart';

class InstitutionRepositoryImpl implements InstitutionRepository {
  final ApiClient _apiClient;

  InstitutionRepositoryImpl(this._apiClient);

  @override
  Future<Either<Failure, PaginatedList<InstitutionDto>>> getInstitutions({
    String? searchTerm,
    bool? isSuspended,
    String? sortBy,
    bool sortDescending = false,
    int pageNumber = 1,
    int pageSize = 20,
  }) async {
    try {
      final queryParameters = {
        if (searchTerm != null && searchTerm.isNotEmpty)
          'SearchTerm': searchTerm,
        if (isSuspended != null) 'IsSuspended': isSuspended,
        if (sortBy != null && sortBy.isNotEmpty) 'SortBy': sortBy,
        'SortDescending': sortDescending,
        'PageNumber': pageNumber,
        'PageSize': pageSize,
      };

      final result = await _apiClient.get<PaginatedList<InstitutionDto>>(
        '/institutions',
        queryParameters: queryParameters,
        fromData: (json) => PaginatedList.fromJson(
          json,
          (itemJson) => InstitutionDto.fromJson(itemJson),
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
  Future<Either<Failure, InstitutionDto>> getInstitutionById(String id) async {
    try {
      final result = await _apiClient.get<InstitutionDto>(
        '/institutions/$id',
        fromData: (json) =>
            InstitutionDto.fromJson(json as Map<String, dynamic>),
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
  Future<Either<Failure, InstitutionDto>> createInstitution(
    String name,
    InstitutionType type,
    String timezone,
  ) async {
    try {
      final result = await _apiClient.post<InstitutionDto>(
        '/institutions',
        body: {
          'name': name,
          'type': type.toJson(),
          'timezone': timezone,
        },
        fromData: (json) =>
            InstitutionDto.fromJson(json as Map<String, dynamic>),
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
  Future<Either<Failure, InstitutionDto>> updateInstitution(
    String id,
    String name,
    String? logoUrl,
    String timezone,
  ) async {
    try {
      final result = await _apiClient.put<InstitutionDto>(
        '/institutions/$id',
        body: {
          'name': name,
          'logoUrl': logoUrl,
          'timezone': timezone,
        },
        fromData: (json) =>
            InstitutionDto.fromJson(json as Map<String, dynamic>),
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
  Future<Either<Failure, void>> suspendInstitution(String id) async {
    try {
      await _apiClient.post<void>('/institutions/$id/suspend',
          fromData: (_) {});
      return right(null);
    } on ApiException catch (e) {
      return left(ServerFailure(e.message));
    } catch (e) {
      return left(UnknownFailure(e.toString()));
    }
  }

  @override
  Future<Either<Failure, void>> reactivateInstitution(String id) async {
    try {
      await _apiClient.post<void>('/institutions/$id/reactivate',
          fromData: (_) {});
      return right(null);
    } on ApiException catch (e) {
      return left(ServerFailure(e.message));
    } catch (e) {
      return left(UnknownFailure(e.toString()));
    }
  }

  @override
  Future<Either<Failure, void>> deleteInstitution(String id) async {
    try {
      await _apiClient.delete<void>('/institutions/$id', fromData: (_) {});
      return right(null);
    } on ApiException catch (e) {
      return left(ServerFailure(e.message));
    } catch (e) {
      return left(UnknownFailure(e.toString()));
    }
  }

  @override
  Future<Either<Failure, void>> restoreInstitution(String id) async {
    try {
      await _apiClient.post<void>('/institutions/$id/restore',
          fromData: (_) {});
      return right(null);
    } on ApiException catch (e) {
      return left(ServerFailure(e.message));
    } catch (e) {
      return left(UnknownFailure(e.toString()));
    }
  }
}
