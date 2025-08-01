import 'package:camera/camera.dart';
import 'package:logger/logger.dart';

class CameraService {
  static final CameraService _instance = CameraService._internal();
  factory CameraService() => _instance;
  CameraService._internal();

  final Logger _logger = Logger();
  List<CameraDescription>? _cameras;
  bool _isInitialized = false;

  bool get isInitialized => _isInitialized;
  bool get hasCameras => _cameras?.isNotEmpty ?? false;
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
      _cameras = await availableCameras();
      _isInitialized = true;
      
      _logger.i('Camera service initialized. Found ${_cameras?.length ?? 0} cameras');
      return true;
    } catch (e) {
      _logger.e('Failed to initialize camera service: $e');
      _cameras = [];
      _isInitialized = true;
      return false;
    }
  }

  String getCameraStatus() {
    if (!_isInitialized) {
      return 'Not initialized';
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
    return _isInitialized && hasCameras;
  }
}