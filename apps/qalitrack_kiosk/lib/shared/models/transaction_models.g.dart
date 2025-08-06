// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'transaction_models.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

WeightReadingDto _$WeightReadingDtoFromJson(Map<String, dynamic> json) =>
    WeightReadingDto(
      weight: (json['weight'] as num).toDouble(),
      timestamp: DateTime.parse(json['timestamp'] as String),
      isStable: json['isStable'] as bool,
      unit: json['unit'] as String? ?? 'kg',
    );

Map<String, dynamic> _$WeightReadingDtoToJson(WeightReadingDto instance) =>
    <String, dynamic>{
      'weight': instance.weight,
      'timestamp': instance.timestamp.toIso8601String(),
      'isStable': instance.isStable,
      'unit': instance.unit,
    };

TransactionDto _$TransactionDtoFromJson(Map<String, dynamic> json) =>
    TransactionDto(
      id: json['id'] as String,
      driverId: json['driverId'] as String,
      vehicleId: json['vehicleId'] as String,
      customerId: json['customerId'] as String?,
      productId: json['productId'] as String?,
      tareWeight: (json['tareWeight'] as num?)?.toDouble(),
      grossWeight: (json['grossWeight'] as num?)?.toDouble(),
      netWeight: (json['netWeight'] as num?)?.toDouble(),
      timestamp: DateTime.parse(json['timestamp'] as String),
      status: json['status'] as String,
      notes: json['notes'] as String?,
    );

Map<String, dynamic> _$TransactionDtoToJson(TransactionDto instance) =>
    <String, dynamic>{
      'id': instance.id,
      'driverId': instance.driverId,
      'vehicleId': instance.vehicleId,
      'customerId': instance.customerId,
      'productId': instance.productId,
      'tareWeight': instance.tareWeight,
      'grossWeight': instance.grossWeight,
      'netWeight': instance.netWeight,
      'timestamp': instance.timestamp.toIso8601String(),
      'status': instance.status,
      'notes': instance.notes,
    };

CreateTransactionRequest _$CreateTransactionRequestFromJson(
        Map<String, dynamic> json) =>
    CreateTransactionRequest(
      driverId: json['driverId'] as String,
      vehicleId: json['vehicleId'] as String,
      customerId: json['customerId'] as String?,
      productId: json['productId'] as String?,
      transactionType: json['transactionType'] as String? ?? 'weighing',
    );

Map<String, dynamic> _$CreateTransactionRequestToJson(
        CreateTransactionRequest instance) =>
    <String, dynamic>{
      'driverId': instance.driverId,
      'vehicleId': instance.vehicleId,
      'customerId': instance.customerId,
      'productId': instance.productId,
      'transactionType': instance.transactionType,
    };

UpdateWeightRequest _$UpdateWeightRequestFromJson(Map<String, dynamic> json) =>
    UpdateWeightRequest(
      transactionId: json['transactionId'] as String,
      weight: (json['weight'] as num).toDouble(),
      weightType: json['weightType'] as String,
      timestamp: DateTime.parse(json['timestamp'] as String),
    );

Map<String, dynamic> _$UpdateWeightRequestToJson(
        UpdateWeightRequest instance) =>
    <String, dynamic>{
      'transactionId': instance.transactionId,
      'weight': instance.weight,
      'weightType': instance.weightType,
      'timestamp': instance.timestamp.toIso8601String(),
    };
