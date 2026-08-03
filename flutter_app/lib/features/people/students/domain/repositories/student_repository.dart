import 'package:fpdart/fpdart.dart';
import '../../../../../core/errors/failures.dart';
import '../../../../../core/network/paginated_list.dart';
import '../models/student_dto.dart';

abstract class StudentRepository {
  Future<Either<Failure, StudentDto>> getStudentById(String id);

  Future<Either<Failure, PaginatedList<StudentDto>>> getStudents(
    String institutionId, {
    String? searchTerm,
    String? sortBy,
    bool sortDescending = false,
    int pageNumber = 1,
    int pageSize = 20,
  });

  Future<Either<Failure, StudentDto>> createStudent(
    String profileId,
    String? studentNumber,
  );

  Future<Either<Failure, StudentDto>> updateStudent(
    String id,
    String? studentNumber,
  );

  Future<Either<Failure, void>> deleteStudent(String id);

  Future<Either<Failure, void>> restoreStudent(String id);
}
