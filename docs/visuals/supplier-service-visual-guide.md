# Supplier Service Visual Guide

## 🎯 **Service Overview**

### **Business Purpose**
The Supplier Service manages QaliTrack's supplier relationships, including supplier registration, contract management, procurement processes, performance monitoring, and compliance tracking. It provides centralized supplier information for procurement operations and vendor management.

### **Key Business Problems Solved**
- **Supplier Registration**: Centralized supplier onboarding and documentation
- **Contract Management**: Supplier agreements and procurement terms
- **Performance Monitoring**: Supplier performance metrics and KPI tracking
- **Procurement Management**: Purchase orders and delivery tracking
- **Compliance Management**: Regulatory compliance and certification tracking
- **Vendor Relationship Management**: Strategic supplier partnerships

### **Integration Role**
Central supplier registry that provides supplier information for procurement operations, validates supplier capabilities, and tracks supplier performance across the supply chain network.

---

## 🏗️ **Architecture Diagram**

```
┌─────────────────────────────────────────────────────────────────┐
│                       Supplier Service                          │
│                         Port: 7009                              │
└─────────────────────────────────────────────────────────────────┘
┌─────────────────────────────────────────────────────────────────┐
│                          API Layer                              │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐   │
│  │ SuppliersController    │  │ ContractsController  │  │PerformanceController │  │
│  │  - CRUD operations     │  │  - Contract mgmt     │  │  - KPI tracking      │  │
│  │  - Registration mgmt   │  │  - Terms negotiation │  │  - Rating system     │  │
│  │  - Category mgmt       │  │  - Renewal processes │  │  - Performance reports│  │
│  │  - Capability tracking │  │  - Compliance tracking│  │  - Improvement plans │  │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘   │
└─────────────────────────────────────────────────────────────────┘
┌─────────────────────────────────────────────────────────────────┐
│                         Core Layer                              │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐   │
│  │ SupplierService    │  │ ContractService     │  │PerformanceService   │   │
│  │  - Supplier logic  │  │  - Contract logic   │  │  - KPI calculation  │   │
│  │  - Registration    │  │  - Terms validation │  │  - Performance eval │   │
│  │  - Capability mgmt │  │  - Renewal mgmt     │  │  - Rating algorithms│   │
│  │  - Validation      │  │  - Compliance check │  │  - Trend analysis   │   │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘   │
│                                                                 │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐   │
│  │    Supplier        │  │ SupplierContract    │  │ SupplierPerformance │   │
│  │  - Id, Name        │  │  - Id, SupplierId   │  │  - Id, SupplierId   │   │
│  │  - Type, Category  │  │  - Terms, Rates     │  │  - Period, Score    │   │
│  │  - Capabilities    │  │  - StartDate, EndDate│  │  - Quality, Delivery│   │
│  │  - Contact         │  │  - Status           │  │  - Cost, Rating     │   │
│  │  - Status          │  │  - Renewals         │  │  - Trends           │   │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘   │
└─────────────────────────────────────────────────────────────────┘
┌─────────────────────────────────────────────────────────────────┐
│                    Infrastructure Layer                         │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐   │
│  │ SupplierRepository │  │ ContractRepository  │  │PerformanceRepository│   │
│  │  - CRUD operations │  │  - CRUD operations  │  │  - CRUD operations  │   │
│  │  - Search queries  │  │  - Contract queries │  │  - KPI queries      │   │
│  │  - Category queries│  │  - Renewal tracking │  │  - Performance qry  │   │
│  │  - Capability qry  │  │  - Compliance qry   │  │  - Trend analysis   │   │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘   │
│                                                                 │
│  ┌─────────────────────────────────────────────────────────────┐  │
│  │                  SupplierDbContext                           │  │
│  │  - Suppliers, Contracts, Performance, Procurement Tables    │  │
│  │  - Entity configurations and relationships                  │  │
│  │  - Database migrations and seeding                          │  │
│  └─────────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
```

---

## 📊 **Entity Model**

### **Core Entities**

