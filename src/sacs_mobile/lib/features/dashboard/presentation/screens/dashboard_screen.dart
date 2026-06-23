import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:intl/intl.dart';
import 'package:google_fonts/google_fonts.dart';

import '../state/dashboard_provider.dart';
import '../../../auth/presentation/state/auth_provider.dart';
import '../../../../core/theme/app_theme.dart';

class DashboardScreen extends ConsumerStatefulWidget {
  const DashboardScreen({super.key});

  @override
  ConsumerState<DashboardScreen> createState() => _DashboardScreenState();
}

class _DashboardScreenState extends ConsumerState<DashboardScreen> {
  int _currentIndex = 0;

  @override
  Widget build(BuildContext context) {
    final dashboardState = ref.watch(dashboardProvider);
    final authState = ref.watch(authProvider);

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
            onRefresh: () => ref.read(dashboardProvider.notifier).refreshDashboard(),
            color: AppTheme.primaryLight,
            child: SingleChildScrollView(
              physics: const AlwaysScrollableScrollPhysics(),
              padding: const EdgeInsets.all(20.0),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  // Header Greeting
                  _buildHeader(authState.user?.firstName ?? 'Student'),
                  const SizedBox(height: 24),

                  // Progress Overview Card
                  _buildProgressOverview(),
                  const SizedBox(height: 28),

                  // AI Study Recommendations Section
                  _buildAIRecommendations(dashboardState.aiRecommendations),
                  const SizedBox(height: 28),

                  // Calendar Widget (Horizonal date selector)
                  _buildCalendarStrip(),
                  const SizedBox(height: 28),

                  // Upcoming Deadlines list
                  _buildUpcomingDeadlines(dashboardState),
                ],
              ),
            ),
          ),
        ),
      ),
      bottomNavigationBar: _buildBottomNavBar(),
    );
  }

  Widget _buildHeader(String name) {
    return Row(
      mainAxisAlignment: MainAxisAlignment.spaceBetween,
      children: [
        Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              'Hello, $name',
              style: GoogleFonts.outfit(
                fontSize: 26,
                fontWeight: FontWeight.bold,
                color: AppTheme.textPrimary,
              ),
            ),
            const SizedBox(height: 4),
            Text(
              'Ready to track your academic goals?',
              style: Theme.of(context).textTheme.bodyMedium,
            ),
          ],
        ),
        IconButton(
          onPressed: () => ref.read(authProvider.notifier).logout(),
          icon: const Icon(Icons.logout_rounded, color: AppTheme.error),
          tooltip: 'Sign Out',
        ),
      ],
    );
  }

  Widget _buildProgressOverview() {
    return Container(
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(
        gradient: LinearGradient(
          colors: [
            AppTheme.primary.withOpacity(0.8),
            AppTheme.primaryLight.withOpacity(0.4),
          ],
        ),
        borderRadius: BorderRadius.circular(20),
        border: Border.all(color: Colors.white.withOpacity(0.1)),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              Text(
                'Semester Progress',
                style: GoogleFonts.outfit(
                  fontSize: 18,
                  fontWeight: FontWeight.w600,
                  color: AppTheme.textPrimary,
                ),
              ),
              const Icon(Icons.analytics_outlined, color: AppTheme.accent),
            ],
          ),
          const SizedBox(height: 16),
          const LinearProgressIndicator(
            value: 0.65, // Example progress
            backgroundColor: AppTheme.bgDark,
            valueColor: AlwaysStoppedAnimation(AppTheme.accent),
            minHeight: 10,
            borderRadius: BorderRadius.all(Radius.circular(5)),
          ),
          const SizedBox(height: 12),
          Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              Text(
                '65% completed',
                style: Theme.of(context).textTheme.bodyMedium?.copyWith(
                      color: AppTheme.textPrimary,
                    ),
              ),
              Text(
                '4 Active Courses',
                style: Theme.of(context).textTheme.bodyMedium?.copyWith(
                      color: AppTheme.textPrimary,
                    ),
              ),
            ],
          ),
        ],
      ),
    );
  }

  Widget _buildAIRecommendations(List<String> recommendations) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Row(
          children: [
            const Icon(Icons.auto_awesome_rounded, color: AppTheme.accent, size: 20),
            const SizedBox(width: 8),
            Text(
              'AI Study Assistant',
              style: Theme.of(context).textTheme.titleLarge,
            ),
          ],
        ),
        const SizedBox(height: 12),
        if (recommendations.isEmpty)
          const Card(
            child: Padding(
              padding: EdgeInsets.all(16.0),
              child: Text("Loading recommendation updates..."),
            ),
          )
        else
          SizedBox(
            height: 110,
            child: ListView.builder(
              scrollDirection: Axis.horizontal,
              itemCount: recommendations.length,
              itemBuilder: (context, index) {
                return Container(
                  width: 280,
                  margin: const EdgeInsets.only(right: 16),
                  padding: const EdgeInsets.all(16),
                  decoration: BoxDecoration(
                    color: AppTheme.bgDarkSecondary,
                    borderRadius: BorderRadius.circular(16),
                    border: Border.all(
                      color: AppTheme.accent.withOpacity(0.2),
                    ),
                  ),
                  child: Row(
                    children: [
                      const Icon(Icons.lightbulb_outline_rounded, color: AppTheme.accent),
                      const SizedBox(width: 12),
                      Expanded(
                        child: Text(
                          recommendations[index],
                          style: const TextStyle(fontSize: 13, height: 1.4),
                          maxLines: 3,
                          overflow: TextOverflow.ellipsis,
                        ),
                      ),
                    ],
                  ),
                );
              },
            ),
          ),
      ],
    );
  }

  Widget _buildCalendarStrip() {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text(
          'Academic Calendar',
          style: Theme.of(context).textTheme.titleLarge,
        ),
        const SizedBox(height: 12),
        SizedBox(
          height: 80,
          child: ListView.builder(
            scrollDirection: Axis.horizontal,
            itemCount: 7,
            itemBuilder: (context, index) {
              final date = DateTime.now().add(Duration(days: index));
              final isToday = index == 0;
              return Container(
                width: 60,
                margin: const EdgeInsets.only(right: 12),
                decoration: BoxDecoration(
                  color: isToday ? AppTheme.primaryLight : AppTheme.bgDarkSecondary,
                  borderRadius: BorderRadius.circular(12),
                  border: Border.all(
                    color: isToday ? AppTheme.accent : Colors.white.withOpacity(0.05),
                  ),
                ),
                child: Column(
                  mainAxisAlignment: MainAxisAlignment.center,
                  children: [
                    Text(
                      DateFormat('E').format(date),
                      style: TextStyle(
                        color: isToday ? Colors.white : AppTheme.textSecondary,
                        fontWeight: FontWeight.bold,
                      ),
                    ),
                    const SizedBox(height: 4),
                    Text(
                      date.day.toString(),
                      style: GoogleFonts.outfit(
                        fontSize: 18,
                        fontWeight: FontWeight.bold,
                        color: Colors.white,
                      ),
                    ),
                  ],
                ),
              );
            },
          ),
        ),
      ],
    );
  }

  Widget _buildUpcomingDeadlines(DashboardState state) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text(
          'Upcoming Deadlines',
          style: Theme.of(context).textTheme.titleLarge,
        ),
        const SizedBox(height: 12),
        if (state.isLoading)
          const Center(
            child: Padding(
              padding: EdgeInsets.all(24.0),
              child: CircularProgressIndicator(),
            ),
          )
        else if (state.upcomingEvents.isEmpty)
          const Card(
            child: Padding(
              padding: EdgeInsets.symmetric(vertical: 24, horizontal: 16),
              child: Center(
                child: Text('No upcoming deadlines. Hooray!'),
              ),
            ),
          )
        else
          ListView.builder(
            shrinkWrap: true,
            physics: const NeverScrollableScrollPhysics(),
            itemCount: state.upcomingEvents.length,
            itemBuilder: (context, index) {
              final event = state.upcomingEvents[index];
              final isHighPriority = event.priority.toLowerCase() == 'high' || event.priority.toLowerCase() == 'critical';

              return Card(
                margin: const EdgeInsets.only(bottom: 12),
                child: ListTile(
                  leading: CircleAvatar(
                    backgroundColor: isHighPriority ? AppTheme.error.withOpacity(0.15) : AppTheme.primaryLight.withOpacity(0.15),
                    child: Icon(
                      Icons.alarm,
                      color: isHighPriority ? AppTheme.error : AppTheme.primaryLight,
                    ),
                  ),
                  title: Text(
                    event.title,
                    style: const TextStyle(fontWeight: FontWeight.bold),
                  ),
                  subtitle: Text(
                    '${event.courseName} • Due ${DateFormat('MMM d, h:mm a').format(event.dueDate)}',
                    style: const TextStyle(fontSize: 12),
                  ),
                  trailing: Container(
                    padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
                    decoration: BoxDecoration(
                      color: isHighPriority ? AppTheme.error.withOpacity(0.2) : Colors.white.withOpacity(0.05),
                      borderRadius: BorderRadius.circular(8),
                    ),
                    child: Text(
                      event.priority,
                      style: TextStyle(
                        fontSize: 11,
                        color: isHighPriority ? AppTheme.error : AppTheme.textSecondary,
                        fontWeight: FontWeight.bold,
                      ),
                    ),
                  ),
                ),
              );
            },
          ),
      ],
    );
  }

  Widget _buildBottomNavBar() {
    return BottomNavigationBar(
      type: BottomNavigationBarType.fixed,
      backgroundColor: AppTheme.bgDarkSecondary,
      selectedItemColor: AppTheme.primaryLight,
      unselectedItemColor: AppTheme.textSecondary,
      currentIndex: _currentIndex,
      onTap: (index) {
        setState(() {
          _currentIndex = index;
        });

        // Handle navigation
        switch (index) {
          case 0:
            context.go('/');
            break;
          case 1:
            context.go('/ai-features');
            break;
          case 2:
            context.go('/create-event');
            break;
          case 3:
            context.go('/notifications');
            break;
        }
      },
      items: const [
        BottomNavigationBarItem(
          icon: Icon(Icons.dashboard_rounded),
          label: 'Dashboard',
        ),
        BottomNavigationBarItem(
          icon: Icon(Icons.auto_awesome_rounded),
          label: 'AI Helper',
        ),
        BottomNavigationBarItem(
          icon: Icon(Icons.add_circle_outline),
          label: 'Add Event',
        ),
        BottomNavigationBarItem(
          icon: Icon(Icons.notifications_rounded),
          label: 'Reminders',
        ),
      ],
    );
  }
}
