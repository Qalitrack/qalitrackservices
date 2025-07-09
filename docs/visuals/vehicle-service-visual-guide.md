# Vehicle Service Visual Guide

## 🎯 **Service Overview**

### **Business Purpose**
The Vehicle Service manages QaliTrack's vehicle fleet registry, including vehicle registration, specifications, maintenance tracking, SACCO associations, and compliance management. It provides centralized vehicle information for transporters and fleet management operations.

### **Key Business Problems Solved**
- **Fleet Registration**: Centralized vehicle registration and documentation
- **Compliance Management**: Vehicle licensing, insurance, and regulatory compliance
- **Maintenance Tracking**: Preventive and corrective maintenance scheduling
- **SACCO Integration**: Vehicle-SACCO relationship management
- **Capacity Planning**: Fleet capacity analysis and optimization
- **Performance Monitoring**: Vehicle performance and utilization tracking

### **Integration Role**
Central vehicle registry that provides vehicle information to transporters, validates vehicle assignments, and tracks vehicle performance across the logistics network.

---

## 🏗️ **Architecture Diagram**

```
┌─────────────────────────────────────────────────────────────────┐
│                       Vehicle Service                           │
│                         Port: 7003                              │
└─────────────────────────────────────────────────────────────────┘
┌─────────────────────────────────────────────────────────────────┐
│                          API Layer                              │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐   │
│  │ VehiclesController     │  │MaintenanceController │  │ ComplianceController │  │
│  │  - CRUD operations     │  │  - Schedule management│  │  - License tracking  │  │
│  │  - Registration mgmt   │  │  - Work order system  │  │  - Insurance mgmt    │  │
│  │  - Fleet management    │  │  - Service records    │  │  - Inspection records│  │
│  │  - Assignment tracking │  │  - Cost tracking      │  │  - Violation tracking│  │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘   │
└─────────────────────────────────────────────────────────────────┘
┌─────────────────────────────────────────────────────────────────┐
│                         Core Layer                              │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐   │
│  │  VehicleService    │  │MaintenanceService   │  │ComplianceService    │   │
│  │  - Vehicle logic   │  │  - Maintenance logic│  │  - Compliance logic │   │
│  │  - Fleet mgmt     │  │  - Schedule mgmt    │  │  - License mgmt     │   │
│  │  - Assignment     │  │  - Cost calculation │  │  - Insurance mgmt   │   │
│  │  - Validation     │  │  - Preventive care  │  │  - Violation mgmt   │   │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘   │
│                                                                 │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐   │
│  │    Vehicle         │  │ VehicleMaintenance  │  │VehicleCompliance    │   │
│  │  - Id, RegNumber   │  │  - Id, VehicleId    │  │  - Id, VehicleId    │   │
│  │  - Make, Model     │  │  - Type, Date       │  │  - License, Insurance│   │
│  │  - Capacity        │  │  - Cost, Status     │  │  - Expiry, Status   │   │
│  │  - TransporterId   │  │  - Description      │  │  - Violations       │   │
│  │  - SaccoId         │  │  - NextService      │  │  - Inspections      │   │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘   │
└─────────────────────────────────────────────────────────────────┘
┌─────────────────────────────────────────────────────────────────┐
│                    Infrastructure Layer                         │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐   │
│  │ VehicleRepository  │  │MaintenanceRepository│  │ComplianceRepository │   │
│  │  - CRUD operations │  │  - CRUD operations  │  │  - CRUD operations  │   │
│  │  - Fleet queries   │  │  - Schedule queries │  │  - License queries  │   │
│  │  - Assignment qry  │  │  - Cost queries     │  │  - Insurance queries│   │
│  │  - Performance qry │  │  - History queries  │  │  - Violation queries│   │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘   │
│                                                                 │
│  ┌─────────────────────────────────────────────────────────────┐  │
│  │                 VehicleDbContext                             │  │
│  │  - Vehicles, Maintenance, Compliance, SACCO Tables          │  │
│  │  - Entity configurations and relationships                  │  │
│  │  - Database migrations and seeding                          │  │
│  └─────────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
```

---

## 📊 **Entity Model**

### **Core Entities**

