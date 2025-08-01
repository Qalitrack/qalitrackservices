import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:provider/provider.dart';
import 'package:virtual_keyboard_multi_language/virtual_keyboard_multi_language.dart';

import 'package:qalitrack_kiosk/features/authentication/admin_access.dart';

void main() {
  group('Virtual Keyboard Tests', () {
    testWidgets('AdminLoginDialog should show virtual keyboard when field is focused', (WidgetTester tester) async {
      final adminProvider = AdminAccessProvider();
      
      await tester.pumpWidget(
        MaterialApp(
          home: Scaffold(
            body: AdminLoginDialog(adminProvider: adminProvider),
          ),
        ),
      );

      await tester.pumpAndSettle();

      // Find the username text field
      final usernameField = find.byType(TextField).first;
      expect(usernameField, findsOneWidget);

      // Initially keyboard should not be visible
      expect(find.byType(VirtualKeyboard), findsNothing);

      // Tap the username field
      await tester.tap(usernameField);
      await tester.pumpAndSettle();

      // Virtual keyboard should now be visible
      expect(find.byType(VirtualKeyboard), findsOneWidget);
      
      // Verify keyboard is configured for username (alphanumeric)
      final virtualKeyboard = tester.widget<VirtualKeyboard>(find.byType(VirtualKeyboard));
      expect(virtualKeyboard.type, equals(VirtualKeyboardType.Alphanumeric));
    });

    testWidgets('Virtual keyboard should switch between username and password fields', (WidgetTester tester) async {
      final adminProvider = AdminAccessProvider();
      
      await tester.pumpWidget(
        MaterialApp(
          home: Scaffold(
            body: AdminLoginDialog(adminProvider: adminProvider),
          ),
        ),
      );

      await tester.pumpAndSettle();

      // Find text fields
      final textFields = find.byType(TextField);
      expect(textFields, findsNWidgets(2));
      
      final usernameField = textFields.first;
      final passwordField = textFields.last;

      // Tap username field first
      await tester.tap(usernameField);
      await tester.pumpAndSettle();

      // Keyboard should be visible and connected to username field
      expect(find.byType(VirtualKeyboard), findsOneWidget);
      
      // Now tap password field
      await tester.tap(passwordField);
      await tester.pumpAndSettle();

      // Keyboard should still be visible but now connected to password field
      expect(find.byType(VirtualKeyboard), findsOneWidget);
    });

    testWidgets('Hide keyboard button should work', (WidgetTester tester) async {
      final adminProvider = AdminAccessProvider();
      
      await tester.pumpWidget(
        MaterialApp(
          home: Scaffold(
            body: AdminLoginDialog(adminProvider: adminProvider),
          ),
        ),
      );

      await tester.pumpAndSettle();

      // Tap username field to show keyboard
      final usernameField = find.byType(TextField).first;
      await tester.tap(usernameField);
      await tester.pumpAndSettle();

      // Keyboard should be visible
      expect(find.byType(VirtualKeyboard), findsOneWidget);
      
      // Find and tap hide keyboard button
      final hideKeyboardButton = find.text('Hide Keyboard');
      expect(hideKeyboardButton, findsOneWidget);
      
      await tester.tap(hideKeyboardButton);
      await tester.pumpAndSettle();

      // Keyboard should now be hidden
      expect(find.byType(VirtualKeyboard), findsNothing);
    });

    testWidgets('Login dialog should have default credentials hint', (WidgetTester tester) async {
      final adminProvider = AdminAccessProvider();
      
      await tester.pumpWidget(
        MaterialApp(
          home: Scaffold(
            body: AdminLoginDialog(adminProvider: adminProvider),
          ),
        ),
      );

      await tester.pumpAndSettle();

      // Should show default credentials hint
      expect(find.text('Default: admin / admin123'), findsOneWidget);
      
      // Should show info icon
      expect(find.byIcon(Icons.info_outline), findsOneWidget);
    });

    testWidgets('Login dialog should handle authentication', (WidgetTester tester) async {
      final adminProvider = AdminAccessProvider();
      
      await tester.pumpWidget(
        MaterialApp(
          home: Scaffold(
            body: AdminLoginDialog(adminProvider: adminProvider),
          ),
        ),
      );

      await tester.pumpAndSettle();

      // Find login button
      final loginButton = find.text('Login');
      expect(loginButton, findsOneWidget);

      // Initially login button should be enabled
      final loginButtonWidget = tester.widget<ElevatedButton>(
        find.ancestor(
          of: find.text('Login'),
          matching: find.byType(ElevatedButton),
        ),
      );
      expect(loginButtonWidget.onPressed, isNotNull);

      // Find text fields
      final textFields = find.byType(TextField);
      final usernameField = textFields.first;
      final passwordField = textFields.last;

      // Test fields are readonly (to prevent system keyboard)
      final usernameWidget = tester.widget<TextField>(usernameField);
      final passwordWidget = tester.widget<TextField>(passwordField);
      
      expect(usernameWidget.readOnly, isTrue);
      expect(passwordWidget.readOnly, isTrue);
      expect(passwordWidget.obscureText, isTrue);
    });

    testWidgets('Virtual keyboard should support English layout', (WidgetTester tester) async {
      final adminProvider = AdminAccessProvider();
      
      await tester.pumpWidget(
        MaterialApp(
          home: Scaffold(
            body: AdminLoginDialog(adminProvider: adminProvider),
          ),
        ),
      );

      await tester.pumpAndSettle();

      // Tap username field to show keyboard
      final usernameField = find.byType(TextField).first;
      await tester.tap(usernameField);
      await tester.pumpAndSettle();

      // Verify keyboard has English layout
      final virtualKeyboard = tester.widget<VirtualKeyboard>(find.byType(VirtualKeyboard));
      expect(virtualKeyboard.defaultLayouts, contains(VirtualKeyboardDefaultLayouts.English));
      expect(virtualKeyboard.height, equals(300));
      expect(virtualKeyboard.textColor, equals(Colors.black));
    });
  });
}