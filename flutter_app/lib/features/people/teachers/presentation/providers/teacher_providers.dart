import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../../auth/presentation/providers/auth_providers.dart';
import '../../../../../core/network/paginated_list.dart';
import '../../domain/models/teacher_dto.dart';
import '../../domain/repositories/teacher_repository.dart';
import '../../data/repositories/teacher_repository_impl.dart';

final teacherRepositoryProvider = Provider<TeacherRepository>((ref) {
  final apiClient = ref.watch(apiClientProvider);
  return TeacherRepositoryImpl(apiClient);
});

// Filter class for teachers
class TeacherFilter {
  final String institutionId;
  final String? departmentId;
  final String? searchTerm;
  final String? sortBy;
  final bool sortDescending;
  final int pageNumber;
  final int pageSize;

  TeacherFilter({
    required this.institutionId,
    this.departmentId,
    this.searchTerm,
    this.sortBy,
    this.sortDescending = false,
    this.pageNumber = 1,
    this.pageSize = 20,
  });

  @override
  bool operator ==(Object other) =>
      identical(this, other) ||
      other is TeacherFilter &&
          runtimeType == other.runtimeType &&
          institutionId == other.institutionId &&
          departmentId == other.departmentId &&
          searchTerm == other.searchTerm &&
          sortBy == other.sortBy &&
          sortDescending == other.sortDescending &&
          pageNumber == other.pageNumber &&
          pageSize == other.pageSize;

  @override
  int get hashCode =>
      institutionId.hashCode ^
      departmentId.hashCode ^
      searchTerm.hashCode ^
      sortBy.hashCode ^
      sortDescending.hashCode ^
      pageNumber.hashCode ^
      pageSize.hashCode;
}

final teachersListProvider =
    FutureProvider.family<PaginatedList<TeacherDto>, TeacherFilter>(
        (ref, filter) async {
  final repository = ref.watch(teacherRepositoryProvider);
  final result = await repository.getTeachers(
    filter.institutionId,
    departmentId: filter.departmentId,
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

final teacherDetailsProvider =
    FutureProvider.family<TeacherDto, String>((ref, id) async {
  final repository = ref.watch(teacherRepositoryProvider);
  final result = await repository.getTeacherById(id);
  return result.fold(
    (failure) => throw Exception(failure.message),
    (teacher) => teacher,
  );
});
