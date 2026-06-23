import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../data/auth_repository.dart';
import '../../data/auth_api_service.dart';
import '../../data/models/user_model.dart';
import '../../../../core/network/api_client.dart';

class AuthState {
  final bool isLoading;
  final bool isAuthenticated;
  final UserModel? user;
  final String? errorMessage;

  AuthState({
    this.isLoading = false,
    this.isAuthenticated = false,
    this.user,
    this.errorMessage,
  });

  AuthState copyWith({
    bool? isLoading,
    bool? isAuthenticated,
    UserModel? user,
    String? errorMessage,
  }) {
    return AuthState(
      isLoading: isLoading ?? this.isLoading,
      isAuthenticated: isAuthenticated ?? this.isAuthenticated,
      user: user ?? this.user,
      errorMessage: errorMessage,
    );
  }
}

// Dependancy Injection Providers
final apiClientProvider = Provider<ApiClient>((ref) => ApiClient());
final authApiServiceProvider = Provider<AuthApiService>((ref) {
  final apiClient = ref.watch(apiClientProvider);
  return AuthApiService(apiClient);
});
final authRepositoryProvider = Provider<AuthRepository>((ref) {
  final apiService = ref.watch(authApiServiceProvider);
  return AuthRepository(apiService);
});

class AuthNotifier extends StateNotifier<AuthState> {
  final AuthRepository _repository;

  AuthNotifier(this._repository) : super(AuthState()) {
    _checkInitialAuth();
  }

  Future<void> _checkInitialAuth() async {
    state = state.copyWith(isLoading: true);
    final hasSession = await _repository.hasActiveSession();
    if (hasSession) {
      final cachedUser = await _repository.getCachedUser();
      state = AuthState(isAuthenticated: true, user: cachedUser);
    } else {
      state = AuthState(isAuthenticated: false);
    }
  }

  Future<void> login(String email, String password) async {
    state = state.copyWith(isLoading: true);
    try {
      final authResponse = await _repository.login(email, password);
      state = AuthState(isAuthenticated: true, user: authResponse.user);
    } catch (e) {
      state = AuthState(errorMessage: e.toString());
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
    state = state.copyWith(isLoading: true);
    try {
      final authResponse = await _repository.register(
        email: email,
        password: password,
        firstName: firstName,
        lastName: lastName,
        matriculationNumber: matriculationNumber,
        academicLevel: academicLevel,
        institutionId: institutionId,
        phoneNumber: phoneNumber,
      );
      state = AuthState(isAuthenticated: true, user: authResponse.user);
    } catch (e) {
      state = AuthState(errorMessage: e.toString());
    }
  }

  Future<void> logout() async {
    state = state.copyWith(isLoading: true);
    await _repository.logout();
    state = AuthState(isAuthenticated: false);
  }
}

final authProvider = StateNotifierProvider<AuthNotifier, AuthState>((ref) {
  final repository = ref.watch(authRepositoryProvider);
  return AuthNotifier(repository);
});
