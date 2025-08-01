import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:flutter_gen/gen_l10n/app_localizations.dart';
import '../../main.dart';
import '../../shared/models/biometric_models.dart';
import '../weighing/weighing_screen.dart';
import 'face_detection_screen.dart';
import 'settings_screen.dart';
import 'admin_access.dart';

class KioskHomeScreen extends StatefulWidget {
  const KioskHomeScreen({Key? key}) : super(key: key);

  @override
  State<KioskHomeScreen> createState() => _KioskHomeScreenState();
}

class _KioskHomeScreenState extends State<KioskHomeScreen>
    with SingleTickerProviderStateMixin {
  late AnimationController _animationController;
  late Animation<double> _fadeAnimation;
  late Animation<Offset> _slideAnimation;

  @override
  void initState() {
    super.initState();
    _setupAnimations();
  }

  void _setupAnimations() {
    _animationController = AnimationController(
      duration: const Duration(seconds: 2),
      vsync: this,
    );

    _fadeAnimation = Tween<double>(
      begin: 0.0,
      end: 1.0,
    ).animate(
      CurvedAnimation(
        parent: _animationController,
        curve: Curves.easeInOut,
      ),
    );

    _slideAnimation = Tween<Offset>(
      begin: const Offset(0, 0.5),
      end: Offset.zero,
    ).animate(
      CurvedAnimation(
        parent: _animationController,
        curve: Curves.easeOutCubic,
      ),
    );

    _animationController.forward();
  }

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context);
    
    return Consumer2<KioskStateProvider, LanguageProvider>(
      builder: (context, kioskProvider, languageProvider, child) {
        return Scaffold(
          body: GestureDetector(
            onTap: () => FocusScope.of(context).unfocus(),
            child: Container(
            decoration: const BoxDecoration(
              gradient: LinearGradient(
                begin: Alignment.topCenter,
                end: Alignment.bottomCenter,
                colors: [
                  Color(0xFF2E8B57), // QaliTrack green
                  Color(0xFF1F5F3F), // Darker green
                ],
              ),
            ),
            child: SafeArea(
              child: Padding(
                padding: const EdgeInsets.all(32.0),
                child: Column(
                  children: [
                    _buildHeader(l10n, languageProvider),
                    Expanded(
                      child: AnimatedBuilder(
                        animation: _animationController,
                        builder: (context, child) {
                          return FadeTransition(
                            opacity: _fadeAnimation,
                            child: SlideTransition(
                              position: _slideAnimation,
                              child: _buildMainContent(l10n, kioskProvider),
                            ),
                          );
                        },
                      ),
                    ),
                    _buildFooter(l10n),
                  ],
                ),
              ),
            ),
          ),
          ),
        );
      },
    );
  }

  Widget _buildHeader(AppLocalizations? l10n, LanguageProvider languageProvider) {
    return Row(
      mainAxisAlignment: MainAxisAlignment.spaceBetween,
      children: [
        Text(
          'QaliTrack',
          style: const TextStyle(
            fontSize: 42,
            fontWeight: FontWeight.bold,
            color: Colors.white,
          ),
        ),
        Row(
          children: [
            _buildLanguageSelector(l10n, languageProvider),
            const SizedBox(width: 16),
            Consumer<AdminAccessProvider>(
              builder: (context, adminProvider, child) {
                if (adminProvider.isAdminMode) {
                  return ElevatedButton.icon(
                    onPressed: () => _openSettings(context),
                    icon: const Icon(Icons.settings),
                    label: const Text('Settings'),
                    style: ElevatedButton.styleFrom(
                      backgroundColor: Colors.white,
                      foregroundColor: const Color(0xFF2E8B57),
                    ),
                  );
                }
                return const SizedBox.shrink();
              },
            ),
          ],
        ),
      ],
    );
  }

  Widget _buildLanguageSelector(AppLocalizations? l10n, LanguageProvider languageProvider) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 8),
      decoration: BoxDecoration(
        color: Colors.white.withOpacity(0.2),
        borderRadius: BorderRadius.circular(20),
        border: Border.all(color: Colors.white.withOpacity(0.3)),
      ),
      child: DropdownButton<String>(
        value: languageProvider.currentLocale.languageCode,
        dropdownColor: const Color(0xFF2E8B57),
        underline: const SizedBox.shrink(),
        icon: const Icon(Icons.language, color: Colors.white),
        style: const TextStyle(color: Colors.white, fontSize: 16),
        items: [
          DropdownMenuItem(
            value: 'en',
            child: Text(l10n?.english ?? 'English'),
          ),
          DropdownMenuItem(
            value: 'sw',
            child: Text(l10n?.swahili ?? 'Kiswahili'),
          ),
        ],
        onChanged: (String? newLanguage) {
          if (newLanguage != null) {
            languageProvider.setLanguage(newLanguage);
          }
        },
      ),
    );
  }

  Widget _buildMainContent(AppLocalizations? l10n, KioskStateProvider kioskProvider) {
    return Center(
      child: Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          Container(
            width: 240,
            height: 240,
            decoration: BoxDecoration(
              color: Colors.white.withOpacity(0.1),
              shape: BoxShape.circle,
              border: Border.all(
                color: Colors.white.withOpacity(0.3),
                width: 3,
              ),
            ),
            child: const Icon(
              Icons.scale,
              size: 120,
              color: Colors.white,
            ),
          ),
          const SizedBox(height: 48),
          Text(
            l10n?.welcome ?? 'Welcome',
            style: const TextStyle(
              fontSize: 48,
              fontWeight: FontWeight.bold,
              color: Colors.white,
            ),
            textAlign: TextAlign.center,
          ),
          const SizedBox(height: 24),
          Text(
            l10n?.touchToStart ?? 'Touch to Start',
            style: TextStyle(
              fontSize: 28,
              color: Colors.white.withOpacity(0.9),
              fontWeight: FontWeight.w500,
            ),
            textAlign: TextAlign.center,
          ),
          const SizedBox(height: 64),
          SizedBox(
            width: 400,
            height: 100,
            child: ElevatedButton(
              onPressed: () => _startWeighingProcess(kioskProvider),
              style: ElevatedButton.styleFrom(
                backgroundColor: Colors.white,
                foregroundColor: const Color(0xFF2E8B57),
                shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(50),
                ),
                elevation: 12,
              ),
              child: Row(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  const Icon(Icons.camera_alt, size: 40),
                  const SizedBox(width: 16),
                  Text(
                    l10n?.startWeighing ?? 'Start Weighing',
                    style: const TextStyle(
                      fontSize: 32,
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                ],
              ),
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildFooter(AppLocalizations? l10n) {
    return Row(
      mainAxisAlignment: MainAxisAlignment.spaceBetween,
      children: [
        Text(
          '© ${DateTime.now().year}',
          style: TextStyle(
            color: Colors.white.withOpacity(0.6),
            fontSize: 16,
          ),
        ),
        Row(
          children: [
            _buildConnectionStatus(),
            const SizedBox(width: 32),
            Text(
              _getCurrentTime(),
              style: TextStyle(
                color: Colors.white.withOpacity(0.8),
                fontSize: 18,
                fontWeight: FontWeight.w500,
              ),
            ),
          ],
        ),
      ],
    );
  }

  Widget _buildConnectionStatus() {
    return Row(
      children: [
        // Network Status
        Icon(
          Icons.wifi,
          color: Colors.greenAccent,
          size: 24,
        ),
        const SizedBox(width: 8),
        Text(
          'Network',
          style: TextStyle(
            color: Colors.white.withOpacity(0.8),
            fontSize: 16,
          ),
        ),
        const SizedBox(width: 24),
        // Backend Status
        Icon(
          Icons.cloud_done,
          color: Colors.greenAccent,
          size: 24,
        ),
        const SizedBox(width: 8),
        Text(
          'Services',
          style: TextStyle(
            color: Colors.white.withOpacity(0.8),
            fontSize: 16,
          ),
        ),
      ],
    );
  }

  void _startWeighingProcess(KioskStateProvider kioskProvider) {
    kioskProvider.recordActivity();
    
    Navigator.of(context).push(
      MaterialPageRoute(
        builder: (context) => FaceDetectionScreen(
          onDriverIdentified: (driver) {
            Navigator.of(context).pushReplacement(
              MaterialPageRoute(
                builder: (context) => WeighingScreen(driver: driver),
              ),
            );
          },
          onCancel: () {
            Navigator.of(context).pop();
          },
        ),
      ),
    );
  }

  void _openSettings(BuildContext context) {
    showDialog(
      context: context,
      builder: (context) => AdminLoginDialog(
        adminProvider: context.read<AdminAccessProvider>(),
      ),
    ).then((success) {
      if (success == true) {
        Navigator.of(context).push(
          MaterialPageRoute(
            builder: (context) => const SettingsScreen(),
          ),
        );
      }
    });
  }

  String _getCurrentTime() {
    final now = DateTime.now();
    return '${now.hour.toString().padLeft(2, '0')}:'
           '${now.minute.toString().padLeft(2, '0')}';
  }

  @override
  void dispose() {
    _animationController.dispose();
    super.dispose();
  }
}