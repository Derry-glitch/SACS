import 'package:dio/dio.dart';
import '../models/auth_response_model.dart';
import '../models/event_model.dart';
import 'storage_service.dart';
import '../core/constants/app_constants.dart';
import '../core/errors/failures.dart';

class ApiService {
  final Dio _dio;
  final StorageService _storageService;

  ApiService({StorageService? storageService})
      : _storageService = storageService ?? StorageService(),
        _dio = Dio(BaseOptions(baseUrl: AppConstants.baseUrl)) {
    _dio.interceptors.add(
      InterceptorsWrapper(
        onRequest: (options, handler) async {
          final token = await _storageService.getAccessToken();
          if (token != null) {
            options.headers['Authorization'] = 'Bearer $token';
          }
          return handler.next(options);
        },
        onError: (DioException error, handler) async {
          if (error.response?.statusCode == 401) {
            final refreshToken = await _storageService.getRefreshToken();
            if (refreshToken != null) {
              try {
                // Request token refresh
                final dioRefresh = Dio(BaseOptions(baseUrl: AppConstants.baseUrl));
                final response = await dioRefresh.post(
                  '/api/Auth/refresh-token',
                  data: {'refreshToken': refreshToken},
                );

                if (response.statusCode == 200) {
                  final data = response.data;
                  final newAccessToken = data['accessToken'];
                  final newRefreshToken = data['refreshToken'];

                  // Save new tokens, user stays the same
                  final cachedUser = await _storageService.getCachedUser();
                  if (cachedUser != null) {
                    await _storageService.saveSession(
                      newAccessToken,
                      newRefreshToken,
                      cachedUser,
                    );
                  }

                  // Resend request with new token
                  final options = error.requestOptions;
                  options.headers['Authorization'] = 'Bearer $newAccessToken';
                  final cloneResponse = await _dio.fetch(options);
                  return handler.resolve(cloneResponse);
                }
              } catch (_) {
                // If refresh fails, clear storage
                await _storageService.clearSession();
              }
            }
          }
          return handler.next(error);
        },
      ),
    );
  }

  Future<AuthResponseModel> login(String email, String password) async {
    try {
      final response = await _dio.post(
        '/api/Auth/login',
        data: {
          'email': email,
          'password': password,
        },
      );
      return AuthResponseModel.fromJson(response.data as Map<String, dynamic>);
    } on DioException catch (e) {
      throw _handleDioError(e);
    }
  }

  Future<AuthResponseModel> register({
    required String email,
    required String password,
    required String firstName,
    required String lastName,
    required String matriculationNumber,
    required int academicLevel,
    required int institutionId,
    String? phoneNumber,
  }) async {
    try {
      final response = await _dio.post(
        '/api/Auth/register',
        data: {
          'email': email,
          'password': password,
          'firstName': firstName,
          'lastName': lastName,
          'matriculationNumber': matriculationNumber,
          'academicLevel': academicLevel,
          'institutionId': institutionId,
          'phoneNumber': phoneNumber,
        },
      );
      return AuthResponseModel.fromJson(response.data as Map<String, dynamic>);
    } on DioException catch (e) {
      throw _handleDioError(e);
    }
  }

  Future<void> logout(String refreshToken) async {
    try {
      await _dio.post(
        '/api/Auth/logout',
        data: {
          'refreshToken': refreshToken,
        },
      );
    } on DioException catch (e) {
      throw _handleDioError(e);
    }
  }

  Future<List<EventModel>> getAllEvents() async {
    try {
      final response = await _dio.get('/api/Events/all');
      final list = response.data as List<dynamic>;
      await _storageService.saveCache('events', list);
      return list.map((item) => EventModel.fromJson(item as Map<String, dynamic>)).toList();
    } on DioException catch (e) {
      final cached = await _storageService.getCache('events');
      if (cached != null && cached is List) {
        return cached.map((item) => EventModel.fromJson(item as Map<String, dynamic>)).toList();
      }
      throw _handleDioError(e);
    }
  }

  Future<EventModel> createEvent(Map<String, dynamic> data) async {
    try {
      final response = await _dio.post('/api/Events/create', data: data);
      return EventModel.fromJson(response.data as Map<String, dynamic>);
    } on DioException catch (e) {
      throw _handleDioError(e);
    }
  }

