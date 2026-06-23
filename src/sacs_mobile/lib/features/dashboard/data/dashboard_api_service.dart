import 'models/calendar_event_model.dart';
import '../../../core/network/api_client.dart';
import '../../../core/network/api_endpoints.dart';

class DashboardApiService {
  final ApiClient _apiClient;

  DashboardApiService(this._apiClient);

  Future<List<CalendarEventModel>> getUpcomingEvents() async {
    final response = await _apiClient.get(ApiEndpoints.upcomingCalendar);
    final list = response.data as List<dynamic>;
    return list.map((item) => CalendarEventModel.fromJson(item as Map<String, dynamic>)).toList();
  }

  Future<List<CalendarEventModel>> getMonthlyEvents(DateTime date) async {
    final dateStr = '${date.year}-${date.month.toString().padLeft(2, '0')}-${date.day.toString().padLeft(2, '0')}';
    final response = await _apiClient.get(
      ApiEndpoints.monthlyCalendar,
      queryParameters: {'date': dateStr},
    );
    final list = response.data as List<dynamic>;
    return list.map((item) => CalendarEventModel.fromJson(item as Map<String, dynamic>)).toList();
  }
}
