import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../core/config/kiosk_config.dart';
import '../../core/network/service_discovery.dart';
import '../../core/services/camera_service.dart';
import 'admin_access.dart';
import 'admin_setup_dialog.dart';
import 'driver_registration_dialog.dart';

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
  
  // Drivers list management
  List<Map<String, dynamic>> _registeredDrivers = [
    {
      'name': 'James Mbugua',
      'license': 'DL001234567',
      'isComplete': true,
      'registrationDate': DateTime.now().subtract(const Duration(days: 5)),
    },
    {
      'name': 'Grace Wanjiku',
      'license': 'DL009876543',
      'isComplete': true,
      'registrationDate': DateTime.now().subtract(const Duration(days: 2)),
    },
  ];

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
                  // Dynamic driver list
                  if (_registeredDrivers.isEmpty)
                    const Padding(
                      padding: EdgeInsets.all(16.0),
                      child: Text(
                        'No drivers registered yet.',
                        style: TextStyle(
                          fontSize: 16,
                          color: Colors.grey,
                          fontStyle: FontStyle.italic,
                        ),
                        textAlign: TextAlign.center,
                      ),
                    )
                  else
                    ..._registeredDrivers.asMap().entries.map((entry) {
                      final index = entry.key;
                      final driver = entry.value;
                      return ListTile(
                        leading: CircleAvatar(
                          backgroundColor: driver['isComplete'] ? Colors.green.shade100 : Colors.orange.shade100,
                          child: Icon(
                            Icons.person,
                            color: driver['isComplete'] ? Colors.green.shade700 : Colors.orange.shade700,
                          ),
                        ),
                        title: Text(driver['name']),
                        subtitle: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            Text('License: ${driver['license']}'),
                            Text(
                              driver['isComplete'] 
                                ? 'Facial recognition: Complete'
                                : 'Facial recognition: Incomplete',
                              style: TextStyle(
                                color: driver['isComplete'] ? Colors.green : Colors.orange,
                                fontSize: 12,
                              ),
                            ),
                          ],
                        ),
                        trailing: Row(
                          mainAxisSize: MainAxisSize.min,
                          children: [
                            Icon(
                              driver['isComplete'] ? Icons.verified : Icons.warning_amber,
                              color: driver['isComplete'] ? Colors.green : Colors.orange,
                            ),
                            const SizedBox(width: 8),
                            IconButton(
                              icon: const Icon(Icons.delete, color: Colors.red),
                              onPressed: () => _removeDriver(index, driver['license'], driver['name']),
                            ),
                          ],
                        ),
                      );
                    }).toList(),
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

  void _removeDriver(int index, String driverLicense, String driverName) {
    showDialog(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Remove Driver'),
        content: Text('Are you sure you want to remove $driverName ($driverLicense) from the system?'),
        actions: [
          TextButton(
            onPressed: () => Navigator.of(context).pop(),
            child: const Text('Cancel'),
          ),
          ElevatedButton(
            style: ElevatedButton.styleFrom(backgroundColor: Colors.red),
            onPressed: () {
              Navigator.of(context).pop();
              
              // Remove driver from list
              setState(() {
                _registeredDrivers.removeAt(index);
              });
              
              ScaffoldMessenger.of(context).showSnackBar(
                SnackBar(
                  content: Text('$driverName has been removed from the system'),
                  backgroundColor: Colors.orange,
                  action: SnackBarAction(
                    label: 'UNDO',
                    textColor: Colors.white,
                    onPressed: () {
                      // Re-add the driver (simple undo functionality)
                      setState(() {
                        _registeredDrivers.insert(index, {
                          'name': driverName,
                          'license': driverLicense,
                          'isComplete': true, // Assume it was complete
                          'registrationDate': DateTime.now(),
                        });
                      });
                    },
                  ),
                ),
              );
              // TODO: Implement actual driver removal from database
            },
            child: const Text('Remove', style: TextStyle(color: Colors.white)),
          ),
        ],
      ),
    );
  }

  void _showDriverRegistrationDialog() {
    Navigator.of(context).push(
      MaterialPageRoute(
        builder: (context) => DriverRegistrationDialog(
          onRegistrationComplete: (name, driverLicense, isCaptureComplete) {
            // Add new driver to the list
            setState(() {
              _registeredDrivers.add({
                'name': name,
                'license': driverLicense,
                'isComplete': isCaptureComplete,
                'registrationDate': DateTime.now(),
              });
            });
            
            // Show success message
            ScaffoldMessenger.of(context).showSnackBar(
              SnackBar(
                content: Text(
                  isCaptureComplete 
                    ? '$name has been registered successfully with facial recognition'
                    : '$name has been registered (facial recognition incomplete)',
                ),
                backgroundColor: isCaptureComplete ? Colors.green : Colors.orange,
                action: !isCaptureComplete ? SnackBarAction(
                  label: 'COMPLETE LATER',
                  textColor: Colors.white,
                  onPressed: () {
                    // TODO: Show dialog to complete facial recognition later
                  },
                ) : null,
              ),
            );
          },
        ),
        fullscreenDialog: true,
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