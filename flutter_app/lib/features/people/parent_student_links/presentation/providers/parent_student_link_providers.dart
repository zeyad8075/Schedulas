import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../../auth/presentation/providers/auth_providers.dart';
import '../../../students/domain/models/student_dto.dart';
import '../../domain/repositories/parent_student_link_repository.dart';
import '../../data/repositories/parent_student_link_repository_impl.dart';

final parentStudentLinkRepositoryProvider =
    Provider<ParentStudentLinkRepository>((ref) {
  final apiClient = ref.watch(apiClientProvider);
  return ParentStudentLinkRepositoryImpl(apiClient);
});

final parentStudentsProvider =
    FutureProvider.family<List<StudentDto>, String>((ref, parentId) async {
  final repository = ref.watch(parentStudentLinkRepositoryProvider);
  final result = await repository.getParentStudents(parentId);
  return result.fold(
    (failure) => throw Exception(failure.message),
    (students) => students,
  );
});