#### **Vehicle Entity**
```csharp
public class Vehicle : BaseEntity
{
    public string RegistrationNumber { get; set; }      // "KBA 123A"
    public string ChassisNumber { get; set; }           // "WJMM62NV7KC123456"
    public string EngineNumber { get; set; }            // "4M40T123456"
    public VehicleDetails Details { get; set; }
    public VehicleSpecifications Specifications { get; set; }
    public string TransporterId { get; set; }           // References Transporter Service
    public string? SaccoId { get; set; }                // References SACCO if applicable
    public VehicleOwnership Ownership { get; set; }
    public VehicleStatus Status { get; set; }           // Active, Inactive, Maintenance, Decommissioned
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedBy { get; set; }
    public string UpdatedBy { get; set; }
}

public class VehicleDetails
{
    public string Make { get; set; }                    // "Isuzu"
    public string Model { get; set; }                   // "FVZ 1400"
    public int Year { get; set; }                       // 2020
    public string Color { get; set; }                   // "White"
    public VehicleType Type { get; set; }               // Truck, Trailer, Tanker
    public string Category { get; set; }                // "Heavy Commercial"
    public FuelType FuelType { get; set; }              // Diesel, Petrol, Electric
    public TransmissionType Transmission { get; set; }  // Manual, Automatic
    public string Country { get; set; }                 // "Kenya"
    public DateTime FirstRegistration { get; set; }     // 2020-03-15
    public DateTime ImportDate { get; set; }            // 2020-02-20
}

public enum VehicleType
{
    Truck,              // Standard truck
    Trailer,            // Trailer (requires tractor)
    Tanker,             // Liquid cargo tanker
    Tractor,            // Tractor unit
    Van,                // Delivery van
    Pickup,             // Pickup truck
    Specialized         // Specialized vehicle
}

public enum FuelType
{
    Diesel,             // Diesel engine
    Petrol,             // Petrol engine
    Electric,           // Electric vehicle
    Hybrid,             // Hybrid engine
    Gas,                // Natural gas
    Other               // Other fuel type
}

public enum TransmissionType
{
    Manual,             // Manual transmission
    Automatic,          // Automatic transmission
    SemiAutomatic,      // Semi-automatic
    CVT                 // Continuously variable transmission
}

public class VehicleSpecifications
{
    public decimal MaxLoadCapacity { get; set; }        // 15.0 (tons)
    public decimal GrossVehicleWeight { get; set; }     // 22.0 (tons)
    public decimal TareWeight { get; set; }             // 7.0 (tons)
    public CargoAreaDimensions CargoDimensions { get; set; }
    public EngineSpecifications Engine { get; set; }
    public decimal FuelTankCapacity { get; set; }       // 200.0 (liters)
    public decimal FuelConsumption { get; set; }        // 8.5 (km/liter)
    public int SeatingCapacity { get; set; }            // 2 (driver + assistant)
    public List<string> SpecialFeatures { get; set; }   // ["GPS", "Reverse Camera", "Air Conditioning"]
}

public class CargoAreaDimensions
{
    public decimal Length { get; set; }                 // 6.2 (meters)
    public decimal Width { get; set; }                  // 2.4 (meters)
    public decimal Height { get; set; }                 // 2.5 (meters)
    public decimal Volume { get; set; }                 // 37.2 (cubic meters)
    public string LoadingType { get; set; }             // "Rear Loading", "Side Loading", "Top Loading"
}

public class EngineSpecifications
{
    public decimal Displacement { get; set; }           // 5.2 (liters)
    public int Power { get; set; }                      // 140 (HP)
    public int Torque { get; set; }                     // 450 (Nm)
    public string Configuration { get; set; }           // "4-cylinder inline"
    public string EmissionStandard { get; set; }        // "Euro 4"
}

public class VehicleOwnership
{
    public OwnershipType Type { get; set; }             // Own, Lease, Hire
    public string OwnerName { get; set; }               // "John Mwangi"
    public string OwnerContact { get; set; }            // "+254712345678"
    public string OwnerAddress { get; set; }            // "P.O. Box 123, Nairobi"
    public DateTime? LeaseStartDate { get; set; }       // For leased vehicles
    public DateTime? LeaseEndDate { get; set; }         // For leased vehicles
    public decimal? LeaseAmount { get; set; }           // Monthly lease amount
    public string? LeaseCompany { get; set; }           // "ABC Leasing Ltd"
}

public enum OwnershipType
{
    Own,                // Owned by transporter
    Lease,              // Leased vehicle
    Hire,               // Hired vehicle
    Financed,           // Financed vehicle
    Partnership         // Partnership arrangement
}
```

#### **VehicleMaintenance Entity**
```csharp
public class VehicleMaintenance : BaseEntity
{
    public string VehicleId { get; set; }               // References Vehicle
    public MaintenanceType Type { get; set; }           // Preventive, Corrective, Emergency
    public string Description { get; set; }             // "Engine oil change and filter replacement"
    public DateTime ScheduledDate { get; set; }         // 2024-01-25
    public DateTime? ActualDate { get; set; }           // Actual service date
    public int Mileage { get; set; }                    // 45000 (km)
    public MaintenanceDetails Details { get; set; }
    public MaintenanceStatus Status { get; set; }       // Scheduled, InProgress, Completed, Cancelled
    public string ServiceProviderId { get; set; }       // References service provider
    public MaintenanceCost Cost { get; set; }
    public DateTime? NextServiceDate { get; set; }      // Next scheduled service
    public int? NextServiceMileage { get; set; }        // Next service mileage
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedBy { get; set; }
    public string UpdatedBy { get; set; }
    
    // Navigation properties
    public Vehicle Vehicle { get; set; }
}

public enum MaintenanceType
{
    Preventive,         // Scheduled maintenance
    Corrective,         // Repair maintenance
    Emergency,          // Emergency repair
    Inspection,         // Inspection service
    Upgrade,            // System upgrade
    Recall              // Manufacturer recall
}

public class MaintenanceDetails
{
    public List<string> ServicesPerformed { get; set; } // ["Oil Change", "Filter Replacement"]
    public List<string> PartsReplaced { get; set; }     // ["Oil Filter", "Air Filter"]
    public List<string> IssuesFound { get; set; }       // ["Worn brake pads", "Low tire pressure"]
    public List<string> Recommendations { get; set; }   // ["Replace brake pads soon", "Check tire pressure weekly"]
    public string TechnicianName { get; set; }          // "Peter Kamau"
    public string TechnicianLicense { get; set; }       // "MECH-2024-001"
    public string ServiceLocation { get; set; }         // "ABC Auto Service"
    public TimeSpan ServiceDuration { get; set; }       // 3 hours
}

public class MaintenanceCost
{
    public decimal LaborCost { get; set; }              // 5000.0 (KES)
    public decimal PartsCost { get; set; }              // 8000.0 (KES)
    public decimal TotalCost { get; set; }              // 13000.0 (KES)
    public string Currency { get; set; }                // "KES"
    public string InvoiceNumber { get; set; }           // "INV-2024-001"
    public DateTime InvoiceDate { get; set; }           // 2024-01-25
    public PaymentStatus PaymentStatus { get; set; }    // Paid, Pending, Overdue
}

public enum PaymentStatus
{
    Paid,               // Payment completed
    Pending,            // Payment pending
    Overdue,            // Payment overdue
    Disputed,           // Payment disputed
    Cancelled           // Payment cancelled
}
```

