import 'package:fpdart/fpdart.dart';
import '../../../../core/errors/failures.dart';
import '../../../../core/network/api_client.dart';
import '../../../../core/network/paginated_list.dart';
import '../../domain/models/notification_dto.dart';
import '../../domain/models/notification_enums.dart';
import '../../domain/repositories/notification_repository.dart';

class NotificationRepositoryImpl implements NotificationRepository {
  final ApiClient _apiClient;

  NotificationRepositoryImpl(this._apiClient);

  @override
  Future<Either<Failure, PaginatedList<NotificationDto>>> getNotifications({
    bool? isRead,
    NotificationCategory? category,
    int pageNumber = 1,
    int pageSize = 20,
  }) async {
    try {
      final queryParams = <String, dynamic>{
        'pageNumber': pageNumber,
        'pageSize': pageSize,
      };
      if (isRead != null) queryParams['isRead'] = isRead;
      if (category != null) queryParams['category'] = category.toJson();

      final result = await _apiClient.get<PaginatedList<NotificationDto>>(
        '/api/v1/notifications',
        queryParameters: queryParams,
        fromData: (json) => PaginatedList.fromJson(
          json as Map<String, dynamic>,
          (item) => NotificationDto.fromJson(item),
        ),
      );
      return right(result);
    } catch (e) {
      return left(UnknownFailure(e.toString()));
    }
  }

  @override
  Future<Either<Failure, PaginatedList<NotificationDto>>>
      getUnreadNotifications({
    int pageNumber = 1,
    int pageSize = 20,
  }) async {
    try {
      final queryParams = <String, dynamic>{
        'pageNumber': pageNumber,
        'pageSize': pageSize,
      };

      final result = await _apiClient.get<PaginatedList<NotificationDto>>(
        '/api/v1/notifications/unread',
        queryParameters: queryParams,
        fromData: (json) => PaginatedList.fromJson(
          json as Map<String, dynamic>,
          (item) => NotificationDto.fromJson(item),
        ),
      );
      return right(result);
    } catch (e) {
      return left(UnknownFailure(e.toString()));
    }
  }

  @override
  Future<Either<Failure, void>> markAsRead(String id) async {
    try {
      await _apiClient.post<void>(
        '/api/v1/notifications/$id/read',
        fromData: (_) {},
      );
      return right(null);
    } catch (e) {
      return left(UnknownFailure(e.toString()));
    }
  }

  @override
  Future<Either<Failure, void>> markAllAsRead() async {
    try {
      await _apiClient.post<void>(
        '/api/v1/notifications/read-all',
        fromData: (_) {},
      );
      return right(null);
    } catch (e) {
      return left(UnknownFailure(e.toString()));
    }
  }

  @override
  Future<Either<Failure, void>> deleteNotification(String id) async {
    try {
      await _apiClient.delete<void>(
        '/api/v1/notifications/$id',
        fromData: (_) {},
      );
      return right(null);
    } catch (e) {
      return left(UnknownFailure(e.toString()));
    }
  }
}