#### **Supplier Entity**
```csharp
public class Supplier : BaseEntity
{
    public string Name { get; set; }                    // "Bamburi Cement Ltd"
    public string Code { get; set; }                    // "SUP-BAM-001"
    public string RegistrationNumber { get; set; }      // "CR-2010-001"
    public CompanyInformation Company { get; set; }
    public ContactInformation Contact { get; set; }
    public SupplierCapabilities Capabilities { get; set; }
    public SupplierType Type { get; set; }              // RawMaterial, Finished, Service, Equipment
    public List<string> Categories { get; set; }        // ["Cement", "Aggregates", "Additives"]
    public SupplierStatus Status { get; set; }          // Active, Inactive, Suspended, Blacklisted
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedBy { get; set; }
    public string UpdatedBy { get; set; }
}

public class CompanyInformation
{
    public string LegalName { get; set; }               // "Bamburi Cement Limited"
    public string TradingName { get; set; }             // "Bamburi"
    public string TaxNumber { get; set; }               // "KRA-PIN-P123456789A"
    public string VATNumber { get; set; }               // "VAT-001234567"
    public string BusinessLicense { get; set; }         // "BL-2020-001"
    public CompanySize Size { get; set; }               // Small, Medium, Large, Enterprise
    public DateTime EstablishedDate { get; set; }       // 1951-01-01
    public Address RegisteredAddress { get; set; }
    public Address OperatingAddress { get; set; }
    public List<Address> BranchAddresses { get; set; }
    public string Website { get; set; }                 // "https://www.bamburi.co.ke"
    public FinancialInformation Financial { get; set; }
}

public enum CompanySize
{
    Micro,              // < 10 employees
    Small,              // 10-49 employees
    Medium,             // 50-249 employees
    Large,              // 250-999 employees
    Enterprise         // 1000+ employees
}

public class FinancialInformation
{
    public decimal AnnualRevenue { get; set; }          // 15000000000.0 (KES)
    public string Currency { get; set; }                // "KES"
    public string CreditRating { get; set; }            // "AAA", "AA+", "A", etc.
    public decimal CreditLimit { get; set; }            // 50000000.0 (KES)
    public int PaymentTermsDays { get; set; }           // 30
    public string PreferredPaymentMethod { get; set; }  // "Bank Transfer"
    public string BankName { get; set; }                // "KCB Bank"
    public string BankAccountNumber { get; set; }       // "1234567890"
    public string BankBranch { get; set; }              // "Nairobi Branch"
    public string SwiftCode { get; set; }               // "KCBLKENX"
}

public class ContactInformation
{
    public string PrimaryContactName { get; set; }      // "John Mwangi"
    public string PrimaryContactTitle { get; set; }     // "Sales Manager"
    public string PrimaryContactPhone { get; set; }     // "+254712345678"
    public string PrimaryContactEmail { get; set; }     // "john.mwangi@bamburi.co.ke"
    public string SecondaryContactName { get; set; }    // "Mary Wanjiku"
    public string SecondaryContactTitle { get; set; }   // "Account Manager"
    public string SecondaryContactPhone { get; set; }   // "+254723456789"
    public string SecondaryContactEmail { get; set; }   // "mary.wanjiku@bamburi.co.ke"
    public string CustomerServicePhone { get; set; }    // "+254700123456"
    public string CustomerServiceEmail { get; set; }    // "support@bamburi.co.ke"
    public string EmergencyContact { get; set; }        // "+254711987654"
    public List<string> PreferredCommunication { get; set; } // ["Email", "Phone", "WhatsApp"]
}

public class SupplierCapabilities
{
    public List<ProductCategory> ProductCategories { get; set; }
    public ProductionCapacity Capacity { get; set; }
    public QualityStandards Quality { get; set; }
    public DeliveryCapabilities Delivery { get; set; }
    public List<string> Certifications { get; set; }    // ["ISO 9001", "ISO 14001", "OHSAS 18001"]
    public List<string> Locations { get; set; }         // ["Nairobi", "Mombasa", "Kisumu"]
    public TechnicalCapabilities Technical { get; set; }
    public List<string> SpecialServices { get; set; }   // ["Custom blends", "Just-in-time delivery"]
}

public class ProductCategory
{
    public string Name { get; set; }                    // "Portland Cement"
    public string Code { get; set; }                    // "PC"
    public List<string> Products { get; set; }          // ["Grade 32.5", "Grade 42.5"]
    public decimal MinOrderQuantity { get; set; }       // 100.0 (tons)
    public decimal MaxOrderQuantity { get; set; }       // 10000.0 (tons)
    public string Unit { get; set; }                    // "Tons"
    public int LeadTimeDays { get; set; }               // 7
    public bool IsActive { get; set; }                  // true
}

public class ProductionCapacity
{
    public decimal DailyCapacity { get; set; }          // 5000.0 (tons per day)
    public decimal MonthlyCapacity { get; set; }        // 150000.0 (tons per month)
    public decimal YearlyCapacity { get; set; }         // 1800000.0 (tons per year)
    public decimal CurrentUtilization { get; set; }     // 85.0 (%)
    public decimal AvailableCapacity { get; set; }      // 750.0 (tons per day)
    public List<string> ProductionLines { get; set; }   // ["Line 1", "Line 2", "Line 3"]
    public string CapacityUnit { get; set; }            // "Tons"
    public DateTime LastUpdated { get; set; }           // 2024-01-20
}

public class QualityStandards
{
    public List<string> Standards { get; set; }         // ["KEBS KS 02-1055:2017", "ASTM C150"]
    public List<string> TestCertificates { get; set; }  // ["Certificate of Analysis", "Compliance Certificate"]
    public decimal QualityScore { get; set; }           // 4.5 (out of 5)
    public int DefectRate { get; set; }                 // 2 (per 1000 units)
    public List<string> QualityControlMeasures { get; set; } // ["Batch testing", "Continuous monitoring"]
    public bool HasQualityManagementSystem { get; set; } // true
    public string QMSCertification { get; set; }        // "ISO 9001:2015"
}

public class DeliveryCapabilities
{
    public List<string> DeliveryMethods { get; set; }   // ["Truck", "Rail", "Bulk carrier"]
    public List<string> ServiceAreas { get; set; }      // ["Nairobi", "Mombasa", "Central Kenya"]
    public int StandardLeadTime { get; set; }           // 7 (days)
    public int ExpressLeadTime { get; set; }            // 3 (days)
    public bool OffersJustInTime { get; set; }          // true
    public bool OffersScheduledDelivery { get; set; }   // true
    public decimal OnTimeDeliveryRate { get; set; }     // 95.0 (%)
    public List<string> PackagingOptions { get; set; }  // ["Bulk", "50kg bags", "1.5T bulk bags"]
    public bool HasOwnFleet { get; set; }               // true
    public int FleetSize { get; set; }                  // 250
}

public class TechnicalCapabilities
{
    public List<string> TechnicalServices { get; set; } // ["Product development", "Technical support"]
    public bool HasRnDFacility { get; set; }            // true
    public List<string> TechnicalCertifications { get; set; } // ["NEMA License", "Engineering certification"]
    public bool OffersCustomization { get; set; }       // true
    public bool OffersProductDevelopment { get; set; }  // true
    public List<string> TechnicalSupport { get; set; }  // ["On-site support", "Remote monitoring"]
    public bool HasTechnicalTraining { get; set; }      // true
}

public enum SupplierType
{
    RawMaterial,        // Raw material supplier
    Finished,           // Finished goods supplier
    Service,            // Service provider
    Equipment,          // Equipment supplier
    Logistics,          // Logistics provider
    Utility,            // Utility provider
    Maintenance,        // Maintenance contractor
    Consultant         // Consulting services
}

public enum SupplierStatus
{
    Active,             // Active supplier
    Inactive,           // Inactive supplier
    Suspended,          // Temporarily suspended
    Blacklisted,        // Blacklisted supplier
    Prospective,        // Potential supplier
    OnHold,             // On hold
    Terminated         // Contract terminated
}
```

