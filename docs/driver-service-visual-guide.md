# Driver Service Visual Guide

## 🎯 **Service Overview**

### **Business Purpose**
The Driver Service manages QaliTrack's driver registry, including personnel management, license validation, certification tracking, performance monitoring, and compliance management. It provides centralized driver information for transporter fleet operations and regulatory compliance.

### **Key Business Problems Solved**
- **Driver Registration**: Centralized driver registration and documentation
- **License Management**: Driver license validation and renewal tracking
- **Performance Monitoring**: Driver performance metrics and safety tracking
- **Compliance Management**: Regulatory compliance and violation tracking
- **Certification Management**: Professional certifications and training records
- **Assignment Optimization**: Driver-vehicle matching and scheduling

### **Integration Role**
Central driver registry that provides driver information to transporters, validates driver assignments, and tracks driver performance and compliance across the logistics network.

---

## 🏗️ **Architecture Diagram**

```
┌─────────────────────────────────────────────────────────────────┐
│                        Driver Service                           │
│                         Port: 7004                              │
└─────────────────────────────────────────────────────────────────┘
┌─────────────────────────────────────────────────────────────────┐
│                          API Layer                              │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐   │
│  │  DriversController     │  │PerformanceController │  │ ComplianceController │  │
│  │  - CRUD operations     │  │  - KPI tracking       │  │  - License tracking  │  │
│  │  - Assignment mgmt     │  │  - Safety monitoring  │  │  - Violation mgmt    │  │
│  │  - Availability checks │  │  - Rating system      │  │  - Certification mgmt│  │
│  │  - Performance views   │  │  - Training records   │  │  - Medical records   │  │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘   │
└─────────────────────────────────────────────────────────────────┘
┌─────────────────────────────────────────────────────────────────┐
│                         Core Layer                              │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐   │
│  │  DriverService     │  │PerformanceService   │  │ComplianceService    │   │
│  │  - Driver logic    │  │  - Performance logic│  │  - Compliance logic │   │
│  │  - Assignment     │  │  - KPI calculation  │  │  - License validation│   │
│  │  - Availability   │  │  - Safety analysis  │  │  - Violation tracking│   │
│  │  - Validation     │  │  - Rating algorithms│  │  - Certification mgmt│   │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘   │
│                                                                 │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐   │
│  │     Driver         │  │ DriverPerformance   │  │ DriverCompliance    │   │
│  │  - Id, Name        │  │  - Id, DriverId     │  │  - Id, DriverId     │   │
│  │  - License         │  │  - Period, Score    │  │  - License, Medical │   │
│  │  - Contact         │  │  - Safety, Rating   │  │  - Expiry, Status   │   │
│  │  - TransporterId   │  │  - Violations       │  │  - Certifications   │   │
│  │  - Status          │  │  - Training         │  │  - Violations       │   │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘   │
└─────────────────────────────────────────────────────────────────┘
┌─────────────────────────────────────────────────────────────────┐
│                    Infrastructure Layer                         │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐   │
│  │ DriverRepository   │  │PerformanceRepository │  │ComplianceRepository │   │
│  │  - CRUD operations │  │  - CRUD operations   │  │  - CRUD operations  │   │
│  │  - Search queries  │  │  - KPI queries       │  │  - License queries  │   │
│  │  - Assignment qry  │  │  - Performance qry   │  │  - Violation queries│   │
│  │  - Availability qry│  │  - Rating queries    │  │  - Training queries │   │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘   │
│                                                                 │
│  ┌─────────────────────────────────────────────────────────────┐  │
│  │                   DriverDbContext                            │  │
│  │  - Drivers, Performance, Compliance, Training Tables        │  │
│  │  - Entity configurations and relationships                  │  │
│  │  - Database migrations and seeding                          │  │
│  └─────────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
```

---

## 📊 **Entity Model**

### **Core Entities**

