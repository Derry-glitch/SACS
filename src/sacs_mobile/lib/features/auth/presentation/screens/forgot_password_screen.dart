import 'package:flutter/material.dart';
import '../../../../core/theme/app_theme.dart';

class ForgotPasswordScreen extends StatefulWidget {
  const ForgotPasswordScreen({super.key});

  @override
  State<ForgotPasswordScreen> createState() => _ForgotPasswordScreenState();
}

class _ForgotPasswordScreenState extends State<ForgotPasswordScreen> {
  final _formKey = GlobalKey<FormState>();
  final _emailController = TextEditingController();
  bool _isSuccess = false;

  @override
  void dispose() {
    _emailController.dispose();
    super.dispose();
  }

  void _submit() {
    if (_formKey.currentState!.validate()) {
      // Simulate backend trigger
      setState(() {
        _isSuccess = true;
      });
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Reset Password'),
        backgroundColor: Colors.transparent,
        elevation: 0,
      ),
      extendBodyBehindAppBar: true,
      body: Container(
        decoration: const BoxDecoration(
          gradient: LinearGradient(
            begin: Alignment.topLeft,
            end: Alignment.bottomRight,
            colors: [
              AppTheme.bgDark,
              Color(0xFF1E1E38),
            ],
          ),
        ),
        child: SafeArea(
          child: Padding(
            padding: const EdgeInsets.symmetric(horizontal: 24.0),
            child: _isSuccess
                ? Column(
                    mainAxisAlignment: MainAxisAlignment.center,
                    crossAxisAlignment: CrossAxisAlignment.stretch,
                    children: [
                      const Icon(
                        Icons.mark_email_read_outlined,
                        size: 80,
                        color: AppTheme.success,
                      ),
                      const SizedBox(height: 24),
                      Text(
                        'Email Sent',
                        textAlign: TextAlign.center,
                        style: Theme.of(context).textTheme.headlineMedium?.copyWith(
                              fontWeight: FontWeight.bold,
                            ),
                      ),
                      const SizedBox(height: 12),
                      Text(
                        'We have sent a password reset link to ${_emailController.text}. Please check your inbox.',
                        textAlign: TextAlign.center,
                        style: Theme.of(context).textTheme.bodyMedium,
                      ),
                      const SizedBox(height: 36),
                      ElevatedButton(
                        onPressed: () => Navigator.of(context).pop(),
                        style: ElevatedButton.styleFrom(
                          backgroundColor: AppTheme.primaryLight,
                          foregroundColor: AppTheme.textPrimary,
                          padding: const EdgeInsets.symmetric(vertical: 16),
                          shape: RoundedRectangleBorder(
                            borderRadius: BorderRadius.circular(12),
                          ),
                        ),
                        child: const Text('Back to Login'),
                      ),
                    ],
                  )
                : Form(
                    key: _formKey,
                    child: Column(
                      mainAxisAlignment: MainAxisAlignment.center,
                      crossAxisAlignment: CrossAxisAlignment.stretch,
                      children: [
                        Text(
                          'Trouble Signing In?',
                          textAlign: TextAlign.center,
                          style: Theme.of(context).textTheme.headlineMedium?.copyWith(
                                fontWeight: FontWeight.bold,
                              ),
                        ),
                        const SizedBox(height: 12),
                        Text(
                          'Enter your email address and we will send you a secure link to reset your account password.',
                          textAlign: TextAlign.center,
                          style: Theme.of(context).textTheme.bodyMedium,
                        ),
                        const SizedBox(height: 36),
                        TextFormField(
                          controller: _emailController,
                          keyboardType: TextInputType.emailAddress,
                          validator: (value) {
                            if (value == null || value.isEmpty) {
                              return 'Please enter your email.';
                            }
                            if (!value.contains('@')) {
                              return 'Please enter a valid email.';
                            }
                            return null;
                          },
                          decoration: const InputDecoration(
                            labelText: 'Email Address',
                            prefixIcon: Icon(Icons.email_outlined),
                          ),
                        ),
                        const SizedBox(height: 24),
                        ElevatedButton(
                          onPressed: _submit,
                          style: ElevatedButton.styleFrom(
                            backgroundColor: AppTheme.primaryLight,
                            foregroundColor: AppTheme.textPrimary,
                            padding: const EdgeInsets.symmetric(vertical: 16),
                            shape: RoundedRectangleBorder(
                              borderRadius: BorderRadius.circular(12),
                            ),
                          ),
                          child: const Text(
                            'Send Reset Link',
                            style: TextStyle(
                              fontSize: 16,
                              fontWeight: FontWeight.bold,
                            ),
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
}