#### **SupplierContract Entity**
```csharp
public class SupplierContract : BaseEntity
{
    public string SupplierId { get; set; }              // References Supplier
    public string ContractNumber { get; set; }          // "SC-2024-001"
    public ContractType Type { get; set; }              // Master, Framework, Spot, Service
    public ContractScope Scope { get; set; }
    public ContractTerms Terms { get; set; }
    public PricingStructure Pricing { get; set; }
    public DeliveryTerms Delivery { get; set; }
    public QualityRequirements Quality { get; set; }
    public DateTime StartDate { get; set; }             // 2024-01-01
    public DateTime EndDate { get; set; }               // 2024-12-31
    public ContractStatus Status { get; set; }          // Draft, Active, Expired, Terminated, Renewed
    public List<ContractAmendment> Amendments { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedBy { get; set; }
    public string UpdatedBy { get; set; }
    
    // Navigation properties
    public Supplier Supplier { get; set; }
}

public enum ContractType
{
    Master,             // Master supply agreement
    Framework,          // Framework agreement
    Spot,               // Spot purchase
    Service,            // Service agreement
    Maintenance,        // Maintenance contract
    Exclusive,          // Exclusive supply
    Preferred          // Preferred supplier
}

public class ContractScope
{
    public List<string> ProductCategories { get; set; } // ["Cement", "Aggregates"]
    public List<string> ServiceTypes { get; set; }      // ["Supply", "Delivery", "Technical support"]
    public List<string> Locations { get; set; }         // ["Nairobi Plant", "Mombasa Plant"]
    public decimal MinAnnualVolume { get; set; }        // 50000.0 (tons)
    public decimal MaxAnnualVolume { get; set; }        // 200000.0 (tons)
    public string VolumeUnit { get; set; }              // "Tons"
    public bool IsExclusive { get; set; }               // false
    public decimal EstimatedValue { get; set; }         // 500000000.0 (KES)
    public string Currency { get; set; }                // "KES"
}

public class ContractTerms
{
    public PaymentTerms Payment { get; set; }
    public LegalTerms Legal { get; set; }
    public PerformanceTerms Performance { get; set; }
    public RiskAndInsurance Risk { get; set; }
    public ComplianceTerms Compliance { get; set; }
    public TerminationTerms Termination { get; set; }
    public DisputeResolution Dispute { get; set; }
}

public class PaymentTerms
{
    public int PaymentPeriodDays { get; set; }          // 30
    public string PaymentMethod { get; set; }           // "Bank transfer"
    public string PaymentSchedule { get; set; }         // "Net 30 days"
    public decimal EarlyPaymentDiscount { get; set; }   // 2.0 (%)
    public int EarlyPaymentDays { get; set; }           // 10
    public decimal LatePaymentPenalty { get; set; }     // 1.5 (% per month)
    public string Currency { get; set; }                // "KES"
    public bool RequiresPurchaseOrder { get; set; }     // true
    public decimal CreditLimit { get; set; }            // 10000000.0 (KES)
}

public class LegalTerms
{
    public string GoverningLaw { get; set; }            // "Laws of Kenya"
    public string Jurisdiction { get; set; }            // "High Court of Kenya"
    public string ContractLanguage { get; set; }        // "English"
    public List<string> Warranties { get; set; }        // ["Product quality", "Delivery performance"]
    public List<string> Liabilities { get; set; }       // ["Product defects", "Delivery delays"]
    public decimal LiabilityLimit { get; set; }         // 50000000.0 (KES)
    public bool HasForceNovelleClause { get; set; }     // true
    public bool HasConfidentialityClause { get; set; }  // true
}

public class PerformanceTerms
{
    public List<PerformanceKPI> KPIs { get; set; }
    public decimal MinPerformanceScore { get; set; }    // 80.0 (%)
    public string ReviewFrequency { get; set; }         // "Monthly"
    public List<string> Penalties { get; set; }         // ["Quality penalty", "Delivery penalty"]
    public List<string> Incentives { get; set; }        // ["Volume bonus", "Performance bonus"]
    public bool HasSLA { get; set; }                    // true
    public string SLADocument { get; set; }             // "SLA-001"
}

public class PerformanceKPI
{
    public string Name { get; set; }                    // "On-time delivery"
    public string Description { get; set; }             // "Percentage of deliveries on time"
    public decimal Target { get; set; }                 // 95.0 (%)
    public decimal Weight { get; set; }                 // 30.0 (% of total score)
    public string MeasurementMethod { get; set; }       // "Delivery date vs scheduled date"
    public string ReportingFrequency { get; set; }      // "Monthly"
}

public class RiskAndInsurance
{
    public List<string> RiskCategories { get; set; }    // ["Quality", "Delivery", "Financial"]
    public List<string> RiskMitigation { get; set; }    // ["Insurance", "Bonds", "Guarantees"]
    public List<InsuranceRequirement> Insurance { get; set; }
    public decimal PerformanceBondValue { get; set; }   // 5000000.0 (KES)
    public bool RequiresPerformanceBond { get; set; }   // true
    public bool RequiresAdvancePaymentBond { get; set; } // false
}

public class InsuranceRequirement
{
    public string Type { get; set; }                    // "General liability"
    public decimal MinCoverage { get; set; }            // 50000000.0 (KES)
    public string Currency { get; set; }                // "KES"
    public bool IsRequired { get; set; }                // true
    public string CertificateRequired { get; set; }     // "Certificate of insurance"
}

public class ComplianceTerms
{
    public List<string> RequiredCertifications { get; set; } // ["ISO 9001", "KEBS certification"]
    public List<string> RegulatoryCompliance { get; set; } // ["NEMA compliance", "KBS standards"]
    public bool RequiresAudit { get; set; }             // true
    public string AuditFrequency { get; set; }          // "Annual"
    public List<string> ComplianceReports { get; set; } // ["Quality reports", "Safety reports"]
    public bool RequiresBackgroundCheck { get; set; }   // true
}

public class TerminationTerms
{
    public int NoticePeriodDays { get; set; }           // 60
    public List<string> TerminationCauses { get; set; } // ["Breach of contract", "Insolvency"]
    public string TerminationProcedure { get; set; }    // "Written notice required"
    public List<string> PostTerminationObligations { get; set; } // ["Return confidential info"]
    public bool HasAutoRenewal { get; set; }            // true
    public int AutoRenewalPeriodDays { get; set; }      // 365
}

public class DisputeResolution
{
    public string Method { get; set; }                  // "Arbitration"
    public string Location { get; set; }                // "Nairobi, Kenya"
    public string Rules { get; set; }                   // "KCAA Arbitration Rules"
    public string Language { get; set; }                // "English"
    public int NumberOfArbitrators { get; set; }        // 1
    public bool RequiresMediation { get; set; }         // true
    public int MediationPeriodDays { get; set; }        // 30
}

public class ContractAmendment
{
    public string AmendmentNumber { get; set; }         // "AMD-001"
    public DateTime EffectiveDate { get; set; }         // 2024-06-01
    public string Description { get; set; }             // "Price increase due to material cost"
    public List<string> ChangedClauses { get; set; }    // ["Clause 3.2 - Pricing"]
    public string Reason { get; set; }                  // "Market conditions"
    public string ApprovedBy { get; set; }              // "John Mwangi"
    public DateTime ApprovalDate { get; set; }          // 2024-05-15
}

public enum ContractStatus
{
    Draft,              // Contract in draft
    Active,             // Contract active
    Expired,            // Contract expired
    Terminated,         // Contract terminated
    Renewed,            // Contract renewed
    Suspended,          // Contract suspended
    UnderReview        // Contract under review
}
```

