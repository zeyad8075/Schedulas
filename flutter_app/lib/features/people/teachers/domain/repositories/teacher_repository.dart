import 'package:fpdart/fpdart.dart';
import '../../../../../core/errors/failures.dart';
import '../../../../../core/network/paginated_list.dart';
import '../models/teacher_dto.dart';

abstract class TeacherRepository {
  Future<Either<Failure, TeacherDto>> getTeacherById(String id);

  Future<Either<Failure, PaginatedList<TeacherDto>>> getTeachers(
    String institutionId, {
    String? departmentId,
    String? searchTerm,
    String? sortBy,
    bool sortDescending = false,
    int pageNumber = 1,
    int pageSize = 20,
  });

  Future<Either<Failure, TeacherDto>> createTeacher(
    String profileId,
    String? departmentId,
  );

  Future<Either<Failure, TeacherDto>> updateTeacher(
    String id,
    String? departmentId,
  );

  Future<Either<Failure, void>> deleteTeacher(String id);

  Future<Either<Failure, void>> restoreTeacher(String id);
}
