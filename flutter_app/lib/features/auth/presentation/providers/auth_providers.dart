import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../../core/network/api_client.dart';
import '../../../../core/storage/secure_storage_service.dart';
import '../../data/repositories/auth_repository_impl.dart';
import '../../domain/repositories/auth_repository.dart';
import '../../domain/user_profile.dart';
import 'auth_state.dart';

final secureStorageProvider = Provider<SecureStorageService>((ref) {
  return SecureStorageService();
});

final cacheStorePathProvider = Provider<String>((ref) {
  throw UnimplementedError(
      'cacheStorePathProvider must be overridden in main.dart');
});

final apiClientProvider = Provider<ApiClient>((ref) {
  return ApiClient(
      ref.watch(secureStorageProvider), ref.watch(cacheStorePathProvider));
});

final authRepositoryProvider = Provider<AuthRepository>((ref) {
  return AuthRepositoryImpl(
      ref.watch(apiClientProvider), ref.watch(secureStorageProvider));
});

class AuthNotifier extends StateNotifier<AuthState> {
  final AuthRepository _authRepository;
  final SecureStorageService _storageService;

  AuthNotifier(this._authRepository, this._storageService)
      : super(AuthStateUnknown()) {
    checkAuthStatus();
  }

  Future<void> checkAuthStatus() async {
    final token = await _storageService.readAccessToken();
    if (token == null) {
      state = AuthStateUnauthenticated();
      return;
    }

    state = AuthStateRefreshing();
    final profileResult = await _authRepository.me();
    profileResult.fold((failure) async {
      // If getting profile failed (e.g. 401), attempt refresh
      final refreshResult = await _authRepository.refresh();
      refreshResult.fold((f) {
        state = AuthStateUnauthenticated();
      }, (_) async {
        final profileResultRetry = await _authRepository.me();
        profileResultRetry.fold((f) {
          state = AuthStateUnauthenticated();
        }, (profile) {
          state = AuthStateAuthenticated(profile);
        });
      });
    }, (profile) {
      state = AuthStateAuthenticated(profile);
    });
  }

  Future<void> login(String email, String password) async {
    state = AuthStateAuthenticating();
    final result = await _authRepository.login(email, password);
    result.fold(
      (failure) => state = AuthStateFailure(failure),
      (_) => checkAuthStatus(),
    );
  }

  Future<void> logout() async {
    await _authRepository.logout();
    state = AuthStateUnauthenticated();
  }

  void resetError() {
    if (state is AuthStateFailure) {
      state = AuthStateUnauthenticated();
    }
  }
}

final authNotifierProvider =
    StateNotifierProvider<AuthNotifier, AuthState>((ref) {
  return AuthNotifier(
      ref.watch(authRepositoryProvider), ref.watch(secureStorageProvider));
});

final isAuthenticatedProvider = Provider<bool>((ref) {
  final state = ref.watch(authNotifierProvider);
  return state is AuthStateAuthenticated;
});

final currentProfileProvider = Provider<UserProfile?>((ref) {
  final state = ref.watch(authNotifierProvider);
  if (state is AuthStateAuthenticated) {
    return state.user;
  }
  return null;
});
