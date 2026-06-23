import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../features/auth/presentation/screens/login_screen.dart';
import '../../features/auth/presentation/screens/register_screen.dart';
import '../../features/auth/presentation/screens/forgot_password_screen.dart';
import '../../features/dashboard/presentation/screens/dashboard_screen.dart';
import '../../features/events/presentation/screens/event_create_screen.dart';
import '../../features/events/presentation/screens/study_planner_screen.dart';
import '../../features/ai/presentation/screens/ai_features_screen.dart';
import '../../features/notifications/presentation/screens/notification_center_screen.dart';
import '../../features/auth/presentation/state/auth_provider.dart';

final routerProvider = Provider<GoRouter>((ref) {
  final authState = ref.watch(authProvider);

  return GoRouter(
    initialLocation: '/login',
    redirect: (context, state) {
      final isLoggedIn = authState.isAuthenticated;
      final isLoggingIn = state.matchedLocation == '/login' ||
          state.matchedLocation == '/register' ||
          state.matchedLocation == '/forgot-password';

      if (!isLoggedIn && !isLoggingIn) {
        return '/login';
      }
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
        path: '/forgot-password',
        builder: (context, state) => const ForgotPasswordScreen(),
      ),
      GoRoute(
        path: '/',
        builder: (context, state) => const DashboardScreen(),
      ),
      GoRoute(
        path: '/create-event',
        builder: (context, state) => const EventCreateScreen(),
      ),
      GoRoute(
        path: '/study-planner',
        builder: (context, state) => const StudyPlannerScreen(),
      ),
      GoRoute(
        path: '/ai-features',
        builder: (context, state) => const AIFeaturesScreen(),
      ),
      GoRoute(
        path: '/notifications',
        builder: (context, state) => const NotificationCenterScreen(),
      ),
    ],
  );
});
