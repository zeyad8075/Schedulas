import '../../../../core/errors/failures.dart';
import '../../domain/user_profile.dart';

sealed class AuthState {}

class AuthStateUnknown extends AuthState {}

class AuthStateUnauthenticated extends AuthState {}

class AuthStateAuthenticating extends AuthState {}

class AuthStateAuthenticated extends AuthState {
  final UserProfile user;
  AuthStateAuthenticated(this.user);
}

class AuthStateRefreshing extends AuthState {}

class AuthStateFailure extends AuthState {
  final Failure failure;
  AuthStateFailure(this.failure);
}
