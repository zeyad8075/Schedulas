import 'package:fpdart/fpdart.dart';
import '../../../../core/network/api_client.dart';
import '../../../../core/network/api_exception.dart';
import '../../../../core/errors/failures.dart';
import '../../../../core/network/paginated_list.dart';
import '../../domain/models/academic_term_dto.dart';
import '../../domain/models/holiday_dto.dart';
import '../../domain/repositories/academic_calendar_repository.dart';

class AcademicCalendarRepositoryImpl implements AcademicCalendarRepository {
  final ApiClient _apiClient;

  AcademicCalendarRepositoryImpl(this._apiClient);

  // --- Academic Terms ---

  @override
  Future<Either<Failure, PaginatedList<AcademicTermDto>>> getAcademicTerms({
    String? institutionId,
    String? searchTerm,
    bool? isActive,
    String? sortBy,
    bool sortDescending = false,
    int pageNumber = 1,
    int pageSize = 20,
  }) async {
    try {
      final queryParameters = {
        if (institutionId != null && institutionId.isNotEmpty)
          'InstitutionId': institutionId,
        if (searchTerm != null && searchTerm.isNotEmpty)
          'SearchTerm': searchTerm,
        if (isActive != null) 'IsActive': isActive,
        if (sortBy != null && sortBy.isNotEmpty) 'SortBy': sortBy,
        'SortDescending': sortDescending,
        'PageNumber': pageNumber,
        'PageSize': pageSize,
      };

      final result = await _apiClient.get<PaginatedList<AcademicTermDto>>(
        '/academicterms',
        queryParameters: queryParameters,
        fromData: (json) => PaginatedList.fromJson(
          json,
          (itemJson) => AcademicTermDto.fromJson(itemJson),
        ),
      );
      return right(result);
    } on ApiException catch (e) {
      if (e.statusCode == 401 || e.statusCode == 403) {
        return left(const UnauthorizedFailure('Not authorized'));
      }
      return left(ServerFailure(e.message));
    } catch (e) {
      return left(UnknownFailure(e.toString()));
    }
  }

  @override
  Future<Either<Failure, AcademicTermDto>> getAcademicTermById(
      String id) async {
    try {
      final result = await _apiClient.get<AcademicTermDto>(
        '/academicterms/$id',
        fromData: (json) =>
            AcademicTermDto.fromJson(json as Map<String, dynamic>),
      );
      return right(result);
    } on ApiException catch (e) {
      if (e.statusCode == 404) return left(const ServerFailure('Not found'));
      return left(ServerFailure(e.message));
    } catch (e) {
      return left(UnknownFailure(e.toString()));
    }
  }

  @override
  Future<Either<Failure, AcademicTermDto>> createAcademicTerm(
    String institutionId,
    String name,
    DateTime startDate,
    DateTime endDate,
  ) async {
    try {
      final result = await _apiClient.post<AcademicTermDto>(
        '/academicterms',
        body: {
          'institutionId': institutionId,
          'name': name,
          'startDate': startDate.toIso8601String().split('T')[0],
          'endDate': endDate.toIso8601String().split('T')[0],
        },
        fromData: (json) =>
            AcademicTermDto.fromJson(json as Map<String, dynamic>),
      );
      return right(result);
    } on ApiException catch (e) {
      if (e.statusCode == 400 && e.fieldMessages != null) {
        return left(ValidationFailure('Validation error', e.fieldMessages!));
      }
      return left(ServerFailure(e.message));
    } catch (e) {
      return left(UnknownFailure(e.toString()));
    }
  }

  @override
  Future<Either<Failure, AcademicTermDto>> updateAcademicTerm(
    String id,
    String name,
    DateTime startDate,
    DateTime endDate,
    bool isActive,
  ) async {
    try {
      final result = await _apiClient.put<AcademicTermDto>(
        '/academicterms/$id',
        body: {
          'name': name,
          'startDate': startDate.toIso8601String().split('T')[0],
          'endDate': endDate.toIso8601String().split('T')[0],
          'isActive': isActive,
        },
        fromData: (json) =>
            AcademicTermDto.fromJson(json as Map<String, dynamic>),
      );
      return right(result);
    } on ApiException catch (e) {
      if (e.statusCode == 400 && e.fieldMessages != null) {
        return left(ValidationFailure('Validation error', e.fieldMessages!));
      }
      return left(ServerFailure(e.message));
    } catch (e) {
      return left(UnknownFailure(e.toString()));
    }
  }

  @override
  Future<Either<Failure, void>> deleteAcademicTerm(String id) async {
    try {
      await _apiClient.delete<void>('/academicterms/$id', fromData: (_) {});
      return right(null);
    } on ApiException catch (e) {
      return left(ServerFailure(e.message));
    } catch (e) {
      return left(UnknownFailure(e.toString()));
    }
  }

