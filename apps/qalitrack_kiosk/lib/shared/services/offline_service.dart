import 'dart:convert';
import 'package:shared_preferences/shared_preferences.dart';
import 'package:logger/logger.dart';
import '../models/biometric_models.dart';
import '../models/transaction_models.dart';
import '../../core/config/kiosk_config.dart';

class OfflineService {
  static final OfflineService _instance = OfflineService._internal();
  factory OfflineService() => _instance;
  OfflineService._internal();

  final Logger _logger = Logger();
  final KioskConfig _config = KioskConfig();
  
  static const String _driversKey = 'offline_drivers';
  static const String _transactionsKey = 'offline_transactions';
  static const String _syncQueueKey = 'sync_queue';
  static const String _lastSyncKey = 'last_sync_timestamp';

  bool _isOfflineMode = false;
  List<DriverDto> _cachedDrivers = [];
  List<TransactionDto> _pendingTransactions = [];

  bool get isOfflineMode => _isOfflineMode;
  List<DriverDto> get cachedDrivers => List.unmodifiable(_cachedDrivers);
  List<TransactionDto> get pendingTransactions => List.unmodifiable(_pendingTransactions);

  Future<void> initialize() async {
    final offlineEnabled = _config.getValue<bool>('features.offline_mode_enabled', true);
    if (offlineEnabled) {
      await _loadOfflineData();
      _logger.i('Offline service initialized');
    }
  }

  void setOfflineMode(bool isOffline) {
    _isOfflineMode = isOffline;
    _logger.i('Offline mode: ${isOffline ? 'enabled' : 'disabled'}');
  }

  Future<void> cacheDrivers(List<DriverDto> drivers) async {
    try {
      _cachedDrivers = drivers;
      final prefs = await SharedPreferences.getInstance();
      final driversJson = drivers.map((d) => d.toJson()).toList();
      await prefs.setString(_driversKey, jsonEncode(driversJson));
      _logger.d('Cached ${drivers.length} drivers for offline use');
    } catch (e) {
      _logger.e('Failed to cache drivers: $e');
    }
  }

  Future<DriverDto?> findDriverOffline(String query) async {
    try {
      final searchQuery = query.toLowerCase();
      
      for (final driver in _cachedDrivers) {
        if (driver.employeeId.toLowerCase().contains(searchQuery) ||
            driver.fullName.toLowerCase().contains(searchQuery) ||
            driver.phoneNumber.contains(searchQuery)) {
          _logger.d('Found offline driver: ${driver.fullName}');
          return driver;
        }
      }
      
      _logger.w('No offline driver found for query: $query');
      return null;
    } catch (e) {
      _logger.e('Error searching offline drivers: $e');
      return null;
    }
  }

  Future<void> storeOfflineTransaction(TransactionDto transaction) async {
    try {
      _pendingTransactions.add(transaction);
      await _savePendingTransactions();
      await _addToSyncQueue('transaction', transaction.toJson());
      _logger.i('Stored offline transaction: ${transaction.id}');
    } catch (e) {
      _logger.e('Failed to store offline transaction: $e');
    }
  }

  Future<List<TransactionDto>> getOfflineTransactions() async {
    return List.from(_pendingTransactions);
  }

  Future<bool> syncWhenOnline() async {
    if (_isOfflineMode) {
      _logger.w('Cannot sync while in offline mode');
      return false;
    }

    try {
      final syncQueue = await _getSyncQueue();
      if (syncQueue.isEmpty) {
        _logger.d('No offline data to sync');
        return true;
      }

      int syncedCount = 0;
      for (final item in syncQueue) {
        final success = await _syncItem(item);
        if (success) {
          syncedCount++;
        }
      }

      if (syncedCount > 0) {
        await _clearSyncedItems(syncedCount);
        await _updateLastSyncTime();
        _logger.i('Synced $syncedCount offline items');
      }

      return syncedCount == syncQueue.length;
    } catch (e) {
      _logger.e('Sync failed: $e');
      return false;
    }
  }

  Future<bool> _syncItem(Map<String, dynamic> item) async {
    try {
      final type = item['type'] as String;
      final data = item['data'] as Map<String, dynamic>;
      final timestamp = DateTime.parse(item['timestamp'] as String);

      switch (type) {
        case 'transaction':
          return await _syncTransaction(data, timestamp);
        case 'weight_reading':
          return await _syncWeightReading(data, timestamp);
        default:
          _logger.w('Unknown sync item type: $type');
          return false;
      }
    } catch (e) {
      _logger.e('Failed to sync item: $e');
      return false;
    }
  }

  Future<bool> _syncTransaction(Map<String, dynamic> data, DateTime timestamp) async {
    try {
      // In a real implementation, this would call the transaction service API
      _logger.d('Syncing transaction from ${timestamp.toIso8601String()}');
      
      // Remove from pending transactions
      _pendingTransactions.removeWhere((t) => t.id == data['id']);
      await _savePendingTransactions();
      
      return true;
    } catch (e) {
      _logger.e('Failed to sync transaction: $e');
      return false;
    }
  }

