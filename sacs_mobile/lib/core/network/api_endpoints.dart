class ApiEndpoints {
  static const String baseUrl = 'https://sacs-api.azurewebsites.net'; // Production backend placeholder

  // Authentication
  static const String register = '/api/auth/register';
  static const String login = '/api/auth/login';
  static const String refresh = '/api/auth/refresh';
  static const String logout = '/api/auth/logout';

  // Events
  static const String createEvent = '/api/events/create';
  static const String allEvents = '/api/events/all';
  static String eventById(int id) => '/api/events/$id';
  static String updateEvent(int id) => '/api/events/update/$id';
  static String deleteEvent(int id) => '/api/events/delete/$id';
  static const String uploadAttachment = '/api/events/upload';

  // Calendar
  static const String dailyCalendar = '/api/calendar/day';
  static const String weeklyCalendar = '/api/calendar/week';
  static const String monthlyCalendar = '/api/calendar/month';
  static const String upcomingCalendar = '/api/calendar/upcoming';

  // Reminders
  static const String configureReminders = '/api/reminders/configure';
  static const String myReminders = '/api/reminders/my-reminders';
  static String deleteReminder(int id) => '/api/reminders/delete/$id';

  // AI Features
  static const String extractDeadline = '/api/ai/extract-deadline';
  static const String summarizeNotes = '/api/ai/summarize-notes';
  static const String generateQuiz = '/api/ai/generate-quiz';
  static const String generateStudyPlan = '/api/ai/generate-study-plan';
}
