# Weighbridge Service Visual Guide

## 🎯 **Service Overview**

### **Business Purpose**
The Weighbridge Service manages QaliTrack's weighbridge equipment, including equipment registration, calibration management, operator assignments, maintenance tracking, and compliance monitoring. It provides centralized weighbridge information for weight data collection and regulatory compliance.

### **Key Business Problems Solved**
- **Equipment Management**: Centralized weighbridge equipment registration and tracking
- **Calibration Management**: Regular calibration scheduling and compliance tracking
- **Operator Management**: Operator certification and assignment management
- **Maintenance Tracking**: Preventive and corrective maintenance scheduling
- **Compliance Monitoring**: Regulatory compliance and certification tracking
- **Accuracy Assurance**: Weight measurement accuracy and quality control

### **Integration Role**
Central weighbridge registry that provides equipment information for weight data collection, validates weighbridge operations, and ensures measurement accuracy across the logistics network.

---

## 🏗️ **Architecture Diagram**

```
┌─────────────────────────────────────────────────────────────────┐
│                    Weighbridge Service                          │
│                         Port: 7007                              │
└─────────────────────────────────────────────────────────────────┘
┌─────────────────────────────────────────────────────────────────┐
│                          API Layer                              │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐   │
│  │WeighbridgesController  │  │OperatorsController   │  │MaintenanceController │  │
│  │  - CRUD operations     │  │  - Operator mgmt     │  │  - Maintenance mgmt  │  │
│  │  - Equipment mgmt      │  │  - Certification mgmt│  │  - Schedule mgmt     │  │
│  │  - Calibration mgmt    │  │  - Assignment mgmt   │  │  - Service records   │  │
│  │  - Status monitoring   │  │  - Performance tracking│  │  - Cost tracking     │  │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘   │
└─────────────────────────────────────────────────────────────────┘
┌─────────────────────────────────────────────────────────────────┐
│                         Core Layer                              │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐   │
│  │WeighbridgeService  │  │ OperatorService     │  │MaintenanceService   │   │
│  │  - Equipment logic│  │  - Operator logic   │  │  - Maintenance logic│   │
│  │  - Calibration    │  │  - Certification    │  │  - Schedule logic   │   │
│  │  - Status mgmt    │  │  - Assignment logic │  │  - Cost calculation │   │
│  │  - Validation     │  │  - Performance eval │  │  - Preventive care  │   │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘   │
│                                                                 │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐   │
│  │  Weighbridge       │  │WeighbridgeOperator  │  │WeighbridgeMaintenance│   │
│  │  - Id, Name        │  │  - Id, OperatorId   │  │  - Id, WeighbridgeId │   │
│  │  - Type, Capacity  │  │  - WeighbridgeId    │  │  - Type, Date        │   │
│  │  - Location        │  │  - Certification    │  │  - Cost, Status      │   │
│  │  - Calibration     │  │  - Assignment       │  │  - NextService       │   │
│  │  - Status          │  │  - Performance      │  │  - Description       │   │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘   │
└─────────────────────────────────────────────────────────────────┘
┌─────────────────────────────────────────────────────────────────┐
│                    Infrastructure Layer                         │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐   │
│  │WeighbridgeRepository│  │ OperatorRepository  │  │MaintenanceRepository │   │
│  │  - CRUD operations │  │  - CRUD operations  │  │  - CRUD operations   │   │
│  │  - Equipment queries│  │  - Assignment queries│  │  - Schedule queries  │   │
│  │  - Calibration qry │  │  - Performance queries│  │  - Cost queries      │   │
│  │  - Status queries  │  │  - Certification qry │  │  - History queries   │   │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘   │
│                                                                 │
│  ┌─────────────────────────────────────────────────────────────┐  │
│  │                WeighbridgeDbContext                          │  │
│  │  - Weighbridges, Operators, Maintenance, Calibration Tables │  │
│  │  - Entity configurations and relationships                  │  │
│  │  - Database migrations and seeding                          │  │
│  └─────────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
```

---

## 📊 **Entity Model**

### **Core Entities**

