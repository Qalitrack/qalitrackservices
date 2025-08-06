import 'package:flutter_test/flutter_test.dart';
import 'package:qalitrack_kiosk/core/config/kiosk_config.dart';
import 'package:qalitrack_kiosk/shared/services/offline_service.dart';
import 'package:qalitrack_kiosk/shared/models/biometric_models.dart';
import 'package:qalitrack_kiosk/shared/models/transaction_models.dart';

void main() {
  group('KioskConfig Tests', () {
    test('should initialize with default configuration', () async {
      final config = KioskConfig();
      await config.initialize();
      
      expect(config.isInitialized, true);
      expect(config.getValue<String>('api.gateway_url'), 'http://localhost:7000');
      expect(config.getValue<bool>('features.offline_mode_enabled'), true);
    });

    test('should set and get configuration values', () async {
      final config = KioskConfig();
      await config.initialize();
      
      await config.setValue('test.value', 'test_data');
      expect(config.getValue<String>('test.value'), 'test_data');
      
      await config.setValue('test.number', 42);
      expect(config.getValue<int>('test.number'), 42);
    });

    test('should return default value for non-existent keys', () async {
      final config = KioskConfig();
      await config.initialize();
      
      expect(config.getValue<String>('non.existent', 'default'), 'default');
      expect(config.getValue<int>('non.existent', 999), 999);
    });
  });

  group('OfflineService Tests', () {
    test('should initialize correctly', () async {
      final offlineService = OfflineService();
      await offlineService.initialize();
      
      expect(offlineService.cachedDrivers, isEmpty);
      expect(offlineService.pendingTransactions, isEmpty);
    });

    test('should cache and retrieve drivers', () async {
      final offlineService = OfflineService();
      await offlineService.initialize();
      
      final testDrivers = [
        DriverDto(
          id: '1',
          firstName: 'John',
          lastName: 'Doe',
          middleName: '',
          phoneNumber: '+254700000000',
          email: 'john.doe@example.com',
          employeeId: 'EMP001',
          profilePhotoUrl: '',
          biometricEnabled: true,
        ),
        DriverDto(
          id: '2',
          firstName: 'Jane',
          lastName: 'Smith',
          middleName: 'M',
          phoneNumber: '+254700000001',
          email: 'jane.smith@example.com',
          employeeId: 'EMP002',
          profilePhotoUrl: '',
          biometricEnabled: true,
        ),
      ];
      
      await offlineService.cacheDrivers(testDrivers);
      expect(offlineService.cachedDrivers.length, 2);
      
      final foundDriver = await offlineService.findDriverOffline('EMP001');
      expect(foundDriver, isNotNull);
      expect(foundDriver!.firstName, 'John');
    });

    test('should store and retrieve offline transactions', () async {
      final offlineService = OfflineService();
      await offlineService.initialize();
      
      final testTransaction = TransactionDto(
        id: 'trans_001',
        driverId: 'driver_001',
        vehicleId: 'vehicle_001',
        timestamp: DateTime.now(),
        status: 'completed',
        tareWeight: 1000.0,
        grossWeight: 3000.0,
        netWeight: 2000.0,
      );
      
      await offlineService.storeOfflineTransaction(testTransaction);
      
      final transactions = await offlineService.getOfflineTransactions();
      expect(transactions.length, 1);
      expect(transactions.first.id, 'trans_001');
    });
  });

  group('Model Serialization Tests', () {
    test('DriverDto should serialize and deserialize correctly', () {
      final driver = DriverDto(
        id: '123',
        firstName: 'John',
        lastName: 'Doe',
        middleName: 'M',
        phoneNumber: '+254700000000',
        email: 'john@example.com',
        employeeId: 'EMP123',
        profilePhotoUrl: 'https://example.com/photo.jpg',
        biometricEnabled: true,
        biometricRegistrationDate: DateTime(2024, 1, 1),
      );
      
      final json = driver.toJson();
      final deserializedDriver = DriverDto.fromJson(json);
      
      expect(deserializedDriver.id, driver.id);
      expect(deserializedDriver.fullName, driver.fullName);
      expect(deserializedDriver.biometricEnabled, driver.biometricEnabled);
    });

    test('TransactionDto should serialize and deserialize correctly', () {
      final transaction = TransactionDto(
        id: 'trans_123',
        driverId: 'driver_123',
        vehicleId: 'vehicle_123',
        timestamp: DateTime(2024, 1, 1, 10, 30),
        status: 'completed',
        tareWeight: 1500.5,
        grossWeight: 3200.8,
        netWeight: 1700.3,
        notes: 'Test transaction',
      );
      
      final json = transaction.toJson();
      final deserializedTransaction = TransactionDto.fromJson(json);
      
      expect(deserializedTransaction.id, transaction.id);
      expect(deserializedTransaction.tareWeight, transaction.tareWeight);
      expect(deserializedTransaction.status, transaction.status);
    });

    test('WeightReadingDto should serialize and deserialize correctly', () {
      final weightReading = WeightReadingDto(
        weight: 2500.75,
        timestamp: DateTime(2024, 1, 1, 14, 45),
        isStable: true,
        unit: 'kg',
      );
      
      final json = weightReading.toJson();
      final deserializedReading = WeightReadingDto.fromJson(json);
      
      expect(deserializedReading.weight, weightReading.weight);
      expect(deserializedReading.isStable, weightReading.isStable);
      expect(deserializedReading.unit, weightReading.unit);
    });
  });

  group('Configuration Validation Tests', () {
    test('should validate required configuration sections', () async {
      final config = KioskConfig();
      await config.initialize();
      
      // Test that all required sections exist
      expect(config.getValue('api'), isNotNull);
      expect(config.getValue('services'), isNotNull);
      expect(config.getValue('security'), isNotNull);
      expect(config.getValue('ui'), isNotNull);
      expect(config.getValue('hardware'), isNotNull);
      expect(config.getValue('features'), isNotNull);
    });

    test('should have sensible default values', () async {
      final config = KioskConfig();
      await config.initialize();
      
      expect(config.getValue<int>('api.timeout'), greaterThan(0));
      expect(config.getValue<double>('hardware.face_detection_confidence'), 
             allOf(greaterThanOrEqualTo(0.0), lessThanOrEqualTo(1.0)));
      expect(config.getValue<int>('security.admin_session_timeout'), greaterThan(0));
    });
  });

  group('Edge Cases and Error Handling', () {
    test('should handle null and empty values gracefully', () async {
      final offlineService = OfflineService();
      await offlineService.initialize();
      
      // Test with empty driver list
      await offlineService.cacheDrivers([]);
      expect(offlineService.cachedDrivers, isEmpty);
      
      // Test with null search query
      final result = await offlineService.findDriverOffline('');
      expect(result, isNull);
    });

    test('should handle malformed data gracefully', () {
      expect(() {
        // Test with incomplete JSON
        DriverDto.fromJson({'id': '123'});
      }, throwsA(isA<TypeError>()));
    });
  });
}