# Transporter Service Visual Guide

## 🎯 **Service Overview**

### **Business Purpose**
The Transporter Service manages QaliTrack's transportation network, including both customer-owned transport companies and independent third-party transporters. It handles fleet management, service areas, contracts, and transporter-customer relationships while maintaining clean service boundaries.

### **Key Business Problems Solved**
- **Fleet Management**: Centralized management of transportation resources
- **Service Area Coverage**: Geographic coverage and route optimization
- **Contract Management**: Transporter agreements and service terms
- **Capacity Planning**: Transportation capacity across regions
- **Performance Tracking**: Transporter performance metrics and KPIs
- **Customer-Transporter Matching**: Efficient assignment of transporters to customers

### **Integration Role**
Central transportation authority that manages transporter information, validates transport assignments, and provides transportation services to customer orders and logistics operations.

---

## 🏗️ **Architecture Diagram**

```
┌─────────────────────────────────────────────────────────────────┐
│                      Transporter Service                        │
│                         Port: 7010                              │
└─────────────────────────────────────────────────────────────────┘
┌─────────────────────────────────────────────────────────────────┐
│                          API Layer                              │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐   │
│  │TransportersController│  │  ContractsController│  │  PerformanceController│  │
│  │  - CRUD operations   │  │  - Contract management│  │  - KPI tracking       │  │
│  │  - Search & filter   │  │  - Terms negotiation  │  │  - Performance reports│  │
│  │  │  - Fleet management  │  │  - Renewal processes  │  │  - Rating system     │  │
│  │  - Service areas     │  │  - Billing integration │  │  - Analytics         │  │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘   │
└─────────────────────────────────────────────────────────────────┘
┌─────────────────────────────────────────────────────────────────┐
│                         Core Layer                              │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐   │
│  │TransporterService│  │  ContractService    │  │PerformanceService   │   │
│  │  - Business logic│  │  - Contract logic   │  │  - KPI calculations │   │
│  │  - Fleet mgmt    │  │  - Terms validation │  │  - Performance eval │   │
│  │  - Area coverage │  │  - Renewal alerts   │  │  - Rating algorithms│   │
│  │  - Validation    │  │  - Billing prep     │  │  - Trend analysis   │   │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘   │
│                                                                 │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐   │
│  │   Transporter      │  │TransporterContract │  │PerformanceMetrics  │   │
│  │  - Id, Name        │  │  - Id, TransporterId│  │  - Id, TransporterId│   │
│  │  - Type, Fleet     │  │  - CustomerId       │  │  - DeliveryScore    │   │
│  │  - ServiceAreas    │  │  - Terms, Rates     │  │  - TimelyDelivery   │   │
│  │  - Capacity        │  │  - StartDate, EndDate│  │  - CustomerRating   │   │
│  │  - IsActive        │  │  - IsActive         │  │  - Period           │   │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘   │
└─────────────────────────────────────────────────────────────────┘
┌─────────────────────────────────────────────────────────────────┐
│                    Infrastructure Layer                         │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐   │
│  │TransporterRepository│  │ContractRepository │  │PerformanceRepository │   │
│  │  - CRUD operations │  │  - CRUD operations │  │  - CRUD operations  │   │
│  │  - Search queries  │  │  - Contract queries │  │  - Metrics queries  │   │
│  │  - Fleet queries   │  │  - Renewal tracking │  │  - KPI calculations │   │
│  │  - Area filtering  │  │  - Customer links   │  │  - Trend analysis   │   │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘   │
│                                                                 │
│  ┌─────────────────────────────────────────────────────────────┐  │
│  │                TransporterDbContext                          │  │
│  │  - Transporters, Contracts, Performance Tables              │  │
│  │  - Entity configurations and relationships                  │  │
│  │  - Database migrations and seeding                          │  │
│  └─────────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
```

---

## 📊 **Entity Model**

### **Core Entities**

