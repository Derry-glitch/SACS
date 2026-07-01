import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:go_router/go_router.dart';
import '../services/api_service.dart';
import '../widgets/loading_widget.dart';
import '../widgets/empty_state_widget.dart';
import '../widgets/retry_widget.dart';
import '../core/theme/app_theme.dart';

class AnnouncementsScreen extends StatefulWidget {
  const AnnouncementsScreen({super.key});

  @override
  State<AnnouncementsScreen> createState() => _AnnouncementsScreenState();
}

class _AnnouncementsScreenState extends State<AnnouncementsScreen> {
  final ApiService _apiService = ApiService();
  bool _isLoading = true;
  String? _errorMessage;
  List<dynamic> _announcements = [];

  @override
  void initState() {
    super.initState();
    _fetchAnnouncements();
  }

  Future<void> _fetchAnnouncements() async {
    setState(() {
      _isLoading = true;
      _errorMessage = null;
    });

    try {
      final data = await _apiService.getAnnouncements();
      setState(() {
        _announcements = data;
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
          'Academic Announcements',
          style: GoogleFonts.outfit(fontWeight: FontWeight.bold),
        ),
        backgroundColor: AppTheme.bgDark,
        elevation: 0,
        actions: [
          IconButton(
            icon: const Icon(Icons.refresh),
            onPressed: _fetchAnnouncements,
            tooltip: 'Refresh',
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
                  child: LoadingSkeletonList(itemCount: 4, cardHeight: 90.0),
                )
              : _errorMessage != null
                  ? RetryWidget(
                      errorMessage: _errorMessage!,
                      onRetry: _fetchAnnouncements,
                    )
                  : _announcements.isEmpty
                      ? const EmptyStateWidget(
                          title: 'No Announcements Yet',
                          description: 'Keep an eye out for updates from SACS Academy.',
                          icon: Icons.campaign_rounded,
                        )
                      : ListView.builder(
                          padding: const EdgeInsets.all(24.0),
                          itemCount: _announcements.length,
                          itemBuilder: (context, idx) {
                            final announcement = _announcements[idx];
                            final id = announcement['id'] as int;
                            final title = announcement['title'] as String;
                            final message = announcement['message'] as String;
                            final priority = announcement['priority'] as String;
                            final creatorName = announcement['creatorName'] as String? ?? 'Admin';
                            final isRead = announcement['isRead'] as bool? ?? false;

                            final isUrgent = priority.toLowerCase() == 'urgent' || priority.toLowerCase() == 'high';

                            return Container(
                              margin: const EdgeInsets.only(bottom: 16),
                              decoration: BoxDecoration(
                                color: AppTheme.bgDarkSecondary,
                                borderRadius: BorderRadius.circular(16),
                                border: Border.all(
                                  color: isUrgent
                                      ? AppTheme.error.withOpacity(0.4)
                                      : Colors.white.withOpacity(0.06),
                                  width: isUrgent ? 1.5 : 1.0,
                                ),
                                boxShadow: isUrgent
                                    ? [
                                        BoxShadow(
                                          color: AppTheme.error.withOpacity(0.04),
                                          blurRadius: 8,
                                          offset: const Offset(0, 4),
                                        )
                                      ]
                                    : null,
                              ),
                              child: ListTile(
                                contentPadding: const EdgeInsets.all(18),
                                shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
                                onTap: () => context.push('/announcement-detail', extra: id).then((_) => _fetchAnnouncements()),
                                title: Row(
                                  children: [
                                    if (!isRead)
                                      Container(
                                        width: 8,
                                        height: 8,
                                        margin: const EdgeInsets.only(right: 8),
                                        decoration: const BoxDecoration(
                                          color: AppTheme.accent,
                                          shape: BoxShape.circle,
                                        ),
                                      ),
                                    Expanded(
                                      child: Text(
                                        title,
                                        style: GoogleFonts.outfit(
                                          color: AppTheme.textPrimary,
                                          fontWeight: FontWeight.bold,
                                          fontSize: 16,
                                        ),
                                        maxLines: 1,
                                        overflow: TextOverflow.ellipsis,
                                      ),
                                    ),
                                    if (isUrgent)
                                      Container(
                                        padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
                                        decoration: BoxDecoration(
                                          color: AppTheme.error.withOpacity(0.12),
                                          borderRadius: BorderRadius.circular(6),
                                        ),
                                        child: Text(
                                          'URGENT',
                                          style: GoogleFonts.outfit(
                                            color: AppTheme.error,
                                            fontSize: 10,
                                            fontWeight: FontWeight.bold,
                                            letterSpacing: 0.5,
                                          ),
                                        ),
                                      ),
                                  ],
                                ),
                                subtitle: Column(
                                  crossAxisAlignment: CrossAxisAlignment.start,
                                  children: [
                                    const SizedBox(height: 8),
                                    Text(
                                      message,
                                      style: GoogleFonts.inter(
                                        color: AppTheme.textSecondary,
                                        fontSize: 13,
                                        height: 1.4,
                                      ),
                                      maxLines: 2,
                                      overflow: TextOverflow.ellipsis,
                                    ),
                                    const SizedBox(height: 14),
                                    Row(
                                      mainAxisAlignment: MainAxisAlignment.spaceBetween,
                                      children: [
                                        Text(
                                          'By $creatorName',
                                          style: GoogleFonts.inter(
                                            color: AppTheme.textSecondary.withOpacity(0.5),
                                            fontSize: 11,
                                          ),
                                        ),
                                        Icon(
                                          Icons.arrow_forward_ios_rounded,
                                          color: isUrgent ? AppTheme.error : AppTheme.textSecondary.withOpacity(0.3),
                                          size: 12,
                                        ),
                                      ],
                                    ),
                                  ],
                                ),
                              ),
                            );
                          },
                        ),
        ),
      ),
    );
  }
}
