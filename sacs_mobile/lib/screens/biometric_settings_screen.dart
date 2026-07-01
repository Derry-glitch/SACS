import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:provider/provider.dart';
import '../services/biometric_service.dart';
import '../services/storage_service.dart';
import '../providers/theme_provider.dart';
import '../core/theme/app_theme.dart';

class BiometricSettingsScreen extends StatefulWidget {
  const BiometricSettingsScreen({super.key});

  @override
  State<BiometricSettingsScreen> createState() => _BiometricSettingsScreenState();
}

class _BiometricSettingsScreenState extends State<BiometricSettingsScreen> {
  final BiometricService _biometricService = BiometricService();
  final StorageService _storageService = StorageService();

  bool _canCheckBiometrics = false;
  bool _biometricsEnabled = false;
  bool _pinEnabled = false;

  @override
  void initState() {
    super.initState();
    _loadSettings();
  }

  Future<void> _loadSettings() async {
    final canCheck = await _biometricService.canCheckBiometrics();
    final bioEnabled = await _biometricService.isBiometricsEnabled();
    final pinEnabled = await _biometricService.isPinEnabled();

    setState(() {
      _canCheckBiometrics = canCheck;
      _biometricsEnabled = bioEnabled;
      _pinEnabled = pinEnabled;
    });
  }

