import 'package:flutter/material.dart';
import '../models/user_model.dart';
import '../services/api_service.dart';
import '../services/storage_service.dart';

class AuthProvider extends ChangeNotifier {
  final ApiService _apiService;
  final StorageService _storageService;

  bool _isLoading = false;
  bool _isAuthenticated = false;
  UserModel? _user;
  String? _errorMessage;

  AuthProvider({ApiService? apiService, StorageService? storageService})
      : _apiService = apiService ?? ApiService(),
        _storageService = storageService ?? StorageService() {
    autoLogin();
  }

  bool get isLoading => _isLoading;
  bool get isAuthenticated => _isAuthenticated;
  UserModel? get user => _user;
  String? get errorMessage => _errorMessage;

  void _setLoading(bool value) {
    _isLoading = value;
    notifyListeners();
  }

  void _clearError() {
    _errorMessage = null;
  }

  Future<void> autoLogin() async {
    _setLoading(true);
    _clearError();
    try {
      final hasSession = await _storageService.hasActiveSession();
      if (hasSession) {
        final cachedUser = await _storageService.getCachedUser();
        if (cachedUser != null) {
          _user = cachedUser;
          _isAuthenticated = true;
        }
      }
    } catch (e) {
      _errorMessage = e.toString();
    } finally {
      _setLoading(false);
    }
  }

  Future<void> login(String email, String password) async {
    _setLoading(true);
    _clearError();
    try {
      final response = await _apiService.login(email, password);
      await _storageService.saveSession(
        response.accessToken,
        response.refreshToken,
        response.user,
      );
      _user = response.user;
      _isAuthenticated = true;
    } catch (e) {
      _errorMessage = e.toString();
      _isAuthenticated = false;
      rethrow;
    } finally {
      _setLoading(false);
    }
  }

  Future<void> register({
    required String email,
    required String password,
    required String firstName,
    required String lastName,
    required String matriculationNumber,
    required int academicLevel,
    required int institutionId,
    String? phoneNumber,
  }) async {
    _setLoading(true);
    _clearError();
    try {
      final response = await _apiService.register(
        email: email,
        password: password,
        firstName: firstName,
        lastName: lastName,
        matriculationNumber: matriculationNumber,
        academicLevel: academicLevel,
        institutionId: institutionId,
        phoneNumber: phoneNumber,
      );
      await _storageService.saveSession(
        response.accessToken,
        response.refreshToken,
        response.user,
      );
      _user = response.user;
      _isAuthenticated = true;
    } catch (e) {
      _errorMessage = e.toString();
      _isAuthenticated = false;
      rethrow;
    } finally {
      _setLoading(false);
    }
  }

  Future<void> logout() async {
    _setLoading(true);
    _clearError();
    try {
      final refreshToken = await _storageService.getRefreshToken();
      if (refreshToken != null) {
        await _apiService.logout(refreshToken);
      }
    } catch (e) {
      // Local logout should still proceed even if API call fails
    } finally {
      await _storageService.clearSession();
      _user = null;
      _isAuthenticated = false;
      _setLoading(false);
    }
  }
}
