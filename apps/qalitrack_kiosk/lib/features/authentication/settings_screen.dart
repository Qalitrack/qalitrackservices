import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../core/config/kiosk_config.dart';
import '../../core/network/service_discovery.dart';
import 'admin_access.dart';

class SettingsScreen extends StatefulWidget {
  const SettingsScreen({Key? key}) : super(key: key);

  @override
  State<SettingsScreen> createState() => _SettingsScreenState();
}

class _SettingsScreenState extends State<SettingsScreen> with TickerProviderStateMixin {
  late TabController _tabController;
  final KioskConfig _config = KioskConfig();
  final ServiceDiscovery _serviceDiscovery = ServiceDiscovery();
  
  bool _isScanning = false;
  String? _scanStatus;

  @override
  void initState() {
    super.initState();
    _tabController = TabController(length: 4, vsync: this);
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
              tabs: const [
                Tab(text: 'Network'),
                Tab(text: 'Security'),
                Tab(text: 'Hardware'),
                Tab(text: 'Features'),
              ],
            ),
          ),
          body: TabBarView(
            controller: _tabController,
            children: [
              _buildNetworkTab(),
              _buildSecurityTab(),
              _buildHardwareTab(),
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
                    initialValue: _config.getValue<String>('api.gateway_url'),
                    decoration: const InputDecoration(
                      labelText: 'Gateway URL',
                      border: OutlineInputBorder(),
                    ),
                    onChanged: (value) {
                      _config.setValue('api.gateway_url', value);
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

  Widget _buildSecurityTab() {
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
                    'Security Settings',
                    style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
                  ),
                  const SizedBox(height: 16),
                  TextFormField(
                    initialValue: _config.getValue<int>('security.admin_session_timeout').toString(),
                    decoration: const InputDecoration(
                      labelText: 'Admin Session Timeout (seconds)',
                      border: OutlineInputBorder(),
                    ),
                    keyboardType: TextInputType.number,
                    onChanged: (value) {
                      final timeout = int.tryParse(value);
                      if (timeout != null) {
                        _config.setValue('security.admin_session_timeout', timeout);
                      }
                    },
                  ),
                  const SizedBox(height: 16),
                  TextFormField(
                    initialValue: _config.getValue<int>('security.max_failed_attempts').toString(),
                    decoration: const InputDecoration(
                      labelText: 'Max Failed Login Attempts',
                      border: OutlineInputBorder(),
                    ),
                    keyboardType: TextInputType.number,
                    onChanged: (value) {
                      final attempts = int.tryParse(value);
                      if (attempts != null) {
                        _config.setValue('security.max_failed_attempts', attempts);
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