#### **Driver Entity**
```csharp
public class Driver : BaseEntity
{
    public PersonalInformation Personal { get; set; }
    public ContactInformation Contact { get; set; }
    public EmergencyContact Emergency { get; set; }
    public EmploymentDetails Employment { get; set; }
    public string TransporterId { get; set; }           // References Transporter Service
    public DriverStatus Status { get; set; }            // Active, Inactive, Suspended, Terminated
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedBy { get; set; }
    public string UpdatedBy { get; set; }
}

public class PersonalInformation
{
    public string FirstName { get; set; }               // "John"
    public string LastName { get; set; }                // "Kamau"
    public string MiddleName { get; set; }               // "Mwangi"
    public string NationalId { get; set; }              // "12345678"
    public DateTime DateOfBirth { get; set; }           // 1985-03-15
    public string Gender { get; set; }                  // "Male"
    public string MaritalStatus { get; set; }           // "Married"
    public string Nationality { get; set; }             // "Kenyan"
    public string Religion { get; set; }                // "Christian"
    public Address ResidentialAddress { get; set; }
    public string PhotoUrl { get; set; }                // URL to driver photo
}

public class ContactInformation
{
    public string PrimaryPhone { get; set; }            // "+254712345678"
    public string SecondaryPhone { get; set; }          // "+254723456789"
    public string Email { get; set; }                   // "john.kamau@email.com"
    public string AlternativeEmail { get; set; }        // "j.kamau@gmail.com"
    public string WhatsAppNumber { get; set; }          // "+254712345678"
    public string TelegramHandle { get; set; }          // "@johnkamau"
    public List<string> PreferredContactMethods { get; set; } // ["Phone", "WhatsApp", "Email"]
}

public class EmergencyContact
{
    public string Name { get; set; }                    // "Mary Kamau"
    public string Relationship { get; set; }            // "Spouse"
    public string Phone { get; set; }                   // "+254734567890"
    public string AlternativePhone { get; set; }        // "+254745678901"
    public string Email { get; set; }                   // "mary.kamau@email.com"
    public Address Address { get; set; }
}

public class EmploymentDetails
{
    public string EmployeeId { get; set; }              // "EMP-2024-001"
    public DateTime HireDate { get; set; }              // 2024-01-15
    public DateTime? TerminationDate { get; set; }      // null if active
    public EmploymentType Type { get; set; }            // Permanent, Contract, Casual
    public string Position { get; set; }                // "Heavy Truck Driver"
    public string Department { get; set; }              // "Operations"
    public decimal BasicSalary { get; set; }            // 45000.0 (KES)
    public List<string> Benefits { get; set; }          // ["Medical", "Transport", "Meal Allowance"]
    public string SupervisorId { get; set; }            // References another driver/supervisor
    public WorkSchedule Schedule { get; set; }
}

public enum EmploymentType
{
    Permanent,          // Permanent employee
    Contract,           // Contract employee
    Casual,             // Casual worker
    Temporary,          // Temporary assignment
    Consultant,         // Consultant/freelancer
    Volunteer           // Volunteer driver
}

public class WorkSchedule
{
    public string Pattern { get; set; }                 // "5 days a week"
    public TimeSpan StartTime { get; set; }             // 06:00
    public TimeSpan EndTime { get; set; }               // 18:00
    public List<string> WorkDays { get; set; }          // ["Monday", "Tuesday", ..., "Friday"]
    public List<string> RestDays { get; set; }          // ["Saturday", "Sunday"]
    public decimal WeeklyHours { get; set; }            // 40.0
    public bool IsFlexible { get; set; }                // true
    public string Notes { get; set; }                   // "May work weekends during peak season"
}
```

#### **DriverCompliance Entity**
```csharp
public class DriverCompliance : BaseEntity
{
    public string DriverId { get; set; }                // References Driver
    public ComplianceType Type { get; set; }            // License, Medical, Training
    public ComplianceDetails Details { get; set; }
    public DateTime IssueDate { get; set; }             // 2024-01-01
    public DateTime ExpiryDate { get; set; }            // 2025-01-01
    public ComplianceStatus Status { get; set; }        // Valid, Expired, Suspended, Revoked
    public string IssuingAuthority { get; set; }        // "NTSA", "MOH", "Training Institute"
    public string DocumentNumber { get; set; }          // "DL-2024-001"
    public ComplianceCost Cost { get; set; }
    public List<string> Restrictions { get; set; }      // ["No night driving", "Weight limit 10 tons"]
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedBy { get; set; }
    public string UpdatedBy { get; set; }
    
    // Navigation properties
    public Driver Driver { get; set; }
}

public enum ComplianceType
{
    License,            // Driving license
    Medical,            // Medical certificate
    Training,           // Training certificate
    Insurance,          // Personal insurance
    Permit,             // Special permit
    Certification,      // Professional certification
    Background,         // Background check
    Drug,              // Drug test
    Psychological      // Psychological evaluation
}

public class ComplianceDetails
{
    public string DocumentType { get; set; }            // "Class CE Heavy Truck License"
    public List<string> VehicleCategories { get; set; } // ["Truck", "Trailer", "Tanker"]
    public List<string> Endorsements { get; set; }      // ["Hazmat", "Passenger", "School Bus"]
    public string MedicalConditions { get; set; }       // "None"
    public string TrainingCourse { get; set; }          // "Defensive Driving Course"
    public string InstructorName { get; set; }          // "Peter Mwangi"
    public decimal Score { get; set; }                  // 85.5 (exam score)
    public string Grade { get; set; }                   // "A"
    public List<string> Competencies { get; set; }      // ["Highway driving", "City driving", "Reversing"]
}

public class ComplianceCost
{
    public decimal Amount { get; set; }                 // 5000.0 (KES)
    public string Currency { get; set; }                // "KES"
    public DateTime PaymentDate { get; set; }           // 2024-01-01
    public string PaymentMethod { get; set; }           // "Bank Transfer"
    public string ReceiptNumber { get; set; }           // "REC-2024-001"
    public PaymentStatus Status { get; set; }           // Paid, Pending, Overdue
}
```

