# C4 Level 3: Master Data Service Component Flows

## 👥 **Master Data Service Components**

**Navigation**: [← Main Component Flows](03-component-flows.md) | [← Infrastructure Components](03-component-flows-infrastructure.md) | [Data Manager Components →](03-component-flows-datamanager.md)

This document details the component-level interactions for all 13 QaliTrack Master Data services, showing internal architecture and external dependencies.

---

## **User Service Component Flow (:7001)**

```mermaid
flowchart TD
    subgraph "API Layer"
        AuthController[Auth Controller]
        UserController[User Controller]
        RoleController[Role Controller]
    end
    
    subgraph "Core Layer"
        AuthService[Auth Service]
        UserService[User Service]
        RoleService[Role Service]
        JWTService[JWT Service]
        PasswordService[Password Service]
    end
    
    subgraph "Infrastructure Layer"
        UserRepo[User Repository]
        RoleRepo[Role Repository]
        TokenRepo[Token Repository]
        UserDB[(User Database)]
    end
    
    subgraph "External Dependencies"
        Redis[(Redis Cache)]
        EmailService[Email Service]
        AuditLog[Audit Logger]
    end

    AuthController -->|Login Request| AuthService
    AuthService -->|Validate Credentials| PasswordService
    AuthService -->|Generate Token| JWTService
    AuthService -->|Cache Token| Redis
    
    UserController -->|User Operations| UserService
    UserService -->|Data Access| UserRepo
    UserRepo -->|Query/Update| UserDB
    
    RoleController -->|Role Management| RoleService
    RoleService -->|Role Data| RoleRepo
    RoleRepo -->|RBAC Data| UserDB
    
    AuthService -->|Password Reset| EmailService
    UserService -->|User Actions| AuditLog
```

### **User Service Components**

**Auth Controller**
- Handles login, logout, and token refresh endpoints
- Validates user credentials and manages authentication flow
- Integrates with JWT service for token generation

**User Service Core**
- Manages user profiles, preferences, and account settings
- Implements password policies and security requirements
- Handles user lifecycle operations (create, update, deactivate)

**Role Service Core**
- Manages role-based access control (RBAC)
- Defines permissions and role hierarchies
- Supports dynamic role assignment and inheritance

---

## **Customer Service Component Flow (:7008)**

```mermaid
flowchart TD
    subgraph "API Layer"
        CustomerController[Customer Controller]
        OrderController[Order Controller]
        ContractController[Contract Controller]
        ContactController[Contact Controller]
    end
    
    subgraph "Core Layer"
        CustomerService[Customer Service]
        OrderService[Order Service]
        ContractService[Contract Service]
        ContactService[Contact Service]
        ValidationService[Validation Service]
    end
    
    subgraph "Infrastructure Layer"
        CustomerRepo[Customer Repository]
        OrderRepo[Order Repository]
        ContractRepo[Contract Repository]
        ContactRepo[Contact Repository]
        CustomerDB[(Customer Database)]
    end
    
    subgraph "External Dependencies"
        ProductSvc[Product Service :7005]
        TransporterSvc[Transporter Service :7010]
        EventBus[Event Bus]
        CreditCheck[Credit Check Service]
    end

    CustomerController -->|Customer CRUD| CustomerService
    CustomerService -->|Validate Data| ValidationService
    CustomerService -->|Store Data| CustomerRepo
    CustomerRepo -->|Persist| CustomerDB
    
    OrderController -->|Order Management| OrderService
    OrderService -->|Product Validation| ProductSvc
    OrderService -->|Transport Assignment| TransporterSvc
    OrderService -->|Order Events| EventBus
    
    ContractController -->|Contract Management| ContractService
    ContractService -->|Credit Validation| CreditCheck
    ContractService -->|Contract Data| ContractRepo
    
    ContactController -->|Contact Management| ContactService
    ContactService -->|Contact Data| ContactRepo
```

### **Customer Service Components**