#### **Transporter Entity**
```csharp
public class Transporter : BaseEntity
{
    public string Name { get; set; }                    // "Mombasa Transport Co."
    public string Code { get; set; }                    // "MTC-001"
    public string RegistrationNumber { get; set; }      // "TR-2024-001"
    public TransporterType Type { get; set; }           // CustomerOwned, Independent
    public string? OwnerCustomerId { get; set; }        // References Customer Service if customer-owned
    public CompanyDetails Company { get; set; }
    public ContactInformation Contact { get; set; }
    public FleetCapacity Fleet { get; set; }
    public List<ServiceArea> ServiceAreas { get; set; }
    public TransporterStatus Status { get; set; }       // Active, Inactive, Suspended
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedBy { get; set; }
    public string UpdatedBy { get; set; }
}

public enum TransporterType
{
    CustomerOwned,      // Customer owns the transport company
    Independent,        // Third-party transporter
    Partner,           // Strategic partner
    Contracted         // Long-term contracted transporter
}

public class CompanyDetails
{
    public string LegalName { get; set; }               // "Mombasa Transport Company Ltd"
    public string TradingName { get; set; }             // "MTC Logistics"
    public string TaxNumber { get; set; }               // "KRA-PIN-A123456789"
    public string LicenseNumber { get; set; }           // "NTSA-LICENSE-789"
    public Address RegisteredAddress { get; set; }
    public Address OperatingAddress { get; set; }
}

public class ContactInformation
{
    public string PrimaryContactName { get; set; }      // "John Kamau"
    public string PrimaryContactPhone { get; set; }     // "+254712345678"
    public string PrimaryContactEmail { get; set; }     // "john@mtc.co.ke"
    public string SecondaryContactName { get; set; }    // "Mary Wanjiku"
    public string SecondaryContactPhone { get; set; }   // "+254723456789"
    public string SecondaryContactEmail { get; set; }   // "mary@mtc.co.ke"
    public string EmergencyContact { get; set; }        // "+254734567890"
}

public class FleetCapacity
{
    public int TotalVehicles { get; set; }              // 25
    public int AvailableVehicles { get; set; }          // 20
    public decimal TotalCapacityTons { get; set; }      // 500.0
    public decimal AvailableCapacityTons { get; set; }  // 400.0
    public VehicleTypeBreakdown VehicleTypes { get; set; }
    public DateTime LastUpdated { get; set; }
}

public class VehicleTypeBreakdown
{
    public int Trucks { get; set; }                     // 15
    public int Trailers { get; set; }                   // 8
    public int Tankers { get; set; }                    // 2
    public int SpecialVehicles { get; set; }            // 0
}
```

#### **ServiceArea Entity**
```csharp
public class ServiceArea : BaseEntity
{
    public string TransporterId { get; set; }          // References Transporter
    public string Region { get; set; }                 // "Nairobi", "Mombasa", "Kisumu"
    public string County { get; set; }                 // "Nairobi County"
    public List<string> Cities { get; set; }           // ["Nairobi", "Kiambu", "Machakos"]
    public GeographicBounds Bounds { get; set; }
    public ServiceLevel ServiceLevel { get; set; }     // Primary, Secondary, OnDemand
    public decimal CoverageRadius { get; set; }        // 50.0 (km)
    public bool IsActive { get; set; }                 // Service area availability
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Navigation properties
    public Transporter Transporter { get; set; }
}

public class GeographicBounds
{
    public decimal NorthLatitude { get; set; }         // -1.0
    public decimal SouthLatitude { get; set; }         // -1.5
    public decimal EastLongitude { get; set; }         // 37.0
    public decimal WestLongitude { get; set; }         // 36.5
}

public enum ServiceLevel
{
    Primary,        // Main service area with guaranteed coverage
    Secondary,      // Secondary area with limited coverage
    OnDemand,      // Available on request only
    Seasonal       // Seasonal coverage (e.g., harvest season)
}
```