#### **VehicleCompliance Entity**
```csharp
public class VehicleCompliance : BaseEntity
{
    public string VehicleId { get; set; }               // References Vehicle
    public ComplianceType Type { get; set; }            // License, Insurance, Inspection
    public string DocumentNumber { get; set; }          // "INS-2024-001"
    public ComplianceDetails Details { get; set; }
    public DateTime IssueDate { get; set; }             // 2024-01-01
    public DateTime ExpiryDate { get; set; }            // 2024-12-31
    public ComplianceStatus Status { get; set; }        // Valid, Expired, Suspended, Cancelled
    public string IssuingAuthority { get; set; }        // "NTSA", "Insurance Company"
    public ComplianceCost Cost { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedBy { get; set; }
    public string UpdatedBy { get; set; }
    
    // Navigation properties
    public Vehicle Vehicle { get; set; }
}

public enum ComplianceType
{
    License,            // Vehicle license
    Insurance,          // Vehicle insurance
    Inspection,         // Vehicle inspection
    Permit,             // Special permit
    Certification,      // Safety certification
    Registration        // Vehicle registration
}

public class ComplianceDetails
{
    public string DocumentType { get; set; }            // "Comprehensive Insurance"
    public string CoverageDetails { get; set; }         // "Third party, fire, theft"
    public decimal CoverageAmount { get; set; }         // 2000000.0 (KES)
    public string Conditions { get; set; }              // "No night driving"
    public List<string> Restrictions { get; set; }      // ["Commercial use only"]
    public string ContactPerson { get; set; }           // "Jane Wanjiku"
    public string ContactPhone { get; set; }            // "+254723456789"
    public string ContactEmail { get; set; }            // "jane@insurance.co.ke"
}

public class ComplianceCost
{
    public decimal Amount { get; set; }                 // 45000.0 (KES)
    public string Currency { get; set; }                // "KES"
    public DateTime PaymentDate { get; set; }           // 2024-01-01
    public string PaymentMethod { get; set; }           // "Bank Transfer"
    public string ReceiptNumber { get; set; }           // "REC-2024-001"
    public PaymentStatus Status { get; set; }           // Paid, Pending, Overdue
}
```

#### **VehicleAssignment Entity**
```csharp
public class VehicleAssignment : BaseEntity
{
    public string VehicleId { get; set; }               // References Vehicle
    public string? DriverId { get; set; }               // References Driver Service
    public string? RouteId { get; set; }                // References Route Service
    public string? OrderId { get; set; }                // References Customer Service order
    public DateTime AssignedDate { get; set; }          // 2024-01-25T06:00:00Z
    public DateTime? StartDate { get; set; }            // Trip start date
    public DateTime? EndDate { get; set; }              // Trip end date
    public AssignmentStatus Status { get; set; }        // Assigned, InProgress, Completed, Cancelled
    public AssignmentDetails Details { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedBy { get; set; }
    public string UpdatedBy { get; set; }
    
    // Navigation properties
    public Vehicle Vehicle { get; set; }
}

public enum AssignmentStatus
{
    Assigned,           // Vehicle assigned to trip
    InProgress,         // Trip in progress
    Completed,          // Trip completed
    Cancelled,          // Assignment cancelled
    Rescheduled        // Assignment rescheduled
}

public class AssignmentDetails
{
    public string Origin { get; set; }                  // "Mombasa Plant"
    public string Destination { get; set; }             // "Nairobi Warehouse"
    public decimal LoadWeight { get; set; }             // 45.0 (tons)
    public string CargoType { get; set; }               // "Cement"
    public string SpecialInstructions { get; set; }     // "Handle with care"
    public decimal EstimatedDistance { get; set; }      // 485.5 (km)
    public TimeSpan EstimatedDuration { get; set; }     // 6 hours 30 minutes
    public string Priority { get; set; }                // "High", "Medium", "Low"
}
```

