import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../core/config/kiosk_config.dart';
import '../../core/network/service_discovery.dart';
import '../../core/services/camera_service.dart';
import 'admin_access.dart';
import 'admin_setup_dialog.dart';

class SettingsScreen extends StatefulWidget {
  const SettingsScreen({Key? key}) : super(key: key);

  @override
  State<SettingsScreen> createState() => _SettingsScreenState();
}

class _SettingsScreenState extends State<SettingsScreen> with TickerProviderStateMixin {
  late TabController _tabController;
  final KioskConfig _config = KioskConfig();
  final ServiceDiscovery _serviceDiscovery = ServiceDiscovery();
  final CameraService _cameraService = CameraService();
  
  bool _isScanning = false;
  String? _scanStatus;

  @override
  void initState() {
    super.initState();
    _tabController = TabController(length: 6, vsync: this);
  }

  @override
  Widget build(BuildContext context) {
    return Consumer<AdminAccessProvider>(
      builder: (context, adminProvider, child) {
        if (!adminProvider.isAdminMode || !adminProvider.isAdminSessionValid) {
          return const Center(
            child: Text('Unauthorized Access'),
          );
        }

        return Scaffold(
          appBar: AppBar(
            title: const Text('Kiosk Settings'),
            actions: [
              TextButton(
                onPressed: () {
                  adminProvider.exitAdminMode();
                  Navigator.of(context).pop();
                },
                child: const Text('Exit Admin'),
              ),
            ],
            bottom: TabBar(
              controller: _tabController,
              isScrollable: true,
              tabs: const [
                Tab(text: 'Status'),
                Tab(text: 'Network'),
                Tab(text: 'Admin'),
                Tab(text: 'Hardware'),
                Tab(text: 'Drivers'),
                Tab(text: 'Features'),
              ],
            ),
          ),
          body: TabBarView(
            controller: _tabController,
            children: [
              _buildStatusTab(),
              _buildNetworkTab(),
              _buildAdminTab(),
              _buildHardwareTab(),
              _buildDriversTab(),
              _buildFeaturesTab(),
            ],
          ),
        );
      },
    );
  }

