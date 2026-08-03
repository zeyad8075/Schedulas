import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../../features/activities/presentation/screens/activities_screen.dart';
import '../../features/calendar/presentation/screens/calendar_screen.dart';

import '../../features/auth/presentation/providers/auth_providers.dart';
import '../../features/auth/presentation/providers/auth_state.dart';
import '../../features/auth/presentation/screens/forgot_password_screen.dart';
import '../../features/auth/presentation/screens/login_screen.dart';
import '../../features/auth/presentation/screens/register_screen.dart';
import '../../features/dashboard/presentation/screens/dashboard_screen.dart';
import '../../features/notifications/presentation/screens/notifications_screen.dart';
import '../../features/notifications/presentation/screens/notification_details_screen.dart';
import '../../features/notifications/domain/models/notification_dto.dart';
import '../../features/settings/presentation/screens/settings_screen.dart';
import '../../features/academic/presentation/screens/academic_screen.dart';
import '../../features/academic/presentation/screens/institutions_screen.dart';
import '../../features/academic/presentation/screens/departments_screen.dart';
import '../../features/academic/presentation/screens/programs_screen.dart';
import '../../features/academic/presentation/screens/courses_screen.dart';
import '../../features/academic/presentation/screens/classes_screen.dart';
import '../../features/academic/presentation/screens/academic_calendar_screen.dart';
import '../../features/people/presentation/screens/people_screen.dart';
import '../../features/people/parent_student_links/presentation/screens/parent_students_screen.dart';
import '../../features/reports/presentation/screens/reports_screen.dart';
import '../widgets/more_screen.dart';
import '../widgets/app_shell.dart';

final routerProvider = Provider<GoRouter>((ref) {
  final authState = ref.watch(authNotifierProvider);

  return GoRouter(
    initialLocation: '/login',
    redirect: (context, state) {
      final isAuthRoute = state.matchedLocation == '/login' ||
          state.matchedLocation == '/forgot-password';

      if (authState is AuthStateUnknown || authState is AuthStateRefreshing) {
        if (state.matchedLocation != '/splash') return '/splash';
        return null;
      }

      if (authState is AuthStateUnauthenticated ||
          authState is AuthStateFailure) {
        if (!isAuthRoute) return '/login';
        return null;
      }

      if (authState is AuthStateAuthenticated) {
        if (isAuthRoute || state.matchedLocation == '/splash') {
                return '/dashboard';
              }
        return null;
      }

      return null;
    },
    routes: [
      GoRoute(
        path: '/splash',
        builder: (context, state) => const Scaffold(
          body: Center(child: CircularProgressIndicator()),
        ),
      ),
      GoRoute(path: '/login', builder: (context, state) => const LoginScreen()),
      GoRoute(
          path: '/forgot-password',
          builder: (context, state) => const ForgotPasswordScreen()),
      GoRoute(
        path: '/settings',
        builder: (context, state) => const SettingsScreen(),
      ),
      GoRoute(
        path: '/register',
        builder: (context, state) => const RegisterScreen(),
      ),
      GoRoute(
        path: '/academic',
        builder: (context, state) => const AcademicScreen(),
        routes: [
          GoRoute(
              path: 'institutions',
              builder: (context, state) => const InstitutionsScreen()),
          GoRoute(
              path: 'departments',
              builder: (context, state) => const DepartmentsScreen()),
          GoRoute(
              path: 'programs',
              builder: (context, state) => const ProgramsScreen()),
          GoRoute(
              path: 'courses',
              builder: (context, state) => const CoursesScreen()),
          GoRoute(
              path: 'classes',
              builder: (context, state) => const ClassesScreen()),
          GoRoute(
              path: 'calendar',
              builder: (context, state) => const AcademicCalendarScreen()),
        ],
      ),
      GoRoute(
        path: '/people',
        builder: (context, state) => const PeopleScreen(),
        routes: [
          GoRoute(
            path: 'parents/:id',
            builder: (context, state) {
              final parentId = state.pathParameters['id']!;
              return ParentStudentsScreen(parentId: parentId);
            },
          ),
        ],
      ),
      GoRoute(
        path: '/reports',
        builder: (context, state) => const ReportsScreen(),
      ),
      StatefulShellRoute.indexedStack(
        builder: (context, state, navigationShell) =>
            AppShell(navigationShell: navigationShell),
        branches: [
          StatefulShellBranch(routes: [
            GoRoute(
                path: '/dashboard',
                builder: (context, state) => const DashboardScreen()),
          ]),
          StatefulShellBranch(routes: [
            GoRoute(
                path: '/calendar',
                builder: (context, state) => const CalendarScreen()),
          ]),
          StatefulShellBranch(routes: [
            GoRoute(
                path: '/activities',
                builder: (context, state) => const ActivitiesScreen()),
          ]),
          StatefulShellBranch(routes: [
            GoRoute(
              path: '/notifications',
              builder: (context, state) => const NotificationsScreen(),
              routes: [
                GoRoute(
                  path: ':id',
                  builder: (context, state) {
                    final notification = state.extra as NotificationDto;
                    return NotificationDetailsScreen(
                        notification: notification);
                  },
                ),
              ],
            ),
          ]),
          StatefulShellBranch(routes: [
            GoRoute(
                path: '/more', builder: (context, state) => const MoreScreen()),
          ]),
        ],
      ),
    ],
  );
});
