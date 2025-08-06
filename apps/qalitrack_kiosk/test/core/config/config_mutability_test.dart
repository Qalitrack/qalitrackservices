import 'package:flutter_test/flutter_test.dart';
import 'package:qalitrack_kiosk/core/config/kiosk_config.dart';

void main() {
  group('Configuration Mutability Tests', () {
    test('Deep copy should create modifiable nested maps', () {
      final config = KioskConfig();
      
      // Test the _createDeepMutableCopy method indirectly through setValue
      final testMap = {
        'level1': {
          'level2': {
            'level3': 'original_value',
            'another_key': 123
          },
          'sibling': 'sibling_value'
        },
        'root_key': 'root_value'
      };
      
      // Simulate what happens during configuration loading
      config.resetConfigForTesting(testMap);
      
      // Test that we can modify deep nested values without "unmodifiable map" error
      expect(() {
        final result = config.testCreateDeepMutableCopy(testMap);
        
        // Verify we can modify all levels
        result['root_key'] = 'modified_root';
        result['level1']['sibling'] = 'modified_sibling';
        result['level1']['level2']['level3'] = 'modified_deep';
        result['level1']['level2']['new_key'] = 'new_value';
        result['level1']['new_section'] = {'nested': 'value'};
        
        // Verify the values were set correctly
        expect(result['root_key'], equals('modified_root'));
        expect(result['level1']['sibling'], equals('modified_sibling'));
        expect(result['level1']['level2']['level3'], equals('modified_deep'));
        expect(result['level1']['level2']['new_key'], equals('new_value'));
        expect(result['level1']['new_section']['nested'], equals('value'));
      }, returnsNormally);
    });

    test('Path-based configuration modification should work', () {
      final config = KioskConfig();
      
      // Set up test configuration
      final testConfig = {
        'ui': {
          'default_language': 'en',
          'timeout': 300
        },
        'services': {
          'user_service': {
            'port': 7001,
            'endpoint': '/api/users'
          }
        }
      };
      
      config.resetConfigForTesting(testConfig);
      
      // Test modifying existing nested values
      expect(() {
        config.testSetValueLocally('ui.default_language', 'sw');
        config.testSetValueLocally('services.user_service.port', 7002);
        config.testSetValueLocally('services.user_service.new_setting', 'test_value');
        config.testSetValueLocally('new_section.new_key', 'new_value');
      }, returnsNormally);
      
      // Verify values were set correctly
      expect(config.testGetValue('ui.default_language'), equals('sw'));
      expect(config.testGetValue('services.user_service.port'), equals(7002));
      expect(config.testGetValue('services.user_service.new_setting'), equals('test_value'));
      expect(config.testGetValue('new_section.new_key'), equals('new_value'));
    });

    test('Original default config should remain const and unmodifiable', () {
      final config = KioskConfig();
      
      // Verify that accessing the default config doesn't cause issues
      expect(() {
        final defaultLang = config.testGetDefaultValue('ui.default_language');
        expect(defaultLang, equals('en'));
        
        final timeout = config.testGetDefaultValue('security.admin_session_timeout');
        expect(timeout, equals(300));
      }, returnsNormally);
    });

    test('Configuration with deep nesting should be modifiable', () {
      final config = KioskConfig();
      
      final deepConfig = {
        'a': {
          'b': {
            'c': {
              'd': {
                'e': 'deep_value'
              }
            }
          }
        }
      };
      
      config.resetConfigForTesting(deepConfig);
      
      // Test deep modification
      expect(() {
        config.testSetValueLocally('a.b.c.d.e', 'modified_deep_value');
        config.testSetValueLocally('a.b.c.d.f', 'new_deep_value');
        config.testSetValueLocally('a.b.c.new_branch.value', 'branch_value');
      }, returnsNormally);
      
      expect(config.testGetValue('a.b.c.d.e'), equals('modified_deep_value'));
      expect(config.testGetValue('a.b.c.d.f'), equals('new_deep_value'));
      expect(config.testGetValue('a.b.c.new_branch.value'), equals('branch_value'));
    });
  });
}