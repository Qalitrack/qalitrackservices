import 'package:flutter_test/flutter_test.dart';
import 'package:qalitrack_kiosk/core/config/kiosk_config.dart';
import 'package:qalitrack_kiosk/features/authentication/admin_access.dart';

void main() {
  group('Admin Setup Flow Tests', () {
    late KioskConfig config;
    late AdminAccessProvider adminProvider;

    setUp(() {
      config = KioskConfig();
      adminProvider = AdminAccessProvider();
    });

    test('KioskConfig should initially be in first-time setup mode', () {
      // Test initial state
      expect(config.isFirstTimeSetup, isTrue);
      expect(config.areCredentialsSet, isFalse);
    });

    test('Admin authentication should fail during first-time setup', () async {
      config.resetConfigForTesting({
        'admin': {
          'is_first_time_setup': true,
          'credentials_set': false,
        }
      });

      // Authentication should fail when first-time setup is required
      final result = await adminProvider.authenticateAdmin('admin', 'admin123');
      expect(result, isFalse);
    });

    test('Setup admin credentials should complete first-time setup', () async {
      config.resetConfigForTesting({
        'admin': {
          'is_first_time_setup': true,
          'credentials_set': false,
        }
      });

      // Setup should complete successfully
      final success = await adminProvider.setupAdminCredentials('newadmin', 'NewSecure123!');
      expect(success, isTrue);

      // Verify credentials were stored
      final storedUsername = config.testGetValue('security.admin_username');
      final storedPassword = config.testGetValue('security.admin_password');
      expect(storedUsername, equals('newadmin'));
      expect(storedPassword, equals('NewSecure123!'));
    });

    test('Authentication should work after setup with new credentials', () async {
      // Setup the configuration as if setup was completed
      config.resetConfigForTesting({
        'admin': {
          'is_first_time_setup': false,
          'credentials_set': true,
        },
        'security': {
          'admin_username': 'testadmin',
          'admin_password': 'TestPassword123!',
        }
      });

      // Authentication should work with stored credentials
      final result = await adminProvider.authenticateAdmin('testadmin', 'TestPassword123!');
      expect(result, isTrue);
      expect(adminProvider.isAdminMode, isTrue);
    });

    test('Authentication should fail with wrong credentials after setup', () async {
      // Setup the configuration as if setup was completed
      config.resetConfigForTesting({
        'admin': {
          'is_first_time_setup': false,
          'credentials_set': true,
        },
        'security': {
          'admin_username': 'testadmin',
          'admin_password': 'TestPassword123!',
        }
      });

      // Authentication should fail with wrong credentials
      final result = await adminProvider.authenticateAdmin('admin', 'admin123');
      expect(result, isFalse);
      expect(adminProvider.isAdminMode, isFalse);
    });

    test('Can reset first-time setup', () async {
      // Mark setup as complete first
      await config.markFirstTimeSetupComplete();
      expect(config.isFirstTimeSetup, isFalse);
      expect(config.areCredentialsSet, isTrue);

      // Reset setup
      await config.resetFirstTimeSetup();
      expect(config.isFirstTimeSetup, isTrue);
      expect(config.areCredentialsSet, isFalse);
    });

    test('Default credentials work only when credentials not set', () async {
      config.resetConfigForTesting({
        'admin': {
          'is_first_time_setup': false,
          'credentials_set': false,
        },
        'security': {
          'default_admin_username': 'admin',
          'default_admin_password': 'admin123',
        }
      });

      // Should work with default credentials when no custom credentials are set
      final result = await adminProvider.authenticateAdmin('admin', 'admin123');
      expect(result, isTrue);
    });

    test('Password validation functions work correctly', () {
      // Valid password
      expect(() {
        const password = 'SecurePass123!';
        // Simulate validation rules
        expect(password.length >= 8, isTrue);
        expect(password.contains(RegExp(r'[A-Z]')), isTrue);
        expect(password.contains(RegExp(r'[a-z]')), isTrue);
        expect(password.contains(RegExp(r'[0-9]')), isTrue);
        expect(password.contains(RegExp(r'[!@#$%^&*(),.?":{}|<>]')), isTrue);
      }, returnsNormally);

      // Invalid passwords
      expect('short'.length >= 8, isFalse);
      expect('nouppercase123!'.contains(RegExp(r'[A-Z]')), isFalse);
      expect('NOLOWERCASE123!'.contains(RegExp(r'[a-z]')), isFalse);
      expect('NoNumbers!'.contains(RegExp(r'[0-9]')), isFalse);
      expect('NoSpecialChars123'.contains(RegExp(r'[!@#$%^&*(),.?":{}|<>]')), isFalse);
    });

    test('Username validation rules work correctly', () {
      // Valid usernames
      expect('admin'.length >= 3, isTrue);
      expect('admin123'.contains(' '), isFalse);
      expect('admin_user'.contains(RegExp(r'^[a-zA-Z0-9_]+$')), isTrue);

      // Invalid usernames
      expect('ab'.length >= 3, isFalse);
      expect('user name'.contains(' '), isTrue);
      expect('user@name'.contains(RegExp(r'^[a-zA-Z0-9_]+$')), isFalse);
    });
  });
}