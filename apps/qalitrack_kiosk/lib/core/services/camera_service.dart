import 'package:camera/camera.dart';
import 'package:logger/logger.dart';
import 'package:flutter/foundation.dart';
import 'dart:io';
import 'dart:async';

// Platform-specific imports  
import 'package:camera_windows/camera_windows.dart' if (dart.library.html) 'dart:typed_data' as camera_windows;
import 'package:camera_platform_interface/camera_platform_interface.dart' as camera_platform;

class CameraService {
  static final CameraService _instance = CameraService._internal();
  factory CameraService() => _instance;
  CameraService._internal();

  final Logger _logger = Logger();
  List<CameraDescription>? _cameras;
  bool _isInitialized = false;
  bool _isPlatformSupported = true;
  bool _isWindowsDesktop = false;
  
  // Windows-specific camera support
  int? _windowsCameraId;
  StreamSubscription<camera_windows.FrameAvailabledEvent>? _frameSubscription;

  bool get isInitialized => _isInitialized;
  bool get hasCameras => _cameras?.isNotEmpty ?? false;
  bool get isPlatformSupported => _isPlatformSupported;
  bool get supportsFrameStreaming => _isWindowsDesktop && Platform.isWindows;
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
      
      // Check if we're on Windows desktop
      _isWindowsDesktop = !kIsWeb && Platform.isWindows;
      
      if (_isWindowsDesktop) {
        return await _initializeWindowsCamera();
      } else {
        return await _initializeStandardCamera();
      }
    } catch (e) {
      _logger.e('Failed to initialize camera service: $e');
      _cameras = [];
      _isInitialized = true;
      _isPlatformSupported = false;
      return false;
    }
  }
  
  Future<bool> _initializeStandardCamera() async {
    try {
      _logger.i('Initializing standard camera...');
      
      _cameras = await availableCameras();
      _isInitialized = true;
      _isPlatformSupported = true;
      
      _logger.i('Standard camera initialized. Found ${_cameras?.length ?? 0} cameras');
      
      if (_cameras?.isNotEmpty == true) {
        for (final camera in _cameras!) {
          _logger.i('Camera found: ${camera.name} (${camera.lensDirection})');
        }
      } else {
        _logger.w('No cameras detected on this system');
      }
      
      return _cameras?.isNotEmpty ?? false;
    } catch (e) {
      _logger.e('Standard camera initialization failed: $e');
      
      if (e.toString().contains('MissingPluginException')) {
        _logger.w('Standard camera plugin not available for this platform');
        _isPlatformSupported = false;
        return await _tryWindowsCameraFallback();
      }
      
      _cameras = [];
      _isInitialized = true;
      return false;
    }
  }
  
  Future<bool> _initializeWindowsCamera() async {
    try {
      _logger.i('Initializing Windows desktop camera...');
      
      // Register Windows camera platform
      camera_windows.CameraWindows.registerWith();
      
      // Get available cameras using standard API
      _cameras = await availableCameras();
      
      _isInitialized = true;
      _isPlatformSupported = true;
      
      _logger.i('Windows camera initialized. Found ${_cameras?.length ?? 0} cameras');
      
      if (_cameras?.isNotEmpty == true) {
        for (final camera in _cameras!) {
          _logger.i('Windows camera found: ${camera.name} (${camera.lensDirection})');
        }
        _logger.i('Windows desktop camera platform registered for frame streaming support');
        return true;
      } else {
        _logger.w('No Windows cameras detected');
        return false;
      }
    } catch (e) {
      _logger.e('Windows camera initialization failed: $e');
      // Fallback to standard camera attempt
      return await _tryStandardCameraFallback();
    }
  }
  
  Future<bool> _tryWindowsCameraFallback() async {
    if (Platform.isWindows) {
      _logger.i('Trying Windows camera fallback...');
      return await _initializeWindowsCamera();
    }
    return false;
  }
  
  Future<bool> _tryStandardCameraFallback() async {
    _logger.i('Trying standard camera fallback...');
    _windowsCameraId = null;
    _isWindowsDesktop = false;
    return await _initializeStandardCamera();
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
    final windowsSupportText = _isWindowsDesktop ? ' (Windows desktop support)' : '';
    final streamingSupportText = supportsFrameStreaming ? ' with frame streaming' : '';
    
    if (frontCameraAvailable && backCameraAvailable) {
      return 'Front & back cameras ready$windowsSupportText$streamingSupportText';
    } else if (frontCameraAvailable) {
      return 'Front camera ready$windowsSupportText$streamingSupportText';
    } else if (backCameraAvailable) {
      return 'Back camera ready$windowsSupportText$streamingSupportText';
    } else {
      return 'Camera detected but not accessible$windowsSupportText';
    }
  }

  bool isCameraHealthy() {
    return _isInitialized && _isPlatformSupported && hasCameras;
  }
  
  // Windows-specific frame streaming methods
  Future<bool> startFrameStreaming(CameraDescription camera, Function(Uint8List) onFrameAvailable) async {
    if (!supportsFrameStreaming) {
      _logger.w('Frame streaming not supported on this platform');
      return false;
    }
    
    try {
      _logger.i('Starting Windows camera frame streaming for: ${camera.name}');
      
      // Create camera instance for Windows frame streaming if not exists
      if (_windowsCameraId == null) {
        _windowsCameraId = await camera_platform.CameraPlatform.instance.createCamera(
          camera,
          ResolutionPreset.medium,
          enableAudio: false,
        );
        
        // Initialize the camera
        await camera_platform.CameraPlatform.instance.initializeCamera(_windowsCameraId!);
      }
      
      // Set up frame streaming subscription using camera_windows API
      _frameSubscription = (camera_platform.CameraPlatform.instance as camera_windows.CameraWindows)
          .onFrameAvailable(_windowsCameraId!)
          .listen(
        (camera_windows.FrameAvailabledEvent event) {
          _logger.d('Frame received: ${event.bytes.length} bytes');
          onFrameAvailable(event.bytes);
        },
        onError: (error) {
          _logger.e('Frame streaming error: $error');
        },
      );
      
      _logger.i('Windows camera frame streaming started successfully');
      return true;
    } catch (e) {
      _logger.e('Failed to start frame streaming: $e');
      return false;
    }
  }
  
  Future<void> stopFrameStreaming() async {
    if (_frameSubscription != null) {
      _logger.i('Stopping Windows camera frame streaming');
      await _frameSubscription!.cancel();
      _frameSubscription = null;
    }
  }
  
  void dispose() {
    stopFrameStreaming();
    if (_windowsCameraId != null) {
      camera_platform.CameraPlatform.instance.dispose(_windowsCameraId!);
      _windowsCameraId = null;
    }
  }
}