#### **DriverPerformance Entity**
```csharp
public class DriverPerformance : BaseEntity
{
    public string DriverId { get; set; }                // References Driver
    public string Period { get; set; }                  // "2024-01" (Monthly)
    public PerformanceMetrics Metrics { get; set; }
    public SafetyRecord Safety { get; set; }
    public CustomerFeedback Customer { get; set; }
    public TrainingRecord Training { get; set; }
    public decimal OverallScore { get; set; }            // 8.5 (out of 10)
    public PerformanceRating Rating { get; set; }       // Excellent, Good, Fair, Poor
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Navigation properties
    public Driver Driver { get; set; }
}

public class PerformanceMetrics
{
    public int TotalTrips { get; set; }                 // 45
    public decimal TotalDistance { get; set; }          // 12000.0 (km)
    public TimeSpan TotalDrivingTime { get; set; }      // 180 hours
    public decimal AverageSpeed { get; set; }           // 66.7 (km/h)
    public decimal FuelEfficiency { get; set; }         // 8.5 (km/liter)
    public int OnTimeDeliveries { get; set; }           // 42
    public decimal OnTimePercentage { get; set; }       // 93.3%
    public int LateDeliveries { get; set; }             // 3
    public decimal AverageDelayTime { get; set; }       // 1.5 hours
}

public class SafetyRecord
{
    public int SafetyViolations { get; set; }           // 0
    public int TrafficViolations { get; set; }          // 1
    public int Accidents { get; set; }                  // 0
    public int NearMisses { get; set; }                 // 2
    public decimal SafetyScore { get; set; }            // 9.2 (out of 10)
    public List<string> SafetyIncidents { get; set; }   // ["Overspeeding on Highway"]
    public List<string> SafetyAchievements { get; set; } // ["30 days accident-free"]
    public DateTime LastIncidentDate { get; set; }      // 2024-01-15
    public string LastIncidentType { get; set; }        // "Traffic Violation"
}

public class CustomerFeedback
{
    public decimal AverageRating { get; set; }          // 4.5 (out of 5)
    public int TotalRatings { get; set; }               // 15
    public int PositiveReviews { get; set; }            // 12
    public int NegativeReviews { get; set; }            // 3
    public List<string> CommonPraises { get; set; }     // ["Punctual", "Professional", "Careful"]
    public List<string> CommonComplaints { get; set; }  // ["Rough handling", "Late arrival"]
    public decimal RecommendationScore { get; set; }     // 8.7 (out of 10)
    public string BestReview { get; set; }              // "Excellent driver, very professional"
    public string WorstReview { get; set; }             // "Arrived 30 minutes late"
}

public class TrainingRecord
{
    public List<string> CoursesCompleted { get; set; }  // ["Defensive Driving", "Customer Service"]
    public List<string> CertificationsEarned { get; set; } // ["Safe Driver Certificate"]
    public decimal TrainingHours { get; set; }          // 40.0
    public DateTime LastTrainingDate { get; set; }      // 2024-01-20
    public string NextTrainingDue { get; set; }         // "First Aid Training"
    public DateTime NextTrainingDate { get; set; }      // 2024-04-15
    public decimal TrainingScore { get; set; }          // 92.5 (average score)
}

public enum PerformanceRating
{
    Excellent,          // 9.0-10.0
    Good,              // 7.0-8.9
    Fair,              // 5.0-6.9
    Poor,              // 3.0-4.9
    Unacceptable       // 0.0-2.9
}
```

#### **DriverAssignment Entity**
```csharp
public class DriverAssignment : BaseEntity
{
    public string DriverId { get; set; }                // References Driver
    public string? VehicleId { get; set; }              // References Vehicle Service
    public string? RouteId { get; set; }                // References Route Service
    public string? OrderId { get; set; }                // References Customer Service order
    public DateTime AssignedDate { get; set; }          // 2024-01-25T06:00:00Z
    public DateTime? StartDate { get; set; }            // Trip start date
    public DateTime? EndDate { get; set; }              // Trip end date
    public AssignmentStatus Status { get; set; }        // Assigned, InProgress, Completed, Cancelled
    public AssignmentDetails Details { get; set; }
    public List<string> RequiredSkills { get; set; }    // ["Heavy Truck", "Hazmat", "Long Distance"]
    public AssignmentPriority Priority { get; set; }    // High, Medium, Low
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedBy { get; set; }
    public string UpdatedBy { get; set; }
    
    // Navigation properties
    public Driver Driver { get; set; }
}

public enum AssignmentStatus
{
    Assigned,           // Driver assigned to trip
    InProgress,         // Trip in progress
    Completed,          // Trip completed
    Cancelled,          // Assignment cancelled
    Rescheduled,        // Assignment rescheduled
    Paused             // Assignment paused
}

public class AssignmentDetails
{
    public string Origin { get; set; }                  // "Mombasa Plant"
    public string Destination { get; set; }             // "Nairobi Warehouse"
    public decimal EstimatedDistance { get; set; }      // 485.5 (km)
    public TimeSpan EstimatedDuration { get; set; }     // 6 hours 30 minutes
    public string CargoType { get; set; }               // "Cement"
    public decimal LoadWeight { get; set; }             // 45.0 (tons)
    public List<string> SpecialInstructions { get; set; } // ["Handle with care", "Urgent delivery"]
    public List<string> RequiredDocuments { get; set; } // ["Delivery Note", "Insurance Certificate"]
    public string CustomerContact { get; set; }         // "+254712345678"
    public decimal CompensationAmount { get; set; }      // 8000.0 (KES)
    public string Notes { get; set; }                   // "First time to this location"
}

public enum AssignmentPriority
{
    High,               // Urgent assignment
    Medium,             // Normal assignment
    Low,                // Low priority assignment
    Critical           // Critical/emergency assignment
}
```

