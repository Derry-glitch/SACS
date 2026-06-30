import 'package:go_router/go_router.dart';
import '../../screens/login_screen.dart';
import '../../screens/register_screen.dart';
import '../../screens/dashboard_screen.dart';
import '../../screens/event_detail_screen.dart';
import '../../screens/event_form_screen.dart';
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
    ],
  );
}
