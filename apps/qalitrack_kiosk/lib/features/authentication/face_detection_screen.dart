import 'dart:typed_data';
import 'dart:ui' as ui;
import 'dart:math';
import 'dart:io';
import 'package:flutter/material.dart';
import 'package:flutter/foundation.dart';
import 'package:camera/camera.dart';
import 'package:google_mlkit_face_detection/google_mlkit_face_detection.dart';
import 'package:logger/logger.dart';
import '../../shared/services/biometric_service.dart';
import '../../shared/models/biometric_models.dart';
import '../../core/config/kiosk_config.dart';
import 'package:flutter_gen/gen_l10n/app_localizations.dart';

class FaceDetectionScreen extends StatefulWidget {
  final Function(DriverDto driver) onDriverIdentified;
  final VoidCallback onCancel;

  const FaceDetectionScreen({
    Key? key,
    required this.onDriverIdentified,
    required this.onCancel,
  }) : super(key: key);

  @override
  State<FaceDetectionScreen> createState() => _FaceDetectionScreenState();
}

class _FaceDetectionScreenState extends State<FaceDetectionScreen> {
  final Logger _logger = Logger();
  final BiometricService _biometricService = BiometricService();
  final KioskConfig _config = KioskConfig();
  
  CameraController? _cameraController;
  FaceDetector? _faceDetector;
  bool _isDetecting = false;
  bool _isProcessing = false;
  List<Face> _faces = [];
  String _status = '';
  int _detectionAttempts = 0;
  static const int _maxDetectionAttempts = 10;

  @override
  void initState() {
    super.initState();
    _initializeCamera();
    _initializeFaceDetector();
  }

  Future<void> _initializeCamera() async {
    try {
      // Check platform support first
      if (!kIsWeb && (Platform.isLinux || Platform.isWindows || Platform.isMacOS)) {
        setState(() {
          _status = 'Camera not supported on desktop platforms';
        });
        return;
      }
      
      final cameras = await availableCameras();
      if (cameras.isEmpty) {
        setState(() {
          _status = 'No camera available';
        });
        return;
      }

      // Prefer front camera for kiosk
      CameraDescription? frontCamera;
      for (final camera in cameras) {
        if (camera.lensDirection == CameraLensDirection.front) {
          frontCamera = camera;
          break;
        }
      }

      final selectedCamera = frontCamera ?? cameras.first;
      
      _cameraController = CameraController(
        selectedCamera,
        _getCameraResolution(),
        enableAudio: false,
      );

      await _cameraController!.initialize();
      
      if (mounted) {
        setState(() {
          _status = 'Camera ready';
        });
        _startFaceDetection();
      }
    } catch (e) {
      _logger.e('Camera initialization failed: $e');
      setState(() {
        _status = 'Camera initialization failed';
      });
    }
  }

  ResolutionPreset _getCameraResolution() {
    final quality = _config.getValue<String>('hardware.camera_quality', 'high');
    switch (quality) {
      case 'low': return ResolutionPreset.low;
      case 'medium': return ResolutionPreset.medium;
      case 'high': return ResolutionPreset.high;
      default: return ResolutionPreset.medium;
    }
  }

  void _initializeFaceDetector() {
    final options = FaceDetectorOptions(
      enableContours: true,
      enableLandmarks: true,
      enableClassification: true,
      enableTracking: true,
      minFaceSize: 0.1,
      performanceMode: FaceDetectorMode.accurate,
    );
    
    _faceDetector = FaceDetector(options: options);
  }

  void _startFaceDetection() {
    if (_cameraController == null || !_cameraController!.value.isInitialized) {
      return;
    }

    _cameraController!.startImageStream((CameraImage image) {
      if (!_isDetecting && !_isProcessing) {
        _isDetecting = true;
        _detectFaces(image);
      }
    });
  }

  Future<void> _detectFaces(CameraImage image) async {
    try {
      final inputImage = _convertCameraImageToInputImage(image);
      if (inputImage == null) {
        _isDetecting = false;
        return;
      }

      final detectedFaces = await _faceDetector!.processImage(inputImage);
      
      if (mounted) {
        setState(() {
          _faces = detectedFaces;
          _updateStatus(detectedFaces);
        });

        if (detectedFaces.isNotEmpty && !_isProcessing) {
          await _processFaceDetection(image, detectedFaces);
        }
      }
    } catch (e) {
      _logger.e('Face detection error: $e');
    } finally {
      _isDetecting = false;
    }
  }

