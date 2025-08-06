import 'dart:convert';
import 'package:shared_preferences/shared_preferences.dart';
import 'package:logger/logger.dart';
import '../../shared/models/auth_models.dart';
import '../api/api_client.dart';

class AuthService {
  static final AuthService _instance = AuthService._internal();
  factory AuthService() => _instance;
  AuthService._internal();

  final Logger _logger = Logger();
  final ApiClient _apiClient = ApiClient();
  
  static const String _tokenKey = 'auth_token';
  static const String _refreshTokenKey = 'refresh_token';
  static const String _userKey = 'user_info';
  static const String _expiryKey = 'token_expiry';

  UserInfo? _currentUser;
  String? _currentToken;
  DateTime? _tokenExpiry;

  UserInfo? get currentUser => _currentUser;
  bool get isAuthenticated => _currentUser != null && _currentToken != null;

  Future<void> initialize() async {
    await _loadStoredAuth();
  }

  Future<LoginResponse?> login(LoginRequest request) async {
    try {
      final response = await _apiClient.post(
        '/api/auth/login',
        data: request.toJson(),
      );

      if (response.statusCode == 200 && response.data != null) {
        final loginResponse = LoginResponse.fromJson(response.data);
        await _storeAuthData(loginResponse);
        _logger.i('Login successful for user: ${loginResponse.user.username}');
        return loginResponse;
      }
      return null;
    } catch (e) {
      _logger.e('Login failed: $e');
      rethrow;
    }
  }

  Future<bool> refreshToken() async {
    try {
      final prefs = await SharedPreferences.getInstance();
      final refreshToken = prefs.getString(_refreshTokenKey);
      
      if (refreshToken == null) {
        _logger.w('No refresh token available');
        return false;
      }

      final request = RefreshTokenRequest(refreshToken: refreshToken);
      final response = await _apiClient.post(
        '/api/auth/refresh',
        data: request.toJson(),
      );

      if (response.statusCode == 200 && response.data != null) {
        final loginResponse = LoginResponse.fromJson(response.data);
        await _storeAuthData(loginResponse);
        _logger.i('Token refreshed successfully');
        return true;
      }
      return false;
    } catch (e) {
      _logger.e('Token refresh failed: $e');
      return false;
    }
  }

  Future<void> logout() async {
    try {
      // Call logout endpoint if authenticated
      if (_currentToken != null) {
        await _apiClient.post('/api/auth/logout');
      }
    } catch (e) {
      _logger.w('Logout API call failed: $e');
    } finally {
      await _clearAuthData();
      _logger.i('User logged out');
    }
  }

  Future<String?> getToken() async {
    if (_currentToken != null && _tokenExpiry != null) {
      // Check if token is about to expire (within 5 minutes)
      if (_tokenExpiry!.isBefore(DateTime.now().add(const Duration(minutes: 5)))) {
        _logger.d('Token expiring soon, attempting refresh');
        final refreshed = await refreshToken();
        if (!refreshed) {
          await logout();
          return null;
        }
      }
    }
    return _currentToken;
  }

  Future<bool> validateToken() async {
    try {
      final response = await _apiClient.get('/api/auth/validate');
      return response.statusCode == 200;
    } catch (e) {
      _logger.w('Token validation failed: $e');
      return false;
    }
  }

  Future<void> _storeAuthData(LoginResponse loginResponse) async {
    final prefs = await SharedPreferences.getInstance();
    
    await prefs.setString(_tokenKey, loginResponse.token);
    await prefs.setString(_userKey, jsonEncode(loginResponse.user.toJson()));
    await prefs.setString(_expiryKey, loginResponse.expiry.toIso8601String());
    
    _currentToken = loginResponse.token;
    _currentUser = loginResponse.user;
    _tokenExpiry = loginResponse.expiry;
  }

  Future<void> _loadStoredAuth() async {
    try {
      final prefs = await SharedPreferences.getInstance();
      
      final token = prefs.getString(_tokenKey);
      final userJson = prefs.getString(_userKey);
      final expiryString = prefs.getString(_expiryKey);
      
      if (token != null && userJson != null && expiryString != null) {
        _currentToken = token;
        _currentUser = UserInfo.fromJson(jsonDecode(userJson));
        _tokenExpiry = DateTime.parse(expiryString);
        
        // Check if token is expired
        if (_tokenExpiry!.isBefore(DateTime.now())) {
          _logger.w('Stored token is expired');
          await _clearAuthData();
        } else {
          _logger.i('Loaded stored auth for user: ${_currentUser!.username}');
        }
      }
    } catch (e) {
      _logger.e('Failed to load stored auth: $e');
      await _clearAuthData();
    }
  }

  Future<void> _clearAuthData() async {
    final prefs = await SharedPreferences.getInstance();
    
    await prefs.remove(_tokenKey);
    await prefs.remove(_refreshTokenKey);
    await prefs.remove(_userKey);
    await prefs.remove(_expiryKey);
    
    _currentToken = null;
    _currentUser = null;
    _tokenExpiry = null;
  }
}