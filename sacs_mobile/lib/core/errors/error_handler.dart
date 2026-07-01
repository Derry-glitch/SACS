import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';
import '../theme/app_theme.dart';

class ErrorHandler {
  static String getErrorMessage(dynamic error) {
    if (error == null) return 'An unknown error occurred.';
    
    final str = error.toString();
    if (str.contains('SocketException') || str.contains('NetworkResource') || str.contains('Failed host lookup')) {
      return 'No internet connection. Please verify your connection and try again.';
    }
    if (str.contains('Unauthorized') || str.contains('401')) {
      return 'Your session has expired. Please sign in again.';
    }
    if (str.contains('Forbidden') || str.contains('403')) {
      return 'You do not have permission to access this resource.';
    }
    if (str.contains('500') || str.contains('Internal Server Error')) {
      return 'Server error occurred. Our engineers have been notified.';
    }
    
    return str.replaceAll('Failure: ', '').replaceAll('Exception: ', '');
  }

  static void showErrorDialog(
    BuildContext context,
    dynamic error, {
    VoidCallback? onRetry,
  }) {
    final message = getErrorMessage(error);

    showDialog(
      context: context,
      barrierDismissible: onRetry == null,
      builder: (context) {
        return AlertDialog(
          backgroundColor: AppTheme.bgDarkSecondary,
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(20),
            side: BorderSide(color: Colors.white.withOpacity(0.08)),
          ),
          title: Row(
            children: [
              const Icon(Icons.error_outline_rounded, color: AppTheme.error, size: 28),
              const SizedBox(width: 12),
              Text(
                'Access Error',
                style: GoogleFonts.outfit(
                  color: AppTheme.textPrimary,
                  fontWeight: FontWeight.bold,
                  fontSize: 18,
                ),
              ),
            ],
          ),
          content: Text(
            message,
            style: GoogleFonts.inter(
              color: AppTheme.textSecondary,
              fontSize: 14,
              height: 1.4,
            ),
          ),
          actions: [
            if (onRetry != null)
              TextButton(
                onPressed: () {
                  Navigator.pop(context);
                },
                child: Text(
                  'Cancel',
                  style: GoogleFonts.inter(color: AppTheme.textSecondary),
                ),
              ),
            ElevatedButton(
              onPressed: () {
                Navigator.pop(context);
                if (onRetry != null) {
                  onRetry();
                }
              },
              style: ElevatedButton.styleFrom(
                backgroundColor: onRetry != null ? AppTheme.primaryLight : AppTheme.error,
                shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(12),
                ),
                padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 10),
              ),
              child: Text(
                onRetry != null ? 'Retry Connection' : 'Acknowledge',
                style: GoogleFonts.inter(
                  color: Colors.white,
                  fontWeight: FontWeight.w600,
                  fontSize: 13,
                ),
              ),
            ),
          ],
        );
      },
    );
  }
}