  Future<void> _toggleBiometrics(bool value) async {
    if (value) {
      // Authenticate first before enabling
      final success = await _biometricService.authenticate('Confirm biometric authorization identity');
      if (!success) {
        if (mounted) {
          ScaffoldMessenger.of(context).showSnackBar(
            const SnackBar(
              content: Text('Biometric verification failed.'),
              backgroundColor: AppTheme.error,
            ),
          );
        }
        return;
      }
    }

    await _biometricService.setBiometricsEnabled(value);
    setState(() {
      _biometricsEnabled = value;
    });

    if (mounted) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text(value ? 'Biometric login enabled.' : 'Biometric login disabled.'),
          backgroundColor: AppTheme.success,
        ),
      );
    }
  }

  Future<void> _togglePin(bool value) async {
    if (value) {
      // Setup PIN Dialog/Prompt
      final pin = await _showSetupPinDialog();
      if (pin == null || pin.length != 4) return;

      await _storageService.savePin(pin);
      await _biometricService.setPinEnabled(true);
      setState(() {
        _pinEnabled = true;
      });

      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text('Security PIN successfully enabled.'),
            backgroundColor: AppTheme.success,
          ),
        );
      }
    } else {
      // Disable PIN
      await _storageService.clearPin();
      await _biometricService.setPinEnabled(false);
      setState(() {
        _pinEnabled = false;
      });

      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text('Security PIN disabled.'),
            backgroundColor: AppTheme.error,
          ),
        );
      }
    }
  }

  Future<String?> _showSetupPinDialog() async {
    final pinController1 = TextEditingController();
    final pinController2 = TextEditingController();
    final formKey = GlobalKey<FormState>();

    return showDialog<String>(
      context: context,
      barrierDismissible: false,
      builder: (context) {
        return AlertDialog(
          backgroundColor: AppTheme.bgDarkSecondary,
          title: Text(
            'Setup Secure PIN',
            style: GoogleFonts.outfit(color: AppTheme.textPrimary, fontWeight: FontWeight.bold),
          ),
          content: Form(
            key: formKey,
            child: Column(
              mainAxisSize: MainAxisSize.min,
              children: [
                Text(
                  'Create a 4-digit PIN to secure your local access when offline.',
                  style: GoogleFonts.inter(color: AppTheme.textSecondary, fontSize: 13),
                ),
                const SizedBox(height: 16),
                TextFormField(
                  controller: pinController1,
                  obscureText: true,
                  keyboardType: TextInputType.number,
                  maxLength: 4,
                  style: GoogleFonts.outfit(color: Colors.white, letterSpacing: 8, fontSize: 18),
                  decoration: const InputDecoration(
                    labelText: 'Enter 4-digit PIN',
                    counterText: '',
                  ),
                  validator: (val) {
                    if (val == null || val.length != 4 || int.tryParse(val) == null) {
                      return 'Must be exactly 4 digits';
                    }
                    return null;
                  },
                ),
                const SizedBox(height: 12),
                TextFormField(
                  controller: pinController2,
                  obscureText: true,
                  keyboardType: TextInputType.number,
                  maxLength: 4,
                  style: GoogleFonts.outfit(color: Colors.white, letterSpacing: 8, fontSize: 18),
                  decoration: const InputDecoration(
                    labelText: 'Confirm 4-digit PIN',
                    counterText: '',
                  ),
                  validator: (val) {
                    if (val != pinController1.text) {
                      return 'PIN codes must match';
                    }
                    return null;
                  },
                ),
              ],
            ),
          ),
          actions: [
            TextButton(
              onPressed: () => Navigator.pop(context, null),
              child: const Text('Cancel', style: TextStyle(color: AppTheme.textSecondary)),
            ),
            ElevatedButton(
              onPressed: () {
                if (formKey.currentState!.validate()) {
                  Navigator.pop(context, pinController1.text);
                }
              },
              style: ElevatedButton.styleFrom(backgroundColor: AppTheme.primaryLight),
              child: const Text('Save PIN'),
            ),
          ],
        );
      },
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: Text(
          'Security Settings',
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
          child: ListView(
            padding: const EdgeInsets.all(24.0),
            children: [
              Text(
                'Hardening & Protection',
                style: GoogleFonts.outfit(
                  color: AppTheme.textPrimary,
                  fontSize: 20,
                  fontWeight: FontWeight.bold,
                ),
              ),
              const SizedBox(height: 8),
              Text(
                'Configure biological authentication and local offline security measures to protect your student register credentials.',
                style: GoogleFonts.inter(
                  color: AppTheme.textSecondary,
                  fontSize: 13,
                  height: 1.4,
                ),
              ),
              const SizedBox(height: 24),
              
              // Biometrics Settings Tile
              Container(
                padding: const EdgeInsets.all(16),
                decoration: BoxDecoration(
                  color: AppTheme.bgDarkSecondary,
                  borderRadius: BorderRadius.circular(16),
                  border: Border.all(color: Colors.white.withOpacity(0.04)),
                ),
                child: Row(
                  children: [
                    Container(
                      padding: const EdgeInsets.all(10),
                      decoration: BoxDecoration(
                        color: AppTheme.primaryLight.withOpacity(0.12),
                        shape: BoxShape.circle,
                      ),
                      child: const Icon(Icons.fingerprint_rounded, color: AppTheme.primaryLight, size: 24),
                    ),
                    const SizedBox(width: 16),
                    Expanded(
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Text(
                            'Biometric Access',
                            style: GoogleFonts.outfit(
                              color: AppTheme.textPrimary,
                              fontWeight: FontWeight.bold,
                              fontSize: 15,
                            ),
                          ),
                          const SizedBox(height: 2),
                          Text(
                            _canCheckBiometrics
                                ? 'Use Face ID / Fingerprint to sign in.'
                                : 'Hardware biometrics not available on this device.',
                            style: GoogleFonts.inter(
                              color: AppTheme.textSecondary,
                              fontSize: 11,
                            ),
                          ),
                        ],
                      ),
                    ),
                    Switch(
                      value: _biometricsEnabled,
                      onChanged: _canCheckBiometrics ? _toggleBiometrics : null,
                      activeColor: AppTheme.primaryLight,
                    ),
                  ],
                ),
              ),
              const SizedBox(height: 16),

              // PIN Lock Settings Tile
              Container(
                padding: const EdgeInsets.all(16),
                decoration: BoxDecoration(
                  color: AppTheme.bgDarkSecondary,
                  borderRadius: BorderRadius.circular(16),
                  border: Border.all(color: Colors.white.withOpacity(0.04)),
                ),
                child: Row(
                  children: [
                    Container(
                      padding: const EdgeInsets.all(10),
                      decoration: BoxDecoration(
                        color: AppTheme.accent.withOpacity(0.12),
                        shape: BoxShape.circle,
                      ),
                      child: const Icon(Icons.lock_person_rounded, color: AppTheme.accent, size: 24),
                    ),
                    const SizedBox(width: 16),
                    Expanded(
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Text(
                            'Local Screen PIN Lock',
                            style: GoogleFonts.outfit(
                              color: AppTheme.textPrimary,
                              fontWeight: FontWeight.bold,
                              fontSize: 15,
                            ),
                          ),
                          const SizedBox(height: 2),
                          Text(
                            'Secure app unlock with a local 4-digit security code.',
                            style: GoogleFonts.inter(
                              color: AppTheme.textSecondary,
                              fontSize: 11,
                            ),
                          ),
                        ],
                      ),
                    ),
                    Switch(
                      value: _pinEnabled,
                      onChanged: _togglePin,
                      activeColor: AppTheme.accent,
                    ),
                  ],
                ),
              ),
              const SizedBox(height: 16),
              Container(
                padding: const EdgeInsets.all(16),
                decoration: BoxDecoration(
                  color: AppTheme.bgDarkSecondary,
                  borderRadius: BorderRadius.circular(16),
                  border: Border.all(color: Colors.white.withOpacity(0.04)),
                ),
                child: Row(
                  children: [
                    Container(
                      padding: const EdgeInsets.all(10),
                      decoration: BoxDecoration(
                        color: AppTheme.primaryLight.withOpacity(0.12),
                        shape: BoxShape.circle,
                      ),
                      child: const Icon(Icons.brightness_4_rounded, color: AppTheme.primaryLight, size: 24),
                    ),
                    const SizedBox(width: 16),
                    Expanded(
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Text(
                            'Dark Mode',
                            style: GoogleFonts.outfit(
                              color: AppTheme.textPrimary,
                              fontWeight: FontWeight.bold,
                              fontSize: 15,
                            ),
                          ),
                          const SizedBox(height: 2),
                          Text(
                            'Enable SACS premium dark styling.',
                            style: GoogleFonts.inter(
                              color: AppTheme.textSecondary,
                              fontSize: 11,
                            ),
                          ),
                        ],
                      ),
                    ),
                    Switch(
                      value: context.watch<ThemeProvider>().isDarkMode,
                      onChanged: (val) {
                        context.read<ThemeProvider>().toggleTheme(val);
                      },
                      activeColor: AppTheme.primaryLight,
                    ),
                  ],
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