  @override
  Future<Either<Failure, void>> restoreAcademicTerm(String id) async {
    try {
      await _apiClient.post<void>('/academicterms/$id/restore',
          fromData: (_) {});
      return right(null);
    } on ApiException catch (e) {
      return left(ServerFailure(e.message));
    } catch (e) {
      return left(UnknownFailure(e.toString()));
    }
  }

  // --- Holidays ---

  @override
  Future<Either<Failure, PaginatedList<HolidayDto>>> getHolidays({
    String? institutionId,
    String? academicTermId,
    String? searchTerm,
    String? sortBy,
    bool sortDescending = false,
    int pageNumber = 1,
    int pageSize = 20,
  }) async {
    try {
      final queryParameters = {
        if (institutionId != null && institutionId.isNotEmpty)
          'InstitutionId': institutionId,
        if (academicTermId != null && academicTermId.isNotEmpty)
          'AcademicTermId': academicTermId,
        if (searchTerm != null && searchTerm.isNotEmpty)
          'SearchTerm': searchTerm,
        if (sortBy != null && sortBy.isNotEmpty) 'SortBy': sortBy,
        'SortDescending': sortDescending,
        'PageNumber': pageNumber,
        'PageSize': pageSize,
      };

      final result = await _apiClient.get<PaginatedList<HolidayDto>>(
        '/holidays',
        queryParameters: queryParameters,
        fromData: (json) => PaginatedList.fromJson(
          json,
          (itemJson) => HolidayDto.fromJson(itemJson),
        ),
      );
      return right(result);
    } on ApiException catch (e) {
      if (e.statusCode == 401 || e.statusCode == 403) {
        return left(const UnauthorizedFailure('Not authorized'));
      }
      return left(ServerFailure(e.message));
    } catch (e) {
      return left(UnknownFailure(e.toString()));
    }
  }

  @override
  Future<Either<Failure, HolidayDto>> getHolidayById(String id) async {
    try {
      final result = await _apiClient.get<HolidayDto>(
        '/holidays/$id',
        fromData: (json) => HolidayDto.fromJson(json as Map<String, dynamic>),
      );
      return right(result);
    } on ApiException catch (e) {
      if (e.statusCode == 404) return left(const ServerFailure('Not found'));
      return left(ServerFailure(e.message));
    } catch (e) {
      return left(UnknownFailure(e.toString()));
    }
  }

  @override
  Future<Either<Failure, HolidayDto>> createHoliday(
    String institutionId,
    String? academicTermId,
    String name,
    DateTime holidayDate,
  ) async {
    try {
      final result = await _apiClient.post<HolidayDto>(
        '/holidays',
        body: {
          'institutionId': institutionId,
          'academicTermId': academicTermId,
          'name': name,
          'holidayDate': holidayDate.toIso8601String().split('T')[0],
        },
        fromData: (json) => HolidayDto.fromJson(json as Map<String, dynamic>),
      );
      return right(result);
    } on ApiException catch (e) {
      if (e.statusCode == 400 && e.fieldMessages != null) {
        return left(ValidationFailure('Validation error', e.fieldMessages!));
      }
      return left(ServerFailure(e.message));
    } catch (e) {
      return left(UnknownFailure(e.toString()));
    }
  }

  @override
  Future<Either<Failure, HolidayDto>> updateHoliday(
    String id,
    String name,
    DateTime holidayDate,
  ) async {
    try {
      final result = await _apiClient.put<HolidayDto>(
        '/holidays/$id',
        body: {
          'name': name,
          'holidayDate': holidayDate.toIso8601String().split('T')[0],
        },
        fromData: (json) => HolidayDto.fromJson(json as Map<String, dynamic>),
      );
      return right(result);
    } on ApiException catch (e) {
      if (e.statusCode == 400 && e.fieldMessages != null) {
        return left(ValidationFailure('Validation error', e.fieldMessages!));
      }
      return left(ServerFailure(e.message));
    } catch (e) {
      return left(UnknownFailure(e.toString()));
    }
  }

  @override
  Future<Either<Failure, void>> deleteHoliday(String id) async {
    try {
      await _apiClient.delete<void>('/holidays/$id', fromData: (_) {});
      return right(null);
    } on ApiException catch (e) {
      return left(ServerFailure(e.message));
    } catch (e) {
      return left(UnknownFailure(e.toString()));
    }
  }

  @override
  Future<Either<Failure, void>> restoreHoliday(String id) async {
    try {
      await _apiClient.post<void>('/holidays/$id/restore', fromData: (_) {});
      return right(null);
    } on ApiException catch (e) {
      return left(ServerFailure(e.message));
    } catch (e) {
      return left(UnknownFailure(e.toString()));
    }
  }
}
