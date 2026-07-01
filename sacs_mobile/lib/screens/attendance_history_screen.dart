import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:provider/provider.dart';
import '../services/api_service.dart';
import '../providers/auth_provider.dart';
import '../widgets/loading_widget.dart';
import '../widgets/empty_state_widget.dart';
import '../widgets/retry_widget.dart';
import '../core/theme/app_theme.dart';

class AttendanceHistoryScreen extends StatefulWidget {
  const AttendanceHistoryScreen({super.key});

  @override
  State<AttendanceHistoryScreen> createState() => _AttendanceHistoryScreenState();
}

class _AttendanceHistoryScreenState extends State<AttendanceHistoryScreen> {
  final ApiService _apiService = ApiService();
  bool _isLoading = true;
  String? _errorMessage;

  double _attendancePercentage = 100.0;
  int _totalClasses = 0;
  int _classesAttended = 0;
  List<dynamic> _records = [];

  @override
  void initState() {
    super.initState();
    _fetchHistory();
  }

  Future<void> _fetchHistory() async {
    setState(() {
      _isLoading = true;
      _errorMessage = null;
    });

    try {
      final authProvider = Provider.of<AuthProvider>(context, listen: false);
      final studentId = authProvider.user?.id;
      if (studentId == null) {
        throw Exception('User is not authenticated');
      }

      final data = await _apiService.getAttendanceHistory(studentId);
      setState(() {
        _attendancePercentage = (data['attendancePercentage'] as num?)?.toDouble() ?? 100.0;
        _totalClasses = (data['totalClasses'] as num?)?.toInt() ?? 0;
        _classesAttended = (data['classesAttended'] as num?)?.toInt() ?? 0;
        _records = data['records'] as List<dynamic>? ?? [];
        _isLoading = false;
      });
    } catch (e) {
      setState(() {
        _errorMessage = e.toString().replaceAll('Failure: ', '');
        _isLoading = false;
      });
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: Text(
          'Attendance Logs',
          style: GoogleFonts.outfit(fontWeight: FontWeight.bold),
        ),
        backgroundColor: AppTheme.bgDark,
        elevation: 0,
        actions: [
          IconButton(
            icon: const Icon(Icons.refresh),
            onPressed: _fetchHistory,
            tooltip: 'Refresh Logs',
          )
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
          child: _isLoading
              ? const Padding(
                  padding: EdgeInsets.all(24.0),
                  child: LoadingSkeletonList(itemCount: 4, cardHeight: 80.0),
                )
              : _errorMessage != null
                  ? RetryWidget(
                      errorMessage: _errorMessage!,
                      onRetry: _fetchHistory,
                    )
                  : SingleChildScrollView(
                      padding: const EdgeInsets.all(24.0),
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          // Stats Overview Card
                          _buildStatsCard(),
                          const SizedBox(height: 32),

                          Text(
                            'Check-in History',
                            style: GoogleFonts.outfit(
                              color: AppTheme.textPrimary,
                              fontSize: 18,
                              fontWeight: FontWeight.bold,
                            ),
                          ),
                          const SizedBox(height: 16),

                           if (_records.isEmpty)
                            const EmptyStateWidget(
                              title: 'No Check-in Records',
                              description: 'You haven\'t signed into any attendance sessions yet.',
                              icon: Icons.history_toggle_off_rounded,
                            )
                          else
                            ListView.builder(
                              shrinkWrap: true,
                              physics: const NeverScrollableScrollPhysics(),
                              itemCount: _records.length,
                              itemBuilder: (context, idx) {
                                final record = _records[idx];
                                final courseCode = record['courseCode'] as String;
                                final courseTitle = record['courseTitle'] as String;
                                final dateStr = record['date'] as String;
                                final status = record['status'] as String;
                                final notes = record['notes'] as String?;

                                Color statusColor = AppTheme.success;
                                if (status.toLowerCase() == 'absent') {
                                  statusColor = AppTheme.error;
                                } else if (status.toLowerCase() == 'late') {
                                  statusColor = AppTheme.accent;
                                }

                                return Container(
                                  margin: const EdgeInsets.only(bottom: 12),
                                  padding: const EdgeInsets.all(16),
                                  decoration: BoxDecoration(
                                    color: AppTheme.bgDarkSecondary,
                                    borderRadius: BorderRadius.circular(16),
                                    border: Border.all(color: Colors.white.withOpacity(0.05)),
                                  ),
                                  child: Row(
                                    crossAxisAlignment: CrossAxisAlignment.start,
                                    children: [
                                      // Status Circle Icon
                                      Container(
                                        padding: const EdgeInsets.all(10),
                                        decoration: BoxDecoration(
                                          color: statusColor.withOpacity(0.1),
                                          shape: BoxShape.circle,
                                        ),
                                        child: Icon(
                                          status.toLowerCase() == 'absent'
                                              ? Icons.cancel_outlined
                                              : status.toLowerCase() == 'late'
                                                  ? Icons.access_time_rounded
                                                  : Icons.check_circle_outline_rounded,
                                          color: statusColor,
                                          size: 20,
                                        ),
                                      ),
                                      const SizedBox(width: 16),

                                      // Main Record Info
                                      Expanded(
                                        child: Column(
                                          crossAxisAlignment: CrossAxisAlignment.start,
                                          children: [
                                            Row(
                                              mainAxisAlignment: MainAxisAlignment.spaceBetween,
                                              children: [
                                                Text(
                                                  courseCode,
                                                  style: GoogleFonts.outfit(
                                                    color: AppTheme.textPrimary,
                                                    fontWeight: FontWeight.bold,
                                                    fontSize: 14,
                                                  ),
                                                ),
                                                Text(
                                                  dateStr,
                                                  style: GoogleFonts.inter(
                                                    color: AppTheme.textSecondary,
                                                    fontSize: 11,
                                                  ),
                                                ),
                                              ],
                                            ),
                                            const SizedBox(height: 4),
                                            Text(
                                              courseTitle,
                                              style: GoogleFonts.inter(
                                                color: AppTheme.textSecondary,
                                                fontSize: 12,
                                              ),
                                              maxLines: 1,
                                              overflow: TextOverflow.ellipsis,
                                            ),
                                            if (notes != null && notes.isNotEmpty) ...[
                                              const SizedBox(height: 8),
                                              Text(
                                                notes,
                                                style: GoogleFonts.inter(
                                                  color: AppTheme.textSecondary.withOpacity(0.6),
                                                  fontSize: 11,
                                                  fontStyle: FontStyle.italic,
                                                ),
                                              ),
                                            ]
                                          ],
                                        ),
                                      ),
                                    ],
                                  ),
                                );
                              },
                            ),
                        ],
                      ),
                    ),
        ),
      ),
    );
  }

  Widget _buildStatsCard() {
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.all(24),
      decoration: BoxDecoration(
        color: AppTheme.bgDarkSecondary,
        borderRadius: BorderRadius.circular(24),
        border: Border.all(color: AppTheme.accent.withOpacity(0.15)),
        boxShadow: [
          BoxShadow(
            color: AppTheme.accent.withOpacity(0.03),
            blurRadius: 16,
            offset: const Offset(0, 4),
          )
        ],
      ),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceBetween,
        children: [
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  'Tracked Classes',
                  style: GoogleFonts.inter(color: AppTheme.textSecondary, fontSize: 13),
                ),
                const SizedBox(height: 4),
                Text(
                  '$_classesAttended / $_totalClasses',
                  style: GoogleFonts.outfit(
                    color: AppTheme.textPrimary,
                    fontSize: 24,
                    fontWeight: FontWeight.bold,
                  ),
                ),
                const SizedBox(height: 6),
                Text(
                  'Keep your presence above 75% to remain eligible for examinations.',
                  style: GoogleFonts.inter(
                    color: AppTheme.textSecondary.withOpacity(0.6),
                    fontSize: 11,
                    height: 1.4,
                  ),
                ),
              ],
            ),
          ),
          const SizedBox(width: 24),

          // Percentage ring
          Stack(
            alignment: Alignment.center,
            children: [
              SizedBox(
                width: 80,
                height: 80,
                child: CircularProgressIndicator(
                  value: _totalClasses > 0 ? (_classesAttended / _totalClasses) : 1.0,
                  backgroundColor: Colors.white.withOpacity(0.04),
                  valueColor: AlwaysStoppedAnimation<Color>(
                    _attendancePercentage >= 75.0 ? AppTheme.success : AppTheme.error,
                  ),
                  strokeWidth: 8,
                ),
              ),
              Column(
                mainAxisSize: MainAxisSize.min,
                children: [
                  Text(
                    '${_attendancePercentage.toStringAsFixed(0)}%',
                    style: GoogleFonts.outfit(
                      color: AppTheme.textPrimary,
                      fontSize: 18,
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                  Text(
                    'Rate',
                    style: GoogleFonts.inter(
                      color: AppTheme.textSecondary,
                      fontSize: 9,
                    ),
                  ),
                ],
              ),
            ],
          ),
        ],
      ),
    );
  }
}
