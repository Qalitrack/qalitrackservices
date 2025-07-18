using System.ComponentModel.DataAnnotations;

namespace WeighbridgeService.Core.DTOs;

public class WeighbridgeHardwareStatusDto
{
    public string WeighbridgeId { get; set; } = string.Empty;
    public bool IsOnline { get; set; }
    public bool IsCalibrated { get; set; }
    public string ConnectionStatus { get; set; } = string.Empty; // "connected", "disconnected", "error"
    public string OperationalStatus { get; set; } = string.Empty; // "operational", "maintenance", "error"
    public decimal? CurrentWeight { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime LastCommunication { get; set; }
    public string FirmwareVersion { get; set; } = string.Empty;
    public Dictionary<string, object> SystemParameters { get; set; } = new Dictionary<string, object>();
}

public class WeighbridgeControlCommandDto
{
    [Required]
    public string Command { get; set; } = string.Empty; // "zero", "calibrate", "reset", "start", "stop"
    
    public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();
    
    public string? Operator { get; set; }
    
    public string? Reason { get; set; }
}

public class WeighbridgeControlResultDto
{
    public string WeighbridgeId { get; set; } = string.Empty;
    public string Command { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public Dictionary<string, object> ResultData { get; set; } = new Dictionary<string, object>();
    public DateTime ExecutedAt { get; set; }
    public string? ExecutedBy { get; set; }
}

public class WeighbridgeTestResultDto
{
    public string WeighbridgeId { get; set; } = string.Empty;
    public bool ConnectionTest { get; set; }
    public bool CalibrationTest { get; set; }
    public bool LoadCellTest { get; set; }
    public bool DisplayTest { get; set; }
    public bool CommunicationTest { get; set; }
    public List<string> TestResults { get; set; } = new List<string>();
    public List<string> Warnings { get; set; } = new List<string>();
    public List<string> Errors { get; set; } = new List<string>();
    public DateTime TestedAt { get; set; }
    public bool OverallResult { get; set; }
}

public class WeighbridgeRemoteUpdateDto
{
    [Required]
    public string UpdateType { get; set; } = string.Empty; // "firmware", "configuration", "calibration"
    
    public string? FirmwareVersion { get; set; }
    
    public string? ConfigurationData { get; set; }
    
    public Dictionary<string, object> CalibrationParameters { get; set; } = new Dictionary<string, object>();
    
    public bool ForceUpdate { get; set; } = false;
    
    public string? ScheduledTime { get; set; }
}

public class WeighbridgeUpdateResultDto
{
    public string WeighbridgeId { get; set; } = string.Empty;
    public string UpdateType { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string? PreviousVersion { get; set; }
    public string? NewVersion { get; set; }
    public DateTime UpdateStarted { get; set; }
    public DateTime? UpdateCompleted { get; set; }
    public List<string> UpdateLog { get; set; } = new List<string>();
    public bool RequiresRestart { get; set; }
}