#### **SupplierPerformance Entity**
```csharp
public class SupplierPerformance : BaseEntity
{
    public string SupplierId { get; set; }              // References Supplier
    public string Period { get; set; }                  // "2024-01" (Monthly)
    public QualityPerformance Quality { get; set; }
    public DeliveryPerformance Delivery { get; set; }
    public ServicePerformance Service { get; set; }
    public CostPerformance Cost { get; set; }
    public CompliancePerformance Compliance { get; set; }
    public decimal OverallScore { get; set; }            // 8.5 (out of 10)
    public PerformanceRating Rating { get; set; }       // Excellent, Good, Fair, Poor
    public List<string> Strengths { get; set; }         // ["Excellent quality", "On-time delivery"]
    public List<string> Weaknesses { get; set; }        // ["High prices", "Limited capacity"]
    public List<string> ImprovementActions { get; set; } // ["Cost optimization", "Capacity expansion"]
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Navigation properties
    public Supplier Supplier { get; set; }
}

public class QualityPerformance
{
    public decimal QualityScore { get; set; }           // 9.2 (out of 10)
    public int TotalDeliveries { get; set; }            // 45
    public int AcceptedDeliveries { get; set; }         // 43
    public int RejectedDeliveries { get; set; }         // 2
    public decimal AcceptanceRate { get; set; }         // 95.6 (%)
    public int QualityComplaints { get; set; }          // 3
    public int QualityComplaintsResolved { get; set; }  // 3
    public decimal DefectRate { get; set; }             // 0.5 (%)
    public List<string> QualityIssues { get; set; }     // ["Moisture content high", "Particle size variation"]
    public List<string> QualityImprovements { get; set; } // ["Better quality control", "Equipment upgrade"]
}

public class DeliveryPerformance
{
    public decimal DeliveryScore { get; set; }          // 8.8 (out of 10)
    public int TotalDeliveries { get; set; }            // 45
    public int OnTimeDeliveries { get; set; }           // 40
    public int LateDeliveries { get; set; }             // 5
    public decimal OnTimeRate { get; set; }             // 88.9 (%)
    public decimal AverageDelayDays { get; set; }       // 1.5
    public int DeliveryComplaints { get; set; }         // 2
    public List<string> DeliveryIssues { get; set; }    // ["Traffic delays", "Vehicle breakdown"]
    public decimal OrderFulfillmentRate { get; set; }   // 95.0 (%)
    public decimal ShortageRate { get; set; }           // 2.0 (%)
}

public class ServicePerformance
{
    public decimal ServiceScore { get; set; }           // 8.5 (out of 10)
    public decimal ResponseTime { get; set; }           // 2.5 (hours)
    public decimal ResolutionTime { get; set; }         // 24.0 (hours)
    public int ServiceRequests { get; set; }            // 15
    public int ServiceRequestsResolved { get; set; }    // 14
    public decimal ResolutionRate { get; set; }         // 93.3 (%)
    public decimal CustomerSatisfaction { get; set; }   // 4.2 (out of 5)
    public List<string> ServiceIssues { get; set; }     // ["Slow response", "Incomplete information"]
    public List<string> ServiceImprovements { get; set; } // ["Faster response", "Better communication"]
}

public class CostPerformance
{
    public decimal CostScore { get; set; }              // 7.5 (out of 10)
    public decimal AverageUnitCost { get; set; }        // 12500.0 (KES per ton)
    public decimal MarketBenchmark { get; set; }        // 12000.0 (KES per ton)
    public decimal CostVariance { get; set; }           // 4.2 (% above benchmark)
    public decimal PriceStability { get; set; }         // 85.0 (%)
    public int PriceChanges { get; set; }               // 3
    public decimal VolumeDiscounts { get; set; }        // 2.0 (%)
    public decimal PaymentDiscounts { get; set; }       // 1.5 (%)
    public decimal TotalSpend { get; set; }             // 562500000.0 (KES)
}

public class CompliancePerformance
{
    public decimal ComplianceScore { get; set; }        // 9.8 (out of 10)
    public int ComplianceChecks { get; set; }           // 12
    public int CompliancePassed { get; set; }           // 12
    public int ComplianceFailed { get; set; }           // 0
    public decimal ComplianceRate { get; set; }         // 100.0 (%)
    public List<string> CertificationsValid { get; set; } // ["ISO 9001", "KEBS", "NEMA"]
    public List<string> CertificationsExpired { get; set; } // []
    public int AuditsPassed { get; set; }               // 2
    public int AuditsFailed { get; set; }               // 0
    public List<string> ComplianceIssues { get; set; }  // []
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

#### **ProcurementOrder Entity**
```csharp
public class ProcurementOrder : BaseEntity
{
    public string OrderNumber { get; set; }             // "PO-2024-001"
    public string SupplierId { get; set; }              // References Supplier
    public string ContractId { get; set; }              // References SupplierContract
    public OrderType Type { get; set; }                 // Standard, Emergency, Blanket, Service
    public List<OrderItem> Items { get; set; }
    public OrderTerms Terms { get; set; }
    public OrderStatus Status { get; set; }             // Draft, Sent, Acknowledged, InProgress, Delivered, Completed, Cancelled
    public DateTime OrderDate { get; set; }             // 2024-01-25
    public DateTime RequiredDate { get; set; }          // 2024-02-01
    public DateTime? AcknowledgedDate { get; set; }     // Supplier acknowledgment
    public DateTime? DeliveryDate { get; set; }         // Actual delivery
    public string RequestedBy { get; set; }             // "John Mwangi"
    public string ApprovedBy { get; set; }              // "Mary Wanjiku"
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedBy { get; set; }
    public string UpdatedBy { get; set; }
    
    // Navigation properties
    public Supplier Supplier { get; set; }
    public SupplierContract Contract { get; set; }
}

public enum OrderType
{
    Standard,           // Standard purchase order
    Emergency,          // Emergency order
    Blanket,           // Blanket order
    Service,           // Service order
    Capital,           // Capital expenditure
    Maintenance,       // Maintenance order
    Consignment       // Consignment order
}

public class OrderItem
{
    public string ProductCode { get; set; }             // "PC-425"
    public string ProductName { get; set; }             // "Portland Cement Grade 42.5"
    public string Description { get; set; }             // "High-strength cement for construction"
    public decimal Quantity { get; set; }               // 500.0
    public string Unit { get; set; }                    // "Tons"
    public decimal UnitPrice { get; set; }              // 12500.0 (KES)
    public decimal TotalPrice { get; set; }             // 6250000.0 (KES)
    public DateTime RequiredDate { get; set; }          // 2024-02-01
    public string DeliveryLocation { get; set; }        // "Nairobi Plant"
    public List<string> SpecialRequirements { get; set; } // ["Quality certificate", "Delivery note"]
    public ItemStatus Status { get; set; }              // Pending, Acknowledged, InProgress, Delivered
}

public enum ItemStatus
{
    Pending,            // Item pending
    Acknowledged,       // Item acknowledged
    InProgress,         // Item in progress
    Delivered,          // Item delivered
    Cancelled,          // Item cancelled
    Backordered        // Item backordered
}

public class OrderTerms
{
    public string DeliveryTerms { get; set; }           // "FOB Destination"
    public string PaymentTerms { get; set; }            // "Net 30 days"
    public string ShippingMethod { get; set; }          // "Truck delivery"
    public string DeliveryAddress { get; set; }         // "Nairobi Plant, Industrial Area"
    public string BillingAddress { get; set; }          // "QaliTrack HQ, Nairobi"
    public List<string> SpecialInstructions { get; set; } // ["Call before delivery", "Quality check required"]
    public decimal TotalValue { get; set; }             // 6250000.0 (KES)
    public string Currency { get; set; }                // "KES"
    public decimal TaxAmount { get; set; }              // 1000000.0 (KES)
    public decimal GrandTotal { get; set; }             // 7250000.0 (KES)
}

public enum OrderStatus
{
    Draft,              // Order in draft
    Sent,               // Order sent to supplier
    Acknowledged,       // Order acknowledged by supplier
    InProgress,         // Order in progress
    Delivered,          // Order delivered
    Completed,          // Order completed
    Cancelled,          // Order cancelled
    OnHold             // Order on hold
}
```

### **Entity Relationships**

```
┌─────────────────┐         ┌─────────────────┐         ┌─────────────────┐
│    Supplier     │◄────────┤ SupplierContract│         │SupplierPerformance│
│                 │         │                 │         │                 │
│ • Id            │         │ • SupplierId    │         │ • SupplierId    │
│ • Name          │         │ • ContractNumber│         │ • Period        │
│ • Type          │         │ • Terms         │         │ • Quality       │
│ • Category      │         │ • Pricing       │         │ • Delivery      │
│ • Capabilities  │         │ • StartDate     │         │ • Service       │
│ • Status        │         │ • EndDate       │         │ • OverallScore  │
└─────────────────┘         │ • Status        │         └─────────────────┘
         │                  └─────────────────┘                   │
         │                            │                           │
         ▼                            ▼                           ▼
