import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:provider/provider.dart';
import 'package:go_router/go_router.dart';
import '../providers/event_provider.dart';
import '../models/event_model.dart';
import '../core/theme/app_theme.dart';

class EventDetailScreen extends StatelessWidget {
  final EventModel event;

  const EventDetailScreen({super.key, required this.event});

  Color _getPriorityColor(String priority) {
    switch (priority.toLowerCase()) {
      case 'low':
        return AppTheme.textSecondary;
      case 'medium':
        return AppTheme.primaryLight;
      case 'high':
        return AppTheme.accent;
      case 'critical':
        return AppTheme.error;
      default:
        return AppTheme.textSecondary;
    }
  }

  Future<void> _confirmDelete(BuildContext context) async {
    final confirmed = await showDialog<bool>(
      context: context,
      builder: (context) {
        return AlertDialog(
          backgroundColor: AppTheme.bgDarkSecondary,
          title: Text(
            'Delete Event',
            style: GoogleFonts.outfit(color: AppTheme.textPrimary, fontWeight: FontWeight.bold),
          ),
          content: Text(
            'Are you sure you want to delete "${event.title}"? This action cannot be undone.',
            style: GoogleFonts.inter(color: AppTheme.textSecondary),
          ),
          actions: [
            TextButton(
              onPressed: () => Navigator.of(context).pop(false),
              child: const Text('Cancel', style: TextStyle(color: AppTheme.textSecondary)),
            ),
            TextButton(
              onPressed: () => Navigator.of(context).pop(true),
              child: const Text('Delete', style: TextStyle(color: AppTheme.error)),
            ),
          ],
        );
      },
    );

    if (confirmed == true) {
      if (!context.mounted) return;
      final provider = context.read<EventProvider>();
      try {
        await provider.deleteEvent(event.id);
        if (!context.mounted) return;
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Academic event deleted successfully.')),
        );
        context.pop();
      } catch (e) {
        if (!context.mounted) return;
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text(provider.errorMessage ?? 'Deletion failed: $e'),
            backgroundColor: AppTheme.error,
          ),
        );
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    final priorityColor = _getPriorityColor(event.priorityLevel);

