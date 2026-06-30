import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';
import '../services/api_service.dart';
import '../core/theme/app_theme.dart';

class StudyPlannerScreen extends StatefulWidget {
  const StudyPlannerScreen({super.key});

  @override
  State<StudyPlannerScreen> createState() => _StudyPlannerScreenState();
}

class _StudyPlannerScreenState extends State<StudyPlannerScreen> {
  final ApiService _apiService = ApiService();
  final _formKey = GlobalKey<FormState>();
  final _coursesController = TextEditingController(text: 'CSC201, CSC202, CSC301');
  final _deadlineTitleController = TextEditingController(text: 'Final Semester Exam');
  final _deadlineCourseController = TextEditingController(text: 'CSC201');

  DateTime _selectedDeadlineDate = DateTime.now().add(const Duration(days: 14));
  String _selectedPriority = 'High';
  bool _isProcessing = false;
  String? _errorMessage;

  // Study hours configuration mapping
  final Map<String, double> _freeStudyHours = {
    'Monday': 2.0,
    'Tuesday': 3.0,
    'Wednesday': 2.0,
    'Thursday': 2.0,
    'Friday': 3.0,
    'Saturday': 5.0,
    'Sunday': 0.0,
  };

  // Response plan data
  String? _planName;
  List<dynamic> _planEntries = [];

  @override
  void dispose() {
    _coursesController.dispose();
    _deadlineTitleController.dispose();
    _deadlineCourseController.dispose();
    super.dispose();
  }

  Future<void> _generatePlan() async {
    if (!_formKey.currentState!.validate()) return;

    setState(() {
      _isProcessing = true;
      _planName = null;
      _planEntries = [];
      _errorMessage = null;
    });

    // Split courses by comma
    final courses = _coursesController.text
        .split(',')
        .map((c) => c.trim().toUpperCase())
        .where((c) => c.isNotEmpty)
        .toList();

    // Construct deadline payload list
    final deadlines = [
      {
        'courseCode': _deadlineCourseController.text.trim().toUpperCase(),
        'title': _deadlineTitleController.text.trim(),
        'dueDate': _selectedDeadlineDate.toUtc().toIso8601String(),
        'priority': _selectedPriority,
      }
    ];

    try {
      final data = await _apiService.generateStudyPlan(courses, deadlines, _freeStudyHours);
      setState(() {
        _planName = data['planName'] as String? ?? 'Smart Study Plan';
        _planEntries = data['entries'] as List<dynamic>? ?? [];
        _isProcessing = false;
      });
    } catch (e) {
      setState(() {
        _errorMessage = 'Failed to generate study plan: ${e.toString()}';
        _isProcessing = false;
      });
    }
  }