  Future<EventModel> updateEvent(int id, Map<String, dynamic> data) async {
    try {
      final response = await _dio.put('/api/Events/update/$id', data: data);
      return EventModel.fromJson(response.data as Map<String, dynamic>);
    } on DioException catch (e) {
      throw _handleDioError(e);
    }
  }

  Future<void> deleteEvent(int id) async {
    try {
      await _dio.delete('/api/Events/delete/$id');
    } on DioException catch (e) {
      throw _handleDioError(e);
    }
  }

  Future<Map<String, dynamic>> verifyStudentId(String matricNumber) async {
    try {
      final response = await _dio.get('/api/Students/verify/$matricNumber');
      return response.data as Map<String, dynamic>;
    } on DioException catch (e) {
      throw _handleDioError(e);
    }
  }

  Future<Map<String, dynamic>> summarizeText(String text) async {
    try {
      final response = await _dio.post('/api/AI/summarize-text', data: {'text': text});
      return response.data as Map<String, dynamic>;
    } on DioException catch (e) {
      throw _handleDioError(e);
    }
  }

  Future<Map<String, dynamic>> generateQuiz(String content, String difficulty) async {
    try {
      final response = await _dio.post(
        '/api/AI/generate-quiz',
        data: {'content': content, 'difficultyLevel': difficulty},
      );
      return response.data as Map<String, dynamic>;
    } on DioException catch (e) {
      throw _handleDioError(e);
    }
  }

  Future<Map<String, dynamic>> generateStudyPlan(
    List<String> courses,
    List<Map<String, dynamic>> deadlines,
    Map<String, double> freeHours,
  ) async {
    try {
      final response = await _dio.post(
        '/api/AI/generate-study-plan',
        data: {
          'courses': courses,
          'deadlines': deadlines,
          'freeStudyHours': freeHours,
        },
      );
      return response.data as Map<String, dynamic>;
    } on DioException catch (e) {
      throw _handleDioError(e);
    }
  }

  Future<Map<String, dynamic>> checkInAttendance(String code) async {
    try {
      final response = await _dio.post('/api/Attendance/check-in', data: {'code': code});
      return response.data as Map<String, dynamic>;
    } on DioException catch (e) {
      throw _handleDioError(e);
    }
  }

  Future<Map<String, dynamic>> getAttendanceHistory(int studentId) async {
    try {
      final response = await _dio.get('/api/Attendance/history/$studentId');
      final data = response.data as Map<String, dynamic>;
      await _storageService.saveCache('attendance_history', data);
      return data;
    } on DioException catch (e) {
      final cached = await _storageService.getCache('attendance_history');
      if (cached != null && cached is Map<String, dynamic>) {
        return cached;
      }
      throw _handleDioError(e);
    }
  }

  Future<List<dynamic>> getAnnouncements() async {
    try {
      final response = await _dio.get('/api/Announcements/all');
      final list = response.data as List<dynamic>;
      await _storageService.saveCache('announcements', list);
      return list;
    } on DioException catch (e) {
      final cached = await _storageService.getCache('announcements');
      if (cached != null && cached is List) {
        return cached;
      }
      throw _handleDioError(e);
    }
  }

  Future<Map<String, dynamic>> getAnnouncementDetails(int id) async {
    try {
      final response = await _dio.get('/api/Announcements/$id');
      return response.data as Map<String, dynamic>;
    } on DioException catch (e) {
      throw _handleDioError(e);
    }
  }

  Future<Map<String, dynamic>> getUserNotifications(int userId) async {
    try {
      final response = await _dio.get('/api/Notifications/user/$userId');
      final data = response.data as Map<String, dynamic>;
      await _storageService.saveCache('notifications', data);
      return data;
    } on DioException catch (e) {
      final cached = await _storageService.getCache('notifications');
      if (cached != null && cached is Map<String, dynamic>) {
        return cached;
      }
      throw _handleDioError(e);
    }
  }

  Future<Map<String, dynamic>> markNotificationAsRead(int id) async {
    try {
      final response = await _dio.put('/api/Notifications/mark-read/$id');
      return response.data as Map<String, dynamic>;
    } on DioException catch (e) {
      throw _handleDioError(e);
    }
  }

