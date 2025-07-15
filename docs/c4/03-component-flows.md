# C4 Level 3: Component Flows

## 🔧 **QaliTrack Component Interactions**

This diagram shows the detailed interactions between services, components, and how data flows through the QaliTrack system during key business processes.

### **Component Overview**
The system is built around event-driven microservices with clear data flow patterns, authentication boundaries, and business process orchestration.

## 🔄 **Authentication & Authorization Flow**

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
        Operator[Operator Web Portal]
        Kiosk[Self-Service Kiosk]
    end
    
    subgraph "Gateway Layer"
        Gateway[API Gateway :7000]
    end
    
    subgraph "Core Services"
        VehicleSvc[Vehicle Service :7003]
        WeightSvc[Weight Data Service]
        TransactionSvc[Transaction Service]
    end
    
    subgraph "Infrastructure"
        PostgreSQL[(PostgreSQL with TimescaleDB)]
        Hardware[Weighbridge Hardware]
    end

    %% Simplified flow paths
    Driver -->|1. Vehicle Registration| Gateway
    Gateway -->|2. Validate Vehicle| VehicleSvc
    Hardware -->|3. Capture Weight| WeightSvc
    
    %% Manned operations
    Operator -->|4a. Create Transaction - Manned| Gateway
    
    %% Unmanned operations  
    Kiosk -->|4b. Create Transaction - Unmanned| Gateway
    Kiosk -->|Face Detection Auth| Gateway
    
    %% Common transaction processing
    Gateway -->|5. Process Transaction| TransactionSvc
    TransactionSvc -->|6. Store & Notify| PostgreSQL
    
    %% Data storage
    VehicleSvc -->|Vehicle Data| PostgreSQL
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
    class VehicleSvc,WeightSvc,TransactionSvc services
    class PostgreSQL,Hardware infrastructure
```

### **🔄 Manned vs Unmanned Operations**

The transaction flow supports both manned and unmanned weighing operations:

**Manned Operations (4a):**
- Operator uses web portal to create transactions
- Human oversight and validation
- Real-time operator assistance
- Manual intervention capabilities

**Unmanned Operations (4b):**
- Self-service kiosk with face detection authentication
- Automated transaction creation
- Touch interface for driver interaction
- Automatic receipt generation

Both paths converge at the Gateway for consistent transaction processing, ensuring the same data integrity and compliance standards regardless of operation mode.

## 🔗 **masterdata to masterdata Integration Patterns**

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

    Customer->>Product: 1. Validate ProductId
    Product->>Product: Check product catalog
    Product->>Customer: Product details & pricing
    
    Customer->>Supplier: 2. Check supplier availability
    Supplier->>Supplier: Verify supplier capacity
    Supplier->>Customer: Supplier confirmation
    
    Customer->>Transaction: 3. Create order transaction
    Transaction->>Transaction: Generate transaction ID
    Transaction->>Events: Publish OrderCreated event
    
    Events->>Compliance: 4. Compliance validation
    Compliance->>Product: Check product regulations
    Compliance->>Supplier: Check supplier licenses
    Compliance->>Events: Publish ComplianceValidated event
    
    Events->>Customer: 5. Order confirmation
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

    Transporter->>Vehicle: 1. Check vehicle availability
    Vehicle->>SACCO: Verify SACCO registration
    SACCO->>Vehicle: SACCO membership status
    Vehicle->>Transporter: Available vehicles list
    
    Transporter->>Driver: 2. Check driver availability
    Driver->>SACCO: Verify driver membership
    Driver->>Driver: Check license validity
    Driver->>Transporter: Available drivers list
    
    Transporter->>Route: 3. Optimize route assignment
    Route->>Route: Calculate optimal route
    Route->>Transporter: Route recommendations
    
    Transporter->>Transaction: 4. Create transport assignment
    Transaction->>Vehicle: Lock vehicle assignment
    Transaction->>Driver: Lock driver assignment
    Transaction->>Route: Lock route assignment
    Transaction->>Transporter: Assignment confirmation
```

