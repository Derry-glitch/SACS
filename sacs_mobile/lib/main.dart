import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import 'core/theme/app_theme.dart';
import 'core/routes/app_routes.dart';
import 'providers/auth_provider.dart';
import 'providers/event_provider.dart';
import 'services/api_service.dart';
import 'services/storage_service.dart';

void main() {
  WidgetsFlutterBinding.ensureInitialized();
  
  final storageService = StorageService();
  final apiService = ApiService(storageService: storageService);

  runApp(
    MultiProvider(
      providers: [
        ChangeNotifierProvider(
          create: (_) => AuthProvider(
            apiService: apiService,
            storageService: storageService,
          ),
        ),
        ChangeNotifierProvider(
          create: (_) => EventProvider(
            apiService: apiService,
          ),
        ),
      ],
      child: const SacsApp(),
    ),
  );
}

class SacsApp extends StatefulWidget {
  const SacsApp({super.key});

  @override
  State<SacsApp> createState() => _SacsAppState();
}

class _SacsAppState extends State<SacsApp> {
  late AppRouter _appRouter;

  @override
  void initState() {
    super.initState();
    // Initialize the router with the authProvider
    _appRouter = AppRouter(context.read<AuthProvider>());
  }

  @override
  Widget build(BuildContext context) {
    return MaterialApp.router(
      title: 'SACS Mobile',
      debugShowCheckedModeBanner: false,
      theme: AppTheme.darkTheme,
      themeMode: ThemeMode.dark, // Default to a premium dark mode
      routerConfig: _appRouter.router,
    );
  }
}
