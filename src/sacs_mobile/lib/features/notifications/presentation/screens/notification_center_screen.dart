import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:intl/intl.dart';
import 'package:google_fonts/google_fonts.dart';

import '../state/notifications_provider.dart';
import '../../../../core/theme/app_theme.dart';

class NotificationCenterScreen extends ConsumerWidget {
  const NotificationCenterScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final state = ref.watch(notificationsProvider);

    return Scaffold(
      body: Container(
        decoration: const BoxDecoration(
          gradient: LinearGradient(
            begin: Alignment.topCenter,
            end: Alignment.bottomCenter,
            colors: [AppTheme.bgDark, AppTheme.bgDarkSecondary],
          ),
        ),
        child: SafeArea(
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              // Header
              Padding(
                padding: const EdgeInsets.symmetric(horizontal: 24.0, vertical: 16.0),
                child: Row(
                  children: [
                    IconButton(
                      onPressed: () => context.go('/'),
                      icon: const Icon(Icons.arrow_back_ios_new_rounded, color: Colors.white),
                    ),
                    const SizedBox(width: 8),
                    Text(
                      'Reminders & Alerts',
                      style: GoogleFonts.outfit(
                        fontSize: 24,
                        fontWeight: FontWeight.bold,
                        color: Colors.white,
                      ),
                    ),
                  ],
                ),
              ),

              // Content list
              Expanded(
                child: RefreshIndicator(
                  onRefresh: () => ref.read(notificationsProvider.notifier).refreshReminders(),
                  color: AppTheme.primaryLight,
                  child: state.isLoading
                      ? const Center(child: CircularProgressIndicator())
                      : state.reminders.isEmpty
                          ? const Center(
                              child: Text(
                                'No scheduled reminders.',
                                style: TextStyle(color: AppTheme.textSecondary),
                              ),
                            )
                          : ListView.builder(
                              padding: const EdgeInsets.symmetric(horizontal: 24.0, vertical: 12.0),
                              itemCount: state.reminders.length,
                              itemBuilder: (context, index) {
                                final reminder = state.reminders[index];
                                final isPending = reminder.status.toLowerCase() == 'pending';

                                return Card(
                                  margin: const EdgeInsets.only(bottom: 12),
                                  child: ListTile(
                                    leading: CircleAvatar(
                                      backgroundColor: isPending ? AppTheme.accent.withOpacity(0.15) : AppTheme.success.withOpacity(0.15),
                                      child: Icon(
                                        isPending ? Icons.notifications_active_rounded : Icons.check_circle_rounded,
                                        color: isPending ? AppTheme.accent : AppTheme.success,
                                      ),
                                    ),
                                    title: Text(
                                      reminder.eventTitle,
                                      style: const TextStyle(fontWeight: FontWeight.bold),
                                    ),
                                    subtitle: Text(
                                      'Alert at ${DateFormat('MMM d, h:mm a').format(reminder.scheduledTime)} (${reminder.reminderType})',
                                      style: const TextStyle(fontSize: 12),
                                    ),
                                    trailing: IconButton(
                                      icon: const Icon(Icons.delete_outline_rounded, color: AppTheme.error),
                                      onPressed: () {
                                        ref.read(notificationsProvider.notifier).deleteReminder(reminder.id);
                                        ScaffoldMessenger.of(context).showSnackBar(
                                          const SnackBar(content: Text('Reminder deleted.')),
                                        );
                                      },
                                    ),
                                  ),
                                );
                              },
                            ),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
