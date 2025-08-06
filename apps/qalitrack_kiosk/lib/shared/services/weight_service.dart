import 'dart:async';
import 'dart:math';
import 'package:logger/logger.dart';
import '../models/transaction_models.dart';
import '../../core/api/api_client.dart';
import '../../core/config/kiosk_config.dart';

class WeightService {
  static final WeightService _instance = WeightService._internal();
  factory WeightService() => _instance;
  WeightService._internal();

  final Logger _logger = Logger();
  final ApiClient _apiClient = ApiClient();
  final KioskConfig _config = KioskConfig();
  
  StreamController<WeightReadingDto>? _weightStreamController;
  Timer? _weightTimer;
  double _lastStableWeight = 0.0;
  DateTime _lastStableTime = DateTime.now();
  List<double> _recentReadings = [];

  Stream<WeightReadingDto> startWeightStream() {
    _weightStreamController?.close();
    _weightStreamController = StreamController<WeightReadingDto>.broadcast();
    
    _logger.i('Starting weight monitoring stream');
    
    // Start periodic weight readings (simulated for demo)
    _weightTimer = Timer.periodic(const Duration(milliseconds: 500), (timer) {
      _getWeightReading();
    });
    
    return _weightStreamController!.stream;
  }

  void stopWeightStream() {
    _logger.i('Stopping weight monitoring stream');
    _weightTimer?.cancel();
    _weightStreamController?.close();
    _weightStreamController = null;
    _recentReadings.clear();
  }

  Future<WeightReadingDto?> getCurrentWeight() async {
    try {
      final response = await _apiClient.get('/api/weight-data/current');
      
      if (response.statusCode == 200 && response.data != null) {
        final weight = WeightReadingDto.fromJson(response.data);
        _logger.d('Current weight: ${weight.weight} kg, stable: ${weight.isStable}');
        return weight;
      }
      
      return null;
    } catch (e) {
      _logger.e('Error getting current weight: $e');
      // Return simulated weight for demo
      return _generateSimulatedWeight();
    }
  }

  Future<List<WeightReadingDto>> getWeightHistory(
    DateTime startTime,
    DateTime endTime,
  ) async {
    try {
      final response = await _apiClient.get(
        '/api/weight-data/history',
        queryParameters: {
          'startTime': startTime.toIso8601String(),
          'endTime': endTime.toIso8601String(),
        },
      );

      if (response.statusCode == 200 && response.data != null) {
        final List<dynamic> weightsJson = response.data;
        final weights = weightsJson
            .map((json) => WeightReadingDto.fromJson(json))
            .toList();
        
        _logger.d('Fetched ${weights.length} weight readings');
        return weights;
      }
      
      return [];
    } catch (e) {
      _logger.e('Error fetching weight history: $e');
      return [];
    }
  }

  Future<bool> calibrateScale() async {
    try {
      _logger.i('Initiating scale calibration');
      
      final response = await _apiClient.post('/api/weight-data/calibrate');
      
      if (response.statusCode == 200) {
        _logger.i('Scale calibration successful');
        return true;
      }
      
      return false;
    } catch (e) {
      _logger.e('Scale calibration failed: $e');
      return false;
    }
  }

  Future<bool> tareScale() async {
    try {
      _logger.i('Taring scale');
      
      final response = await _apiClient.post('/api/weight-data/tare');
      
      if (response.statusCode == 200) {
        _logger.i('Scale tared successfully');
        return true;
      }
      
      return false;
    } catch (e) {
      _logger.e('Scale tare failed: $e');
      return false;
    }
  }

  void _getWeightReading() async {
    try {
      // Try to get real weight reading from API
      final weight = await getCurrentWeight();
      if (weight != null) {
        _weightStreamController?.add(weight);
        return;
      }
    } catch (e) {
      _logger.w('Failed to get real weight reading, using simulated data: $e');
    }
    
    // Fall back to simulated weight for demo
    final simulatedWeight = _generateSimulatedWeight();
    _weightStreamController?.add(simulatedWeight);
  }

  WeightReadingDto _generateSimulatedWeight() {
    final random = Random();
    
    // Simulate weight fluctuations
    double baseWeight = 1500.0; // Base weight in kg
    double variation = random.nextDouble() * 10 - 5; // ±5 kg variation
    double currentWeight = baseWeight + variation;
    
    // Add some realistic weight patterns
    final now = DateTime.now();
    final secondsInMinute = now.second;
    
    // Simulate loading/unloading patterns
    if (secondsInMinute < 20) {
      // Empty vehicle weight
      currentWeight = 800 + (random.nextDouble() * 100);
    } else if (secondsInMinute < 40) {
      // Loading process - gradually increasing weight
      final progress = (secondsInMinute - 20) / 20;
      currentWeight = 800 + (progress * 1200) + (random.nextDouble() * 50);
    } else {
      // Loaded vehicle weight
      currentWeight = 2000 + (random.nextDouble() * 200);
    }
    
    // Add small random fluctuations
    currentWeight += (random.nextDouble() - 0.5) * 2;
    
    // Track recent readings for stability calculation
    _recentReadings.add(currentWeight);
    if (_recentReadings.length > 10) {
      _recentReadings.removeAt(0);
    }
    
    // Calculate stability
    final isStable = _calculateStability(currentWeight);
    
    return WeightReadingDto(
      weight: double.parse(currentWeight.toStringAsFixed(1)),
      timestamp: now,
      isStable: isStable,
      unit: 'kg',
    );
  }

  bool _calculateStability(double currentWeight) {
    if (_recentReadings.length < 5) return false;
    
    final threshold = _config.getValue<double>('hardware.weight_stability_threshold', 0.1) ?? 0.1;
    
    // Calculate standard deviation of recent readings
    final mean = _recentReadings.reduce((a, b) => a + b) / _recentReadings.length;
    final variance = _recentReadings
        .map((reading) => pow(reading - mean, 2))
        .reduce((a, b) => a + b) / _recentReadings.length;
    final standardDeviation = sqrt(variance);
    
    final isStable = standardDeviation < threshold;
    
    if (isStable && 
        (currentWeight - _lastStableWeight).abs() > threshold ||
        DateTime.now().difference(_lastStableTime).inSeconds > 5) {
      _lastStableWeight = currentWeight;
      _lastStableTime = DateTime.now();
    }
    
    return isStable;
  }

  double get lastStableWeight => _lastStableWeight;
  DateTime get lastStableTime => _lastStableTime;
}