## 📱 **Real-Time Data Processing Components**

### **Weight Data Processing Pipeline**

```mermaid
flowchart LR
    subgraph "Hardware Layer"
        LoadCells[Load Cells]
        Sensors[Weight Sensors]
    end
    
    subgraph "Data Acquisition"
        HardwareInterface[Hardware Interface]
        DataBuffer[Data Buffer]
        Validator[Data Validator]
    end
    
    subgraph "Weight Data Service"
        Processor[Weight Processor]
        Calibrator[Calibration Engine]
        Publisher[Event Publisher]
    end
    
    subgraph "Storage Layer"
        PostgreSQL[(PostgreSQL with TimescaleDB)]
        Kafka[Kafka Stream]
        Redis[(Redis Cache)]
    end
    
    subgraph "Consumer Services"
        TransactionSvc[Transaction Service]
        AnalyticsSvc[Analytics Service]
        ComplianceSvc[Compliance Service]
    end

    LoadCells -->|Raw Signals| HardwareInterface
    Sensors -->|Digital Data| HardwareInterface
    HardwareInterface -->|Buffered Data| DataBuffer
    DataBuffer -->|Validated Data| Validator
    
    Validator -->|Clean Data| Processor
    Processor -->|Calibrated Weight| Calibrator
    Calibrator -->|Accurate Weight| Publisher
    
    Publisher -->|Time-Series Data| PostgreSQL
    Publisher -->|Weight Events| Kafka
    Publisher -->|Current Weight| Redis
    
    Kafka -->|Real-Time Events| TransactionSvc
    Kafka -->|Analytics Events| AnalyticsSvc
    Kafka -->|Compliance Events| ComplianceSvc
```

### **Event-Driven Architecture Components**

```mermaid
flowchart TD
    subgraph "Event Producers"
        WeightEvents[Weight Data Events]
        TransactionEvents[Transaction Events]
        ComplianceEvents[Compliance Events]
        UserEvents[User Activity Events]
    end
    
    subgraph "Message Streaming"
        Kafka[Apache Kafka Cluster]
        Topics[Event Topics]
    end
    
    subgraph "Event Consumers"
        Analytics[Analytics Service]
        Compliance[Compliance Service]
        DataSync[Data Sync Service]
        Archive[Archive Service]
        Notifications[Notification Service]
    end
    
    subgraph "Event Processing"
        StreamProcessor[Stream Processor]
        EventStore[Event Store]
        Replay[Event Replay]
    end

    WeightEvents -->|Weight measurements| Kafka
    TransactionEvents -->|Business transactions| Kafka
    ComplianceEvents -->|Regulatory events| Kafka
    UserEvents -->|User actions| Kafka
    
    Kafka -->|Real-time streams| Topics
    Topics -->|Event consumption| Analytics
    Topics -->|Compliance monitoring| Compliance
    Topics -->|Multi-site sync| DataSync
    Topics -->|Long-term storage| Archive
    Topics -->|User notifications| Notifications
    
    Topics -->|Stream processing| StreamProcessor
    StreamProcessor -->|Processed events| EventStore
    EventStore -->|Historical replay| Replay
```

## 🔐 **Security Component Interactions**

### **Role-Based Access Control (RBAC) Flow**