#### **VehicleSacco Entity**
```csharp
public class VehicleSacco : BaseEntity
{
    public string Name { get; set; }                    // "Mombasa Transport SACCO"
    public string RegistrationNumber { get; set; }      // "SACCO-2020-001"
    public string LicenseNumber { get; set; }           // "SACCO-LIC-001"
    public SaccoDetails Details { get; set; }
    public ContactInformation Contact { get; set; }
    public List<string> VehicleIds { get; set; }        // References to vehicles
    public SaccoStatus Status { get; set; }             // Active, Inactive, Suspended
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedBy { get; set; }
    public string UpdatedBy { get; set; }
}

public class SaccoDetails
{
    public string Type { get; set; }                    // "Transport SACCO"
    public int MemberCount { get; set; }                // 150
    public int VehicleCount { get; set; }               // 75
    public string OperatingRegion { get; set; }         // "Coast Region"
    public List<string> Services { get; set; }          // ["Cargo Transport", "Passenger Transport"]
    public string ChairmanName { get; set; }            // "John Mwangi"
    public string SecretaryName { get; set; }           // "Mary Wanjiku"
    public string TreasurerName { get; set; }           // "Peter Kamau"
}

public enum SaccoStatus
{
    Active,             // SACCO is operational
    Inactive,           // SACCO is not operational
    Suspended,          // SACCO is suspended
    Dissolved,          // SACCO has been dissolved
    UnderReview         // SACCO is under review
}
```

### **Entity Relationships**

```
┌─────────────────┐         ┌─────────────────┐         ┌─────────────────┐
│    Vehicle      │◄────────┤VehicleMaintenance│         │VehicleCompliance│
│                 │         │                 │         │                 │
│ • Id            │         │ • VehicleId     │         │ • VehicleId     │
│ • RegNumber     │         │ • Type          │         │ • Type          │
│ • Make/Model    │         │ • Description   │         │ • DocumentNumber│
│ • Capacity      │         │ • ScheduledDate │         │ • ExpiryDate    │
│ • TransporterId │         │ • Status        │         │ • Status        │
│ • SaccoId       │         │ • Cost          │         │ • Cost          │
│ • Status        │         └─────────────────┘         └─────────────────┘
└─────────────────┘                   │                           │
         │                           │                           │
         │                           ▼                           ▼
         ▼                 ┌─────────────────┐         ┌─────────────────┐
┌─────────────────┐         │VehicleAssignment│         │  VehicleSacco   │
│ TransporterService│         │                 │         │                 │
│ (External)      │         │ • VehicleId     │         │ • Id            │
│                 │         │ • DriverId      │         │ • Name          │
│ • Fleet Info    │         │ • RouteId       │         │ • RegNumber     │
│ • Assignments   │         │ • OrderId       │         │ • MemberCount   │
│ • Performance   │         │ • Status        │         │ • VehicleCount  │
└─────────────────┘         │ • Details       │         │ • Status        │
                           └─────────────────┘         └─────────────────┘
```

---

## 🔌 **API Endpoints**

### **Vehicle Management**

#### **GET /api/vehicles**
**Purpose**: Retrieve paginated vehicle list with filtering
```json
// Request
GET /api/vehicles?page=1&size=10&transporter=trans-001&type=Truck&status=Active

// Response
{
  "data": [
    {
      "id": "vehicle-001",
      "registrationNumber": "KBA 123A",
      "chassisNumber": "WJMM62NV7KC123456",
      "engineNumber": "4M40T123456",
      "details": {
        "make": "Isuzu",
        "model": "FVZ 1400",
        "year": 2020,
        "color": "White",
        "type": "Truck",
        "category": "Heavy Commercial",
        "fuelType": "Diesel",
        "transmission": "Manual",
        "country": "Kenya",
        "firstRegistration": "2020-03-15T00:00:00Z",
        "importDate": "2020-02-20T00:00:00Z"
      },
      "specifications": {
        "maxLoadCapacity": 15.0,
        "grossVehicleWeight": 22.0,
        "tareWeight": 7.0,
        "cargoDimensions": {
          "length": 6.2,
          "width": 2.4,
          "height": 2.5,
          "volume": 37.2,
          "loadingType": "Rear Loading"
        },
        "engine": {
          "displacement": 5.2,
          "power": 140,
          "torque": 450,
          "configuration": "4-cylinder inline",
          "emissionStandard": "Euro 4"
        },
        "fuelTankCapacity": 200.0,
        "fuelConsumption": 8.5,
        "seatingCapacity": 2,
        "specialFeatures": ["GPS", "Reverse Camera", "Air Conditioning"]
      },
      "transporterId": "trans-001",
      "saccoId": "sacco-001",
      "ownership": {
        "type": "Own",
        "ownerName": "John Mwangi",
        "ownerContact": "+254712345678",
        "ownerAddress": "P.O. Box 123, Nairobi",
        "leaseStartDate": null,
        "leaseEndDate": null,
        "leaseAmount": null,
        "leaseCompany": null
      },
      "status": "Active",
      "createdAt": "2024-01-15T10:30:00Z",
      "updatedAt": "2024-01-20T14:45:00Z"
    }
  ],
  "pagination": {
    "page": 1,
    "size": 10,
    "total": 45,
    "totalPages": 5
  }
}
```

