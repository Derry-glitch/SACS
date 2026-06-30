import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:provider/provider.dart';
import '../providers/auth_provider.dart';
import '../services/api_service.dart';
import '../core/theme/app_theme.dart';

class IdVerificationScreen extends StatefulWidget {
  const IdVerificationScreen({super.key});

  @override
  State<IdVerificationScreen> createState() => _IdVerificationScreenState();
}

class _IdVerificationScreenState extends State<IdVerificationScreen> {
  final ApiService _apiService = ApiService();
  final _formKey = GlobalKey<FormState>();
  final _matricController = TextEditingController();

  bool _isChecking = false;
  bool _hasSearched = false;
  bool _isVerified = false;

  // Verified details
  String? _matricNumber;
  String? _firstName;
  String? _lastName;
  String? _department;
  int? _academicLevel;
  String? _institutionName;
  String? _profileImageUrl;

  @override
  void dispose() {
    _matricController.dispose();
    super.dispose();
  }

  Future<void> _verifyMatric(String matric) async {
    if (matric.trim().isEmpty) return;

    setState(() {
      _isChecking = true;
      _hasSearched = false;
    });

    try {
      final data = await _apiService.verifyStudentId(matric.trim());
      setState(() {
        _isVerified = data['isVerified'] as bool? ?? false;
        if (_isVerified) {
          _matricNumber = data['matriculationNumber'] as String?;
          _firstName = data['firstName'] as String?;
          _lastName = data['lastName'] as String?;
          _department = data['department'] as String?;
          _academicLevel = data['academicLevel'] as int?;
          _institutionName = data['institutionName'] as String?;
          _profileImageUrl = data['profileImageUrl'] as String?;
        }
        _hasSearched = true;
        _isChecking = false;
      });
    } catch (e) {
      setState(() {
        _isVerified = false;
        _hasSearched = true;
        _isChecking = false;
      });
    }
  }

