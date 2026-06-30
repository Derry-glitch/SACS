import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:google_fonts/google_fonts.dart';
import '../providers/auth_provider.dart';
import '../providers/event_provider.dart';
import '../widgets/student_welcome_card.dart';
import '../widgets/user_profile_section.dart';
import '../widgets/upcoming_deadlines_widget.dart';
import '../widgets/calendar_preview_widget.dart';
import '../widgets/quick_actions_widget.dart';
import '../core/theme/app_theme.dart';

class DashboardScreen extends StatefulWidget {
  const DashboardScreen({super.key});

  @override
  State<DashboardScreen> createState() => _DashboardScreenState();
}

class _DashboardScreenState extends State<DashboardScreen> {
  @override
  void initState() {
    super.initState();
    // Fetch events after the widget tree is built
    WidgetsBinding.instance.addPostFrameCallback((_) {
      context.read<EventProvider>().fetchEvents();
    });
  }

  @override
  Widget build(BuildContext context) {
    final authState = context.watch<AuthProvider>();
    final eventState = context.watch<EventProvider>();
    final currentUser = authState.user;

    return Scaffold(
      body: Container(
        decoration: const BoxDecoration(
          gradient: LinearGradient(
            begin: Alignment.topCenter,
            end: Alignment.bottomCenter,
            colors: [
              AppTheme.bgDark,
              AppTheme.bgDarkSecondary,
            ],
          ),
        ),
        child: SafeArea(
          child: RefreshIndicator(
            onRefresh: () => context.read<EventProvider>().fetchEvents(),
            color: AppTheme.primaryLight,
            child: SingleChildScrollView(
              physics: const AlwaysScrollableScrollPhysics(),
              padding: const EdgeInsets.all(20.0),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  // Greeting Header
                  Row(
                    mainAxisAlignment: MainAxisAlignment.spaceBetween,
                    children: [
                      Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Text(
                            'SACS Companion',
                            style: GoogleFonts.outfit(
                              fontSize: 26,
                              fontWeight: FontWeight.bold,
                              color: AppTheme.textPrimary,
                            ),
                          ),
                          const SizedBox(height: 4),
                          Text(
                            'Your academic schedule, organized.',
                            style: Theme.of(context).textTheme.bodyMedium,
                          ),
                        ],
                      ),
                      IconButton(
                        onPressed: () => context.read<EventProvider>().fetchEvents(),
                        icon: const Icon(Icons.sync_rounded, color: AppTheme.primaryLight),
                        tooltip: 'Sync Data',
                      ),
                    ],
                  ),
                  const SizedBox(height: 20),

                  // Student Welcome Card
                  if (currentUser != null) ...[
                    StudentWelcomeCard(user: currentUser),
                    const SizedBox(height: 20),
                  ],

                  // User Profile Section (with Logout button)
                  if (currentUser != null) ...[
                    UserProfileSection(user: currentUser),
                    const SizedBox(height: 24),
                  ],

                  // Quick Action Buttons
                  Text(
                    'Quick Actions',
                    style: Theme.of(context).textTheme.titleLarge,
                  ),
                  const SizedBox(height: 12),
                  const QuickActionsWidget(),
                  const SizedBox(height: 28),

                  // Calendar Preview Strip
                  Text(
                    'Academic Calendar',
                    style: Theme.of(context).textTheme.titleLarge,
                  ),
                  const SizedBox(height: 12),
                  const CalendarPreviewWidget(),
                  const SizedBox(height: 28),

                  // Upcoming Deadlines List
                  Text(
                    'Upcoming Deadlines',
                    style: Theme.of(context).textTheme.titleLarge,
                  ),
                  const SizedBox(height: 12),
                  UpcomingDeadlinesWidget(
                    events: eventState.events,
                    isLoading: eventState.isLoading,
                  ),
                ],
              ),
            ),
          ),
        ),
      ),
    );
  }
}