#### **DriverViolation Entity**
```csharp
public class DriverViolation : BaseEntity
{
    public string DriverId { get; set; }                // References Driver
    public ViolationType Type { get; set; }             // Traffic, Safety, Policy, Customer
    public string Description { get; set; }             // "Overspeeding on Nairobi-Mombasa Highway"
    public DateTime ViolationDate { get; set; }         // 2024-01-15
    public ViolationSeverity Severity { get; set; }     // Minor, Major, Severe, Critical
    public ViolationDetails Details { get; set; }
    public string ReportedBy { get; set; }              // "Traffic Police", "Customer", "Supervisor"
    public ViolationStatus Status { get; set; }         // Reported, Investigated, Resolved, Appealed
    public PenaltyInformation Penalty { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedBy { get; set; }
    public string UpdatedBy { get; set; }
    
    // Navigation properties
    public Driver Driver { get; set; }
}

public enum ViolationType
{
    Traffic,            // Traffic violation
    Safety,             // Safety violation
    Policy,             // Company policy violation
    Customer,           // Customer complaint
    Regulatory,         // Regulatory violation
    Behavioral,         // Behavioral issue
    Operational        // Operational violation
}

public enum ViolationSeverity
{
    Minor,              // Minor infraction
    Major,              // Major violation
    Severe,             // Severe violation
    Critical,           // Critical violation
    Termination        // Termination-worthy violation
}

public class ViolationDetails
{
    public string Location { get; set; }                // "Nairobi-Mombasa Highway KM 120"
    public string OfficerName { get; set; }             // "PC John Mwangi"
    public string OfficerBadge { get; set; }            // "12345"
    public string TicketNumber { get; set; }            // "TKT-2024-001"
    public decimal Speed { get; set; }                  // 95.0 (km/h)
    public decimal SpeedLimit { get; set; }             // 80.0 (km/h)
    public string VehicleUsed { get; set; }             // "KBA 123A"
    public List<string> Witnesses { get; set; }         // ["Assistant driver", "Customer rep"]
    public string EvidenceUrls { get; set; }            // URLs to photos/videos
    public string DriverExplanation { get; set; }       // Driver's explanation
}

public class PenaltyInformation
{
    public decimal FineAmount { get; set; }             // 3000.0 (KES)
    public string Currency { get; set; }                // "KES"
    public DateTime DueDate { get; set; }               // 2024-02-15
    public PaymentStatus PaymentStatus { get; set; }    // Paid, Pending, Overdue
    public string PenaltyType { get; set; }             // "Fine", "Suspension", "Warning"
    public int SuspensionDays { get; set; }             // 0
    public bool RequiresRetraining { get; set; }        // true
    public string RetrainingType { get; set; }          // "Defensive Driving Course"
    public bool AffectsRating { get; set; }             // true
    public decimal RatingImpact { get; set; }           // -0.5 points
}

public enum ViolationStatus
{
    Reported,           // Violation reported
    Investigated,       // Under investigation
    Resolved,           // Resolved/closed
    Appealed,           // Driver appealed
    Dismissed,          // Violation dismissed
    Pending            // Pending review
}
```

### **Entity Relationships**

```
┌─────────────────┐         ┌─────────────────┐         ┌─────────────────┐
│     Driver      │◄────────┤ DriverCompliance│         │ DriverPerformance│
│                 │         │                 │         │                 │
│ • Id            │         │ • DriverId      │         │ • DriverId      │
│ • Personal      │         │ • Type          │         │ • Period        │
│ • Contact       │         │ • Details       │         │ • Metrics       │
│ • Employment    │         │ • ExpiryDate    │         │ • Safety        │
│ • TransporterId │         │ • Status        │         │ • Customer      │
│ • Status        │         │ • Cost          │         │ • OverallScore  │
└─────────────────┘         └─────────────────┘         └─────────────────┘
         │                                                        │
         │                                                        │
         ▼                                                        ▼
┌─────────────────┐         ┌─────────────────┐         ┌─────────────────┐
│ DriverAssignment│         │ DriverViolation │         │ TransporterService│
│                 │         │                 │         │ (External)      │
│ • DriverId      │         │ • DriverId      │         │                 │
│ • VehicleId     │         │ • Type          │         │ • Driver Info   │
│ • RouteId       │         │ • Description   │         │ • Assignments   │
│ • OrderId       │         │ • Severity      │         │ • Performance   │
│ • Status        │         │ • Status        │         │ • Fleet Mgmt    │
│ • Details       │         │ • Penalty       │         └─────────────────┘
└─────────────────┘         └─────────────────┘
```

---

## 🔌 **API Endpoints**

### **Driver Management**

