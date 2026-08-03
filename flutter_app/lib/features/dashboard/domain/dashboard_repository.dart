import 'package:fpdart/fpdart.dart';
import '../../../../core/errors/failures.dart';
import 'dashboard_data.dart';

abstract class DashboardRepository {
  Future<Either<Failure, DashboardData>> getDashboardData();
}