**Customer Service Core**
- Manages customer profiles, business information, and relationships
- Handles customer classification (customer/supplier/both)
- Implements customer lifecycle and status management

**Order Service Core**
- Processes customer orders and requirements
- Validates product availability and specifications
- Coordinates with transport services for delivery planning

**Contract Service Core**
- Manages customer contracts and agreements
- Handles pricing, terms, and credit arrangements
- Monitors contract compliance and renewals

---

## **Vehicle Service Component Flow (:7003)**

```mermaid
flowchart TD
    subgraph "API Layer"
        VehicleController[Vehicle Controller]
        RegistrationController[Registration Controller]
        MaintenanceController[Maintenance Controller]
        InspectionController[Inspection Controller]
    end
    
    subgraph "Core Layer"
        VehicleService[Vehicle Service]
        RegistrationService[Registration Service]
        MaintenanceService[Maintenance Service]
        InspectionService[Inspection Service]
        ComplianceService[Compliance Service]
    end
    
    subgraph "Infrastructure Layer"
        VehicleRepo[Vehicle Repository]
        RegistrationRepo[Registration Repository]
        MaintenanceRepo[Maintenance Repository]
        InspectionRepo[Inspection Repository]
        VehicleDB[(Vehicle Database)]
    end
    
    subgraph "External Dependencies"
        SACCOSvc[SACCO Service :7011]
        TransporterSvc[Transporter Service :7010]
        ComplianceAPI[Compliance API]
        QRGenerator[QR Code Generator]
    end

    VehicleController -->|Vehicle CRUD| VehicleService
    VehicleService -->|SACCO Validation| SACCOSvc
    VehicleService -->|Owner Validation| TransporterSvc
    VehicleService -->|Generate QR| QRGenerator
    
    RegistrationController -->|Registration Management| RegistrationService
    RegistrationService -->|Compliance Check| ComplianceAPI
    RegistrationService -->|Registration Data| RegistrationRepo
    
    MaintenanceController -->|Maintenance Tracking| MaintenanceService
    MaintenanceService -->|Maintenance Records| MaintenanceRepo
    
    InspectionController -->|Inspection Management| InspectionService
    InspectionService -->|Compliance Validation| ComplianceService
    InspectionService -->|Inspection Data| InspectionRepo
    
    VehicleRepo -->|Store| VehicleDB
    RegistrationRepo -->|Store| VehicleDB
    MaintenanceRepo -->|Store| VehicleDB
    InspectionRepo -->|Store| VehicleDB
```

### **Vehicle Service Components**

**Vehicle Service Core**
- Manages vehicle registration, specifications, and ownership
- Handles vehicle lifecycle from registration to decommission
- Integrates with SACCO and Transporter services for validation

**Registration Service Core**
- Processes vehicle registration and documentation
- Validates compliance with regulatory requirements
- Generates QR codes for vehicle identification

**Maintenance Service Core**
- Tracks vehicle maintenance schedules and history
- Manages service records and warranty information
- Alerts for upcoming maintenance requirements

**Inspection Service Core**
- Manages vehicle inspections and certifications
- Tracks compliance with safety and regulatory standards
- Handles inspection scheduling and results

---

## **Driver Service Component Flow (:7004)**

