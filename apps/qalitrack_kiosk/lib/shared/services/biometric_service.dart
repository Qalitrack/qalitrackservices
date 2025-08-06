import 'dart:convert';
import 'dart:typed_data';
import 'package:logger/logger.dart';
import '../models/biometric_models.dart';
import '../../core/api/api_client.dart';

class BiometricService {
  static final BiometricService _instance = BiometricService._internal();
  factory BiometricService() => _instance;
  BiometricService._internal();

  final Logger _logger = Logger();
  final ApiClient _apiClient = ApiClient();

  Future<BiometricVerificationResultDto?> verifyFace(Uint8List imageBytes) async {
    try {
      // Convert image bytes to base64
      final base64Image = base64Encode(imageBytes);
      
      final verificationDto = BiometricVerificationDto(
        biometricType: 'face',
        biometricData: base64Image,
      );

      _logger.d('Sending face verification request');
      
      final response = await _apiClient.post(
        '/api/drivers/biometric/verify',
        data: verificationDto.toJson(),
      );

      if (response.statusCode == 200 && response.data != null) {
        final result = BiometricVerificationResultDto.fromJson(response.data);
        _logger.i('Face verification result: ${result.isMatch}, confidence: ${result.confidenceScore}');
        return result;
      }
      
      _logger.w('Face verification failed: Invalid response');
      return null;
    } catch (e) {
      _logger.e('Face verification error: $e');
      return null;
    }
  }

  Future<DriverDto?> getDriverByBiometric(Uint8List imageBytes) async {
    try {
      final base64Image = base64Encode(imageBytes);
      
      final verificationDto = BiometricVerificationDto(
        biometricType: 'face',
        biometricData: base64Image,
      );

      _logger.d('Searching for driver by face');
      
      final response = await _apiClient.post(
        '/api/drivers/biometric/identify',
        data: verificationDto.toJson(),
      );

      if (response.statusCode == 200 && response.data != null) {
        final driver = DriverDto.fromJson(response.data);
        _logger.i('Driver identified: ${driver.fullName}');
        return driver;
      }
      
      _logger.w('Driver identification failed: No match found');
      return null;
    } catch (e) {
      _logger.e('Driver identification error: $e');
      return null;
    }
  }

  Future<bool> registerDriverBiometric(String driverId, Uint8List imageBytes) async {
    try {
      final base64Image = base64Encode(imageBytes);
      
      final registrationDto = BiometricRegistrationDto(
        biometricType: 'face',
        biometricData: base64Image,
        description: 'Kiosk face registration',
      );

      _logger.d('Registering biometric for driver: $driverId');
      
      final response = await _apiClient.post(
        '/api/drivers/$driverId/biometric/register',
        data: registrationDto.toJson(),
      );

      if (response.statusCode == 200 || response.statusCode == 201) {
        _logger.i('Biometric registration successful for driver: $driverId');
        return true;
      }
      
      _logger.w('Biometric registration failed');
      return false;
    } catch (e) {
      _logger.e('Biometric registration error: $e');
      return false;
    }
  }

  Future<List<DriverDto>> getAvailableDrivers() async {
    try {
      _logger.d('Fetching available drivers');
      
      final response = await _apiClient.get('/api/drivers');

      if (response.statusCode == 200 && response.data != null) {
        final List<dynamic> driversJson = response.data;
        final drivers = driversJson
            .map((json) => DriverDto.fromJson(json))
            .toList();
        
        _logger.i('Fetched ${drivers.length} drivers');
        return drivers;
      }
      
      _logger.w('Failed to fetch drivers: Invalid response');
      return [];
    } catch (e) {
      _logger.e('Error fetching drivers: $e');
      return [];
    }
  }

  Future<DriverDto?> getDriverById(String driverId) async {
    try {
      _logger.d('Fetching driver: $driverId');
      
      final response = await _apiClient.get('/api/drivers/$driverId');

      if (response.statusCode == 200 && response.data != null) {
        final driver = DriverDto.fromJson(response.data);
        _logger.i('Fetched driver: ${driver.fullName}');
        return driver;
      }
      
      _logger.w('Driver not found: $driverId');
      return null;
    } catch (e) {
      _logger.e('Error fetching driver: $e');
      return null;
    }
  }
}