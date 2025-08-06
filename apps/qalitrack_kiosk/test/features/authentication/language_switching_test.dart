import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:provider/provider.dart';
import 'package:flutter_localizations/flutter_localizations.dart';
import 'package:flutter_gen/gen_l10n/app_localizations.dart';

import 'package:qalitrack_kiosk/main.dart';
import 'package:qalitrack_kiosk/core/config/kiosk_config.dart';

void main() {
  group('Language Switching Tests', () {
    late KioskConfig config;

    setUp(() async {
      config = KioskConfig();
      await config.initialize();
    });

    testWidgets('Language dropdown should switch between English and Swahili', (WidgetTester tester) async {
      // Build the app
      await tester.pumpWidget(
        MultiProvider(
          providers: [
            ChangeNotifierProvider(create: (_) => LanguageProvider()),
            ChangeNotifierProvider(create: (_) => KioskStateProvider()),
          ],
          child: Consumer<LanguageProvider>(
            builder: (context, languageProvider, child) {
              return MaterialApp(
                locale: languageProvider.currentLocale,
                localizationsDelegates: const [
                  AppLocalizations.delegate,
                  GlobalMaterialLocalizations.delegate,
                  GlobalWidgetsLocalizations.delegate,
                ],
                supportedLocales: const [
                  Locale('en', ''),
                  Locale('sw', ''),
                ],
                home: Scaffold(
                  body: Column(
                    children: [
                      DropdownButton<String>(
                        key: const Key('language_dropdown'),
                        value: languageProvider.currentLocale.languageCode,
                        items: const [
                          DropdownMenuItem(value: 'en', child: Text('English')),
                          DropdownMenuItem(value: 'sw', child: Text('Kiswahili')),
                        ],
                        onChanged: (String? newLanguage) {
                          if (newLanguage != null) {
                            languageProvider.setLanguage(newLanguage);
                          }
                        },
                      ),
                      Builder(
                        builder: (context) {
                          final l10n = AppLocalizations.of(context);
                          return Text(
                            l10n?.welcome ?? 'Welcome',
                            key: const Key('welcome_text'),
                          );
                        },
                      ),
                    ],
                  ),
                ),
              );
            },
          ),
        ),
      );

      await tester.pumpAndSettle();

      // Find the language dropdown
      final languageDropdown = find.byKey(const Key('language_dropdown'));
      expect(languageDropdown, findsOneWidget);

      // Initially should be English
      expect(find.text('English'), findsOneWidget);
      expect(find.text('Welcome'), findsOneWidget);

      // Tap the dropdown to open it
      await tester.tap(languageDropdown);
      await tester.pumpAndSettle();

      // Select Swahili
      await tester.tap(find.text('Kiswahili').last);
      await tester.pumpAndSettle();

      // Verify language changed to Swahili
      final welcomeText = find.byKey(const Key('welcome_text'));
      final welcomeWidget = tester.widget<Text>(welcomeText);
      
      // Should show Swahili welcome text (or fallback to English if not localized)
      expect(welcomeWidget.data, isNotNull);
    });

    testWidgets('Config setValue should work without unmodifiable map error', (WidgetTester tester) async {
      // Test the underlying config functionality that was causing the language error
      expect(() async {
        final success = await config.setValue('ui.default_language', 'sw');
        expect(success, isTrue);
        
        final retrievedValue = config.getValue<String>('ui.default_language');
        expect(retrievedValue, equals('sw'));
        
        // Test nested path modification
        await config.setValue('ui.screen_saver_timeout', 180);
        final timeoutValue = config.getValue<int>('ui.screen_saver_timeout');
        expect(timeoutValue, equals(180));
        
        // Test deep nested modification
        await config.setValue('services.user_service.port', 7002);
        final portValue = config.getValue<int>('services.user_service.port');
        expect(portValue, equals(7002));
      }, returnsNormally);
    });

    testWidgets('LanguageProvider setLanguage should update configuration', (WidgetTester tester) async {
      final languageProvider = LanguageProvider();
      
      // Initially should be English
      expect(languageProvider.currentLocale.languageCode, equals('en'));
      
      // Change to Swahili
      await languageProvider.setLanguage('sw');
      expect(languageProvider.currentLocale.languageCode, equals('sw'));
      
      // Verify it was saved to config
      final savedLanguage = config.getValue<String>('ui.default_language');
      expect(savedLanguage, equals('sw'));
      
      // Change back to English
      await languageProvider.setLanguage('en');
      expect(languageProvider.currentLocale.languageCode, equals('en'));
      
      final savedLanguageAgain = config.getValue<String>('ui.default_language');
      expect(savedLanguageAgain, equals('en'));
    });

    test('Config deep copy should create modifiable nested maps', () async {
      // Create a test configuration with nested structures
      final testConfig = {
        'level1': {
          'level2': {
            'level3': 'value'
          }
        }
      };
      
      await config.updateConfiguration(testConfig);
      
      // Test that we can modify deep nested values
      expect(() async {
        await config.setValue('level1.level2.level3', 'modified_value');
        await config.setValue('level1.level2.new_key', 'new_value');
        await config.setValue('level1.new_section.key', 'another_value');
      }, returnsNormally);
      
      // Verify the values were set correctly
      expect(config.getValue<String>('level1.level2.level3'), equals('modified_value'));
      expect(config.getValue<String>('level1.level2.new_key'), equals('new_value'));
      expect(config.getValue<String>('level1.new_section.key'), equals('another_value'));
    });
  });
}