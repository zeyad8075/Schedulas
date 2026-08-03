import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../../auth/presentation/providers/auth_providers.dart';
import '../../../../../core/network/paginated_list.dart';
import '../../../domain/models/user_role.dart';
import '../../domain/models/profile_dto.dart';
import '../../domain/repositories/profile_repository.dart';
import '../../data/repositories/profile_repository_impl.dart';

final profileRepositoryProvider = Provider<ProfileRepository>((ref) {
  final apiClient = ref.watch(apiClientProvider);
  return ProfileRepositoryImpl(apiClient);
});

final myProfileProvider = FutureProvider<ProfileDto>((ref) async {
  final repository = ref.watch(profileRepositoryProvider);
  final result = await repository.getMyProfile();
  return result.fold(
    (failure) => throw Exception(failure.message),
    (profile) => profile,
  );
});

final profileDetailsProvider =
    FutureProvider.family<ProfileDto, String>((ref, id) async {
  final repository = ref.watch(profileRepositoryProvider);
  final result = await repository.getProfileById(id);
  return result.fold(
    (failure) => throw Exception(failure.message),
    (profile) => profile,
  );
});

// A provider to handle filters for the profile list
class ProfileFilter {
  final String institutionId;
  final String? searchTerm;
  final UserRole? role;
  final String? sortBy;
  final bool sortDescending;
  final int pageNumber;
  final int pageSize;

  ProfileFilter({
    required this.institutionId,
    this.searchTerm,
    this.role,
    this.sortBy,
    this.sortDescending = false,
    this.pageNumber = 1,
    this.pageSize = 20,
  });

  @override
  bool operator ==(Object other) =>
      identical(this, other) ||
      other is ProfileFilter &&
          runtimeType == other.runtimeType &&
          institutionId == other.institutionId &&
          searchTerm == other.searchTerm &&
          role == other.role &&
          sortBy == other.sortBy &&
          sortDescending == other.sortDescending &&
          pageNumber == other.pageNumber &&
          pageSize == other.pageSize;

  @override
  int get hashCode =>
      institutionId.hashCode ^
      searchTerm.hashCode ^
      role.hashCode ^
      sortBy.hashCode ^
      sortDescending.hashCode ^
      pageNumber.hashCode ^
      pageSize.hashCode;
}

final profilesListProvider =
    FutureProvider.family<PaginatedList<ProfileDto>, ProfileFilter>(
        (ref, filter) async {
  final repository = ref.watch(profileRepositoryProvider);
  final result = await repository.getProfiles(
    filter.institutionId,
    searchTerm: filter.searchTerm,
    role: filter.role,
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
