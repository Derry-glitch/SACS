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

  // --- Offline Cache Support ---
  Future<void> saveCache(String key, dynamic data) async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.setString('cache_$key', jsonEncode(data));
  }

  Future<dynamic> getCache(String key) async {
    final prefs = await SharedPreferences.getInstance();
    final jsonStr = prefs.getString('cache_$key');
    if (jsonStr != null) {
      try {
        return jsonDecode(jsonStr);
      } catch (_) {
        return null;
      }
    }
    return null;
  }

  Future<void> clearCache() async {
    final prefs = await SharedPreferences.getInstance();
    final keys = prefs.getKeys().where((k) => k.startsWith('cache_')).toList();
    for (final key in keys) {
      await prefs.remove(key);
    }
  }

  // --- PIN Lock Support ---
  static const String _securePinKey = 'sacs_user_pin';

  Future<void> savePin(String pin) async {
    await _secureStorage.write(key: _securePinKey, value: pin);
  }

  Future<String?> getPin() async {
    return await _secureStorage.read(key: _securePinKey);
  }

  Future<void> clearPin() async {
    await _secureStorage.delete(key: _securePinKey);
  }
}