```mermaid
flowchart TD
    subgraph "API Layer"
        DriverController[Driver Controller]
        LicenseController[License Controller]
        CertificationController[Certification Controller]
        PerformanceController[Performance Controller]
    end
    
    subgraph "Core Layer"
        DriverService[Driver Service]
        LicenseService[License Service]
        CertificationService[Certification Service]
        PerformanceService[Performance Service]
        ValidationService[Validation Service]
    end
    
    subgraph "Infrastructure Layer"
        DriverRepo[Driver Repository]
        LicenseRepo[License Repository]
        CertificationRepo[Certification Repository]
        PerformanceRepo[Performance Repository]
        DriverDB[(Driver Database)]
    end
    
    subgraph "External Dependencies"
        SACCOSvc[SACCO Service :7011]
        TransporterSvc[Transporter Service :7010]
        LicenseAPI[License Validation API]
        BiometricSvc[Biometric Service]
    end

    DriverController -->|Driver CRUD| DriverService
    DriverService -->|SACCO Membership| SACCOSvc
    DriverService -->|Employment Check| TransporterSvc
    DriverService -->|Biometric Data| BiometricSvc
    
    LicenseController -->|License Management| LicenseService
    LicenseService -->|License Validation| LicenseAPI
    LicenseService -->|License Data| LicenseRepo
    
    CertificationController -->|Certification Management| CertificationService
    CertificationService -->|Certification Data| CertificationRepo
    
    PerformanceController -->|Performance Tracking| PerformanceService
    PerformanceService -->|Performance Data| PerformanceRepo
    
    DriverRepo -->|Store| DriverDB
    LicenseRepo -->|Store| DriverDB
    CertificationRepo -->|Store| DriverDB
    PerformanceRepo -->|Store| DriverDB
```

### **Driver Service Components**

**Driver Service Core**
- Manages driver profiles, employment history, and qualifications
- Handles driver lifecycle and status management
- Integrates biometric data for identity verification

**License Service Core**
- Manages driver licenses and endorsements
- Validates license status with external authorities
- Tracks license renewals and violations

**Certification Service Core**
- Manages driver certifications and training records
- Tracks specialized endorsements and qualifications
- Handles certification renewals and compliance

**Performance Service Core**
- Tracks driver performance metrics and ratings
- Monitors safety records and violations
- Provides performance analytics and reporting

---

## **Product Service Component Flow (:7005)**

```mermaid
flowchart TD
    subgraph "API Layer"
        ProductController[Product Controller]
        CategoryController[Category Controller]
        PricingController[Pricing Controller]
        SpecificationController[Specification Controller]
    end
    
    subgraph "Core Layer"
        ProductService[Product Service]
        CategoryService[Category Service]
        PricingService[Pricing Service]
        SpecificationService[Specification Service]
        QualityService[Quality Service]
    end
    
    subgraph "Infrastructure Layer"
        ProductRepo[Product Repository]
        CategoryRepo[Category Repository]
        PricingRepo[Pricing Repository]
        SpecificationRepo[Specification Repository]
        ProductDB[(Product Database)]
    end
    
    subgraph "External Dependencies"
        SupplierSvc[Supplier Service :7009]
        ComplianceSvc[Compliance Service]
        InventoryAPI[Inventory API]
        QualityAPI[Quality Standards API]
    end

    ProductController -->|Product CRUD| ProductService
    ProductService -->|Supplier Validation| SupplierSvc
    ProductService -->|Quality Standards| QualityAPI
    ProductService -->|Inventory Check| InventoryAPI
    
    CategoryController -->|Category Management| CategoryService
    CategoryService -->|Category Data| CategoryRepo
    
    PricingController -->|Pricing Management| PricingService
    PricingService -->|Pricing Data| PricingRepo
    
    SpecificationController -->|Specification Management| SpecificationService
    SpecificationService -->|Compliance Check| ComplianceSvc
    SpecificationService -->|Specification Data| SpecificationRepo
    
    ProductRepo -->|Store| ProductDB
    CategoryRepo -->|Store| ProductDB
    PricingRepo -->|Store| ProductDB
    SpecificationRepo -->|Store| ProductDB
```

### **Product Service Components**

**Product Service Core**
- Manages product catalog, SKUs, and classifications
- Handles product lifecycle from creation to discontinuation
- Integrates with suppliers for product sourcing

**Category Service Core**
- Manages product categories and hierarchies
- Handles category-specific rules and attributes
- Supports dynamic categorization and tagging

**Pricing Service Core**
- Manages product pricing and discount structures
- Handles tiered pricing and customer-specific rates
- Supports dynamic pricing based on market conditions