#### **Weighbridge Entity**
```csharp
public class Weighbridge : BaseEntity
{
    public string Name { get; set; }                    // "Mombasa Plant Weighbridge 1"
    public string Code { get; set; }                    // "WB-MOB-001"
    public string SerialNumber { get; set; }            // "WB123456789"
    public WeighbridgeDetails Details { get; set; }
    public WeighbridgeSpecifications Specifications { get; set; }
    public LocationInformation Location { get; set; }
    public CalibrationInformation Calibration { get; set; }
    public WeighbridgeStatus Status { get; set; }       // Active, Inactive, Maintenance, Calibration
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedBy { get; set; }
    public string UpdatedBy { get; set; }
}

public class WeighbridgeDetails
{
    public string Manufacturer { get; set; }            // "Avery Weigh-Tronix"
    public string Model { get; set; }                   // "BWB Series"
    public int Year { get; set; }                       // 2020
    public WeighbridgeType Type { get; set; }           // Truck, Rail, Portable
    public string Technology { get; set; }              // "Load Cell"
    public DateTime InstallationDate { get; set; }      // 2020-03-15
    public DateTime CommissioningDate { get; set; }     // 2020-03-20
    public string WarrantyPeriod { get; set; }          // "3 years"
    public DateTime WarrantyExpiry { get; set; }        // 2023-03-20
    public string SupportContact { get; set; }          // "+254700123456"
}

public enum WeighbridgeType
{
    Truck,              // Truck weighbridge
    Rail,               // Railway weighbridge
    Portable,           // Portable weighbridge
    Axle,               // Axle weighbridge
    Livestock,          // Livestock weighbridge
    Industrial         // Industrial weighbridge
}

public class WeighbridgeSpecifications
{
    public decimal MaxCapacity { get; set; }            // 80.0 (tons)
    public decimal MinCapacity { get; set; }            // 0.5 (tons)
    public decimal Accuracy { get; set; }               // 0.02 (% of capacity)
    public decimal Resolution { get; set; }             // 20.0 (kg)
    public PlatformDimensions Platform { get; set; }
    public string LoadCellCount { get; set; }           // "8 load cells"
    public string LoadCellType { get; set; }            // "Shear beam"
    public string DisplayType { get; set; }             // "Digital LED"
    public List<string> Features { get; set; }          // ["Automatic zero", "Tare function", "Data logging"]
    public EnvironmentalSpecs Environmental { get; set; }
}

public class PlatformDimensions
{
    public decimal Length { get; set; }                 // 18.0 (meters)
    public decimal Width { get; set; }                  // 3.2 (meters)
    public decimal Height { get; set; }                 // 0.15 (meters)
    public decimal Area { get; set; }                   // 57.6 (square meters)
    public string Material { get; set; }                // "Steel plate"
    public decimal Thickness { get; set; }              // 12.0 (mm)
}

public class EnvironmentalSpecs
{
    public string OperatingTemperature { get; set; }    // "-10°C to +40°C"
    public string Humidity { get; set; }                // "≤95% RH"
    public string IPRating { get; set; }                // "IP67"
    public string PowerRequirement { get; set; }        // "220V AC, 50Hz"
    public string PowerConsumption { get; set; }        // "50W"
    public bool HasLightning { get; set; }              // true
    public bool HasSurgeProtection { get; set; }        // true
}

public class LocationInformation
{
    public string SiteName { get; set; }                // "Mombasa Plant"
    public string SiteCode { get; set; }                // "MOB-PLT-001"
    public Address Address { get; set; }
    public GeoCoordinate Coordinates { get; set; }
    public string AccessInstructions { get; set; }      // "Use main gate, proceed to weighbridge area"
    public string Landmarks { get; set; }               // "Next to main warehouse"
    public OperatingHours Hours { get; set; }
    public SecurityInformation Security { get; set; }
}

public class SecurityInformation
{
    public bool HasCCTV { get; set; }                   // true
    public bool HasSecurityGuard { get; set; }          // true
    public bool HasAccessControl { get; set; }          // true
    public bool HasLighting { get; set; }               // true
    public string SecurityContact { get; set; }         // "+254712345678"
    public List<string> SecurityMeasures { get; set; }  // ["CCTV", "Guard post", "Access cards"]
}

public class CalibrationInformation
{
    public DateTime LastCalibration { get; set; }       // 2024-01-15
    public DateTime NextCalibration { get; set; }       // 2024-07-15
    public CalibrationFrequency Frequency { get; set; } // SixMonthly, Yearly
    public string CalibrationAuthority { get; set; }    // "KEBS"
    public string CertificateNumber { get; set; }       // "CAL-2024-001"
    public CalibrationStatus Status { get; set; }       // Valid, Expired, Due, Overdue
    public List<string> TestWeights { get; set; }       // ["5T", "10T", "20T", "50T"]
    public decimal AccuracyTolerance { get; set; }      // 0.1 (% deviation allowed)
}

public enum CalibrationFrequency
{
    Monthly,            // Monthly calibration
    Quarterly,          // Quarterly calibration
    SixMonthly,         // Six-monthly calibration
    Yearly,             // Yearly calibration
    BiYearly,           // Two-yearly calibration
    AsRequired         // As required basis
}

public enum CalibrationStatus
{
    Valid,              // Calibration valid
    Expired,            // Calibration expired
    Due,                // Calibration due soon
    Overdue,            // Calibration overdue
    InProgress,         // Calibration in progress
    Failed             // Calibration failed
}
```

