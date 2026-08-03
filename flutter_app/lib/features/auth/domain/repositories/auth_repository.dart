import 'package:fpdart/fpdart.dart';
import '../../../../core/errors/failures.dart';
import '../user_profile.dart';

abstract class AuthRepository {
  Future<Either<Failure, Unit>> login(String email, String password);
  Future<Either<Failure, Unit>> register(String email, String password,
      String fullName, int role, String institutionId, String? departmentId);
  Future<Either<Failure, Unit>> forgotPassword(String email);
  Future<Either<Failure, Unit>> refresh();
  Future<Either<Failure, Unit>> logout();
  Future<Either<Failure, UserProfile>> me();
}