**Specification Service Core**
- Manages detailed product specifications and attributes
- Handles quality standards and compliance requirements
- Supports technical documentation and certifications

---

## **Route Service Component Flow (:7006)**

```mermaid
flowchart TD
    subgraph "API Layer"
        RouteController[Route Controller]
        OptimizationController[Optimization Controller]
        GateController[Gate Controller]
        TimingController[Timing Controller]
    end
    
    subgraph "Core Layer"
        RouteService[Route Service]
        OptimizationService[Optimization Service]
        GateService[Gate Service]
        TimingService[Timing Service]
        ValidationService[Validation Service]
    end
    
    subgraph "Infrastructure Layer"
        RouteRepo[Route Repository]
        OptimizationRepo[Optimization Repository]
        GateRepo[Gate Repository]
        TimingRepo[Timing Repository]
        RouteDB[(Route Database)]
    end
    
    subgraph "External Dependencies"
        WeighbridgeSvc[Weighbridge Service :7007]
        OrganizationSvc[Organization Service :7002]
        MapsAPI[Maps API]
        TrafficAPI[Traffic API]
    end

    RouteController -->|Route CRUD| RouteService
    RouteService -->|Gate Validation| WeighbridgeSvc
    RouteService -->|Plant Validation| OrganizationSvc
    RouteService -->|Route Mapping| MapsAPI
    
    OptimizationController -->|Route Optimization| OptimizationService
    OptimizationService -->|Traffic Data| TrafficAPI
    OptimizationService -->|Optimization Data| OptimizationRepo
    
    GateController -->|Gate Management| GateService
    GateService -->|Gate Data| GateRepo
    
    TimingController -->|Timing Management| TimingService
    TimingService -->|Timing Data| TimingRepo
    
    RouteRepo -->|Store| RouteDB
    OptimizationRepo -->|Store| RouteDB
    GateRepo -->|Store| RouteDB
    TimingRepo -->|Store| RouteDB
```

### **Route Service Components**

**Route Service Core**
- Manages transport routes between plants and facilities
- Handles route definitions, waypoints, and constraints
- Integrates with mapping services for route validation

**Optimization Service Core**
- Optimizes routes for efficiency, cost, and time
- Considers traffic patterns, vehicle constraints, and delivery windows
- Provides alternative route suggestions

**Gate Service Core**
- Manages gate assignments and access control
- Handles gate-specific rules and restrictions
- Coordinates with weighbridge services for gate operations

**Timing Service Core**
- Tracks expected vs actual transit times
- Manages delivery schedules and time windows
- Provides timing analytics and performance metrics

---

## **Weighbridge Service Component Flow (:7007)**

```mermaid
flowchart TD
    subgraph "API Layer"
        WeighbridgeController[Weighbridge Controller]
        CalibrationController[Calibration Controller]
        MaintenanceController[Maintenance Controller]
        ConfigController[Config Controller]
    end
    
    subgraph "Core Layer"
        WeighbridgeService[Weighbridge Service]
        CalibrationService[Calibration Service]
        MaintenanceService[Maintenance Service]
        ConfigService[Config Service]
        ValidationService[Validation Service]
    end
    
    subgraph "Infrastructure Layer"
        WeighbridgeRepo[Weighbridge Repository]
        CalibrationRepo[Calibration Repository]
        MaintenanceRepo[Maintenance Repository]
        ConfigRepo[Config Repository]
        WeighbridgeDB[(Weighbridge Database)]
    end
    
    subgraph "External Dependencies"
        OrganizationSvc[Organization Service :7002]
        HardwareAPI[Hardware API]
        CalibrationAPI[Calibration API]
        ComplianceAPI[Compliance API]
    end

    WeighbridgeController -->|Weighbridge CRUD| WeighbridgeService
    WeighbridgeService -->|Location Validation| OrganizationSvc
    WeighbridgeService -->|Hardware Integration| HardwareAPI
    WeighbridgeService -->|Compliance Check| ComplianceAPI
    
    CalibrationController -->|Calibration Management| CalibrationService
    CalibrationService -->|Calibration API| CalibrationAPI
    CalibrationService -->|Calibration Data| CalibrationRepo
    
    MaintenanceController -->|Maintenance Tracking| MaintenanceService
    MaintenanceService -->|Maintenance Data| MaintenanceRepo
    
    ConfigController -->|Configuration Management| ConfigService
    ConfigService -->|Config Data| ConfigRepo
    
    WeighbridgeRepo -->|Store| WeighbridgeDB
    CalibrationRepo -->|Store| WeighbridgeDB
    MaintenanceRepo -->|Store| WeighbridgeDB
    ConfigRepo -->|Store| WeighbridgeDB
```

