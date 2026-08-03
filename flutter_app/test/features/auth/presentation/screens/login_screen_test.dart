import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:mocktail/mocktail.dart';
import 'package:schedulas/core/errors/failures.dart';
import 'package:schedulas/features/auth/domain/repositories/auth_repository.dart';
import 'package:schedulas/features/auth/presentation/providers/auth_providers.dart';
import 'package:schedulas/features/auth/presentation/screens/login_screen.dart';
import 'package:fpdart/fpdart.dart';

class MockAuthRepository extends Mock implements AuthRepository {}

void main() {
  late MockAuthRepository mockAuthRepository;

  setUp(() {
    mockAuthRepository = MockAuthRepository();
  });

  Widget createWidgetUnderTest() {
    return ProviderScope(
      overrides: [
        authRepositoryProvider.overrideWithValue(mockAuthRepository),
      ],
      child: const MaterialApp(
        home: Directionality(
          textDirection: TextDirection.rtl,
          child: LoginScreen(),
        ),
      ),
    );
  }

  group('LoginScreen Widget Tests', () {
    testWidgets('should display validation errors when fields are empty',
        (WidgetTester tester) async {
      await tester.pumpWidget(createWidgetUnderTest());

      // Find login button and tap it
      final loginButton = find.byType(ElevatedButton);
      await tester.tap(loginButton);
      await tester.pumpAndSettle();

      // Expect validation errors
      expect(find.text('البريد الإلكتروني مطلوب'), findsOneWidget);
      expect(find.text('كلمة المرور مطلوبة'), findsOneWidget);
    });

    testWidgets('should call login on AuthRepository when fields are valid',
        (WidgetTester tester) async {
      when(() => mockAuthRepository.login(any(), any()))
          .thenAnswer((_) async => right(unit));

      await tester.pumpWidget(createWidgetUnderTest());

      // Fill in fields
      await tester.enterText(
          find.byType(TextFormField).first, 'test@example.com');
      await tester.enterText(find.byType(TextFormField).last, 'password123');

      // Tap login
      final loginButton = find.byType(ElevatedButton);
      await tester.tap(loginButton);

      // We pump once to start the async operation (loading state)
      await tester.pump();

      verify(() => mockAuthRepository.login('test@example.com', 'password123'))
          .called(1);
    });

    testWidgets('should display error message on failure',
        (WidgetTester tester) async {
      when(() => mockAuthRepository.login(any(), any())).thenAnswer((_) async =>
          left(const UnauthorizedFailure('بيانات الدخول غير صحيحة')));

      await tester.pumpWidget(createWidgetUnderTest());

      await tester.enterText(
          find.byType(TextFormField).first, 'test@example.com');
      await tester.enterText(find.byType(TextFormField).last, 'wrong_password');

      await tester.tap(find.byType(ElevatedButton));
      await tester.pumpAndSettle();

      expect(find.text('بيانات الدخول غير صحيحة'), findsOneWidget);
    });
  });
}
