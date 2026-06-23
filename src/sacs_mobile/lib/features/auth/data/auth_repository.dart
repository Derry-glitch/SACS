import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import 'package:shared_preferences/shared_preferences.dart';
import 'dart:convert';

import 'auth_api_service.dart';
import 'models/auth_response_model.dart';
import 'models/user_model.dart';
import '../../../core/errors/failures.dart';

class AuthRepository {
  final AuthApiService _apiService;
  final FlutterSecureStorage _secureStorage;

  AuthRepository(this._apiService, [FlutterSecureStorage? secureStorage])
      : _secureStorage = secureStorage ?? const FlutterSecureStorage();

  Future<AuthResponseModel> login(String email, String password) async {
    try {
      final authResponse = await _apiService.login(email, password);
      await _saveSession(authResponse);
      return authResponse;
    } catch (e) {
      if (e is Failure) rethrow;
      throw ServerFailure(e.toString());
    }
  }

  Future<AuthResponseModel> register({
    required String email,
    required String password,
    required String firstName,
    required String lastName,
    required String matriculationNumber,
    required int academicLevel,
    required int institutionId,
    String? phoneNumber,
  }) async {
    try {
      final authResponse = await _apiService.register(
        email: email,
        password: password,
        firstName: firstName,
        lastName: lastName,
        matriculationNumber: matriculationNumber,
        academicLevel: academicLevel,
        institutionId: institutionId,
        phoneNumber: phoneNumber,
      );
      await _saveSession(authResponse);
      return authResponse;
    } catch (e) {
      if (e is Failure) rethrow;
      throw ServerFailure(e.toString());
    }
  }

  Future<void> logout() async {
    try {
      final refreshToken = await _secureStorage.read(key: 'refreshToken');
      if (refreshToken != null) {
        await _apiService.logout(refreshToken);
      }
    } finally {
      await _clearSession();
    }
  }

  Future<UserModel?> getCachedUser() async {
    final prefs = await SharedPreferences.getInstance();
    final userJson = prefs.getString('cached_user');
    if (userJson != null) {
      return UserModel.fromJson(jsonDecode(userJson) as Map<String, dynamic>);
    }
    return null;
  }

  Future<bool> hasActiveSession() async {
    final token = await _secureStorage.read(key: 'accessToken');
    return token != null;
  }

  Future<void> _saveSession(AuthResponseModel response) async {
    await _secureStorage.write(key: 'accessToken', value: response.accessToken);
    await _secureStorage.write(key: 'refreshToken', value: response.refreshToken);

    final prefs = await SharedPreferences.getInstance();
    await prefs.setString('cached_user', jsonEncode(response.user.toJson()));
  }

  Future<void> _clearSession() async {
    await _secureStorage.delete(key: 'accessToken');
    await _secureStorage.delete(key: 'refreshToken');

    final prefs = await SharedPreferences.getInstance();
    await prefs.remove('cached_user');
  }
}
