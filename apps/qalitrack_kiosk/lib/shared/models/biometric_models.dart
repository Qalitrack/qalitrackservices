import 'package:json_annotation/json_annotation.dart';

part 'biometric_models.g.dart';

@JsonSerializable()
class BiometricVerificationDto {
  final String biometricType;
  final String biometricData;

  BiometricVerificationDto({
    required this.biometricType,
    required this.biometricData,
  });

  factory BiometricVerificationDto.fromJson(Map<String, dynamic> json) =>
      _$BiometricVerificationDtoFromJson(json);

  Map<String, dynamic> toJson() => _$BiometricVerificationDtoToJson(this);
}

@JsonSerializable()
class BiometricVerificationResultDto {
  final bool isMatch;
  final double confidenceScore;
  final String biometricType;
  final DateTime verificationTimestamp;
  final String? notes;

  BiometricVerificationResultDto({
    required this.isMatch,
    required this.confidenceScore,
    required this.biometricType,
    required this.verificationTimestamp,
    this.notes,
  });

  factory BiometricVerificationResultDto.fromJson(Map<String, dynamic> json) =>
      _$BiometricVerificationResultDtoFromJson(json);

  Map<String, dynamic> toJson() => _$BiometricVerificationResultDtoToJson(this);
}

@JsonSerializable()
class BiometricRegistrationDto {
  final String biometricType;
  final String biometricData;
  final String? description;

  BiometricRegistrationDto({
    required this.biometricType,
    required this.biometricData,
    this.description,
  });

  factory BiometricRegistrationDto.fromJson(Map<String, dynamic> json) =>
      _$BiometricRegistrationDtoFromJson(json);

  Map<String, dynamic> toJson() => _$BiometricRegistrationDtoToJson(this);
}

@JsonSerializable()
class DriverDto {
  final String id;
  final String firstName;
  final String lastName;
  final String middleName;
  final String phoneNumber;
  final String email;
  final String employeeId;
  final String profilePhotoUrl;
  final bool biometricEnabled;
  final DateTime? biometricRegistrationDate;

  DriverDto({
    required this.id,
    required this.firstName,
    required this.lastName,
    required this.middleName,
    required this.phoneNumber,
    required this.email,
    required this.employeeId,
    required this.profilePhotoUrl,
    required this.biometricEnabled,
    this.biometricRegistrationDate,
  });

  factory DriverDto.fromJson(Map<String, dynamic> json) =>
      _$DriverDtoFromJson(json);

  Map<String, dynamic> toJson() => _$DriverDtoToJson(this);

  String get fullName => '$firstName $middleName $lastName'.trim();
}