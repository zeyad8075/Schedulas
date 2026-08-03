import 'package:fpdart/fpdart.dart';
import '../../../../core/errors/failures.dart';
import '../../../../core/network/paginated_list.dart';
import '../models/class_dto.dart';

abstract class ClassRepository {
  Future<Either<Failure, PaginatedList<ClassDto>>> getClasses({
    String? courseId,
    String? academicTermId,
    String? searchTerm,
    bool? isActive,
    String? sortBy,
    bool sortDescending = false,
    int pageNumber = 1,
    int pageSize = 20,
  });

  Future<Either<Failure, ClassDto>> getClassById(String id);

  Future<Either<Failure, ClassDto>> createClass(
    String courseId,
    String academicTermId,
    String name,
  );

  Future<Either<Failure, ClassDto>> updateClass(
    String id,
    String name,
    bool isActive,
  );

  Future<Either<Failure, void>> deleteClass(String id);

  Future<Either<Failure, void>> restoreClass(String id);
}
