import 'package:json_annotation/json_annotation.dart';

part 'transaction_models.g.dart';

@JsonSerializable()
class WeightReadingDto {
  final double weight;
  final DateTime timestamp;
  final bool isStable;
  final String unit;

  WeightReadingDto({
    required this.weight,
    required this.timestamp,
    required this.isStable,
    this.unit = 'kg',
  });

  factory WeightReadingDto.fromJson(Map<String, dynamic> json) =>
      _$WeightReadingDtoFromJson(json);

  Map<String, dynamic> toJson() => _$WeightReadingDtoToJson(this);
}

@JsonSerializable()
class TransactionDto {
  final String id;
  final String driverId;
  final String vehicleId;
  final String? customerId;
  final String? productId;
  final double? tareWeight;
  final double? grossWeight;
  final double? netWeight;
  final DateTime timestamp;
  final String status;
  final String? notes;

  TransactionDto({
    required this.id,
    required this.driverId,
    required this.vehicleId,
    this.customerId,
    this.productId,
    this.tareWeight,
    this.grossWeight,
    this.netWeight,
    required this.timestamp,
    required this.status,
    this.notes,
  });

  factory TransactionDto.fromJson(Map<String, dynamic> json) =>
      _$TransactionDtoFromJson(json);

  Map<String, dynamic> toJson() => _$TransactionDtoToJson(this);
}

@JsonSerializable()
class CreateTransactionRequest {
  final String driverId;
  final String vehicleId;
  final String? customerId;
  final String? productId;
  final String transactionType;

  CreateTransactionRequest({
    required this.driverId,
    required this.vehicleId,
    this.customerId,
    this.productId,
    this.transactionType = 'weighing',
  });

  factory CreateTransactionRequest.fromJson(Map<String, dynamic> json) =>
      _$CreateTransactionRequestFromJson(json);

  Map<String, dynamic> toJson() => _$CreateTransactionRequestToJson(this);
}

@JsonSerializable()
class UpdateWeightRequest {
  final String transactionId;
  final double weight;
  final String weightType;
  final DateTime timestamp;

  UpdateWeightRequest({
    required this.transactionId,
    required this.weight,
    required this.weightType,
    required this.timestamp,
  });

  factory UpdateWeightRequest.fromJson(Map<String, dynamic> json) =>
      _$UpdateWeightRequestFromJson(json);

  Map<String, dynamic> toJson() => _$UpdateWeightRequestToJson(this);
}

enum TransactionStatus {
  @JsonValue('pending')
  pending,
  @JsonValue('in_progress')
  inProgress,
  @JsonValue('completed')
  completed,
  @JsonValue('cancelled')
  cancelled,
  @JsonValue('failed')
  failed,
}

enum WeightType {
  @JsonValue('tare')
  tare,
  @JsonValue('gross')
  gross,
}