#### **GET /api/drivers**
**Purpose**: Retrieve paginated driver list with filtering
```json
// Request
GET /api/drivers?page=1&size=10&transporter=trans-001&status=Active&license=valid

// Response
{
  "data": [
    {
      "id": "driver-001",
      "personal": {
        "firstName": "John",
        "lastName": "Kamau",
        "middleName": "Mwangi",
        "nationalId": "12345678",
        "dateOfBirth": "1985-03-15T00:00:00Z",
        "gender": "Male",
        "maritalStatus": "Married",
        "nationality": "Kenyan",
        "religion": "Christian",
        "residentialAddress": {
          "street": "Kibera Road",
          "city": "Nairobi",
          "postalCode": "00100",
          "country": "Kenya"
        },
        "photoUrl": "https://storage.qalitrack.com/photos/driver-001.jpg"
      },
      "contact": {
        "primaryPhone": "+254712345678",
        "secondaryPhone": "+254723456789",
        "email": "john.kamau@email.com",
        "alternativeEmail": "j.kamau@gmail.com",
        "whatsAppNumber": "+254712345678",
        "preferredContactMethods": ["Phone", "WhatsApp", "Email"]
      },
      "emergency": {
        "name": "Mary Kamau",
        "relationship": "Spouse",
        "phone": "+254734567890",
        "email": "mary.kamau@email.com",
        "address": {
          "street": "Kibera Road",
          "city": "Nairobi",
          "postalCode": "00100",
          "country": "Kenya"
        }
      },
      "employment": {
        "employeeId": "EMP-2024-001",
        "hireDate": "2024-01-15T00:00:00Z",
        "terminationDate": null,
        "type": "Permanent",
        "position": "Heavy Truck Driver",
        "department": "Operations",
        "basicSalary": 45000.0,
        "benefits": ["Medical", "Transport", "Meal Allowance"],
        "supervisorId": "driver-005",
        "schedule": {
          "pattern": "5 days a week",
          "startTime": "06:00:00",
          "endTime": "18:00:00",
          "workDays": ["Monday", "Tuesday", "Wednesday", "Thursday", "Friday"],
          "restDays": ["Saturday", "Sunday"],
          "weeklyHours": 40.0,
          "isFlexible": true,
          "notes": "May work weekends during peak season"
        }
      },
      "transporterId": "trans-001",
      "status": "Active",
      "createdAt": "2024-01-15T10:30:00Z",
      "updatedAt": "2024-01-20T14:45:00Z"
    }
  ],
  "pagination": {
    "page": 1,
    "size": 10,
    "total": 35,
    "totalPages": 4
  }
}
```

#### **GET /api/drivers/{id}**
**Purpose**: Retrieve specific driver details
```json
// Request
GET /api/drivers/driver-001

// Response: Complete driver object with all information
```

#### **POST /api/drivers**
**Purpose**: Create new driver
```json
// Request
POST /api/drivers
{
  "personal": {
    "firstName": "Peter",
    "lastName": "Mwangi",
    "middleName": "Kiprotich",
    "nationalId": "87654321",
    "dateOfBirth": "1990-07-22T00:00:00Z",
    "gender": "Male",
    "maritalStatus": "Single",
    "nationality": "Kenyan",
    "religion": "Christian",
    "residentialAddress": {
      "street": "Kenyatta Avenue",
      "city": "Kisumu",
      "postalCode": "40100",
      "country": "Kenya"
    }
  },
  "contact": {
    "primaryPhone": "+254734567890",
    "email": "peter.mwangi@email.com",
    "whatsAppNumber": "+254734567890",
    "preferredContactMethods": ["Phone", "WhatsApp"]
  },
  "emergency": {
    "name": "Jane Mwangi",
    "relationship": "Sister",
    "phone": "+254745678901",
    "email": "jane.mwangi@email.com",
    "address": {
      "street": "Kenyatta Avenue",
      "city": "Kisumu",
      "postalCode": "40100",
      "country": "Kenya"
    }
  },
  "employment": {
    "employeeId": "EMP-2024-002",
    "hireDate": "2024-02-01T00:00:00Z",
    "type": "Permanent",
    "position": "Truck Driver",
    "department": "Operations",
    "basicSalary": 40000.0,
    "benefits": ["Medical", "Transport"],
    "schedule": {
      "pattern": "5 days a week",
      "startTime": "06:00:00",
      "endTime": "18:00:00",
      "workDays": ["Monday", "Tuesday", "Wednesday", "Thursday", "Friday"],
      "restDays": ["Saturday", "Sunday"],
      "weeklyHours": 40.0,
      "isFlexible": true
    }
  },
  "transporterId": "trans-002",
  "status": "Active"
}

// Response: Created driver object
```

#### **PUT /api/drivers/{id}**
**Purpose**: Update existing driver
```json
// Request: Updated driver object
// Response: Updated driver object
```

#### **DELETE /api/drivers/{id}**
**Purpose**: Soft delete driver (mark as inactive)
```json
// Request
DELETE /api/drivers/driver-001

// Response
{
  "message": "Driver deactivated successfully",
  "driverId": "driver-001"
}
```

### **Compliance Management**

#### **GET /api/drivers/{id}/compliance**
**Purpose**: Retrieve driver compliance records
```json
// Request
GET /api/drivers/driver-001/compliance?type=License&status=Valid

// Response
{
  "data": [
    {
      "id": "comp-001",
      "driverId": "driver-001",
      "type": "License",
      "details": {
        "documentType": "Class CE Heavy Truck License",
        "vehicleCategories": ["Truck", "Trailer"],
        "endorsements": ["Hazmat"],
        "medicalConditions": "None",
        "trainingCourse": "Defensive Driving Course",
        "instructorName": "Peter Mwangi",
        "score": 85.5,
        "grade": "A",
        "competencies": ["Highway driving", "City driving", "Reversing"]
      },
      "issueDate": "2024-01-01T00:00:00Z",
      "expiryDate": "2025-01-01T00:00:00Z",
      "status": "Valid",
      "issuingAuthority": "NTSA",
      "documentNumber": "DL-2024-001",
      "cost": {
        "amount": 5000.0,
        "currency": "KES",
        "paymentDate": "2024-01-01T00:00:00Z",
        "paymentMethod": "Bank Transfer",
        "receiptNumber": "REC-2024-001",
        "status": "Paid"
      },
      "restrictions": ["No night driving after 10 PM"],
      "createdAt": "2024-01-01T10:00:00Z",
      "updatedAt": "2024-01-01T10:00:00Z"
    }
  ]
}
```