#### **WeighbridgeOperator Entity**
```csharp
public class WeighbridgeOperator : BaseEntity
{
    public string OperatorId { get; set; }              // References employee/person
    public string WeighbridgeId { get; set; }           // References Weighbridge
    public OperatorDetails Details { get; set; }
    public OperatorCertification Certification { get; set; }
    public OperatorAssignment Assignment { get; set; }
    public OperatorPerformance Performance { get; set; }
    public OperatorStatus Status { get; set; }          // Active, Inactive, Training, Suspended
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedBy { get; set; }
    public string UpdatedBy { get; set; }
    
    // Navigation properties
    public Weighbridge Weighbridge { get; set; }
}

public class OperatorDetails
{
    public string Name { get; set; }                    // "John Kamau"
    public string EmployeeId { get; set; }              // "WB-OP-001"
    public string ContactPhone { get; set; }            // "+254712345678"
    public string Email { get; set; }                   // "john.kamau@qalitrack.com"
    public DateTime HireDate { get; set; }              // 2024-01-15
    public string Department { get; set; }              // "Operations"
    public string Supervisor { get; set; }              // "Mary Wanjiku"
    public List<string> Languages { get; set; }         // ["English", "Swahili"]
    public string ShiftPattern { get; set; }            // "Day shift", "Night shift", "Rotating"
}

public class OperatorCertification
{
    public string CertificateNumber { get; set; }       // "WB-CERT-001"
    public string CertificateType { get; set; }         // "Weighbridge Operator Certificate"
    public DateTime IssueDate { get; set; }             // 2024-01-01
    public DateTime ExpiryDate { get; set; }            // 2025-01-01
    public string IssuingAuthority { get; set; }        // "KEBS"
    public CertificationStatus Status { get; set; }     // Valid, Expired, Suspended, Revoked
    public List<string> Competencies { get; set; }      // ["Operation", "Basic maintenance", "Data entry"]
    public decimal TrainingHours { get; set; }          // 40.0
    public decimal ExamScore { get; set; }              // 85.5
    public string TrainingProvider { get; set; }        // "QaliTrack Training Center"
}

public enum CertificationStatus
{
    Valid,              // Certification valid
    Expired,            // Certification expired
    Suspended,          // Certification suspended
    Revoked,            // Certification revoked
    Pending,            // Certification pending
    Renewal            // Renewal in progress
}

public class OperatorAssignment
{
    public List<string> AssignedWeighbridges { get; set; } // ["WB-MOB-001", "WB-MOB-002"]
    public string PrimaryWeighbridge { get; set; }      // "WB-MOB-001"
    public WorkSchedule Schedule { get; set; }
    public DateTime AssignmentDate { get; set; }        // 2024-01-15
    public string AssignmentType { get; set; }          // "Primary", "Backup", "Training"
    public bool CanOperateUnsupervised { get; set; }    // true
    public List<string> Restrictions { get; set; }      // ["No night shifts", "Supervision required"]
}

public class OperatorPerformance
{
    public int WeighingsPerformed { get; set; }         // 1250
    public decimal AccuracyScore { get; set; }          // 99.2 (%)
    public decimal SpeedScore { get; set; }             // 8.5 (weighings per hour)
    public int ErrorCount { get; set; }                 // 3
    public decimal ErrorRate { get; set; }              // 0.24 (%)
    public int CustomerComplaints { get; set; }         // 1
    public decimal CustomerRating { get; set; }         // 4.3 (out of 5)
    public DateTime LastPerformanceReview { get; set; } // 2024-01-31
    public string PerformanceNotes { get; set; }        // "Excellent accuracy, needs speed improvement"
}

public enum OperatorStatus
{
    Active,             // Operator is active
    Inactive,           // Operator is inactive
    Training,           // Operator in training
    Suspended,          // Operator suspended
    OnLeave,            // Operator on leave
    Transferred        // Operator transferred
}
```

#### **WeighbridgeMaintenance Entity**
```csharp
public class WeighbridgeMaintenance : BaseEntity
{
    public string WeighbridgeId { get; set; }           // References Weighbridge
    public MaintenanceType Type { get; set; }           // Preventive, Corrective, Calibration, Emergency
    public string Description { get; set; }             // "Load cell calibration and platform cleaning"
    public DateTime ScheduledDate { get; set; }         // 2024-01-25
    public DateTime? ActualDate { get; set; }           // Actual service date
    public MaintenanceDetails Details { get; set; }
    public MaintenanceStatus Status { get; set; }       // Scheduled, InProgress, Completed, Cancelled
    public string ServiceProviderId { get; set; }       // References service provider
    public MaintenanceCost Cost { get; set; }
    public DateTime? NextServiceDate { get; set; }      // Next scheduled service
    public MaintenanceImpact Impact { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedBy { get; set; }
    public string UpdatedBy { get; set; }
    
    // Navigation properties
    public Weighbridge Weighbridge { get; set; }
}

public enum MaintenanceType
{
    Preventive,         // Scheduled maintenance
    Corrective,         // Repair maintenance
    Calibration,        // Calibration service
    Emergency,          // Emergency repair
    Upgrade,            // System upgrade
    Inspection         // Inspection service
}

public class MaintenanceDetails
{
    public List<string> ServicesPerformed { get; set; } // ["Load cell check", "Platform cleaning"]
    public List<string> PartsReplaced { get; set; }     // ["Load cell cable", "Display unit"]
    public List<string> IssuesFound { get; set; }       // ["Loose connection", "Dirt accumulation"]
    public List<string> Recommendations { get; set; }   // ["Monthly cleaning", "Cable inspection"]
    public string TechnicianName { get; set; }          // "Peter Mwangi"
    public string TechnicianCertification { get; set; } // "CERT-WB-001"
    public string ServiceCompany { get; set; }          // "Weighbridge Services Ltd"
    public TimeSpan ServiceDuration { get; set; }       // 4 hours
    public string WarrantyPeriod { get; set; }          // "6 months"
}

public class MaintenanceCost
{
    public decimal LaborCost { get; set; }              // 15000.0 (KES)
    public decimal PartsCost { get; set; }              // 25000.0 (KES)
    public decimal TravelCost { get; set; }             // 5000.0 (KES)
    public decimal TotalCost { get; set; }              // 45000.0 (KES)
    public string Currency { get; set; }                // "KES"
    public string InvoiceNumber { get; set; }           // "INV-WB-2024-001"
    public DateTime InvoiceDate { get; set; }           // 2024-01-25
    public PaymentStatus PaymentStatus { get; set; }    // Paid, Pending, Overdue
}

public class MaintenanceImpact
{
    public TimeSpan DowntimeRequired { get; set; }      // 4 hours
    public DateTime PlannedStartTime { get; set; }      // 2024-01-25T06:00:00Z
    public DateTime PlannedEndTime { get; set; }        // 2024-01-25T10:00:00Z
    public DateTime? ActualStartTime { get; set; }      // Actual start time
    public DateTime? ActualEndTime { get; set; }        // Actual end time
    public bool RequiresShutdown { get; set; }          // true
    public bool AffectsOperations { get; set; }         // true
    public string ImpactDescription { get; set; }       // "Weighbridge unavailable for 4 hours"
    public List<string> AlternativeArrangements { get; set; } // ["Use backup weighbridge"]
}
```