#### **TransporterContract Entity**
```csharp
public class TransporterContract : BaseEntity
{
    public string TransporterId { get; set; }          // References Transporter
    public string? CustomerId { get; set; }            // References Customer Service (if customer-specific)
    public string ContractNumber { get; set; }         // "TC-2024-001"
    public ContractType Type { get; set; }             // Master, Customer, Spot
    public ContractTerms Terms { get; set; }
    public PricingStructure Pricing { get; set; }
    public DateTime StartDate { get; set; }            // 2024-01-01
    public DateTime EndDate { get; set; }              // 2024-12-31
    public ContractStatus Status { get; set; }         // Draft, Active, Expired, Terminated
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedBy { get; set; }
    public string UpdatedBy { get; set; }
    
    // Navigation properties
    public Transporter Transporter { get; set; }
}

public enum ContractType
{
    Master,         // Master agreement with transporter
    Customer,       // Customer-specific contract
    Spot,          // Spot rate agreement
    Framework      // Framework agreement
}

public class ContractTerms
{
    public string PaymentTerms { get; set; }           // "Net 30 days"
    public string DeliveryTerms { get; set; }          // "FOB Destination"
    public string QualityStandards { get; set; }       // "ISO 9001:2015"
    public string SafetyRequirements { get; set; }     // "OHSAS 18001"
    public string InsuranceRequirements { get; set; }  // "Min 10M KES coverage"
    public string PerformanceKPIs { get; set; }        // "95% on-time delivery"
    public string PenaltyClause { get; set; }          // "2% of invoice for delays"
    public string TerminationClause { get; set; }      // "30 days notice"
}

public class PricingStructure
{
    public decimal BaseRate { get; set; }              // 50.0 (per ton)
    public string Currency { get; set; }               // "KES"
    public PricingModel Model { get; set; }            // PerTon, PerTrip, PerKm
    public decimal MinimumCharge { get; set; }         // 500.0
    public decimal MaximumCharge { get; set; }         // 10000.0
    public List<RateModifier> Modifiers { get; set; }
    public DateTime LastUpdated { get; set; }
}

public enum PricingModel
{
    PerTon,         // Rate per ton of cargo
    PerTrip,        // Fixed rate per trip
    PerKm,          // Rate per kilometer
    PerHour,        // Rate per hour
    Combined        // Combination of above
}

public class RateModifier
{
    public string Type { get; set; }                   // "Distance", "Urgency", "Fuel"
    public decimal Factor { get; set; }                // 1.2 (20% increase)
    public string Condition { get; set; }              // "Distance > 100km"
    public bool IsActive { get; set; }
}
```

#### **PerformanceMetrics Entity**
```csharp
public class PerformanceMetrics : BaseEntity
{
    public string TransporterId { get; set; }          // References Transporter
    public string Period { get; set; }                 // "2024-01" (Monthly)
    public DeliveryMetrics Delivery { get; set; }
    public QualityMetrics Quality { get; set; }
    public FinancialMetrics Financial { get; set; }
    public CustomerSatisfaction Customer { get; set; }
    public decimal OverallScore { get; set; }           // 8.5 (out of 10)
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Navigation properties
    public Transporter Transporter { get; set; }
}

public class DeliveryMetrics
{
    public int TotalDeliveries { get; set; }           // 150
    public int OnTimeDeliveries { get; set; }          // 142
    public decimal OnTimePercentage { get; set; }      // 94.7%
    public decimal AverageDeliveryTime { get; set; }   // 2.5 hours
    public int DelayedDeliveries { get; set; }         // 8
    public decimal AverageDelayTime { get; set; }      // 1.2 hours
}

public class QualityMetrics
{
    public int QualityIssues { get; set; }             // 3
    public int DamageReports { get; set; }             // 1
    public int SafetyIncidents { get; set; }           // 0
    public decimal QualityScore { get; set; }          // 9.2 (out of 10)
    public int ComplianceViolations { get; set; }      // 0
}

public class FinancialMetrics
{
    public decimal TotalRevenue { get; set; }          // 750000.0
    public decimal AverageRate { get; set; }           // 5000.0
    public decimal PaymentTimeliness { get; set; }     // 96.5%
    public int InvoiceDisputes { get; set; }           // 2
    public decimal CollectionEfficiency { get; set; }  // 98.5%
}

public class CustomerSatisfaction
{
    public decimal AverageRating { get; set; }         // 4.2 (out of 5)
    public int TotalRatings { get; set; }              // 45
    public int PositiveReviews { get; set; }           // 38
    public int NegativeReviews { get; set; }           // 7
    public decimal RecommendationScore { get; set; }   // 8.1 (out of 10)
}
```