#### **POST /api/drivers/{id}/compliance**
**Purpose**: Add new compliance record
```json
// Request
POST /api/drivers/driver-001/compliance
{
  "type": "Medical",
  "details": {
    "documentType": "Medical Fitness Certificate",
    "vehicleCategories": ["Truck"],
    "medicalConditions": "None",
    "competencies": ["Physical fitness", "Mental alertness", "Vision 20/20"]
  },
  "issueDate": "2024-01-15T00:00:00Z",
  "expiryDate": "2025-01-15T00:00:00Z",
  "status": "Valid",
  "issuingAuthority": "Ministry of Health",
  "documentNumber": "MED-2024-001",
  "cost": {
    "amount": 2000.0,
    "currency": "KES",
    "paymentDate": "2024-01-15T00:00:00Z",
    "paymentMethod": "Cash",
    "receiptNumber": "MOH-REC-001",
    "status": "Paid"
  },
  "restrictions": []
}

// Response: Created compliance record
```

### **Performance Management**

#### **GET /api/drivers/{id}/performance**
**Purpose**: Retrieve driver performance metrics
```json
// Request
GET /api/drivers/driver-001/performance?period=2024-01

// Response
{
  "data": [
    {
      "id": "perf-001",
      "driverId": "driver-001",
      "period": "2024-01",
      "metrics": {
        "totalTrips": 45,
        "totalDistance": 12000.0,
        "totalDrivingTime": "180:00:00",
        "averageSpeed": 66.7,
        "fuelEfficiency": 8.5,
        "onTimeDeliveries": 42,
        "onTimePercentage": 93.3,
        "lateDeliveries": 3,
        "averageDelayTime": 1.5
      },
      "safety": {
        "safetyViolations": 0,
        "trafficViolations": 1,
        "accidents": 0,
        "nearMisses": 2,
        "safetyScore": 9.2,
        "safetyIncidents": ["Overspeeding on Highway"],
        "safetyAchievements": ["30 days accident-free"],
        "lastIncidentDate": "2024-01-15T00:00:00Z",
        "lastIncidentType": "Traffic Violation"
      },
      "customer": {
        "averageRating": 4.5,
        "totalRatings": 15,
        "positiveReviews": 12,
        "negativeReviews": 3,
        "commonPraises": ["Punctual", "Professional", "Careful"],
        "commonComplaints": ["Rough handling", "Late arrival"],
        "recommendationScore": 8.7,
        "bestReview": "Excellent driver, very professional",
        "worstReview": "Arrived 30 minutes late"
      },
      "training": {
        "coursesCompleted": ["Defensive Driving", "Customer Service"],
        "certificationsEarned": ["Safe Driver Certificate"],
        "trainingHours": 40.0,
        "lastTrainingDate": "2024-01-20T00:00:00Z",
        "nextTrainingDue": "First Aid Training",
        "nextTrainingDate": "2024-04-15T00:00:00Z",
        "trainingScore": 92.5
      },
      "overallScore": 8.5,
      "rating": "Good",
      "periodStart": "2024-01-01T00:00:00Z",
      "periodEnd": "2024-01-31T23:59:59Z",
      "createdAt": "2024-02-01T10:00:00Z",
      "updatedAt": "2024-02-01T10:00:00Z"
    }
  ]
}
```

### **Assignment Management**

#### **GET /api/drivers/{id}/assignments**
**Purpose**: Retrieve driver assignments
```json
// Request
GET /api/drivers/driver-001/assignments?status=InProgress

// Response
{
  "data": [
    {
      "id": "assign-001",
      "driverId": "driver-001",
      "vehicleId": "vehicle-001",
      "routeId": "route-001",
      "orderId": "order-001",
      "assignedDate": "2024-01-25T06:00:00Z",
      "startDate": "2024-01-25T06:30:00Z",
      "endDate": null,
      "status": "InProgress",
      "details": {
        "origin": "Mombasa Plant",
        "destination": "Nairobi Warehouse",
        "estimatedDistance": 485.5,
        "estimatedDuration": "06:30:00",
        "cargoType": "Cement",
        "loadWeight": 45.0,
        "specialInstructions": ["Handle with care", "Urgent delivery"],
        "requiredDocuments": ["Delivery Note", "Insurance Certificate"],
        "customerContact": "+254712345678",
        "compensationAmount": 8000.0,
        "notes": "First time to this location"
      },
      "requiredSkills": ["Heavy Truck", "Long Distance"],
      "priority": "High",
      "createdAt": "2024-01-24T15:00:00Z",
      "updatedAt": "2024-01-25T06:30:00Z"
    }
  ]
}
```

#### **POST /api/drivers/{id}/assignments**
**Purpose**: Create new driver assignment
```json
// Request
POST /api/drivers/driver-001/assignments
{
  "vehicleId": "vehicle-002",
  "routeId": "route-002",
  "orderId": "order-002",
  "assignedDate": "2024-01-26T06:00:00Z",
  "details": {
    "origin": "Kisumu Plant",
    "destination": "Nairobi Warehouse",
    "estimatedDistance": 350.0,
    "estimatedDuration": "05:00:00",
    "cargoType": "Cement",
    "loadWeight": 40.0,
    "specialInstructions": ["Urgent delivery"],
    "requiredDocuments": ["Delivery Note"],
    "customerContact": "+254723456789",
    "compensationAmount": 7000.0
  },
  "requiredSkills": ["Heavy Truck"],
  "priority": "Medium"
}

// Response: Created assignment record
```