#### **WeighbridgeCalibration Entity**
```csharp
public class WeighbridgeCalibration : BaseEntity
{
    public string WeighbridgeId { get; set; }           // References Weighbridge
    public string CalibrationNumber { get; set; }       // "CAL-WB-2024-001"
    public CalibrationType Type { get; set; }           // Initial, Periodic, Verification, Repair
    public DateTime CalibrationDate { get; set; }       // 2024-01-15
    public CalibrationDetails Details { get; set; }
    public CalibrationResults Results { get; set; }
    public CalibrationStatus Status { get; set; }       // Passed, Failed, Conditional, InProgress
    public string CalibrationAuthority { get; set; }    // "KEBS"
    public string TechnicianName { get; set; }          // "Engineer John Mwangi"
    public string CertificateNumber { get; set; }       // "KEBS-CAL-2024-001"
    public DateTime ValidFrom { get; set; }             // 2024-01-15
    public DateTime ValidUntil { get; set; }            // 2024-07-15
    public CalibrationCost Cost { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedBy { get; set; }
    public string UpdatedBy { get; set; }
    
    // Navigation properties
    public Weighbridge Weighbridge { get; set; }
}

public enum CalibrationType
{
    Initial,            // Initial calibration
    Periodic,           // Periodic calibration
    Verification,       // Verification calibration
    Repair,             // Post-repair calibration
    Complaint,          // Due to complaint
    Regulatory         // Regulatory requirement
}

public class CalibrationDetails
{
    public List<string> StandardWeights { get; set; }   // ["5000kg", "10000kg", "20000kg"]
    public string CalibrationMethod { get; set; }       // "Direct comparison"
    public string ReferenceStandard { get; set; }       // "KEBS certified weights"
    public EnvironmentalConditions Environment { get; set; }
    public List<string> TestPoints { get; set; }        // ["0kg", "5000kg", "10000kg", "20000kg"]
    public int NumberOfReadings { get; set; }           // 3
    public string CalibrationProcedure { get; set; }    // "KEBS-WB-001"
    public List<string> EquipmentUsed { get; set; }     // ["Certified weights", "Digital multimeter"]
}

public class EnvironmentalConditions
{
    public decimal Temperature { get; set; }            // 25.5 (°C)
    public decimal Humidity { get; set; }               // 65.0 (%)
    public decimal AtmosphericPressure { get; set; }    // 1013.25 (hPa)
    public string WindConditions { get; set; }          // "Calm"
    public string Notes { get; set; }                   // "Stable conditions throughout test"
}

public class CalibrationResults
{
    public decimal MaxPermissibleError { get; set; }    // 20.0 (kg)
    public decimal MaxObservedError { get; set; }       // 15.0 (kg)
    public bool WithinTolerance { get; set; }           // true
    public List<TestResult> TestResults { get; set; }
    public decimal UncertaintyOfMeasurement { get; set; } // 5.0 (kg)
    public string OverallResult { get; set; }           // "PASS"
    public List<string> Observations { get; set; }      // ["All readings within tolerance"]
    public List<string> Recommendations { get; set; }   // ["Continue regular maintenance"]
}

public class TestResult
{
    public decimal AppliedLoad { get; set; }            // 10000.0 (kg)
    public decimal Indication { get; set; }             // 10015.0 (kg)
    public decimal Error { get; set; }                  // 15.0 (kg)
    public decimal ErrorPercentage { get; set; }        // 0.15 (%)
    public bool WithinTolerance { get; set; }           // true
    public string Notes { get; set; }                   // "Reading stable"
}

public class CalibrationCost
{
    public decimal ServiceFee { get; set; }             // 25000.0 (KES)
    public decimal TravelCost { get; set; }             // 5000.0 (KES)
    public decimal CertificateFee { get; set; }         // 3000.0 (KES)
    public decimal TotalCost { get; set; }              // 33000.0 (KES)
    public string Currency { get; set; }                // "KES"
    public string InvoiceNumber { get; set; }           // "KEBS-INV-2024-001"
    public DateTime InvoiceDate { get; set; }           // 2024-01-15
    public PaymentStatus PaymentStatus { get; set; }    // Paid, Pending, Overdue
}
```

### **Entity Relationships**

```
┌─────────────────┐         ┌─────────────────┐         ┌─────────────────┐
│  Weighbridge    │◄────────┤WeighbridgeOperator│         │WeighbridgeMaintenance│
│                 │         │                 │         │                 │
│ • Id            │         │ • WeighbridgeId │         │ • WeighbridgeId │
│ • Name          │         │ • OperatorId    │         │ • Type          │
│ • Type          │         │ • Certification │         │ • Description   │
│ • Capacity      │         │ • Assignment    │         │ • ScheduledDate │
│ • Location      │         │ • Performance   │         │ • Status        │
│ • Status        │         │ • Status        │         │ • Cost          │
└─────────────────┘         └─────────────────┘         └─────────────────┘
         │                                                        │
         │                                                        │
         ▼                                                        ▼
┌─────────────────┐                                    ┌─────────────────┐
│WeighbridgeCalibration│                                 │  Future Weight  │
│                 │                                    │  Data Service   │
│ • WeighbridgeId │                                    │ (Integration)   │
│ • CalibrationNumber│                                  │                 │
│ • Type          │                                    │ • Weight Records│
│ • Date          │                                    │ • Transactions  │
│ • Results       │                                    │ • Reports       │
│ • Status        │                                    │ • Analytics     │
│ • Certificate   │                                    └─────────────────┘
└─────────────────┘
```

---

## 🔌 **API Endpoints**