### **Entity Relationships**

```
┌─────────────────┐         ┌─────────────────┐         ┌─────────────────┐
│   Transporter   │◄────────┤   ServiceArea   │         │TransporterContract│
│                 │         │                 │         │                 │
│ • Id            │         │ • TransporterId │         │ • TransporterId │
│ • Name          │         │ • Region        │         │ • CustomerId    │
│ • Type          │         │ • County        │         │ • Terms         │
│ • Fleet         │         │ • Cities        │         │ • Pricing       │
│ • ServiceAreas  │         │ • Bounds        │         │ • StartDate     │
│ • Status        │         │ • ServiceLevel  │         │ • EndDate       │
└─────────────────┘         └─────────────────┘         └─────────────────┘
         │                                                        │
         │                                                        │
         └────────────────────────────────────────────────────────┘
                                   │
                                   ▼
                         ┌─────────────────┐
                         │PerformanceMetrics│
                         │                 │
                         │ • TransporterId │
                         │ • Period        │
                         │ • Delivery      │
                         │ • Quality       │
                         │ • Financial     │
                         │ • Customer      │
                         │ • OverallScore  │
                         └─────────────────┘
```

---

## 🔌 **API Endpoints**

### **Transporter Management**

#### **GET /api/transporters**
**Purpose**: Retrieve paginated transporter list with filtering
```json
// Request
GET /api/transporters?page=1&size=10&type=Independent&region=Nairobi&status=Active

// Response
{
  "data": [
    {
      "id": "trans-001",
      "name": "Mombasa Transport Co.",
      "code": "MTC-001",
      "registrationNumber": "TR-2024-001",
      "type": "Independent",
      "ownerCustomerId": null,
      "company": {
        "legalName": "Mombasa Transport Company Ltd",
        "tradingName": "MTC Logistics",
        "taxNumber": "KRA-PIN-A123456789",
        "licenseNumber": "NTSA-LICENSE-789",
        "registeredAddress": {
          "street": "Moi Avenue",
          "city": "Mombasa",
          "postalCode": "80100",
          "country": "Kenya"
        }
      },
      "contact": {
        "primaryContactName": "John Kamau",
        "primaryContactPhone": "+254712345678",
        "primaryContactEmail": "john@mtc.co.ke"
      },
      "fleet": {
        "totalVehicles": 25,
        "availableVehicles": 20,
        "totalCapacityTons": 500.0,
        "availableCapacityTons": 400.0,
        "vehicleTypes": {
          "trucks": 15,
          "trailers": 8,
          "tankers": 2,
          "specialVehicles": 0
        }
      },
      "serviceAreas": [
        {
          "region": "Nairobi",
          "county": "Nairobi County",
          "cities": ["Nairobi", "Kiambu", "Machakos"],
          "serviceLevel": "Primary",
          "coverageRadius": 50.0,
          "isActive": true
        }
      ],
      "status": "Active",
      "createdAt": "2024-01-15T10:30:00Z",
      "updatedAt": "2024-01-20T14:45:00Z"
    }
  ],
  "pagination": {
    "page": 1,
    "size": 10,
    "total": 12,
    "totalPages": 2
  }
}
```

#### **GET /api/transporters/{id}**
**Purpose**: Retrieve specific transporter details
```json
// Request
GET /api/transporters/trans-001

// Response: Complete transporter object with all details
```

