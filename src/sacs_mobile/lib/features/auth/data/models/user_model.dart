class UserModel {
  final int id;
  final String email;
  final String firstName;
  final String lastName;
  final String role;
  final int institutionId;
  final String? matriculationNumber;

  UserModel({
    required this.id,
    required this.email,
    required this.firstName,
    required this.lastName,
    required this.role,
    required this.institutionId,
    this.matriculationNumber,
  });

  factory UserModel.fromJson(Map<String, dynamic> json) {
    return UserModel(
      id: json['id'] as int,
      email: json['email'] as String,
      firstName: json['firstName'] as String,
      lastName: json['lastName'] as String,
      role: json['role'] as String,
      institutionId: json['institutionId'] as int,
      matriculationNumber: json['matriculationNumber'] as String?,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'email': email,
      'firstName': firstName,
      'lastName': lastName,
      'role': role,
      'institutionId': institutionId,
      'matriculationNumber': matriculationNumber,
    };
  }
}
