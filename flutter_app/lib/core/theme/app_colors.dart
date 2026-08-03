import 'package:flutter/material.dart';

/// Placeholder palette, per PROJECT_CONSTITUTION.md §21: exact hex values
/// were explicitly deferred to a dedicated UI/UX design phase rather than
/// invented ad hoc. These are reasonable, accessible defaults so the app
/// is usable today — swap this file's values (not its structure) once
/// the real palette is finalized; every screen references these tokens,
/// never a raw Color literal, so that swap stays a one-file change.
class AppColors {
  AppColors._();

  // Primary: institutional trust-blue tone (Constitution §21).
  static const Color primary = Color(0xFF1B4B66);
  static const Color primaryLight = Color(0xFF3E7691);
  static const Color primaryDark = Color(0xFF0D2E40);

  // Secondary/Accent: warm accent for calls-to-action.
  static const Color accent = Color(0xFFE0A458);

  // Semantic — activity types, consistent across calendar/notifications/reports (§19).
  static const Color activityAssignment = Color(0xFF3E7691);
  static const Color activityExam = Color(0xFFC0453A);
  static const Color activityProject = Color(0xFF4F8A5B);
  static const Color activityPresentation = Color(0xFF8B5FBF);
  static const Color activityEvent = Color(0xFFE0A458);

  // Semantic — states.
  static const Color success = Color(0xFF4F8A5B);
  static const Color warning = Color(0xFFCE8A1E);
  static const Color error = Color(0xFFC0453A);
  static const Color info = Color(0xFF3E7691);

  static Color activityColor(String activityType) => switch (activityType) {
        'assignment' => activityAssignment,
        'exam' => activityExam,
        'project' => activityProject,
        'presentation' => activityPresentation,
        _ => activityEvent,
      };
}
