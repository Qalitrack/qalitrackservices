import 'dart:io';
import 'dart:convert';
import 'package:dio/dio.dart';
import 'package:logger/logger.dart';
import '../config/kiosk_config.dart';

class ServiceDiscovery {
  static final ServiceDiscovery _instance = ServiceDiscovery._internal();
  factory ServiceDiscovery() => _instance;
  ServiceDiscovery._internal();

  final Logger _logger = Logger();
  final KioskConfig _config = KioskConfig();
  final Map<String, ServiceEndpoint> _discoveredServices = {};

  Future<void> discoverServices() async {
    _logger.i('Starting service discovery...');
    
    final gatewayUrl = _config.getValue<String>('api.gateway_url');
    if (gatewayUrl != null) {
      final gatewayHost = Uri.parse(gatewayUrl).host;
      await _scanNetworkForServices(gatewayHost);
    }
    
    await _discoverByBroadcast();
    await _discoverByWellKnownPorts();
    
    _logger.i('Service discovery completed. Found ${_discoveredServices.length} services');
  }

  Map<String, ServiceEndpoint> get discoveredServices => Map.unmodifiable(_discoveredServices);

  Future<void> _scanNetworkForServices(String knownHost) async {
    try {
      // Try to connect to the API Gateway first
      final dio = Dio();
      final response = await dio.get('http://$knownHost:7000/health').timeout(
        const Duration(seconds: 5),
      );
      
      if (response.statusCode == 200) {
        _discoveredServices['api_gateway'] = ServiceEndpoint(
          name: 'api_gateway',
          host: knownHost,
          port: 7000,
          health: '/health',
          isHealthy: true,
        );
        
        // Try to get service registry from gateway
        try {
          final servicesResponse = await dio.get('http://$knownHost:7000/api/services');
          if (servicesResponse.statusCode == 200 && servicesResponse.data is List) {
            for (final service in servicesResponse.data) {
              _addDiscoveredService(ServiceEndpoint.fromJson(service));
            }
          }
        } catch (e) {
          _logger.w('Could not fetch service registry: $e');
        }
      }
    } catch (e) {
      _logger.w('Failed to scan known host $knownHost: $e');
    }
  }

  Future<void> _discoverByBroadcast() async {
    try {
      final socket = await RawDatagramSocket.bind(InternetAddress.anyIPv4, 0);
      socket.broadcastEnabled = true;
      
      final discoveryMessage = jsonEncode({
        'type': 'service_discovery',
        'client': 'qalitrack_kiosk',
        'timestamp': DateTime.now().toIso8601String(),
      });
      
      socket.send(
        utf8.encode(discoveryMessage),
        InternetAddress('255.255.255.255'),
        8765, // Discovery port
      );
      
      await Future.delayed(const Duration(seconds: 3));
      socket.close();
    } catch (e) {
      _logger.w('Broadcast discovery failed: $e');
    }
  }

  Future<void> _discoverByWellKnownPorts() async {
    final localNetwork = await _getLocalNetworkRange();
    if (localNetwork == null) return;

    final wellKnownPorts = [7000, 7001, 7004, 7100, 7101];
    final futures = <Future>[];

    for (int i = 1; i < 255; i++) {
      final host = '${localNetwork.split('.').take(3).join('.')}.$i';
      for (final port in wellKnownPorts) {
        futures.add(_checkServiceAtAddress(host, port));
      }
    }

    await Future.wait(futures);
  }

  Future<String?> _getLocalNetworkRange() async {
    try {
      final interfaces = await NetworkInterface.list();
      for (final interface in interfaces) {
        for (final addr in interface.addresses) {
          if (addr.type == InternetAddressType.IPv4 && 
              !addr.isLoopback && 
              addr.address.startsWith('192.168.')) {
            return addr.address;
          }
        }
      }
    } catch (e) {
      _logger.w('Failed to get local network range: $e');
    }
    return null;
  }

  Future<void> _checkServiceAtAddress(String host, int port) async {
    try {
      final socket = await Socket.connect(host, port, timeout: const Duration(seconds: 2));
      socket.destroy();
      
      // If connection successful, try to identify the service
      final dio = Dio();
      try {
        final response = await dio.get('http://$host:$port/health').timeout(
          const Duration(seconds: 3),
        );
        
        if (response.statusCode == 200) {
          final serviceName = _identifyService(port, response.data);
          if (serviceName != null) {
            _discoveredServices[serviceName] = ServiceEndpoint(
              name: serviceName,
              host: host,
              port: port,
              health: '/health',
              isHealthy: true,
            );
          }
        }
      } catch (_) {
        // Service might not have health endpoint, but is reachable
        final serviceName = _identifyServiceByPort(port);
        if (serviceName != null) {
          _discoveredServices[serviceName] = ServiceEndpoint(
            name: serviceName,
            host: host,
            port: port,
            health: '/health',
            isHealthy: false,
          );
        }
      }
    } catch (_) {
      // Connection failed, service not available
    }
  }

  String? _identifyService(int port, dynamic healthData) {
    // Try to identify service from health response
    if (healthData is Map<String, dynamic> && healthData.containsKey('service')) {
      return healthData['service'].toString().toLowerCase();
    }
    
    return _identifyServiceByPort(port);
  }

  String? _identifyServiceByPort(int port) {
    switch (port) {
      case 7000: return 'api_gateway';
      case 7001: return 'user_service';
      case 7004: return 'driver_service';
      case 7100: return 'transaction_service';
      case 7101: return 'weight_data_service';
      default: return null;
    }
  }

  void _addDiscoveredService(ServiceEndpoint service) {
    _discoveredServices[service.name] = service;
    _logger.d('Discovered service: ${service.name} at ${service.host}:${service.port}');
  }
}

class ServiceEndpoint {
  final String name;
  final String host;
  final int port;
  final String health;
  final bool isHealthy;
  final Map<String, dynamic> metadata;

  ServiceEndpoint({
    required this.name,
    required this.host,
    required this.port,
    required this.health,
    required this.isHealthy,
    this.metadata = const {},
  });

  String get baseUrl => 'http://$host:$port';

  factory ServiceEndpoint.fromJson(Map<String, dynamic> json) {
    return ServiceEndpoint(
      name: json['name'] ?? '',
      host: json['host'] ?? '',
      port: json['port'] ?? 0,
      health: json['health'] ?? '/health',
      isHealthy: json['isHealthy'] ?? false,
      metadata: json['metadata'] ?? {},
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'name': name,
      'host': host,
      'port': port,
      'health': health,
      'isHealthy': isHealthy,
      'metadata': metadata,
    };
  }
}