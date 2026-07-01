import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:go_router/go_router.dart';
import '../providers/auth_provider.dart';
import '../providers/event_provider.dart';
import '../widgets/student_welcome_card.dart';
import '../widgets/user_profile_section.dart';
import '../widgets/upcoming_deadlines_widget.dart';
import '../widgets/calendar_preview_widget.dart';
import '../widgets/quick_actions_widget.dart';
import '../services/api_service.dart';
import '../core/theme/app_theme.dart';

class DashboardScreen extends StatefulWidget {
  const DashboardScreen({super.key});

  @override
  State<DashboardScreen> createState() => _DashboardScreenState();
}

class _DashboardScreenState extends State<DashboardScreen> {
  double? _attendancePercentage;
  bool _loadingAttendance = false;
  int _unreadNotificationsCount = 0;

  @override
  void initState() {
    super.initState();
    // Fetch events after the widget tree is built
    WidgetsBinding.instance.addPostFrameCallback((_) {
      context.read<EventProvider>().fetchEvents();
      _fetchAttendance();
      _fetchNotifications();
    });
  }

  Future<void> _fetchAttendance() async {
    final authState = context.read<AuthProvider>();
    final studentId = authState.user?.id;
    if (studentId == null) return;

    if (mounted) {
      setState(() {
        _loadingAttendance = true;
      });
    }

    try {
      final apiService = ApiService();
      final data = await apiService.getAttendanceHistory(studentId);
      if (mounted) {
        setState(() {
          _attendancePercentage = (data['attendancePercentage'] as num?)?.toDouble();
          _loadingAttendance = false;
        });
      }
    } catch (e) {
      if (mounted) {
        setState(() {
          _loadingAttendance = false;
        });
      }
    }
  }

  Future<void> _fetchNotifications() async {
    final authState = context.read<AuthProvider>();
    final studentId = authState.user?.id;
    if (studentId == null) return;

    try {
      final apiService = ApiService();
      final data = await apiService.getUserNotifications(studentId);
      if (mounted) {
        setState(() {
          _unreadNotificationsCount = (data['unreadCount'] as num?)?.toInt() ?? 0;
        });
      }
    } catch (e) {
      // Ignored
    }
  }

  @override
  Widget build(BuildContext context) {
    final authState = context.watch<AuthProvider>();
    final eventState = context.watch<EventProvider>();
    final currentUser = authState.user;

    return Scaffold(
      floatingActionButton: FloatingActionButton(
        onPressed: () => context.push('/create-event'),
        backgroundColor: AppTheme.primaryLight,
        child: const Icon(Icons.add_rounded, color: Colors.white, size: 28),
      ),
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
            onRefresh: () => Future.wait([
              context.read<EventProvider>().fetchEvents(),
              _fetchAttendance(),
              _fetchNotifications(),
            ]),
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
                      Row(
                        children: [
                          Stack(
                            children: [
                              IconButton(
                                icon: const Icon(Icons.notifications_none_rounded, color: AppTheme.primaryLight, size: 28),
                                onPressed: () => context.push('/notifications').then((_) => _fetchNotifications()),
                                tooltip: 'Notifications',
                              ),
                              if (_unreadNotificationsCount > 0)
                                Positioned(
                                  right: 6,
                                  top: 6,
                                  child: Container(
                                    padding: const EdgeInsets.all(4),
                                    decoration: const BoxDecoration(
                                      color: AppTheme.error,
                                      shape: BoxShape.circle,
                                    ),
                                    constraints: const BoxConstraints(
                                      minWidth: 16,
                                      minHeight: 16,
                                    ),
                                    child: Text(
                                      '$_unreadNotificationsCount',
                                      style: GoogleFonts.inter(
                                        color: Colors.white,
                                        fontSize: 9,
                                        fontWeight: FontWeight.bold,
                                      ),
                                      textAlign: TextAlign.center,
                                    ),
                                  ),
                                ),
                            ],
                          ),
                          IconButton(
                            onPressed: () {
                              context.read<EventProvider>().fetchEvents();
                              _fetchAttendance();
                              _fetchNotifications();
                            },
                            icon: const Icon(Icons.sync_rounded, color: AppTheme.primaryLight),
                            tooltip: 'Sync Data',
                          ),
                        ],
                      ),
                    ],
                  ),
                  const SizedBox(height: 20),

                  // Student Welcome Card
                  if (currentUser != null) ...[
                    StudentWelcomeCard(user: currentUser),
                    const SizedBox(height: 20),
                  ],

                  // Attendance Card Display
                  if (_attendancePercentage != null) ...[
                    _buildAttendanceCard(_attendancePercentage!),
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

  Widget _buildAttendanceCard(double percentage) {
    final isGood = percentage >= 75.0;
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 16),
      decoration: BoxDecoration(
        color: AppTheme.bgDarkSecondary,
        borderRadius: BorderRadius.circular(20),
        border: Border.all(
          color: Colors.white.withOpacity(0.06),
        ),
      ),
      child: Row(
        children: [
          Container(
            padding: const EdgeInsets.all(10),
            decoration: BoxDecoration(
              color: (isGood ? AppTheme.success : AppTheme.error).withOpacity(0.12),
              shape: BoxShape.circle,
            ),
            child: Icon(
              isGood ? Icons.done_all_rounded : Icons.warning_amber_rounded,
              color: isGood ? AppTheme.success : AppTheme.error,
              size: 20,
            ),
          ),
          const SizedBox(width: 14),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  'Average Attendance',
                  style: GoogleFonts.outfit(
                    color: AppTheme.textPrimary,
                    fontSize: 14,
                    fontWeight: FontWeight.bold,
                  ),
                ),
                const SizedBox(height: 4),
                ClipRRect(
                  borderRadius: BorderRadius.circular(4),
                  child: LinearProgressIndicator(
                    value: percentage / 100.0,
                    backgroundColor: Colors.white10,
                    valueColor: AlwaysStoppedAnimation<Color>(
                      isGood ? AppTheme.success : AppTheme.error,
                    ),
                    minHeight: 6,
                  ),
                ),
              ],
            ),
          ),
          const SizedBox(width: 16),
          Column(
            crossAxisAlignment: CrossAxisAlignment.end,
            children: [
              Text(
                '${percentage.toStringAsFixed(0)}%',
                style: GoogleFonts.outfit(
                  color: isGood ? AppTheme.success : AppTheme.error,
                  fontSize: 20,
                  fontWeight: FontWeight.bold,
                ),
              ),
              Text(
                isGood ? 'Good' : 'Low',
                style: GoogleFonts.inter(
                  color: AppTheme.textSecondary,
                  fontSize: 10,
                ),
              ),
            ],
          ),
        ],
      ),
    );
  }
}
