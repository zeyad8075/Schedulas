import 'package:fpdart/fpdart.dart';
import '../../../../../core/errors/failures.dart';
import '../../../../../core/network/paginated_list.dart';
import '../models/profile_dto.dart';
import '../../../domain/models/user_role.dart';
import '../../../domain/models/theme_enum.dart' as model_theme;

abstract class ProfileRepository {
  Future<Either<Failure, ProfileDto>> getMyProfile();

  Future<Either<Failure, ProfileDto>> getProfileById(String id);

  Future<Either<Failure, PaginatedList<ProfileDto>>> getProfiles(
    String institutionId, {
    String? searchTerm,
    UserRole? role,
    String? sortBy,
    bool sortDescending = false,
    int pageNumber = 1,
    int pageSize = 20,
  });

  Future<Either<Failure, ProfileDto>> updateProfile(
    String fullName,
    String? phoneNumber,
  );

  Future<Either<Failure, void>> updateTheme(model_theme.Theme preferredTheme);

  Future<Either<Failure, void>> suspendProfile(String id);

  Future<Either<Failure, void>> activateProfile(String id);
}
