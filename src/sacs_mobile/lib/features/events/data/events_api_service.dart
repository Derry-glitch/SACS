import 'dart:io';
import 'package:dio/dio.dart';
import 'models/event_model.dart';
import '../../../core/network/api_client.dart';
import '../../../core/network/api_endpoints.dart';

class EventsApiService {
  final ApiClient _apiClient;

  EventsApiService(this._apiClient);

  Future<EventModel> createEvent({
    required String title,
    String? description,
    required int courseId,
    required int eventType, // Enum representation (0=Assignment, 1=Quiz, etc)
    required DateTime dueDateTime,
    required String priorityLevel,
    String? notes,
    String? attachmentUrl,
    int? durationMinutes,
    String? venue,
    String? seatNumber,
    String? supervisorName,
    int? progressPercentage,
    String? studyTopic,
    int? studyDuration,
  }) async {
    final response = await _apiClient.post(
      ApiEndpoints.createEvent,
      data: {
        'title': title,
        'description': description,
        'courseId': courseId,
        'eventType': eventType,
        'dueDateTime': dueDateTime.toIso8601String(),
        'priorityLevel': priorityLevel,
        'notes': notes,
        'attachmentUrl': attachmentUrl,
        'durationMinutes': durationMinutes,
        'venue': venue,
        'seatNumber': seatNumber,
        'supervisorName': supervisorName,
        'progressPercentage': progressPercentage,
        'studyTopic': studyTopic,
        'studyDuration': studyDuration,
      },
    );
    return EventModel.fromJson(response.data as Map<String, dynamic>);
  }

  Future<String> uploadAttachment(File file) async {
    final fileName = file.path.split('/').last;
    final formData = FormData.fromMap({
      'file': await MultipartFile.fromFile(file.path, filename: fileName),
    });

    final response = await _apiClient.post(
      ApiEndpoints.uploadAttachment,
      data: formData,
      options: Options(contentType: 'multipart/form-data'),
    );

    return response.data['attachmentUrl'] as String;
  }
}
