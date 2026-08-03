import 'package:flutter_test/flutter_test.dart';

import 'package:mocktail/mocktail.dart';
import 'package:schedulas/core/errors/failures.dart';
import 'package:schedulas/core/network/api_client.dart';
import 'package:schedulas/core/network/api_exception.dart';
import 'package:schedulas/core/storage/secure_storage_service.dart';
import 'package:schedulas/features/auth/data/repositories/auth_repository_impl.dart';

class MockApiClient extends Mock implements ApiClient {}

class MockSecureStorageService extends Mock implements SecureStorageService {}

void main() {
  late AuthRepositoryImpl repository;
  late MockApiClient mockApiClient;
  late MockSecureStorageService mockStorageService;

  setUpAll(() {
    registerFallbackValue(Uri());
    registerFallbackValue(<String, dynamic>{});
    registerFallbackValue((dynamic _) => <String, dynamic>{});
  });

  setUp(() {
    mockApiClient = MockApiClient();
    mockStorageService = MockSecureStorageService();
    repository = AuthRepositoryImpl(mockApiClient, mockStorageService);
  });

  group('login', () {
    const tEmail = 'test@test.com';
    const tPassword = 'password';

    test('should return Unit and save tokens when login is successful',
        () async {
      // arrange
      when(() => mockApiClient.post<Map<String, dynamic>>(
                any(),
                body: any(named: 'body'),
                fromData: any(named: 'fromData'),
              ))
          .thenAnswer((_) async => {
                'accessToken': 'access_token_123',
                'refreshToken': 'refresh_token_123'
              });

      when(() => mockStorageService.saveAccessToken(any()))
          .thenAnswer((_) async => {});
      when(() => mockStorageService.saveRefreshToken(any()))
          .thenAnswer((_) async => {});

      // act
      final result = await repository.login(tEmail, tPassword);

      // assert
      expect(result.isRight(), true);
      verify(() => mockStorageService.saveAccessToken('access_token_123'))
          .called(1);
      verify(() => mockStorageService.saveRefreshToken('refresh_token_123'))
          .called(1);
    });

    test('should return UnauthorizedFailure when login fails with 401',
        () async {
      // arrange
      when(() => mockApiClient.post<Map<String, dynamic>>(
                any(),
                body: any(named: 'body'),
                fromData: any(named: 'fromData'),
              ))
          .thenThrow(
              const ApiException('Invalid credentials', statusCode: 401));

      // act
      final result = await repository.login(tEmail, tPassword);

      // assert
      expect(result.isLeft(), true);
      result.fold(
        (failure) => expect(failure, isA<UnauthorizedFailure>()),
        (_) => fail('Expected failure'),
      );
    });
  });

  group('refresh', () {
    test('should return Unit and save new tokens on success', () async {
      // arrange
      when(() => mockStorageService.readRefreshToken())
          .thenAnswer((_) async => 'old_refresh_token');
      when(() => mockApiClient.post<Map<String, dynamic>>(
                any(),
                body: any(named: 'body'),
                fromData: any(named: 'fromData'),
              ))
          .thenAnswer((_) async =>
              {'accessToken': 'new_access', 'refreshToken': 'new_refresh'});
      when(() => mockStorageService.saveAccessToken(any()))
          .thenAnswer((_) async => {});
      when(() => mockStorageService.saveRefreshToken(any()))
          .thenAnswer((_) async => {});

      // act
      final result = await repository.refresh();

      // assert
      expect(result.isRight(), true);
      verify(() => mockStorageService.saveAccessToken('new_access')).called(1);
      verify(() => mockStorageService.saveRefreshToken('new_refresh'))
          .called(1);
    });

    test('should return UnauthorizedFailure if no refresh token exists locally',
        () async {
      // arrange
      when(() => mockStorageService.readRefreshToken())
          .thenAnswer((_) async => null);

      // act
      final result = await repository.refresh();

      // assert
      expect(result.isLeft(), true);
      result.fold(
        (failure) => expect(failure, isA<UnauthorizedFailure>()),
        (_) => fail('Expected failure'),
      );
      verifyNever(() => mockApiClient.post(any(),
          body: any(named: 'body'), fromData: any(named: 'fromData')));
    });
  });

  group('logout', () {
    test('should clear storage and return unit', () async {
      // arrange
      when(() => mockStorageService.clear()).thenAnswer((_) async => {});

      // act
      final result = await repository.logout();

      // assert
      expect(result.isRight(), true);
      verify(() => mockStorageService.clear()).called(1);
    });
  });

  group('forgotPassword', () {
    test('should return unit when successful', () async {
      // arrange
      when(() => mockApiClient.post<dynamic>(
            any(),
            body: any(named: 'body'),
            fromData: any(named: 'fromData'),
          )).thenAnswer((_) async => {});

      // act
      final result = await repository.forgotPassword('test@test.com');

      // assert
      expect(result.isRight(), true);
    });
  });
}