### **Weighbridge Management**

#### **GET /api/weighbridges**
**Purpose**: Retrieve paginated weighbridge list with filtering
```json
// Request
GET /api/weighbridges?page=1&size=10&location=Mombasa&status=Active&type=Truck

// Response
{
  "data": [
    {
      "id": "wb-001",
      "name": "Mombasa Plant Weighbridge 1",
      "code": "WB-MOB-001",
      "serialNumber": "WB123456789",
      "details": {
        "manufacturer": "Avery Weigh-Tronix",
        "model": "BWB Series",
        "year": 2020,
        "type": "Truck",
        "technology": "Load Cell",
        "installationDate": "2020-03-15T00:00:00Z",
        "commissioningDate": "2020-03-20T00:00:00Z",
        "warrantyPeriod": "3 years",
        "warrantyExpiry": "2023-03-20T00:00:00Z",
        "supportContact": "+254700123456"
      },
      "specifications": {
        "maxCapacity": 80.0,
        "minCapacity": 0.5,
        "accuracy": 0.02,
        "resolution": 20.0,
        "platform": {
          "length": 18.0,
          "width": 3.2,
          "height": 0.15,
          "area": 57.6,
          "material": "Steel plate",
          "thickness": 12.0
        },
        "loadCellCount": "8 load cells",
        "loadCellType": "Shear beam",
        "displayType": "Digital LED",
        "features": ["Automatic zero", "Tare function", "Data logging"],
        "environmental": {
          "operatingTemperature": "-10°C to +40°C",
          "humidity": "≤95% RH",
          "ipRating": "IP67",
          "powerRequirement": "220V AC, 50Hz",
          "powerConsumption": "50W",
          "hasLightning": true,
          "hasSurgeProtection": true
        }
      },
      "location": {
        "siteName": "Mombasa Plant",
        "siteCode": "MOB-PLT-001",
        "address": {
          "street": "Industrial Area",
          "city": "Mombasa",
          "postalCode": "80100",
          "country": "Kenya"
        },
        "coordinates": {
          "latitude": -4.0435,
          "longitude": 39.6682
        },
        "accessInstructions": "Use main gate, proceed to weighbridge area",
        "landmarks": "Next to main warehouse",
        "hours": {
          "openTime": "06:00:00",
          "closeTime": "18:00:00",
          "operatingDays": ["Monday", "Tuesday", "Wednesday", "Thursday", "Friday"],
          "closedDays": ["Saturday", "Sunday"]
        },
        "security": {
          "hasCCTV": true,
          "hasSecurityGuard": true,
          "hasAccessControl": true,
          "hasLighting": true,
          "securityContact": "+254712345678",
          "securityMeasures": ["CCTV", "Guard post", "Access cards"]
        }
      },
      "calibration": {
        "lastCalibration": "2024-01-15T00:00:00Z",
        "nextCalibration": "2024-07-15T00:00:00Z",
        "frequency": "SixMonthly",
        "calibrationAuthority": "KEBS",
        "certificateNumber": "CAL-2024-001",
        "status": "Valid",
        "testWeights": ["5T", "10T", "20T", "50T"],
        "accuracyTolerance": 0.1
      },
      "status": "Active",
      "createdAt": "2020-03-15T10:30:00Z",
      "updatedAt": "2024-01-20T14:45:00Z"
    }
  ],
  "pagination": {
    "page": 1,
    "size": 10,
    "total": 8,
    "totalPages": 1
  }
}
```

#### **GET /api/weighbridges/{id}**
**Purpose**: Retrieve specific weighbridge details
```json
// Request
GET /api/weighbridges/wb-001

// Response: Complete weighbridge object with all specifications
```

#### **POST /api/weighbridges**
**Purpose**: Create new weighbridge
```json
// Request
POST /api/weighbridges
{
  "name": "Nairobi Plant Weighbridge 1",
  "code": "WB-NAI-001",
  "serialNumber": "WB987654321",
  "details": {
    "manufacturer": "Mettler Toledo",
    "model": "PowerMount",
    "year": 2021,
    "type": "Truck",
    "technology": "Load Cell",
    "installationDate": "2021-05-10T00:00:00Z",
    "commissioningDate": "2021-05-15T00:00:00Z",
    "warrantyPeriod": "5 years",
    "warrantyExpiry": "2026-05-15T00:00:00Z",
    "supportContact": "+254711123456"
  },
  "specifications": {
    "maxCapacity": 100.0,
    "minCapacity": 1.0,
    "accuracy": 0.02,
    "resolution": 20.0,
    "platform": {
      "length": 20.0,
      "width": 3.5,
      "height": 0.2,
      "area": 70.0,
      "material": "Steel plate",
      "thickness": 15.0
    },
    "loadCellCount": "10 load cells",
    "loadCellType": "Compression",
    "displayType": "Touch screen",
    "features": ["Automatic zero", "Tare function", "Data logging", "Truck ID"],
    "environmental": {
      "operatingTemperature": "-10°C to +40°C",
      "humidity": "≤95% RH",
      "ipRating": "IP68",
      "powerRequirement": "220V AC, 50Hz",
      "powerConsumption": "75W",
      "hasLightning": true,
      "hasSurgeProtection": true
    }
  },
  "location": {
    "siteName": "Nairobi Plant",
    "siteCode": "NAI-PLT-001",
    "address": {
      "street": "Industrial Area",
      "city": "Nairobi",
      "postalCode": "00100",
      "country": "Kenya"
    },
    "coordinates": {
      "latitude": -1.3194,
      "longitude": 36.8441
    },
    "accessInstructions": "Use gate 2, follow signs to weighbridge",
    "landmarks": "Behind the cement silos",
    "hours": {
      "openTime": "05:00:00",
      "closeTime": "20:00:00",
      "operatingDays": ["Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"],
      "closedDays": ["Sunday"]
    },
    "security": {
      "hasCCTV": true,
      "hasSecurityGuard": true,
      "hasAccessControl": true,
      "hasLighting": true,
      "securityContact": "+254722345678",
      "securityMeasures": ["CCTV", "Guard post", "Biometric access"]
    }
  },
  "calibration": {
    "frequency": "SixMonthly",
    "calibrationAuthority": "KEBS",
    "testWeights": ["10T", "20T", "50T"],
    "accuracyTolerance": 0.1
  },
  "status": "Active"
}

// Response: Created weighbridge object
```

