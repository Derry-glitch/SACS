import 'dart:io';
import 'package:flutter/foundation.dart';

class ApiEndpoints {
  static String get baseUrl {
    if (kIsWeb) {
      return 'http://localhost:5058';
    }
    try {
      if (Platform.isAndroid) {
        return 'http://10.0.2.2:5058';
      }
    } catch (_) {}
    return 'http://localhost:5058';
  }

  // Authentication
  static const String register = '/api/auth/register';
  static const String login = '/api/auth/login';
  static const String refresh = '/api/auth/refresh';
  static const String logout = '/api/auth/logout';

  // Events
  static const String createEvent = '/api/Events/create';
  static const String allEvents = '/api/Events/all';
  static String eventById(int id) => '/api/Events/$id';
  static String updateEvent(int id) => '/api/Events/update/$id';
  static String deleteEvent(int id) => '/api/Events/delete/$id';
  static const String uploadAttachment = '/api/Events/upload';

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
  static const String extractDeadline = '/api/DeadlineExtraction/extract';
  static const String summarizeNotes = '/api/LectureSummary/summarize';
  static const String generateQuiz = '/api/QuizGeneration/generate';
  static const String generateStudyPlan = '/api/StudyPlan/generate';
}
