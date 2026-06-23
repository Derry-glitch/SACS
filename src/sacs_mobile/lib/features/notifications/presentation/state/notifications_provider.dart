import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../data/notifications_api_service.dart';
import '../../data/models/reminder_model.dart';
import '../../../auth/presentation/state/auth_provider.dart';

class NotificationsState {
  final bool isLoading;
  final List<ReminderModel> reminders;
  final String? errorMessage;

  NotificationsState({
    this.isLoading = false,
    this.reminders = const [],
    this.errorMessage,
  });

  NotificationsState copyWith({
    bool? isLoading,
    List<ReminderModel>? reminders,
    String? errorMessage,
  }) {
    return NotificationsState(
      isLoading: isLoading ?? this.isLoading,
      reminders: reminders ?? this.reminders,
      errorMessage: errorMessage,
    );
  }
}

final notificationsApiServiceProvider = Provider<NotificationsApiService>((ref) {
  final apiClient = ref.watch(apiClientProvider);
  return NotificationsApiService(apiClient);
});

class NotificationsNotifier extends StateNotifier<NotificationsState> {
  final NotificationsApiService _apiService;

  NotificationsNotifier(this._apiService) : super(NotificationsState()) {
    refreshReminders();
  }

  Future<void> refreshReminders() async {
    state = state.copyWith(isLoading: true);
    try {
      final list = await _apiService.getMyReminders();
      state = NotificationsState(reminders: list);
    } catch (e) {
      state = state.copyWith(isLoading: false, errorMessage: e.toString());
    }
  }

  Future<void> deleteReminder(int id) async {
    try {
      await _apiService.deleteReminder(id);
      final updatedList = state.reminders.where((r) => r.id != id).toList();
      state = state.copyWith(reminders: updatedList);
    } catch (e) {
      state = state.copyWith(errorMessage: e.toString());
    }
  }
}

final notificationsProvider = StateNotifierProvider<NotificationsNotifier, NotificationsState>((ref) {
  final apiService = ref.watch(notificationsApiServiceProvider);
  return NotificationsNotifier(apiService);
});
