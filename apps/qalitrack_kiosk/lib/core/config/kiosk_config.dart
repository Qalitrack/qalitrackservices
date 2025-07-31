import 'dart:convert';
import 'dart:io';
import 'package:crypto/crypto.dart';
import 'package:shared_preferences/shared_preferences.dart';
import 'package:logger/logger.dart';

class KioskConfig {
  static final KioskConfig _instance = KioskConfig._internal();
  factory KioskConfig() => _instance;
  KioskConfig._internal();

  final Logger _logger = Logger();
  
  // Configuration keys
  static const String _configKey = 'kiosk_config_encrypted';
  static const String _configVersionKey = 'config_version';
  static const String _lastConfigUpdateKey = 'last_config_update';
  
  // Default fallback configuration
  static const Map<String, dynamic> _defaultConfig = {
    'api': {
      'gateway_url': 'http://localhost:7000',
      'timeout': 30000,
      'retry_attempts': 3,
      'service_discovery_enabled': true,
    },
    'services': {
      'user_service': {'port': 7001, 'endpoint': '/api/users'},
      'driver_service': {'port': 7004, 'endpoint': '/api/drivers'},
      'transaction_service': {'port': 7100, 'endpoint': '/api/transactions'},
      'weight_data_service': {'port': 7101, 'endpoint': '/api/weight-data'},
    },
    'security': {
      'admin_session_timeout': 300, // 5 minutes
      'jwt_timeout_minutes': 60,
      'max_failed_attempts': 3,
      'lockout_duration_minutes': 15,
    },
    'ui': {
      'default_language': 'en',
      'session_timeout_seconds': 300,
      'screen_saver_timeout': 120,
      'admin_access_key_sequence': ['ctrl', 'shift', 'alt', 'a'],
    },
    'hardware': {
      'camera_quality': 'high',
      'face_detection_confidence': 0.8,
      'weight_stability_threshold': 0.1,
      'printer_timeout': 10000,
    },
    'features': {
      'offline_mode_enabled': true,
      'biometric_required': true,
      'receipt_printing_enabled': true,
      'multi_language_enabled': true,
    }
  };

  Map<String, dynamic> _currentConfig = Map.from(_defaultConfig);
  bool _isInitialized = false;
  String? _configEncryptionKey;

  Map<String, dynamic> get config => Map.unmodifiable(_currentConfig);
  bool get isInitialized => _isInitialized;

  Future<void> initialize() async {
    try {
      _configEncryptionKey = await _getOrCreateEncryptionKey();
      await _loadConfiguration();
      _isInitialized = true;
      _logger.i('Kiosk configuration initialized');
    } catch (e) {
      _logger.e('Failed to initialize configuration: $e');
      _currentConfig = Map.from(_defaultConfig);
      _isInitialized = true;
    }
  }

  // Get configuration value with path notation (e.g., 'api.gateway_url')
  T? getValue<T>(String path, [T? defaultValue]) {
    final parts = path.split('.');
    dynamic current = _currentConfig;
    
    for (final part in parts) {
      if (current is Map<String, dynamic> && current.containsKey(part)) {
        current = current[part];
      } else {
        return defaultValue;
      }
    }
    
    return current is T ? current : defaultValue;
  }

  // Set configuration value with path notation
  Future<bool> setValue(String path, dynamic value) async {
    final parts = path.split('.');
    Map<String, dynamic> current = _currentConfig;
    
    // Navigate to the parent of the target key
    for (int i = 0; i < parts.length - 1; i++) {
      final part = parts[i];
      if (!current.containsKey(part) || current[part] is! Map<String, dynamic>) {
        current[part] = <String, dynamic>{};
      }
      current = current[part];
    }
    
    // Set the value
    current[parts.last] = value;
    
    // Save configuration
    return await _saveConfiguration();
  }

  Future<bool> updateConfiguration(Map<String, dynamic> newConfig) async {
    try {
      // Validate configuration structure
      if (!_validateConfiguration(newConfig)) {
        _logger.e('Invalid configuration structure');
        return false;
      }

      _currentConfig = newConfig;
      final success = await _saveConfiguration();
      
      if (success) {
        _logger.i('Configuration updated successfully');
        await _logConfigurationChange('Configuration updated');
      }
      
      return success;
    } catch (e) {
      _logger.e('Failed to update configuration: $e');
      return false;
    }
  }

  Future<Map<String, dynamic>?> exportConfiguration() async {
    try {
      // Return sanitized configuration (without sensitive data)
      final sanitized = Map<String, dynamic>.from(_currentConfig);
      _sanitizeExportConfig(sanitized);
      return sanitized;
    } catch (e) {
      _logger.e('Failed to export configuration: $e');
      return null;
    }
  }

  Future<bool> importConfiguration(Map<String, dynamic> config) async {
    try {
      if (!_validateConfiguration(config)) {
        return false;
      }
      
      await _logConfigurationChange('Configuration imported');
      return await updateConfiguration(config);
    } catch (e) {
      _logger.e('Failed to import configuration: $e');
      return false;
    }
  }

  Future<void> resetToDefaults() async {
    _logger.w('Resetting configuration to defaults');
    _currentConfig = Map.from(_defaultConfig);
    await _saveConfiguration();
    await _logConfigurationChange('Configuration reset to defaults');
  }