### **Operator Management**

#### **GET /api/weighbridges/{id}/operators**
**Purpose**: Retrieve weighbridge operators
```json
// Request
GET /api/weighbridges/wb-001/operators?status=Active

// Response
{
  "data": [
    {
      "id": "op-001",
      "operatorId": "EMP-WB-001",
      "weighbridgeId": "wb-001",
      "details": {
        "name": "John Kamau",
        "employeeId": "WB-OP-001",
        "contactPhone": "+254712345678",
        "email": "john.kamau@qalitrack.com",
        "hireDate": "2024-01-15T00:00:00Z",
        "department": "Operations",
        "supervisor": "Mary Wanjiku",
        "languages": ["English", "Swahili"],
        "shiftPattern": "Day shift"
      },
      "certification": {
        "certificateNumber": "WB-CERT-001",
        "certificateType": "Weighbridge Operator Certificate",
        "issueDate": "2024-01-01T00:00:00Z",
        "expiryDate": "2025-01-01T00:00:00Z",
        "issuingAuthority": "KEBS",
        "status": "Valid",
        "competencies": ["Operation", "Basic maintenance", "Data entry"],
        "trainingHours": 40.0,
        "examScore": 85.5,
        "trainingProvider": "QaliTrack Training Center"
      },
      "assignment": {
        "assignedWeighbridges": ["WB-MOB-001"],
        "primaryWeighbridge": "WB-MOB-001",
        "schedule": {
          "pattern": "5 days a week",
          "startTime": "06:00:00",
          "endTime": "14:00:00",
          "workDays": ["Monday", "Tuesday", "Wednesday", "Thursday", "Friday"],
          "restDays": ["Saturday", "Sunday"],
          "weeklyHours": 40.0,
          "isFlexible": false
        },
        "assignmentDate": "2024-01-15T00:00:00Z",
        "assignmentType": "Primary",
        "canOperateUnsupervised": true,
        "restrictions": []
      },
      "performance": {
        "weighingsPerformed": 1250,
        "accuracyScore": 99.2,
        "speedScore": 8.5,
        "errorCount": 3,
        "errorRate": 0.24,
        "customerComplaints": 1,
        "customerRating": 4.3,
        "lastPerformanceReview": "2024-01-31T00:00:00Z",
        "performanceNotes": "Excellent accuracy, needs speed improvement"
      },
      "status": "Active",
      "createdAt": "2024-01-15T10:00:00Z",
      "updatedAt": "2024-01-31T15:00:00Z"
    }
  ]
}
```

### **Maintenance Management**

#### **GET /api/weighbridges/{id}/maintenance**
**Purpose**: Retrieve weighbridge maintenance history
```json
// Request
GET /api/weighbridges/wb-001/maintenance?type=Preventive&status=Completed

// Response
{
  "data": [
    {
      "id": "maint-001",
      "weighbridgeId": "wb-001",
      "type": "Preventive",
      "description": "Load cell calibration and platform cleaning",
      "scheduledDate": "2024-01-25T08:00:00Z",
      "actualDate": "2024-01-25T09:00:00Z",
      "details": {
        "servicesPerformed": ["Load cell check", "Platform cleaning"],
        "partsReplaced": ["Load cell cable"],
        "issuesFound": ["Loose connection", "Dirt accumulation"],
        "recommendations": ["Monthly cleaning", "Cable inspection"],
        "technicianName": "Peter Mwangi",
        "technicianCertification": "CERT-WB-001",
        "serviceCompany": "Weighbridge Services Ltd",
        "serviceDuration": "04:00:00",
        "warrantyPeriod": "6 months"
      },
      "status": "Completed",
      "serviceProviderId": "service-001",
      "cost": {
        "laborCost": 15000.0,
        "partsCost": 25000.0,
        "travelCost": 5000.0,
        "totalCost": 45000.0,
        "currency": "KES",
        "invoiceNumber": "INV-WB-2024-001",
        "invoiceDate": "2024-01-25T00:00:00Z",
        "paymentStatus": "Paid"
      },
      "nextServiceDate": "2024-04-25T08:00:00Z",
      "impact": {
        "downtimeRequired": "04:00:00",
        "plannedStartTime": "2024-01-25T06:00:00Z",
        "plannedEndTime": "2024-01-25T10:00:00Z",
        "actualStartTime": "2024-01-25T06:30:00Z",
        "actualEndTime": "2024-01-25T10:30:00Z",
        "requiresShutdown": true,
        "affectsOperations": true,
        "impactDescription": "Weighbridge unavailable for 4 hours",
        "alternativeArrangements": ["Use backup weighbridge"]
      },
      "createdAt": "2024-01-20T10:00:00Z",
      "updatedAt": "2024-01-25T15:00:00Z"
    }
  ]
}
```

### **Calibration Management**

