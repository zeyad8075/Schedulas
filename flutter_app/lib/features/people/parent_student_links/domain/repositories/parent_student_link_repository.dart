import 'package:fpdart/fpdart.dart';
import '../../../../../core/errors/failures.dart';
import '../../../students/domain/models/student_dto.dart';

abstract class ParentStudentLinkRepository {
  Future<Either<Failure, List<StudentDto>>> getParentStudents(String parentId);

  Future<Either<Failure, void>> linkParentToStudent(
      String parentId, String studentId);

  Future<Either<Failure, void>> unlinkParentFromStudent(String linkId);
}
