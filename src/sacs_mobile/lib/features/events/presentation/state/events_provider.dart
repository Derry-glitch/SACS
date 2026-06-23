import 'dart:io';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../data/events_api_service.dart';
import '../../data/models/event_model.dart';
import '../../../auth/presentation/state/auth_provider.dart';

class EventsState {
  final bool isLoading;
  final bool isSuccess;
  final EventModel? createdEvent;
  final String? errorMessage;

  EventsState({
    this.isLoading = false,
    this.isSuccess = false,
    this.createdEvent,
    this.errorMessage,
  });

  EventsState copyWith({
    bool? isLoading,
    bool? isSuccess,
    EventModel? createdEvent,
    String? errorMessage,
  }) {
    return EventsState(
      isLoading: isLoading ?? this.isLoading,
      isSuccess: isSuccess ?? this.isSuccess,
      createdEvent: createdEvent ?? this.createdEvent,
      errorMessage: errorMessage,
    );
  }
}

final eventsApiServiceProvider = Provider<EventsApiService>((ref) {
  final apiClient = ref.watch(apiClientProvider);
  return EventsApiService(apiClient);
});

class EventsNotifier extends StateNotifier<EventsState> {
  final EventsApiService _apiService;

  EventsNotifier(this._apiService) : super(EventsState());

  Future<void> createEvent({
    required String title,
    String? description,
    required int courseId,
    required int eventType,
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
    state = state.copyWith(isLoading: true, isSuccess: false);
    try {
      final event = await _apiService.createEvent(
        title: title,
        description: description,
        courseId: courseId,
        eventType: eventType,
        dueDateTime: dueDateTime,
        priorityLevel: priorityLevel,
        notes: notes,
        attachmentUrl: attachmentUrl,
        durationMinutes: durationMinutes,
        venue: venue,
        seatNumber: seatNumber,
        supervisorName: supervisorName,
        progressPercentage: progressPercentage,
        studyTopic: studyTopic,
        studyDuration: studyDuration,
      );
      state = EventsState(isSuccess: true, createdEvent: event);
    } catch (e) {
      state = EventsState(errorMessage: e.toString());
    }
  }

  Future<String?> uploadFile(File file) async {
    state = state.copyWith(isLoading: true);
    try {
      final url = await _apiService.uploadAttachment(file);
      state = state.copyWith(isLoading: false);
      return url;
    } catch (e) {
      state = state.copyWith(isLoading: false, errorMessage: e.toString());
      return null;
    }
  }
}

final eventsProvider = StateNotifierProvider<EventsNotifier, EventsState>((ref) {
  final apiService = ref.watch(eventsApiServiceProvider);
  return EventsNotifier(apiService);
});
