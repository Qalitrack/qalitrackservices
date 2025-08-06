import 'package:logger/logger.dart';
import '../models/transaction_models.dart';
import '../../core/api/api_client.dart';

class TransactionService {
  static final TransactionService _instance = TransactionService._internal();
  factory TransactionService() => _instance;
  TransactionService._internal();

  final Logger _logger = Logger();
  final ApiClient _apiClient = ApiClient();

  Future<TransactionDto> createTransaction(CreateTransactionRequest request) async {
    try {
      _logger.d('Creating transaction for driver: ${request.driverId}');
      
      final response = await _apiClient.post(
        '/api/transactions',
        data: request.toJson(),
      );

      if (response.statusCode == 201 && response.data != null) {
        final transaction = TransactionDto.fromJson(response.data);
        _logger.i('Transaction created: ${transaction.id}');
        return transaction;
      }
      
      throw Exception('Failed to create transaction');
    } catch (e) {
      _logger.e('Error creating transaction: $e');
      rethrow;
    }
  }

  Future<TransactionDto> updateWeight(UpdateWeightRequest request) async {
    try {
      _logger.d('Updating weight for transaction: ${request.transactionId}');
      
      final response = await _apiClient.put(
        '/api/transactions/${request.transactionId}/weight',
        data: request.toJson(),
      );

      if (response.statusCode == 200 && response.data != null) {
        final transaction = TransactionDto.fromJson(response.data);
        _logger.i('Weight updated for transaction: ${transaction.id}');
        return transaction;
      }
      
      throw Exception('Failed to update weight');
    } catch (e) {
      _logger.e('Error updating weight: $e');
      rethrow;
    }
  }

  Future<TransactionDto> completeTransaction(String transactionId) async {
    try {
      _logger.d('Completing transaction: $transactionId');
      
      final response = await _apiClient.put(
        '/api/transactions/$transactionId/complete',
      );

      if (response.statusCode == 200 && response.data != null) {
        final transaction = TransactionDto.fromJson(response.data);
        _logger.i('Transaction completed: ${transaction.id}');
        return transaction;
      }
      
      throw Exception('Failed to complete transaction');
    } catch (e) {
      _logger.e('Error completing transaction: $e');
      rethrow;
    }
  }

  Future<TransactionDto?> getTransaction(String transactionId) async {
    try {
      _logger.d('Fetching transaction: $transactionId');
      
      final response = await _apiClient.get('/api/transactions/$transactionId');

      if (response.statusCode == 200 && response.data != null) {
        final transaction = TransactionDto.fromJson(response.data);
        _logger.d('Transaction fetched: ${transaction.id}');
        return transaction;
      }
      
      return null;
    } catch (e) {
      _logger.e('Error fetching transaction: $e');
      return null;
    }
  }

  Future<List<TransactionDto>> getTransactionsByDriver(String driverId) async {
    try {
      _logger.d('Fetching transactions for driver: $driverId');
      
      final response = await _apiClient.get(
        '/api/transactions',
        queryParameters: {'driverId': driverId},
      );

      if (response.statusCode == 200 && response.data != null) {
        final List<dynamic> transactionsJson = response.data;
        final transactions = transactionsJson
            .map((json) => TransactionDto.fromJson(json))
            .toList();
        
        _logger.d('Fetched ${transactions.length} transactions for driver');
        return transactions;
      }
      
      return [];
    } catch (e) {
      _logger.e('Error fetching driver transactions: $e');
      return [];
    }
  }

  Future<bool> cancelTransaction(String transactionId) async {
    try {
      _logger.d('Cancelling transaction: $transactionId');
      
      final response = await _apiClient.put(
        '/api/transactions/$transactionId/cancel',
      );

      if (response.statusCode == 200) {
        _logger.i('Transaction cancelled: $transactionId');
        return true;
      }
      
      return false;
    } catch (e) {
      _logger.e('Error cancelling transaction: $e');
      return false;
    }
  }
}