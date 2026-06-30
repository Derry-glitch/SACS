import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:go_router/go_router.dart';
import '../models/event_model.dart';
import '../core/theme/app_theme.dart';

class UpcomingDeadlinesWidget extends StatelessWidget {
  final List<EventModel> events;
  final bool isLoading;

  const UpcomingDeadlinesWidget({
    super.key,
    required this.events,
    required this.isLoading,
  });

  @override
  Widget build(BuildContext context) {
    if (isLoading) {
      return const Center(
        child: Padding(
          padding: EdgeInsets.all(24.0),
          child: CircularProgressIndicator(),
        ),
      );
    }

    if (events.isEmpty) {
      return Card(
        child: Padding(
          padding: const EdgeInsets.all(24.0),
          child: Center(
            child: Text(
              'No upcoming deadlines. Hooray!',
              style: GoogleFonts.inter(
                fontSize: 14,
                color: AppTheme.textSecondary,
              ),
            ),
          ),
        ),
      );
    }

    // Filter upcoming events (today or in future) and sort them by due date
    final now = DateTime.now();
    final upcomingEvents = events
        .where((e) => e.dueDateTime.isAfter(now.subtract(const Duration(days: 1))))
        .toList()
      ..sort((a, b) => a.dueDateTime.compareTo(b.dueDateTime));

    if (upcomingEvents.isEmpty) {
      return Card(
        child: Padding(
          padding: const EdgeInsets.all(24.0),
          child: Center(
            child: Text(
              'No upcoming deadlines. Hooray!',
              style: GoogleFonts.inter(
                fontSize: 14,
                color: AppTheme.textSecondary,
              ),
            ),
          ),
        ),
      );
    }

    return ListView.builder(
      shrinkWrap: true,
      physics: const NeverScrollableScrollPhysics(),
      itemCount: upcomingEvents.length,
      itemBuilder: (context, index) {
        final event = upcomingEvents[index];
        final isHighPriority = event.priorityLevel.toLowerCase() == 'high' ||
            event.priorityLevel.toLowerCase() == 'critical';

        return Card(
          margin: const EdgeInsets.only(bottom: 12),
          child: ListTile(
            onTap: () => context.push('/event-details', extra: event),
            contentPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
            leading: CircleAvatar(
              backgroundColor: isHighPriority
                  ? AppTheme.error.withOpacity(0.15)
                  : AppTheme.primaryLight.withOpacity(0.15),
              child: Icon(
                _getEventIcon(event.eventType),
                color: isHighPriority ? AppTheme.error : AppTheme.primaryLight,
              ),
            ),
            title: Text(
              event.title,
              style: GoogleFonts.outfit(
                fontSize: 15,
                fontWeight: FontWeight.bold,
                color: AppTheme.textPrimary,
              ),
            ),
            subtitle: Padding(
              padding: const EdgeInsets.only(top: 4.0),
              child: Text(
                '${event.courseCode} • Due ${DateFormat('MMM d, y • h:mm a').format(event.dueDateTime)}',
                style: GoogleFonts.inter(
                  fontSize: 12,
                  color: AppTheme.textSecondary,
                ),
              ),
            ),
            trailing: Container(
              padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
              decoration: BoxDecoration(
                color: isHighPriority
                    ? AppTheme.error.withOpacity(0.2)
                    : AppTheme.bgDark,
                borderRadius: BorderRadius.circular(8),
                border: Border.all(
                  color: isHighPriority
                      ? AppTheme.error.withOpacity(0.4)
                      : Colors.white.withOpacity(0.08),
                ),
              ),
              child: Text(
                event.priorityLevel.toUpperCase(),
                style: GoogleFonts.inter(
                  fontSize: 10,
                  color: isHighPriority ? AppTheme.error : AppTheme.textSecondary,
                  fontWeight: FontWeight.bold,
                ),
              ),
            ),
          ),
        );
      },
    );
  }

  IconData _getEventIcon(String eventType) {
    switch (eventType.toLowerCase()) {
      case 'assignment':
        return Icons.assignment_outlined;
      case 'quiz':
        return Icons.quiz_outlined;
      case 'exam':
        return Icons.menu_book_outlined;
      case 'project':
        return Icons.folder_shared_outlined;
      case 'studysession':
        return Icons.alarm_on_outlined;
      default:
        return Icons.alarm;
    }
  }
}
