import 'package:fpdart/fpdart.dart';
import '../../../../core/errors/failures.dart';
import '../../../../core/network/paginated_list.dart';
import '../models/program_dto.dart';

abstract class ProgramRepository {
  Future<Either<Failure, PaginatedList<ProgramDto>>> getPrograms({
    String? departmentId,
    String? searchTerm,
    bool? isActive,
    String? sortBy,
    bool sortDescending = false,
    int pageNumber = 1,
    int pageSize = 20,
  });

  Future<Either<Failure, ProgramDto>> getProgramById(String id);

  Future<Either<Failure, ProgramDto>> createProgram(
    String departmentId,
    String name,
  );

  Future<Either<Failure, ProgramDto>> updateProgram(
    String id,
    String name,
    bool isActive,
  );

  Future<Either<Failure, void>> deleteProgram(String id);

  Future<Either<Failure, void>> restoreProgram(String id);
}