  Future<void> _loadConfiguration() async {
    try {
      final prefs = await SharedPreferences.getInstance();
      final encryptedConfig = prefs.getString(_configKey);
      
      if (encryptedConfig != null && _configEncryptionKey != null) {
        final decrypted = _decryptString(encryptedConfig, _configEncryptionKey!);
        final configMap = jsonDecode(decrypted) as Map<String, dynamic>;
        
        // Merge with defaults to ensure all keys exist
        _currentConfig = _mergeWithDefaults(configMap, _defaultConfig);
        _logger.d('Configuration loaded from storage');
      } else {
        _logger.d('No stored configuration found, using defaults');
      }
    } catch (e) {
      _logger.e('Failed to load configuration: $e');
      _currentConfig = Map.from(_defaultConfig);
    }
  }

  Future<bool> _saveConfiguration() async {
    try {
      if (_configEncryptionKey == null) {
        _logger.e('No encryption key available');
        return false;
      }

      final prefs = await SharedPreferences.getInstance();
      final configJson = jsonEncode(_currentConfig);
      final encrypted = _encryptString(configJson, _configEncryptionKey!);
      
      await prefs.setString(_configKey, encrypted);
      await prefs.setString(_lastConfigUpdateKey, DateTime.now().toIso8601String());
      await prefs.setInt(_configVersionKey, (prefs.getInt(_configVersionKey) ?? 0) + 1);
      
      _logger.d('Configuration saved to storage');
      return true;
    } catch (e) {
      _logger.e('Failed to save configuration: $e');
      return false;
    }
  }

  Future<String> _getOrCreateEncryptionKey() async {
    final prefs = await SharedPreferences.getInstance();
    const keyName = 'config_encryption_key';
    
    String? key = prefs.getString(keyName);
    if (key == null) {
      // Generate a new encryption key based on device characteristics
      final deviceInfo = await _getDeviceFingerprint();
      key = _generateEncryptionKey(deviceInfo);
      await prefs.setString(keyName, key);
      _logger.d('Generated new encryption key');
    }
    
    return key;
  }

  String _generateEncryptionKey(String deviceInfo) {
    const secret = 'QaliTrackKiosk2024'; // Should be configurable in production
    final combined = '$secret$deviceInfo';
    final bytes = utf8.encode(combined);
    final digest = sha256.convert(bytes);
    return base64.encode(digest.bytes);
  }

  Future<String> _getDeviceFingerprint() async {
    try {
      final hostname = Platform.localHostname;
      final environment = Platform.environment;
      final version = Platform.version;
      
      return '$hostname-${environment['USER'] ?? 'unknown'}-$version';
    } catch (e) {
      return 'unknown-device';
    }
  }

  String _encryptString(String plainText, String key) {
    // Simple XOR encryption (should use proper encryption in production)
    final keyBytes = base64.decode(key);
    final textBytes = utf8.encode(plainText);
    final encrypted = <int>[];
    
    for (int i = 0; i < textBytes.length; i++) {
      encrypted.add(textBytes[i] ^ keyBytes[i % keyBytes.length]);
    }
    
    return base64.encode(encrypted);
  }

  String _decryptString(String encryptedText, String key) {
    final keyBytes = base64.decode(key);
    final encryptedBytes = base64.decode(encryptedText);
    final decrypted = <int>[];
    
    for (int i = 0; i < encryptedBytes.length; i++) {
      decrypted.add(encryptedBytes[i] ^ keyBytes[i % keyBytes.length]);
    }
    
    return utf8.decode(decrypted);
  }

  bool _validateConfiguration(Map<String, dynamic> config) {
    // Validate required configuration sections
    final requiredSections = ['api', 'services', 'security', 'ui', 'hardware', 'features'];
    
    for (final section in requiredSections) {
      if (!config.containsKey(section) || config[section] is! Map<String, dynamic>) {
        _logger.e('Missing or invalid configuration section: $section');
        return false;
      }
    }
    
    // Validate specific critical values
    final apiConfig = config['api'] as Map<String, dynamic>;
    if (!apiConfig.containsKey('gateway_url') || apiConfig['gateway_url'] is! String) {
      _logger.e('Invalid API gateway URL configuration');
      return false;
    }
    
    return true;
  }

  Map<String, dynamic> _mergeWithDefaults(Map<String, dynamic> config, Map<String, dynamic> defaults) {
    final merged = Map<String, dynamic>.from(defaults);
    
    for (final key in config.keys) {
      if (defaults.containsKey(key)) {
        if (config[key] is Map<String, dynamic> && defaults[key] is Map<String, dynamic>) {
          merged[key] = _mergeWithDefaults(
            config[key] as Map<String, dynamic>,
            defaults[key] as Map<String, dynamic>,
          );
        } else {
          merged[key] = config[key];
        }
      }
    }
    
    return merged;
  }

  void _sanitizeExportConfig(Map<String, dynamic> config) {
    // Remove sensitive information from export
    final security = config['security'] as Map<String, dynamic>?;
    security?.remove('admin_access_key_sequence');
    
    // Remove any keys that might contain sensitive data
    config.remove('internal_keys');
    config.remove('encryption_keys');
  }

  Future<void> _logConfigurationChange(String change) async {
    try {
      final prefs = await SharedPreferences.getInstance();
      final logs = prefs.getStringList('config_audit_log') ?? [];
      
      final logEntry = {
        'timestamp': DateTime.now().toIso8601String(),
        'change': change,
        'user': 'system', // Could be updated to track actual admin user
      };
      
      logs.add(jsonEncode(logEntry));
      
      // Keep only last 100 log entries
      if (logs.length > 100) {
        logs.removeRange(0, logs.length - 100);
      }
      
      await prefs.setStringList('config_audit_log', logs);
    } catch (e) {
      _logger.e('Failed to log configuration change: $e');
    }
  }
}