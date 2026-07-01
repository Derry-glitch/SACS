import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:provider/provider.dart';
import '../services/api_service.dart';
import '../providers/auth_provider.dart';
import '../core/theme/app_theme.dart';

class NotificationScreen extends StatefulWidget {
  const NotificationScreen({super.key});

  @override
  State<NotificationScreen> createState() => _NotificationScreenState();
}

class _NotificationScreenState extends State<NotificationScreen> {
  final ApiService _apiService = ApiService();
  bool _isLoading = true;
  String? _errorMessage;
  List<dynamic> _notifications = [];

  @override
  void initState() {
    super.initState();
    _fetchNotifications();
  }

  Future<void> _fetchNotifications() async {
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

      final data = await _apiService.getUserNotifications(studentId);
      setState(() {
        _notifications = data['notifications'] as List<dynamic>? ?? [];
        _isLoading = false;
      });
    } catch (e) {
      setState(() {
        _errorMessage = e.toString().replaceAll('Failure: ', '');
        _isLoading = false;
      });
    }
  }

  Future<void> _markAsRead(int id) async {
    try {
      await _apiService.markNotificationAsRead(id);
      // Update item state in memory
      setState(() {
        final index = _notifications.indexWhere((n) => n['id'] == id);
        if (index != -1) {
          _notifications[index] = {
            ..._notifications[index] as Map<String, dynamic>,
            'isRead': true,
          };
        }
      });
    } catch (e) {
      // Ignored
    }
  }

  Future<void> _markAllAsRead() async {
    for (var item in _notifications) {
      final isRead = item['isRead'] as bool? ?? false;
      if (!isRead) {
        final id = item['id'] as int;
        await _markAsRead(id);
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    final unreadCount = _notifications.where((n) => !(n['isRead'] as bool? ?? false)).length;

    return Scaffold(
      appBar: AppBar(
        title: Text(
          'Alerts & Notices',
          style: GoogleFonts.outfit(fontWeight: FontWeight.bold),
        ),
        backgroundColor: AppTheme.bgDark,
        elevation: 0,
        actions: [
          if (unreadCount > 0)
            TextButton.icon(
              onPressed: _markAllAsRead,
              icon: const Icon(Icons.done_all_rounded, size: 16, color: AppTheme.accent),
              label: Text(
                'Mark All Read',
                style: GoogleFonts.outfit(
                  color: AppTheme.accent,
                  fontSize: 12,
                  fontWeight: FontWeight.bold,
                ),
              ),
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
                              onPressed: _fetchNotifications,
                              style: ElevatedButton.styleFrom(backgroundColor: AppTheme.accent),
                              child: const Text('Retry'),
                            )
                          ],
                        ),
                      ),
                    )
                  : _notifications.isEmpty
                      ? Center(
                          child: Column(
                            mainAxisAlignment: MainAxisAlignment.center,
                            children: [
                              Icon(Icons.notifications_off_rounded, color: AppTheme.textSecondary.withOpacity(0.2), size: 64),
                              const SizedBox(height: 16),
                              Text(
                                'Your inbox is clear.',
                                style: GoogleFonts.inter(color: AppTheme.textSecondary, fontSize: 14),
                              ),
                            ],
                          ),
                        )
                      : ListView.builder(
                          padding: const EdgeInsets.all(24.0),
                          itemCount: _notifications.length,
                          itemBuilder: (context, idx) {
                            final notification = _notifications[idx];
                            final id = notification['id'] as int;
                            final title = notification['title'] as String;
                            final body = notification['body'] as String;
                            final sentAtStr = notification['sentAt'] as String;
                            final isRead = notification['isRead'] as bool? ?? false;

                            // simple date formatting
                            final dateVal = DateTime.parse(sentAtStr).toLocal();
                            final timeLabel = '${dateVal.hour.toString().padLeft(2, '0')}:${dateVal.minute.toString().padLeft(2, '0')}';
                            final dateLabel = '${dateVal.day}/${dateVal.month}/${dateVal.year}';

                            return Container(
                              margin: const EdgeInsets.only(bottom: 12),
                              decoration: BoxDecoration(
                                color: AppTheme.bgDarkSecondary,
                                borderRadius: BorderRadius.circular(16),
                                border: Border.all(
                                  color: isRead ? Colors.white.withOpacity(0.04) : AppTheme.accent.withOpacity(0.2),
                                ),
                              ),
                              child: ListTile(
                                contentPadding: const EdgeInsets.all(16),
                                shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
                                onTap: () {
                                  if (!isRead) {
                                    _markAsRead(id);
                                  }
                                },
                                leading: Container(
                                  padding: const EdgeInsets.all(10),
                                  decoration: BoxDecoration(
                                    color: (isRead ? AppTheme.primaryLight : AppTheme.accent).withOpacity(0.1),
                                    shape: BoxShape.circle,
                                  ),
                                  child: Icon(
                                    isRead ? Icons.notifications_none_rounded : Icons.notifications_active_rounded,
                                    color: isRead ? AppTheme.primaryLight : AppTheme.accent,
                                    size: 20,
                                  ),
                                ),
                                title: Row(
                                  children: [
                                    Expanded(
                                      child: Text(
                                        title,
                                        style: GoogleFonts.outfit(
                                          color: AppTheme.textPrimary,
                                          fontWeight: isRead ? FontWeight.w600 : FontWeight.bold,
                                          fontSize: 14,
                                        ),
                                      ),
                                    ),
                                    if (!isRead)
                                      Container(
                                        width: 8,
                                        height: 8,
                                        decoration: const BoxDecoration(
                                          color: AppTheme.accent,
                                          shape: BoxShape.circle,
                                        ),
                                      ),
                                  ],
                                ),
                                subtitle: Column(
                                  crossAxisAlignment: CrossAxisAlignment.start,
                                  children: [
                                    const SizedBox(height: 6),
                                    Text(
                                      body,
                                      style: GoogleFonts.inter(
                                        color: AppTheme.textSecondary,
                                        fontSize: 12,
                                        height: 1.4,
                                      ),
                                    ),
                                    const SizedBox(height: 10),
                                    Text(
                                      '$dateLabel @ $timeLabel',
                                      style: GoogleFonts.inter(
                                        color: AppTheme.textSecondary.withOpacity(0.5),
                                        fontSize: 10,
                                      ),
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
