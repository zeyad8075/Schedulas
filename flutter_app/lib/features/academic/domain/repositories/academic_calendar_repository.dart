import 'package:fpdart/fpdart.dart';
import '../../../../core/errors/failures.dart';
import '../../../../core/network/paginated_list.dart';
import '../models/academic_term_dto.dart';
import '../models/holiday_dto.dart';

abstract class AcademicCalendarRepository {
  // --- Academic Terms ---
  Future<Either<Failure, PaginatedList<AcademicTermDto>>> getAcademicTerms({
    String? institutionId,
    String? searchTerm,
    bool? isActive,
    String? sortBy,
    bool sortDescending = false,
    int pageNumber = 1,
    int pageSize = 20,
  });

  Future<Either<Failure, AcademicTermDto>> getAcademicTermById(String id);

  Future<Either<Failure, AcademicTermDto>> createAcademicTerm(
    String institutionId,
    String name,
    DateTime startDate,
    DateTime endDate,
  );

  Future<Either<Failure, AcademicTermDto>> updateAcademicTerm(
    String id,
    String name,
    DateTime startDate,
    DateTime endDate,
    bool isActive,
  );

  Future<Either<Failure, void>> deleteAcademicTerm(String id);

  Future<Either<Failure, void>> restoreAcademicTerm(String id);

  // --- Holidays ---
  Future<Either<Failure, PaginatedList<HolidayDto>>> getHolidays({
    String? institutionId,
    String? academicTermId,
    String? searchTerm,
    String? sortBy,
    bool sortDescending = false,
    int pageNumber = 1,
    int pageSize = 20,
  });

  Future<Either<Failure, HolidayDto>> getHolidayById(String id);

  Future<Either<Failure, HolidayDto>> createHoliday(
    String institutionId,
    String? academicTermId,
    String name,
    DateTime holidayDate,
  );

  Future<Either<Failure, HolidayDto>> updateHoliday(
    String id,
    String name,
    DateTime holidayDate,
  );

  Future<Either<Failure, void>> deleteHoliday(String id);

  Future<Either<Failure, void>> restoreHoliday(String id);
}
