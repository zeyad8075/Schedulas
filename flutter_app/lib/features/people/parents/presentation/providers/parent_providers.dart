import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../../auth/presentation/providers/auth_providers.dart';
import '../../../../../core/network/paginated_list.dart';
import '../../domain/models/parent_dto.dart';
import '../../domain/repositories/parent_repository.dart';
import '../../data/repositories/parent_repository_impl.dart';

final parentRepositoryProvider = Provider<ParentRepository>((ref) {
  final apiClient = ref.watch(apiClientProvider);
  return ParentRepositoryImpl(apiClient);
});

// Filter class for parents
class ParentFilter {
  final String? searchTerm;
  final String? sortBy;
  final bool sortDescending;
  final int pageNumber;
  final int pageSize;

  ParentFilter({
    this.searchTerm,
    this.sortBy,
    this.sortDescending = false,
    this.pageNumber = 1,
    this.pageSize = 20,
  });

  @override
  bool operator ==(Object other) =>
      identical(this, other) ||
      other is ParentFilter &&
          runtimeType == other.runtimeType &&
          searchTerm == other.searchTerm &&
          sortBy == other.sortBy &&
          sortDescending == other.sortDescending &&
          pageNumber == other.pageNumber &&
          pageSize == other.pageSize;

  @override
  int get hashCode =>
      searchTerm.hashCode ^
      sortBy.hashCode ^
      sortDescending.hashCode ^
      pageNumber.hashCode ^
      pageSize.hashCode;
}

final parentsListProvider =
    FutureProvider.family<PaginatedList<ParentDto>, ParentFilter>(
        (ref, filter) async {
  final repository = ref.watch(parentRepositoryProvider);
  final result = await repository.getParents(
    searchTerm: filter.searchTerm,
    sortBy: filter.sortBy,
    sortDescending: filter.sortDescending,
    pageNumber: filter.pageNumber,
    pageSize: filter.pageSize,
  );

  return result.fold(
    (failure) => throw Exception(failure.message),
    (list) => list,
  );
});

final parentDetailsProvider =
    FutureProvider.family<ParentDto, String>((ref, id) async {
  final repository = ref.watch(parentRepositoryProvider);
  final result = await repository.getParentById(id);
  return result.fold(
    (failure) => throw Exception(failure.message),
    (parent) => parent,
  );
});
