# C4 Level 3: Infrastructure Component Flows

## 🏗️ **Infrastructure Components**

**Navigation**: [← Main Component Flows](03-component-flows.md) | **Next**: [Master Data Components →](03-component-flows-masterdata.md)

This document details the component-level interactions for all QaliTrack infrastructure services including API Gateway, Service Discovery, Cache Layer, Message Streaming, and Database components.

---

## **API Gateway Component Flow**

```mermaid
flowchart TD
    subgraph "Client Requests"
        WebApp[Web Portal]
        MobileApp[Mobile App]
        KioskApp[Kiosk App]
        AdminPanel[Admin Panel]
    end
    
    subgraph "API Gateway :7000"
        RequestRouter[Request Router]
        AuthMiddleware[Auth Middleware]
        RateLimiter[Rate Limiter]
        LoadBalancer[Load Balancer]
        ResponseAggregator[Response Aggregator]
    end
    
    subgraph "Service Discovery"
        ServiceRegistry[Service Registry]
        HealthMonitor[Health Monitor]
        ConfigManager[Config Manager]
    end
    
    subgraph "Target Services"
        UserSvc[User Service :7001]
        CustomerSvc[Customer Service :7008]
        VehicleSvc[Vehicle Service :7003]
        WeightSvc[Weight Data Service]
        TransactionSvc[Transaction Service]
    end
    
    subgraph "Infrastructure"
        Redis[(Redis Cache)]
        Metrics[Metrics Collector]
        Logs[Log Aggregator]
    end

    WebApp -->|HTTPS Request| RequestRouter
    MobileApp -->|HTTPS Request| RequestRouter
    KioskApp -->|HTTPS Request| RequestRouter
    AdminPanel -->|HTTPS Request| RequestRouter
    
    RequestRouter -->|Validate| AuthMiddleware
    AuthMiddleware -->|Check Cache| Redis
    AuthMiddleware -->|Rate Check| RateLimiter
    RateLimiter -->|Route Decision| LoadBalancer
    
    LoadBalancer -->|Service Lookup| ServiceRegistry
    ServiceRegistry -->|Health Status| HealthMonitor
    LoadBalancer -->|Forward Request| UserSvc
    LoadBalancer -->|Forward Request| CustomerSvc
    LoadBalancer -->|Forward Request| VehicleSvc
    LoadBalancer -->|Forward Request| WeightSvc
    LoadBalancer -->|Forward Request| TransactionSvc
    
    UserSvc -->|Response| ResponseAggregator
    CustomerSvc -->|Response| ResponseAggregator
    VehicleSvc -->|Response| ResponseAggregator
    WeightSvc -->|Response| ResponseAggregator
    TransactionSvc -->|Response| ResponseAggregator
    
    ResponseAggregator -->|Metrics| Metrics
    ResponseAggregator -->|Logs| Logs
    ResponseAggregator -->|Final Response| WebApp
```

### **API Gateway Components**

**Request Router**
- Routes incoming requests to appropriate services
- Handles URL path matching and HTTP method routing
- Manages request transformation and header manipulation

**Auth Middleware**
- Validates JWT tokens from client requests
- Integrates with Redis cache for token validation performance
- Performs **coarse-grained authorization** at service level (e.g., "User must be Operator+ to access /api/vehicles/*")
- Enforces **role hierarchy** with inheritance (User < Operator < SiteManager < Admin < SuperAdmin)
- Uses **YAML-based authorization rules** for flexible, environment-specific access control
- Extracts user context and permissions for downstream services
- Forwards user information via headers (X-User-ID, X-User-Roles, X-User-Permissions) to services for fine-grained control
- Supports **client-specific authorization configurations** for multi-tenant deployments

**Rate Limiter**
- Implements rate limiting per client/user/endpoint
- Prevents API abuse and ensures fair resource usage
- Configurable limits based on user roles and service tiers

**Load Balancer**
- Distributes requests across healthy service instances
- Implements round-robin, weighted, and health-based routing
- Integrates with Service Discovery for dynamic instance management

---

## **Service Discovery Component Flow**

