import 'dart:io';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../data/ai_api_service.dart';
import '../../../auth/presentation/state/auth_provider.dart';

class AiState {
  final bool isLoading;
  final bool isSuccess;
  final String? resultMessage;
  final String? errorMessage;

  AiState({
    this.isLoading = false,
    this.isSuccess = false,
    this.resultMessage,
    this.errorMessage,
  });

  AiState copyWith({
    bool? isLoading,
    bool? isSuccess,
    String? resultMessage,
    String? errorMessage,
  }) {
    return AiState(
      isLoading: isLoading ?? this.isLoading,
      isSuccess: isSuccess ?? this.isSuccess,
      resultMessage: resultMessage ?? this.resultMessage,
      errorMessage: errorMessage,
    );
  }
}

final aiApiServiceProvider = Provider<AiApiService>((ref) {
  final apiClient = ref.watch(apiClientProvider);
  return AiApiService(apiClient);
});

class AiNotifier extends StateNotifier<AiState> {
  final AiApiService _apiService;

  AiNotifier(this._apiService) : super(AiState());

  Future<void> extractDeadline(String rawContent, String sourceChannel) async {
    state = state.copyWith(isLoading: true, isSuccess: false);
    try {
      final res = await _apiService.extractDeadline(rawContent, sourceChannel);
      state = AiState(isSuccess: true, resultMessage: res['message'] ?? 'Deadline extraction started.');
    } catch (e) {
      state = AiState(errorMessage: e.toString());
    }
  }

  Future<void> summarizeNotes(File file, int courseOfferingId) async {
    state = state.copyWith(isLoading: true, isSuccess: false);
    try {
      final res = await _apiService.summarizeNotes(file, courseOfferingId);
      state = AiState(isSuccess: true, resultMessage: res['message'] ?? 'Note summarization queued.');
    } catch (e) {
      state = AiState(errorMessage: e.toString());
    }
  }

  Future<void> generateQuiz({
    required int courseOfferingId,
    required String title,
    required String lectureNoteContent,
    required String difficultyLevel,
  }) async {
    state = state.copyWith(isLoading: true, isSuccess: false);
    try {
      final res = await _apiService.generateQuiz(
        courseOfferingId: courseOfferingId,
        title: title,
        lectureNoteContent: lectureNoteContent,
        difficultyLevel: difficultyLevel,
      );
      state = AiState(isSuccess: true, resultMessage: res['message'] ?? 'AI Quiz generation started.');
    } catch (e) {
      state = AiState(errorMessage: e.toString());
    }
  }

  Future<void> generateStudyPlan({
    required String name,
    required Map<String, double> availableFreeHours,
  }) async {
    state = state.copyWith(isLoading: true, isSuccess: false);
    try {
      final res = await _apiService.generateStudyPlan(name: name, availableFreeHours: availableFreeHours);
      state = AiState(isSuccess: true, resultMessage: res['message'] ?? 'AI Study Plan generation started.');
    } catch (e) {
      state = AiState(errorMessage: e.toString());
    }
  }
}

final aiProvider = StateNotifierProvider<AiNotifier, AiState>((ref) {
  final apiService = ref.watch(aiApiServiceProvider);
  return AiNotifier(apiService);
});