┌─────────────────┐         ┌─────────────────┐         ┌─────────────────┐
│ProcurementOrder │         │ Future Product  │         │  Future Cost    │
│                 │         │   Service       │         │   Management    │
│ • SupplierId    │         │ (Integration)   │         │   Service       │
│ • ContractId    │         │                 │         │ (Integration)   │
│ • OrderNumber   │         │ • Product Info  │         │                 │
│ • Items         │         │ • Specifications│         │ • Cost Analysis │
│ • Terms         │         │ • Pricing       │         │ • Budget Mgmt   │
│ • Status        │         │ • Quality       │         │ • Procurement   │
└─────────────────┘         └─────────────────┘         └─────────────────┘
```

---

## 🔌 **API Endpoints**

### **Supplier Management**

#### **GET /api/suppliers**
**Purpose**: Retrieve paginated supplier list with filtering
```json
// Request
GET /api/suppliers?page=1&size=10&type=RawMaterial&category=Cement&status=Active

// Response
{
  "data": [
    {
      "id": "supplier-001",
      "name": "Bamburi Cement Ltd",
      "code": "SUP-BAM-001",
      "registrationNumber": "CR-2010-001",
      "company": {
        "legalName": "Bamburi Cement Limited",
        "tradingName": "Bamburi",
        "taxNumber": "KRA-PIN-P123456789A",
        "vatNumber": "VAT-001234567",
        "businessLicense": "BL-2020-001",
        "size": "Large",
        "establishedDate": "1951-01-01T00:00:00Z",
        "registeredAddress": {
          "street": "Nyali Road",
          "city": "Mombasa",
          "postalCode": "80100",
          "country": "Kenya"
        },
        "website": "https://www.bamburi.co.ke",
        "financial": {
          "annualRevenue": 15000000000.0,
          "currency": "KES",
          "creditRating": "AA+",
          "creditLimit": 50000000.0,
          "paymentTermsDays": 30,
          "preferredPaymentMethod": "Bank Transfer",
          "bankName": "KCB Bank",
          "bankAccountNumber": "1234567890",
          "bankBranch": "Nairobi Branch",
          "swiftCode": "KCBLKENX"
        }
      },
      "contact": {
        "primaryContactName": "John Mwangi",
        "primaryContactTitle": "Sales Manager",
        "primaryContactPhone": "+254712345678",
        "primaryContactEmail": "john.mwangi@bamburi.co.ke",
        "secondaryContactName": "Mary Wanjiku",
        "secondaryContactTitle": "Account Manager",
        "secondaryContactPhone": "+254723456789",
        "secondaryContactEmail": "mary.wanjiku@bamburi.co.ke",
        "customerServicePhone": "+254700123456",
        "customerServiceEmail": "support@bamburi.co.ke",
        "emergencyContact": "+254711987654",
        "preferredCommunication": ["Email", "Phone", "WhatsApp"]
      },
      "capabilities": {
        "productCategories": [
          {
            "name": "Portland Cement",
            "code": "PC",
            "products": ["Grade 32.5", "Grade 42.5"],
            "minOrderQuantity": 100.0,
            "maxOrderQuantity": 10000.0,
            "unit": "Tons",
            "leadTimeDays": 7,
            "isActive": true
          }
        ],
        "capacity": {
          "dailyCapacity": 5000.0,
          "monthlyCapacity": 150000.0,
          "yearlyCapacity": 1800000.0,
          "currentUtilization": 85.0,
          "availableCapacity": 750.0,
          "productionLines": ["Line 1", "Line 2", "Line 3"],
          "capacityUnit": "Tons",
          "lastUpdated": "2024-01-20T00:00:00Z"
        },
        "quality": {
          "standards": ["KEBS KS 02-1055:2017", "ASTM C150"],
          "testCertificates": ["Certificate of Analysis", "Compliance Certificate"],
          "qualityScore": 4.5,
          "defectRate": 2,
          "qualityControlMeasures": ["Batch testing", "Continuous monitoring"],
          "hasQualityManagementSystem": true,
          "qmsCertification": "ISO 9001:2015"
        },
        "delivery": {
          "deliveryMethods": ["Truck", "Rail", "Bulk carrier"],
          "serviceAreas": ["Nairobi", "Mombasa", "Central Kenya"],
          "standardLeadTime": 7,
          "expressLeadTime": 3,
          "offersJustInTime": true,
          "offersScheduledDelivery": true,
          "onTimeDeliveryRate": 95.0,
          "packagingOptions": ["Bulk", "50kg bags", "1.5T bulk bags"],
          "hasOwnFleet": true,
          "fleetSize": 250
        },
        "certifications": ["ISO 9001", "ISO 14001", "OHSAS 18001"],
        "locations": ["Nairobi", "Mombasa", "Kisumu"],
        "technical": {
          "technicalServices": ["Product development", "Technical support"],
          "hasRnDFacility": true,
          "technicalCertifications": ["NEMA License", "Engineering certification"],
          "offersCustomization": true,
          "offersProductDevelopment": true,
          "technicalSupport": ["On-site support", "Remote monitoring"],
          "hasTechnicalTraining": true
        },
        "specialServices": ["Custom blends", "Just-in-time delivery"]
      },
      "type": "RawMaterial",
      "categories": ["Cement", "Aggregates", "Additives"],
      "status": "Active",
      "createdAt": "2020-01-15T10:30:00Z",
      "updatedAt": "2024-01-20T14:45:00Z"
    }
  ],
  "pagination": {
    "page": 1,
    "size": 10,
    "total": 25,
    "totalPages": 3
  }
}
```

#### **GET /api/suppliers/{id}**
**Purpose**: Retrieve specific supplier details
```json
// Request
GET /api/suppliers/supplier-001

