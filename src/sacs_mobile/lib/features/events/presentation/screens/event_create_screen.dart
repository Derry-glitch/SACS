import 'dart:io';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:intl/intl.dart';
import 'package:file_picker/file_picker.dart';
import 'package:google_fonts/google_fonts.dart';

import '../state/events_provider.dart';
import '../../../../core/theme/app_theme.dart';

class EventCreateScreen extends ConsumerStatefulWidget {
  const EventCreateScreen({super.key});

  @override
  ConsumerState<EventCreateScreen> createState() => _EventCreateScreenState();
}

class _EventCreateScreenState extends ConsumerState<EventCreateScreen> {
  final _formKey = GlobalKey<FormState>();
  
  // Shared Fields
  final _titleController = TextEditingController();
  final _descController = TextEditingController();
  final _notesController = TextEditingController();
  
  int _courseId = 1; // Default/Mock Course Offering ID
  int _selectedTypeIndex = 0; // 0=Assignment, 1=Quiz, 2=Exam, 3=Project
  DateTime _selectedDateTime = DateTime.now().add(const Duration(days: 2));
  String _selectedPriority = 'Medium';
  String? _uploadedAttachmentUrl;

  // Type-specific Fields
  final _durationController = TextEditingController();
  final _venueController = TextEditingController();
  final _seatNumberController = TextEditingController();
  final _supervisorController = TextEditingController();
  double _progressPercentage = 0.0;

  @override
  void dispose() {
    _titleController.dispose();
    _descController.dispose();
    _notesController.dispose();
    _durationController.dispose();
    _venueController.dispose();
    _seatNumberController.dispose();
    _supervisorController.dispose();
    super.dispose();
  }

