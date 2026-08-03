import 'package:fpdart/fpdart.dart';
import '../../../../core/errors/failures.dart';
import '../../../../core/network/paginated_list.dart';
import '../models/department_dto.dart';

abstract class DepartmentRepository {
  Future<Either<Failure, PaginatedList<DepartmentDto>>> getDepartments({
    String? institutionId,
    String? searchTerm,
    bool? isActive,
    String? sortBy,
    bool sortDescending = false,
    int pageNumber = 1,
    int pageSize = 20,
  });

  Future<Either<Failure, DepartmentDto>> getDepartmentById(String id);

  Future<Either<Failure, DepartmentDto>> createDepartment(
    String institutionId,
    String name,
  );

  Future<Either<Failure, DepartmentDto>> updateDepartment(
    String id,
    String name,
    bool isActive,
  );

  Future<Either<Failure, void>> deleteDepartment(String id);

  Future<Either<Failure, void>> restoreDepartment(String id);
}
