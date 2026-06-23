import 'dart:io';
import 'package:dio/dio.dart';
import '../../../core/network/api_client.dart';
import '../../../core/network/api_endpoints.dart';

class AiApiService {
  final ApiClient _apiClient;

  AiApiService(this._apiClient);

  Future<Map<String, dynamic>> extractDeadline(String rawContent, String sourceChannel) async {
    final response = await _apiClient.post(
      ApiEndpoints.extractDeadline,
      data: {
        'rawContent': rawContent,
        'sourceChannel': sourceChannel,
      },
    );
    return response.data as Map<String, dynamic>;
  }

  Future<Map<String, dynamic>> summarizeNotes(File file, int courseOfferingId) async {
    final fileName = file.path.split('/').last;
    final formData = FormData.fromMap({
      'file': await MultipartFile.fromFile(file.path, filename: fileName),
      'courseOfferingId': courseOfferingId,
    });

    final response = await _apiClient.post(
      ApiEndpoints.summarizeNotes,
      data: formData,
      options: Options(contentType: 'multipart/form-data'),
    );
    return response.data as Map<String, dynamic>;
  }

  Future<Map<String, dynamic>> generateQuiz({
    required int courseOfferingId,
    required String title,
    required String lectureNoteContent,
    required String difficultyLevel,
  }) async {
    final response = await _apiClient.post(
      ApiEndpoints.generateQuiz,
      data: {
        'courseOfferingId': courseOfferingId,
        'title': title,
        'lectureNoteContent': lectureNoteContent,
        'difficultyLevel': difficultyLevel,
      },
    );
    return response.data as Map<String, dynamic>;
  }

  Future<Map<String, dynamic>> generateStudyPlan({
    required String name,
    required Map<String, double> availableFreeHours,
  }) async {
    final response = await _apiClient.post(
      ApiEndpoints.generateStudyPlan,
      data: {
        'name': name,
        'availableFreeHours': availableFreeHours,
      },
    );
    return response.data as Map<String, dynamic>;
  }
}
