import 'dart:async';
import 'dart:io';
import 'package:flutter/foundation.dart';
import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:provider/provider.dart';
import 'package:flutter_dotenv/flutter_dotenv.dart';
import 'package:flutter_localizations/flutter_localizations.dart';
import 'package:flutter_gen/gen_l10n/app_localizations.dart';
import 'package:window_manager/window_manager.dart';

import 'core/config/kiosk_config.dart';
import 'core/api/api_client.dart';
import 'core/auth/auth_service.dart';
import 'core/network/service_discovery.dart';
import 'features/authentication/admin_access.dart';
import 'features/authentication/kiosk_home_screen.dart';

void main() async {
  WidgetsFlutterBinding.ensureInitialized();
  
  // Initialize window manager only for desktop platforms
  if (!kIsWeb && (Platform.isWindows || Platform.isLinux || Platform.isMacOS)) {
    await windowManager.ensureInitialized();
    
    const windowOptions = WindowOptions(
      size: Size(1920, 1080),
      center: true,
      backgroundColor: Colors.transparent,
      skipTaskbar: false,
      titleBarStyle: TitleBarStyle.hidden,
      fullScreen: true,
    );
    
    windowManager.waitUntilReadyToShow(windowOptions, () async {
      await windowManager.show();
      await windowManager.focus();
      await windowManager.setAsFrameless();
      await windowManager.setFullScreen(true);
      await windowManager.setAlwaysOnTop(true);
    });
  }
  
  // Load environment configuration
  try {
    await dotenv.load(fileName: '.env');
  } catch (e) {
    debugPrint('Warning: Could not load .env file: $e');
  }
  
  // Initialize core services
  await _initializeServices();
  
  // Set app to fullscreen kiosk mode
  await SystemChrome.setEnabledSystemUIMode(SystemUiMode.immersive);
  await SystemChrome.setPreferredOrientations([DeviceOrientation.landscapeLeft]);
  
  runApp(const QaliTrackKioskApp());
}

Future<void> _initializeServices() async {
  try {
    // Initialize configuration
    final config = KioskConfig();
    await config.initialize();
    
    // Initialize API client
    final apiClient = ApiClient();
    apiClient.initialize();
    
    // Initialize authentication service
    final authService = AuthService();
    await authService.initialize();
    
    // Start service discovery
    final serviceDiscovery = ServiceDiscovery();
    await serviceDiscovery.discoverServices();
    
    debugPrint('All services initialized successfully');
  } catch (e) {
    debugPrint('Service initialization failed: $e');
  }
}

class QaliTrackKioskApp extends StatelessWidget {
  const QaliTrackKioskApp({Key? key}) : super(key: key);

  @override
  Widget build(BuildContext context) {
    return MultiProvider(
      providers: [
        ChangeNotifierProvider(create: (_) => AdminAccessProvider()),
        ChangeNotifierProvider(create: (_) => KioskStateProvider()),
        ChangeNotifierProvider(create: (_) => LanguageProvider()),
      ],
      child: Consumer<LanguageProvider>(
        builder: (context, languageProvider, child) {
          return MaterialApp(
            title: 'QaliTrack Kiosk',
            debugShowCheckedModeBanner: false,
            theme: _buildTheme(),
            locale: languageProvider.currentLocale,
            localizationsDelegates: const [
              AppLocalizations.delegate,
              GlobalMaterialLocalizations.delegate,
              GlobalWidgetsLocalizations.delegate,
              GlobalCupertinoLocalizations.delegate,
            ],
            supportedLocales: const [
              Locale('en', ''),
              Locale('sw', ''),
            ],
            home: const KioskMainScreen(),
          );
        },
      ),
    );
  }

  ThemeData _buildTheme() {
    return ThemeData(
      useMaterial3: true,
      colorScheme: ColorScheme.fromSeed(
        seedColor: const Color(0xFF2E8B57), // QaliTrack green
        brightness: Brightness.light,
      ),
      appBarTheme: const AppBarTheme(
        centerTitle: true,
        elevation: 0,
      ),
      elevatedButtonTheme: ElevatedButtonThemeData(
        style: ElevatedButton.styleFrom(
          minimumSize: const Size(120, 48),
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(8),
          ),
        ),
      ),
      cardTheme: CardTheme(
        elevation: 2,
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(12),
        ),
      ),
      // Optimize for kiosk usage - larger touch targets
      materialTapTargetSize: MaterialTapTargetSize.padded,
    );
  }
}

class KioskMainScreen extends StatelessWidget {
  const KioskMainScreen({Key? key}) : super(key: key);

  @override
  Widget build(BuildContext context) {
    return Consumer<AdminAccessProvider>(
      builder: (context, adminProvider, child) {
        return AdminKeySequenceListener(
          adminProvider: adminProvider,
          child: Scaffold(
            body: Stack(
              children: [
                const KioskHomeScreen(),
                if (adminProvider.isAdminMode)
                  Positioned(
                    top: 16,
                    right: 16,
                    child: Container(
                      padding: const EdgeInsets.symmetric(
                        horizontal: 12,
                        vertical: 6,
                      ),
                      decoration: BoxDecoration(
                        color: Colors.red.shade100,
                        borderRadius: BorderRadius.circular(16),
                        border: Border.all(color: Colors.red),
                      ),
                      child: const Text(
                        'ADMIN MODE',
                        style: TextStyle(
                          color: Colors.red,
                          fontWeight: FontWeight.bold,
                          fontSize: 12,
                        ),
                      ),
                    ),
                  ),
              ],
            ),
          ),
        );
      },
    );
  }
}

class KioskStateProvider extends ChangeNotifier {
  bool _isIdle = true;
  DateTime _lastActivity = DateTime.now();
  Timer? _idleTimer;
  
  static const Duration _idleTimeout = Duration(minutes: 5);

  bool get isIdle => _isIdle;
  DateTime get lastActivity => _lastActivity;

  void recordActivity() {
    _lastActivity = DateTime.now();
    if (_isIdle) {
      _isIdle = false;
      notifyListeners();
    }
    
    _resetIdleTimer();
  }

  void _resetIdleTimer() {
    _idleTimer?.cancel();
    _idleTimer = Timer(_idleTimeout, () {
      _isIdle = true;
      notifyListeners();
    });
  }

  @override
  void dispose() {
    _idleTimer?.cancel();
    super.dispose();
  }
}

class LanguageProvider extends ChangeNotifier {
  final KioskConfig _config = KioskConfig();
  Locale _currentLocale = const Locale('en', '');

  Locale get currentLocale => _currentLocale;

  LanguageProvider() {
    _loadLanguage();
  }

  void _loadLanguage() {
    final languageCode = _config.getValue<String>('ui.default_language', 'en') ?? 'en';
    _currentLocale = Locale(languageCode, '');
  }

  Future<void> setLanguage(String languageCode) async {
    _currentLocale = Locale(languageCode, '');
    await _config.setValue('ui.default_language', languageCode);
    notifyListeners();
  }
}
