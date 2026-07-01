import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';
import '../services/api_service.dart';
import '../core/theme/app_theme.dart';

class AnnouncementDetailScreen extends StatefulWidget {
  final int announcementId;
  const AnnouncementDetailScreen({super.key, required this.announcementId});

  @override
  State<AnnouncementDetailScreen> createState() => _AnnouncementDetailScreenState();
}

class _AnnouncementDetailScreenState extends State<AnnouncementDetailScreen> {
  final ApiService _apiService = ApiService();
  bool _isLoading = true;
  String? _errorMessage;
  Map<String, dynamic>? _announcement;

  @override
  void initState() {
    super.initState();
    _fetchDetails();
  }

  Future<void> _fetchDetails() async {
    setState(() {
      _isLoading = true;
      _errorMessage = null;
    });

    try {
      final data = await _apiService.getAnnouncementDetails(widget.announcementId);
      setState(() {
        _announcement = data;
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
    String titleText = 'Announcement Details';
    bool isUrgent = false;

    if (_announcement != null) {
      titleText = _announcement!['title'] as String? ?? 'Notice';
      final prio = _announcement!['priority'] as String? ?? 'Normal';
      isUrgent = prio.toLowerCase() == 'urgent' || prio.toLowerCase() == 'high';
    }

    return Scaffold(
      appBar: AppBar(
        title: Text(
          titleText,
          style: GoogleFonts.outfit(fontWeight: FontWeight.bold, fontSize: 16),
        ),
        backgroundColor: isUrgent ? AppTheme.error.withOpacity(0.15) : AppTheme.bgDark,
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
          child: _isLoading
              ? const Center(child: CircularProgressIndicator(color: AppTheme.accent))
              : _errorMessage != null
                  ? Center(
                      child: Padding(
                        padding: const EdgeInsets.all(24.0),
                        child: Column(
                          mainAxisAlignment: MainAxisAlignment.center,
                          children: [
                            const Icon(Icons.error_outline_rounded, color: AppTheme.error, size: 48),
                            const SizedBox(height: 16),
                            Text(
                              _errorMessage!,
                              style: GoogleFonts.inter(color: AppTheme.textSecondary, fontSize: 14),
                              textAlign: TextAlign.center,
                            ),
                            const SizedBox(height: 16),
                            ElevatedButton(
                              onPressed: _fetchDetails,
                              style: ElevatedButton.styleFrom(backgroundColor: AppTheme.primaryLight),
                              child: const Text('Retry'),
                            )
                          ],
                        ),
                      ),
                    )
                  : SingleChildScrollView(
                      padding: const EdgeInsets.all(24.0),
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          // Urgent Notice Banner
                          if (isUrgent) ...[
                            Container(
                              width: double.infinity,
                              padding: const EdgeInsets.all(16),
                              decoration: BoxDecoration(
                                color: AppTheme.error.withOpacity(0.12),
                                borderRadius: BorderRadius.circular(12),
                                border: Border.all(color: AppTheme.error.withOpacity(0.3)),
                              ),
                              child: Row(
                                children: [
                                  const Icon(Icons.warning_rounded, color: AppTheme.error, size: 20),
                                  const SizedBox(width: 12),
                                  Expanded(
                                    child: Text(
                                      'This is an urgent academic notification. Please read carefully and take appropriate action.',
                                      style: GoogleFonts.inter(
                                        color: AppTheme.error,
                                        fontSize: 12,
                                        fontWeight: FontWeight.w600,
                                        height: 1.4,
                                      ),
                                    ),
                                  ),
                                ],
                              ),
                            ),
                            const SizedBox(height: 24),
                          ],

                          // Creator details
                          Row(
                            children: [
                              CircleAvatar(
                                radius: 20,
                                backgroundColor: isUrgent ? AppTheme.error.withOpacity(0.12) : AppTheme.primaryLight.withOpacity(0.12),
                                child: Icon(
                                  Icons.person_rounded,
                                  color: isUrgent ? AppTheme.error : AppTheme.primaryLight,
                                  size: 20,
                                ),
                              ),
                              const SizedBox(width: 14),
                              Expanded(
                                child: Column(
                                  crossAxisAlignment: CrossAxisAlignment.start,
                                  children: [
                                    Text(
                                      _announcement!['creatorName'] as String? ?? 'Academic Administrator',
                                      style: GoogleFonts.outfit(
                                        color: AppTheme.textPrimary,
                                        fontWeight: FontWeight.bold,
                                        fontSize: 14,
                                      ),
                                    ),
                                    const SizedBox(height: 2),
                                    Text(
                                      'Published on ${_formatDate(_announcement!['createdAt'] as String?)}',
                                      style: GoogleFonts.inter(
                                        color: AppTheme.textSecondary,
                                        fontSize: 11,
                                      ),
                                    ),
                                  ],
                                ),
                              ),
                              Container(
                                padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 6),
                                decoration: BoxDecoration(
                                  color: (isUrgent ? AppTheme.error : AppTheme.primaryLight).withOpacity(0.12),
                                  borderRadius: BorderRadius.circular(8),
                                ),
                                child: Text(
                                  (_announcement!['priority'] as String).toUpperCase(),
                                  style: GoogleFonts.outfit(
                                    color: isUrgent ? AppTheme.error : AppTheme.primaryLight,
                                    fontSize: 10,
                                    fontWeight: FontWeight.bold,
                                  ),
                                ),
                              ),
                            ],
                          ),
                          const SizedBox(height: 28),

                          const Divider(color: Colors.white12),
                          const SizedBox(height: 20),

                          // Full content message body
                          Text(
                            _announcement!['message'] as String? ?? '',
                            style: GoogleFonts.inter(
                              color: AppTheme.textPrimary,
                              fontSize: 14,
                              height: 1.6,
                            ),
                          ),
                        ],
                      ),
                    ),
        ),
      ),
    );
  }

  String _formatDate(String? isoString) {
    if (isoString == null) return '';
    try {
      final dateVal = DateTime.parse(isoString).toLocal();
      final timeLabel = '${dateVal.hour.toString().padLeft(2, '0')}:${dateVal.minute.toString().padLeft(2, '0')}';
      final dateLabel = '${dateVal.day}/${dateVal.month}/${dateVal.year}';
      return '$dateLabel @ $timeLabel';
    } catch (_) {
      return '';
    }
  }
}
