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

  bool _isPlatformCameraSupported() {
    // Camera plugin has limited support on desktop platforms
    if (kIsWeb) return true;
    if (Platform.isAndroid || Platform.isIOS) return true;
    
    // Desktop platforms have limited camera support
    if (Platform.isLinux || Platform.isWindows || Platform.isMacOS) {
      _logger.w('Camera support on desktop platforms is limited');
      return false; // Disable camera on desktop for now
    }
    
    return true;
  }

  Future<bool> initialize() async {
    try {
      // Check platform support first
      if (!_isPlatformCameraSupported()) {
        _logger.w('Camera not supported on this platform');
        _cameras = [];
        _isInitialized = true;
        _isPlatformSupported = false;
        return false;
      }
      
      _cameras = await availableCameras();
      _isInitialized = true;
      _isPlatformSupported = true;
      
      _logger.i('Camera service initialized. Found ${_cameras?.length ?? 0} cameras');
      return true;
    } catch (e) {
      _logger.e('Failed to initialize camera service: $e');
      if (e.toString().contains('MissingPluginException')) {
        _logger.w('Camera plugin not properly configured for this platform');
        _isPlatformSupported = false;
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