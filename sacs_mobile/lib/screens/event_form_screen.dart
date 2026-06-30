import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:provider/provider.dart';
import 'package:go_router/go_router.dart';
import '../providers/event_provider.dart';
import '../models/event_model.dart';
import '../core/theme/app_theme.dart';

class EventFormScreen extends StatefulWidget {
  final EventModel? event;

  const EventFormScreen({super.key, this.event});

  @override
  State<EventFormScreen> createState() => _EventFormScreenState();
}

class _EventFormScreenState extends State<EventFormScreen> {
  final _formKey = GlobalKey<FormState>();

  late bool _isEditMode;
  
  // Form fields
  late String _title;
  late String? _description;
  late int _courseId;
  late String _eventType; // Assignment, Quiz, Exam, Project, StudySession
  late DateTime _dueDateTime;
  late String _priorityLevel; // Low, Medium, High, Critical
  late String? _notes;
  late String? _attachmentUrl;

  // Type-specific fields
  int? _durationMinutes;
  String? _venue;
  String? _seatNumber;
  String? _supervisorName;
  int? _progressPercentage;
  String? _studyTopic;
  int? _studyDuration;

  // Hardcoded courses mapped to seeded semester offering IDs
  final List<Map<String, dynamic>> _courses = [
    {'id': 1, 'code': 'CSC201', 'title': 'Java Programming'},
    {'id': 2, 'code': 'CSC202', 'title': 'Data Structures'},
    {'id': 3, 'code': 'CSC301', 'title': 'Operating Systems'},
  ];

  final List<String> _eventTypes = [
    'Assignment',
    'Quiz',
    'Exam',
    'Project',
    'StudySession',
  ];

  final List<String> _priorities = [
    'Low',
    'Medium',
    'High',
    'Critical',
  ];

  @override
  void initState() {
    super.initState();
    _isEditMode = widget.event != null;

    if (_isEditMode) {
      final e = widget.event!;
      _title = e.title;
      _description = e.description;
      _courseId = e.courseId;
      _eventType = e.eventType;
      _dueDateTime = e.dueDateTime;
      _priorityLevel = e.priorityLevel;
      _notes = e.notes;
      _attachmentUrl = e.attachmentUrl;
      _durationMinutes = e.durationMinutes;
      _venue = e.venue;
      _seatNumber = e.seatNumber;
      _supervisorName = e.supervisorName;
      _progressPercentage = e.progressPercentage;
      _studyTopic = e.studyTopic;
      _studyDuration = e.studyDuration;
    } else {
      _title = '';
      _description = '';
      _courseId = 1;
      _eventType = 'Assignment';
      _dueDateTime = DateTime.now().add(const Duration(days: 1, hours: 2));
      _priorityLevel = 'Medium';
      _notes = '';
      _attachmentUrl = '';
      _durationMinutes = 60;
      _venue = '';
      _seatNumber = '';
      _supervisorName = '';
      _progressPercentage = 0;
      _studyTopic = '';
      _studyDuration = 60;
    }
  }

  // Type-specific field mapping to index for backend
  int _getEventTypeIndex(String typeName) {
    switch (typeName) {
      case 'Assignment': return 0;
      case 'Quiz': return 1;
      case 'Exam': return 2;
      case 'Project': return 3;
      case 'StudySession': return 4;
      default: return 0;
    }
  }

  Future<void> _selectDateTime(BuildContext context) async {
    final pickedDate = await showDatePicker(
      context: context,
      initialDate: _dueDateTime,
      firstDate: DateTime.now(),
      lastDate: DateTime.now().add(const Duration(days: 365)),
      builder: (context, child) {
        return Theme(
          data: AppTheme.darkTheme.copyWith(
            colorScheme: ColorScheme.dark(
              primary: AppTheme.primaryLight,
              surface: AppTheme.bgDarkSecondary,
              onSurface: AppTheme.textPrimary,
            ),
          ),
          child: child!,
        );
      },
    );

    if (pickedDate != null) {
      if (!context.mounted) return;
      final pickedTime = await showTimePicker(
        context: context,
        initialTime: TimeOfDay.fromDateTime(_dueDateTime),
        builder: (context, child) {
          return Theme(
            data: AppTheme.darkTheme.copyWith(
              colorScheme: ColorScheme.dark(
                primary: AppTheme.primaryLight,
                surface: AppTheme.bgDarkSecondary,
                onSurface: AppTheme.textPrimary,
              ),
            ),
            child: child!,
          );
        },
      );

      if (pickedTime != null) {
        setState(() {
          _dueDateTime = DateTime(
            pickedDate.year,
            pickedDate.month,
            pickedDate.day,
            pickedTime.hour,
            pickedTime.minute,
          );
        });
      }
    }
  }