### **Weighbridge Service Components**

**Weighbridge Service Core**
- Manages weighbridge equipment and specifications
- Handles weighbridge registration and certification
- Integrates with hardware systems for operational control

**Calibration Service Core**
- Manages weighbridge calibration schedules and procedures
- Tracks calibration history and compliance
- Integrates with external calibration services

**Maintenance Service Core**
- Tracks weighbridge maintenance and repairs
- Manages preventive maintenance schedules
- Handles equipment lifecycle and replacement planning

**Config Service Core**
- Manages weighbridge configuration and settings
- Handles operational parameters and thresholds
- Supports remote configuration management

---

## **Supplier Service Component Flow (:7009)**

```mermaid
flowchart TD
    subgraph "API Layer"
        SupplierController[Supplier Controller]
        ProcurementController[Procurement Controller]
        ContractController[Contract Controller]
        PerformanceController[Performance Controller]
    end
    
    subgraph "Core Layer"
        SupplierService[Supplier Service]
        ProcurementService[Procurement Service]
        ContractService[Contract Service]
        PerformanceService[Performance Service]
        ValidationService[Validation Service]
    end
    
    subgraph "Infrastructure Layer"
        SupplierRepo[Supplier Repository]
        ProcurementRepo[Procurement Repository]
        ContractRepo[Contract Repository]
        PerformanceRepo[Performance Repository]
        SupplierDB[(Supplier Database)]
    end
    
    subgraph "External Dependencies"
        ProductSvc[Product Service :7005]
        CustomerSvc[Customer Service :7008]
        ComplianceAPI[Compliance API]
        CreditAPI[Credit Check API]
    end

    SupplierController -->|Supplier CRUD| SupplierService
    SupplierService -->|Product Validation| ProductSvc
    SupplierService -->|Customer Relationship| CustomerSvc
    SupplierService -->|Credit Check| CreditAPI
    
    ProcurementController -->|Procurement Management| ProcurementService
    ProcurementService -->|Procurement Data| ProcurementRepo
    
    ContractController -->|Contract Management| ContractService
    ContractService -->|Compliance Check| ComplianceAPI
    ContractService -->|Contract Data| ContractRepo
    
    PerformanceController -->|Performance Tracking| PerformanceService
    PerformanceService -->|Performance Data| PerformanceRepo
    
    SupplierRepo -->|Store| SupplierDB
    ProcurementRepo -->|Store| SupplierDB
    ContractRepo -->|Store| SupplierDB
    PerformanceRepo -->|Store| SupplierDB
```

### **Supplier Service Components**

**Supplier Service Core**
- Manages supplier profiles, capabilities, and relationships
- Handles supplier onboarding and qualification processes
- Integrates with customer service for dual-role entities

**Procurement Service Core**
- Manages procurement processes and purchase orders
- Handles supplier selection and negotiation
- Tracks procurement performance and costs

**Contract Service Core**
- Manages supplier contracts and agreements
- Handles terms, conditions, and compliance requirements
- Monitors contract performance and renewals

**Performance Service Core**
- Tracks supplier performance metrics and KPIs
- Monitors delivery performance, quality, and compliance
- Provides supplier scorecards and analytics

