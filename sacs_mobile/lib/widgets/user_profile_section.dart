import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:provider/provider.dart';
import 'package:go_router/go_router.dart';
import '../models/user_model.dart';
import '../providers/auth_provider.dart';
import '../core/theme/app_theme.dart';

class UserProfileSection extends StatelessWidget {
  final UserModel user;

  const UserProfileSection({super.key, required this.user});

  @override
  Widget build(BuildContext context) {
    return Card(
      child: Padding(
        padding: const EdgeInsets.all(16.0),
        child: Row(
          children: [
            CircleAvatar(
              radius: 28,
              backgroundColor: AppTheme.primaryLight.withOpacity(0.15),
              child: const Icon(
                Icons.person_rounded,
                size: 28,
                color: AppTheme.primaryLight,
              ),
            ),
            const SizedBox(width: 16),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    '${user.firstName} ${user.lastName}',
                    style: GoogleFonts.outfit(
                      fontSize: 16,
                      fontWeight: FontWeight.w600,
                      color: AppTheme.textPrimary,
                    ),
                  ),
                  const SizedBox(height: 2),
                  Text(
                    user.email,
                    style: GoogleFonts.inter(
                      fontSize: 12,
                      color: AppTheme.textSecondary,
                    ),
                    maxLines: 1,
                    overflow: TextOverflow.ellipsis,
                  ),
                ],
              ),
            ),
            IconButton(
              onPressed: () {
                context.push('/biometric-settings');
              },
              icon: const Icon(Icons.security_rounded, color: AppTheme.primaryLight),
              tooltip: 'Security Settings',
            ),
            IconButton(
              onPressed: () {
                // Logout flow
                context.read<AuthProvider>().logout();
              },
              icon: const Icon(Icons.logout_rounded, color: AppTheme.error),
              tooltip: 'Logout',
            ),
          ],
        ),
      ),
    );
  }
}