  void _simulateScan() {
    final user = context.read<AuthProvider>().user;
    if (user != null && user.matriculationNumber != null) {
      _matricController.text = user.matriculationNumber!;
      _verifyMatric(user.matriculationNumber!);
    } else {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('No student logged in to simulate.')),
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: Text(
          'ID Verification Portal',
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
                    'Invigilator Control Panel',
                    style: GoogleFonts.outfit(
                      color: AppTheme.textPrimary,
                      fontSize: 22,
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                  const SizedBox(height: 8),
                  Text(
                    'Enter or simulate scanning a student\'s matriculation number to perform an instant database verification audit.',
                    style: GoogleFonts.inter(
                      color: AppTheme.textSecondary,
                      fontSize: 13,
                      height: 1.4,
                    ),
                  ),
                  const SizedBox(height: 32),

                  // Input controls
                  Text(
                    'Matriculation Number',
                    style: GoogleFonts.outfit(
                      color: AppTheme.textSecondary,
                      fontSize: 14,
                      fontWeight: FontWeight.w600,
                    ),
                  ),
                  const SizedBox(height: 8),
                  TextFormField(
                    controller: _matricController,
                    decoration: InputDecoration(
                      hintText: 'e.g., U1234567',
                      hintStyle: TextStyle(color: AppTheme.textSecondary.withOpacity(0.5)),
                      filled: true,
                      fillColor: AppTheme.bgDarkSecondary,
                      contentPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 16),
                      enabledBorder: OutlineInputBorder(
                        borderRadius: BorderRadius.circular(12),
                        borderSide: BorderSide(color: Colors.white.withOpacity(0.08)),
                      ),
                      focusedBorder: OutlineInputBorder(
                        borderRadius: BorderRadius.circular(12),
                        borderSide: const BorderSide(color: AppTheme.primaryLight),
                      ),
                    ),
                    style: const TextStyle(color: AppTheme.textPrimary),
                    validator: (val) => val == null || val.trim().isEmpty ? 'Enter matriculation number' : null,
                  ),
                  const SizedBox(height: 20),

                  // Verify / Action buttons
                  Row(
                    children: [
                      Expanded(
                        child: SizedBox(
                          height: 50,
                          child: ElevatedButton.icon(
                            onPressed: _isChecking ? null : () => _verifyMatric(_matricController.text),
                            icon: _isChecking
                                ? const SizedBox(
                                    width: 20,
                                    height: 20,
                                    child: CircularProgressIndicator(color: Colors.white, strokeWidth: 2),
                                  )
                                : const Icon(Icons.verified_outlined, size: 20),
                            label: Text(
                              'Verify Student ID',
                              style: GoogleFonts.outfit(fontWeight: FontWeight.bold),
                            ),
                            style: ElevatedButton.styleFrom(
                              backgroundColor: AppTheme.primaryLight,
                              foregroundColor: Colors.white,
                              shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
                            ),
                          ),
                        ),
                      ),
                      const SizedBox(width: 12),
                      SizedBox(
                        height: 50,
                        child: OutlinedButton.icon(
                          onPressed: _isChecking ? null : _simulateScan,
                          icon: const Icon(Icons.qr_code_scanner_rounded, size: 20, color: AppTheme.accent),
                          label: Text(
                            'Simulate Scan',
                            style: GoogleFonts.outfit(color: AppTheme.accent, fontWeight: FontWeight.bold),
                          ),
                          style: OutlinedButton.styleFrom(
                            side: const BorderSide(color: AppTheme.accent, width: 1.5),
                            shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
                          ),
                        ),
                      ),
                    ],
                  ),
                  const SizedBox(height: 36),

                  // Verification State Card Rendering
                  if (_hasSearched) ...[
                    if (_isVerified) ...[
                      // VERIFIED SUCCESS CARD
                      Container(
                        width: double.infinity,
                        padding: const EdgeInsets.all(24.0),
                        decoration: BoxDecoration(
                          color: AppTheme.bgDarkSecondary,
                          borderRadius: BorderRadius.circular(20),
                          border: Border.all(color: AppTheme.success, width: 2),
                          boxShadow: [
                            BoxShadow(
                              color: AppTheme.success.withOpacity(0.06),
                              blurRadius: 16,
                            )
                          ],
                        ),
                        child: Column(
                          children: [
                            const Icon(
                              Icons.check_circle_rounded,
                              color: AppTheme.success,
                              size: 54,
                            ),
                            const SizedBox(height: 12),
                            Text(
                              'ACCESS GRANTED',
                              style: GoogleFonts.outfit(
                                color: AppTheme.success,
                                fontSize: 18,
                                fontWeight: FontWeight.bold,
                                letterSpacing: 1.2,
                              ),
                            ),
                            Text(
                              'Student details matched database record',
                              style: GoogleFonts.inter(
                                color: AppTheme.textSecondary,
                                fontSize: 11,
                              ),
                            ),
                            const Divider(height: 36, color: Colors.white12),

                            // Student Profile Photo Placeholder
                            Container(
                              width: 80,
                              height: 80,
                              decoration: BoxDecoration(
                                shape: BoxShape.circle,
                                border: Border.all(color: AppTheme.primaryLight, width: 1.5),
                                color: AppTheme.bgDark,
                              ),
                              child: _profileImageUrl != null
                                  ? ClipRRect(
                                      borderRadius: BorderRadius.circular(40),
                                      child: Image.network(_profileImageUrl!, fit: BoxFit.cover),
                                    )
                                  : Center(
                                      child: Text(
                                        '${_firstName?.substring(0, 1) ?? ""}${_lastName?.substring(0, 1) ?? ""}',
                                        style: GoogleFonts.outfit(
                                          color: AppTheme.textPrimary,
                                          fontSize: 26,
                                          fontWeight: FontWeight.bold,
                                        ),
                                      ),
                                    ),
                            ),
                            const SizedBox(height: 16),

                            // Detail Rows
                            _buildInfoRow('Full Name', '$_firstName $_lastName'),
                            const SizedBox(height: 12),
                            _buildInfoRow('Matric No.', _matricNumber ?? ''),
                            const SizedBox(height: 12),
                            _buildInfoRow('Department', _department ?? ''),
                            const SizedBox(height: 12),
                            _buildInfoRow('Academic Level', '${_academicLevel ?? 100} Level'),
                            const SizedBox(height: 12),
                            _buildInfoRow('Institution', _institutionName ?? ''),
                          ],
                        ),
                      )
                    ] else ...[
                      // VERIFIED FAILURE CARD
                      Container(
                        width: double.infinity,
                        padding: const EdgeInsets.all(24.0),
                        decoration: BoxDecoration(
                          color: AppTheme.bgDarkSecondary,
                          borderRadius: BorderRadius.circular(20),
                          border: Border.all(color: AppTheme.error, width: 2),
                          boxShadow: [
                            BoxShadow(
                              color: AppTheme.error.withOpacity(0.06),
                              blurRadius: 16,
                            )
                          ],
                        ),
                        child: Column(
                          children: [
                            const Icon(
                              Icons.cancel_rounded,
                              color: AppTheme.error,
                              size: 54,
                            ),
                            const SizedBox(height: 12),
                            Text(
                              'ACCESS DENIED',
                              style: GoogleFonts.outfit(
                                color: AppTheme.error,
                                fontSize: 18,
                                fontWeight: FontWeight.bold,
                                letterSpacing: 1.2,
                              ),
                            ),
                            const SizedBox(height: 6),
                            Text(
                              'No matching active student profile found for this matriculation number in SACS.',
                              style: GoogleFonts.inter(
                                color: AppTheme.textSecondary,
                                fontSize: 12,
                              ),
                              textAlign: TextAlign.center,
                            ),
                          ],
                        ),
                      )
                    ]
                  ],
                ],
              ),
            ),
          ),
        ),
      ),
    );
  }

  Widget _buildInfoRow(String label, String value) {
    return Row(
      mainAxisAlignment: MainAxisAlignment.spaceBetween,
      children: [
        Text(
          label,
          style: GoogleFonts.inter(color: AppTheme.textSecondary, fontSize: 13),
        ),
        Text(
          value,
          style: GoogleFonts.inter(color: AppTheme.textPrimary, fontSize: 13, fontWeight: FontWeight.bold),
        ),
      ],
    );
  }
}