#### **GET /api/weighbridges/{id}/calibrations**
**Purpose**: Retrieve weighbridge calibration records
```json
// Request
GET /api/weighbridges/wb-001/calibrations?status=Passed

// Response
{
  "data": [
    {
      "id": "cal-001",
      "weighbridgeId": "wb-001",
      "calibrationNumber": "CAL-WB-2024-001",
      "type": "Periodic",
      "calibrationDate": "2024-01-15T00:00:00Z",
      "details": {
        "standardWeights": ["5000kg", "10000kg", "20000kg"],
        "calibrationMethod": "Direct comparison",
        "referenceStandard": "KEBS certified weights",
        "environment": {
          "temperature": 25.5,
          "humidity": 65.0,
          "atmosphericPressure": 1013.25,
          "windConditions": "Calm",
          "notes": "Stable conditions throughout test"
        },
        "testPoints": ["0kg", "5000kg", "10000kg", "20000kg"],
        "numberOfReadings": 3,
        "calibrationProcedure": "KEBS-WB-001",
        "equipmentUsed": ["Certified weights", "Digital multimeter"]
      },
      "results": {
        "maxPermissibleError": 20.0,
        "maxObservedError": 15.0,
        "withinTolerance": true,
        "testResults": [
          {
            "appliedLoad": 10000.0,
            "indication": 10015.0,
            "error": 15.0,
            "errorPercentage": 0.15,
            "withinTolerance": true,
            "notes": "Reading stable"
          }
        ],
        "uncertaintyOfMeasurement": 5.0,
        "overallResult": "PASS",
        "observations": ["All readings within tolerance"],
        "recommendations": ["Continue regular maintenance"]
      },
      "status": "Passed",
      "calibrationAuthority": "KEBS",
      "technicianName": "Engineer John Mwangi",
      "certificateNumber": "KEBS-CAL-2024-001",
      "validFrom": "2024-01-15T00:00:00Z",
      "validUntil": "2024-07-15T00:00:00Z",
      "cost": {
        "serviceFee": 25000.0,
        "travelCost": 5000.0,
        "certificateFee": 3000.0,
        "totalCost": 33000.0,
        "currency": "KES",
        "invoiceNumber": "KEBS-INV-2024-001",
        "invoiceDate": "2024-01-15T00:00:00Z",
        "paymentStatus": "Paid"
      },
      "createdAt": "2024-01-15T10:00:00Z",
      "updatedAt": "2024-01-15T16:00:00Z"
    }
  ]
}
```

### **Business-Specific Endpoints**

#### **GET /api/weighbridges/availability**
**Purpose**: Check weighbridge availability
```json
// Request
GET /api/weighbridges/availability?location=Mombasa&date=2024-01-25&time=08:00

// Response
{
  "availableWeighbridges": [
    {
      "id": "wb-001",
      "name": "Mombasa Plant Weighbridge 1",
      "status": "Active",
      "calibrationStatus": "Valid",
      "operatorStatus": "Available",
      "maintenanceStatus": "Up to date",
      "currentQueue": 2,
      "estimatedWaitTime": "15 minutes",
      "availabilityScore": 95.0
    }
  ]
}
```

#### **GET /api/weighbridges/status-summary**
**Purpose**: System-wide status summary
```json
// Request
GET /api/weighbridges/status-summary

// Response
{
  "summary": {
    "totalWeighbridges": 8,
    "activeWeighbridges": 7,
    "inMaintenanceWeighbridges": 1,
    "calibrationCompliance": 87.5,
    "operatorCoverage": 95.0,
    "averageUtilization": 75.0,
    "totalOperators": 15,
    "certifiedOperators": 14,
    "maintenanceScheduled": 3,
    "calibrationsDue": 2
  }
}
```

#### **POST /api/weighbridges/bulk-status-update**
**Purpose**: Bulk update weighbridge status
```json
// Request
POST /api/weighbridges/bulk-status-update
{
  "weighbridges": [
    {
      "id": "wb-001",
      "status": "Maintenance",
      "reason": "Scheduled calibration"
    },
    {
      "id": "wb-002",
      "status": "Active",
      "reason": "Maintenance completed"
    }
  ]
}

// Response
{
  "updated": 2,
  "failed": 0,
  "results": [
    {
      "id": "wb-001",
      "status": "Success",
      "message": "Weighbridge status updated to Maintenance"
    },
    {
      "id": "wb-002",
      "status": "Success",
      "message": "Weighbridge status updated to Active"
    }
  ]
}
```

---

## 🔗 **Integration Points**

### **Service Dependencies**

#### **Future Weight Data Service Integration**
```
┌─────────────────┐         ┌─────────────────┐
│ Weight Data     │◄───────►│Weighbridge Service│
│ Service (Future)│         │                 │
│                 │         │ • Equipment Info│
│ • Weight Records│         │ • Calibration   │
│ • Transactions  │         │ • Operators     │
│ • Reports       │         │ • Maintenance   │
│ • Analytics     │         │ • Status        │
└─────────────────┘         └─────────────────┘
```

#### **Customer Service Integration** (Indirect)
```
┌─────────────────┐         ┌─────────────────┐
│ Customer Service│    ?    │Weighbridge Service│
│                 │  ──────►│                 │
│ • Orders        │         │ • Equipment Info│
│ • Deliveries    │         │ • Availability  │
│ • Weight Reqs   │         │ • Capacity      │
└─────────────────┘         └─────────────────┘
```

### **Integration Patterns**

