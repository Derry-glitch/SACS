import 'dart:async';
import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';
import '../core/theme/app_theme.dart';
import '../services/network_service.dart';

class RetryWidget extends StatefulWidget {
  final String errorMessage;
  final VoidCallback onRetry;

  const RetryWidget({
    super.key,
    required this.errorMessage,
    required this.onRetry,
  });

  @override
  State<RetryWidget> createState() => _RetryWidgetState();
}

class _RetryWidgetState extends State<RetryWidget> {
  final NetworkService _networkService = NetworkService();
  StreamSubscription? _subscription;
  bool _isRetrying = false;

  @override
  void initState() {
    super.initState();
    // Auto retry when network changes back to online
    _subscription = _networkService.onConnectivityChanged.listen((online) {
      if (online && !_isRetrying) {
        _handleRetry();
      }
    });
  }

  @override
  void dispose() {
    _subscription?.cancel();
    super.dispose();
  }

  void _handleRetry() {
    if (mounted) {
      setState(() {
        _isRetrying = true;
      });
    }

    widget.onRetry();

    // Reset button state after a short delay
    Future.delayed(const Duration(seconds: 2), () {
      if (mounted) {
        setState(() {
          _isRetrying = false;
        });
      }
    });
  }

  @override
  Widget build(BuildContext context) {
    return Center(
      child: Padding(
        padding: const EdgeInsets.all(28.0),
        child: Container(
          padding: const EdgeInsets.symmetric(horizontal: 24, vertical: 32),
          decoration: BoxDecoration(
            color: AppTheme.bgDarkSecondary,
            borderRadius: BorderRadius.circular(20),
            border: Border.all(
              color: Colors.white.withOpacity(0.06),
            ),
          ),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              Container(
                padding: const EdgeInsets.all(16),
                decoration: BoxDecoration(
                  color: AppTheme.error.withOpacity(0.12),
                  shape: BoxShape.circle,
                ),
                child: const Icon(
                  Icons.wifi_off_rounded,
                  color: AppTheme.error,
                  size: 40,
                ),
              ),
              const SizedBox(height: 20),
              Text(
                'Connection Problem',
                style: GoogleFonts.outfit(
                  color: Colors.white,
                  fontSize: 18,
                  fontWeight: FontWeight.bold,
                ),
              ),
              const SizedBox(height: 8),
              Text(
                widget.errorMessage,
                style: GoogleFonts.inter(
                  color: AppTheme.textSecondary,
                  fontSize: 13,
                ),
                textAlign: TextAlign.center,
              ),
              const SizedBox(height: 24),
              ElevatedButton.icon(
                onPressed: _isRetrying ? null : _handleRetry,
                style: ElevatedButton.styleFrom(
                  backgroundColor: AppTheme.primaryLight,
                  foregroundColor: Colors.white,
                  padding: const EdgeInsets.symmetric(horizontal: 28, vertical: 12),
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(12),
                  ),
                ),
                icon: _isRetrying
                    ? const SizedBox(
                        width: 18,
                        height: 18,
                        child: CircularProgressIndicator(
                          strokeWidth: 2,
                          color: Colors.white,
                        ),
                      )
                    : const Icon(Icons.refresh_rounded, size: 20),
                label: Text(
                  _isRetrying ? 'Retrying...' : 'Retry Connection',
                  style: GoogleFonts.inter(
                    fontWeight: FontWeight.bold,
                  ),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
