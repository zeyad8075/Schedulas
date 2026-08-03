import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../../auth/presentation/providers/auth_providers.dart';
import '../../../../../core/network/paginated_list.dart';
import '../../domain/models/student_dto.dart';
import '../../domain/repositories/student_repository.dart';
import '../../data/repositories/student_repository_impl.dart';

final studentRepositoryProvider = Provider<StudentRepository>((ref) {
  final apiClient = ref.watch(apiClientProvider);
  return StudentRepositoryImpl(apiClient);
});

// Filter class for students
class StudentFilter {
  final String institutionId;
  final String? searchTerm;
  final String? sortBy;
  final bool sortDescending;
  final int pageNumber;
  final int pageSize;

  StudentFilter({
    required this.institutionId,
    this.searchTerm,
    this.sortBy,
    this.sortDescending = false,
    this.pageNumber = 1,
    this.pageSize = 20,
  });

  @override
  bool operator ==(Object other) =>
      identical(this, other) ||
      other is StudentFilter &&
          runtimeType == other.runtimeType &&
          institutionId == other.institutionId &&
          searchTerm == other.searchTerm &&
          sortBy == other.sortBy &&
          sortDescending == other.sortDescending &&
          pageNumber == other.pageNumber &&
          pageSize == other.pageSize;

  @override
  int get hashCode =>
      institutionId.hashCode ^
      searchTerm.hashCode ^
      sortBy.hashCode ^
      sortDescending.hashCode ^
      pageNumber.hashCode ^
      pageSize.hashCode;
}

final studentsListProvider =
    FutureProvider.family<PaginatedList<StudentDto>, StudentFilter>(
        (ref, filter) async {
  final repository = ref.watch(studentRepositoryProvider);
  final result = await repository.getStudents(
    filter.institutionId,
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

final studentDetailsProvider =
    FutureProvider.family<StudentDto, String>((ref, id) async {
  final repository = ref.watch(studentRepositoryProvider);
  final result = await repository.getStudentById(id);
  return result.fold(
    (failure) => throw Exception(failure.message),
    (student) => student,
  );
});
