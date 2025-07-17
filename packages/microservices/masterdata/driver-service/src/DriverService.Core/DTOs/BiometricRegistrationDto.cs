using System.ComponentModel.DataAnnotations;

namespace DriverService.Core.DTOs;

public class BiometricRegistrationDto
{
    [Required]
    public string BiometricType { get; set; } = string.Empty; // "fingerprint" or "face"
    
    [Required]
    public string BiometricData { get; set; } = string.Empty; // Base64 encoded biometric data
    
    public string? Description { get; set; }
}

public class BiometricVerificationDto
{
    [Required]
    public string BiometricType { get; set; } = string.Empty; // "fingerprint" or "face"
    
    [Required]
    public string BiometricData { get; set; } = string.Empty; // Base64 encoded biometric data for verification
}

public class BiometricVerificationResultDto
{
    public bool IsMatch { get; set; }
    public double ConfidenceScore { get; set; }
    public string BiometricType { get; set; } = string.Empty;
    public DateTime VerificationTimestamp { get; set; }
    public string? Notes { get; set; }
}

public class BiometricSettingsDto
{
    public bool BiometricEnabled { get; set; }
    public bool FingerprintEnabled { get; set; }
    public bool FaceRecognitionEnabled { get; set; }
}