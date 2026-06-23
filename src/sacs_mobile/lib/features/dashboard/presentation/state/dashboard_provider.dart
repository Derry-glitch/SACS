import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../data/dashboard_api_service.dart';
import '../../data/models/calendar_event_model.dart';
import '../../../auth/presentation/state/auth_provider.dart';

class DashboardState {
  final bool isLoading;
  final List<CalendarEventModel> upcomingEvents;
  final List<CalendarEventModel> monthlyEvents;
  final List<String> aiRecommendations;
  final String? errorMessage;

  DashboardState({
    this.isLoading = false,
    this.upcomingEvents = const [],
    this.monthlyEvents = const [],
    this.aiRecommendations = const [],
    this.errorMessage,
  });

  DashboardState copyWith({
    bool? isLoading,
    List<CalendarEventModel>? upcomingEvents,
    List<CalendarEventModel>? monthlyEvents,
    List<String>? aiRecommendations,
    String? errorMessage,
  }) {
    return DashboardState(
      isLoading: isLoading ?? this.isLoading,
      upcomingEvents: upcomingEvents ?? this.upcomingEvents,
      monthlyEvents: monthlyEvents ?? this.monthlyEvents,
      aiRecommendations: aiRecommendations ?? this.aiRecommendations,
      errorMessage: errorMessage,
    );
  }
}

final dashboardApiServiceProvider = Provider<DashboardApiService>((ref) {
  final apiClient = ref.watch(apiClientProvider);
  return DashboardApiService(apiClient);
});

class DashboardNotifier extends StateNotifier<DashboardState> {
  final DashboardApiService _apiService;

  DashboardNotifier(this._apiService) : super(DashboardState()) {
    refreshDashboard();
  }

  Future<void> refreshDashboard() async {
    state = state.copyWith(isLoading: true);
    try {
      final upcoming = await _apiService.getUpcomingEvents();
      final monthly = await _apiService.getMonthlyEvents(DateTime.now());
      
      // Calculate AI recommendations locally based on upcoming events & deadlines
      final recommendations = _generateRecommendations(upcoming);

      state = DashboardState(
        upcomingEvents: upcoming,
        monthlyEvents: monthly,
        aiRecommendations: recommendations,
      );
    } catch (e) {
      state = state.copyWith(isLoading: false, errorMessage: e.toString());
    }
  }

  List<String> _generateRecommendations(List<CalendarEventModel> events) {
    final recommendations = <String>[];
    if (events.isEmpty) {
      recommendations.add("You're all caught up! Great time to summarize your lecture notes.");
      recommendations.add("Generate a mock quiz from your recent lectures to test your retention.");
      return recommendations;
    }

    final exams = events.where((e) => e.eventType.toLowerCase() == 'exam').toList();
    if (exams.isNotEmpty) {
      recommendations.add("Upcoming exam: '${exams.first.title}'. Generate a customized study plan to start revision.");
    }

    final assignments = events.where((e) => e.eventType.toLowerCase() == 'assignment').toList();
    if (assignments.isNotEmpty) {
      recommendations.add("Assignment deadline approaching: '${assignments.first.title}'. Use our study planner to allocate hours.");
    }

    if (recommendations.length < 2) {
      recommendations.add("Optimize your schedule. Try using the AI planner to allocate study blocks.");
    }

    return recommendations;
  }
}

final dashboardProvider = StateNotifierProvider<DashboardNotifier, DashboardState>((ref) {
  final apiService = ref.watch(dashboardApiServiceProvider);
  return DashboardNotifier(apiService);
});