  Future<Map<String, dynamic>> lecturerCreateAnnouncement(Map<String, dynamic> data) async {
    try {
      final response = await _dio.post('/api/Lecturer/create-announcement', data: data);
      return response.data as Map<String, dynamic>;
    } on DioException catch (e) {
      throw _handleDioError(e);
    }
  }

  Future<Map<String, dynamic>> lecturerCreateAttendanceSession(int offeringId, int duration) async {
    try {
      final response = await _dio.post('/api/Lecturer/create-attendance-session', data: {
        'courseOfferingId': offeringId,
        'durationInMinutes': duration,
      });
      return response.data as Map<String, dynamic>;
    } on DioException catch (e) {
      throw _handleDioError(e);
    }
  }

  Future<List<dynamic>> lecturerGetCourseAttendance(int courseId) async {
    try {
      final response = await _dio.get('/api/Lecturer/course-attendance/$courseId');
      return response.data as List<dynamic>;
    } on DioException catch (e) {
      throw _handleDioError(e);
    }
  }

  Future<List<dynamic>> lecturerGetCourseStudents(int courseId) async {
    try {
      final response = await _dio.get('/api/Lecturer/students/$courseId');
      return response.data as List<dynamic>;
    } on DioException catch (e) {
      throw _handleDioError(e);
    }
  }

  Future<Map<String, dynamic>> lecturerVerifyStudentId(String matricNo) async {
    try {
      final response = await _dio.post('/api/Lecturer/verify-student-id', data: {
        'matriculationNumber': matricNo,
      });
      return response.data as Map<String, dynamic>;
    } on DioException catch (e) {
      throw _handleDioError(e);
    }
  }

  Future<List<dynamic>> adminGetAllStudents() async {
    try {
      final response = await _dio.get('/api/Admin/all-students');
      return response.data as List<dynamic>;
    } on DioException catch (e) {
      throw _handleDioError(e);
    }
  }

  Future<List<dynamic>> adminGetAllLecturers() async {
    try {
      final response = await _dio.get('/api/Admin/all-lecturers');
      return response.data as List<dynamic>;
    } on DioException catch (e) {
      throw _handleDioError(e);
    }
  }

  Future<Map<String, dynamic>> adminCreateDepartment(String name, String code) async {
    try {
      final response = await _dio.post('/api/Admin/create-department', data: {
        'name': name,
        'code': code,
      });
      return response.data as Map<String, dynamic>;
    } on DioException catch (e) {
      throw _handleDioError(e);
    }
  }

  Future<Map<String, dynamic>> adminCreateCourse(int deptId, String code, String title, int credits) async {
    try {
      final response = await _dio.post('/api/Admin/create-course', data: {
        'departmentId': deptId,
        'code': code,
        'title': title,
        'creditUnits': credits,
      });
      return response.data as Map<String, dynamic>;
    } on DioException catch (e) {
      throw _handleDioError(e);
    }
  }

  Future<Map<String, dynamic>> adminRemoveStudent(int id) async {
    try {
      final response = await _dio.delete('/api/Admin/remove-student/$id');
      return response.data as Map<String, dynamic>;
    } on DioException catch (e) {
      throw _handleDioError(e);
    }
  }

  Future<Map<String, dynamic>> adminGetSystemStats() async {
    try {
      final response = await _dio.get('/api/Admin/system-stats');
      return response.data as Map<String, dynamic>;
    } on DioException catch (e) {
      throw _handleDioError(e);
    }
  }

  Failure _handleDioError(DioException error) {
    if (error.response != null) {
      final data = error.response?.data;
      String? message;
      if (data is Map) {
        message = data['message'] ?? data['title'] ?? data['errors']?.toString() ?? data['Detail'];
      }
      return ServerFailure(message ?? 'Server error ${error.response?.statusCode}');
    }
    if (error.type == DioExceptionType.connectionTimeout ||
        error.type == DioExceptionType.receiveTimeout) {
      return NetworkFailure('Network connection timeout');
    }
    if (error.type == DioExceptionType.connectionError) {
      return NetworkFailure('Failed to connect to the server. Please check if the backend is running.');
    }
    return NetworkFailure('No internet connection or server unreachable: ${error.message}');
  }
}
