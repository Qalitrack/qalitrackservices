// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'biometric_models.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

BiometricVerificationDto _$BiometricVerificationDtoFromJson(
        Map<String, dynamic> json) =>
    BiometricVerificationDto(
      biometricType: json['biometricType'] as String,
      biometricData: json['biometricData'] as String,
    );

Map<String, dynamic> _$BiometricVerificationDtoToJson(
        BiometricVerificationDto instance) =>
    <String, dynamic>{
      'biometricType': instance.biometricType,
      'biometricData': instance.biometricData,
    };

BiometricVerificationResultDto _$BiometricVerificationResultDtoFromJson(
        Map<String, dynamic> json) =>
    BiometricVerificationResultDto(
      isMatch: json['isMatch'] as bool,
      confidenceScore: (json['confidenceScore'] as num).toDouble(),
      biometricType: json['biometricType'] as String,
      verificationTimestamp:
          DateTime.parse(json['verificationTimestamp'] as String),
      notes: json['notes'] as String?,
    );

Map<String, dynamic> _$BiometricVerificationResultDtoToJson(
        BiometricVerificationResultDto instance) =>
    <String, dynamic>{
      'isMatch': instance.isMatch,
      'confidenceScore': instance.confidenceScore,
      'biometricType': instance.biometricType,
      'verificationTimestamp': instance.verificationTimestamp.toIso8601String(),
      'notes': instance.notes,
    };

BiometricRegistrationDto _$BiometricRegistrationDtoFromJson(
        Map<String, dynamic> json) =>
    BiometricRegistrationDto(
      biometricType: json['biometricType'] as String,
      biometricData: json['biometricData'] as String,
      description: json['description'] as String?,
    );

Map<String, dynamic> _$BiometricRegistrationDtoToJson(
        BiometricRegistrationDto instance) =>
    <String, dynamic>{
      'biometricType': instance.biometricType,
      'biometricData': instance.biometricData,
      'description': instance.description,
    };

DriverDto _$DriverDtoFromJson(Map<String, dynamic> json) => DriverDto(
      id: json['id'] as String,
      firstName: json['firstName'] as String,
      lastName: json['lastName'] as String,
      middleName: json['middleName'] as String,
      phoneNumber: json['phoneNumber'] as String,
      email: json['email'] as String,
      employeeId: json['employeeId'] as String,
      profilePhotoUrl: json['profilePhotoUrl'] as String,
      biometricEnabled: json['biometricEnabled'] as bool,
      biometricRegistrationDate: json['biometricRegistrationDate'] == null
          ? null
          : DateTime.parse(json['biometricRegistrationDate'] as String),
    );

Map<String, dynamic> _$DriverDtoToJson(DriverDto instance) => <String, dynamic>{
      'id': instance.id,
      'firstName': instance.firstName,
      'lastName': instance.lastName,
      'middleName': instance.middleName,
      'phoneNumber': instance.phoneNumber,
      'email': instance.email,
      'employeeId': instance.employeeId,
      'profilePhotoUrl': instance.profilePhotoUrl,
      'biometricEnabled': instance.biometricEnabled,
      'biometricRegistrationDate':
          instance.biometricRegistrationDate?.toIso8601String(),
    };