### **Violation Management**

#### **GET /api/drivers/{id}/violations**
**Purpose**: Retrieve driver violation records
```json
// Request
GET /api/drivers/driver-001/violations?type=Traffic&status=Resolved

// Response
{
  "data": [
    {
      "id": "viol-001",
      "driverId": "driver-001",
      "type": "Traffic",
      "description": "Overspeeding on Nairobi-Mombasa Highway",
      "violationDate": "2024-01-15T14:30:00Z",
      "severity": "Major",
      "details": {
        "location": "Nairobi-Mombasa Highway KM 120",
        "officerName": "PC John Mwangi",
        "officerBadge": "12345",
        "ticketNumber": "TKT-2024-001",
        "speed": 95.0,
        "speedLimit": 80.0,
        "vehicleUsed": "KBA 123A",
        "witnesses": ["Assistant driver"],
        "driverExplanation": "Emergency medical delivery"
      },
      "reportedBy": "Traffic Police",
      "status": "Resolved",
      "penalty": {
        "fineAmount": 3000.0,
        "currency": "KES",
        "dueDate": "2024-02-15T00:00:00Z",
        "paymentStatus": "Paid",
        "penaltyType": "Fine",
        "suspensionDays": 0,
        "requiresRetraining": true,
        "retrainingType": "Defensive Driving Course",
        "affectsRating": true,
        "ratingImpact": -0.5
      },
      "createdAt": "2024-01-15T15:00:00Z",
      "updatedAt": "2024-01-20T10:00:00Z"
    }
  ]
}
```

#### **POST /api/drivers/{id}/violations**
**Purpose**: Report new violation
```json
// Request
POST /api/drivers/driver-001/violations
{
  "type": "Policy",
  "description": "Late arrival to pickup point",
  "violationDate": "2024-01-22T08:30:00Z",
  "severity": "Minor",
  "details": {
    "location": "Mombasa Plant Gate",
    "vehicleUsed": "KBA 123A",
    "witnesses": ["Gate security"],
    "driverExplanation": "Traffic jam due to accident"
  },
  "reportedBy": "Supervisor",
  "penalty": {
    "fineAmount": 0.0,
    "currency": "KES",
    "penaltyType": "Warning",
    "suspensionDays": 0,
    "requiresRetraining": false,
    "affectsRating": false,
    "ratingImpact": 0.0
  }
}

// Response: Created violation record
```

### **Business-Specific Endpoints**

#### **GET /api/drivers/availability**
**Purpose**: Check driver availability
```json
// Request
GET /api/drivers/availability?transporter=trans-001&date=2024-01-25&skills=Heavy Truck

// Response
{
  "availableDrivers": [
    {
      "id": "driver-001",
      "name": "John Kamau",
      "skills": ["Heavy Truck", "Long Distance"],
      "rating": 4.5,
      "performanceScore": 8.5,
      "licenseStatus": "Valid",
      "medicalStatus": "Valid",
      "availabilityScore": 95.0,
      "lastAssignment": "2024-01-20T00:00:00Z",
      "preferredRoutes": ["route-001", "route-002"]
    }
  ]
}
```

#### **GET /api/drivers/performance-summary**
**Purpose**: Fleet-wide performance summary
```json
// Request
GET /api/drivers/performance-summary?transporter=trans-001&period=2024-01

// Response
{
  "summary": {
    "totalDrivers": 25,
    "activeDrivers": 23,
    "averageRating": 4.2,
    "averagePerformanceScore": 8.1,
    "topPerformers": [
      {
        "id": "driver-001",
        "name": "John Kamau",
        "score": 9.2,
        "rating": "Excellent"
      }
    ],
    "complianceRate": 96.0,
    "safetyScore": 8.9,
    "trainingCompletionRate": 85.0
  }
}
```

#### **POST /api/drivers/bulk-update**
**Purpose**: Bulk update driver information
```json
// Request
POST /api/drivers/bulk-update
{
  "drivers": [
    {
      "id": "driver-001",
      "status": "Suspended",
      "reason": "License expired"
    },
    {
      "id": "driver-002",
      "status": "Active",
      "reason": "Medical renewed"
    }
  ]
}

// Response
{
  "updated": 2,
  "failed": 0,
  "results": [
    {
      "id": "driver-001",
      "status": "Success",
      "message": "Driver status updated to Suspended"
    },
    {
      "id": "driver-002",
      "status": "Success",
      "message": "Driver status updated to Active"
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
│Transporter Service│◄──────►│ Driver Service  │
│                 │         │                 │
│ • Driver Info   │         │ • Driver Details│
│ • Assignments   │         │ • Availability  │
│ • Performance   │         │ • Compliance    │
└─────────────────┘         └─────────────────┘
```

#### **Vehicle Service Integration** (Future)
```
┌─────────────────┐         ┌─────────────────┐
│ Vehicle Service │◄───────►│ Driver Service  │
│                 │         │                 │
│ • Vehicle Info  │         │ • Driver Info   │
│ • Assignments   │         │ • Assignments   │
│ • Performance   │         │ • Performance   │
└─────────────────┘         └─────────────────┘
```

#### **Route Service Integration** (Future)
```
┌─────────────────┐         ┌─────────────────┐
│ Route Service   │◄───────►│ Driver Service  │
│                 │         │                 │
│ • Route Info    │         │ • Driver Info   │
│ • Schedules     │         │ • Assignments   │
│ • Progress      │         │ • Performance   │
└─────────────────┘         └─────────────────┘
```