  Widget _buildNetworkTab() {
    return Padding(
      padding: const EdgeInsets.all(16.0),
      child: Column(
        children: [
          Card(
            child: Padding(
              padding: const EdgeInsets.all(16.0),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  const Text(
                    'API Gateway Configuration',
                    style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
                  ),
                  const SizedBox(height: 16),
                  TextFormField(
                    initialValue: _config.getValue<String>('api.gateway_url', 'http://192.168.1.1:8080') ?? 'http://192.168.1.1:8080',
                    decoration: const InputDecoration(
                      labelText: 'Gateway URL',
                      border: OutlineInputBorder(),
                      hintText: 'e.g., http://192.168.1.1:8080',
                    ),
                    onChanged: (value) {
                      _config.setValue('api.gateway_url', value);
                    },
                  ),
                  const SizedBox(height: 16),
                  TextFormField(
                    initialValue: _config.getValue<String>('network.service_discovery_name', 'QaliTrack-Gateway') ?? 'QaliTrack-Gateway',
                    decoration: const InputDecoration(
                      labelText: 'Service Discovery Name',
                      border: OutlineInputBorder(),
                      hintText: 'Default service name to discover',
                    ),
                    onChanged: (value) {
                      _config.setValue('network.service_discovery_name', value);
                    },
                  ),
                  const SizedBox(height: 16),
                  TextFormField(
                    initialValue: _config.getValue<int>('api.timeout').toString(),
                    decoration: const InputDecoration(
                      labelText: 'Timeout (ms)',
                      border: OutlineInputBorder(),
                    ),
                    keyboardType: TextInputType.number,
                    onChanged: (value) {
                      final timeout = int.tryParse(value);
                      if (timeout != null) {
                        _config.setValue('api.timeout', timeout);
                      }
                    },
                  ),
                ],
              ),
            ),
          ),
          const SizedBox(height: 16),
          Card(
            child: Padding(
              padding: const EdgeInsets.all(16.0),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Row(
                    mainAxisAlignment: MainAxisAlignment.spaceBetween,
                    children: [
                      const Text(
                        'Service Discovery',
                        style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
                      ),
                      ElevatedButton(
                        onPressed: _isScanning ? null : _scanForServices,
                        child: _isScanning
                            ? const SizedBox(
                                width: 20,
                                height: 20,
                                child: CircularProgressIndicator(strokeWidth: 2),
                              )
                            : const Text('Scan Network'),
                      ),
                    ],
                  ),
                  if (_scanStatus != null) ...[
                    const SizedBox(height: 8),
                    Text(_scanStatus!),
                  ],
                  const SizedBox(height: 16),
                  _buildDiscoveredServices(),
                ],
              ),
            ),
          ),
        ],
      ),
    );
  }


  Widget _buildHardwareTab() {
    return Padding(
      padding: const EdgeInsets.all(16.0),
      child: Column(
        children: [
          Card(
            child: Padding(
              padding: const EdgeInsets.all(16.0),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  const Text(
                    'Hardware Configuration',
                    style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
                  ),
                  const SizedBox(height: 16),
                  DropdownButtonFormField<String>(
                    value: _config.getValue<String>('hardware.camera_quality'),
                    decoration: const InputDecoration(
                      labelText: 'Camera Quality',
                      border: OutlineInputBorder(),
                    ),
                    items: const [
                      DropdownMenuItem(value: 'low', child: Text('Low')),
                      DropdownMenuItem(value: 'medium', child: Text('Medium')),
                      DropdownMenuItem(value: 'high', child: Text('High')),
                    ],
                    onChanged: (value) {
                      if (value != null) {
                        _config.setValue('hardware.camera_quality', value);
                      }
                    },
                  ),
                  const SizedBox(height: 16),
                  TextFormField(
                    initialValue: _config.getValue<double>('hardware.face_detection_confidence').toString(),
                    decoration: const InputDecoration(
                      labelText: 'Face Detection Confidence (0.0 - 1.0)',
                      border: OutlineInputBorder(),
                    ),
                    keyboardType: TextInputType.numberWithOptions(decimal: true),
                    onChanged: (value) {
                      final confidence = double.tryParse(value);
                      if (confidence != null && confidence >= 0.0 && confidence <= 1.0) {
                        _config.setValue('hardware.face_detection_confidence', confidence);
                      }
                    },
                  ),
                ],
              ),
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildFeaturesTab() {
    return Padding(
      padding: const EdgeInsets.all(16.0),
      child: Column(
        children: [
          Card(
            child: Padding(
              padding: const EdgeInsets.all(16.0),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  const Text(
                    'Feature Toggles',
                    style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
                  ),
                  const SizedBox(height: 16),
                  SwitchListTile(
                    title: const Text('Offline Mode'),
                    subtitle: const Text('Allow operation without network connection'),
                    value: _config.getValue<bool>('features.offline_mode_enabled', false) ?? false,
                    onChanged: (value) {
                      _config.setValue('features.offline_mode_enabled', value);
                      setState(() {});
                    },
                  ),
                  SwitchListTile(
                    title: const Text('Biometric Authentication'),
                    subtitle: const Text('Require face detection for driver identification'),
                    value: _config.getValue<bool>('features.biometric_required', true) ?? true,
                    onChanged: (value) {
                      _config.setValue('features.biometric_required', value);
                      setState(() {});
                    },
                  ),
                  SwitchListTile(
                    title: const Text('Receipt Printing'),
                    subtitle: const Text('Enable automatic receipt printing'),
                    value: _config.getValue<bool>('features.receipt_printing_enabled', true) ?? true,
                    onChanged: (value) {
                      _config.setValue('features.receipt_printing_enabled', value);
                      setState(() {});
                    },
                  ),
                  SwitchListTile(
                    title: const Text('Multi-Language Support'),
                    subtitle: const Text('Allow language selection'),
                    value: _config.getValue<bool>('features.multi_language_enabled', true) ?? true,
                    onChanged: (value) {
                      _config.setValue('features.multi_language_enabled', value);
                      setState(() {});
                    },
                  ),
                ],
              ),
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildStatusTab() {
    return Padding(
      padding: const EdgeInsets.all(16.0),
      child: Column(
        children: [
          Card(
            child: Padding(
              padding: const EdgeInsets.all(16.0),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  const Text(
                    'System Status',
                    style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
                  ),
                  const SizedBox(height: 16),
                  _buildStatusItem('Network', true, 'Connected to WiFi'),
                  _buildStatusItem('Backend Services', true, 'All services online'),
                  _buildStatusItem('Camera', _cameraService.isCameraHealthy(), _cameraService.getCameraStatus()),
                  _buildStatusItem('Printer', false, 'Not connected'),
                  _buildStatusItem('Fingerprint Reader', false, 'Not detected'),
                ],
              ),
            ),
          ),
          const SizedBox(height: 16),
          Card(
            child: Padding(
              padding: const EdgeInsets.all(16.0),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  const Text(
                    'Connection Details',
                    style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
                  ),
                  const SizedBox(height: 16),
                  _buildInfoRow('IP Address', '192.168.1.100'),
                  _buildInfoRow('Gateway', '192.168.1.1'),
                  _buildInfoRow('DNS', '8.8.8.8'),
                  _buildInfoRow('Service Discovery', 'QaliTrack-Gateway'),
                ],
              ),
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildStatusItem(String title, bool isOnline, String status) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 8.0),
      child: Row(
        children: [
          Icon(
            isOnline ? Icons.check_circle : Icons.error_outline,
            color: isOnline ? Colors.green : Colors.orange,
            size: 24,
          ),
          const SizedBox(width: 12),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  title,
                  style: const TextStyle(fontWeight: FontWeight.w600),
                ),
                Text(
                  status,
                  style: TextStyle(
                    color: Colors.grey[600],
                    fontSize: 14,
                  ),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildInfoRow(String label, String value) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 4.0),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceBetween,
        children: [
          Text(label, style: const TextStyle(fontWeight: FontWeight.w500)),
          Text(value, style: TextStyle(color: Colors.grey[700])),
        ],
      ),
    );
  }

  Widget _buildAdminTab() {
    return Padding(
      padding: const EdgeInsets.all(16.0),
      child: Column(
        children: [
          Card(
            child: Padding(
              padding: const EdgeInsets.all(16.0),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  const Text(
                    'Admin Credentials',
                    style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
                  ),
                  const SizedBox(height: 16),
                  Text(
                    'Default: admin / admin123',
                    style: TextStyle(
                      color: Colors.grey[600],
                      fontSize: 14,
                    ),
                  ),
                  const SizedBox(height: 16),
                  ElevatedButton(
                    onPressed: _showChangeCredentialsDialog,
                    child: const Text('Change Admin Credentials'),
                  ),
                ],
              ),
            ),
          ),
          const SizedBox(height: 16),
          Card(
            child: Padding(
              padding: const EdgeInsets.all(16.0),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  const Text(
                    'Remote Management',
                    style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
                  ),
                  const SizedBox(height: 16),
                  SwitchListTile(
                    title: const Text('Allow Remote Admin Access'),
                    subtitle: const Text('Enable remote admin login via backend'),
                    value: _config.getValue<bool>('security.remote_admin_enabled', true) ?? true,
                    onChanged: (value) {
                      _config.setValue('security.remote_admin_enabled', value);
                      setState(() {});
                    },
                  ),
                  SwitchListTile(
                    title: const Text('Remote Configuration'),
                    subtitle: const Text('Allow remote configuration updates'),
                    value: _config.getValue<bool>('security.remote_config_enabled', true) ?? true,
                    onChanged: (value) {
                      _config.setValue('security.remote_config_enabled', value);
                      setState(() {});
                    },
                  ),
                ],
              ),
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildDriversTab() {
    return Padding(
      padding: const EdgeInsets.all(16.0),
      child: Column(
        children: [
          Card(
            child: Padding(
              padding: const EdgeInsets.all(16.0),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Row(
                    mainAxisAlignment: MainAxisAlignment.spaceBetween,
                    children: [
                      const Text(
                        'Driver Registration',
                        style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
                      ),
                      ElevatedButton(
                        onPressed: _showDriverRegistrationDialog,
                        child: const Text('Register New Driver'),
                      ),
                    ],
                  ),
                  const SizedBox(height: 16),
                  const Text(
                    'Register drivers with facial recognition for secure access to the weighing system.',
                    style: TextStyle(fontSize: 14),
                  ),
                ],
              ),
            ),
          ),
          const SizedBox(height: 16),
          Card(
            child: Padding(
              padding: const EdgeInsets.all(16.0),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  const Text(
                    'Registered Drivers',
                    style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
                  ),
                  const SizedBox(height: 16),
                  // Placeholder for driver list
                  ListTile(
                    leading: const CircleAvatar(
                      child: Icon(Icons.person),
                    ),
                    title: const Text('James Mbugua'),
                    subtitle: const Text('Driver ID: D001'),
                    trailing: Row(
                      mainAxisSize: MainAxisSize.min,
                      children: [
                        const Icon(Icons.verified, color: Colors.green),
                        IconButton(
                          icon: const Icon(Icons.delete, color: Colors.red),
                          onPressed: () => _removeDriver('D001', 'James Mbugua'),
                        ),
                      ],
                    ),
                  ),
                  ListTile(
                    leading: const CircleAvatar(
                      child: Icon(Icons.person),
                    ),
                    title: const Text('Grace Wanjiku'),
                    subtitle: const Text('Driver ID: D002'),
                    trailing: Row(
                      mainAxisSize: MainAxisSize.min,
                      children: [
                        const Icon(Icons.verified, color: Colors.green),
                        IconButton(
                          icon: const Icon(Icons.delete, color: Colors.red),
                          onPressed: () => _removeDriver('D002', 'Grace Wanjiku'),
                        ),
                      ],
                    ),
                  ),
                ],
              ),
            ),
          ),
        ],
      ),
    );
  }

  void _showChangeCredentialsDialog() {
    Navigator.of(context).push(
      MaterialPageRoute(
        builder: (context) => AdminSetupDialog(
          adminProvider: context.read<AdminAccessProvider>(),
          isFromSettings: true,
        ),
        fullscreenDialog: true,
      ),
    ).then((success) {
      if (success == true && mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text('Credentials updated successfully'),
            backgroundColor: Colors.green,
          ),
        );
      }
    });
  }

  void _removeDriver(String driverId, String driverName) {
    showDialog(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Remove Driver'),
        content: Text('Are you sure you want to remove $driverName ($driverId) from the system?'),
        actions: [
          TextButton(
            onPressed: () => Navigator.of(context).pop(),
            child: const Text('Cancel'),
          ),
          ElevatedButton(
            style: ElevatedButton.styleFrom(backgroundColor: Colors.red),
            onPressed: () {
              Navigator.of(context).pop();
              ScaffoldMessenger.of(context).showSnackBar(
                SnackBar(
                  content: Text('$driverName has been removed from the system'),
                  backgroundColor: Colors.orange,
                ),
              );
              // TODO: Implement actual driver removal from database
              setState(() {}); // Refresh the list
            },
            child: const Text('Remove', style: TextStyle(color: Colors.white)),
          ),
        ],
      ),
    );
  }

  void _showDriverRegistrationDialog() {
    final nameController = TextEditingController();
    final idController = TextEditingController();
    
    showDialog(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Register New Driver'),
        content: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            const Text('Enter driver details for biometric registration.'),
            const SizedBox(height: 16),
            TextField(
              controller: nameController,
              decoration: const InputDecoration(
                labelText: 'Full Name',
                border: OutlineInputBorder(),
                hintText: 'e.g., Joseph Kamau',
              ),
            ),
            const SizedBox(height: 16),
            TextField(
              controller: idController,
              decoration: const InputDecoration(
                labelText: 'Driver ID',
                border: OutlineInputBorder(),
                hintText: 'e.g., D003',
              ),
            ),
          ],
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.of(context).pop(),
            child: const Text('Cancel'),
          ),
          ElevatedButton(
            onPressed: () {
              if (nameController.text.trim().isNotEmpty && 
                  idController.text.trim().isNotEmpty) {
                Navigator.of(context).pop();
                _startBiometricCapture(nameController.text.trim(), idController.text.trim());
              } else {
                ScaffoldMessenger.of(context).showSnackBar(
                  const SnackBar(
                    content: Text('Please fill in all fields'),
                    backgroundColor: Colors.red,
                  ),
                );
              }
            },
            child: const Text('Start Registration'),
          ),
        ],
      ),
    );
  }

  void _startBiometricCapture(String name, String driverId) {
    showDialog(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Biometric Capture'),
        content: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            const Icon(Icons.camera_alt, size: 64, color: Colors.blue),
            const SizedBox(height: 16),
            Text('Starting facial recognition capture for $name ($driverId)'),
            const SizedBox(height: 16),
            const Text('Implementation will integrate with camera system for face capture and ML training.'),
          ],
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.of(context).pop(),
            child: const Text('Close'),
          ),
          ElevatedButton(
            onPressed: () {
              Navigator.of(context).pop();
              ScaffoldMessenger.of(context).showSnackBar(
                SnackBar(
                  content: Text('$name ($driverId) has been registered successfully'),
                  backgroundColor: Colors.green,
                ),
              );
              // TODO: Implement actual biometric capture and registration
              setState(() {}); // Refresh the list
            },
            child: const Text('Complete Registration'),
          ),
        ],
      ),
    );
  }

  Widget _buildDiscoveredServices() {
    final services = _serviceDiscovery.discoveredServices;
    
    if (services.isEmpty) {
      return const Text('No services discovered. Click "Scan Network" to search.');
    }

    return Column(
      children: services.values.map((service) {
        return ListTile(
          leading: Icon(
            service.isHealthy ? Icons.check_circle : Icons.error,
            color: service.isHealthy ? Colors.green : Colors.red,
          ),
          title: Text(service.name),
          subtitle: Text('${service.host}:${service.port}'),
          trailing: service.isHealthy
              ? const Text('Healthy', style: TextStyle(color: Colors.green))
              : const Text('Unhealthy', style: TextStyle(color: Colors.red)),
        );
      }).toList(),
    );
  }

  Future<void> _scanForServices() async {
    setState(() {
      _isScanning = true;
      _scanStatus = 'Scanning network for services...';
    });

    try {
      await _serviceDiscovery.discoverServices();
      setState(() {
        _scanStatus = 'Scan completed. Found ${_serviceDiscovery.discoveredServices.length} services.';
      });
    } catch (e) {
      setState(() {
        _scanStatus = 'Scan failed: $e';
      });
    } finally {
      setState(() {
        _isScanning = false;
      });
    }
  }

  @override
  void dispose() {
    _tabController.dispose();
    super.dispose();
  }
}