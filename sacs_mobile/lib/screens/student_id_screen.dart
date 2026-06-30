import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:provider/provider.dart';
import 'package:qr_flutter/qr_flutter.dart';
import '../providers/auth_provider.dart';
import '../services/api_service.dart';
import '../core/theme/app_theme.dart';

class StudentIdScreen extends StatefulWidget {
  const StudentIdScreen({super.key});

  @override
  State<StudentIdScreen> createState() => _StudentIdScreenState();
}

class _StudentIdScreenState extends State<StudentIdScreen> {
  final ApiService _apiService = ApiService();
  bool _isLoading = true;
  String? _errorMessage;

  // Local verified student details
  String? _matricNumber;
  String? _firstName;
  String? _lastName;
  String? _department;
  int? _academicLevel;
  String? _institutionName;
  String? _profileImageUrl;

  @override
  void initState() {
    super.initState();
    _fetchStudentDetails();
  }

  Future<void> _fetchStudentDetails() async {
    final user = context.read<AuthProvider>().user;
    if (user == null || user.matriculationNumber == null) {
      setState(() {
        _errorMessage = 'Matriculation number not found. Please log in again.';
        _isLoading = false;
      });
      return;
    }

    try {
      final data = await _apiService.verifyStudentId(user.matriculationNumber!);
      setState(() {
        _matricNumber = data['matriculationNumber'] as String?;
        _firstName = data['firstName'] as String?;
        _lastName = data['lastName'] as String?;
        _department = data['department'] as String?;
        _academicLevel = data['academicLevel'] as int?;
        _institutionName = data['institutionName'] as String?;
        _profileImageUrl = data['profileImageUrl'] as String?;
        _isLoading = false;
      });
    } catch (e) {
      // Fallback to local profile info if the API call fails or is disconnected
      setState(() {
        _matricNumber = user.matriculationNumber;
        _firstName = user.firstName;
        _lastName = user.lastName;
        _department = 'Computer Science';
        _academicLevel = 200;
        _institutionName = 'SACS Academy';
        _profileImageUrl = null;
        _isLoading = false;
      });
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: Text(
          'Digital Student ID',
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
          child: _isLoading
              ? const Center(child: CircularProgressIndicator(color: AppTheme.primaryLight))
              : _errorMessage != null
                  ? Center(
                      child: Text(
                        _errorMessage!,
                        style: const TextStyle(color: AppTheme.error),
                      ),
                    )
                  : Center(
                      child: SingleChildScrollView(
                        padding: const EdgeInsets.symmetric(horizontal: 24.0, vertical: 16.0),
                        child: Column(
                          mainAxisAlignment: MainAxisAlignment.center,
                          children: [
                            // Premium Student ID Card Container
                            Container(
                              width: double.infinity,
                              constraints: const BoxConstraints(maxWidth: 350),
                              decoration: BoxDecoration(
                                gradient: LinearGradient(
                                  colors: [
                                    AppTheme.bgDarkSecondary,
                                    AppTheme.bgDarkSecondary.withBlue(65),
                                  ],
                                  begin: Alignment.topLeft,
                                  end: Alignment.bottomRight,
                                ),
                                borderRadius: BorderRadius.circular(24),
                                border: Border.all(
                                  color: AppTheme.primaryLight.withOpacity(0.3),
                                  width: 1.5,
                                ),
                                boxShadow: [
                                  BoxShadow(
                                    color: AppTheme.primaryLight.withOpacity(0.08),
                                    blurRadius: 24,
                                    offset: const Offset(0, 8),
                                  )
                                ],
                              ),
                              child: Column(
                                children: [
                                  // Card Top Header
                                  Padding(
                                    padding: const EdgeInsets.all(20.0),
                                    child: Row(
                                      mainAxisAlignment: MainAxisAlignment.spaceBetween,
                                      children: [
                                        Expanded(
                                          child: Column(
                                            crossAxisAlignment: CrossAxisAlignment.start,
                                            children: [
                                              Text(
                                                _institutionName?.toUpperCase() ?? 'SACS INSTITUTION',
                                                style: GoogleFonts.outfit(
                                                  color: AppTheme.primaryLight,
                                                  fontWeight: FontWeight.bold,
                                                  fontSize: 14,
                                                  letterSpacing: 1.2,
                                                ),
                                                overflow: TextOverflow.ellipsis,
                                              ),
                                              const SizedBox(height: 2),
                                              Text(
                                                'Digital Student ID Card',
                                                style: GoogleFonts.inter(
                                                  color: AppTheme.textSecondary,
                                                  fontSize: 10,
                                                  fontWeight: FontWeight.w500,
                                                ),
                                              ),
                                            ],
                                          ),
                                        ),
                                        const Icon(
                                          Icons.verified_user_rounded,
                                          color: AppTheme.success,
                                          size: 26,
                                        ),
                                      ],
                                    ),
                                  ),
                                  const Divider(height: 1, color: Colors.white12),

                                  // Student Details Section
                                  Padding(
                                    padding: const EdgeInsets.symmetric(horizontal: 20.0, vertical: 24.0),
                                    child: Column(
                                      children: [
                                        // Student Photo Placeholder
                                        Stack(
                                          alignment: Alignment.center,
                                          children: [
                                            Container(
                                              width: 100,
                                              height: 100,
                                              decoration: BoxDecoration(
                                                shape: BoxShape.circle,
                                                border: Border.all(color: AppTheme.primaryLight, width: 2),
                                                color: AppTheme.bgDark,
                                              ),
                                              child: _profileImageUrl != null
                                                  ? ClipRRect(
                                                      borderRadius: BorderRadius.circular(50),
                                                      child: Image.network(_profileImageUrl!, fit: BoxFit.cover),
                                                    )
                                                  : Center(
                                                      child: Text(
                                                        '${_firstName?.substring(0, 1) ?? ""}${_lastName?.substring(0, 1) ?? ""}',
                                                        style: GoogleFonts.outfit(
                                                          color: AppTheme.textPrimary,
                                                          fontSize: 32,
                                                          fontWeight: FontWeight.bold,
                                                        ),
                                                      ),
                                                    ),
                                            ),
                                          ],
                                        ),
                                        const SizedBox(height: 16),

                                        // Student Name
                                        Text(
                                          '$_firstName $_lastName',
                                          style: GoogleFonts.outfit(
                                            color: AppTheme.textPrimary,
                                            fontSize: 20,
                                            fontWeight: FontWeight.bold,
                                          ),
                                          textAlign: TextAlign.center,
                                        ),
                                        const SizedBox(height: 24),

                                        // Info Rows
                                        _buildCardRow('Matric No.', _matricNumber ?? 'N/A'),
                                        const SizedBox(height: 12),
                                        _buildCardRow('Department', _department ?? 'Computer Science'),
                                        const SizedBox(height: 12),
                                        _buildCardRow('Academic Level', '${_academicLevel ?? 100} Level'),
                                      ],
                                    ),
                                  ),

                                  // Card Bottom QR Code Section
                                  Container(
                                    width: double.infinity,
                                    padding: const EdgeInsets.symmetric(vertical: 24.0),
                                    decoration: BoxDecoration(
                                      color: Colors.white,
                                      borderRadius: const BorderRadius.vertical(bottom: Radius.circular(23)),
                                    ),
                                    child: Column(
                                      children: [
                                        // QR Code
                                        QrImageView(
                                          data: _matricNumber ?? 'SACS-STUDENT-ID',
                                          version: QrVersions.auto,
                                          size: 150.0,
                                          gapless: false,
                                          eyeStyle: const QrEyeStyle(
                                            eyeShape: QrEyeShape.square,
                                            color: Colors.black,
                                          ),
                                          dataModuleStyle: const QrDataModuleStyle(
                                            dataModuleShape: QrDataModuleShape.square,
                                            color: Colors.black,
                                          ),
                                        ),
                                        const SizedBox(height: 12),
                                        Text(
                                          'SCAN QR CODE TO VERIFY IDENTITY',
                                          style: GoogleFonts.inter(
                                            color: Colors.black54,
                                            fontSize: 10,
                                            fontWeight: FontWeight.bold,
                                            letterSpacing: 0.8,
                                          ),
                                        ),
                                      ],
                                    ),
                                  ),
                                ],
                              ),
                            ),
                          ],
                        ),
                      ),
                    ),
        ),
      ),
    );
  }

  Widget _buildCardRow(String label, String value) {
    return Row(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        SizedBox(
          width: 90,
          child: Text(
            label,
            style: GoogleFonts.inter(
              color: AppTheme.textSecondary,
              fontSize: 12,
              fontWeight: FontWeight.w500,
            ),
          ),
        ),
        const SizedBox(width: 8),
        Expanded(
          child: Text(
            value,
            style: GoogleFonts.inter(
              color: AppTheme.textPrimary,
              fontSize: 13,
              fontWeight: FontWeight.bold,
            ),
            overflow: TextOverflow.ellipsis,
            maxLines: 2,
          ),
        ),
      ],
    );
  }
}
