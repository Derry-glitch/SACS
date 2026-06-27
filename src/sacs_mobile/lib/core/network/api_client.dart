import 'package:dio/dio.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import 'api_endpoints.dart';
import '../errors/failures.dart';

class ApiClient {
  final Dio _dio;
  final FlutterSecureStorage _storage;

  ApiClient({Dio? dio, FlutterSecureStorage? storage})
      : _dio = dio ?? Dio(BaseOptions(baseUrl: ApiEndpoints.baseUrl)),
        _storage = storage ?? const FlutterSecureStorage() {
    _dio.interceptors.add(
      InterceptorsWrapper(
        onRequest: (options, handler) async {
          final token = await _storage.read(key: 'accessToken');
          if (token != null) {
            options.headers['Authorization'] = 'Bearer $token';
          }
          return handler.next(options);
        },
        onError: (DioException error, handler) async {
          if (error.response?.statusCode == 401) {
            final refreshToken = await _storage.read(key: 'refreshToken');
            final accessToken = await _storage.read(key: 'accessToken');
            if (refreshToken != null && accessToken != null) {
              try {
                // Request token refresh
                final dioRefresh = Dio(BaseOptions(baseUrl: ApiEndpoints.baseUrl));
                final response = await dioRefresh.post(
                  ApiEndpoints.refresh,
                  data: {
                    'accessToken': accessToken,
                    'refreshToken': refreshToken,
                  },
                );

                if (response.statusCode == 200) {
                  final data = response.data;
                  final newAccessToken = data['accessToken'];
                  final newRefreshToken = data['refreshToken'];

                  await _storage.write(key: 'accessToken', value: newAccessToken);
                  await _storage.write(key: 'refreshToken', value: newRefreshToken);

                  // Resend request with new token
                  final options = error.requestOptions;
                  options.headers['Authorization'] = 'Bearer $newAccessToken';
                  final cloneResponse = await _dio.fetch(options);
                  return handler.resolve(cloneResponse);
                }
              } catch (_) {
                // If refresh fails, clear storage and bubble up
                await _storage.delete(key: 'accessToken');
                await _storage.delete(key: 'refreshToken');
              }
            }
          }
          return handler.next(error);
        },
      ),
    );
  }

  Future<Response> get(String path, {Map<String, dynamic>? queryParameters}) async {
    try {
      return await _dio.get(path, queryParameters: queryParameters);
    } on DioException catch (e) {
      throw _handleDioError(e);
    }
  }

  Future<Response> post(String path, {dynamic data, Options? options}) async {
    try {
      return await _dio.post(path, data: data, options: options);
    } on DioException catch (e) {
      throw _handleDioError(e);
    }
  }

  Future<Response> put(String path, {dynamic data}) async {
    try {
      return await _dio.put(path, data: data);
    } on DioException catch (e) {
      throw _handleDioError(e);
    }
  }

  Future<Response> delete(String path) async {
    try {
      return await _dio.delete(path);
    } on DioException catch (e) {
      throw _handleDioError(e);
    }
  }

  Failure _handleDioError(DioException error) {
    if (error.response != null) {
      final data = error.response?.data;
      final message = data is Map ? data['message'] ?? data['title'] : null;
      return ServerFailure(message ?? 'Server error ${error.response?.statusCode}');
    }
    if (error.type == DioExceptionType.connectionTimeout ||
        error.type == DioExceptionType.receiveTimeout) {
      return NetworkFailure('Network connection timeout');
    }
    return NetworkFailure('No internet connection');
  }
}
