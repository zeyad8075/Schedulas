import 'package:flutter/material.dart';
import 'failures.dart';
import '../theme/app_colors.dart';

class ErrorUIHelper {
  static void showSnackbar(BuildContext context, String message,
      {bool isError = true}) {
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(
        content: Text(message),
        backgroundColor: isError ? AppColors.error : AppColors.success,
        behavior: SnackBarBehavior.floating,
      ),
    );
  }

  static void handleFailure(BuildContext context, Failure failure) {
    String message;
    if (failure is UnauthorizedFailure) {
      message = 'انتهت صلاحية الجلسة، الرجاء تسجيل الدخول مجدداً';
    } else if (failure is NetworkFailure) {
      message = 'فشل الاتصال بالإنترنت، الرجاء التحقق من الشبكة';
    } else if (failure is ValidationFailure) {
      message = failure.message;
    } else if (failure is ServerFailure) {
      message = 'حدث خطأ في الخادم، الرجاء المحاولة لاحقاً';
    } else {
      message = failure.message;
    }

    showSnackbar(context, message, isError: true);
  }
}
