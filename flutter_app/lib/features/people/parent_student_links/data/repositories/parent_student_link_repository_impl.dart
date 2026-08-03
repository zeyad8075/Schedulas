import 'package:fpdart/fpdart.dart';
import '../../../../../core/network/api_client.dart';
import '../../../../../core/network/api_exception.dart';
import '../../../../../core/errors/failures.dart';
import '../../../students/domain/models/student_dto.dart';
import '../../domain/repositories/parent_student_link_repository.dart';

class ParentStudentLinkRepositoryImpl implements ParentStudentLinkRepository {
  final ApiClient _apiClient;

  ParentStudentLinkRepositoryImpl(this._apiClient);

  @override
  Future<Either<Failure, List<StudentDto>>> getParentStudents(
      String parentId) async {
    try {
      final result = await _apiClient.get<List<StudentDto>>(
        '/parent-student-links/$parentId/students',
        fromData: (json) {
          final list = json as List;
          return list
              .map((item) => StudentDto.fromJson(item as Map<String, dynamic>))
              .toList();
        },
      );
      return right(result);
    } on ApiException catch (e) {
      if (e.statusCode == 404) {
                return left(const ServerFailure('Parent not found'));
              }
      return left(ServerFailure(e.message));
    } catch (e) {
      return left(UnknownFailure(e.toString()));
    }
  }

  @override
  Future<Either<Failure, void>> linkParentToStudent(
      String parentId, String studentId) async {
    try {
      await _apiClient.post<void>(
        '/parent-student-links',
        body: {
          'parentId': parentId,
          'studentId': studentId,
        },
        fromData: (_) {},
      );
      return right(null);
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
  Future<Either<Failure, void>> unlinkParentFromStudent(String linkId) async {
    try {
      await _apiClient.delete<void>('/parent-student-links/$linkId',
          fromData: (_) {});
      return right(null);
    } on ApiException catch (e) {
      return left(ServerFailure(e.message));
    } catch (e) {
      return left(UnknownFailure(e.toString()));
    }
  }
}
