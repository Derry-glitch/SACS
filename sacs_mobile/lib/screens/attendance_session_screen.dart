import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';
import '../services/api_service.dart';
import '../core/theme/app_theme.dart';

class AttendanceSessionScreen extends StatefulWidget {
  const AttendanceSessionScreen({super.key});

  @override
  State<AttendanceSessionScreen> createState() => _AttendanceSessionScreenState();
}

class _AttendanceSessionScreenState extends State<AttendanceSessionScreen> {
  final _formKey = GlobalKey<FormState>();
  final _offeringIdController = TextEditingController();
  final _durationController = TextEditingController(text: '15');
  final ApiService _apiService = ApiService();

  bool _isLoading = false;
  String? _sessionCode;
  int? _activeOfferingId;
  List<dynamic> _checkedInStudents = [];

  @override
  void dispose() {
    _offeringIdController.dispose();
    _durationController.dispose();
    super.dispose();
  }

  Future<void> _startSession() async {
    if (!_formKey.currentState!.validate()) return;

    setState(() {
      _isLoading = true;
      _sessionCode = null;
    });

    try {
      final offeringId = int.parse(_offeringIdController.text);
      final duration = int.parse(_durationController.text);

      final response = await _apiService.lecturerCreateAttendanceSession(offeringId, duration);

      setState(() {
        _sessionCode = response['code'] as String?;
        _activeOfferingId = offeringId;
        _isLoading = false;
      });

      _fetchCheckedInStudents();
    } catch (e) {
      setState(() {
        _isLoading = false;
      });
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text(e.toString().replaceAll('Failure: ', '')),
            backgroundColor: AppTheme.error,
          ),
        );
      }
    }
  }

  Future<void> _fetchCheckedInStudents() async {
    if (_activeOfferingId == null) return;
    try {
      final students = await _apiService.lecturerGetCourseAttendance(_activeOfferingId!);
      setState(() {
        _checkedInStudents = students;
      });
    } catch (e) {
      // Ignored
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: Text(
          'Start Attendance Register',
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
                crossAxisAlignment: CrossAxisAlignment.stretch,
                children: [
                  if (_sessionCode == null) ...[
                    Text(
                      'Configure Class Session',
                      style: GoogleFonts.outfit(
                        fontSize: 20,
                        fontWeight: FontWeight.bold,
                        color: AppTheme.textPrimary,
                      ),
                    ),
                    const SizedBox(height: 8),
                    Text(
                      'Input course offering ID and session duration to generate check-in token.',
                      style: GoogleFonts.inter(
                        color: AppTheme.textSecondary,
                        fontSize: 13,
                      ),
                    ),
                    const SizedBox(height: 32),
                    TextFormField(
                      controller: _offeringIdController,
                      keyboardType: TextInputType.number,
                      validator: (val) {
                        if (val == null || val.isEmpty) return 'Please enter course offering ID.';
                        if (int.tryParse(val) == null) return 'Must be a valid integer.';
                        return null;
                      },
                      decoration: const InputDecoration(
                        labelText: 'Course Offering ID',
                        prefixIcon: Icon(Icons.school_rounded),
                      ),
                    ),
                    const SizedBox(height: 16),
                    TextFormField(
                      controller: _durationController,
                      keyboardType: TextInputType.number,
                      validator: (val) {
                        if (val == null || val.isEmpty) return 'Please enter duration.';
                        if (int.tryParse(val) == null) return 'Must be a valid integer.';
                        return null;
                      },
                      decoration: const InputDecoration(
                        labelText: 'Duration (Minutes)',
                        prefixIcon: Icon(Icons.timer_rounded),
                      ),
                    ),
                    const SizedBox(height: 28),
                    ElevatedButton(
                      onPressed: _isLoading ? null : _startSession,
                      style: ElevatedButton.styleFrom(
                        backgroundColor: AppTheme.primaryLight,
                        padding: const EdgeInsets.symmetric(vertical: 16),
                        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
                      ),
                      child: _isLoading
                          ? const SizedBox(
                              height: 20,
                              width: 20,
                              child: CircularProgressIndicator(color: Colors.white, strokeWidth: 2),
                            )
                          : Text(
                              'Generate Code',
                              style: GoogleFonts.outfit(fontSize: 16, fontWeight: FontWeight.bold, color: Colors.white),
                            ),
                    ),
                  ] else ...[
                    // Active Session Panel
                    Center(
                      child: Container(
                        padding: const EdgeInsets.all(28),
                        decoration: BoxDecoration(
                          color: AppTheme.bgDarkSecondary,
                          borderRadius: BorderRadius.circular(24),
                          border: Border.all(color: AppTheme.primaryLight.withOpacity(0.3)),
                        ),
                        child: Column(
                          children: [
                            Text(
                              'ACTIVE SESSION CODE',
                              style: GoogleFonts.outfit(
                                fontSize: 13,
                                color: AppTheme.textSecondary,
                                fontWeight: FontWeight.bold,
                                letterSpacing: 1.5,
                              ),
                            ),
                            const SizedBox(height: 12),
                            Text(
                              _sessionCode!,
                              style: GoogleFonts.outfit(
                                fontSize: 48,
                                color: AppTheme.primaryLight,
                                fontWeight: FontWeight.bold,
                                letterSpacing: 6,
                              ),
                            ),
                            const SizedBox(height: 8),
                            Text(
                              'Instruct students to enter this 6-digit code in their attendance portal.',
                              textAlign: TextAlign.center,
                              style: GoogleFonts.inter(
                                fontSize: 12,
                                color: AppTheme.textSecondary,
                              ),
                            ),
                          ],
                        ),
                      ),
                    ),
                    const SizedBox(height: 36),

                    Row(
                      mainAxisAlignment: MainAxisAlignment.spaceBetween,
                      children: [
                        Text(
                          'Enrolled Student Check-ins',
                          style: GoogleFonts.outfit(
                            fontSize: 18,
                            fontWeight: FontWeight.bold,
                            color: AppTheme.textPrimary,
                          ),
                        ),
                        IconButton(
                          icon: const Icon(Icons.refresh_rounded, color: AppTheme.primaryLight),
                          onPressed: _fetchCheckedInStudents,
                          tooltip: 'Sync logs',
                        ),
                      ],
                    ),
                    const SizedBox(height: 16),
                    _checkedInStudents.isEmpty
                        ? Container(
                            padding: const EdgeInsets.all(24),
                            alignment: Alignment.center,
                            child: Text(
                              'Waiting for student check-ins...',
                              style: GoogleFonts.inter(color: AppTheme.textSecondary, fontStyle: FontStyle.italic),
                            ),
                          )
                        : ListView.builder(
                            shrinkWrap: true,
                            physics: const NeverScrollableScrollPhysics(),
                            itemCount: _checkedInStudents.length,
                            itemBuilder: (context, idx) {
                              final student = _checkedInStudents[idx];
                              final name = student['studentName'] as String;
                              final matric = student['matriculationNumber'] as String;
                              final date = student['date'] as String;
                              final status = student['status'] as String;

                              return Container(
                                margin: const EdgeInsets.only(bottom: 12),
                                padding: const EdgeInsets.all(16),
                                decoration: BoxDecoration(
                                  color: AppTheme.bgDarkSecondary,
                                  borderRadius: BorderRadius.circular(16),
                                  border: Border.all(color: Colors.white.withOpacity(0.04)),
                                ),
                                child: Row(
                                  mainAxisAlignment: MainAxisAlignment.spaceBetween,
                                  children: [
                                    Column(
                                      crossAxisAlignment: CrossAxisAlignment.start,
                                      children: [
                                        Text(
                                          name,
                                          style: GoogleFonts.outfit(
                                            color: AppTheme.textPrimary,
                                            fontWeight: FontWeight.bold,
                                          ),
                                        ),
                                        const SizedBox(height: 4),
                                        Text(
                                          '$matric • $date',
                                          style: GoogleFonts.inter(
                                            color: AppTheme.textSecondary,
                                            fontSize: 11,
                                          ),
                                        ),
                                      ],
                                    ),
                                    Container(
                                      padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
                                      decoration: BoxDecoration(
                                        color: AppTheme.success.withOpacity(0.12),
                                        borderRadius: BorderRadius.circular(6),
                                      ),
                                      child: Text(
                                        status.toUpperCase(),
                                        style: GoogleFonts.outfit(
                                          color: AppTheme.success,
                                          fontSize: 10,
                                          fontWeight: FontWeight.bold,
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