#### **POST /api/transporters**
**Purpose**: Create new transporter
```json
// Request
POST /api/transporters
{
  "name": "Nairobi Express Transport",
  "code": "NET-001",
  "registrationNumber": "TR-2024-005",
  "type": "Independent",
  "ownerCustomerId": null,
  "company": {
    "legalName": "Nairobi Express Transport Ltd",
    "tradingName": "NET Logistics",
    "taxNumber": "KRA-PIN-B987654321",
    "licenseNumber": "NTSA-LICENSE-456",
    "registeredAddress": {
      "street": "Kenyatta Avenue",
      "city": "Nairobi",
      "postalCode": "00100",
      "country": "Kenya"
    }
  },
  "contact": {
    "primaryContactName": "Peter Mwangi",
    "primaryContactPhone": "+254722345678",
    "primaryContactEmail": "peter@net.co.ke"
  },
  "fleet": {
    "totalVehicles": 15,
    "availableVehicles": 12,
    "totalCapacityTons": 300.0,
    "availableCapacityTons": 240.0,
    "vehicleTypes": {
      "trucks": 10,
      "trailers": 5,
      "tankers": 0,
      "specialVehicles": 0
    }
  },
  "status": "Active"
}

// Response: Created transporter object
```

#### **PUT /api/transporters/{id}**
**Purpose**: Update existing transporter
```json
// Request: Updated transporter object
// Response: Updated transporter object
```

#### **DELETE /api/transporters/{id}**
**Purpose**: Soft delete transporter (mark as inactive)
```json
// Request
DELETE /api/transporters/trans-001

// Response
{
  "message": "Transporter deactivated successfully",
  "transporterId": "trans-001"
}
```

### **Service Area Management**

#### **GET /api/transporters/{id}/service-areas**
**Purpose**: Retrieve transporter service areas
```json
// Request
GET /api/transporters/trans-001/service-areas

// Response
{
  "data": [
    {
      "id": "sa-001",
      "transporterId": "trans-001",
      "region": "Nairobi",
      "county": "Nairobi County",
      "cities": ["Nairobi", "Kiambu", "Machakos"],
      "bounds": {
        "northLatitude": -1.0,
        "southLatitude": -1.5,
        "eastLongitude": 37.0,
        "westLongitude": 36.5
      },
      "serviceLevel": "Primary",
      "coverageRadius": 50.0,
      "isActive": true,
      "createdAt": "2024-01-15T10:30:00Z",
      "updatedAt": "2024-01-20T14:45:00Z"
    }
  ]
}
```

#### **POST /api/transporters/{id}/service-areas**
**Purpose**: Add new service area for transporter
```json
// Request
POST /api/transporters/trans-001/service-areas
{
  "region": "Kisumu",
  "county": "Kisumu County",
  "cities": ["Kisumu", "Kakamega", "Busia"],
  "bounds": {
    "northLatitude": 0.0,
    "southLatitude": -0.5,
    "eastLongitude": 34.5,
    "westLongitude": 34.0
  },
  "serviceLevel": "Secondary",
  "coverageRadius": 40.0,
  "isActive": true
}

// Response: Created service area object
```

### **Contract Management**

#### **GET /api/transporters/{id}/contracts**
**Purpose**: Retrieve transporter contracts
```json
// Request
GET /api/transporters/trans-001/contracts?status=Active

// Response
{
  "data": [
    {
      "id": "contract-001",
      "transporterId": "trans-001",
      "customerId": "cust-001",
      "contractNumber": "TC-2024-001",
      "type": "Master",
      "terms": {
        "paymentTerms": "Net 30 days",
        "deliveryTerms": "FOB Destination",
        "qualityStandards": "ISO 9001:2015",
        "safetyRequirements": "OHSAS 18001",
        "insuranceRequirements": "Min 10M KES coverage",
        "performanceKPIs": "95% on-time delivery",
        "penaltyClause": "2% of invoice for delays",
        "terminationClause": "30 days notice"
      },
      "pricing": {
        "baseRate": 50.0,
        "currency": "KES",
        "model": "PerTon",
        "minimumCharge": 500.0,
        "maximumCharge": 10000.0,
        "modifiers": [
          {
            "type": "Distance",
            "factor": 1.2,
            "condition": "Distance > 100km",
            "isActive": true
          }
        ]
      },
      "startDate": "2024-01-01T00:00:00Z",
      "endDate": "2024-12-31T23:59:59Z",
      "status": "Active",
      "createdAt": "2024-01-15T10:30:00Z",
      "updatedAt": "2024-01-20T14:45:00Z"
    }
  ]
}
```