#### **GET /api/vehicles/{id}**
**Purpose**: Retrieve specific vehicle details
```json
// Request
GET /api/vehicles/vehicle-001

// Response: Complete vehicle object with all specifications
```

#### **POST /api/vehicles**
**Purpose**: Create new vehicle
```json
// Request
POST /api/vehicles
{
  "registrationNumber": "KCA 456B",
  "chassisNumber": "WJMM62NV7KC789012",
  "engineNumber": "4M40T789012",
  "details": {
    "make": "Isuzu",
    "model": "FVZ 1400",
    "year": 2021,
    "color": "Blue",
    "type": "Truck",
    "category": "Heavy Commercial",
    "fuelType": "Diesel",
    "transmission": "Manual",
    "country": "Kenya",
    "firstRegistration": "2021-03-15T00:00:00Z",
    "importDate": "2021-02-20T00:00:00Z"
  },
  "specifications": {
    "maxLoadCapacity": 15.0,
    "grossVehicleWeight": 22.0,
    "tareWeight": 7.0,
    "cargoDimensions": {
      "length": 6.2,
      "width": 2.4,
      "height": 2.5,
      "volume": 37.2,
      "loadingType": "Rear Loading"
    },
    "engine": {
      "displacement": 5.2,
      "power": 140,
      "torque": 450,
      "configuration": "4-cylinder inline",
      "emissionStandard": "Euro 4"
    },
    "fuelTankCapacity": 200.0,
    "fuelConsumption": 8.5,
    "seatingCapacity": 2,
    "specialFeatures": ["GPS", "Reverse Camera"]
  },
  "transporterId": "trans-001",
  "saccoId": "sacco-001",
  "ownership": {
    "type": "Own",
    "ownerName": "Mary Wanjiku",
    "ownerContact": "+254723456789",
    "ownerAddress": "P.O. Box 456, Nairobi"
  },
  "status": "Active"
}

// Response: Created vehicle object
```

#### **PUT /api/vehicles/{id}**
**Purpose**: Update existing vehicle
```json
// Request: Updated vehicle object
// Response: Updated vehicle object
```

#### **DELETE /api/vehicles/{id}**
**Purpose**: Soft delete vehicle (mark as inactive)
```json
// Request
DELETE /api/vehicles/vehicle-001

// Response
{
  "message": "Vehicle deactivated successfully",
  "vehicleId": "vehicle-001"
}
```

### **Maintenance Management**

#### **GET /api/vehicles/{id}/maintenance**
**Purpose**: Retrieve vehicle maintenance history
```json
// Request
GET /api/vehicles/vehicle-001/maintenance?type=Preventive&status=Completed

// Response
{
  "data": [
    {
      "id": "maint-001",
      "vehicleId": "vehicle-001",
      "type": "Preventive",
      "description": "Engine oil change and filter replacement",
      "scheduledDate": "2024-01-25T08:00:00Z",
      "actualDate": "2024-01-25T09:30:00Z",
      "mileage": 45000,
      "details": {
        "servicesPerformed": ["Oil Change", "Filter Replacement"],
        "partsReplaced": ["Oil Filter", "Air Filter"],
        "issuesFound": ["Worn brake pads"],
        "recommendations": ["Replace brake pads soon"],
        "technicianName": "Peter Kamau",
        "technicianLicense": "MECH-2024-001",
        "serviceLocation": "ABC Auto Service",
        "serviceDuration": "03:00:00"
      },
      "status": "Completed",
      "serviceProviderId": "service-001",
      "cost": {
        "laborCost": 5000.0,
        "partsCost": 8000.0,
        "totalCost": 13000.0,
        "currency": "KES",
        "invoiceNumber": "INV-2024-001",
        "invoiceDate": "2024-01-25T00:00:00Z",
        "paymentStatus": "Paid"
      },
      "nextServiceDate": "2024-04-25T08:00:00Z",
      "nextServiceMileage": 50000,
      "createdAt": "2024-01-20T10:00:00Z",
      "updatedAt": "2024-01-25T12:00:00Z"
    }
  ]
}
```

#### **POST /api/vehicles/{id}/maintenance**
**Purpose**: Schedule new maintenance
```json
// Request
POST /api/vehicles/vehicle-001/maintenance
{
  "type": "Preventive",
  "description": "Brake pad replacement and wheel alignment",
  "scheduledDate": "2024-02-15T08:00:00Z",
  "mileage": 48000,
  "serviceProviderId": "service-001",
  "estimatedCost": 15000.0
}

// Response: Created maintenance record
```

#### **PUT /api/maintenance/{id}**
**Purpose**: Update maintenance record
```json
// Request
PUT /api/maintenance/maint-001
{
  "actualDate": "2024-01-25T09:30:00Z",
  "status": "Completed",
  "details": {
    "servicesPerformed": ["Oil Change", "Filter Replacement"],
    "partsReplaced": ["Oil Filter", "Air Filter"],
    "issuesFound": ["Worn brake pads"],
    "recommendations": ["Replace brake pads soon"],
    "technicianName": "Peter Kamau",
    "technicianLicense": "MECH-2024-001",
    "serviceLocation": "ABC Auto Service",
    "serviceDuration": "03:00:00"
  },
  "cost": {
    "laborCost": 5000.0,
    "partsCost": 8000.0,
    "totalCost": 13000.0,
    "currency": "KES",
    "invoiceNumber": "INV-2024-001",
    "invoiceDate": "2024-01-25T00:00:00Z",
    "paymentStatus": "Paid"
  },
  "nextServiceDate": "2024-04-25T08:00:00Z",
  "nextServiceMileage": 50000
}

// Response: Updated maintenance record
```

