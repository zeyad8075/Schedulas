import 'package:fpdart/fpdart.dart';
import '../../../../core/errors/failures.dart';
import '../../../../core/network/paginated_list.dart';
import '../models/institution_dto.dart';

abstract class InstitutionRepository {
  Future<Either<Failure, PaginatedList<InstitutionDto>>> getInstitutions({
    String? searchTerm,
    bool? isSuspended,
    String? sortBy,
    bool sortDescending = false,
    int pageNumber = 1,
    int pageSize = 20,
  });

  Future<Either<Failure, InstitutionDto>> getInstitutionById(String id);

  Future<Either<Failure, InstitutionDto>> createInstitution(
    String name,
    InstitutionType type,
    String timezone,
  );

  Future<Either<Failure, InstitutionDto>> updateInstitution(
    String id,
    String name,
    String? logoUrl,
    String timezone,
  );

  Future<Either<Failure, void>> suspendInstitution(String id);

  Future<Either<Failure, void>> reactivateInstitution(String id);

  Future<Either<Failure, void>> deleteInstitution(String id);

  Future<Either<Failure, void>> restoreInstitution(String id);
}