#### **POST /api/transporters/{id}/contracts**
**Purpose**: Create new contract for transporter
```json
// Request
POST /api/transporters/trans-001/contracts
{
  "customerId": "cust-002",
  "contractNumber": "TC-2024-002",
  "type": "Customer",
  "terms": {
    "paymentTerms": "Net 15 days",
    "deliveryTerms": "FOB Origin",
    "qualityStandards": "ISO 9001:2015",
    "safetyRequirements": "OHSAS 18001",
    "insuranceRequirements": "Min 5M KES coverage",
    "performanceKPIs": "90% on-time delivery",
    "penaltyClause": "1% of invoice for delays",
    "terminationClause": "15 days notice"
  },
  "pricing": {
    "baseRate": 45.0,
    "currency": "KES",
    "model": "PerTon",
    "minimumCharge": 450.0,
    "maximumCharge": 9000.0,
    "modifiers": []
  },
  "startDate": "2024-02-01T00:00:00Z",
  "endDate": "2024-12-31T23:59:59Z",
  "status": "Active"
}

// Response: Created contract object
```

### **Performance Management**

#### **GET /api/transporters/{id}/performance**
**Purpose**: Retrieve transporter performance metrics
```json
// Request
GET /api/transporters/trans-001/performance?period=2024-01

// Response
{
  "data": [
    {
      "id": "perf-001",
      "transporterId": "trans-001",
      "period": "2024-01",
      "delivery": {
        "totalDeliveries": 150,
        "onTimeDeliveries": 142,
        "onTimePercentage": 94.7,
        "averageDeliveryTime": 2.5,
        "delayedDeliveries": 8,
        "averageDelayTime": 1.2
      },
      "quality": {
        "qualityIssues": 3,
        "damageReports": 1,
        "safetyIncidents": 0,
        "qualityScore": 9.2,
        "complianceViolations": 0
      },
      "financial": {
        "totalRevenue": 750000.0,
        "averageRate": 5000.0,
        "paymentTimeliness": 96.5,
        "invoiceDisputes": 2,
        "collectionEfficiency": 98.5
      },
      "customer": {
        "averageRating": 4.2,
        "totalRatings": 45,
        "positiveReviews": 38,
        "negativeReviews": 7,
        "recommendationScore": 8.1
      },
      "overallScore": 8.5,
      "periodStart": "2024-01-01T00:00:00Z",
      "periodEnd": "2024-01-31T23:59:59Z",
      "createdAt": "2024-02-01T10:30:00Z",
      "updatedAt": "2024-02-01T10:30:00Z"
    }
  ]
}
```

### **Business-Specific Endpoints**

#### **GET /api/transporters/search**
**Purpose**: Search transporters by criteria
```json
// Request
GET /api/transporters/search?region=Nairobi&capacity=100&type=Independent&available=true

// Response
{
  "data": [
    {
      "id": "trans-001",
      "name": "Mombasa Transport Co.",
      "type": "Independent",
      "fleet": {
        "availableVehicles": 20,
        "availableCapacityTons": 400.0
      },
      "serviceAreas": [
        {
          "region": "Nairobi",
          "serviceLevel": "Primary",
          "coverageRadius": 50.0
        }
      ],
      "overallScore": 8.5,
      "matchScore": 95.2
    }
  ]
}
```

#### **POST /api/transporters/assign**
**Purpose**: Assign transporter to customer order
```json
// Request
POST /api/transporters/assign
{
  "customerId": "cust-001",
  "orderId": "order-001",
  "origin": "Mombasa Plant",
  "destination": "Nairobi Warehouse",
  "requiredCapacity": 50.0,
  "preferredTransporterId": "trans-001"
}

// Response
{
  "assignment": {
    "transporterId": "trans-001",
    "transporterName": "Mombasa Transport Co.",
    "estimatedCost": 2500.0,
    "estimatedTime": "4 hours",
    "contractId": "contract-001",
    "assignmentId": "assign-001"
  },
  "availability": {
    "vehicleId": "vehicle-001",
    "driverId": "driver-001",
    "estimatedPickup": "2024-01-25T08:00:00Z",
    "estimatedDelivery": "2024-01-25T12:00:00Z"
  }
}
```