### **Compliance Management**

#### **GET /api/vehicles/{id}/compliance**
**Purpose**: Retrieve vehicle compliance records
```json
// Request
GET /api/vehicles/vehicle-001/compliance?type=Insurance&status=Valid

// Response
{
  "data": [
    {
      "id": "comp-001",
      "vehicleId": "vehicle-001",
      "type": "Insurance",
      "documentNumber": "INS-2024-001",
      "details": {
        "documentType": "Comprehensive Insurance",
        "coverageDetails": "Third party, fire, theft",
        "coverageAmount": 2000000.0,
        "conditions": "Commercial use only",
        "restrictions": ["No night driving after 10 PM"],
        "contactPerson": "Jane Wanjiku",
        "contactPhone": "+254723456789",
        "contactEmail": "jane@insurance.co.ke"
      },
      "issueDate": "2024-01-01T00:00:00Z",
      "expiryDate": "2024-12-31T23:59:59Z",
      "status": "Valid",
      "issuingAuthority": "ABC Insurance Company",
      "cost": {
        "amount": 45000.0,
        "currency": "KES",
        "paymentDate": "2024-01-01T00:00:00Z",
        "paymentMethod": "Bank Transfer",
        "receiptNumber": "REC-2024-001",
        "status": "Paid"
      },
      "createdAt": "2024-01-01T10:00:00Z",
      "updatedAt": "2024-01-01T10:00:00Z"
    }
  ]
}
```

#### **POST /api/vehicles/{id}/compliance**
**Purpose**: Add new compliance record
```json
// Request
POST /api/vehicles/vehicle-001/compliance
{
  "type": "License",
  "documentNumber": "LIC-2024-001",
  "details": {
    "documentType": "Commercial Vehicle License",
    "coverageDetails": "General cargo transport",
    "coverageAmount": 0,
    "conditions": "Valid for commercial use",
    "restrictions": ["Weight limit 22 tons"],
    "contactPerson": "NTSA Officer",
    "contactPhone": "+254700123456",
    "contactEmail": "info@ntsa.go.ke"
  },
  "issueDate": "2024-01-01T00:00:00Z",
  "expiryDate": "2024-12-31T23:59:59Z",
  "status": "Valid",
  "issuingAuthority": "NTSA",
  "cost": {
    "amount": 5000.0,
    "currency": "KES",
    "paymentDate": "2024-01-01T00:00:00Z",
    "paymentMethod": "Cash",
    "receiptNumber": "NTSA-REC-001",
    "status": "Paid"
  }
}

// Response: Created compliance record
```

### **Assignment Management**

#### **GET /api/vehicles/{id}/assignments**
**Purpose**: Retrieve vehicle assignments
```json
// Request
GET /api/vehicles/vehicle-001/assignments?status=InProgress

// Response
{
  "data": [
    {
      "id": "assign-001",
      "vehicleId": "vehicle-001",
      "driverId": "driver-001",
      "routeId": "route-001",
      "orderId": "order-001",
      "assignedDate": "2024-01-25T06:00:00Z",
      "startDate": "2024-01-25T06:30:00Z",
      "endDate": null,
      "status": "InProgress",
      "details": {
        "origin": "Mombasa Plant",
        "destination": "Nairobi Warehouse",
        "loadWeight": 45.0,
        "cargoType": "Cement",
        "specialInstructions": "Handle with care",
        "estimatedDistance": 485.5,
        "estimatedDuration": "06:30:00",
        "priority": "High"
      },
      "createdAt": "2024-01-24T15:00:00Z",
      "updatedAt": "2024-01-25T06:30:00Z"
    }
  ]
}
```

#### **POST /api/vehicles/{id}/assignments**
**Purpose**: Create new vehicle assignment
```json
// Request
POST /api/vehicles/vehicle-001/assignments
{
  "driverId": "driver-002",
  "routeId": "route-002",
  "orderId": "order-002",
  "assignedDate": "2024-01-26T06:00:00Z",
  "details": {
    "origin": "Kisumu Plant",
    "destination": "Nairobi Warehouse",
    "loadWeight": 40.0,
    "cargoType": "Cement",
    "specialInstructions": "Urgent delivery",
    "estimatedDistance": 350.0,
    "estimatedDuration": "05:00:00",
    "priority": "High"
  }
}

// Response: Created assignment record
```

### **SACCO Management**

