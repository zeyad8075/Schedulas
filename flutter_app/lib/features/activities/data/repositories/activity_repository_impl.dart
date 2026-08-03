import 'package:fpdart/fpdart.dart';
import '../../../../core/network/api_client.dart';
import '../../../../core/errors/failures.dart';
import '../../../../core/network/paginated_list.dart';
import '../../domain/models/activity_dto.dart';
import '../../domain/models/activity_enums.dart';
import '../../domain/repositories/activity_repository.dart';

class ActivityRepositoryImpl implements ActivityRepository {
  final ApiClient _apiClient;

  ActivityRepositoryImpl(this._apiClient);

  @override
  Future<Either<Failure, PaginatedList<ActivityDto>>> getActivities({
    required String institutionId,
    String? classId,
    ActivityType? type,
    ActivityStatus? status,
    String? searchTerm,
    String? sortBy,
    String? dateFrom,
    String? dateTo,
    bool sortDescending = false,
    int pageNumber = 1,
    int pageSize = 20,
  }) async {
    try {
      final result = await _apiClient.get<PaginatedList<ActivityDto>>(
        '/api/v1/activities',
        queryParameters: {
          'institutionId': institutionId,
          if (classId != null) 'classId': classId,
          if (type != null) 'type': type.value,
          if (status != null) 'status': status.value,
          if (searchTerm != null) 'searchTerm': searchTerm,
          if (sortBy != null) 'sortBy': sortBy,
          if (dateFrom != null) 'dateFrom': dateFrom,
          if (dateTo != null) 'dateTo': dateTo,
          'sortDescending': sortDescending,
          'pageNumber': pageNumber,
          'pageSize': pageSize,
        },
        fromData: (json) => PaginatedList.fromJson(
          json,
          (itemJson) => ActivityDto.fromJson(itemJson),
        ),
      );
      return right(result);
    } catch (e) {
      return left(UnknownFailure(e.toString()));
    }
  }

  @override
  Future<Either<Failure, ActivityDto>> getActivity(String id) async {
    try {
      final result = await _apiClient.get<ActivityDto>(
        '/api/v1/activities/$id',
        fromData: (json) => ActivityDto.fromJson(json as Map<String, dynamic>),
      );
      return right(result);
    } catch (e) {
      return left(UnknownFailure(e.toString()));
    }
  }

  @override
  Future<Either<Failure, ActivitySubmissionResult>> createActivity(
      Map<String, dynamic> payload) async {
    try {
      final result = await _apiClient.post<ActivitySubmissionResult>(
        '/api/v1/activities',
        body: payload,
        fromData: (json) =>
            ActivitySubmissionResult.fromJson(json as Map<String, dynamic>),
      );
      return right(result);
    } catch (e) {
      return left(UnknownFailure(e.toString()));
    }
  }

  @override
  Future<Either<Failure, ActivitySubmissionResult>> updateActivity(
      String id, Map<String, dynamic> data) async {
    try {
      final result = await _apiClient.put<ActivitySubmissionResult>(
        '/api/v1/activities/$id',
        body: data,
        fromData: (json) =>
            ActivitySubmissionResult.fromJson(json as Map<String, dynamic>),
      );
      return right(result);
    } catch (e) {
      return left(UnknownFailure(e.toString()));
    }
  }

  @override
  Future<Either<Failure, void>> deleteActivity(String id) async {
    try {
      await _apiClient.delete<void>(
        '/api/v1/activities/$id',
        fromData: (_) {},
      );
      return right(null);
    } catch (e) {
      return left(UnknownFailure(e.toString()));
    }
  }

  @override
  Future<Either<Failure, void>> restoreActivity(String id) async {
    try {
      await _apiClient.post<void>(
        '/api/v1/activities/$id/restore',
        fromData: (_) {},
      );
      return right(null);
    } catch (e) {
      return left(UnknownFailure(e.toString()));
    }
  }

  @override
  Future<Either<Failure, void>> cancelActivity(String id) async {
    try {
      await _apiClient.post<void>(
        '/api/v1/activities/$id/cancel',
        fromData: (_) {},
      );
      return right(null);
    } catch (e) {
      return left(UnknownFailure(e.toString()));
    }
  }

  @override
  Future<Either<Failure, ActivityDto>> overrideActivity(
      Map<String, dynamic> data) async {
    try {
      final result = await _apiClient.post<ActivityDto>(
        '/api/v1/activities/override',
        body: data,
        fromData: (json) => ActivityDto.fromJson(json as Map<String, dynamic>),
      );
      return right(result);
    } catch (e) {
      return left(UnknownFailure(e.toString()));
    }
  }
}