#### **GET /api/transporters/capacity**
**Purpose**: Check available capacity across regions
```json
// Request
GET /api/transporters/capacity?region=Nairobi&date=2024-01-25

// Response
{
  "region": "Nairobi",
  "date": "2024-01-25",
  "capacity": {
    "totalCapacityTons": 1200.0,
    "availableCapacityTons": 800.0,
    "utilization": 33.3,
    "transporterCount": 5,
    "availableTransporters": 3
  },
  "transporters": [
    {
      "id": "trans-001",
      "name": "Mombasa Transport Co.",
      "availableCapacityTons": 400.0,
      "availableVehicles": 20
    }
  ]
}
```

---

## 🔗 **Integration Points**

### **Service Dependencies**

#### **Customer Service Integration**
```
┌─────────────────┐         ┌─────────────────┐
│ Customer Service│◄───────►│Transporter Service│
│                 │         │                 │
│ • TransporterId │         │ • Customer Info │
│ • Orders        │         │ • Assignments   │
│ • Preferences   │         │ • Availability  │
└─────────────────┘         └─────────────────┘
```

**Integration Flow:**
1. Customer Service stores TransporterId reference in Customer and Order entities
2. Transporter Service validates transporter availability and capacity
3. Customer Service uses TransporterId for order assignment
4. Transporter Service tracks performance and generates reports

#### **Vehicle Service Integration** (Future)
```
┌─────────────────┐         ┌─────────────────┐
│Transporter Service│◄───────►│ Vehicle Service │
│                 │         │                 │
│ • Fleet Info    │         │ • Vehicle Details│
│ • Capacity      │         │ • Maintenance   │
│ • Assignments   │         │ • Availability  │
└─────────────────┘         └─────────────────┘
```

#### **Driver Service Integration** (Future)
```
┌─────────────────┐         ┌─────────────────┐
│Transporter Service│◄───────►│ Driver Service  │
│                 │         │                 │
│ • Driver Info   │         │ • Licenses      │
│ • Assignments   │         │ • Availability  │
│ • Performance   │         │ • Violations    │
└─────────────────┘         └─────────────────┘
```

### **Integration Patterns**

#### **Transporter Assignment Pattern**
```csharp
// Customer Service -> Transporter Service
public async Task<TransporterAssignment> AssignTransporterAsync(AssignmentRequest request)
{
    var assignment = await _transporterService.AssignTransporterAsync(new TransporterAssignmentRequest
    {
        CustomerId = request.CustomerId,
        OrderId = request.OrderId,
        Origin = request.Origin,
        Destination = request.Destination,
        RequiredCapacity = request.RequiredCapacity,
        PreferredTransporterId = request.PreferredTransporterId
    });
    
    return assignment;
}
```

#### **Capacity Check Pattern**
```csharp
// Customer Service -> Transporter Service
public async Task<bool> CheckCapacityAsync(string region, DateTime date, decimal requiredCapacity)
{
    var capacity = await _transporterService.GetAvailableCapacityAsync(region, date);
    return capacity.AvailableCapacityTons >= requiredCapacity;
}
```

---

## 💼 **Business Use Cases**

### **Use Case 1: Transporter Registration**

**Scenario**: New independent transporter wants to join QaliTrack network
**Actors**: Transporter Owner, Operations Manager, System
**Flow**:
1. Transporter owner submits registration application
2. Operations manager reviews company details and documentation
3. System validates license numbers and tax registration
4. Fleet capacity and service areas are configured
5. Master contract is created with standard terms
6. Transporter becomes available for order assignments

**Business Value**: Expands transportation network and capacity

### **Use Case 2: Customer-Owned Transporter Setup**

