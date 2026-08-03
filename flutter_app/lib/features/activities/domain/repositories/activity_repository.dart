import 'package:fpdart/fpdart.dart';
import '../../../../core/errors/failures.dart';
import '../../../../core/network/paginated_list.dart';
import '../models/activity_dto.dart';
import '../models/activity_enums.dart';

abstract class ActivityRepository {
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
  });

  Future<Either<Failure, ActivityDto>> getActivity(String id);

  Future<Either<Failure, ActivitySubmissionResult>> createActivity(
      Map<String, dynamic> data);

  Future<Either<Failure, ActivitySubmissionResult>> updateActivity(
      String id, Map<String, dynamic> data);

  Future<Either<Failure, void>> deleteActivity(String id);

  Future<Either<Failure, void>> cancelActivity(String id);

  Future<Either<Failure, void>> restoreActivity(String id);

  Future<Either<Failure, ActivityDto>> overrideActivity(
      Map<String, dynamic> data);
}