  Future<void> _selectDate(BuildContext context) async {
    final DateTime? picked = await showDatePicker(
      context: context,
      initialDate: _selectedDeadlineDate,
      firstDate: DateTime.now(),
      lastDate: DateTime.now().add(const Duration(days: 365)),
      builder: (context, child) {
        return Theme(
          data: Theme.of(context).copyWith(
            colorScheme: const ColorScheme.dark(
              primary: AppTheme.success,
              onPrimary: Colors.white,
              surface: AppTheme.bgDarkSecondary,
              onSurface: AppTheme.textPrimary,
            ),
            dialogBackgroundColor: AppTheme.bgDark,
          ),
          child: child!,
        );
      },
    );
    if (picked != null && picked != _selectedDeadlineDate) {
      setState(() {
        _selectedDeadlineDate = picked;
      });
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: Text(
          'Smart Study Planner',
          style: GoogleFonts.outfit(fontWeight: FontWeight.bold),
        ),
        backgroundColor: AppTheme.bgDark,
        elevation: 0,
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
            child: Form(
              key: _formKey,
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    'Create Your Study Timetable',
                    style: GoogleFonts.outfit(
                      color: AppTheme.textPrimary,
                      fontSize: 22,
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                  const SizedBox(height: 8),
                  Text(
                    'Input your courses, upcoming assessment deadlines, and standard daily free study slots to compute an optimized preparation timetable.',
                    style: GoogleFonts.inter(
                      color: AppTheme.textSecondary,
                      fontSize: 13,
                      height: 1.4,
                    ),
                  ),
                  const SizedBox(height: 28),

                  // Courses Input
                  Text(
                    'Active Courses (comma separated)',
                    style: GoogleFonts.outfit(
                      color: AppTheme.textSecondary,
                      fontSize: 14,
                      fontWeight: FontWeight.w600,
                    ),
                  ),
                  const SizedBox(height: 8),
                  TextFormField(
                    controller: _coursesController,
                    decoration: InputDecoration(
                      hintText: 'e.g., CSC201, CSC202, CSC301',
                      filled: true,
                      fillColor: AppTheme.bgDarkSecondary,
                      contentPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 16),
                      enabledBorder: OutlineInputBorder(
                        borderRadius: BorderRadius.circular(12),
                        borderSide: BorderSide(color: Colors.white.withOpacity(0.06)),
                      ),
                      focusedBorder: OutlineInputBorder(
                        borderRadius: BorderRadius.circular(12),
                        borderSide: const BorderSide(color: AppTheme.success),
                      ),
                    ),
                    style: const TextStyle(color: AppTheme.textPrimary),
                    validator: (val) => val == null || val.trim().isEmpty ? 'Enter active course codes' : null,
                  ),
                  const SizedBox(height: 24),

                  // Deadline details card
                  Text(
                    'Upcoming Key Deadline / Exam',
                    style: GoogleFonts.outfit(
                      color: AppTheme.textSecondary,
                      fontSize: 14,
                      fontWeight: FontWeight.w600,
                    ),
                  ),
                  const SizedBox(height: 8),
                  Container(
                    padding: const EdgeInsets.all(20),
                    decoration: BoxDecoration(
                      color: AppTheme.bgDarkSecondary,
                      borderRadius: BorderRadius.circular(20),
                      border: Border.all(color: Colors.white.withOpacity(0.06)),
                    ),
                    child: Column(
                      children: [
                        TextFormField(
                          controller: _deadlineTitleController,
                          decoration: const InputDecoration(
                            labelText: 'Exam/Deadline Title',
                            labelStyle: TextStyle(color: AppTheme.textSecondary),
                          ),
                          style: const TextStyle(color: AppTheme.textPrimary),
                          validator: (val) => val == null || val.trim().isEmpty ? 'Enter event title' : null,
                        ),
                        const SizedBox(height: 12),
                        Row(
                          children: [
                            Expanded(
                              child: TextFormField(
                                controller: _deadlineCourseController,
                                decoration: const InputDecoration(
                                  labelText: 'Course Code',
                                  labelStyle: TextStyle(color: AppTheme.textSecondary),
                                ),
                                style: const TextStyle(color: AppTheme.textPrimary),
                                validator: (val) => val == null || val.trim().isEmpty ? 'Enter course code' : null,
                              ),
                            ),
                            const SizedBox(width: 16),
                            Expanded(
                              child: DropdownButtonFormField<String>(
                                value: _selectedPriority,
                                decoration: const InputDecoration(
                                  labelText: 'Priority',
                                  labelStyle: TextStyle(color: AppTheme.textSecondary),
                                ),
                                dropdownColor: AppTheme.bgDarkSecondary,
                                style: const TextStyle(color: AppTheme.textPrimary),
                                items: ['Low', 'Medium', 'High'].map((String prio) {
                                  return DropdownMenuItem<String>(
                                    value: prio,
                                    child: Text(prio),
                                  );
                                }).toList(),
                                onChanged: (val) {
                                  if (val != null) {
                                    setState(() {
                                      _selectedPriority = val;
                                    });
                                  }
                                },
                              ),
                            ),
                          ],
                        ),
                        const SizedBox(height: 20),
                        Row(
                          mainAxisAlignment: MainAxisAlignment.spaceBetween,
                          children: [
                            Text(
                              'Target Exam Date:',
                              style: GoogleFonts.inter(color: AppTheme.textSecondary, fontSize: 13),
                            ),
                            TextButton.icon(
                              onPressed: () => _selectDate(context),
                              icon: const Icon(Icons.date_range_rounded, color: AppTheme.success, size: 18),
                              label: Text(
                                '${_selectedDeadlineDate.day}/${_selectedDeadlineDate.month}/${_selectedDeadlineDate.year}',
                                style: GoogleFonts.outfit(
                                  color: AppTheme.success,
                                  fontWeight: FontWeight.bold,
                                ),
                              ),
                            ),
                          ],
                        ),
                      ],
                    ),
                  ),
                  const SizedBox(height: 28),

                  // Sliders for study hours mapping
                  Text(
                    'Available Free Study Hours (Daily)',
                    style: GoogleFonts.outfit(
                      color: AppTheme.textSecondary,
                      fontSize: 14,
                      fontWeight: FontWeight.w600,
                    ),
                  ),
                  const SizedBox(height: 8),
                  Container(
                    padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
                    decoration: BoxDecoration(
                      color: AppTheme.bgDarkSecondary,
                      borderRadius: BorderRadius.circular(20),
                      border: Border.all(color: Colors.white.withOpacity(0.06)),
                    ),
                    child: Column(
                      children: _freeStudyHours.keys.map((day) {
                        return Padding(
                          padding: const EdgeInsets.symmetric(vertical: 4.0),
                          child: Row(
                            children: [
                              SizedBox(
                                width: 90,
                                child: Text(
                                  day,
                                  style: GoogleFonts.inter(
                                    color: AppTheme.textPrimary,
                                    fontWeight: FontWeight.w500,
                                    fontSize: 13,
                                  ),
                                ),
                              ),
                              Expanded(
                                child: Slider(
                                  value: _freeStudyHours[day]!,
                                  min: 0.0,
                                  max: 8.0,
                                  divisions: 16,
                                  activeColor: AppTheme.success,
                                  inactiveColor: Colors.white12,
                                  label: '${_freeStudyHours[day]!} hrs',
                                  onChanged: (val) {
                                    setState(() {
                                      _freeStudyHours[day] = val;
                                    });
                                  },
                                ),
                              ),
                              SizedBox(
                                width: 45,
                                child: Text(
                                  '${_freeStudyHours[day]!} h',
                                  style: GoogleFonts.inter(
                                    color: AppTheme.textSecondary,
                                    fontSize: 12,
                                    fontWeight: FontWeight.bold,
                                  ),
                                  textAlign: TextAlign.end,
                                ),
                              ),
                            ],
                          ),
                        );
                      }).toList(),
                    ),
                  ),
                  const SizedBox(height: 24),

                  // Compute Plan Button
                  SizedBox(
                    width: double.infinity,
                    height: 52,
                    child: ElevatedButton.icon(
                      onPressed: _isProcessing ? null : _generatePlan,
                      icon: _isProcessing
                          ? const SizedBox(
                              width: 20,
                              height: 20,
                              child: CircularProgressIndicator(color: Colors.white, strokeWidth: 2),
                            )
                          : const Icon(Icons.rocket_launch_rounded, size: 20),
                      label: Text(
                        'Generate Study Plan',
                        style: GoogleFonts.outfit(fontSize: 15, fontWeight: FontWeight.bold),
                      ),
                      style: ElevatedButton.styleFrom(
                        backgroundColor: AppTheme.success,
                        foregroundColor: Colors.white,
                        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
                      ),
                    ),
                  ),
                  const SizedBox(height: 32),

                  // Error Box
                  if (_errorMessage != null)
                    Container(
                      width: double.infinity,
                      padding: const EdgeInsets.all(16),
                      decoration: BoxDecoration(
                        color: AppTheme.error.withOpacity(0.1),
                        borderRadius: BorderRadius.circular(12),
                        border: Border.all(color: AppTheme.error.withOpacity(0.3)),
                      ),
                      child: Text(
                        _errorMessage!,
                        style: const TextStyle(color: AppTheme.error, fontSize: 13),
                      ),
                    ),

                  // Timetable Display Card
                  if (_planEntries.isNotEmpty) ...[
                    Row(
                      mainAxisAlignment: MainAxisAlignment.spaceBetween,
                      children: [
                        Text(
                          _planName ?? 'Your Optimized Plan',
                          style: GoogleFonts.outfit(
                            color: AppTheme.textPrimary,
                            fontSize: 18,
                            fontWeight: FontWeight.bold,
                          ),
                        ),
                        const Icon(Icons.verified_rounded, color: AppTheme.success, size: 20),
                      ],
                    ),
                    const SizedBox(height: 12),

                    // Plan timeline view
                    ListView.builder(
                      shrinkWrap: true,
                      physics: const NeverScrollableScrollPhysics(),
                      itemCount: _planEntries.length,
                      itemBuilder: (context, idx) {
                        final entry = _planEntries[idx] as Map<String, dynamic>;
                        final dayOfWeek = entry['dayOfWeek'] as String;
                        final dateStr = entry['date'] as String;
                        final startTime = entry['startTime'] as String;
                        final endTime = entry['endTime'] as String;
                        final courseCode = entry['courseCode'] as String;
                        final topic = entry['topic'] as String;
                        final priority = entry['priority'] as String;

                        Color tagColor = AppTheme.primaryLight;
                        if (priority.toLowerCase() == 'high') {
                          tagColor = AppTheme.error;
                        }

                        // format start time HH:MM:SS to HH:MM
                        final timeRange = '${startTime.substring(0, 5)} - ${endTime.substring(0, 5)}';

                        return Container(
                          margin: const EdgeInsets.only(bottom: 16),
                          decoration: BoxDecoration(
                            color: AppTheme.bgDarkSecondary,
                            borderRadius: BorderRadius.circular(16),
                            border: Border.all(color: Colors.white.withOpacity(0.06)),
                          ),
                          child: Row(
                            children: [
                              // Left day border tag
                              Container(
                                width: 6,
                                height: 100,
                                decoration: BoxDecoration(
                                  color: tagColor,
                                  borderRadius: const BorderRadius.only(
                                    topLeft: Radius.circular(16),
                                    bottomLeft: Radius.circular(16),
                                  ),
                                ),
                              ),
                              const SizedBox(width: 16),

                              // Time column
                              Expanded(
                                flex: 4,
                                child: Padding(
                                  padding: const EdgeInsets.symmetric(vertical: 16.0),
                                  child: Column(
                                    crossAxisAlignment: CrossAxisAlignment.start,
                                    children: [
                                      Text(
                                        dayOfWeek.toUpperCase(),
                                        style: GoogleFonts.outfit(
                                          color: tagColor,
                                          fontSize: 12,
                                          fontWeight: FontWeight.bold,
                                          letterSpacing: 1.0,
                                        ),
                                      ),
                                      const SizedBox(height: 4),
                                      Text(
                                        dateStr,
                                        style: GoogleFonts.inter(
                                          color: AppTheme.textSecondary,
                                          fontSize: 10,
                                        ),
                                      ),
                                      const SizedBox(height: 10),
                                      Row(
                                        children: [
                                          const Icon(Icons.access_time_rounded, color: AppTheme.textSecondary, size: 14),
                                          const SizedBox(width: 4),
                                          Text(
                                            timeRange,
                                            style: GoogleFonts.inter(
                                              color: AppTheme.textPrimary,
                                              fontSize: 12,
                                              fontWeight: FontWeight.w600,
                                            ),
                                          ),
                                        ],
                                      ),
                                    ],
                                  ),
                                ),
                              ),

                              // Course and Topic column
                              Expanded(
                                flex: 6,
                                child: Padding(
                                  padding: const EdgeInsets.only(right: 16.0, top: 16, bottom: 16),
                                  child: Column(
                                    crossAxisAlignment: CrossAxisAlignment.start,
                                    children: [
                                      Container(
                                        padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
                                        decoration: BoxDecoration(
                                          color: tagColor.withOpacity(0.12),
                                          borderRadius: BorderRadius.circular(6),
                                        ),
                                        child: Text(
                                          courseCode,
                                          style: GoogleFonts.outfit(
                                            color: tagColor,
                                            fontSize: 11,
                                            fontWeight: FontWeight.bold,
                                          ),
                                        ),
                                      ),
                                      const SizedBox(height: 8),
                                      Text(
                                        topic,
                                        style: GoogleFonts.inter(
                                          color: AppTheme.textPrimary,
                                          fontSize: 13,
                                          fontWeight: FontWeight.w600,
                                        ),
                                        maxLines: 2,
                                        overflow: TextOverflow.ellipsis,
                                      ),
                                    ],
                                  ),
                                ),
                              ),
                            ],
                          ),
                        );
                      },
                    ),
                  ],
                ],
              ),
            ),
          ),
        ),
      ),
    );
  }
}
