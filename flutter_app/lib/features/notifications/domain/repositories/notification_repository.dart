import 'package:fpdart/fpdart.dart';
import '../../../../core/errors/failures.dart';
import '../../../../core/network/paginated_list.dart';
import '../models/notification_dto.dart';
import '../models/notification_enums.dart';

abstract class NotificationRepository {
  Future<Either<Failure, PaginatedList<NotificationDto>>> getNotifications({
    bool? isRead,
    NotificationCategory? category,
    int pageNumber = 1,
    int pageSize = 20,
  });

  Future<Either<Failure, PaginatedList<NotificationDto>>>
      getUnreadNotifications({
    int pageNumber = 1,
    int pageSize = 20,
  });

  Future<Either<Failure, void>> markAsRead(String id);

  Future<Either<Failure, void>> markAllAsRead();

  Future<Either<Failure, void>> deleteNotification(String id);
}