```mermaid
sequenceDiagram
    participant Service as Microservice
    participant Discovery as Service Discovery
    participant Registry as Service Registry
    participant Health as Health Monitor
    participant Gateway as API Gateway
    participant Config as Config Manager

    Note over Service,Config: Service Registration & Discovery

    Service->>Discovery: Register service instance
    Discovery->>Registry: Store service metadata
    Discovery->>Health: Start health monitoring
    Discovery->>Service: Registration confirmed
    
    loop Health Monitoring
        Health->>Service: Health check ping
        Service->>Health: Health status response
        Health->>Registry: Update service status
    end
    
    Gateway->>Discovery: Request service instances
    Discovery->>Registry: Query available services
    Registry->>Discovery: Return healthy instances
    Discovery->>Gateway: Service endpoints list
    
    Config->>Discovery: Configuration update
    Discovery->>Service: Push config changes
    Service->>Discovery: Config applied confirmation
```

### **Service Discovery Components**

**Service Registry**
- Maintains registry of all service instances and their metadata
- Stores service endpoints, health status, and capabilities
- Provides query interface for service lookup operations

**Health Monitor**
- Continuously monitors health of registered services
- Implements configurable health check intervals and timeouts
- Automatically removes unhealthy instances from active pool

**Config Manager**
- Manages centralized configuration for all services
- Pushes configuration updates to registered services
- Handles environment-specific and feature flag configurations

---

## **Cache Layer Component Flow**

```mermaid
flowchart LR
    subgraph "Cache Clients"
        Gateway[API Gateway]
        UserSvc[User Service]
        CustomerSvc[Customer Service]
        WeightSvc[Weight Data Service]
    end
    
    subgraph "Redis Cluster"
        RedisMaster[Redis Master]
        RedisSlave1[Redis Slave 1]
        RedisSlave2[Redis Slave 2]
        Sentinel[Redis Sentinel]
    end
    
    subgraph "Cache Operations"
        SessionCache[Session Cache]
        DataCache[Data Cache]
        QueryCache[Query Cache]
        RealTimeCache[Real-Time Cache]
    end
    
    subgraph "Cache Strategies"
        WriteThrough[Write-Through]
        WriteBack[Write-Back]
        CacheAside[Cache-Aside]
        TTLManager[TTL Manager]
    end

    Gateway -->|Auth Tokens| SessionCache
    UserSvc -->|User Data| DataCache
    CustomerSvc -->|Customer Data| DataCache
    WeightSvc -->|Current Weights| RealTimeCache
    
    SessionCache -->|Store| RedisMaster
    DataCache -->|Store| RedisMaster
    QueryCache -->|Store| RedisMaster
    RealTimeCache -->|Store| RedisMaster
    
    RedisMaster -->|Replicate| RedisSlave1
    RedisMaster -->|Replicate| RedisSlave2
    Sentinel -->|Monitor| RedisMaster
    Sentinel -->|Failover| RedisSlave1
    
    WriteThrough -->|Strategy| RedisMaster
    WriteBack -->|Strategy| RedisMaster
    CacheAside -->|Strategy| RedisMaster
    TTLManager -->|Expiry| RedisMaster
```

### **Cache Layer Components**

**Session Cache**
- Stores user authentication tokens and session data
- Implements TTL-based expiration for security
- Provides fast lookup for API Gateway authentication

**Data Cache**
- Caches frequently accessed master data (customers, products, vehicles)
- Implements cache-aside pattern for optimal performance
- Reduces database load for read-heavy operations

**Query Cache**
- Caches results of complex database queries
- Implements intelligent cache invalidation strategies
- Optimizes performance for analytics and reporting queries

**Real-Time Cache**
- Stores current weight measurements and transaction states
- Provides sub-second access to operational data
- Supports pub/sub for real-time notifications

---

## **Message Streaming Component Flow**

```mermaid
flowchart TD
    subgraph "Event Producers"
        WeightEvents[Weight Data Events]
        TransactionEvents[Transaction Events]
        ComplianceEvents[Compliance Events]
        UserEvents[User Activity Events]
    end
    
    subgraph "Kafka Cluster"
        Broker1[Kafka Broker 1]
        Broker2[Kafka Broker 2]
        Broker3[Kafka Broker 3]
        ZooKeeper[ZooKeeper Ensemble]
    end
    
    subgraph "Topics & Partitions"
        WeightTopic[weight-data-topic]
        TransactionTopic[transaction-topic]
        ComplianceTopic[compliance-topic]
        UserTopic[user-activity-topic]
    end
    
    subgraph "Event Consumers"
        Analytics[Analytics Service]
        Compliance[Compliance Service]
        DataSync[Data Sync Service]
        Archive[Archive Service]
        Notifications[Notification Service]
    end

    WeightEvents -->|Publish| WeightTopic
    TransactionEvents -->|Publish| TransactionTopic
    ComplianceEvents -->|Publish| ComplianceTopic
    UserEvents -->|Publish| UserTopic
    
    WeightTopic -->|Distribute| Broker1
    TransactionTopic -->|Distribute| Broker2
    ComplianceTopic -->|Distribute| Broker3
    UserTopic -->|Distribute| Broker1
    
    Broker1 -->|Coordinate| ZooKeeper
    Broker2 -->|Coordinate| ZooKeeper
    Broker3 -->|Coordinate| ZooKeeper
    
    Broker1 -->|Consume| Analytics
    Broker2 -->|Consume| Compliance
    Broker3 -->|Consume| DataSync
    Broker1 -->|Consume| Archive
    Broker2 -->|Consume| Notifications
```

