# C4 Level 3: Component Flows

## 🔧 **QaliTrack Component Interactions**

This document provides detailed component-level interactions for all QaliTrack services, showing how data flows through the system during key business processes. Due to the comprehensive nature of the system (13 Master Data + 7 Data Manager + Infrastructure services), this documentation is organized into focused sections with cross-references.

### **Navigation**
- [🏗️ Infrastructure Components](03-component-flows-infrastructure.md) - API Gateway, Service Discovery, Cache, Messaging
- [👥 Master Data Service Components](03-component-flows-masterdata.md) - All 13 Master Data Services
- [📊 Data Manager Service Components](03-component-flows-datamanager.md) - All 7 Data Manager Services
- [🔄 Cross-Service Integration Patterns](#-cross-service-integration-patterns)
- [🔐 Security & Authentication Flows](#-security--authentication-flows)

### **Component Overview**
The system is built around event-driven microservices with clear data flow patterns, authentication boundaries, and business process orchestration. Each service follows Clean Architecture principles with API, Core, and Infrastructure layers.

**Service Categories:**
- **Infrastructure Services**: API Gateway, Service Discovery, Cache Layer, Message Streaming, Database
- **Master Data Services (13)**: User, Customer, Vehicle, Driver, Product, Route, Weighbridge, Supplier, Transporter, SACCO, Organization, Report
- **Data Manager Services (7)**: Weight Data, Transaction, Compliance, Analytics, Operational Data, Data Sync, Archive

For detailed component flows of each category, please refer to the dedicated documentation files linked above.

## 🔐 **Security & Authentication Flows**

### **Authentication & Authorization Flow**

```mermaid
sequenceDiagram
    participant Client as Client Application
    participant Gateway as API Gateway
    participant User as User Service
    participant Cache as Redis Cache
    participant Target as Target Service

    Client->>Gateway: Request with JWT Token
    Gateway->>Cache: Check token cache
    alt Token cached and valid
        Cache->>Gateway: Return user context
    else Token not cached or expired
        Gateway->>User: Validate JWT token
        User->>User: Verify signature & expiry
        User->>Gateway: Return user context + permissions
        Gateway->>Cache: Cache user context (TTL: 15 min)
    end
    
    Gateway->>Gateway: Check route permissions
    alt User authorized
        Gateway->>Target: Forward request with user headers
        Note over Gateway,Target: Headers: X-User-ID, X-User-Roles, X-User-Permissions
        Target->>Target: Execute business logic
        Target->>Gateway: Return response
        Gateway->>Client: Return response
    else User not authorized
        Gateway->>Client: 403 Forbidden
    end
```

## 📊 **Complete Weighing Transaction Flow**

```mermaid
flowchart TD
    subgraph "Users"
        Driver[Driver Mobile App]
        
        subgraph "Manned Operations"
            Operator[Operator Web Portal]
        end
        
        subgraph "Unmanned Operations"
            Kiosk[Self-Service Kiosk]
        end
    end
    
    subgraph "Gateway Layer"
        Gateway[API Gateway :7000]
    end
    
    subgraph "Core Services"
        VehicleSvc[Vehicle Service :7003]
        DriverSvc[Driver Service :7004]
        WeightSvc[Weight Data Service]
        TransactionSvc[Transaction Service]
    end
    
    subgraph "Infrastructure"
        PostgreSQL[(PostgreSQL with TimescaleDB)]
        Hardware[Weighbridge Hardware]
        QRReader[QR Code Reader]
        ANPRCamera[ANPR Camera System]
    end

    %% Sequential transaction flow
    QRReader -->|"1: Vehicle QR Detection"| Gateway
    Gateway -->|"QR Data"| VehicleSvc
    ANPRCamera -->|"2: License Plate Recognition"| Gateway
    Gateway -->|"Plate Data"| VehicleSvc
    VehicleSvc -->|"Vehicle Identified"| Gateway
    
    %% Driver and document verification
    %% Manned operations
    Operator -->|"3: Driver Verification • Manned"| Gateway
    Gateway -->|"Verify Driver"| DriverSvc
    
    %% Unmanned operations  
    Kiosk -->|"3: Driver Verification • Unmanned"| Gateway
    Kiosk -->|"3.1: Face Detection Auth"| Gateway
    Kiosk -->|"3.2: Document Verification • QR Code"| Gateway
    Gateway -->|"Verify Driver"| DriverSvc
    
    %% Weight capture after verification
    DriverSvc -->|"Driver Verified"| Gateway
    Hardware -->|"4: Capture Weight"| WeightSvc
    
    %% Transaction processing
    Gateway -->|"5: Process Transaction"| TransactionSvc
    TransactionSvc -->|"6: Store & Notify"| PostgreSQL
    
    %% Data storage
    VehicleSvc -->|Vehicle Data| PostgreSQL
    DriverSvc -->|Driver Data| PostgreSQL
    WeightSvc -->|Weight Data| PostgreSQL
    TransactionSvc -->|Transaction Data| PostgreSQL
    
    %% Responses
    Gateway -->|Success Response| Operator
    Gateway -->|Kiosk Receipt| Kiosk
    Gateway -->|Notification| Driver

    classDef client fill:#e3f2fd
    classDef gateway fill:#fff3e0
    classDef services fill:#f3e5f5
    classDef infrastructure fill:#fce4ec
    
    class Driver,Operator,Kiosk client
    class Gateway gateway
    class VehicleSvc,DriverSvc,WeightSvc,TransactionSvc services
    class PostgreSQL,Hardware,QRReader,ANPRCamera infrastructure
```

### **🔄 Complete Transaction Flow**

The operational transaction flow follows the actual weighbridge sequence:

**Vehicle Identification (Steps 1-2):**
- **Step 1:** QR Code Reader detects vehicle QR code for initial identification
- **Step 2:** ANPR Camera System captures license plate for verification
- Vehicle Service validates vehicle registration and status

**Driver & Document Verification (Step 3):**

**Manned Operations (Step 3):**
- Operator verifies driver credentials via web portal
- Manual document inspection and validation
- Human oversight and intervention capabilities

**Unmanned Operations (Step 3 + 3.1 + 3.2):**
- Self-service kiosk handles driver verification
- **Step 3.1:** Face detection authentication
- **Step 3.2:** Document verification using QR code scanning
- Automated credential validation

**Weight Capture & Processing (Steps 4-6):**
- **Step 4:** Weighbridge hardware captures weight after verification
- **Step 5:** Gateway processes validated transaction
- **Step 6:** Data storage and stakeholder notifications

This sequence ensures complete vehicle and driver verification before any weighing operation, maintaining security and compliance standards.

## 🔄 **Cross-Service Integration Patterns**

### **Customer Order Processing Flow**

```mermaid
sequenceDiagram
    participant Customer as Customer Service
    participant Product as Product Service
    participant Supplier as Supplier Service
    participant Transaction as Transaction Service
    participant Compliance as Compliance Service
    participant Events as Kafka Events

    Note over Customer,Events: End-to-End Order Processing

    Customer->>Product: 1: Validate ProductId
    Product->>Product: Check product catalog
    Product->>Customer: Product details & pricing
    
    Customer->>Supplier: 2: Check supplier availability
    Supplier->>Supplier: Verify supplier capacity
    Supplier->>Customer: Supplier confirmation
    
    Customer->>Transaction: 3: Create order transaction
    Transaction->>Transaction: Generate transaction ID
    Transaction->>Events: Publish OrderCreated event
    
    Events->>Compliance: 4: Compliance validation
    Compliance->>Product: Check product regulations
    Compliance->>Supplier: Check supplier licenses
    Compliance->>Events: Publish ComplianceValidated event
    
    Events->>Customer: 5: Order confirmation
    Customer->>Customer: Update order status
```

### **Vehicle & Driver Assignment Flow**

```mermaid
sequenceDiagram
    participant Transporter as Transporter Service
    participant Vehicle as Vehicle Service
    participant Driver as Driver Service
    participant Route as Route Service
    participant SACCO as SACCO Service
    participant Transaction as Transaction Service

    Note over Transporter,Transaction: Transport Assignment Process

    Transporter->>Vehicle: 1: Check vehicle availability
    Vehicle->>SACCO: Verify SACCO registration
    SACCO->>Vehicle: SACCO membership status
    Vehicle->>Transporter: Available vehicles list
    
    Transporter->>Driver: 2: Check driver availability
    Driver->>SACCO: Verify driver membership
    Driver->>Driver: Check license validity
    Driver->>Transporter: Available drivers list
    
    Transporter->>Route: 3: Optimize route assignment
    Route->>Route: Calculate optimal route
    Route->>Transporter: Route recommendations
    
    Transporter->>Transaction: 4: Create transport assignment
    Transaction->>Vehicle: Lock vehicle assignment
    Transaction->>Driver: Lock driver assignment
    Transaction->>Route: Lock route assignment
    Transaction->>Transporter: Assignment confirmation
```



## 📊 **Data Consistency Patterns**

### **Multi-Service Transaction Pattern**

```mermaid
sequenceDiagram
    participant Client as Client
    participant Transaction as Transaction Service
    participant Customer as Customer Service
    participant Product as Product Service
    participant Vehicle as Vehicle Service
    participant Events as Event Store
    participant Compensation as Compensation Handler

    Client->>Transaction: Create weighing transaction
    Transaction->>Events: Start transaction saga
    
    Transaction->>Customer: Reserve customer allocation
    Customer->>Transaction: Allocation reserved
    Transaction->>Events: Log customer reservation
    
    Transaction->>Product: Reserve product quantity
    Product->>Transaction: Quantity reserved
    Transaction->>Events: Log product reservation
    
    Transaction->>Vehicle: Assign vehicle
    alt Vehicle assignment fails
        Vehicle->>Transaction: Assignment failed
        Transaction->>Compensation: Trigger compensation
        Compensation->>Customer: Release customer allocation
        Compensation->>Product: Release product quantity
        Transaction->>Client: Transaction failed
    else Vehicle assignment succeeds
        Vehicle->>Transaction: Vehicle assigned
        Transaction->>Events: Log vehicle assignment
        Transaction->>Events: Commit transaction saga
        Transaction->>Client: Transaction successful
    end
```

### **Event Sourcing Pattern**

```mermaid
flowchart LR
    subgraph "Command Side"
        Command[Business Command]
        Aggregate[Business Aggregate]
        EventGen[Event Generator]
    end
    
    subgraph "Event Store"
        EventStream[Event Stream]
        Snapshots[Aggregate Snapshots]
    end
    
    subgraph "Query Side"
        EventHandler[Event Handler]
        ReadModel[Read Model]
        Projections[View Projections]
    end
    
    subgraph "Infrastructure"
        Kafka[Event Stream]
        PostgreSQL[(Read Database)]
        Redis[(Query Cache)]
    end

    Command -->|Execute| Aggregate
    Aggregate -->|Generate| EventGen
    EventGen -->|Append| EventStream
    EventStream -->|Snapshot| Snapshots
    
    EventStream -->|Stream Events| Kafka
    Kafka -->|Event Processing| EventHandler
    EventHandler -->|Update| ReadModel
    ReadModel -->|Create| Projections
    
    Projections -->|Store| PostgreSQL
    Projections -->|Cache| Redis
```



---

**Previous Level**: [← Container Architecture](02-container-architecture.md) | **Next Level**: [Service Architectures →](04-service-architectures.md)