// Response: Complete supplier object with all capabilities and information
```

#### **POST /api/suppliers**
**Purpose**: Create new supplier
```json
// Request
POST /api/suppliers
{
  "name": "Kenya Cement Company",
  "code": "SUP-KCC-001",
  "registrationNumber": "CR-2015-002",
  "company": {
    "legalName": "Kenya Cement Company Limited",
    "tradingName": "KCC",
    "taxNumber": "KRA-PIN-P987654321B",
    "vatNumber": "VAT-002345678",
    "businessLicense": "BL-2021-002",
    "size": "Medium",
    "establishedDate": "2015-01-01T00:00:00Z",
    "registeredAddress": {
      "street": "Uhuru Highway",
      "city": "Nairobi",
      "postalCode": "00100",
      "country": "Kenya"
    },
    "website": "https://www.kcc.co.ke",
    "financial": {
      "annualRevenue": 5000000000.0,
      "currency": "KES",
      "creditRating": "A",
      "creditLimit": 20000000.0,
      "paymentTermsDays": 30,
      "preferredPaymentMethod": "Bank Transfer",
      "bankName": "Equity Bank",
      "bankAccountNumber": "0987654321",
      "bankBranch": "Nairobi Branch",
      "swiftCode": "EQBLKENX"
    }
  },
  "contact": {
    "primaryContactName": "Peter Kiprotich",
    "primaryContactTitle": "Sales Director",
    "primaryContactPhone": "+254734567890",
    "primaryContactEmail": "peter.kiprotich@kcc.co.ke",
    "secondaryContactName": "Jane Wanjiru",
    "secondaryContactTitle": "Customer Service Manager",
    "secondaryContactPhone": "+254745678901",
    "secondaryContactEmail": "jane.wanjiru@kcc.co.ke",
    "customerServicePhone": "+254711987654",
    "customerServiceEmail": "support@kcc.co.ke",
    "emergencyContact": "+254722876543",
    "preferredCommunication": ["Email", "Phone"]
  },
  "capabilities": {
    "productCategories": [
      {
        "name": "Portland Cement",
        "code": "PC",
        "products": ["Grade 32.5", "Grade 42.5"],
        "minOrderQuantity": 50.0,
        "maxOrderQuantity": 5000.0,
        "unit": "Tons",
        "leadTimeDays": 10,
        "isActive": true
      }
    ],
    "capacity": {
      "dailyCapacity": 2000.0,
      "monthlyCapacity": 60000.0,
      "yearlyCapacity": 720000.0,
      "currentUtilization": 70.0,
      "availableCapacity": 600.0,
      "productionLines": ["Line A", "Line B"],
      "capacityUnit": "Tons",
      "lastUpdated": "2024-01-20T00:00:00Z"
    },
    "quality": {
      "standards": ["KEBS KS 02-1055:2017"],
      "testCertificates": ["Certificate of Analysis"],
      "qualityScore": 4.2,
      "defectRate": 3,
      "qualityControlMeasures": ["Batch testing"],
      "hasQualityManagementSystem": true,
      "qmsCertification": "ISO 9001:2015"
    },
    "delivery": {
      "deliveryMethods": ["Truck"],
      "serviceAreas": ["Nairobi", "Central Kenya"],
      "standardLeadTime": 10,
      "expressLeadTime": 5,
      "offersJustInTime": false,
      "offersScheduledDelivery": true,
      "onTimeDeliveryRate": 90.0,
      "packagingOptions": ["50kg bags"],
      "hasOwnFleet": true,
      "fleetSize": 50
    },
    "certifications": ["ISO 9001", "KEBS"],
    "locations": ["Nairobi"],
    "technical": {
      "technicalServices": ["Technical support"],
      "hasRnDFacility": false,
      "technicalCertifications": ["NEMA License"],
      "offersCustomization": false,
      "offersProductDevelopment": false,
      "technicalSupport": ["On-site support"],
      "hasTechnicalTraining": false
    },
    "specialServices": ["Scheduled delivery"]
  },
  "type": "RawMaterial",
  "categories": ["Cement"],
  "status": "Active"
}

// Response: Created supplier object
```

### **Contract Management**

#### **GET /api/suppliers/{id}/contracts**
**Purpose**: Retrieve supplier contracts
```json
// Request
GET /api/suppliers/supplier-001/contracts?status=Active

// Response
{
  "data": [
    {
      "id": "contract-001",
      "supplierId": "supplier-001",
      "contractNumber": "SC-2024-001",
      "type": "Master",
      "scope": {
        "productCategories": ["Cement", "Aggregates"],
        "serviceTypes": ["Supply", "Delivery", "Technical support"],
        "locations": ["Nairobi Plant", "Mombasa Plant"],
        "minAnnualVolume": 50000.0,
        "maxAnnualVolume": 200000.0,
        "volumeUnit": "Tons",
        "isExclusive": false,
        "estimatedValue": 500000000.0,
        "currency": "KES"
      },
      "terms": {
        "payment": {
          "paymentPeriodDays": 30,
          "paymentMethod": "Bank transfer",
          "paymentSchedule": "Net 30 days",
          "earlyPaymentDiscount": 2.0,
          "earlyPaymentDays": 10,
          "latePaymentPenalty": 1.5,
          "currency": "KES",
          "requiresPurchaseOrder": true,
          "creditLimit": 10000000.0
        },
        "legal": {
          "governingLaw": "Laws of Kenya",
          "jurisdiction": "High Court of Kenya",
          "contractLanguage": "English",
          "warranties": ["Product quality", "Delivery performance"],
          "liabilities": ["Product defects", "Delivery delays"],
          "liabilityLimit": 50000000.0,
          "hasForceNovelleClause": true,
          "hasConfidentialityClause": true
        },
        "performance": {
          "kpis": [
            {
              "name": "On-time delivery",
              "description": "Percentage of deliveries on time",
              "target": 95.0,
              "weight": 30.0,
              "measurementMethod": "Delivery date vs scheduled date",
              "reportingFrequency": "Monthly"
            }
          ],
          "minPerformanceScore": 80.0,
          "reviewFrequency": "Monthly",
          "penalties": ["Quality penalty", "Delivery penalty"],
          "incentives": ["Volume bonus", "Performance bonus"],
          "hasSLA": true,
          "slaDocument": "SLA-001"
        }
      },
      "pricing": {
        "basePrice": 12500.0,
        "currency": "KES",
        "priceUnit": "Per ton",
        "discountStructure": [
          {
            "volumeThreshold": 1000.0,
            "discount": 2.0,
            "description": "Volume discount for orders above 1000 tons"
          }
        ],
        "priceEscalation": {
          "escalationRate": 5.0,
          "escalationFrequency": "Annual",
          "escalationIndex": "Kenya inflation rate"
        }
      },
      "startDate": "2024-01-01T00:00:00Z",
      "endDate": "2024-12-31T23:59:59Z",
      "status": "Active",
      "amendments": [],
      "createdAt": "2023-12-15T10:00:00Z",
      "updatedAt": "2024-01-01T00:00:00Z"
    }
  ]
}
```

### **Performance Management**

#### **GET /api/suppliers/{id}/performance**
**Purpose**: Retrieve supplier performance metrics
```json
// Request
GET /api/suppliers/supplier-001/performance?period=2024-01

