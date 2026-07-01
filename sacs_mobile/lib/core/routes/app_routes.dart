import 'package:go_router/go_router.dart';
import '../../screens/login_screen.dart';
import '../../screens/register_screen.dart';
import '../../screens/dashboard_screen.dart';
import '../../screens/event_detail_screen.dart';
import '../../screens/event_form_screen.dart';
import '../../screens/student_id_screen.dart';
import '../../screens/id_verification_screen.dart';
import '../../screens/ai_dashboard_screen.dart';
import '../../screens/note_summarizer_screen.dart';
import '../../screens/quiz_generator_screen.dart';
import '../../screens/study_planner_screen.dart';
import '../../screens/attendance_screen.dart';
import '../../screens/attendance_history_screen.dart';
import '../../screens/announcements_screen.dart';
import '../../screens/notification_screen.dart';
import '../../screens/announcement_detail_screen.dart';
import '../../providers/auth_provider.dart';
import '../../models/event_model.dart';

class AppRouter {
  final AuthProvider authProvider;

  AppRouter(this.authProvider);

  late final GoRouter router = GoRouter(
    initialLocation: '/login',
    refreshListenable: authProvider,
    redirect: (context, state) {
      final isLoggedIn = authProvider.isAuthenticated;
      final isLoggingIn = state.matchedLocation == '/login' ||
          state.matchedLocation == '/register';

      // If not logged in and not on login/register, go to login
      if (!isLoggedIn && !isLoggingIn) {
        return '/login';
      }
      
      // If logged in and on login/register, go to dashboard (/)
      if (isLoggedIn && isLoggingIn) {
        return '/';
      }

      return null;
    },
    routes: [
      GoRoute(
        path: '/login',
        builder: (context, state) => const LoginScreen(),
      ),
      GoRoute(
        path: '/register',
        builder: (context, state) => const RegisterScreen(),
      ),
      GoRoute(
        path: '/',
        builder: (context, state) => const DashboardScreen(),
      ),
      GoRoute(
        path: '/event-details',
        builder: (context, state) => EventDetailScreen(
          event: state.extra as EventModel,
        ),
      ),
      GoRoute(
        path: '/create-event',
        builder: (context, state) => const EventFormScreen(),
      ),
      GoRoute(
        path: '/edit-event',
        builder: (context, state) => EventFormScreen(
          event: state.extra as EventModel?,
        ),
      ),
      GoRoute(
        path: '/student-id',
        builder: (context, state) => const StudentIdScreen(),
      ),
      GoRoute(
        path: '/verify-id',
        builder: (context, state) => const IdVerificationScreen(),
      ),
      GoRoute(
        path: '/ai-dashboard',
        builder: (context, state) => const AiDashboardScreen(),
      ),
      GoRoute(
        path: '/note-summarizer',
        builder: (context, state) => const NoteSummarizerScreen(),
      ),
      GoRoute(
        path: '/quiz-generator',
        builder: (context, state) => const QuizGenerationScreen(),
      ),
      GoRoute(
        path: '/study-planner',
        builder: (context, state) => const StudyPlannerScreen(),
      ),
      GoRoute(
        path: '/attendance',
        builder: (context, state) => const AttendanceScreen(),
      ),
      GoRoute(
        path: '/attendance-history',
        builder: (context, state) => const AttendanceHistoryScreen(),
      ),
      GoRoute(
        path: '/announcements',
        builder: (context, state) => const AnnouncementsScreen(),
      ),
      GoRoute(
        path: '/notifications',
        builder: (context, state) => const NotificationScreen(),
      ),
      GoRoute(
        path: '/announcement-detail',
        builder: (context, state) {
          final id = state.extra as int;
          return AnnouncementDetailScreen(announcementId: id);
        },
      ),
    ],
  );
}
