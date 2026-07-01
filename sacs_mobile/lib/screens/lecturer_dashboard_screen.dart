import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:provider/provider.dart';
import 'package:go_router/go_router.dart';
import '../providers/auth_provider.dart';
import '../core/theme/app_theme.dart';

class LecturerDashboardScreen extends StatelessWidget {
  const LecturerDashboardScreen({super.key});

  @override
  Widget build(BuildContext context) {
    final authProvider = context.watch<AuthProvider>();
    final user = authProvider.user;

    return Scaffold(
      appBar: AppBar(
        title: Text(
          'Lecturer Portal',
          style: GoogleFonts.outfit(fontWeight: FontWeight.bold),
        ),
        backgroundColor: AppTheme.bgDark,
        elevation: 0,
        actions: [
          IconButton(
            icon: const Icon(Icons.logout_rounded, color: AppTheme.error),
            onPressed: () async {
              await authProvider.logout();
            },
            tooltip: 'Log Out',
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
          child: SingleChildScrollView(
            padding: const EdgeInsets.all(24.0),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                // Greeting Header
                Text(
                  'Welcome, ${user?.firstName ?? 'Lecturer'}',
                  style: GoogleFonts.outfit(
                    fontSize: 28,
                    fontWeight: FontWeight.bold,
                    color: AppTheme.textPrimary,
                  ),
                ),
                const SizedBox(height: 4),
                Text(
                  'Smart Academic Companion System Management',
                  style: GoogleFonts.inter(
                    color: AppTheme.textSecondary,
                    fontSize: 14,
                  ),
                ),
                const SizedBox(height: 32),

                // Grid actions
                Text(
                  'Quick Management Actions',
                  style: GoogleFonts.outfit(
                    fontSize: 18,
                    fontWeight: FontWeight.bold,
                    color: AppTheme.textPrimary,
                  ),
                ),
                const SizedBox(height: 16),
                GridView.count(
                  shrinkWrap: true,
                  physics: const NeverScrollableScrollPhysics(),
                  crossAxisCount: 2,
                  crossAxisSpacing: 16,
                  mainAxisSpacing: 16,
                  children: [
                    _buildLecturerActionCard(
                      context: context,
                      icon: Icons.qr_code_scanner_rounded,
                      title: 'Start Attendance',
                      subtitle: 'Generate active code',
                      color: AppTheme.primaryLight,
                      onTap: () => context.push('/attendance-session'),
                    ),
                    _buildLecturerActionCard(
                      context: context,
                      icon: Icons.verified_user_rounded,
                      title: 'Verify Student ID',
                      subtitle: 'Scan and match profiles',
                      color: AppTheme.accent,
                      onTap: () => context.push('/verify-id'),
                    ),
                    _buildLecturerActionCard(
                      context: context,
                      icon: Icons.campaign_rounded,
                      title: 'Send Announcement',
                      subtitle: 'Broadcast alerts',
                      color: AppTheme.accent,
                      onTap: () => _showCreateAnnouncementDialog(context),
                    ),
                    _buildLecturerActionCard(
                      context: context,
                      icon: Icons.history_rounded,
                      title: 'View Attendance',
                      subtitle: 'Course registers',
                      color: AppTheme.success,
                      onTap: () => _showCourseSelectDialog(context),
                    ),
                  ],
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }

  Widget _buildLecturerActionCard({
    required BuildContext context,
    required IconData icon,
    required String title,
    required String subtitle,
    required Color color,
    required VoidCallback onTap,
  }) {
    return InkWell(
      onTap: onTap,
      borderRadius: BorderRadius.circular(16),
      child: Container(
        padding: const EdgeInsets.all(16),
        decoration: BoxDecoration(
          color: AppTheme.bgDarkSecondary,
          borderRadius: BorderRadius.circular(16),
          border: Border.all(color: Colors.white.withOpacity(0.06)),
        ),
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Container(
              padding: const EdgeInsets.all(8),
              decoration: BoxDecoration(
                color: color.withOpacity(0.12),
                shape: BoxShape.circle,
              ),
              child: Icon(icon, color: color, size: 28),
            ),
            const Spacer(),
            Text(
              title,
              style: GoogleFonts.outfit(
                color: AppTheme.textPrimary,
                fontWeight: FontWeight.bold,
                fontSize: 16,
              ),
            ),
            const SizedBox(height: 4),
            Text(
              subtitle,
              style: GoogleFonts.inter(
                color: AppTheme.textSecondary,
                fontSize: 11,
              ),
              maxLines: 2,
              overflow: TextOverflow.ellipsis,
            ),
          ],
        ),
      ),
    );
  }

  void _showCreateAnnouncementDialog(BuildContext context) {
    final titleController = TextEditingController();
    final messageController = TextEditingController();
    final deptController = TextEditingController();
    bool isUrgent = false;

    showDialog(
      context: context,
      builder: (context) {
        return StatefulBuilder(
          builder: (context, setState) {
            return AlertDialog(
              backgroundColor: AppTheme.bgDarkSecondary,
              title: Text(
                'Broadcast Announcement',
                style: GoogleFonts.outfit(color: AppTheme.textPrimary, fontWeight: FontWeight.bold),
              ),
              content: SingleChildScrollView(
                child: Column(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    TextField(
                      controller: titleController,
                      style: const TextStyle(color: AppTheme.textPrimary),
                      decoration: const InputDecoration(labelText: 'Title'),
                    ),
                    const SizedBox(height: 12),
                    TextField(
                      controller: messageController,
                      style: const TextStyle(color: AppTheme.textPrimary),
                      maxLines: 3,
                      decoration: const InputDecoration(labelText: 'Message Body'),
                    ),
                    const SizedBox(height: 12),
                    TextField(
                      controller: deptController,
                      style: const TextStyle(color: AppTheme.textPrimary),
                      decoration: const InputDecoration(labelText: 'Target Department (Optional)'),
                    ),
                    const SizedBox(height: 16),
                    Row(
                      children: [
                        Checkbox(
                          value: isUrgent,
                          activeColor: AppTheme.error,
                          onChanged: (val) {
                            setState(() {
                              isUrgent = val ?? false;
                            });
                          },
                        ),
                        Text(
                          'Mark as Urgent',
                          style: GoogleFonts.inter(color: AppTheme.textPrimary),
                        ),
                      ],
                    ),
                  ],
                ),
              ),
              actions: [
                TextButton(
                  onPressed: () => Navigator.pop(context),
                  child: const Text('Cancel', style: TextStyle(color: AppTheme.textSecondary)),
                ),
                ElevatedButton(
                  onPressed: () async {
                    if (titleController.text.isEmpty || messageController.text.isEmpty) {
                      return;
                    }
                    try {
                      final api = Provider.of<AuthProvider>(context, listen: false);
                      // Custom direct post using ApiService
                      final response = await authProvider.user != null
                          ? await api.user != null
                              ? await ApiService().lecturerCreateAnnouncement({
                                  'title': titleController.text,
                                  'message': messageController.text,
                                  'department': deptController.text.trim().isEmpty ? null : deptController.text.trim(),
                                  'priority': isUrgent ? 'Urgent' : 'Normal',
                                })
                              : null
                          : null;

                      if (context.mounted) {
                        Navigator.pop(context);
                        ScaffoldMessenger.of(context).showSnackBar(
                          const SnackBar(
                            content: Text('Announcement broadcasted successfully!'),
                            backgroundColor: AppTheme.success,
                          ),
                        );
                      }
                    } catch (e) {
                      if (context.mounted) {
                        ScaffoldMessenger.of(context).showSnackBar(
                          SnackBar(
                            content: Text('Failed: $e'),
                            backgroundColor: AppTheme.error,
                          ),
                        );
                      }
                    }
                  },
                  style: ElevatedButton.styleFrom(backgroundColor: AppTheme.primaryLight),
                  child: const Text('Broadcast'),
                ),
              ],
            );
          },
        );
      },
    );
  }

  void _showCourseSelectDialog(BuildContext context) {
    final courseIdController = TextEditingController();
    showDialog(
      context: context,
      builder: (context) {
        return AlertDialog(
          backgroundColor: AppTheme.bgDarkSecondary,
          title: Text(
            'Select Course offering',
            style: GoogleFonts.outfit(color: AppTheme.textPrimary, fontWeight: FontWeight.bold),
          ),
          content: TextField(
            controller: courseIdController,
            keyboardType: TextInputType.number,
            style: const TextStyle(color: AppTheme.textPrimary),
            decoration: const InputDecoration(labelText: 'Course or Offering ID'),
          ),
          actions: [
            TextButton(
              onPressed: () => Navigator.pop(context),
              child: const Text('Cancel', style: TextStyle(color: AppTheme.textSecondary)),
            ),
            ElevatedButton(
              onPressed: () {
                final id = int.tryParse(courseIdController.text);
                if (id != null) {
                  Navigator.pop(context);
                  _showAttendanceRecordsDialog(context, id);
                }
              },
              style: ElevatedButton.styleFrom(backgroundColor: AppTheme.success),
              child: const Text('View Register'),
            ),
          ],
        );
      },
    );
  }

  void _showAttendanceRecordsDialog(BuildContext context, int courseId) {
    showModalBottomSheet(
      context: context,
      backgroundColor: AppTheme.bgDarkSecondary,
      isScrollControlled: true,
      shape: const RoundedRectangleBorder(
        borderRadius: BorderRadius.vertical(top: Radius.circular(24)),
      ),
      builder: (context) {
        return Container(
          padding: const EdgeInsets.all(24),
          height: MediaQuery.of(context).size.height * 0.75,
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(
                'Attendance Register (ID: $courseId)',
                style: GoogleFonts.outfit(
                  fontSize: 20,
                  fontWeight: FontWeight.bold,
                  color: AppTheme.textPrimary,
                ),
              ),
              const SizedBox(height: 16),
              Expanded(
                child: FutureBuilder<List<dynamic>>(
                  future: ApiService().lecturerGetCourseAttendance(courseId),
                  builder: (context, snapshot) {
                    if (snapshot.connectionState == ConnectionState.waiting) {
                      return const Center(child: CircularProgressIndicator());
                    }
                    if (snapshot.hasError) {
                      return Center(
                        child: Text(
                          'Error: ${snapshot.error}',
                          style: const TextStyle(color: AppTheme.error),
                        ),
                      );
                    }
                    final list = snapshot.data ?? [];
                    if (list.isEmpty) {
                      return const Center(
                        child: Text(
                          'No attendance tracked for this course.',
                          style: TextStyle(color: AppTheme.textSecondary),
                        ),
                      );
                    }
                    return ListView.builder(
                      itemCount: list.length,
                      itemBuilder: (context, idx) {
                        final rec = list[idx];
                        final name = rec['studentName'] as String;
                        final status = rec['status'] as String;
                        final matric = rec['matriculationNumber'] as String;
                        final date = rec['date'] as String;

                        return Card(
                          color: AppTheme.bgDark,
                          margin: const EdgeInsets.only(bottom: 10),
                          child: ListTile(
                            title: Text(name, style: const TextStyle(color: AppTheme.textPrimary)),
                            subtitle: Text('$matric\n$date', style: const TextStyle(color: AppTheme.textSecondary, fontSize: 11)),
                            trailing: Container(
                              padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
                              decoration: BoxDecoration(
                                color: (status.toLowerCase() == 'present' ? AppTheme.success : AppTheme.error).withOpacity(0.12),
                                borderRadius: BorderRadius.circular(6),
                              ),
                              child: Text(
                                status.toUpperCase(),
                                style: TextStyle(
                                  color: status.toLowerCase() == 'present' ? AppTheme.success : AppTheme.error,
                                  fontSize: 10,
                                  fontWeight: FontWeight.bold,
                                ),
                              ),
                            ),
                          ),
                        );
                      },
                    );
                  },
                ),
              ),
            ],
          ),
        );
      },
    );
  }
}