  Future<void> _submitForm() async {
    if (!_formKey.currentState!.validate()) return;
    _formKey.currentState!.save();

    final provider = context.read<EventProvider>();

    final eventData = <String, dynamic>{
      'title': _title,
      'description': _description,
      'dueDateTime': _dueDateTime.toUtc().toIso8601String(),
      'priorityLevel': _priorityLevel,
      'notes': _notes,
      'attachmentUrl': _attachmentUrl,
    };

    if (!_isEditMode) {
      // Create mode takes course ID and event type index
      eventData['courseId'] = _courseId;
      eventData['eventType'] = _getEventTypeIndex(_eventType);
    } else {
      // Edit mode takes existing event ID
      eventData['id'] = widget.event!.id;
    }

    // Include type-specific parameters depending on event type
    if (_eventType == 'Quiz' || _eventType == 'Exam') {
      eventData['durationMinutes'] = _durationMinutes;
      eventData['venue'] = _venue;
      eventData['seatNumber'] = _seatNumber;
    } else if (_eventType == 'Project') {
      eventData['supervisorName'] = _supervisorName;
      eventData['progressPercentage'] = _progressPercentage;
    } else if (_eventType == 'StudySession') {
      eventData['studyTopic'] = _studyTopic;
      eventData['studyDuration'] = _studyDuration;
    }

    try {
      if (_isEditMode) {
        await provider.updateEvent(widget.event!.id, eventData);
        if (!mounted) return;
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Academic event updated successfully.')),
        );
      } else {
        await provider.createEvent(eventData);
        if (!mounted) return;
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Academic event created successfully.')),
        );
      }
      context.pop();
    } catch (e) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text(provider.errorMessage ?? 'Submission failed: $e'),
          backgroundColor: AppTheme.error,
        ),
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    final eventState = context.watch<EventProvider>();

    return Scaffold(
      appBar: AppBar(
        title: Text(
          _isEditMode ? 'Edit Academic Event' : 'Create Academic Event',
          style: GoogleFonts.outfit(fontWeight: FontWeight.bold),
        ),
        backgroundColor: AppTheme.bgDark,
        elevation: 0,
      ),
      body: Container(
        decoration: const BoxDecoration(
          gradient: LinearGradient(
            begin: Alignment.topCenter,
            end: Alignment.bottomCenter,
            colors: [AppTheme.bgDark, AppTheme.bgDarkSecondary],
          ),
        ),
        child: SafeArea(
          child: eventState.isLoading
              ? const Center(child: CircularProgressIndicator(color: AppTheme.primaryLight))
              : SingleChildScrollView(
                  padding: const EdgeInsets.all(20.0),
                  child: Form(
                    key: _formKey,
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        // Title Input
                        _buildLabel('Title'),
                        TextFormField(
                          initialValue: _title,
                          decoration: _buildInputDecoration('e.g., Assignment 1: Introduction to OOP'),
                          style: const TextStyle(color: AppTheme.textPrimary),
                          validator: (val) => val == null || val.trim().isEmpty ? 'Title is required' : null,
                          onSaved: (val) => _title = val!.trim(),
                        ),
                        const SizedBox(height: 20),

                        // Description Input
                        _buildLabel('Description'),
                        TextFormField(
                          initialValue: _description,
                          maxLines: 3,
                          decoration: _buildInputDecoration('Enter task details or syllabus scope...'),
                          style: const TextStyle(color: AppTheme.textPrimary),
                          onSaved: (val) => _description = val?.trim(),
                        ),
                        const SizedBox(height: 20),

                        // Course Offerings (Read-only course in Edit mode, Dropdown in Create mode)
                        _buildLabel('Course'),
                        if (_isEditMode)
                          Container(
                            padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 14),
                            width: double.infinity,
                            decoration: BoxDecoration(
                              color: AppTheme.bgDarkSecondary,
                              borderRadius: BorderRadius.circular(12),
                              border: Border.all(color: Colors.white.withOpacity(0.08)),
                            ),
                            child: Text(
                              widget.event!.courseCode,
                              style: const TextStyle(color: AppTheme.textPrimary, fontSize: 16),
                            ),
                          )
                        else
                          DropdownButtonFormField<int>(
                            value: _courseId,
                            dropdownColor: AppTheme.bgDarkSecondary,
                            decoration: _buildInputDecoration('Select Course'),
                            items: _courses.map((c) {
                              return DropdownMenuItem<int>(
                                value: c['id'] as int,
                                child: Text('${c['code']} - ${c['title']}', style: const TextStyle(color: AppTheme.textPrimary)),
                              );
                            }).toList(),
                            onChanged: (val) => setState(() => _courseId = val!),
                          ),
                        const SizedBox(height: 20),

                        // Event Type Dropdown (Read-only in Edit mode, Dropdown in Create mode)
                        _buildLabel('Event Type'),
                        if (_isEditMode)
                          Container(
                            padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 14),
                            width: double.infinity,
                            decoration: BoxDecoration(
                              color: AppTheme.bgDarkSecondary,
                              borderRadius: BorderRadius.circular(12),
                              border: Border.all(color: Colors.white.withOpacity(0.08)),
                            ),
                            child: Text(
                              _eventType,
                              style: const TextStyle(color: AppTheme.textPrimary, fontSize: 16),
                            ),
                          )
                        else
                          DropdownButtonFormField<String>(
                            value: _eventType,
                            dropdownColor: AppTheme.bgDarkSecondary,
                            decoration: _buildInputDecoration('Select Event Type'),
                            items: _eventTypes.map((type) {
                              return DropdownMenuItem<String>(
                                value: type,
                                child: Text(type, style: const TextStyle(color: AppTheme.textPrimary)),
                              );
                            }).toList(),
                            onChanged: (val) => setState(() => _eventType = val!),
                          ),
                        const SizedBox(height: 20),

                        // Due Date Time Picker
                        _buildLabel('Due Date & Time'),
                        InkWell(
                          onTap: () => _selectDateTime(context),
                          borderRadius: BorderRadius.circular(12),
                          child: Container(
                            padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 14),
                            decoration: BoxDecoration(
                              color: AppTheme.bgDarkSecondary,
                              borderRadius: BorderRadius.circular(12),
                              border: Border.all(color: Colors.white.withOpacity(0.08)),
                            ),
                            child: Row(
                              mainAxisAlignment: MainAxisAlignment.spaceBetween,
                              children: [
                                Text(
                                  _dueDateTime.toString().substring(0, 16),
                                  style: const TextStyle(color: AppTheme.textPrimary, fontSize: 16),
                                ),
                                const Icon(Icons.calendar_today_rounded, color: AppTheme.primaryLight),
                              ],
                            ),
                          ),
                        ),
                        const SizedBox(height: 20),

                        // Priority Dropdown
                        _buildLabel('Priority Level'),
                        DropdownButtonFormField<String>(
                          value: _priorityLevel,
                          dropdownColor: AppTheme.bgDarkSecondary,
                          decoration: _buildInputDecoration('Select Priority'),
                          items: _priorities.map((prio) {
                            return DropdownMenuItem<String>(
                              value: prio,
                              child: Text(prio, style: const TextStyle(color: AppTheme.textPrimary)),
                            );
                          }).toList(),
                          onChanged: (val) => setState(() => _priorityLevel = val!),
                        ),
                        const SizedBox(height: 20),

                        // Dynamic Custom Inputs based on selected EventType
                        if (_eventType == 'Quiz' || _eventType == 'Exam') ...[
                          _buildLabel('Duration (Minutes)'),
                          TextFormField(
                            initialValue: _durationMinutes?.toString() ?? '60',
                            keyboardType: TextInputType.number,
                            decoration: _buildInputDecoration('Duration in minutes'),
                            style: const TextStyle(color: AppTheme.textPrimary),
                            validator: (val) => val == null || int.tryParse(val) == null ? 'Must be a valid number' : null,
                            onSaved: (val) => _durationMinutes = int.parse(val!),
                          ),
                          const SizedBox(height: 20),

                          _buildLabel('Venue'),
                          TextFormField(
                            initialValue: _venue,
                            decoration: _buildInputDecoration('e.g., Auditorium B / Online'),
                            style: const TextStyle(color: AppTheme.textPrimary),
                            validator: (val) => _eventType == 'Exam' && (val == null || val.trim().isEmpty) ? 'Venue is required for Exams' : null,
                            onSaved: (val) => _venue = val?.trim(),
                          ),
                          const SizedBox(height: 20),

                          _buildLabel('Seat Number (Optional)'),
                          TextFormField(
                            initialValue: _seatNumber,
                            decoration: _buildInputDecoration('e.g., Row D-12'),
                            style: const TextStyle(color: AppTheme.textPrimary),
                            onSaved: (val) => _seatNumber = val?.trim(),
                          ),
                          const SizedBox(height: 20),
                        ],

                        if (_eventType == 'Project') ...[
                          _buildLabel('Supervisor Name'),
                          TextFormField(
                            initialValue: _supervisorName,
                            decoration: _buildInputDecoration('e.g., Dr. Allison'),
                            style: const TextStyle(color: AppTheme.textPrimary),
                            validator: (val) => val == null || val.trim().isEmpty ? 'Supervisor is required' : null,
                            onSaved: (val) => _supervisorName = val!.trim(),
                          ),
                          const SizedBox(height: 20),

                          _buildLabel('Progress Percentage: $_progressPercentage%'),
                          Slider(
                            value: (_progressPercentage ?? 0).toDouble(),
                            min: 0,
                            max: 100,
                            divisions: 20,
                            activeColor: AppTheme.primaryLight,
                            inactiveColor: Colors.white.withOpacity(0.12),
                            onChanged: (val) => setState(() => _progressPercentage = val.toInt()),
                          ),
                          const SizedBox(height: 20),
                        ],

                        if (_eventType == 'StudySession') ...[
                          _buildLabel('Study Topic'),
                          TextFormField(
                            initialValue: _studyTopic,
                            decoration: _buildInputDecoration('e.g., Review chapters 1-4'),
                            style: const TextStyle(color: AppTheme.textPrimary),
                            validator: (val) => val == null || val.trim().isEmpty ? 'Topic is required' : null,
                            onSaved: (val) => _studyTopic = val!.trim(),
                          ),
                          const SizedBox(height: 20),

                          _buildLabel('Duration (Minutes)'),
                          TextFormField(
                            initialValue: _studyDuration?.toString() ?? '60',
                            keyboardType: TextInputType.number,
                            decoration: _buildInputDecoration('e.g., 60'),
                            style: const TextStyle(color: AppTheme.textPrimary),
                            validator: (val) => val == null || int.tryParse(val) == null ? 'Must be a valid number' : null,
                            onSaved: (val) => _studyDuration = int.parse(val!),
                          ),
                          const SizedBox(height: 20),
                        ],

                        // Notes Input
                        _buildLabel('Notes (Optional)'),
                        TextFormField(
                          initialValue: _notes,
                          maxLines: 2,
                          decoration: _buildInputDecoration('Add private study notes or links...'),
                          style: const TextStyle(color: AppTheme.textPrimary),
                          onSaved: (val) => _notes = val?.trim(),
                        ),
                        const SizedBox(height: 20),

                        // Attachment URL Input
                        _buildLabel('Attachment Link (Optional)'),
                        TextFormField(
                          initialValue: _attachmentUrl,
                          decoration: _buildInputDecoration('Paste reference drive / document URL'),
                          style: const TextStyle(color: AppTheme.textPrimary),
                          onSaved: (val) => _attachmentUrl = val?.trim(),
                        ),
                        const SizedBox(height: 36),

                        // Submit Button
                        SizedBox(
                          width: double.infinity,
                          height: 52,
                          child: ElevatedButton(
                            onPressed: _submitForm,
                            style: ElevatedButton.styleFrom(
                              backgroundColor: AppTheme.primaryLight,
                              foregroundColor: Colors.white,
                              shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
                              elevation: 0,
                            ),
                            child: Text(
                              _isEditMode ? 'Update Event' : 'Create Event',
                              style: GoogleFonts.outfit(fontSize: 16, fontWeight: FontWeight.bold),
                            ),
                          ),
                        ),
                        const SizedBox(height: 20),
                      ],
                    ),
                  ),
                ),
        ),
      ),
    );
  }

  Widget _buildLabel(String text) {
    return Padding(
      padding: const EdgeInsets.only(bottom: 8.0, left: 4.0),
      child: Text(
        text,
        style: GoogleFonts.outfit(
          color: AppTheme.textSecondary,
          fontSize: 14,
          fontWeight: FontWeight.w600,
        ),
      ),
    );
  }

  InputDecoration _buildInputDecoration(String hint) {
    return InputDecoration(
      hintText: hint,
      hintStyle: TextStyle(color: AppTheme.textSecondary.withOpacity(0.5)),
      filled: true,
      fillColor: AppTheme.bgDarkSecondary,
      contentPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 14),
      enabledBorder: OutlineInputBorder(
        borderRadius: BorderRadius.circular(12),
        borderSide: BorderSide(color: Colors.white.withOpacity(0.08)),
      ),
      focusedBorder: OutlineInputBorder(
        borderRadius: BorderRadius.circular(12),
        borderSide: const BorderSide(color: AppTheme.primaryLight),
      ),
      errorBorder: OutlineInputBorder(
        borderRadius: BorderRadius.circular(12),
        borderSide: const BorderSide(color: AppTheme.error),
      ),
      focusedErrorBorder: OutlineInputBorder(
        borderRadius: BorderRadius.circular(12),
        borderSide: const BorderSide(color: AppTheme.error),
      ),
    );
  }
}