### **Message Streaming Components**

**Event Topics**
- **weight-data-topic**: Real-time weight measurements and calibration events
- **transaction-topic**: Business transaction lifecycle events
- **compliance-topic**: Regulatory compliance and violation events
- **user-activity-topic**: User actions and audit trail events

**Kafka Brokers**
- Distributed message brokers for high availability
- Handle message persistence, replication, and delivery
- Provide scalable event streaming capabilities

**Consumer Groups**
- Analytics Service: Processes events for business intelligence
- Compliance Service: Monitors regulatory compliance in real-time
- Data Sync Service: Synchronizes data across multiple sites
- Archive Service: Archives events for long-term storage

---

## **Database Component Flow**

```mermaid
flowchart TD
    subgraph "Application Services"
        UserSvc[User Service]
        CustomerSvc[Customer Service]
        VehicleSvc[Vehicle Service]
        WeightSvc[Weight Data Service]
        TransactionSvc[Transaction Service]
    end
    
    subgraph "Database Cluster"
        PrimaryDB[(PostgreSQL Primary)]
        ReplicaDB1[(PostgreSQL Replica 1)]
        ReplicaDB2[(PostgreSQL Replica 2)]
        TimescaleDB[(TimescaleDB Extension)]
    end
    
    subgraph "Database Components"
        ConnectionPool[Connection Pool]
        QueryOptimizer[Query Optimizer]
        IndexManager[Index Manager]
        BackupManager[Backup Manager]
    end
    
    subgraph "Data Types"
        TransactionalData[Transactional Data]
        TimeSeriesData[Time-Series Data]
        DocumentData[JSON Documents]
        SearchData[Full-Text Search]
    end

    UserSvc -->|User Data| ConnectionPool
    CustomerSvc -->|Customer Data| ConnectionPool
    VehicleSvc -->|Vehicle Data| ConnectionPool
    WeightSvc -->|Weight Data| ConnectionPool
    TransactionSvc -->|Transaction Data| ConnectionPool
    
    ConnectionPool -->|Write Operations| PrimaryDB
    ConnectionPool -->|Read Operations| ReplicaDB1
    ConnectionPool -->|Read Operations| ReplicaDB2
    
    PrimaryDB -->|Replicate| ReplicaDB1
    PrimaryDB -->|Replicate| ReplicaDB2
    PrimaryDB -->|Time-Series| TimescaleDB
    
    QueryOptimizer -->|Optimize| PrimaryDB
    IndexManager -->|Manage Indexes| PrimaryDB
    BackupManager -->|Backup| PrimaryDB
    
    TransactionalData -->|Store| PrimaryDB
    TimeSeriesData -->|Store| TimescaleDB
    DocumentData -->|Store| PrimaryDB
    SearchData -->|Index| PrimaryDB
```

### **Database Components**

**PostgreSQL Primary**
- Handles all write operations and critical reads
- Maintains ACID compliance for transactional data
- Supports JSON documents and full-text search

**PostgreSQL Replicas**
- Handle read-only operations for load distribution
- Provide high availability and disaster recovery
- Support analytics and reporting queries

**TimescaleDB Extension**
- Optimized for time-series data (weight measurements, sensor data)
- Provides automatic partitioning and compression
- Supports real-time analytics on temporal data

**Connection Pool**
- Manages database connections efficiently
- Implements connection reuse and load balancing
- Handles connection failover and retry logic

---

**Navigation**: [← Main Component Flows](03-component-flows.md) | **Next**: [Master Data Components →](03-component-flows-masterdata.md)