```mermaid
flowchart TD
    subgraph "Authentication Components"
        JWTValidator[JWT Validator]
        TokenCache[Token Cache]
        UserContext[User Context Builder]
    end
    
    subgraph "Authorization Components"
        RoleResolver[Role Resolver]
        PermissionEngine[Permission Engine]
        ResourceGuard[Resource Guard]
    end
    
    subgraph "Service Components"
        ServiceAuth[Service Authorization]
        BusinessLogic[Business Logic]
        DataAccess[Data Access Layer]
    end
    
    subgraph "User Store"
        UserDB[(User Database)]
        RoleDB[(Role Database)]
        PermissionDB[(Permission Database)]
    end

    Request[Incoming Request] -->|JWT Token| JWTValidator
    JWTValidator -->|Valid Token| TokenCache
    TokenCache -->|User Info| UserContext
    
    UserContext -->|User Roles| RoleResolver
    RoleResolver -->|Role Details| UserDB
    RoleResolver -->|Role Permissions| PermissionEngine
    
    PermissionEngine -->|Permission Check| ResourceGuard
    ResourceGuard -->|Authorized Request| ServiceAuth
    
    ServiceAuth -->|Validated Request| BusinessLogic
    BusinessLogic -->|Data Operations| DataAccess
    DataAccess -->|Query Results| UserDB
    DataAccess -->|Filtered Data| BusinessLogic
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

## 🔄 **Service Discovery & Health Monitoring**

```mermaid
flowchart TD
    subgraph "Service Instances"
        Service1[Service Instance 1]
        Service2[Service Instance 2]
        Service3[Service Instance 3]
    end
    
    subgraph "Service Discovery"
        Registry[Service Registry]
        HealthChecker[Health Checker]
        LoadBalancer[Load Balancer]
    end
    
    subgraph "Monitoring"
        HealthEndpoints[Health Endpoints]
        Metrics[Metrics Collector]
        Alerts[Alert Manager]
    end
    
    subgraph "Client Layer"
        Gateway[API Gateway]
        ServiceClient[Service Client]
    end

    Service1 -->|Register| Registry
    Service2 -->|Register| Registry
    Service3 -->|Register| Registry
    
    HealthChecker -->|Health Check| Service1
    HealthChecker -->|Health Check| Service2
    HealthChecker -->|Health Check| Service3
    
    Service1 -->|Expose| HealthEndpoints
    Service2 -->|Expose| HealthEndpoints
    Service3 -->|Expose| HealthEndpoints
    
    HealthEndpoints -->|Collect| Metrics
    Metrics -->|Trigger| Alerts
    
    Registry -->|Service Discovery| LoadBalancer
    LoadBalancer -->|Route Requests| Gateway
    Gateway -->|Service Calls| ServiceClient
```

## 🔧 **Error Handling & Resilience Patterns**

### **Circuit Breaker Pattern**

```mermaid
stateDiagram-v2
    [*] --> Closed: Initial State
    
    Closed --> Open: Failure threshold exceeded
    Closed --> Closed: Successful requests
    
    Open --> HalfOpen: Timeout period elapsed
    Open --> Open: Requests fail fast
    
    HalfOpen --> Closed: Test request succeeds
    HalfOpen --> Open: Test request fails
    
    note right of Closed
        Normal operation
        All requests pass through
    end note
    
    note right of Open
        Circuit breaker active
        Requests fail immediately
    end note
    
    note right of HalfOpen
        Testing recovery
        Limited requests allowed
    end note
```

### **Retry & Timeout Strategy**

```mermaid
sequenceDiagram
    participant Client as Service Client
    participant CircuitBreaker as Circuit Breaker
    participant Target as Target Service
    participant Fallback as Fallback Handler

    Client->>CircuitBreaker: Service request
    
    alt Circuit Closed
        CircuitBreaker->>Target: Forward request
        alt Request succeeds
            Target->>CircuitBreaker: Success response
            CircuitBreaker->>Client: Return response
        else Request fails
            Target->>CircuitBreaker: Error response
            CircuitBreaker->>CircuitBreaker: Increment failure count
            alt Retry available
                Note over CircuitBreaker: Exponential backoff
                CircuitBreaker->>Target: Retry request
            else Max retries exceeded
                CircuitBreaker->>CircuitBreaker: Open circuit
                CircuitBreaker->>Fallback: Invoke fallback
                Fallback->>Client: Fallback response
            end
        end
    else Circuit Open
        CircuitBreaker->>Fallback: Invoke fallback
        Fallback->>Client: Fallback response
    end
```

---

**Previous Level**: [← Container Architecture](02-container-architecture.md) | **Next Level**: [Service Architectures →](04-service-architectures.md)