// Response
{
  "data": [
    {
      "id": "perf-001",
      "supplierId": "supplier-001",
      "period": "2024-01",
      "quality": {
        "qualityScore": 9.2,
        "totalDeliveries": 45,
        "acceptedDeliveries": 43,
        "rejectedDeliveries": 2,
        "acceptanceRate": 95.6,
        "qualityComplaints": 3,
        "qualityComplaintsResolved": 3,
        "defectRate": 0.5,
        "qualityIssues": ["Moisture content high", "Particle size variation"],
        "qualityImprovements": ["Better quality control", "Equipment upgrade"]
      },
      "delivery": {
        "deliveryScore": 8.8,
        "totalDeliveries": 45,
        "onTimeDeliveries": 40,
        "lateDeliveries": 5,
        "onTimeRate": 88.9,
        "averageDelayDays": 1.5,
        "deliveryComplaints": 2,
        "deliveryIssues": ["Traffic delays", "Vehicle breakdown"],
        "orderFulfillmentRate": 95.0,
        "shortageRate": 2.0
      },
      "service": {
        "serviceScore": 8.5,
        "responseTime": 2.5,
        "resolutionTime": 24.0,
        "serviceRequests": 15,
        "serviceRequestsResolved": 14,
        "resolutionRate": 93.3,
        "customerSatisfaction": 4.2,
        "serviceIssues": ["Slow response", "Incomplete information"],
        "serviceImprovements": ["Faster response", "Better communication"]
      },
      "cost": {
        "costScore": 7.5,
        "averageUnitCost": 12500.0,
        "marketBenchmark": 12000.0,
        "costVariance": 4.2,
        "priceStability": 85.0,
        "priceChanges": 3,
        "volumeDiscounts": 2.0,
        "paymentDiscounts": 1.5,
        "totalSpend": 562500000.0
      },
      "compliance": {
        "complianceScore": 9.8,
        "complianceChecks": 12,
        "compliancePassed": 12,
        "complianceFailed": 0,
        "complianceRate": 100.0,
        "certificationsValid": ["ISO 9001", "KEBS", "NEMA"],
        "certificationsExpired": [],
        "auditsPassed": 2,
        "auditsFailed": 0,
        "complianceIssues": []
      },
      "overallScore": 8.5,
      "rating": "Good",
      "strengths": ["Excellent quality", "On-time delivery"],
      "weaknesses": ["High prices", "Limited capacity"],
      "improvementActions": ["Cost optimization", "Capacity expansion"],
      "periodStart": "2024-01-01T00:00:00Z",
      "periodEnd": "2024-01-31T23:59:59Z",
      "createdAt": "2024-02-01T10:00:00Z",
      "updatedAt": "2024-02-01T10:00:00Z"
    }
  ]
}
```

### **Procurement Management**

#### **GET /api/procurement-orders**
**Purpose**: Retrieve procurement orders
```json
// Request
GET /api/procurement-orders?supplier=supplier-001&status=InProgress

// Response
{
  "data": [
    {
      "id": "po-001",
      "orderNumber": "PO-2024-001",
      "supplierId": "supplier-001",
      "contractId": "contract-001",
      "type": "Standard",
      "items": [
        {
          "productCode": "PC-425",
          "productName": "Portland Cement Grade 42.5",
          "description": "High-strength cement for construction",
          "quantity": 500.0,
          "unit": "Tons",
          "unitPrice": 12500.0,
          "totalPrice": 6250000.0,
          "requiredDate": "2024-02-01T00:00:00Z",
          "deliveryLocation": "Nairobi Plant",
          "specialRequirements": ["Quality certificate", "Delivery note"],
          "status": "InProgress"
        }
      ],
      "terms": {
        "deliveryTerms": "FOB Destination",
        "paymentTerms": "Net 30 days",
        "shippingMethod": "Truck delivery",
        "deliveryAddress": "Nairobi Plant, Industrial Area",
        "billingAddress": "QaliTrack HQ, Nairobi",
        "specialInstructions": ["Call before delivery", "Quality check required"],
        "totalValue": 6250000.0,
        "currency": "KES",
        "taxAmount": 1000000.0,
        "grandTotal": 7250000.0
      },
      "status": "InProgress",
      "orderDate": "2024-01-25T00:00:00Z",
      "requiredDate": "2024-02-01T00:00:00Z",
      "acknowledgedDate": "2024-01-26T10:00:00Z",
      "deliveryDate": null,
      "requestedBy": "John Mwangi",
      "approvedBy": "Mary Wanjiku",
      "createdAt": "2024-01-25T09:00:00Z",
      "updatedAt": "2024-01-26T10:30:00Z"
    }
  ]
}
```

#### **POST /api/procurement-orders**
**Purpose**: Create new procurement order
```json
// Request
POST /api/procurement-orders
{
  "orderNumber": "PO-2024-002",
  "supplierId": "supplier-001",
  "contractId": "contract-001",
  "type": "Standard",
  "items": [
    {
      "productCode": "PC-325",
      "productName": "Portland Cement Grade 32.5",
      "description": "Standard cement for general construction",
      "quantity": 300.0,
      "unit": "Tons",
      "unitPrice": 11500.0,
      "totalPrice": 3450000.0,
      "requiredDate": "2024-02-10T00:00:00Z",
      "deliveryLocation": "Mombasa Plant",
      "specialRequirements": ["Quality certificate"],
      "status": "Pending"
    }
  ],
  "terms": {
    "deliveryTerms": "FOB Destination",
    "paymentTerms": "Net 30 days",
    "shippingMethod": "Truck delivery",
    "deliveryAddress": "Mombasa Plant, Industrial Area",
    "billingAddress": "QaliTrack HQ, Nairobi",
    "specialInstructions": ["Quality check required"],
    "totalValue": 3450000.0,
    "currency": "KES",
    "taxAmount": 552000.0,
    "grandTotal": 4002000.0
  },
  "orderDate": "2024-01-28T00:00:00Z",
  "requiredDate": "2024-02-10T00:00:00Z",
  "requestedBy": "Peter Kiprotich",
  "approvedBy": "Mary Wanjiku"
}

// Response: Created procurement order object
```

### **Business-Specific Endpoints**

#### **GET /api/suppliers/search**
**Purpose**: Advanced supplier search
```json
// Request
GET /api/suppliers/search?category=Cement&location=Nairobi&capacity=1000&rating=Good

// Response
{
  "data": [
    {
      "id": "supplier-001",
      "name": "Bamburi Cement Ltd",
      "type": "RawMaterial",
      "categories": ["Cement"],
      "overallScore": 8.5,
      "rating": "Good",
      "capabilities": {
        "availableCapacity": 750.0,
        "onTimeDeliveryRate": 95.0,
        "qualityScore": 4.5
      },
      "matchScore": 95.2
    }
  ]
}
```

#### **GET /api/suppliers/performance-summary**
**Purpose**: Supplier network performance summary
```json
// Request
GET /api/suppliers/performance-summary?period=2024-01

// Response
{
  "summary": {
    "totalSuppliers": 25,
    "activeSuppliers": 23,
    "averageRating": 8.2,
    "topPerformers": [
      {
        "id": "supplier-001",
        "name": "Bamburi Cement Ltd",
        "score": 9.2,
        "rating": "Excellent"
      }
    ],
    "qualityScore": 8.8,
    "deliveryScore": 8.5,
    "serviceScore": 8.3,
    "complianceRate": 96.0,
    "totalSpend": 2500000000.0
  }
}
```

#### **POST /api/suppliers/bulk-update**
**Purpose**: Bulk update supplier information
```json
// Request
POST /api/suppliers/bulk-update
{
  "suppliers": [
    {
      "id": "supplier-001",
      "status": "Suspended",
      "reason": "Quality issues"
    },
    {
      "id": "supplier-002",
      "status": "Active",
      "reason": "Issues resolved"
    }
  ]
}