### **Integration Patterns**

#### **Driver Assignment Pattern**
```csharp
// Transporter Service -> Driver Service
public async Task<DriverAssignment> AssignDriverAsync(DriverAssignmentRequest request)
{
    var assignment = await _driverService.AssignDriverAsync(new DriverAssignmentRequest
    {
        DriverId = request.DriverId,
        VehicleId = request.VehicleId,
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
// Transporter Service -> Driver Service
public async Task<bool> CheckDriverAvailabilityAsync(string driverId, DateTime date)
{
    var availability = await _driverService.GetDriverAvailabilityAsync(driverId, date);
    return availability.IsAvailable;
}
```

---

## 💼 **Business Use Cases**

### **Use Case 1: Driver Recruitment**

**Scenario**: New driver joins transporter's fleet
**Actors**: HR Manager, Driver, System, NTSA
**Flow**:
1. HR manager creates driver profile with personal information
2. System validates national ID and contact information
3. Driver submits license and medical certificates
4. System creates compliance records and tracks expiry dates
5. Driver completes mandatory training courses
6. System generates employee ID and access credentials
7. Driver becomes available for assignments

**Business Value**: Systematic onboarding ensures compliance and readiness

### **Use Case 2: License Renewal**

**Scenario**: Driver's license is about to expire
**Actors**: Driver, HR Manager, System, NTSA
**Flow**:
1. System sends automated reminder 30 days before expiry
2. Driver initiates renewal process with NTSA
3. HR manager tracks renewal progress
4. Driver submits renewed license to system
5. System updates compliance records
6. Driver's assignment eligibility is maintained
7. Compliance reports are updated

**Business Value**: Prevents license expiry and maintains legal compliance

### **Use Case 3: Performance Review**

**Scenario**: Monthly driver performance evaluation
**Actors**: Supervisor, Driver, System, Customers
**Flow**:
1. System collects performance data from completed trips
2. Customer feedback and ratings are aggregated
3. Safety incidents and violations are reviewed
4. Training completion and certifications are assessed
5. Overall performance score is calculated
6. Supervisor conducts performance review meeting
7. Development plans are created for improvement

**Business Value**: Data-driven performance management and development

### **Use Case 4: Incident Management**

**Scenario**: Driver involved in traffic violation
**Actors**: Traffic Police, Driver, Supervisor, System
**Flow**:
1. Traffic police report violation to system
2. System creates violation record with details
3. Driver provides explanation and evidence
4. Supervisor investigates and determines penalty
5. System tracks fine payment and training requirements
6. Driver's performance rating is updated
7. Incident is resolved and closed

**Business Value**: Systematic incident management and corrective action

---

## 🛣️ **Implementation Roadmap**

### **Phase 1: Foundation** (Month 5, Week 1-2)
- 🔄 **Project Setup**: Create Clean Architecture structure
- 🔄 **Core Entities**: Implement Driver, Compliance, Performance entities
- 🔄 **Database Layer**: Set up Entity Framework with SQLite
- 🔄 **Basic Repository**: Implement repository pattern for data access

### **Phase 2: Core Features** (Month 5, Week 3-4)
- 🔄 **Driver Management**: Complete CRUD operations for drivers
- 🔄 **Compliance Management**: License and certification tracking
- 🔄 **Performance Management**: KPI tracking and rating system
- 🔄 **API Controllers**: REST API endpoints for all operations

### **Phase 3: Integration** (Month 6, Week 1-2)
- 🔄 **Transporter Service Integration**: Driver assignments and availability
- 🔄 **Assignment System**: Driver-vehicle-route assignments
- 🔄 **Violation Management**: Incident tracking and resolution
- 🔄 **API Documentation**: Comprehensive OpenAPI documentation

### **Phase 4: Advanced Features** (Month 6, Week 3-4)
- 🔄 **Performance Analytics**: Advanced performance metrics
- 🔄 **Predictive Analytics**: Performance prediction algorithms
- 🔄 **Automated Compliance**: Automated compliance monitoring
- 🔄 **Monitoring**: Health checks and performance metrics

### **Current Status**: 🔄 **READY FOR IMPLEMENTATION**
- **Priority**: MEDIUM (Fifth priority after Product, Transporter, Route, Vehicle)
- **Dependencies**: Transporter Service, Vehicle Service
- **Estimated Timeline**: 1 month for complete implementation
- **Resource Requirements**: 1 developer, part-time

---

## 📊 **Success Metrics**

### **Technical Metrics**
- **API Response Time**: < 200ms for driver searches
- **Database Performance**: < 100ms for performance queries
- **Service Availability**: 99.9% uptime
- **Integration Success**: 100% assignment accuracy

### **Business Metrics**
- **Driver Pool**: 150+ drivers registered
- **Compliance Rate**: 98% license and medical validity
- **Performance Score**: 8.0+ average driver rating
- **Training Completion**: 90% mandatory training completion

### **Quality Metrics**
- **Safety Score**: 9.0+ average safety rating
- **Customer Satisfaction**: 4.3+ average customer rating
- **Violation Rate**: <5% drivers with violations
- **System Reliability**: 0 compliance failures due to system issues

---

*Driver Service serves as the central driver management hub, enabling comprehensive personnel management, compliance tracking, and performance monitoring across the QaliTrack transportation network.*