**Scenario**: Existing customer wants to register their transport division
**Actors**: Customer, Account Manager, System
**Flow**:
1. Customer identifies their transport division as separate entity
2. Account manager creates transporter record with customer ownership link
3. System sets up service areas based on customer's operations
4. Customer-specific contract terms are negotiated
5. Fleet information is configured
6. Transporter becomes available for customer's orders

**Business Value**: Leverages customer assets and reduces transportation costs

### **Use Case 3: Order Assignment**

**Scenario**: Customer places order requiring transportation
**Actors**: Customer, System, Transporter
**Flow**:
1. Customer places order with specific origin and destination
2. System calculates required transportation capacity
3. System searches for available transporters in service area
4. System considers customer preferences and transporter performance
5. Optimal transporter is assigned based on criteria
6. Transporter receives assignment notification
7. Transportation is scheduled and executed

**Business Value**: Optimizes transportation efficiency and customer satisfaction

### **Use Case 4: Performance Monitoring**

**Scenario**: Monthly performance review of transporters
**Actors**: Operations Manager, System, Transporter
**Flow**:
1. System collects delivery data from completed orders
2. Customer feedback and ratings are aggregated
3. Financial performance metrics are calculated
4. Quality incidents and safety records are reviewed
5. Overall performance score is computed
6. Performance reports are generated
7. Improvement plans are developed for underperforming transporters

**Business Value**: Maintains service quality and drives continuous improvement

---

## 🛣️ **Implementation Roadmap**

### **Phase 1: Foundation** (Month 2, Week 1-2)
- 🔄 **Project Setup**: Create Clean Architecture structure
- 🔄 **Core Entities**: Implement Transporter, ServiceArea, Contract entities
- 🔄 **Database Layer**: Set up Entity Framework with SQLite
- 🔄 **Basic Repository**: Implement repository pattern for data access

### **Phase 2: Core Features** (Month 2, Week 3-4)
- 🔄 **Transporter Management**: Complete CRUD operations for transporters
- 🔄 **Service Area Management**: Geographic service area configuration
- 🔄 **Contract Management**: Contract creation and terms management
- 🔄 **API Controllers**: REST API endpoints for all operations

### **Phase 3: Integration** (Month 3, Week 1-2)
- 🔄 **Customer Service Integration**: Transporter assignment and validation
- 🔄 **Search & Filtering**: Advanced transporter search capabilities
- 🔄 **Capacity Management**: Real-time capacity tracking
- 🔄 **API Documentation**: Comprehensive OpenAPI documentation

### **Phase 4: Advanced Features** (Month 3, Week 3-4)
- 🔄 **Performance Tracking**: KPI monitoring and reporting
- 🔄 **Assignment Optimization**: Smart transporter assignment algorithms
- 🔄 **Contract Automation**: Automated contract renewals and alerts
- 🔄 **Monitoring**: Health checks and performance metrics

### **Current Status**: 🔄 **READY FOR IMPLEMENTATION**
- **Priority**: HIGH (Second priority after Product Service)
- **Dependencies**: Customer Service (for integration)
- **Estimated Timeline**: 1 month for complete implementation
- **Resource Requirements**: 1 developer, part-time

---

## 📊 **Success Metrics**

### **Technical Metrics**
- **API Response Time**: < 200ms for transporter searches
- **Database Performance**: < 100ms for complex geographic queries
- **Service Availability**: 99.9% uptime
- **Integration Success**: 100% assignment success rate

### **Business Metrics**
- **Transporter Network**: 20+ transporters registered
- **Geographic Coverage**: 5+ regions covered
- **Fleet Capacity**: 500+ vehicles tracked
- **Active Contracts**: 50+ active contracts

### **Quality Metrics**
- **Assignment Accuracy**: 95% optimal assignments
- **Performance Tracking**: 100% performance data capture
- **Customer Satisfaction**: 4.0+ average transporter rating
- **System Reliability**: 0 assignment failures

---

*Transporter Service serves as the central transportation management hub, enabling efficient fleet management, optimal transporter assignments, and comprehensive performance tracking across the QaliTrack logistics network.*