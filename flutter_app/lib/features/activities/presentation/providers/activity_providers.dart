import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../auth/presentation/providers/auth_providers.dart';
import '../../../../core/network/paginated_list.dart';
import '../../domain/models/activity_dto.dart';
import '../../domain/models/activity_enums.dart';
import '../../domain/repositories/activity_repository.dart';
import '../../data/repositories/activity_repository_impl.dart';

final activityRepositoryProvider = Provider<ActivityRepository>((ref) {
  final apiClient = ref.watch(apiClientProvider);
  return ActivityRepositoryImpl(apiClient);
});

class ActivitiesFilter {
  final String institutionId;
  final String? classId;
  final ActivityType? type;
  final ActivityStatus? status;
  final String? searchTerm;
  final String? sortBy;
  final String? dateFrom;
  final String? dateTo;
  final bool sortDescending;
  final int pageNumber;
  final int pageSize;

  ActivitiesFilter({
    required this.institutionId,
    this.classId,
    this.type,
    this.status,
    this.searchTerm,
    this.sortBy,
    this.dateFrom,
    this.dateTo,
    this.sortDescending = false,
    this.pageNumber = 1,
    this.pageSize = 20,
  });

  @override
  bool operator ==(Object other) {
    if (identical(this, other)) return true;
    return other is ActivitiesFilter &&
        other.institutionId == institutionId &&
        other.classId == classId &&
        other.type == type &&
        other.status == status &&
        other.searchTerm == searchTerm &&
        other.sortBy == sortBy &&
        other.dateFrom == dateFrom &&
        other.dateTo == dateTo &&
        other.sortDescending == sortDescending &&
        other.pageNumber == pageNumber &&
        other.pageSize == pageSize;
  }

  @override
  int get hashCode {
    return Object.hash(
      institutionId,
      classId,
      type,
      status,
      searchTerm,
      sortBy,
      dateFrom,
      dateTo,
      sortDescending,
      pageNumber,
      pageSize,
    );
  }
}

final activitiesListProvider =
    FutureProvider.family<PaginatedList<ActivityDto>, ActivitiesFilter>(
        (ref, filter) async {
  final repository = ref.watch(activityRepositoryProvider);
  final result = await repository.getActivities(
    institutionId: filter.institutionId,
    classId: filter.classId,
    type: filter.type,
    status: filter.status,
    searchTerm: filter.searchTerm,
    sortBy: filter.sortBy,
    dateFrom: filter.dateFrom,
    dateTo: filter.dateTo,
    sortDescending: filter.sortDescending,
    pageNumber: filter.pageNumber,
    pageSize: filter.pageSize,
  );

  return result.fold(
    (failure) => throw Exception(failure.message),
    (data) => data,
  );
});

final activityDetailsProvider =
    FutureProvider.family<ActivityDto, String>((ref, id) async {
  final repository = ref.watch(activityRepositoryProvider);
  final result = await repository.getActivity(id);

  return result.fold(
    (failure) => throw Exception(failure.message),
    (data) => data,
  );
});