#### **GET /api/saccos**
**Purpose**: Retrieve SACCO list
```json
// Request
GET /api/saccos?region=Coast&status=Active

// Response
{
  "data": [
    {
      "id": "sacco-001",
      "name": "Mombasa Transport SACCO",
      "registrationNumber": "SACCO-2020-001",
      "licenseNumber": "SACCO-LIC-001",
      "details": {
        "type": "Transport SACCO",
        "memberCount": 150,
        "vehicleCount": 75,
        "operatingRegion": "Coast Region",
        "services": ["Cargo Transport", "Passenger Transport"],
        "chairmanName": "John Mwangi",
        "secretaryName": "Mary Wanjiku",
        "treasurerName": "Peter Kamau"
      },
      "contact": {
        "primaryContactName": "John Mwangi",
        "primaryContactPhone": "+254712345678",
        "primaryContactEmail": "chairman@mombasasacco.co.ke",
        "secondaryContactName": "Mary Wanjiku",
        "secondaryContactPhone": "+254723456789",
        "secondaryContactEmail": "secretary@mombasasacco.co.ke"
      },
      "vehicleIds": ["vehicle-001", "vehicle-002", "vehicle-003"],
      "status": "Active",
      "createdAt": "2024-01-15T10:30:00Z",
      "updatedAt": "2024-01-20T14:45:00Z"
    }
  ]
}
```

### **Business-Specific Endpoints**

#### **GET /api/vehicles/availability**
**Purpose**: Check vehicle availability
```json
// Request
GET /api/vehicles/availability?transporter=trans-001&date=2024-01-25&capacity=15

// Response
{
  "availableVehicles": [
    {
      "id": "vehicle-001",
      "registrationNumber": "KBA 123A",
      "specifications": {
        "maxLoadCapacity": 15.0,
        "type": "Truck"
      },
      "status": "Available",
      "currentLocation": "Mombasa Plant",
      "maintenanceStatus": "Up to date",
      "complianceStatus": "Valid",
      "assignmentStatus": "Free",
      "availabilityScore": 95.0
    }
  ]
}
```

#### **GET /api/vehicles/performance**
**Purpose**: Vehicle performance analytics
```json
// Request
GET /api/vehicles/performance?vehicle=vehicle-001&period=2024-01

// Response
{
  "vehicleId": "vehicle-001",
  "period": "2024-01",
  "performance": {
    "totalTrips": 25,
    "totalDistance": 12000.0,
    "totalFuelConsumption": 1400.0,
    "averageFuelEfficiency": 8.6,
    "utilizationRate": 85.0,
    "onTimePerformance": 92.0,
    "maintenanceDowntime": 8.0,
    "complianceScore": 100.0
  },
  "costs": {
    "fuelCost": 168000.0,
    "maintenanceCost": 45000.0,
    "complianceCost": 12000.0,
    "totalCost": 225000.0,
    "costPerKm": 18.75
  },
  "trends": {
    "fuelEfficiencyTrend": "Improving",
    "maintenanceTrend": "Stable",
    "utilizationTrend": "Increasing"
  }
}
```

#### **POST /api/vehicles/bulk-update**
**Purpose**: Bulk update vehicle information
```json
// Request
POST /api/vehicles/bulk-update
{
  "vehicles": [
    {
      "id": "vehicle-001",
      "status": "Maintenance",
      "reason": "Scheduled maintenance"
    },
    {
      "id": "vehicle-002",
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
      "id": "vehicle-001",
      "status": "Success",
      "message": "Vehicle status updated to Maintenance"
    },
    {
      "id": "vehicle-002",
      "status": "Success",
      "message": "Vehicle status updated to Active"
    }
  ]
}
```

---

## 🔗 **Integration Points**

### **Service Dependencies**

#### **Transporter Service Integration**
```
┌─────────────────┐         ┌─────────────────┐
│Transporter Service│◄──────►│ Vehicle Service │
│                 │         │                 │
│ • Fleet Info    │         │ • Vehicle Details│
│ • Capacity      │         │ • Availability  │
│ • Assignments   │         │ • Performance   │
└─────────────────┘         └─────────────────┘
```

#### **Driver Service Integration** (Future)
```
┌─────────────────┐         ┌─────────────────┐
│ Driver Service  │◄───────►│ Vehicle Service │
│                 │         │                 │
│ • Driver Info   │         │ • Assignments   │
│ • Availability  │         │ • Vehicle Info  │
│ • Performance   │         │ • Restrictions  │
└─────────────────┘         └─────────────────┘
```

#### **Route Service Integration** (Future)
```
┌─────────────────┐         ┌─────────────────┐
│ Route Service   │◄───────►│ Vehicle Service │
│                 │         │                 │
│ • Route Info    │         │ • Vehicle Info  │
│ • Schedules     │         │ • Assignments   │
│ • Restrictions  │         │ • Tracking      │
└─────────────────┘         └─────────────────┘
```

### **Integration Patterns**

#### **Vehicle Assignment Pattern**
```csharp
// Transporter Service -> Vehicle Service
public async Task<VehicleAssignment> AssignVehicleAsync(VehicleAssignmentRequest request)
{
    var assignment = await _vehicleService.AssignVehicleAsync(new VehicleAssignmentRequest
    {
        VehicleId = request.VehicleId,
        DriverId = request.DriverId,
        RouteId = request.RouteId,
        OrderId = request.OrderId,
        AssignedDate = request.AssignedDate,
        Details = request.Details
    });
    
    return assignment;
}
```

