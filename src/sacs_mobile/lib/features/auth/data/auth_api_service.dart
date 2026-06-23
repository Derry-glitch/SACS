import 'models/auth_response_model.dart';
import '../../../core/network/api_client.dart';
import '../../../core/network/api_endpoints.dart';

class AuthApiService {
  final ApiClient _apiClient;

  AuthApiService(this._apiClient);

  Future<AuthResponseModel> login(String email, String password) async {
    final response = await _apiClient.post(
      ApiEndpoints.login,
      data: {
        'email': email,
        'password': password,
      },
    );
    return AuthResponseModel.fromJson(response.data as Map<String, dynamic>);
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
    final response = await _apiClient.post(
      ApiEndpoints.register,
      data: {
        'email': email,
        'password': password,
        'firstName': firstName,
        'lastName': lastName,
        'matriculationNumber': matriculationNumber,
        'academicLevel': academicLevel,
        'institutionId': institutionId,
        'phoneNumber': phoneNumber,
      },
    );
    return AuthResponseModel.fromJson(response.data as Map<String, dynamic>);
  }

  Future<void> logout(String refreshToken) async {
    await _apiClient.post(
      ApiEndpoints.logout,
      data: {
        'refreshToken': refreshToken,
      },
    );
  }
}
