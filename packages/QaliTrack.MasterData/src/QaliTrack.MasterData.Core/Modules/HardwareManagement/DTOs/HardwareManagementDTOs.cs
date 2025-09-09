namespace QaliTrack.MasterData.Core.Modules.HardwareManagement.DTOs;

// Weighbridge DTOs
public class WeighbridgeDto
{
    public Guid Id { get; set; }
    public Guid SiteId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public decimal MaxCapacity { get; set; }
    public string DirectionCapability { get; set; } = string.Empty;
    public string WeighbridgeRole { get; set; } = string.Empty;
    public string Manufacturer { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string? SerialNumber { get; set; }
    public DateTime? InstallationDate { get; set; }
    public bool IsActive { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Related data
    public ICollection<PlcConfigurationDto> PlcConfigurations { get; set; } = new List<PlcConfigurationDto>();
    public ICollection<AnprCameraDto> AnprCameras { get; set; } = new List<AnprCameraDto>();
}

public class CreateWeighbridgeDto
{
    public Guid SiteId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public decimal MaxCapacity { get; set; }
    public string DirectionCapability { get; set; } = string.Empty;
    public string WeighbridgeRole { get; set; } = string.Empty;
    public string Manufacturer { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string? SerialNumber { get; set; }
    public DateTime? InstallationDate { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }
}

public class UpdateWeighbridgeDto
{
    public Guid SiteId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public decimal MaxCapacity { get; set; }
    public string DirectionCapability { get; set; } = string.Empty;
    public string WeighbridgeRole { get; set; } = string.Empty;
    public string Manufacturer { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string? SerialNumber { get; set; }
    public DateTime? InstallationDate { get; set; }
    public bool IsActive { get; set; }
    public string? Notes { get; set; }
}

// PLC Configuration DTOs
public class PlcConfigurationDto
{
    public Guid Id { get; set; }
    public Guid WeighbridgeId { get; set; }
    public string PlcType { get; set; } = string.Empty;
    public string PlcAddress { get; set; } = string.Empty;
    public int InputCount { get; set; }
    public int OutputCount { get; set; }
    public string ConfigurationData { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime? LastConnectionTest { get; set; }
    public string? ConnectionStatus { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreatePlcConfigurationDto
{
    public Guid WeighbridgeId { get; set; }
    public string PlcType { get; set; } = string.Empty;
    public string PlcAddress { get; set; } = string.Empty;
    public int InputCount { get; set; } = 0;
    public int OutputCount { get; set; } = 0;
    public string ConfigurationData { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

public class UpdatePlcConfigurationDto
{
    public Guid WeighbridgeId { get; set; }
    public string PlcType { get; set; } = string.Empty;
    public string PlcAddress { get; set; } = string.Empty;
    public int InputCount { get; set; }
    public int OutputCount { get; set; }
    public string ConfigurationData { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime? LastConnectionTest { get; set; }
    public string? ConnectionStatus { get; set; }
}

// ANPR Camera DTOs
public class AnprCameraDto
{
    public Guid Id { get; set; }
    public Guid WeighbridgeId { get; set; }
    public string CameraName { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? LastStatusCheck { get; set; }
    public string? ConfigurationSettings { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateAnprCameraDto
{
    public Guid WeighbridgeId { get; set; }
    public string CameraName { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public string Status { get; set; } = "Offline";
    public string? ConfigurationSettings { get; set; }
    public string? Notes { get; set; }
}

public class UpdateAnprCameraDto
{
    public Guid WeighbridgeId { get; set; }
    public string CameraName { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? LastStatusCheck { get; set; }
    public string? ConfigurationSettings { get; set; }
    public string? Notes { get; set; }
}