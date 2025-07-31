import 'package:flutter/material.dart';
import 'package:flutter_gen/gen_l10n/app_localizations.dart';
import '../../shared/models/biometric_models.dart';

class SimpleFaceDetectionScreen extends StatefulWidget {
  final Function(DriverDto driver) onDriverIdentified;
  final VoidCallback onCancel;

  const SimpleFaceDetectionScreen({
    Key? key,
    required this.onDriverIdentified,
    required this.onCancel,
  }) : super(key: key);

  @override
  State<SimpleFaceDetectionScreen> createState() => _SimpleFaceDetectionScreenState();
}

class _SimpleFaceDetectionScreenState extends State<SimpleFaceDetectionScreen>
    with SingleTickerProviderStateMixin {
  late AnimationController _animationController;
  late Animation<double> _pulseAnimation;
  bool _isDetecting = false;

  @override
  void initState() {
    super.initState();
    _animationController = AnimationController(
      duration: const Duration(seconds: 2),
      vsync: this,
    );
    _pulseAnimation = Tween<double>(begin: 0.8, end: 1.2).animate(
      CurvedAnimation(parent: _animationController, curve: Curves.easeInOut),
    );
    _animationController.repeat(reverse: true);
    
    // Simulate face detection after 3 seconds
    Future.delayed(const Duration(seconds: 3), _simulateDetection);
  }

  void _simulateDetection() {
    setState(() {
      _isDetecting = true;
    });
    
    // Simulate a demo driver identification
    Future.delayed(const Duration(seconds: 2), () {
      final demoDriver = DriverDto(
        id: 'demo_001',
        firstName: 'John',
        lastName: 'Demo',
        middleName: 'Test',
        phoneNumber: '+254700000000',
        email: 'john.demo@qalitrack.com',
        employeeId: 'EMP001',
        profilePhotoUrl: '',
        biometricEnabled: true,
        biometricRegistrationDate: DateTime.now(),
      );
      
      widget.onDriverIdentified(demoDriver);
    });
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
            child: Container(
              width: double.infinity,
              decoration: const BoxDecoration(
                gradient: LinearGradient(
                  begin: Alignment.topCenter,
                  end: Alignment.bottomCenter,
                  colors: [Colors.black87, Colors.black54],
                ),
              ),
              child: Column(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  AnimatedBuilder(
                    animation: _pulseAnimation,
                    builder: (context, child) {
                      return Transform.scale(
                        scale: _pulseAnimation.value,
                        child: Container(
                          width: 200,
                          height: 200,
                          decoration: BoxDecoration(
                            shape: BoxShape.circle,
                            border: Border.all(
                              color: _isDetecting ? Colors.green : Colors.blue,
                              width: 4,
                            ),
                            color: Colors.white.withOpacity(0.1),
                          ),
                          child: Icon(
                            _isDetecting ? Icons.person_search : Icons.face,
                            size: 100,
                            color: _isDetecting ? Colors.green : Colors.blue,
                          ),
                        ),
                      );
                    },
                  ),
                  const SizedBox(height: 32),
                  Text(
                    _isDetecting 
                        ? 'Identifying driver...' 
                        : 'Position your face in the circle',
                    style: const TextStyle(
                      color: Colors.white,
                      fontSize: 24,
                      fontWeight: FontWeight.w500,
                    ),
                    textAlign: TextAlign.center,
                  ),
                  if (_isDetecting) ...[
                    const SizedBox(height: 16),
                    const CircularProgressIndicator(color: Colors.green),
                  ],
                ],
              ),
            ),
          ),
          Container(
            padding: const EdgeInsets.all(24),
            color: Colors.black87,
            child: Column(
              children: [
                Text(
                  l10n?.faceDetectionInstructions ?? 
                  'Please position your face in the camera frame for identification',
                  style: const TextStyle(
                    color: Colors.white70,
                    fontSize: 16,
                  ),
                  textAlign: TextAlign.center,
                ),
                const SizedBox(height: 16),
                Text(
                  'Demo Mode: Automatic identification in 5 seconds',
                  style: TextStyle(
                    color: Colors.orange.shade300,
                    fontSize: 14,
                    fontStyle: FontStyle.italic,
                  ),
                  textAlign: TextAlign.center,
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }

  @override
  void dispose() {
    _animationController.dispose();
    super.dispose();
  }
}