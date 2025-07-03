using WeightDataService.Core.DTOs;
using WeightDataService.Core.Entities;

namespace WeightDataService.Tests.Helpers;

public static class TestDataFactory
{
    public static WeightMeasurement CreateWeightMeasurement(
        string weighbridgeId = "WB001",
        string vehicleRegistration = "ABC123",
        decimal weight = 1000.00m,
        string organizationId = "org1")
    {
        return new WeightMeasurement
        {
            Id = Guid.NewGuid(),
            WeighbridgeId = weighbridgeId,
            VehicleRegistration = vehicleRegistration,
            Weight = weight,
            Type = MeasurementType.In,
            Status = MeasurementStatus.Pending,
            MeasurementDateTime = DateTime.UtcNow,
            OrganizationId = organizationId,
            CreatedBy = "test-user",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public static CreateWeightMeasurementDto CreateWeightMeasurementDto(
        string weighbridgeId = "WB001",
        string vehicleRegistration = "ABC123",
        decimal weight = 1000.00m)
    {
        return new CreateWeightMeasurementDto
        {
            WeighbridgeId = weighbridgeId,
            VehicleRegistration = vehicleRegistration,
            Weight = weight,
            Type = MeasurementType.In,
            TicketReference = "T001",
            Notes = "Test measurement"
        };
    }

    public static WeighbridgeStatus CreateWeighbridgeStatus(
        string weighbridgeId = "WB001",
        string organizationId = "org1")
    {
        return new WeighbridgeStatus
        {
            Id = Guid.NewGuid(),
            WeighbridgeId = weighbridgeId,
            Name = "Test Weighbridge",
            Location = "Test Location",
            Status = MaintenanceStatus.Active,
            MaxCapacity = 50000,
            MinCapacity = 0,
            CurrentWeight = 0,
            LastCalibrationDate = DateTime.UtcNow.AddDays(-30),
            NextCalibrationDate = DateTime.UtcNow.AddDays(335),
            IsOnline = true,
            OrganizationId = organizationId,
            AccuracyTolerance = 0.1m,
            CreatedBy = "test-user",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public static WeightCorrection CreateWeightCorrection(
        Guid measurementId,
        decimal originalWeight = 1000.00m,
        decimal correctedWeight = 1050.00m)
    {
        return new WeightCorrection
        {
            Id = Guid.NewGuid(),
            WeightMeasurementId = measurementId,
            OriginalWeight = originalWeight,
            CorrectedWeight = correctedWeight,
            Reason = "Test correction",
            AuthorizedBy = "test-user",
            CorrectionDateTime = DateTime.UtcNow,
            CreatedBy = "test-user",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public static List<WeightMeasurement> CreateMultipleWeightMeasurements(
        int count = 5,
        string organizationId = "org1")
    {
        var measurements = new List<WeightMeasurement>();
        for (int i = 0; i < count; i++)
        {
            measurements.Add(CreateWeightMeasurement(
                weighbridgeId: $"WB00{i + 1}",
                vehicleRegistration: $"ABC{i + 100}",
                weight: 1000 + (i * 100),
                organizationId: organizationId));
        }
        return measurements;
    }
}