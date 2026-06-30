import 'dart:convert';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import 'package:shared_preferences/shared_preferences.dart';
import '../models/user_model.dart';
import '../core/constants/app_constants.dart';

class StorageService {
  final FlutterSecureStorage _secureStorage;

  StorageService([FlutterSecureStorage? secureStorage])
      : _secureStorage = secureStorage ?? const FlutterSecureStorage();

  Future<void> saveSession(String accessToken, String refreshToken, UserModel user) async {
    await _secureStorage.write(key: AppConstants.secureTokenKey, value: accessToken);
    await _secureStorage.write(key: AppConstants.secureRefreshTokenKey, value: refreshToken);

    final prefs = await SharedPreferences.getInstance();
    await prefs.setString(AppConstants.localUserKey, jsonEncode(user.toJson()));
  }

  Future<void> clearSession() async {
    await _secureStorage.delete(key: AppConstants.secureTokenKey);
    await _secureStorage.delete(key: AppConstants.secureRefreshTokenKey);

    final prefs = await SharedPreferences.getInstance();
    await prefs.remove(AppConstants.localUserKey);
  }

  Future<String?> getAccessToken() async {
    return await _secureStorage.read(key: AppConstants.secureTokenKey);
  }

  Future<String?> getRefreshToken() async {
    return await _secureStorage.read(key: AppConstants.secureRefreshTokenKey);
  }

  Future<UserModel?> getCachedUser() async {
    final prefs = await SharedPreferences.getInstance();
    final userJson = prefs.getString(AppConstants.localUserKey);
    if (userJson != null) {
      return UserModel.fromJson(jsonDecode(userJson) as Map<String, dynamic>);
    }
    return null;
  }

  Future<bool> hasActiveSession() async {
    final token = await getAccessToken();
    return token != null;
  }
}
