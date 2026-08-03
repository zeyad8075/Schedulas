import 'package:fpdart/fpdart.dart';
import '../../../../core/errors/failures.dart';
import '../../../../core/network/paginated_list.dart';
import '../models/course_dto.dart';

abstract class CourseRepository {
  Future<Either<Failure, PaginatedList<CourseDto>>> getCourses({
    String? programId,
    String? searchTerm,
    bool? isActive,
    String? sortBy,
    bool sortDescending = false,
    int pageNumber = 1,
    int pageSize = 20,
  });

  Future<Either<Failure, CourseDto>> getCourseById(String id);

  Future<Either<Failure, CourseDto>> createCourse(
    String programId,
    String name,
    String? code,
  );

  Future<Either<Failure, CourseDto>> updateCourse(
    String id,
    String name,
    String? code,
    bool isActive,
  );

  Future<Either<Failure, void>> deleteCourse(String id);

  Future<Either<Failure, void>> restoreCourse(String id);
}
