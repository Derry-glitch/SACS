import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import 'core/theme/app_theme.dart';
import 'core/routes/app_routes.dart';

void main() {
  WidgetsFlutterBinding.ensureInitialized();
  runApp(
    const ProviderScope(
      child: SacsApp(),
    ),
  );
}

class SacsApp extends ConsumerWidget {
  const SacsApp({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final router = ref.watch(routerProvider);

    return MaterialApp.router(
      title: 'SACS Mobile',
      debugShowCheckedModeBanner: false,
      theme: AppTheme.darkTheme,
      themeMode: ThemeMode.dark, // Default to a premium dark mode
      routerConfig: router,
    );
  }
}