---

## **Transporter Service Component Flow (:7010)**

```mermaid
flowchart TD
    subgraph "API Layer"
        TransporterController[Transporter Controller]
        FleetController[Fleet Controller]
        CapacityController[Capacity Controller]
        AssignmentController[Assignment Controller]
    end
    
    subgraph "Core Layer"
        TransporterService[Transporter Service]
        FleetService[Fleet Service]
        CapacityService[Capacity Service]
        AssignmentService[Assignment Service]
        ValidationService[Validation Service]
    end
    
    subgraph "Infrastructure Layer"
        TransporterRepo[Transporter Repository]
        FleetRepo[Fleet Repository]
        CapacityRepo[Capacity Repository]
        AssignmentRepo[Assignment Repository]
        TransporterDB[(Transporter Database)]
    end
    
    subgraph "External Dependencies"
        VehicleSvc[Vehicle Service :7003]
        DriverSvc[Driver Service :7004]
        SACCOSvc[SACCO Service :7011]
        CustomerSvc[Customer Service :7008]
    end

    TransporterController -->|Transporter CRUD| TransporterService
    TransporterService -->|Customer Relationship| CustomerSvc
    TransporterService -->|SACCO Validation| SACCOSvc
    
    FleetController -->|Fleet Management| FleetService
    FleetService -->|Vehicle Validation| VehicleSvc
    FleetService -->|Driver Validation| DriverSvc
    FleetService -->|Fleet Data| FleetRepo
    
    CapacityController -->|Capacity Management| CapacityService
    CapacityService -->|Capacity Data| CapacityRepo
    
    AssignmentController -->|Assignment Management| AssignmentService
    AssignmentService -->|Assignment Data| AssignmentRepo
    
    TransporterRepo -->|Store| TransporterDB
    FleetRepo -->|Store| TransporterDB
    CapacityRepo -->|Store| TransporterDB
    AssignmentRepo -->|Store| TransporterDB
```

### **Transporter Service Components**

**Transporter Service Core**
- Manages transporter company profiles and business information
- Handles transporter registration and licensing
- Supports dual-role entities (customer-transporter)

**Fleet Service Core**
- Manages transporter fleet composition and capabilities
- Handles vehicle and driver assignments
- Tracks fleet utilization and performance

**Capacity Service Core**
- Manages transporter capacity planning and availability
- Handles capacity reservations and scheduling
- Provides capacity optimization recommendations

**Assignment Service Core**
- Manages transport assignments and job allocation
- Handles route assignments and scheduling
- Tracks assignment performance and completion

---

## **SACCO Service Component Flow (:7011)**

```mermaid
flowchart TD
    subgraph "API Layer"
        SACCOController[SACCO Controller]
        MembershipController[Membership Controller]
        GovernanceController[Governance Controller]
        FinancialController[Financial Controller]
    end
    
    subgraph "Core Layer"
        SACCOService[SACCO Service]
        MembershipService[Membership Service]
        GovernanceService[Governance Service]
        FinancialService[Financial Service]
        ValidationService[Validation Service]
    end
    
    subgraph "Infrastructure Layer"
        SACCORepo[SACCO Repository]
        MembershipRepo[Membership Repository]
        GovernanceRepo[Governance Repository]
        FinancialRepo[Financial Repository]
        SACCODB[(SACCO Database)]
    end
    
    subgraph "External Dependencies"
        VehicleSvc[Vehicle Service :7003]
        DriverSvc[Driver Service :7004]
        TransporterSvc[Transporter Service :7010]
        RegulatoryAPI[Regulatory API]
    end

    SACCOController -->|SACCO CRUD| SACCOService
    SACCOService -->|Regulatory Check| RegulatoryAPI
    
    MembershipController -->|Membership Management| MembershipService
    MembershipService -->|Vehicle Validation| VehicleSvc
    MembershipService -->|Driver Validation| DriverSvc
    MembershipService -->|Transporter Validation| TransporterSvc
    MembershipService -->|Membership Data| MembershipRepo
    
    GovernanceController -->|Governance Management| GovernanceService
    GovernanceService -->|Governance Data| GovernanceRepo
    
    FinancialController -->|Financial Management| FinancialService
    FinancialService -->|Financial Data| FinancialRepo
    
    SACCORepo -->|Store| SACCODB
    MembershipRepo -->|Store| SACCODB
    GovernanceRepo -->|Store| SACCODB
    FinancialRepo -->|Store| SACCODB
```

