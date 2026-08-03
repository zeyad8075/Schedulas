import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../auth/presentation/providers/auth_providers.dart';
import '../../domain/repositories/institution_repository.dart';
import '../../data/repositories/institution_repository_impl.dart';
import '../../domain/repositories/department_repository.dart';
import '../../data/repositories/department_repository_impl.dart';
import '../../domain/repositories/program_repository.dart';
import '../../data/repositories/program_repository_impl.dart';
import '../../domain/repositories/course_repository.dart';
import '../../data/repositories/course_repository_impl.dart';
import '../../domain/repositories/class_repository.dart';
import '../../data/repositories/class_repository_impl.dart';
import '../../domain/repositories/academic_calendar_repository.dart';
import '../../data/repositories/academic_calendar_repository_impl.dart';
import '../../domain/models/institution_dto.dart';
import '../../domain/models/department_dto.dart';
import '../../domain/models/program_dto.dart';
import '../../domain/models/course_dto.dart';
import '../../domain/models/class_dto.dart';
import '../../domain/models/academic_term_dto.dart';
import '../../domain/models/holiday_dto.dart';
import '../../../../core/network/paginated_list.dart';

// --- Repositories ---

final institutionRepositoryProvider = Provider<InstitutionRepository>((ref) {
  return InstitutionRepositoryImpl(ref.watch(apiClientProvider));
});

final departmentRepositoryProvider = Provider<DepartmentRepository>((ref) {
  return DepartmentRepositoryImpl(ref.watch(apiClientProvider));
});

final programRepositoryProvider = Provider<ProgramRepository>((ref) {
  return ProgramRepositoryImpl(ref.watch(apiClientProvider));
});

final courseRepositoryProvider = Provider<CourseRepository>((ref) {
  return CourseRepositoryImpl(ref.watch(apiClientProvider));
});

final classRepositoryProvider = Provider<ClassRepository>((ref) {
  return ClassRepositoryImpl(ref.watch(apiClientProvider));
});

final academicCalendarRepositoryProvider =
    Provider<AcademicCalendarRepository>((ref) {
  return AcademicCalendarRepositoryImpl(ref.watch(apiClientProvider));
});

// --- State Parameters ---

class PaginationParams {
  final int pageNumber;
  final int pageSize;
  final String? searchTerm;
  final bool? isActive;
  final bool? isSuspended; // specific for institutions

  const PaginationParams({
    this.pageNumber = 1,
    this.pageSize = 20,
    this.searchTerm,
    this.isActive,
    this.isSuspended,
  });

  @override
  bool operator ==(Object other) {
    if (identical(this, other)) return true;
    return other is PaginationParams &&
        other.pageNumber == pageNumber &&
        other.pageSize == pageSize &&
        other.searchTerm == searchTerm &&
        other.isActive == isActive &&
        other.isSuspended == isSuspended;
  }

  @override
  int get hashCode =>
      Object.hash(pageNumber, pageSize, searchTerm, isActive, isSuspended);
}

// --- Notifiers ---

final institutionsListProvider = FutureProvider.autoDispose
    .family<PaginatedList<InstitutionDto>, PaginationParams>(
        (ref, params) async {
  final repository = ref.watch(institutionRepositoryProvider);
  final result = await repository.getInstitutions(
    pageNumber: params.pageNumber,
    pageSize: params.pageSize,
    searchTerm: params.searchTerm,
    isSuspended: params.isSuspended,
  );
  return result.fold(
    (l) => throw Exception(l.message),
    (r) => r,
  );
});

final departmentsListProvider = FutureProvider.autoDispose
    .family<PaginatedList<DepartmentDto>, PaginationParams>(
        (ref, params) async {
  final repository = ref.watch(departmentRepositoryProvider);
  final result = await repository.getDepartments(
    pageNumber: params.pageNumber,
    pageSize: params.pageSize,
    searchTerm: params.searchTerm,
    isActive: params.isActive,
  );
  return result.fold(
    (l) => throw Exception(l.message),
    (r) => r,
  );
});

final programsListProvider = FutureProvider.autoDispose
    .family<PaginatedList<ProgramDto>, PaginationParams>((ref, params) async {
  final repository = ref.watch(programRepositoryProvider);
  final result = await repository.getPrograms(
    pageNumber: params.pageNumber,
    pageSize: params.pageSize,
    searchTerm: params.searchTerm,
    isActive: params.isActive,
  );
  return result.fold(
    (l) => throw Exception(l.message),
    (r) => r,
  );
});

final coursesListProvider = FutureProvider.autoDispose
    .family<PaginatedList<CourseDto>, PaginationParams>((ref, params) async {
  final repository = ref.watch(courseRepositoryProvider);
  final result = await repository.getCourses(
    pageNumber: params.pageNumber,
    pageSize: params.pageSize,
    searchTerm: params.searchTerm,
    isActive: params.isActive,
  );
  return result.fold(
    (l) => throw Exception(l.message),
    (r) => r,
  );
});

final classesListProvider = FutureProvider.autoDispose
    .family<PaginatedList<ClassDto>, PaginationParams>((ref, params) async {
  final repository = ref.watch(classRepositoryProvider);
  final result = await repository.getClasses(
    pageNumber: params.pageNumber,
    pageSize: params.pageSize,
    searchTerm: params.searchTerm,
    isActive: params.isActive,
  );
  return result.fold(
    (l) => throw Exception(l.message),
    (r) => r,
  );
});

final academicTermsListProvider = FutureProvider.autoDispose
    .family<PaginatedList<AcademicTermDto>, PaginationParams>(
        (ref, params) async {
  final repository = ref.watch(academicCalendarRepositoryProvider);
  final result = await repository.getAcademicTerms(
    pageNumber: params.pageNumber,
    pageSize: params.pageSize,
    searchTerm: params.searchTerm,
    isActive: params.isActive,
  );
  return result.fold(
    (l) => throw Exception(l.message),
    (r) => r,
  );
});

final holidaysListProvider = FutureProvider.autoDispose
    .family<PaginatedList<HolidayDto>, PaginationParams>((ref, params) async {
  final repository = ref.watch(academicCalendarRepositoryProvider);
  final result = await repository.getHolidays(
    pageNumber: params.pageNumber,
    pageSize: params.pageSize,
    searchTerm: params.searchTerm,
  );
  return result.fold(
    (l) => throw Exception(l.message),
    (r) => r,
  );
});