  InputImage? _convertCameraImageToInputImage(CameraImage image) {
    try {
      // Convert camera image to InputImage for MLKit
      final WriteBuffer allBytes = WriteBuffer();
      for (final Plane plane in image.planes) {
        allBytes.putUint8List(plane.bytes);
      }
      final bytes = allBytes.done().buffer.asUint8List();

      final Size imageSize = Size(image.width.toDouble(), image.height.toDouble());
      
      final camera = _cameraController!.description;
      final imageRotation = InputImageRotationValue.fromRawValue(camera.sensorOrientation) ?? InputImageRotation.rotation0deg;

      final inputImageFormat = InputImageFormatValue.fromRawValue(image.format.raw) ?? InputImageFormat.nv21;

      final inputImageData = InputImageMetadata(
        size: imageSize,
        rotation: imageRotation,
        format: inputImageFormat,
        bytesPerRow: image.planes.first.bytesPerRow,
      );

      return InputImage.fromBytes(
        bytes: bytes,
        metadata: inputImageData,
      );
    } catch (e) {
      _logger.e('Image conversion error: $e');
      return null;
    }
  }

  void _updateStatus(List<Face> faces) {
    if (faces.isEmpty) {
      _status = AppLocalizations.of(context)?.lookAtCamera ?? 'Look directly at the camera';
    } else if (faces.length > 1) {
      _status = 'Multiple faces detected. Please ensure only one person is visible.';
    } else {
      final face = faces.first;
      if (face.headEulerAngleY!.abs() > 10 || face.headEulerAngleZ!.abs() > 10) {
        _status = 'Please look straight at the camera';
      } else {
        _status = AppLocalizations.of(context)?.holdStill ?? 'Hold still for identification';
      }
    }
  }

  Future<void> _processFaceDetection(CameraImage image, List<Face> faces) async {
    if (_isProcessing || faces.isEmpty) return;

    _isProcessing = true;
    _detectionAttempts++;

    try {
      final face = faces.first;
      
      // Check face quality
      if (!_isFaceQualitySufficient(face)) {
        setState(() {
          _status = 'Please move closer and look straight at the camera';
        });
        _isProcessing = false;
        return;
      }

      // Convert image to bytes for biometric service
      final imageBytes = await _convertCameraImageToBytes(image);
      if (imageBytes == null) {
        _isProcessing = false;
        return;
      }

      setState(() {
        _status = 'Identifying driver...';
      });

      // Attempt driver identification
      final driver = await _biometricService.getDriverByBiometric(imageBytes);
      
      if (driver != null) {
        _logger.i('Driver identified: ${driver.fullName}');
        widget.onDriverIdentified(driver);
      } else {
        setState(() {
          _status = AppLocalizations.of(context)?.faceRecognitionFailed ?? 
                    'Face recognition failed. Please try again.';
        });
        
        if (_detectionAttempts >= _maxDetectionAttempts) {
          _showFallbackOptions();
        }
      }
    } catch (e) {
      _logger.e('Face processing error: $e');
      setState(() {
        _status = 'Recognition error. Please try again.';
      });
    } finally {
      _isProcessing = false;
    }
  }

  bool _isFaceQualitySufficient(Face face) {
    final confidence = _config.getValue<double>('hardware.face_detection_confidence', 0.8);
    
    // Check face size (should be reasonably large)
    final faceSize = face.boundingBox.width * face.boundingBox.height;
    final imageSize = _cameraController!.value.previewSize!.width * 
                     _cameraController!.value.previewSize!.height;
    final faceSizeRatio = faceSize / imageSize;
    
    if (faceSizeRatio < 0.05) return false; // Face too small
    
    // Check head pose
    if (face.headEulerAngleY!.abs() > 15 || face.headEulerAngleZ!.abs() > 15) {
      return false; // Head not facing forward
    }
    
    // Check if eyes are open (if landmarks available)
    if (face.landmarks.isNotEmpty) {
      final leftEye = face.landmarks[FaceLandmarkType.leftEye];
      final rightEye = face.landmarks[FaceLandmarkType.rightEye];
      
      if (leftEye == null || rightEye == null) return false;
    }
    
    return true;
  }

  Future<Uint8List?> _convertCameraImageToBytes(CameraImage image) async {
    try {
      // Convert YUV420 to RGB
      final WriteBuffer allBytes = WriteBuffer();
      for (final Plane plane in image.planes) {
        allBytes.putUint8List(plane.bytes);
      }
      return allBytes.done().buffer.asUint8List();
    } catch (e) {
      _logger.e('Image conversion error: $e');
      return null;
    }
  }

