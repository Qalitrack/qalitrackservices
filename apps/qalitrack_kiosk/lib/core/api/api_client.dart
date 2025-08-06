import 'package:dio/dio.dart';
import 'package:flutter_dotenv/flutter_dotenv.dart';
import 'package:logger/logger.dart';
import '../auth/auth_service.dart';

class ApiClient {
  static final ApiClient _instance = ApiClient._internal();
  factory ApiClient() => _instance;
  ApiClient._internal();

  late final Dio _dio;
  final Logger _logger = Logger();
  
  void initialize() {
    final baseUrl = dotenv.env['API_BASE_URL'] ?? 'http://localhost:7000';
    final timeout = int.parse(dotenv.env['API_TIMEOUT'] ?? '30000');

    _dio = Dio(BaseOptions(
      baseUrl: baseUrl,
      connectTimeout: Duration(milliseconds: timeout),
      receiveTimeout: Duration(milliseconds: timeout),
      sendTimeout: Duration(milliseconds: timeout),
      headers: {
        'Content-Type': 'application/json',
        'Accept': 'application/json',
      },
    ));

    _dio.interceptors.add(
      InterceptorsWrapper(
        onRequest: (options, handler) async {
          // Add JWT token if available
          final token = await AuthService().getToken();
          if (token != null) {
            options.headers['Authorization'] = 'Bearer $token';
          }
          
          _logger.d('Request: ${options.method} ${options.path}');
          if (options.data != null) {
            _logger.d('Request Data: ${options.data}');
          }
          
          handler.next(options);
        },
        onResponse: (response, handler) {
          _logger.d('Response: ${response.statusCode} ${response.requestOptions.path}');
          handler.next(response);
        },
        onError: (error, handler) {
          _logger.e('API Error: ${error.message}');
          _logger.e('Error Details: ${error.response?.data}');
          handler.next(error);
        },
      ),
    );

    // Add logging interceptor in debug mode
    if (dotenv.env['DEBUG_MODE'] == 'true') {
      _dio.interceptors.add(LogInterceptor(
        requestBody: true,
        responseBody: true,
        requestHeader: true,
        responseHeader: false,
        error: true,
        logPrint: (obj) => _logger.d(obj),
      ));
    }
  }

  Dio get dio => _dio;

  Future<Response<T>> get<T>(
    String path, {
    Map<String, dynamic>? queryParameters,
    Options? options,
  }) async {
    try {
      return await _dio.get<T>(
        path,
        queryParameters: queryParameters,
        options: options,
      );
    } on DioException catch (e) {
      throw _handleDioError(e);
    }
  }

  Future<Response<T>> post<T>(
    String path, {
    dynamic data,
    Map<String, dynamic>? queryParameters,
    Options? options,
  }) async {
    try {
      return await _dio.post<T>(
        path,
        data: data,
        queryParameters: queryParameters,
        options: options,
      );
    } on DioException catch (e) {
      throw _handleDioError(e);
    }
  }

  Future<Response<T>> put<T>(
    String path, {
    dynamic data,
    Map<String, dynamic>? queryParameters,
    Options? options,
  }) async {
    try {
      return await _dio.put<T>(
        path,
        data: data,
        queryParameters: queryParameters,
        options: options,
      );
    } on DioException catch (e) {
      throw _handleDioError(e);
    }
  }

  Future<Response<T>> delete<T>(
    String path, {
    dynamic data,
    Map<String, dynamic>? queryParameters,
    Options? options,
  }) async {
    try {
      return await _dio.delete<T>(
        path,
        data: data,
        queryParameters: queryParameters,
        options: options,
      );
    } on DioException catch (e) {
      throw _handleDioError(e);
    }
  }

  ApiException _handleDioError(DioException error) {
    switch (error.type) {
      case DioExceptionType.connectionTimeout:
      case DioExceptionType.sendTimeout:
      case DioExceptionType.receiveTimeout:
        return ApiException(
          'Connection timeout. Please check your internet connection.',
          ApiErrorType.timeout,
        );
      case DioExceptionType.badResponse:
        final statusCode = error.response?.statusCode;
        final message = error.response?.data?['message'] ?? 
                        error.response?.statusMessage ?? 
                        'Unknown error occurred';
        
        if (statusCode == 401) {
          return ApiException(message, ApiErrorType.unauthorized);
        } else if (statusCode == 403) {
          return ApiException(message, ApiErrorType.forbidden);
        } else if (statusCode == 404) {
          return ApiException(message, ApiErrorType.notFound);
        } else if (statusCode != null && statusCode >= 500) {
          return ApiException(message, ApiErrorType.serverError);
        } else {
          return ApiException(message, ApiErrorType.badRequest);
        }
      case DioExceptionType.cancel:
        return ApiException('Request was cancelled', ApiErrorType.cancelled);
      case DioExceptionType.connectionError:
        return ApiException(
          'No internet connection. Please check your network.',
          ApiErrorType.networkError,
        );
      default:
        return ApiException(
          error.message ?? 'Unknown error occurred',
          ApiErrorType.unknown,
        );
    }
  }
}

class ApiException implements Exception {
  final String message;
  final ApiErrorType type;

  ApiException(this.message, this.type);

  @override
  String toString() => 'ApiException: $message';
}

enum ApiErrorType {
  timeout,
  networkError,
  unauthorized,
  forbidden,
  notFound,
  badRequest,
  serverError,
  cancelled,
  unknown,
}