### **SACCO Service Components**

**SACCO Service Core**
- Manages SACCO organization profiles and registration
- Handles SACCO licensing and regulatory compliance
- Manages SACCO operational parameters and rules

**Membership Service Core**
- Manages SACCO membership for vehicles, drivers, and transporters
- Handles membership applications and renewals
- Tracks membership benefits and obligations

**Governance Service Core**
- Manages SACCO governance structure and policies
- Handles leadership and committee management
- Tracks governance compliance and reporting

**Financial Service Core**
- Manages SACCO financial operations and accounts
- Handles member contributions and benefits
- Provides financial reporting and analytics

---

## **Organization Service Component Flow (:7002)**

```mermaid
flowchart TD
    subgraph "API Layer"
        OrganizationController[Organization Controller]
        TenantController[Tenant Controller]
        LocationController[Location Controller]
        HierarchyController[Hierarchy Controller]
    end
    
    subgraph "Core Layer"
        OrganizationService[Organization Service]
        TenantService[Tenant Service]
        LocationService[Location Service]
        HierarchyService[Hierarchy Service]
        ValidationService[Validation Service]
    end
    
    subgraph "Infrastructure Layer"
        OrganizationRepo[Organization Repository]
        TenantRepo[Tenant Repository]
        LocationRepo[Location Repository]
        HierarchyRepo[Hierarchy Repository]
        OrganizationDB[(Organization Database)]
    end
    
    subgraph "External Dependencies"
        UserSvc[User Service :7001]
        WeighbridgeSvc[Weighbridge Service :7007]
        GeoAPI[Geolocation API]
        ComplianceAPI[Compliance API]
    end

    OrganizationController -->|Organization CRUD| OrganizationService
    OrganizationService -->|User Validation| UserSvc
    OrganizationService -->|Compliance Check| ComplianceAPI
    
    TenantController -->|Tenant Management| TenantService
    TenantService -->|Tenant Data| TenantRepo
    
    LocationController -->|Location Management| LocationService
    LocationService -->|Geolocation| GeoAPI
    LocationService -->|Weighbridge Integration| WeighbridgeSvc
    LocationService -->|Location Data| LocationRepo
    
    HierarchyController -->|Hierarchy Management| HierarchyService
    HierarchyService -->|Hierarchy Data| HierarchyRepo
    
    OrganizationRepo -->|Store| OrganizationDB
    TenantRepo -->|Store| OrganizationDB
    LocationRepo -->|Store| OrganizationDB
    HierarchyRepo -->|Store| OrganizationDB
```

### **Organization Service Components**

**Organization Service Core**
- Manages organization profiles and business information
- Handles organization registration and licensing
- Provides multi-tenant foundation for the system

**Tenant Service Core**
- Manages tenant isolation and data segregation
- Handles tenant-specific configurations and settings
- Provides tenant context for all operations

**Location Service Core**
- Manages organization locations and facilities
- Handles plant, warehouse, and office location data
- Integrates with weighbridge services for operational locations

**Hierarchy Service Core**
- Manages organizational hierarchy and reporting structures
- Handles department and division management
- Supports complex organizational relationships

---

**Navigation**: [← Main Component Flows](03-component-flows.md) | [← Infrastructure Components](03-component-flows-infrastructure.md) | [Data Manager Components →](03-component-flows-datamanager.md)