  void _showFallbackOptions() {
    showDialog(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Face Recognition Failed'),
        content: const Text(
          'Unable to identify driver after multiple attempts. '
          'Would you like to try again or use manual identification?'
        ),
        actions: [
          TextButton(
            onPressed: () {
              Navigator.of(context).pop();
              widget.onCancel();
            },
            child: const Text('Cancel'),
          ),
          TextButton(
            onPressed: () {
              Navigator.of(context).pop();
              _resetDetection();
            },
            child: const Text('Try Again'),
          ),
          ElevatedButton(
            onPressed: () {
              Navigator.of(context).pop();
              // Navigate to manual driver selection
              _showManualDriverSelection();
            },
            child: const Text('Manual ID'),
          ),
        ],
      ),
    );
  }

  void _resetDetection() {
    setState(() {
      _detectionAttempts = 0;
      _status = 'Position your face in the camera frame';
      _faces.clear();
    });
  }

  void _showManualDriverSelection() {
    // This would show a list of drivers for manual selection
    // Implementation depends on UI requirements
  }

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context);
    
    return Scaffold(
      backgroundColor: Colors.black,
      appBar: AppBar(
        title: Text(l10n?.faceDetectionTitle ?? 'Face Detection'),
        backgroundColor: Colors.black,
        foregroundColor: Colors.white,
        actions: [
          IconButton(
            onPressed: widget.onCancel,
            icon: const Icon(Icons.close),
          ),
        ],
      ),
      body: Column(
        children: [
          Expanded(
            child: _cameraController?.value.isInitialized == true
                ? Stack(
                    children: [
                      CameraPreview(_cameraController!),
                      _buildFaceOverlay(),
                    ],
                  )
                : const Center(
                    child: CircularProgressIndicator(),
                  ),
          ),
          Container(
            padding: const EdgeInsets.all(16),
            color: Colors.black87,
            child: Column(
              children: [
                Text(
                  _status,
                  style: const TextStyle(
                    color: Colors.white,
                    fontSize: 18,
                    fontWeight: FontWeight.w500,
                  ),
                  textAlign: TextAlign.center,
                ),
                const SizedBox(height: 16),
                Text(
                  l10n?.faceDetectionInstructions ?? 
                  'Please position your face in the camera frame for identification',
                  style: const TextStyle(
                    color: Colors.white70,
                    fontSize: 14,
                  ),
                  textAlign: TextAlign.center,
                ),
                if (_isProcessing) ...[
                  const SizedBox(height: 16),
                  const CircularProgressIndicator(),
                ],
              ],
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildFaceOverlay() {
    if (_faces.isEmpty || _cameraController == null) {
      return const SizedBox.shrink();
    }

    return CustomPaint(
      painter: FaceDetectorPainter(
        _faces,
        _cameraController!.value.previewSize!,
      ),
      size: Size.infinite,
    );
  }

  @override
  void dispose() {
    _cameraController?.stopImageStream();
    _cameraController?.dispose();
    _faceDetector?.close();
    super.dispose();
  }
}

class FaceDetectorPainter extends CustomPainter {
  final List<Face> faces;
  final Size imageSize;

  FaceDetectorPainter(this.faces, this.imageSize);

  @override
  void paint(Canvas canvas, Size size) {
    final Paint paint = Paint()
      ..style = PaintingStyle.stroke
      ..strokeWidth = 2.0
      ..color = Colors.green;

    for (final Face face in faces) {
      final Rect rect = _scaleRect(
        rect: face.boundingBox,
        imageSize: imageSize,
        widgetSize: size,
      );
      
      canvas.drawRect(rect, paint);
      
      // Draw face landmarks if available
      face.landmarks.forEach((type, landmark) {
        if (landmark != null) {
          final point = _scalePoint(
            point: landmark.position,
            imageSize: imageSize,
            widgetSize: size,
          );
          canvas.drawCircle(point, 2, paint);
        }
      });
    }
  }

  Rect _scaleRect({
    required Rect rect,
    required Size imageSize,
    required Size widgetSize,
  }) {
    final double scaleX = widgetSize.width / imageSize.width;
    final double scaleY = widgetSize.height / imageSize.height;

    return Rect.fromLTRB(
      rect.left * scaleX,
      rect.top * scaleY,
      rect.right * scaleX,
      rect.bottom * scaleY,
    );
  }

  Offset _scalePoint({
    required Point<num> point,
    required Size imageSize,
    required Size widgetSize,
  }) {
    final double scaleX = widgetSize.width / imageSize.width;
    final double scaleY = widgetSize.height / imageSize.height;

    return Offset(point.x.toDouble() * scaleX, point.y.toDouble() * scaleY);
  }

  @override
  bool shouldRepaint(FaceDetectorPainter oldDelegate) {
    return oldDelegate.faces != faces;
  }
}