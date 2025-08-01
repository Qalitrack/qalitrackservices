import 'package:camera/camera.dart';
import 'package:logger/logger.dart';
import 'package:flutter/foundation.dart';
import 'dart:io';

class CameraService {
  static final CameraService _instance = CameraService._internal();
  factory CameraService() => _instance;
  CameraService._internal();

  final Logger _logger = Logger();
  List<CameraDescription>? _cameras;
  bool _isInitialized = false;
  bool _isPlatformSupported = true;

  bool get isInitialized => _isInitialized;
  bool get hasCameras => _cameras?.isNotEmpty ?? false;
  bool get isPlatformSupported => _isPlatformSupported;
  List<CameraDescription> get cameras => _cameras ?? [];
  
  CameraDescription? get frontCamera {
    if (_cameras == null) return null;
    
    for (final camera in _cameras!) {
      if (camera.lensDirection == CameraLensDirection.front) {
        return camera;
      }
    }
    return null;
  }
  
  CameraDescription? get backCamera {
    if (_cameras == null) return null;
    
    for (final camera in _cameras!) {
      if (camera.lensDirection == CameraLensDirection.back) {
        return camera;
      }
    }
    return null;
  }
  
  CameraDescription? get defaultCamera => frontCamera ?? backCamera;

  Future<bool> initialize() async {
    try {
      _logger.i('Initializing camera service...');
      
      // Try to get available cameras regardless of platform
      _cameras = await availableCameras();
      _isInitialized = true;
      _isPlatformSupported = true;
      
      _logger.i('Camera service initialized. Found ${_cameras?.length ?? 0} cameras');
      
      if (_cameras?.isNotEmpty == true) {
        for (final camera in _cameras!) {
          _logger.i('Camera found: ${camera.name} (${camera.lensDirection})');
        }
      } else {
        _logger.w('No cameras detected on this system');
      }
      
      return _cameras?.isNotEmpty ?? false;
    } catch (e) {
      _logger.e('Failed to initialize camera service: $e');
      
      if (e.toString().contains('MissingPluginException')) {
        _logger.w('Camera plugin not available for this platform');
        _isPlatformSupported = false;
      } else {
        // Other errors might be temporary, so keep platform as supported
        _logger.w('Camera initialization failed but platform may support cameras');
        _isPlatformSupported = true;
      }
      
      _cameras = [];
      _isInitialized = true;
      return false;
    }
  }

  String getCameraStatus() {
    if (!_isInitialized) {
      return 'Not initialized';
    }
    
    if (!_isPlatformSupported) {
      return 'Not supported on this platform';
    }
    
    if (_cameras == null || _cameras!.isEmpty) {
      return 'No camera detected';
    }
    
    final frontCameraAvailable = frontCamera != null;
    final backCameraAvailable = backCamera != null;
    
    if (frontCameraAvailable && backCameraAvailable) {
      return 'Front & back cameras ready';
    } else if (frontCameraAvailable) {
      return 'Front camera ready';
    } else if (backCameraAvailable) {
      return 'Back camera ready';
    } else {
      return 'Camera detected but not accessible';
    }
  }

  bool isCameraHealthy() {
    return _isInitialized && _isPlatformSupported && hasCameras;
  }
}