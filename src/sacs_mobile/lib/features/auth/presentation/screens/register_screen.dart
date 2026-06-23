import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../state/auth_provider.dart';
import '../../../../core/theme/app_theme.dart';

class RegisterScreen extends ConsumerStatefulWidget {
  const RegisterScreen({super.key});

  @override
  ConsumerState<RegisterScreen> createState() => _RegisterScreenState();
}

class _RegisterScreenState extends ConsumerState<RegisterScreen> {
  final _formKey = GlobalKey<FormState>();
  final _emailController = TextEditingController();
  final _passwordController = TextEditingController();
  final _firstNameController = TextEditingController();
  final _lastNameController = TextEditingController();
  final _matricNumberController = TextEditingController();
  final _phoneController = TextEditingController();
  
  int _academicLevel = 100;
  final int _institutionId = 1; // Default institution

  @override
  void dispose() {
    _emailController.dispose();
    _passwordController.dispose();
    _firstNameController.dispose();
    _lastNameController.dispose();
    _matricNumberController.dispose();
    _phoneController.dispose();
    super.dispose();
  }

  void _submit() {
    if (_formKey.currentState!.validate()) {
      ref.read(authProvider.notifier).register(
            email: _emailController.text.trim(),
            password: _passwordController.text,
            firstName: _firstNameController.text.trim(),
            lastName: _lastNameController.text.trim(),
            matriculationNumber: _matricNumberController.text.trim(),
            academicLevel: _academicLevel,
            institutionId: _institutionId,
            phoneNumber: _phoneController.text.trim().isEmpty ? null : _phoneController.text.trim(),
          );
    }
  }

  @override
  Widget build(BuildContext context) {
    final authState = ref.watch(authProvider);

    ref.listen(authProvider, (previous, next) {
      if (next.errorMessage != null) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text(next.errorMessage!),
            backgroundColor: AppTheme.error,
          ),
        );
      }
    });

    return Scaffold(
      appBar: AppBar(
        title: const Text('Create Account'),
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
          child: SingleChildScrollView(
            padding: const EdgeInsets.symmetric(horizontal: 24.0, vertical: 16.0),
            child: Form(
              key: _formKey,
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.stretch,
                children: [
                  Text(
                    'Join SACS Today',
                    textAlign: TextAlign.center,
                    style: Theme.of(context).textTheme.headlineMedium?.copyWith(
                          fontWeight: FontWeight.bold,
                        ),
                  ),
                  const SizedBox(height: 8),
                  Text(
                    'Get started with your academic scheduling assistant',
                    textAlign: TextAlign.center,
                    style: Theme.of(context).textTheme.bodyMedium,
                  ),
                  const SizedBox(height: 36),

                  // First Name
                  TextFormField(
                    controller: _firstNameController,
                    validator: (value) => value == null || value.isEmpty ? 'First name is required.' : null,
                    decoration: const InputDecoration(
                      labelText: 'First Name',
                      prefixIcon: Icon(Icons.person_outline),
                    ),
                  ),
                  const SizedBox(height: 16),

                  // Last Name
                  TextFormField(
                    controller: _lastNameController,
                    validator: (value) => value == null || value.isEmpty ? 'Last name is required.' : null,
                    decoration: const InputDecoration(
                      labelText: 'Last Name',
                      prefixIcon: Icon(Icons.person_outline),
                    ),
                  ),
                  const SizedBox(height: 16),

                  // Email
                  TextFormField(
                    controller: _emailController,
                    keyboardType: TextInputType.emailAddress,
                    validator: (value) {
                      if (value == null || value.isEmpty) return 'Email is required.';
                      if (!value.contains('@')) return 'Enter a valid email.';
                      return null;
                    },
                    decoration: const InputDecoration(
                      labelText: 'Email Address',
                      prefixIcon: Icon(Icons.email_outlined),
                    ),
                  ),
                  const SizedBox(height: 16),

                  // Password
                  TextFormField(
                    controller: _passwordController,
                    obscureText: true,
                    validator: (value) => value == null || value.length < 6 ? 'Password must be at least 6 characters.' : null,
                    decoration: const InputDecoration(
                      labelText: 'Password',
                      prefixIcon: Icon(Icons.lock_outlined),
                    ),
                  ),
                  const SizedBox(height: 16),

                  // Matriculation Number
                  TextFormField(
                    controller: _matricNumberController,
                    validator: (value) => value == null || value.isEmpty ? 'Matriculation number is required.' : null,
                    decoration: const InputDecoration(
                      labelText: 'Matriculation Number',
                      prefixIcon: Icon(Icons.badge_outlined),
                    ),
                  ),
                  const SizedBox(height: 16),

                  // Academic Level Dropdown
                  DropdownButtonFormField<int>(
                    value: _academicLevel,
                    decoration: const InputDecoration(
                      labelText: 'Academic Level',
                      prefixIcon: Icon(Icons.layers_outlined),
                    ),
                    items: [100, 200, 300, 400, 500]
                        .map((lvl) => DropdownMenuItem<int>(
                              value: lvl,
                              child: Text('Level $lvl'),
                            ))
                        .toList(),
                    onChanged: (val) {
                      if (val != null) {
                        setState(() {
                          _academicLevel = val;
                        });
                      }
                    },
                  ),
                  const SizedBox(height: 16),

                  // Phone Number
                  TextFormField(
                    controller: _phoneController,
                    keyboardType: TextInputType.phone,
                    decoration: const InputDecoration(
                      labelText: 'Phone Number (Optional)',
                      prefixIcon: Icon(Icons.phone_outlined),
                    ),
                  ),
                  const SizedBox(height: 32),

                  // Register Button
                  ElevatedButton(
                    onPressed: authState.isLoading ? null : _submit,
                    style: ElevatedButton.styleFrom(
                      backgroundColor: AppTheme.primaryLight,
                      foregroundColor: AppTheme.textPrimary,
                      padding: const EdgeInsets.symmetric(vertical: 16),
                      shape: RoundedRectangleBorder(
                        borderRadius: BorderRadius.circular(12),
                      ),
                    ),
                    child: authState.isLoading
                        ? const SizedBox(
                            height: 20,
                            width: 20,
                            child: CircularProgressIndicator(
                              strokeWidth: 2,
                              valueColor: AlwaysStoppedAnimation(Colors.white),
                            ),
                          )
                        : const Text(
                            'Create Account',
                            style: TextStyle(
                              fontSize: 16,
                              fontWeight: FontWeight.bold,
                            ),
                          ),
                  ),
                  const SizedBox(height: 24),
                ],
              ),
            ),
          ),
        ),
      ),
    );
  }
}
