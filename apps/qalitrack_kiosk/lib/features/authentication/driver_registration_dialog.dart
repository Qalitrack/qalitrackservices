import 'package:flutter/material.dart';
import 'package:flutter/foundation.dart';
import 'dart:io';
import 'package:virtual_keyboard_multi_language/virtual_keyboard_multi_language.dart';
import 'package:camera/camera.dart';
import '../../core/services/camera_service.dart';

class DriverRegistrationDialog extends StatefulWidget {
  final Function(String name, String driverLicense, bool isCaptureComplete) onRegistrationComplete;

  const DriverRegistrationDialog({
    Key? key,
    required this.onRegistrationComplete,
  }) : super(key: key);

  @override
  State<DriverRegistrationDialog> createState() => _DriverRegistrationDialogState();
}

class _DriverRegistrationDialogState extends State<DriverRegistrationDialog>
    with SingleTickerProviderStateMixin {
  late TabController _tabController;
  
  // Form controllers
  final TextEditingController _nameController = TextEditingController();
  final TextEditingController _licenseController = TextEditingController();
  
  // Keyboard state
  bool _showKeyboard = false;
  bool _isNameField = true;
  
  // Form validation
  String? _errorMessage;
  bool _isLoading = false;
  
  // Focus nodes
  late FocusNode _nameFocus;
  late FocusNode _licenseFocus;
  late ScrollController _scrollController;
  
  // Field keys for scrolling
  final GlobalKey _nameKey = GlobalKey();
  final GlobalKey _licenseKey = GlobalKey();
  final List<GlobalKey> _fieldKeys = [];
  int _activeFieldIndex = 0;
  
  // Camera state
  final CameraService _cameraService = CameraService();
  bool _isCameraAvailable = false;
  CameraController? _cameraController;
  bool _isCameraInitialized = false;
  bool _isCapturing = false;
  List<String> _capturedImages = [];
  String _cameraStatus = '';
  
  @override
  void initState() {
    super.initState();
    _tabController = TabController(length: 2, vsync: this);
    _scrollController = ScrollController();
    _fieldKeys.addAll([_nameKey, _licenseKey]);
    
    _nameFocus = FocusNode();
    _licenseFocus = FocusNode();
    
    // Setup focus listeners
    _nameFocus.addListener(() {
      if (_nameFocus.hasFocus) {
        setState(() {
          _showKeyboard = true;
          _isNameField = true;
          _activeFieldIndex = 0;
        });
        _scrollToField(0);
      }
    });
    
    _licenseFocus.addListener(() {
      if (_licenseFocus.hasFocus) {
        setState(() {
          _showKeyboard = true;
          _isNameField = false;
          _activeFieldIndex = 1;
        });
        _scrollToField(1);
      }
    });
    
    // Add listeners for Enter key detection
    _nameController.addListener(_handleNameChange);
    _licenseController.addListener(_handleLicenseChange);
    
    // Check camera availability
    _checkCameraAvailability();
  }
  
  @override
  void dispose() {
    _tabController.dispose();
    _scrollController.dispose();
    _nameFocus.dispose();
    _licenseFocus.dispose();
    _nameController.removeListener(_handleNameChange);
    _licenseController.removeListener(_handleLicenseChange);
    _nameController.dispose();
    _licenseController.dispose();
    _cameraController?.dispose();
    super.dispose();
  }
  
  Future<void> _checkCameraAvailability() async {
    try {
      print('Checking camera availability for driver registration...');
      
      final cameras = await availableCameras();
      print('Found ${cameras.length} camera(s) during driver registration check');
      
      for (final camera in cameras) {
        print('Available camera: ${camera.name} (${camera.lensDirection})');
      }
      
      setState(() {
        _isCameraAvailable = cameras.isNotEmpty;
        _cameraStatus = _isCameraAvailable 
          ? 'Camera available for facial recognition (${cameras.length} camera${cameras.length == 1 ? '' : 's'} detected)'
          : 'No camera detected - registration will be incomplete';
      });
    } catch (e) {
      print('Camera check failed: $e');
      setState(() {
        _isCameraAvailable = false;
        _cameraStatus = e.toString().contains('MissingPluginException')
          ? 'Camera plugin not supported on this platform - registration will be incomplete'
          : 'Camera check failed - registration will be incomplete';
      });
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: Colors.white,
      body: Container(
        width: MediaQuery.of(context).size.width,
        height: MediaQuery.of(context).size.height,
        child: Column(
          children: [
            // Header
            Container(
              padding: const EdgeInsets.all(32),
              child: Row(
                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                children: [
                  const Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        'Register New Driver',
                        style: TextStyle(
                          fontSize: 24,
                          fontWeight: FontWeight.bold,
                          color: Colors.black87,
                        ),
                      ),
                      SizedBox(height: 4),
                      Text(
                        'Driver details and facial recognition setup',
                        style: TextStyle(
                          fontSize: 16,
                          color: Colors.black54,
                        ),
                      ),
                    ],
                  ),
                  Row(
                    children: [
                      Container(
                        padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
                        decoration: BoxDecoration(
                          color: _isCameraAvailable ? Colors.green.shade100 : Colors.orange.shade100,
                          borderRadius: BorderRadius.circular(12),
                          border: Border.all(
                            color: _isCameraAvailable ? Colors.green.shade300 : Colors.orange.shade300,
                          ),
                        ),
                        child: Row(
                          mainAxisSize: MainAxisSize.min,
                          children: [
                            Icon(
                              _isCameraAvailable ? Icons.camera_alt : Icons.camera_alt_outlined,
                              size: 16,
                              color: _isCameraAvailable ? Colors.green.shade700 : Colors.orange.shade700,
                            ),
                            const SizedBox(width: 4),
                            Text(
                              _isCameraAvailable ? 'Camera Ready' : 'No Camera',
                              style: TextStyle(
                                fontSize: 12,
                                fontWeight: FontWeight.w500,
                                color: _isCameraAvailable ? Colors.green.shade700 : Colors.orange.shade700,
                              ),
                            ),
                          ],
                        ),
                      ),
                      const SizedBox(width: 12),
                      IconButton(
                        onPressed: () => Navigator.of(context).pop(),
                        icon: const Icon(Icons.close, color: Colors.black87),
                      ),
                    ],
                  ),
                ],
              ),
            ),
            
            // Tab Bar
            Container(
              margin: const EdgeInsets.symmetric(horizontal: 32),
              child: TabBar(
                controller: _tabController,
                labelColor: Colors.blue,
                unselectedLabelColor: Colors.grey,
                indicatorColor: Colors.blue,
                tabs: const [
                  Tab(
                    icon: Icon(Icons.person),
                    text: 'Driver Details',
                  ),
                  Tab(
                    icon: Icon(Icons.camera_alt),
                    text: 'Facial Recognition',
                  ),
                ],
              ),
            ),
            
            // Tab Content
            Expanded(
              child: TabBarView(
                controller: _tabController,
                children: [
                  _buildDetailsTab(),
                  _buildFacialRecognitionTab(),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }
  
  Widget _buildDetailsTab() {
    return Stack(
      children: [
        // Form layer
        SingleChildScrollView(
          controller: _scrollController,
          padding: EdgeInsets.only(
            left: 32,
            right: 32,
            top: 24,
            bottom: _showKeyboard ? 370 : 24,
          ),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              // Driver Name Field
              Container(
                key: _nameKey,
                child: TextField(
                  controller: _nameController,
                  focusNode: _nameFocus,
                  decoration: const InputDecoration(
                    labelText: 'Full Name',
                    border: OutlineInputBorder(),
                    prefixIcon: Icon(Icons.person),
                    hintText: 'e.g., Joseph Kamau Mwangi',
                  ),
                  style: const TextStyle(fontSize: 18),
                  enabled: !_isLoading,
                  readOnly: true, // Prevent system keyboard
                  onTap: () {
                    _nameFocus.requestFocus();
                  },
                ),
              ),
              const SizedBox(height: 24),
              
              // Driver License Field
              Container(
                key: _licenseKey,
                child: TextField(
                  controller: _licenseController,
                  decoration: const InputDecoration(
                    labelText: 'Driver\'s License Number',
                    border: OutlineInputBorder(),
                    prefixIcon: Icon(Icons.credit_card),
                    hintText: 'e.g., DL123456789',
                  ),
                  style: const TextStyle(fontSize: 18),
                  enabled: !_isLoading,
                  readOnly: true, // Prevent system keyboard
                  onTap: () {
                    _licenseFocus.requestFocus();
                  },
                ),
              ),
              const SizedBox(height: 24),
              
              // Camera status information
              Container(
                padding: const EdgeInsets.all(16),
                decoration: BoxDecoration(
                  color: _isCameraAvailable ? Colors.blue.shade50 : Colors.orange.shade50,
                  borderRadius: BorderRadius.circular(8),
                  border: Border.all(
                    color: _isCameraAvailable ? Colors.blue.shade200 : Colors.orange.shade200,
                  ),
                ),
                child: Row(
                  children: [
                    Icon(
                      _isCameraAvailable ? Icons.info_outline : Icons.warning_amber_outlined,
                      color: _isCameraAvailable ? Colors.blue : Colors.orange,
                    ),
                    const SizedBox(width: 12),
                    Expanded(
                      child: Text(
                        _cameraStatus,
                        style: TextStyle(
                          color: _isCameraAvailable ? Colors.blue.shade700 : Colors.orange.shade700,
                          fontSize: 14,
                        ),
                      ),
                    ),
                  ],
                ),
              ),
              
              // Error message
              if (_errorMessage != null) ...[
                const SizedBox(height: 16),
                Container(
                  padding: const EdgeInsets.all(12),
                  decoration: BoxDecoration(
                    color: Colors.red.shade50,
                    borderRadius: BorderRadius.circular(8),
                    border: Border.all(color: Colors.red.shade200),
                  ),
                  child: Row(
                    children: [
                      const Icon(Icons.error_outline, color: Colors.red),
                      const SizedBox(width: 8),
                      Expanded(
                        child: Text(
                          _errorMessage!,
                          style: const TextStyle(color: Colors.red),
                        ),
                      ),
                    ],
                  ),
                ),
              ],
              
              const SizedBox(height: 32),
              
              // Next button
              if (!_showKeyboard)
                SizedBox(
                  width: double.infinity,
                  child: ElevatedButton(
                    onPressed: _isLoading ? null : _validateDetailsAndProceed,
                    style: ElevatedButton.styleFrom(
                      padding: const EdgeInsets.symmetric(vertical: 16),
                      backgroundColor: Colors.blue,
                      foregroundColor: Colors.white,
                    ),
                    child: _isLoading
                        ? const CircularProgressIndicator(color: Colors.white)
                        : const Text(
                            'Next: Facial Recognition Setup',
                            style: TextStyle(fontSize: 16),
                          ),
                  ),
                ),
            ],
          ),
        ),
        
        // Keyboard overlay
        if (_showKeyboard) _buildKeyboardOverlay(),
      ],
    );
  }
  
  Widget _buildFacialRecognitionTab() {
    return Container(
      padding: const EdgeInsets.all(32),
      child: Column(
        children: [
          // Camera preview area
          Expanded(
            flex: 3,
            child: Container(
              width: double.infinity,
              decoration: BoxDecoration(
                border: Border.all(color: Colors.grey.shade300, width: 2),
                borderRadius: BorderRadius.circular(12),
                color: Colors.grey.shade50,
              ),
              child: _isCameraAvailable ? _buildCameraPreview() : _buildNoCameraView(),
            ),
          ),
          
          const SizedBox(height: 24),
          
          // Capture instructions
          Container(
            padding: const EdgeInsets.all(16),
            decoration: BoxDecoration(
              color: Colors.blue.shade50,
              borderRadius: BorderRadius.circular(8),
              border: Border.all(color: Colors.blue.shade200),
            ),
            child: Column(
              children: [
                const Row(
                  children: [
                    Icon(Icons.info_outline, color: Colors.blue),
                    SizedBox(width: 8),
                    Text(
                      'Facial Recognition Setup',
                      style: TextStyle(
                        fontWeight: FontWeight.bold,
                        color: Colors.blue,
                      ),
                    ),
                  ],
                ),
                const SizedBox(height: 8),
                Text(
                  _isCameraAvailable
                      ? 'Position your face in the camera preview and capture multiple angles for best recognition accuracy.'
                      : 'Camera is not available. Driver registration will be saved but facial recognition will need to be set up later.',
                  style: const TextStyle(fontSize: 14),
                ),
              ],
            ),
          ),
          
          const SizedBox(height: 24),
          
          // Action buttons
          Row(
            children: [
              Expanded(
                child: OutlinedButton(
                  onPressed: () => _tabController.animateTo(0),
                  style: OutlinedButton.styleFrom(
                    padding: const EdgeInsets.symmetric(vertical: 16),
                  ),
                  child: const Text('Back to Details'),
                ),
              ),
              const SizedBox(width: 16),
              Expanded(
                child: ElevatedButton(
                  onPressed: _isLoading ? null : _completeRegistration,
                  style: ElevatedButton.styleFrom(
                    padding: const EdgeInsets.symmetric(vertical: 16),
                    backgroundColor: Colors.green,
                    foregroundColor: Colors.white,
                  ),
                  child: _isLoading
                      ? const CircularProgressIndicator(color: Colors.white)
                      : Text(
                          _isCameraAvailable && _capturedImages.isNotEmpty
                              ? 'Complete Registration'
                              : _isCameraAvailable
                                  ? 'Register (No Photos)'
                                  : 'Register (Camera Required Later)',
                          style: const TextStyle(fontSize: 16),
                        ),
                ),
              ),
            ],
          ),
        ],
      ),
    );
  }
  
  Widget _buildCameraPreview() {
    if (_cameraController != null && _isCameraInitialized) {
      return ClipRRect(
        borderRadius: BorderRadius.circular(10),
        child: CameraPreview(_cameraController!),
      );
    } else {
      return const Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            CircularProgressIndicator(),
            SizedBox(height: 16),
            Text('Initializing camera...'),
          ],
        ),
      );
    }
  }
  
  Widget _buildNoCameraView() {
    return const Center(
      child: Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          Icon(
            Icons.camera_alt_outlined,
            size: 64,
            color: Colors.grey,
          ),
          SizedBox(height: 16),
          Text(
            'Camera Not Available',
            style: TextStyle(
              fontSize: 18,
              fontWeight: FontWeight.bold,
              color: Colors.grey,
            ),
          ),
          SizedBox(height: 8),
          Text(
            'Facial recognition setup will need to be\ncompleted later when camera is available',
            textAlign: TextAlign.center,
            style: TextStyle(color: Colors.grey),
          ),
        ],
      ),
    );
  }
  
  Widget _buildKeyboardOverlay() {
    return Positioned(
      bottom: 0,
      left: 0,
      right: 0,
      child: Container(
        decoration: const BoxDecoration(
          color: Colors.white,
        ),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            // Keyboard header
            Container(
              padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
              decoration: BoxDecoration(
                color: Colors.grey.shade100,
                border: Border(bottom: BorderSide(color: Colors.grey.shade300)),
              ),
              child: Row(
                children: [
                  Expanded(
                    child: Text(
                      'Field ${_activeFieldIndex + 1} of 2: ${_isNameField ? 'Full Name' : 'Driver\'s License'}',
                      style: const TextStyle(fontSize: 14, fontWeight: FontWeight.w500),
                    ),
                  ),
                  IconButton(
                    onPressed: _activeFieldIndex > 0 ? () {
                      _nameFocus.requestFocus();
                    } : null,
                    icon: const Icon(Icons.keyboard_arrow_up),
                    tooltip: 'Previous field',
                  ),
                  IconButton(
                    onPressed: _activeFieldIndex < 1 ? () {
                      _licenseFocus.requestFocus();
                    } : null,
                    icon: const Icon(Icons.keyboard_arrow_down),
                    tooltip: 'Next field',
                  ),
                  IconButton(
                    onPressed: () {
                      setState(() {
                        _showKeyboard = false;
                      });
                      FocusScope.of(context).unfocus();
                    },
                    icon: const Icon(Icons.keyboard_hide),
                    tooltip: 'Hide keyboard',
                  ),
                ],
              ),
            ),
            // Virtual Keyboard
            SizedBox(
              height: 300,
              width: double.infinity,
              child: VirtualKeyboard(
                height: 300,
                textColor: Colors.black,
                textController: _isNameField ? _nameController : _licenseController,
                defaultLayouts: const [VirtualKeyboardDefaultLayouts.English],
                type: VirtualKeyboardType.Alphanumeric,
              ),
            ),
          ],
        ),
      ),
    );
  }
  
  void _scrollToField(int fieldIndex) {
    WidgetsBinding.instance.addPostFrameCallback((_) {
      if (_fieldKeys[fieldIndex].currentContext != null) {
        final RenderBox renderBox = _fieldKeys[fieldIndex].currentContext!.findRenderObject() as RenderBox;
        final position = renderBox.localToGlobal(Offset.zero);
        
        final keyboardHeight = 370.0;
        final screenHeight = MediaQuery.of(context).size.height;
        final availableHeight = screenHeight - keyboardHeight;
        final fieldHeight = renderBox.size.height;
        
        final fieldBottomPosition = position.dy + fieldHeight;
        final targetScrollPosition = fieldBottomPosition - availableHeight + 150;
        
        if (targetScrollPosition > 0) {
          _scrollController.animateTo(
            _scrollController.offset + targetScrollPosition,
            duration: const Duration(milliseconds: 400),
            curve: Curves.easeOutCubic,
          );
        }
      }
    });
  }
  
  void _handleNameChange() {
    final text = _nameController.text;
    if (text.contains('\n')) {
      _nameController.text = text.replaceAll('\n', '');
      _licenseFocus.requestFocus();
    }
  }
  
  void _handleLicenseChange() {
    final text = _licenseController.text;
    if (text.contains('\n')) {
      _licenseController.text = text.replaceAll('\n', '');
      _validateDetailsAndProceed();
    }
  }
  
  void _validateDetailsAndProceed() {
    if (_nameController.text.trim().isEmpty) {
      setState(() {
        _errorMessage = 'Please enter the driver\'s full name';
      });
      return;
    }
    
    if (_licenseController.text.trim().isEmpty) {
      setState(() {
        _errorMessage = 'Please enter the driver\'s license number';
      });
      return;
    }
    
    setState(() {
      _errorMessage = null;
      _showKeyboard = false;
    });
    FocusScope.of(context).unfocus();
    
    // Move to facial recognition tab
    _tabController.animateTo(1);
    
    // Initialize camera if available
    if (_isCameraAvailable && _cameraController == null) {
      _initializeCamera();
    }
  }
  
  Future<void> _initializeCamera() async {
    try {
      print('Initializing camera for driver registration...');
      
      final cameras = await availableCameras();
      if (cameras.isNotEmpty) {
        print('Found ${cameras.length} camera(s), selecting best option...');
        
        // Prefer front camera for facial recognition
        CameraDescription? frontCamera;
        CameraDescription? backCamera;
        
        for (final camera in cameras) {
          print('Evaluating camera: ${camera.name} (${camera.lensDirection})');
          if (camera.lensDirection == CameraLensDirection.front) {
            frontCamera = camera;
          } else if (camera.lensDirection == CameraLensDirection.back) {
            backCamera = camera;
          }
        }
        
        final selectedCamera = frontCamera ?? backCamera ?? cameras.first;
        print('Selected camera: ${selectedCamera.name} (${selectedCamera.lensDirection})');
        
        _cameraController = CameraController(
          selectedCamera,
          ResolutionPreset.medium,
          enableAudio: false,
        );
        
        await _cameraController!.initialize();
        print('Camera controller initialized successfully');
        
        if (mounted) {
          setState(() {
            _isCameraInitialized = true;
          });
        }
      } else {
        print('No cameras available for initialization');
        if (mounted) {
          setState(() {
            _isCameraAvailable = false;
            _cameraStatus = 'No cameras found during initialization';
          });
        }
      }
    } catch (e) {
      print('Camera initialization failed: $e');
      if (mounted) {
        setState(() {
          _isCameraAvailable = false;
          _cameraStatus = e.toString().contains('MissingPluginException')
            ? 'Camera plugin not supported on this platform'
            : 'Camera initialization failed: ${e.toString()}';
        });
      }
    }
  }
  
  Future<void> _completeRegistration() async {
    setState(() {
      _isLoading = true;
    });
    
    // Simulate registration process
    await Future.delayed(const Duration(seconds: 1));
    
    final name = _nameController.text.trim();
    final license = _licenseController.text.trim();
    final isCaptureComplete = _isCameraAvailable && _capturedImages.isNotEmpty;
    
    if (mounted) {
      Navigator.of(context).pop();
      widget.onRegistrationComplete(name, license, isCaptureComplete);
    }
  }
}