#### **Availability Check Pattern**
```csharp
// Transporter Service -> Vehicle Service
public async Task<bool> CheckVehicleAvailabilityAsync(string vehicleId, DateTime date)
{
    var availability = await _vehicleService.GetVehicleAvailabilityAsync(vehicleId, date);
    return availability.IsAvailable;
}
```

---

## 💼 **Business Use Cases**

### **Use Case 1: Vehicle Registration**

**Scenario**: New vehicle is added to transporter's fleet
**Actors**: Fleet Manager, System, NTSA
**Flow**:
1. Fleet manager enters vehicle details and specifications
2. System validates registration number uniqueness
3. Vehicle ownership documents are uploaded
4. System creates vehicle record with all specifications
5. Compliance records are created for license and insurance
6. Vehicle becomes available for assignment
7. SACCO association is established if applicable

**Business Value**: Comprehensive vehicle tracking and compliance management

### **Use Case 2: Maintenance Scheduling**

**Scenario**: Vehicle requires scheduled maintenance
**Actors**: Fleet Manager, Mechanic, System
**Flow**:
1. System identifies vehicle due for maintenance based on mileage/time
2. Fleet manager schedules maintenance appointment
3. Vehicle is temporarily marked as unavailable
4. Mechanic performs maintenance and updates records
5. System calculates next maintenance due date
6. Vehicle is returned to active status
7. Maintenance costs are tracked and reported

**Business Value**: Proactive maintenance reduces breakdowns and extends vehicle life

### **Use Case 3: Compliance Monitoring**

**Scenario**: Vehicle license is about to expire
**Actors**: Compliance Officer, System, NTSA
**Flow**:
1. System monitors compliance expiry dates
2. Automated alerts are sent 30 days before expiry
3. Compliance officer initiates renewal process
4. New documents are uploaded to system
5. System updates compliance status
6. Vehicle remains in active status
7. Compliance reports are generated

**Business Value**: Ensures legal compliance and avoids penalties

### **Use Case 4: Fleet Performance Analysis**

**Scenario**: Monthly fleet performance review
**Actors**: Fleet Manager, System, Management
**Flow**:
1. System collects performance data from all vehicles
2. Key performance indicators are calculated
3. Performance trends are analyzed
4. Underperforming vehicles are identified
5. Maintenance recommendations are generated
6. Fleet optimization suggestions are provided
7. Performance reports are distributed

**Business Value**: Data-driven fleet optimization and cost reduction

---

## 🛣️ **Implementation Roadmap**

### **Phase 1: Foundation** (Month 4, Week 1-2)
- 🔄 **Project Setup**: Create Clean Architecture structure
- 🔄 **Core Entities**: Implement Vehicle, Maintenance, Compliance entities
- 🔄 **Database Layer**: Set up Entity Framework with SQLite
- 🔄 **Basic Repository**: Implement repository pattern for data access

### **Phase 2: Core Features** (Month 4, Week 3-4)
- 🔄 **Vehicle Management**: Complete CRUD operations for vehicles
- 🔄 **Maintenance Management**: Maintenance scheduling and tracking
- 🔄 **Compliance Management**: License and insurance tracking
- 🔄 **API Controllers**: REST API endpoints for all operations

### **Phase 3: Integration** (Month 5, Week 1-2)
- 🔄 **Transporter Service Integration**: Vehicle assignments and availability
- 🔄 **Assignment System**: Vehicle-driver-route assignments
- 🔄 **SACCO Integration**: SACCO-vehicle relationships
- 🔄 **API Documentation**: Comprehensive OpenAPI documentation

### **Phase 4: Advanced Features** (Month 5, Week 3-4)
- 🔄 **Performance Analytics**: Vehicle performance tracking
- 🔄 **Predictive Maintenance**: Maintenance prediction algorithms
- 🔄 **Compliance Automation**: Automated compliance monitoring
- 🔄 **Monitoring**: Health checks and performance metrics

### **Current Status**: 🔄 **READY FOR IMPLEMENTATION**
- **Priority**: MEDIUM (Fourth priority after Product, Transporter, Route)
- **Dependencies**: Transporter Service
- **Estimated Timeline**: 1 month for complete implementation
- **Resource Requirements**: 1 developer, part-time

---

## 📊 **Success Metrics**

### **Technical Metrics**
- **API Response Time**: < 200ms for vehicle searches
- **Database Performance**: < 100ms for fleet queries
- **Service Availability**: 99.9% uptime
- **Integration Success**: 100% assignment accuracy

### **Business Metrics**
- **Fleet Size**: 100+ vehicles registered
- **Maintenance Compliance**: 95% on-time maintenance
- **Legal Compliance**: 100% valid licenses and insurance
- **Fleet Utilization**: 80% average utilization rate

### **Quality Metrics**
- **Data Accuracy**: 99.5% vehicle information accuracy
- **Compliance Rate**: 100% regulatory compliance
- **Maintenance Efficiency**: 90% preventive maintenance ratio
- **System Reliability**: 0 compliance violations due to system issues

---

*Vehicle Service serves as the central vehicle registry and fleet management hub, enabling comprehensive vehicle tracking, maintenance management, and compliance monitoring across the QaliTrack transportation network.*