#### **Equipment Validation Pattern**
```csharp
// Weight Data Service -> Weighbridge Service
public async Task<bool> ValidateWeighbridgeAsync(string weighbridgeId)
{
    var weighbridge = await _weighbridgeService.GetWeighbridgeAsync(weighbridgeId);
    return weighbridge.Status == WeighbridgeStatus.Active && 
           weighbridge.Calibration.Status == CalibrationStatus.Valid;
}
```

#### **Operator Verification Pattern**
```csharp
// Weight Data Service -> Weighbridge Service
public async Task<bool> VerifyOperatorAsync(string operatorId, string weighbridgeId)
{
    var assignment = await _weighbridgeService.GetOperatorAssignmentAsync(operatorId, weighbridgeId);
    return assignment.Status == OperatorStatus.Active && 
           assignment.Certification.Status == CertificationStatus.Valid;
}
```

---

## 💼 **Business Use Cases**

### **Use Case 1: Weighbridge Installation**

**Scenario**: New weighbridge is installed at plant location
**Actors**: Installation Team, Calibration Authority, System Administrator
**Flow**:
1. Installation team completes weighbridge installation
2. System administrator creates weighbridge record with specifications
3. Initial calibration is performed by certified authority
4. Calibration results are recorded and certificate uploaded
5. Operators are trained and certified for the equipment
6. Weighbridge is commissioned and becomes operational
7. Regular maintenance schedule is established

**Business Value**: Systematic equipment deployment with full traceability

### **Use Case 2: Regular Calibration**

**Scenario**: Weighbridge requires periodic calibration
**Actors**: Calibration Authority, Operator, System
**Flow**:
1. System sends automated reminder for upcoming calibration
2. Calibration appointment is scheduled with certified authority
3. Weighbridge is temporarily taken out of service
4. Calibration is performed using certified reference weights
5. Results are documented and certificate is issued
6. System updates calibration status and next due date
7. Weighbridge returns to operational status

**Business Value**: Ensures measurement accuracy and regulatory compliance

### **Use Case 3: Operator Certification**

**Scenario**: New operator requires weighbridge certification
**Actors**: Operator, Training Provider, Supervisor, System
**Flow**:
1. New operator enrolls in weighbridge operation training
2. Training includes theory, practical operation, and safety
3. Operator completes written and practical examinations
4. Certification is issued by authorized training provider
5. System records certification details and expiry date
6. Operator is assigned to specific weighbridges
7. Performance monitoring begins

**Business Value**: Ensures qualified operation and data quality

### **Use Case 4: Maintenance Scheduling**

**Scenario**: Weighbridge requires preventive maintenance
**Actors**: Maintenance Team, Equipment Manufacturer, Operations Manager
**Flow**:
1. System identifies maintenance due based on schedule or usage
2. Maintenance is scheduled during low-activity periods
3. Equipment manufacturer technician performs service
4. Maintenance activities are documented in detail
5. Any parts replaced are recorded with warranty information
6. System updates maintenance records and next service date
7. Weighbridge is returned to service after verification

**Business Value**: Minimizes downtime and extends equipment life

---

## 🛣️ **Implementation Roadmap**

### **Phase 1: Foundation** (Month 7, Week 1-2)
- 🔄 **Project Setup**: Create Clean Architecture structure
- 🔄 **Core Entities**: Implement Weighbridge, Operator, Maintenance entities
- 🔄 **Database Layer**: Set up Entity Framework with SQLite
- 🔄 **Basic Repository**: Implement repository pattern for data access

### **Phase 2: Core Features** (Month 7, Week 3-4)
- 🔄 **Weighbridge Management**: Complete CRUD operations for equipment
- 🔄 **Operator Management**: Operator certification and assignment
- 🔄 **Calibration Management**: Calibration scheduling and tracking
- 🔄 **API Controllers**: REST API endpoints for all operations

### **Phase 3: Integration** (Month 8, Week 1-2)
- 🔄 **Equipment Validation**: Equipment status and availability checks
- 🔄 **Maintenance System**: Maintenance scheduling and tracking
- 🔄 **Compliance Monitoring**: Automated compliance alerts
- 🔄 **API Documentation**: Comprehensive OpenAPI documentation

### **Phase 4: Advanced Features** (Month 8, Week 3-4)
- 🔄 **Performance Analytics**: Equipment performance metrics
- 🔄 **Predictive Maintenance**: Maintenance prediction algorithms
- 🔄 **Integration Readiness**: Prepare for Weight Data Service integration
- 🔄 **Monitoring**: Health checks and performance metrics

### **Current Status**: 🔄 **READY FOR IMPLEMENTATION**
- **Priority**: LOW (Sixth priority - equipment management focus)
- **Dependencies**: None (standalone equipment management)
- **Estimated Timeline**: 1 month for complete implementation
- **Resource Requirements**: 1 developer, part-time

---

## 📊 **Success Metrics**

### **Technical Metrics**
- **API Response Time**: < 150ms for equipment queries
- **Database Performance**: < 100ms for maintenance queries
- **Service Availability**: 99.9% uptime
- **Integration Success**: 100% equipment validation accuracy

### **Business Metrics**
- **Equipment Count**: 10+ weighbridges managed
- **Calibration Compliance**: 100% valid calibrations
- **Operator Coverage**: 95% certified operators
- **Maintenance Compliance**: 90% on-time maintenance

### **Quality Metrics**
- **Calibration Accuracy**: 100% within tolerance
- **Equipment Uptime**: 95% operational availability
- **Operator Competency**: 4.5+ average certification score
- **System Reliability**: 0 compliance violations due to system issues

---

*Weighbridge Service serves as the central equipment management hub, enabling comprehensive weighbridge tracking, calibration management, and operator certification across the QaliTrack logistics network, preparing for future integration with weight data collection systems.*