  Future<bool> _syncWeightReading(Map<String, dynamic> data, DateTime timestamp) async {
    try {
      // In a real implementation, this would call the weight data service API
      _logger.d('Syncing weight reading from ${timestamp.toIso8601String()}');
      return true;
    } catch (e) {
      _logger.e('Failed to sync weight reading: $e');
      return false;
    }
  }

  Future<void> _loadOfflineData() async {
    try {
      final prefs = await SharedPreferences.getInstance();
      
      // Load cached drivers
      final driversJson = prefs.getString(_driversKey);
      if (driversJson != null) {
        final List<dynamic> driversList = jsonDecode(driversJson);
        _cachedDrivers = driversList
            .map((json) => DriverDto.fromJson(json))
            .toList();
        _logger.d('Loaded ${_cachedDrivers.length} cached drivers');
      }
      
      // Load pending transactions
      final transactionsJson = prefs.getString(_transactionsKey);
      if (transactionsJson != null) {
        final List<dynamic> transactionsList = jsonDecode(transactionsJson);
        _pendingTransactions = transactionsList
            .map((json) => TransactionDto.fromJson(json))
            .toList();
        _logger.d('Loaded ${_pendingTransactions.length} pending transactions');
      }
    } catch (e) {
      _logger.e('Failed to load offline data: $e');
    }
  }

  Future<void> _savePendingTransactions() async {
    try {
      final prefs = await SharedPreferences.getInstance();
      final transactionsJson = _pendingTransactions.map((t) => t.toJson()).toList();
      await prefs.setString(_transactionsKey, jsonEncode(transactionsJson));
    } catch (e) {
      _logger.e('Failed to save pending transactions: $e');
    }
  }

  Future<void> _addToSyncQueue(String type, Map<String, dynamic> data) async {
    try {
      final prefs = await SharedPreferences.getInstance();
      final queueJson = prefs.getString(_syncQueueKey) ?? '[]';
      final List<dynamic> queue = jsonDecode(queueJson);
      
      queue.add({
        'type': type,
        'data': data,
        'timestamp': DateTime.now().toIso8601String(),
      });
      
      await prefs.setString(_syncQueueKey, jsonEncode(queue));
    } catch (e) {
      _logger.e('Failed to add to sync queue: $e');
    }
  }

  Future<List<Map<String, dynamic>>> _getSyncQueue() async {
    try {
      final prefs = await SharedPreferences.getInstance();
      final queueJson = prefs.getString(_syncQueueKey) ?? '[]';
      final List<dynamic> queue = jsonDecode(queueJson);
      return queue.cast<Map<String, dynamic>>();
    } catch (e) {
      _logger.e('Failed to get sync queue: $e');
      return [];
    }
  }

  Future<void> _clearSyncedItems(int count) async {
    try {
      final prefs = await SharedPreferences.getInstance();
      final queue = await _getSyncQueue();
      
      if (count >= queue.length) {
        await prefs.remove(_syncQueueKey);
      } else {
        final remaining = queue.sublist(count);
        await prefs.setString(_syncQueueKey, jsonEncode(remaining));
      }
    } catch (e) {
      _logger.e('Failed to clear synced items: $e');
    }
  }

  Future<void> _updateLastSyncTime() async {
    try {
      final prefs = await SharedPreferences.getInstance();
      await prefs.setString(_lastSyncKey, DateTime.now().toIso8601String());
    } catch (e) {
      _logger.e('Failed to update last sync time: $e');
    }
  }

  Future<DateTime?> getLastSyncTime() async {
    try {
      final prefs = await SharedPreferences.getInstance();
      final lastSyncString = prefs.getString(_lastSyncKey);
      return lastSyncString != null ? DateTime.parse(lastSyncString) : null;
    } catch (e) {
      _logger.e('Failed to get last sync time: $e');
      return null;
    }
  }

  Future<void> clearOfflineData() async {
    try {
      final prefs = await SharedPreferences.getInstance();
      await prefs.remove(_driversKey);
      await prefs.remove(_transactionsKey);
      await prefs.remove(_syncQueueKey);
      await prefs.remove(_lastSyncKey);
      
      _cachedDrivers.clear();
      _pendingTransactions.clear();
      
      _logger.i('Cleared all offline data');
    } catch (e) {
      _logger.e('Failed to clear offline data: $e');
    }
  }

  Future<Map<String, dynamic>> getOfflineStats() async {
    final lastSync = await getLastSyncTime();
    return {
      'isOfflineMode': _isOfflineMode,
      'cachedDriversCount': _cachedDrivers.length,
      'pendingTransactionsCount': _pendingTransactions.length,
      'lastSyncTime': lastSync?.toIso8601String(),
      'syncQueueLength': (await _getSyncQueue()).length,
    };
  }
}