import 'package:fpdart/fpdart.dart';
import '../../../../../core/network/api_client.dart';
import '../../../../../core/network/api_exception.dart';
import '../../../../../core/errors/failures.dart';
import '../../../../../core/network/paginated_list.dart';
import '../../domain/models/parent_dto.dart';
import '../../domain/repositories/parent_repository.dart';

class ParentRepositoryImpl implements ParentRepository {
  final ApiClient _apiClient;

  ParentRepositoryImpl(this._apiClient);

  @override
  Future<Either<Failure, ParentDto>> getParentById(String id) async {
    try {
      final result = await _apiClient.get<ParentDto>(
        '/parents/$id',
        fromData: (json) => ParentDto.fromJson(json as Map<String, dynamic>),
      );
      return right(result);
    } on ApiException catch (e) {
      if (e.statusCode == 404) {
                return left(const ServerFailure('Parent not found'));
              }
      return left(ServerFailure(e.message));
    } catch (e) {
      return left(UnknownFailure(e.toString()));
    }
  }

  @override
  Future<Either<Failure, PaginatedList<ParentDto>>> getParents({
    String? searchTerm,
    String? sortBy,
    bool sortDescending = false,
    int pageNumber = 1,
    int pageSize = 20,
  }) async {
    try {
      final queryParameters = {
        if (searchTerm != null && searchTerm.isNotEmpty)
          'SearchTerm': searchTerm,
        if (sortBy != null && sortBy.isNotEmpty) 'SortBy': sortBy,
        'SortDescending': sortDescending,
        'PageNumber': pageNumber,
        'PageSize': pageSize,
      };

      final result = await _apiClient.get<PaginatedList<ParentDto>>(
        '/parents',
        queryParameters: queryParameters,
        fromData: (json) => PaginatedList.fromJson(
          json,
          (itemJson) => ParentDto.fromJson(itemJson),
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
  Future<Either<Failure, ParentDto>> createParent(String profileId) async {
    try {
      final result = await _apiClient.post<ParentDto>(
        '/parents',
        body: {
          'profileId': profileId,
        },
        fromData: (json) => ParentDto.fromJson(json as Map<String, dynamic>),
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
  Future<Either<Failure, void>> deleteParent(String id) async {
    try {
      await _apiClient.delete<void>('/parents/$id', fromData: (_) {});
      return right(null);
    } on ApiException catch (e) {
      return left(ServerFailure(e.message));
    } catch (e) {
      return left(UnknownFailure(e.toString()));
    }
  }

  @override
  Future<Either<Failure, void>> restoreParent(String id) async {
    try {
      await _apiClient.post<void>('/parents/$id/restore', fromData: (_) {});
      return right(null);
    } on ApiException catch (e) {
      return left(ServerFailure(e.message));
    } catch (e) {
      return left(UnknownFailure(e.toString()));
    }
  }
}
