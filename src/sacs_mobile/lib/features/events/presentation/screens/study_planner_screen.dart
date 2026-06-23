import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:intl/intl.dart';
import 'package:google_fonts/google_fonts.dart';

import '../state/events_provider.dart';
import '../../../../core/theme/app_theme.dart';

class StudyPlannerScreen extends ConsumerStatefulWidget {
  const StudyPlannerScreen({super.key});

  @override
  ConsumerState<StudyPlannerScreen> createState() => _StudyPlannerScreenState();
}

class _StudyPlannerScreenState extends ConsumerState<StudyPlannerScreen> {
  final _formKey = GlobalKey<FormState>();
  final _topicController = TextEditingController();
  final _durationController = TextEditingController(); // in minutes
  final _notesController = TextEditingController();
  
  int _courseId = 1;
  DateTime _sessionDateTime = DateTime.now().add(const Duration(hours: 4));

  @override
  void dispose() {
    _topicController.dispose();
    _durationController.dispose();
    _notesController.dispose();
    super.dispose();
  }

  void _submit() {
    if (_formKey.currentState!.validate()) {
      ref.read(eventsProvider.notifier).createEvent(
            title: 'Study: ${_topicController.text.trim()}',
            courseId: _courseId,
            eventType: 4, // 4 = StudySession enum
            dueDateTime: _sessionDateTime,
            priorityLevel: 'Medium',
            notes: _notesController.text.trim().isEmpty ? null : _notesController.text.trim(),
            studyTopic: _topicController.text.trim(),
            studyDuration: int.parse(_durationController.text),
          );
    }
  }

  @override
  Widget build(BuildContext context) {
    final eventState = ref.watch(eventsProvider);

    ref.listen(eventsProvider, (previous, next) {
      if (next.isSuccess) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text('Study Session scheduled successfully!'),
            backgroundColor: AppTheme.success,
          ),
        );
        context.go('/');
      }
    });

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
          child: SingleChildScrollView(
            padding: const EdgeInsets.all(24.0),
            child: Form(
              key: _formKey,
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.stretch,
                children: [
                  Row(
                    children: [
                      IconButton(
                        onPressed: () => context.go('/'),
                        icon: const Icon(Icons.arrow_back_ios_new_rounded, color: Colors.white),
                      ),
                      const SizedBox(width: 8),
                      Text(
                        'Study Session Planner',
                        style: GoogleFonts.outfit(
                          fontSize: 24,
                          fontWeight: FontWeight.bold,
                          color: Colors.white,
                        ),
                      ),
                    ],
                  ),
                  const SizedBox(height: 24),

                  Text(
                    'Plan your focused study slots and SACS will remind you before the session starts.',
                    style: Theme.of(context).textTheme.bodyMedium,
                  ),
                  const SizedBox(height: 28),

                  // Topic
                  TextFormField(
                    controller: _topicController,
                    validator: (val) => val == null || val.isEmpty ? 'Study topic is required.' : null,
                    decoration: const InputDecoration(
                      labelText: 'Study Topic (e.g. Backpropagation revisions)',
                      prefixIcon: Icon(Icons.menu_book_rounded),
                    ),
                  ),
                  const SizedBox(height: 16),

                  // Course
                  DropdownButtonFormField<int>(
                    value: _courseId,
                    decoration: const InputDecoration(
                      labelText: 'Related Course',
                      prefixIcon: Icon(Icons.class_outlined),
                    ),
                    items: const [
                      DropdownMenuItem(value: 1, child: Text('Machine Learning (CS401)')),
                      DropdownMenuItem(value: 2, child: Text('Distributed Systems (CS405)')),
                      DropdownMenuItem(value: 3, child: Text('Compiler Design (CS408)')),
                    ],
                    onChanged: (val) {
                      if (val != null) {
                        setState(() {
                          _courseId = val;
                        });
                      }
                    },
                  ),
                  const SizedBox(height: 16),

                  // Duration in minutes
                  TextFormField(
                    controller: _durationController,
                    keyboardType: TextInputType.number,
                    validator: (val) => val == null || int.tryParse(val) == null ? 'Please enter duration in minutes.' : null,
                    decoration: const InputDecoration(
                      labelText: 'Study Duration (Minutes)',
                      prefixIcon: Icon(Icons.timer_outlined),
                    ),
                  ),
                  const SizedBox(height: 16),

                  // Date Time picker
                  _buildDateTimePicker(),
                  const SizedBox(height: 16),

                  // Notes
                  TextFormField(
                    controller: _notesController,
                    maxLines: 3,
                    decoration: const InputDecoration(
                      labelText: 'Study Goals / Notes',
                      prefixIcon: Icon(Icons.edit_note_rounded),
                    ),
                  ),
                  const SizedBox(height: 36),

                  // Submit
                  ElevatedButton(
                    onPressed: eventState.isLoading ? null : _submit,
                    style: ElevatedButton.styleFrom(
                      backgroundColor: AppTheme.primaryLight,
                      foregroundColor: AppTheme.textPrimary,
                      padding: const EdgeInsets.symmetric(vertical: 16),
                      shape: RoundedRectangleBorder(
                        borderRadius: BorderRadius.circular(12),
                      ),
                    ),
                    child: eventState.isLoading
                        ? const SizedBox(
                            height: 20,
                            width: 20,
                            child: CircularProgressIndicator(strokeWidth: 2),
                          )
                        : const Text(
                            'Schedule Study Block',
                            style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold),
                          ),
                  ),
                ],
              ),
            ),
          ),
        ),
      ),
    );
  }

  Widget _buildDateTimePicker() {
    return InkWell(
      onTap: () async {
        final date = await showDatePicker(
          context: context,
          initialDate: _sessionDateTime,
          firstDate: DateTime.now(),
          lastDate: DateTime.now().add(const Duration(days: 30)),
        );
        if (date != null) {
          final time = await showTimePicker(
            context: context,
            initialTime: TimeOfDay.fromDateTime(_sessionDateTime),
          );
          if (time != null) {
            setState(() {
              _sessionDateTime = DateTime(
                date.year,
                date.month,
                date.day,
                time.hour,
                time.minute,
              );
            });
          }
        }
      },
      child: Container(
        padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 16),
        decoration: BoxDecoration(
          color: AppTheme.bgDarkSecondary,
          borderRadius: BorderRadius.circular(12),
          border: Border.all(color: Colors.white.withOpacity(0.08)),
        ),
        child: Row(
          mainAxisAlignment: MainAxisAlignment.spaceBetween,
          children: [
            Row(
              children: [
                const Icon(Icons.alarm_on_rounded, color: AppTheme.textSecondary),
                const SizedBox(width: 12),
                Text(
                  'Session Start: ${DateFormat('yyyy-MM-dd HH:mm').format(_sessionDateTime)}',
                  style: const TextStyle(color: Colors.white),
                ),
              ],
            ),
            const Icon(Icons.arrow_drop_down, color: AppTheme.textSecondary),
          ],
        ),
      ),
    );
  }
}