// Response
{
  "updated": 2,
  "failed": 0,
  "results": [
    {
      "id": "supplier-001",
      "status": "Success",
      "message": "Supplier status updated to Suspended"
    },
    {
      "id": "supplier-002",
      "status": "Success",
      "message": "Supplier status updated to Active"
    }
  ]
}
```

---

## 🔗 **Integration Points**

### **Service Dependencies**

#### **Future Product Service Integration**
```
┌─────────────────┐         ┌─────────────────┐
│ Product Service │◄───────►│ Supplier Service│
│                 │         │                 │
│ • Product Info  │         │ • Supplier Info │
│ • Specifications│         │ • Capabilities  │
│ • Quality       │         │ • Performance   │
└─────────────────┘         └─────────────────┘
```

#### **Future Cost Management Integration**
```
┌─────────────────┐         ┌─────────────────┐
│ Cost Management │◄───────►│ Supplier Service│
│ Service (Future)│         │                 │
│                 │         │ • Cost Data     │
│ • Cost Analysis │         │ • Pricing       │
│ • Budget Mgmt   │         │ • Contracts     │
│ • Procurement   │         │ • Orders        │
└─────────────────┘         └─────────────────┘
```

#### **Customer Service Integration** (Indirect)
```
┌─────────────────┐         ┌─────────────────┐
│ Customer Service│    ?    │ Supplier Service│
│                 │  ──────►│                 │
│ • Orders        │         │ • Procurement   │
│ • Requirements  │         │ • Supply Chain  │
│ • Specifications│         │ • Quality       │
└─────────────────┘         └─────────────────┘
```

### **Integration Patterns**

#### **Supplier Validation Pattern**
```csharp
// Procurement Service -> Supplier Service
public async Task<bool> ValidateSupplierAsync(string supplierId, string productCategory)
{
    var supplier = await _supplierService.GetSupplierAsync(supplierId);
    return supplier.Status == SupplierStatus.Active && 
           supplier.Categories.Contains(productCategory);
}
```

#### **Capability Check Pattern**
```csharp
// Procurement Service -> Supplier Service
public async Task<bool> CheckSupplierCapabilityAsync(string supplierId, decimal quantity, DateTime requiredDate)
{
    var capabilities = await _supplierService.GetSupplierCapabilitiesAsync(supplierId);
    return capabilities.AvailableCapacity >= quantity && 
           capabilities.StandardLeadTime <= (requiredDate - DateTime.Now).Days;
}
```

---

## 💼 **Business Use Cases**

### **Use Case 1: Supplier Onboarding**

**Scenario**: New supplier wants to become a QaliTrack vendor
**Actors**: Supplier, Procurement Manager, System
**Flow**:
1. Supplier submits registration application with company details
2. Procurement manager reviews supplier capabilities and documentation
3. System validates supplier certifications and compliance requirements
4. Supplier capabilities are assessed against QaliTrack requirements
5. Initial contract terms are negotiated and agreed upon
6. Supplier is activated in the system for procurement orders
7. Performance monitoring begins with first order

**Business Value**: Systematic supplier onboarding ensures quality and compliance

### **Use Case 2: Contract Renewal**

**Scenario**: Existing supplier contract is due for renewal
**Actors**: Procurement Manager, Supplier, Legal Team, System
**Flow**:
1. System alerts about upcoming contract expiry
2. Procurement manager reviews supplier performance history
3. Performance metrics are analyzed for renewal decision
4. Contract terms are renegotiated based on performance
5. Legal team reviews and approves updated contract
6. New contract is signed and activated in system
7. Supplier continues operations under new terms

**Business Value**: Data-driven contract renewals optimize supplier relationships

### **Use Case 3: Procurement Order Processing**

**Scenario**: Plant requires raw materials from supplier
**Actors**: Plant Manager, Procurement Team, Supplier, System
**Flow**:
1. Plant manager identifies material requirement
2. System checks supplier availability and capability
3. Procurement order is created with specifications
4. Order is sent to supplier for acknowledgment
5. Supplier confirms order and delivery schedule
6. Delivery is tracked and performance monitored
7. Order completion updates supplier performance metrics

**Business Value**: Streamlined procurement with performance tracking

### **Use Case 4: Performance Review**

**Scenario**: Monthly supplier performance evaluation
**Actors**: Procurement Manager, Quality Team, Supplier, System
**Flow**:
1. System collects performance data from all transactions
2. Quality metrics are calculated from delivery records
3. Compliance status is verified against requirements
4. Overall performance score is computed
5. Performance report is generated and shared with supplier
6. Improvement plans are developed for underperformers
7. Recognition is given to top performers

**Business Value**: Continuous improvement in supplier relationships

---

## 🛣️ **Implementation Roadmap**

### **Phase 1: Foundation** (Month 8, Week 1-2)
- 🔄 **Project Setup**: Create Clean Architecture structure
- 🔄 **Core Entities**: Implement Supplier, Contract, Performance entities
- 🔄 **Database Layer**: Set up Entity Framework with SQLite
- 🔄 **Basic Repository**: Implement repository pattern for data access

### **Phase 2: Core Features** (Month 8, Week 3-4)
- 🔄 **Supplier Management**: Complete CRUD operations for suppliers
- 🔄 **Contract Management**: Contract creation and terms management
- 🔄 **Performance Management**: KPI tracking and rating system
- 🔄 **API Controllers**: REST API endpoints for all operations

### **Phase 3: Integration** (Month 9, Week 1-2)
- 🔄 **Procurement System**: Order creation and tracking
- 🔄 **Performance Analytics**: Supplier performance metrics
- 🔄 **Compliance Monitoring**: Automated compliance tracking
- 🔄 **API Documentation**: Comprehensive OpenAPI documentation

### **Phase 4: Advanced Features** (Month 9, Week 3-4)
- 🔄 **Advanced Analytics**: Predictive performance analytics
- 🔄 **Integration Readiness**: Prepare for Product and Cost Management integration
- 🔄 **Automated Workflows**: Automated contract and order processes
- 🔄 **Monitoring**: Health checks and performance metrics

### **Current Status**: 🔄 **READY FOR IMPLEMENTATION**
- **Priority**: LOW (Seventh priority - procurement management focus)
- **Dependencies**: None (similar to Customer Service patterns)
- **Estimated Timeline**: 1 month for complete implementation
- **Resource Requirements**: 1 developer, part-time

---

## 📊 **Success Metrics**

### **Technical Metrics**
- **API Response Time**: < 200ms for supplier searches
- **Database Performance**: < 100ms for performance queries
- **Service Availability**: 99.9% uptime
- **Integration Success**: 100% supplier validation accuracy

### **Business Metrics**
- **Supplier Network**: 30+ suppliers registered
- **Contract Compliance**: 95% active contracts
- **Performance Score**: 8.0+ average supplier rating
- **Procurement Value**: 1B+ KES annual procurement

### **Quality Metrics**
- **Supplier Quality**: 95% delivery acceptance rate
- **Delivery Performance**: 90% on-time deliveries
- **Cost Optimization**: 5% cost savings through supplier management
- **System Reliability**: 0 procurement disruptions due to system issues

---

*Supplier Service serves as the central supplier relationship management hub, enabling comprehensive vendor management, contract administration, and performance monitoring across the QaliTrack supply chain network.*