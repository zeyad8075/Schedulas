import 'package:fpdart/fpdart.dart';
import '../../../../../core/errors/failures.dart';
import '../../../../../core/network/paginated_list.dart';
import '../models/parent_dto.dart';

abstract class ParentRepository {
  Future<Either<Failure, ParentDto>> getParentById(String id);

  Future<Either<Failure, PaginatedList<ParentDto>>> getParents({
    String? searchTerm,
    String? sortBy,
    bool sortDescending = false,
    int pageNumber = 1,
    int pageSize = 20,
  });

  Future<Either<Failure, ParentDto>> createParent(String profileId);

  Future<Either<Failure, void>> deleteParent(String id);

  Future<Either<Failure, void>> restoreParent(String id);
}
