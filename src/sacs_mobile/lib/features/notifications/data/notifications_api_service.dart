import 'models/reminder_model.dart';
import '../../../core/network/api_client.dart';
import '../../../core/network/api_endpoints.dart';

class NotificationsApiService {
  final ApiClient _apiClient;

  NotificationsApiService(this._apiClient);

  Future<List<ReminderModel>> getMyReminders() async {
    final response = await _apiClient.get(ApiEndpoints.myReminders);
    final list = response.data as List<dynamic>;
    return list.map((item) => ReminderModel.fromJson(item as Map<String, dynamic>)).toList();
  }

  Future<void> deleteReminder(int id) async {
    await _apiClient.delete(ApiEndpoints.deleteReminder(id));
  }
}