    return Scaffold(
      appBar: AppBar(
        title: Text(
          'Event Details',
          style: GoogleFonts.outfit(fontWeight: FontWeight.bold),
        ),
        backgroundColor: AppTheme.bgDark,
        elevation: 0,
        actions: [
          IconButton(
            icon: const Icon(Icons.edit_rounded, color: AppTheme.primaryLight),
            onPressed: () => context.push('/edit-event', extra: event),
            tooltip: 'Edit Event',
          ),
          IconButton(
            icon: const Icon(Icons.delete_forever_rounded, color: AppTheme.error),
            onPressed: () => _confirmDelete(context),
            tooltip: 'Delete Event',
          ),
        ],
      ),
      body: Container(
        height: double.infinity,
        decoration: const BoxDecoration(
          gradient: LinearGradient(
            begin: Alignment.topCenter,
            end: Alignment.bottomCenter,
            colors: [AppTheme.bgDark, AppTheme.bgDarkSecondary],
          ),
        ),
        child: SafeArea(
          child: SingleChildScrollView(
            padding: const EdgeInsets.all(24.0),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                // Event Type Tag & Priority Tag
                Row(
                  children: [
                    Container(
                      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
                      decoration: BoxDecoration(
                        color: AppTheme.primaryLight.withOpacity(0.12),
                        borderRadius: BorderRadius.circular(20),
                      ),
                      child: Text(
                        event.eventType,
                        style: GoogleFonts.outfit(
                          color: AppTheme.primaryLight,
                          fontSize: 12,
                          fontWeight: FontWeight.bold,
                        ),
                      ),
                    ),
                    const SizedBox(width: 10),
                    Container(
                      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
                      decoration: BoxDecoration(
                        color: priorityColor.withOpacity(0.12),
                        borderRadius: BorderRadius.circular(20),
                      ),
                      child: Text(
                        '${event.priorityLevel} Priority',
                        style: GoogleFonts.outfit(
                          color: priorityColor,
                          fontSize: 12,
                          fontWeight: FontWeight.bold,
                        ),
                      ),
                    ),
                  ],
                ),
                const SizedBox(height: 20),

                // Event Title
                Text(
                  event.title,
                  style: GoogleFonts.outfit(
                    color: AppTheme.textPrimary,
                    fontSize: 24,
                    fontWeight: FontWeight.bold,
                  ),
                ),
                const SizedBox(height: 8),

                // Course Code
                Text(
                  'Course: ${event.courseCode}',
                  style: GoogleFonts.inter(
                    color: AppTheme.textSecondary,
                    fontSize: 15,
                    fontWeight: FontWeight.w600,
                  ),
                ),
                const Divider(height: 40, color: Colors.white12),

                // Due Date Section
                _buildInfoSection(
                  icon: Icons.calendar_today_rounded,
                  title: 'Due Date & Time',
                  content: event.dueDateTime.toString().substring(0, 16),
                ),
                const SizedBox(height: 24),

                // Description Section
                if (event.description != null && event.description!.trim().isNotEmpty) ...[
                  _buildSectionHeader('Description'),
                  const SizedBox(height: 8),
                  Text(
                    event.description!,
                    style: GoogleFonts.inter(color: AppTheme.textPrimary, height: 1.5),
                  ),
                  const SizedBox(height: 24),
                ],

                // Dynamic Metadata Details depending on Type
                if (event.eventType == 'Quiz' || event.eventType == 'Exam') ...[
                  _buildSectionHeader('Exam details'),
                  const SizedBox(height: 12),
                  if (event.durationMinutes != null)
                    _buildMetaRow('Duration', '${event.durationMinutes} Minutes'),
                  if (event.venue != null && event.venue!.isNotEmpty)
                    _buildMetaRow('Venue', event.venue!),
                  if (event.seatNumber != null && event.seatNumber!.isNotEmpty)
                    _buildMetaRow('Seat Number', event.seatNumber!),
                  const SizedBox(height: 24),
                ],

                if (event.eventType == 'Project') ...[
                  _buildSectionHeader('Project scope'),
                  const SizedBox(height: 12),
                  if (event.supervisorName != null && event.supervisorName!.isNotEmpty)
                    _buildMetaRow('Supervisor', event.supervisorName!),
                  if (event.progressPercentage != null) ...[
                    const SizedBox(height: 8),
                    Text(
                      'Completion Progress',
                      style: GoogleFonts.outfit(color: AppTheme.textSecondary, fontSize: 13),
                    ),
                    const SizedBox(height: 6),
                    Row(
                      children: [
                        Expanded(
                          child: ClipRRect(
                            borderRadius: BorderRadius.circular(4),
                            child: LinearProgressIndicator(
                              value: event.progressPercentage! / 100,
                              minHeight: 8,
                              backgroundColor: Colors.white12,
                              valueColor: const AlwaysStoppedAnimation(AppTheme.success),
                            ),
                          ),
                        ),
                        const SizedBox(width: 12),
                        Text(
                          '${event.progressPercentage}%',
                          style: GoogleFonts.outfit(color: AppTheme.success, fontWeight: FontWeight.bold),
                        ),
                      ],
                    ),
                  ],
                  const SizedBox(height: 24),
                ],

                if (event.eventType == 'StudySession') ...[
                  _buildSectionHeader('Study scope'),
                  const SizedBox(height: 12),
                  if (event.studyTopic != null && event.studyTopic!.isNotEmpty)
                    _buildMetaRow('Topic', event.studyTopic!),
                  if (event.studyDuration != null)
                    _buildMetaRow('Duration', '${event.studyDuration} Minutes'),
                  const SizedBox(height: 24),
                ],

                // Private study notes
                if (event.notes != null && event.notes!.trim().isNotEmpty) ...[
                  _buildSectionHeader('Private Notes'),
                  const SizedBox(height: 8),
                  Container(
                    width: double.infinity,
                    padding: const EdgeInsets.all(16),
                    decoration: BoxDecoration(
                      color: AppTheme.bgDarkSecondary,
                      borderRadius: BorderRadius.circular(12),
                      border: Border.all(color: Colors.white.withOpacity(0.04)),
                    ),
                    child: Text(
                      event.notes!,
                      style: GoogleFonts.inter(color: AppTheme.textPrimary, fontStyle: FontStyle.italic),
                    ),
                  ),
                  const SizedBox(height: 24),
                ],

                // Attachment Link
                if (event.attachmentUrl != null && event.attachmentUrl!.trim().isNotEmpty) ...[
                  _buildSectionHeader('Attachment'),
                  const SizedBox(height: 8),
                  InkWell(
                    onTap: () {
                      // Simulating opening attachment link
                      ScaffoldMessenger.of(context).showSnackBar(
                        SnackBar(content: Text('Opening attachment: ${event.attachmentUrl}')),
                      );
                    },
                    borderRadius: BorderRadius.circular(12),
                    child: Container(
                      padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 14),
                      decoration: BoxDecoration(
                        color: AppTheme.bgDarkSecondary,
                        borderRadius: BorderRadius.circular(12),
                        border: Border.all(color: Colors.white.withOpacity(0.08)),
                      ),
                      child: Row(
                        children: [
                          const Icon(Icons.attachment_rounded, color: AppTheme.primaryLight),
                          const SizedBox(width: 12),
                          Expanded(
                            child: Text(
                              event.attachmentUrl!,
                              style: const TextStyle(
                                color: AppTheme.primaryLight,
                                decoration: TextDecoration.underline,
                              ),
                              maxLines: 1,
                              overflow: TextOverflow.ellipsis,
                            ),
                          ),
                        ],
                      ),
                    ),
                  ),
                ],
              ],
            ),
          ),
        ),
      ),
    );
  }

  Widget _buildSectionHeader(String title) {
    return Text(
      title,
      style: GoogleFonts.outfit(
        color: AppTheme.textSecondary,
        fontSize: 14,
        fontWeight: FontWeight.bold,
        letterSpacing: 0.5,
      ),
    );
  }

  Widget _buildInfoSection({required IconData icon, required String title, required String content}) {
    return Row(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Container(
          padding: const EdgeInsets.all(10),
          decoration: BoxDecoration(
            color: AppTheme.primaryLight.withOpacity(0.1),
            borderRadius: BorderRadius.circular(10),
          ),
          child: Icon(icon, color: AppTheme.primaryLight, size: 20),
        ),
        const SizedBox(width: 14),
        Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              title,
              style: GoogleFonts.outfit(
                color: AppTheme.textSecondary,
                fontSize: 12,
                fontWeight: FontWeight.w500,
              ),
            ),
            const SizedBox(height: 4),
            Text(
              content,
              style: GoogleFonts.inter(
                color: AppTheme.textPrimary,
                fontSize: 15,
                fontWeight: FontWeight.bold,
              ),
            ),
          ],
        ),
      ],
    );
  }

  Widget _buildMetaRow(String label, String value) {
    return Padding(
      padding: const EdgeInsets.only(bottom: 8.0),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceBetween,
        children: [
          Text(label, style: GoogleFonts.inter(color: AppTheme.textSecondary, fontSize: 14)),
          Text(value, style: GoogleFonts.inter(color: AppTheme.textPrimary, fontSize: 14, fontWeight: FontWeight.w600)),
        ],
      ),
    );
  }
}
