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
import '../../screens/lecturer_dashboard_screen.dart';
import '../../screens/admin_dashboard_screen.dart';
import '../../screens/attendance_session_screen.dart';
import '../../screens/manage_students_screen.dart';
import '../../screens/system_stats_screen.dart';
import '../../screens/biometric_settings_screen.dart';
import '../../screens/pin_lock_screen.dart';
import 'package:flutter/material.dart';
import '../../providers/auth_provider.dart';
import '../../models/event_model.dart';
import '../../services/biometric_service.dart';
import '../theme/app_theme.dart';

class AppRouter {
  static bool sessionUnlocked = false;
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
        AppRouter.sessionUnlocked = false;
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
        builder: (context, state) {
          final user = authProvider.user;
          final bioService = BiometricService();

          return FutureBuilder<bool>(
            future: Future.wait([
              bioService.isBiometricsEnabled(),
              bioService.isPinEnabled(),
            ]).then((results) => results[0] || results[1]),
            builder: (context, snapshot) {
              if (snapshot.connectionState == ConnectionState.waiting) {
                return const Scaffold(
                  body: Center(
                    child: CircularProgressIndicator(color: AppTheme.primaryLight),
                  ),
                );
              }

              final isSecurityActive = snapshot.data ?? false;
              if (isSecurityActive && !AppRouter.sessionUnlocked) {
                // If biometrics is enabled, try authenticating with biometrics first
                return FutureBuilder<bool>(
                  future: bioService.isBiometricsEnabled().then((enabled) async {
                    if (enabled) {
                      return await bioService.authenticate('Unlock SACS Secure Portal');
                    }
                    return false;
                  }),
                  builder: (context, bioSnapshot) {
                    if (bioSnapshot.connectionState == ConnectionState.waiting) {
                      return const Scaffold(
                        body: Center(
                          child: CircularProgressIndicator(color: AppTheme.primaryLight),
                        ),
                      );
                    }

                    final bioSuccess = bioSnapshot.data ?? false;
                    if (bioSuccess) {
                      AppRouter.sessionUnlocked = true;
                      WidgetsBinding.instance.addPostFrameCallback((_) {
                        (context as Element).markNeedsBuild();
                      });
                    }

                    // Fallback to PinLockScreen if PIN is enabled or biometrics failed/dismissed
                    return PinLockScreen(
                      onSuccess: () {
                        AppRouter.sessionUnlocked = true;
                        WidgetsBinding.instance.addPostFrameCallback((_) {
                          (context as Element).markNeedsBuild();
                        });
                      },
                    );
                  },
                );
              }

              // Normal flow when unlocked
              if (user?.role.toLowerCase() == 'lecturer') {
                return const LecturerDashboardScreen();
              } else if (user?.role.toLowerCase() == 'admin') {
                return const AdminDashboardScreen();
              } else {
                return const DashboardScreen();
              }
            },
          );
        },
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
      GoRoute(
        path: '/lecturer-dashboard',
        builder: (context, state) => const LecturerDashboardScreen(),
      ),
      GoRoute(
        path: '/admin-dashboard',
        builder: (context, state) => const AdminDashboardScreen(),
      ),
      GoRoute(
        path: '/attendance-session',
        builder: (context, state) => const AttendanceSessionScreen(),
      ),
      GoRoute(
        path: '/manage-students',
        builder: (context, state) => const ManageStudentsScreen(),
      ),
      GoRoute(
        path: '/system-stats',
        builder: (context, state) => const SystemStatsScreen(),
      ),
      GoRoute(
        path: '/biometric-settings',
        builder: (context, state) => const BiometricSettingsScreen(),
      ),
      GoRoute(
        path: '/pin-lock',
        builder: (context, state) {
          final callback = state.extra as VoidCallback? ?? () => context.go('/');
          return PinLockScreen(onSuccess: callback);
        },
      ),
    ],
  );
}