  Future<void> _pickFile() async {
    final result = await FilePicker.platform.pickFiles();
    if (result != null && result.files.single.path != null) {
      final file = File(result.files.single.path!);
      final url = await ref.read(eventsProvider.notifier).uploadFile(file);
      if (url != null) {
        setState(() {
          _uploadedAttachmentUrl = url;
        });
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Attachment uploaded successfully!')),
        );
      }
    }
  }

  void _submit() {
    if (_formKey.currentState!.validate()) {
      ref.read(eventsProvider.notifier).createEvent(
            title: _titleController.text.trim(),
            description: _descController.text.trim().isEmpty ? null : _descController.text.trim(),
            courseId: _courseId,
            eventType: _selectedTypeIndex,
            dueDateTime: _selectedDateTime,
            priorityLevel: _selectedPriority,
            notes: _notesController.text.trim().isEmpty ? null : _notesController.text.trim(),
            attachmentUrl: _uploadedAttachmentUrl,
            durationMinutes: _durationController.text.isNotEmpty ? int.parse(_durationController.text) : null,
            venue: _venueController.text.isNotEmpty ? _venueController.text.trim() : null,
            seatNumber: _seatNumberController.text.isNotEmpty ? _seatNumberController.text.trim() : null,
            supervisorName: _supervisorController.text.isNotEmpty ? _supervisorController.text.trim() : null,
            progressPercentage: _selectedTypeIndex == 3 ? _progressPercentage.toInt() : null,
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
            content: Text('Academic Event created successfully!'),
            backgroundColor: AppTheme.success,
          ),
        );
        context.go('/');
      }
      if (next.errorMessage != null) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text(next.errorMessage!),
            backgroundColor: AppTheme.error,
          ),
        );
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
                  // App bar back button & title
                  Row(
                    children: [
                      IconButton(
                        onPressed: () => context.go('/'),
                        icon: const Icon(Icons.arrow_back_ios_new_rounded, color: Colors.white),
                      ),
                      const SizedBox(width: 8),
                      Text(
                        'Create Event',
                        style: GoogleFonts.outfit(
                          fontSize: 24,
                          fontWeight: FontWeight.bold,
                          color: Colors.white,
                        ),
                      ),
                    ],
                  ),
                  const SizedBox(height: 24),

                  // Toggle buttons for Event Type Selection
                  _buildTypeSelector(),
                  const SizedBox(height: 28),

                  // Title Field
                  TextFormField(
                    controller: _titleController,
                    validator: (val) => val == null || val.isEmpty ? 'Title is required.' : null,
                    decoration: const InputDecoration(
                      labelText: 'Event Title',
                      prefixIcon: Icon(Icons.title_rounded),
                    ),
                  ),
                  const SizedBox(height: 16),

                  // Course Offering ID (Mocked as Dropdown)
                  DropdownButtonFormField<int>(
                    value: _courseId,
                    decoration: const InputDecoration(
                      labelText: 'Select Course',
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

                  // Due Date and Time Picker
                  _buildDateTimePicker(),
                  const SizedBox(height: 16),

                  // Priority Selector
                  DropdownButtonFormField<String>(
                    value: _selectedPriority,
                    decoration: const InputDecoration(
                      labelText: 'Priority Level',
                      prefixIcon: Icon(Icons.flag_outlined),
                    ),
                    items: const ['Low', 'Medium', 'High', 'Critical']
                        .map((lvl) => DropdownMenuItem(value: lvl, child: Text(lvl)))
                        .toList(),
                    onChanged: (val) {
                      if (val != null) {
                        setState(() {
                          _selectedPriority = val;
                        });
                      }
                    },
                  ),
                  const SizedBox(height: 16),

                  // Dynamic Fields
                  if (_selectedTypeIndex == 0) _buildAssignmentFields(),
                  if (_selectedTypeIndex == 1) _buildQuizFields(),
                  if (_selectedTypeIndex == 2) _buildExamFields(),
                  if (_selectedTypeIndex == 3) _buildProjectFields(),

                  // Extra Notes
                  const SizedBox(height: 16),
                  TextFormField(
                    controller: _notesController,
                    maxLines: 2,
                    decoration: const InputDecoration(
                      labelText: 'Extra Notes',
                      prefixIcon: Icon(Icons.note_alt_outlined),
                    ),
                  ),
                  const SizedBox(height: 36),

                  // Submit Button
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
                            'Save Academic Event',
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

  Widget _buildTypeSelector() {
    final types = ['Assignment', 'Quiz', 'Exam', 'Project'];
    return ToggleButtons(
      isSelected: List.generate(4, (i) => i == _selectedTypeIndex),
      onPressed: (index) {
        setState(() {
          _selectedTypeIndex = index;
        });
      },
      borderRadius: BorderRadius.circular(12),
      selectedBorderColor: AppTheme.accent,
      selectedColor: Colors.white,
      fillColor: AppTheme.primaryLight.withOpacity(0.3),
      color: AppTheme.textSecondary,
      constraints: const BoxConstraints(
        minHeight: 40,
        minWidth: 80,
      ),
      children: types.map((type) => Padding(
        padding: const EdgeInsets.symmetric(horizontal: 8.0),
        child: Text(type, style: const TextStyle(fontSize: 12, fontWeight: FontWeight.bold)),
      )).toList(),
    );
  }

  Widget _buildDateTimePicker() {
    return InkWell(
      onTap: () async {
        final date = await showDatePicker(
          context: context,
          initialDate: _selectedDateTime,
          firstDate: DateTime.now(),
          lastDate: DateTime.now().add(const Duration(days: 365)),
        );
        if (date != null) {
          final time = await showTimePicker(
            context: context,
            initialTime: TimeOfDay.fromDateTime(_selectedDateTime),
          );
          if (time != null) {
            setState(() {
              _selectedDateTime = DateTime(
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
                const Icon(Icons.calendar_month_outlined, color: AppTheme.textSecondary),
                const SizedBox(width: 12),
                Text(
                  'Due Date: ${DateFormat('yyyy-MM-dd HH:mm').format(_selectedDateTime)}',
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

  Widget _buildAssignmentFields() {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        const SizedBox(height: 16),
        TextFormField(
          controller: _descController,
          maxLines: 3,
          decoration: const InputDecoration(
            labelText: 'Assignment Description',
            prefixIcon: Icon(Icons.description_outlined),
          ),
        ),
        const SizedBox(height: 16),
        OutlinedButton.icon(
          onPressed: _pickFile,
          icon: const Icon(Icons.upload_file_rounded),
          label: Text(_uploadedAttachmentUrl != null ? 'Change Attachment' : 'Upload Assignment Doc'),
          style: OutlinedButton.styleFrom(
            padding: const EdgeInsets.symmetric(vertical: 14),
            shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
          ),
        ),
      ],
    );
  }

  Widget _buildQuizFields() {
    return Column(
      children: [
        const SizedBox(height: 16),
        TextFormField(
          controller: _durationController,
          keyboardType: TextInputType.number,
          validator: (val) => val == null || val.isEmpty ? 'Duration is required.' : null,
          decoration: const InputDecoration(
            labelText: 'Duration (Minutes)',
            prefixIcon: Icon(Icons.timer_outlined),
          ),
        ),
      ],
    );
  }

  Widget _buildExamFields() {
    return Column(
      children: [
        const SizedBox(height: 16),
        TextFormField(
          controller: _durationController,
          keyboardType: TextInputType.number,
          validator: (val) => val == null || val.isEmpty ? 'Duration is required.' : null,
          decoration: const InputDecoration(
            labelText: 'Duration (Minutes)',
            prefixIcon: Icon(Icons.timer_outlined),
          ),
        ),
        const SizedBox(height: 16),
        TextFormField(
          controller: _venueController,
          validator: (val) => val == null || val.isEmpty ? 'Venue is required for exams.' : null,
          decoration: const InputDecoration(
            labelText: 'Exam Venue / Hall',
            prefixIcon: Icon(Icons.place_outlined),
          ),
        ),
        const SizedBox(height: 16),
        TextFormField(
          controller: _seatNumberController,
          decoration: const InputDecoration(
            labelText: 'Seat Number (Optional)',
            prefixIcon: Icon(Icons.chair_outlined),
          ),
        ),
      ],
    );
  }

  Widget _buildProjectFields() {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        const SizedBox(height: 16),
        TextFormField(
          controller: _descController,
          maxLines: 2,
          decoration: const InputDecoration(
            labelText: 'Project Goal / Details',
            prefixIcon: Icon(Icons.description_outlined),
          ),
        ),
        const SizedBox(height: 16),
        TextFormField(
          controller: _supervisorController,
          validator: (val) => val == null || val.isEmpty ? 'Supervisor name is required.' : null,
          decoration: const InputDecoration(
            labelText: 'Supervisor Name',
            prefixIcon: Icon(Icons.supervisor_account_outlined),
          ),
        ),
        const SizedBox(height: 16),
        Text(
          'Project Completion Progress: ${_progressPercentage.toInt()}%',
          style: const TextStyle(fontWeight: FontWeight.bold),
        ),
        Slider(
          value: _progressPercentage,
          min: 0,
          max: 100,
          divisions: 20,
          label: '${_progressPercentage.toInt()}%',
          activeColor: AppTheme.accent,
          onChanged: (val) {
            setState(() {
              _progressPercentage = val;
            });
          },
        ),
      ],
    );
  }
}
