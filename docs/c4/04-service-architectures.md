# C4 Level 4: Service Architecture Details

## ⚙️ **Service Internal Architecture**

This document provides detailed views of the internal structure of key services, showing classes, interfaces, data models, and architectural patterns used within individual services.

## 🗄️ **DataMaster Service Architectures**

### **User Service Architecture** (:7001)

```mermaid
classDiagram
    class UserController {
        +AuthenticateAsync(LoginRequest)
        +RefreshTokenAsync(RefreshRequest)
        +CreateUserAsync(CreateUserRequest)
        +GetUserProfileAsync(userId)
        +UpdateUserProfileAsync(userId, UpdateRequest)
        +ChangePasswordAsync(userId, ChangePasswordRequest)
    }
    
    class IUserService {
        <<interface>>
        +AuthenticateUserAsync(username, password)
        +ValidateTokenAsync(token)
        +RefreshTokenAsync(refreshToken)
        +CreateUserAsync(userDto)
        +GetUserByIdAsync(userId)
        +UpdateUserAsync(userId, userDto)
    }
    
    class UserService {
        -IUserRepository userRepository
        -IPasswordHasher passwordHasher
        -IJwtTokenService tokenService
        -IRoleService roleService
        +AuthenticateUserAsync(username, password)
        +ValidateTokenAsync(token)
        +CreateUserAsync(userDto)
        +GetUserByIdAsync(userId)
    }
    
    class IUserRepository {
        <<interface>>
        +GetByUsernameAsync(username)
        +GetByIdAsync(userId)
        +CreateAsync(user)
        +UpdateAsync(user)
        +GetUserRolesAsync(userId)
    }
    
    class UserRepository {
        -ApplicationDbContext context
        +GetByUsernameAsync(username)
        +GetByIdAsync(userId)
        +CreateAsync(user)
        +UpdateAsync(user)
    }
    
    class User {
        +Id: Guid
        +Username: string
        +Email: string
        +PasswordHash: string
        +FirstName: string
        +LastName: string
        +PhoneNumber: string
        +IsActive: bool
        +LastLoginAt: DateTime
        +CreatedAt: DateTime
        +UserRoles: ICollection~UserRole~
    }
    
    class Role {
        +Id: Guid
        +Name: string
        +Description: string
        +Level: int
        +IsActive: bool
        +RolePermissions: ICollection~RolePermission~
    }
    
    class Permission {
        +Id: Guid
        +Name: string
        +Resource: string
        +Action: string
        +IsActive: bool
    }
    
    class IJwtTokenService {
        <<interface>>
        +GenerateTokenAsync(user)
        +ValidateTokenAsync(token)
        +RefreshTokenAsync(refreshToken)
    }
    
    class ApplicationDbContext {
        +Users: DbSet~User~
        +Roles: DbSet~Role~
        +Permissions: DbSet~Permission~
        +UserRoles: DbSet~UserRole~
        +RolePermissions: DbSet~RolePermission~
    }

    UserController --> IUserService
    IUserService <|-- UserService
    UserService --> IUserRepository
    UserService --> IJwtTokenService
    IUserRepository <|-- UserRepository
    UserRepository --> ApplicationDbContext
    ApplicationDbContext --> User
    ApplicationDbContext --> Role
    ApplicationDbContext --> Permission
```

### **Customer Service Architecture** (:7008)

```mermaid
classDiagram
    class CustomerController {
        +GetCustomersAsync(filters)
        +GetCustomerByIdAsync(customerId)
        +CreateCustomerAsync(createRequest)
        +UpdateCustomerAsync(customerId, updateRequest)
        +DeleteCustomerAsync(customerId)
        +GetCustomerOrdersAsync(customerId)
    }
    
    class ICustomerService {
        <<interface>>
        +GetCustomersAsync(filters)
        +GetCustomerByIdAsync(customerId)
        +CreateCustomerAsync(customerDto)
        +UpdateCustomerAsync(customerId, customerDto)
        +ValidateCustomerExistsAsync(customerId)
    }
    
    class CustomerService {
        -ICustomerRepository customerRepository
        -IOrderRepository orderRepository
        -IProductServiceClient productClient
        -ITransporterServiceClient transporterClient
        +GetCustomersAsync(filters)
        +CreateCustomerAsync(customerDto)
        +ValidateCustomerExistsAsync(customerId)
    }
    
    class Customer {
        +Id: Guid
        +Name: string
        +TaxNumber: string
        +RegistrationNumber: string
        +ContactEmail: string
        +ContactPhone: string
        +BillingAddress: string
        +CustomerType: CustomerType
        +CreditLimit: decimal
        +Status: CustomerStatus
        +TransporterId: Guid?
        +PreferredTransporterId: Guid?
        +IsSupplier: bool
        +IsBuyer: bool
        +PaymentTermsDays: int
        +Currency: string
        +Orders: ICollection~Order~
        +Contacts: ICollection~CustomerContact~
    }
    
    class Order {
        +Id: Guid
        +OrderNumber: string
        +CustomerId: Guid
        +SupplierId: Guid?
        +ProductId: Guid
        +ProductName: string
        +Quantity: decimal
        +UnitPrice: decimal
        +TotalAmount: decimal
        +TransporterId: Guid?
        +RouteId: Guid?
        +OrderDate: DateTime
        +Status: OrderStatus
        +OrderType: OrderType
    }
    
    class IProductServiceClient {
        <<interface>>
        +ValidateProductAsync(productId)
        +GetProductPricingAsync(productId)
    }
    
    class ITransporterServiceClient {
        <<interface>>
        +ValidateTransporterAsync(transporterId)
        +GetTransporterCapacityAsync(transporterId)
    }

    CustomerController --> ICustomerService
    ICustomerService <|-- CustomerService
    CustomerService --> Customer
    CustomerService --> Order
    CustomerService --> IProductServiceClient
    CustomerService --> ITransporterServiceClient
```

## ⚙️ **DataManager Service Architectures**

### **Weight Data Service Architecture**

```mermaid
classDiagram
    class WeightDataController {
        +GetCurrentWeightAsync(weighbridgeId)
        +GetWeightHistoryAsync(weighbridgeId, timeRange)
        +StartWeighingSessionAsync(sessionRequest)
        +EndWeighingSessionAsync(sessionId)
        +CalibrateWeighbridgeAsync(weighbridgeId, calibrationData)
    }
    
    class IWeightDataService {
        <<interface>>
        +GetCurrentWeightAsync(weighbridgeId)
        +StartWeighingSessionAsync(sessionData)
        +ProcessWeightDataAsync(weightReading)
        +ValidateWeightDataAsync(weightData)
    }
    
    class WeightDataService {
        -IWeightDataRepository repository
        -IHardwareInterface hardwareInterface
        -ICalibrationService calibrationService
        -IEventPublisher eventPublisher
        +ProcessWeightDataAsync(weightReading)
        +ValidateWeightDataAsync(weightData)
        +PublishWeightEventAsync(weightEvent)
    }
    
    class IHardwareInterface {
        <<interface>>
        +ReadWeightAsync(weighbridgeId)
        +CalibrateAsync(weighbridgeId, calibrationData)
        +GetHardwareStatusAsync(weighbridgeId)
    }
    
    class SerialHardwareInterface {
        -SerialPort serialPort
        -ILogger logger
        +ReadWeightAsync(weighbridgeId)
        +ParseWeightData(rawData)
        +CalibrateAsync(weighbridgeId, calibrationData)
    }
    
    class WeightReading {
        +Id: Guid
        +WeighbridgeId: Guid
        +Weight: decimal
        +Unit: WeightUnit
        +Timestamp: DateTime
        +IsStable: bool
        +Confidence: decimal
        +RawData: string
        +CalibrationFactor: decimal
        +Temperature: decimal?
        +Humidity: decimal?
    }
    
    class WeighingSession {
        +Id: Guid
        +WeighbridgeId: Guid
        +VehicleId: Guid?
        +DriverId: Guid?
        +StartTime: DateTime
        +EndTime: DateTime?
        +TareWeight: decimal?
        +GrossWeight: decimal?
        +NetWeight: decimal?
        +Status: SessionStatus
        +WeightReadings: ICollection~WeightReading~
    }
    
    class ICalibrationService {
        <<interface>>
        +ApplyCalibrationAsync(rawWeight, weighbridgeId)
        +ValidateCalibrationAsync(weighbridgeId)
        +GetCalibrationFactorAsync(weighbridgeId)
    }
    
    class IEventPublisher {
        <<interface>>
        +PublishWeightEventAsync(weightEvent)
        +PublishSessionEventAsync(sessionEvent)
    }

    WeightDataController --> IWeightDataService
    IWeightDataService <|-- WeightDataService
    WeightDataService --> IHardwareInterface
    WeightDataService --> ICalibrationService
    WeightDataService --> IEventPublisher
    IHardwareInterface <|-- SerialHardwareInterface
    WeightDataService --> WeightReading
    WeightDataService --> WeighingSession
```

### **Transaction Service Architecture**

```mermaid
classDiagram
    class TransactionController {
        +CreateTransactionAsync(createRequest)
        +GetTransactionByIdAsync(transactionId)
        +GetTransactionsAsync(filters)
        +UpdateTransactionStatusAsync(transactionId, status)
        +CompleteTransactionAsync(transactionId)
        +CancelTransactionAsync(transactionId)
    }
    
    class ITransactionService {
        <<interface>>
        +CreateTransactionAsync(transactionDto)
        +ProcessTransactionAsync(transactionId)
        +ValidateTransactionAsync(transactionId)
        +CompleteTransactionAsync(transactionId)
    }
    
    class TransactionService {
        -ITransactionRepository repository
        -IWeightDataServiceClient weightClient
        -ICustomerServiceClient customerClient
        -IProductServiceClient productClient
        -IVehicleServiceClient vehicleClient
        -ITransactionSagaOrchestrator sagaOrchestrator
        +CreateTransactionAsync(transactionDto)
        +ProcessTransactionAsync(transactionId)
        +ValidateAllEntitiesAsync(transaction)
    }
    
    class Transaction {
        +Id: Guid
        +TransactionNumber: string
        +CustomerId: Guid
        +ProductId: Guid
        +VehicleId: Guid
        +DriverId: Guid
        +WeighbridgeId: Guid
        +OrderId: Guid?
        +TareWeight: decimal?
        +GrossWeight: decimal?
        +NetWeight: decimal?
        +TransactionType: TransactionType
        +Status: TransactionStatus
        +StartTime: DateTime
        +EndTime: DateTime?
        +CreatedBy: Guid
        +WeightReadings: ICollection~WeightReading~
        +ComplianceChecks: ICollection~ComplianceCheck~
    }
    
    class ITransactionSagaOrchestrator {
        <<interface>>
        +StartTransactionSagaAsync(transactionId)
        +HandleSagaStepAsync(sagaStep)
        +CompensateSagaAsync(sagaId)
    }
    
    class TransactionSagaOrchestrator {
        -ISagaRepository sagaRepository
        -IEventPublisher eventPublisher
        +StartTransactionSagaAsync(transactionId)
        +ExecuteSagaStepAsync(sagaStep)
        +HandleCompensationAsync(compensationStep)
    }
    
    class SagaStep {
        +Id: Guid
        +SagaId: Guid
        +StepName: string
        +StepType: SagaStepType
        +Status: SagaStepStatus
        +ServiceName: string
        +Request: string
        +Response: string
        +CompensationAction: string
        +ExecutedAt: DateTime?
        +CompensatedAt: DateTime?
    }
    
    class ICustomerServiceClient {
        <<interface>>
        +ValidateCustomerAsync(customerId)
        +GetCustomerDetailsAsync(customerId)
    }
    
    class IWeightDataServiceClient {
        <<interface>>
        +GetWeightDataAsync(weighbridgeId, timeRange)
        +ValidateWeightReadingAsync(weightReading)
    }

    TransactionController --> ITransactionService
    ITransactionService <|-- TransactionService
    TransactionService --> ITransactionSagaOrchestrator
    ITransactionSagaOrchestrator <|-- TransactionSagaOrchestrator
    TransactionService --> Transaction
    TransactionSagaOrchestrator --> SagaStep
    TransactionService --> ICustomerServiceClient
    TransactionService --> IWeightDataServiceClient
```

## 🔧 **Shared Infrastructure Components**

### **Repository Pattern Implementation**

```mermaid
classDiagram
    class IRepository~T~ {
        <<interface>>
        +GetByIdAsync(id)
        +GetAllAsync()
        +FindAsync(predicate)
        +AddAsync(entity)
        +UpdateAsync(entity)
        +DeleteAsync(id)
        +SaveChangesAsync()
    }
    
    class BaseRepository~T~ {
        <<abstract>>
        #DbContext context
        #DbSet~T~ dbSet
        +GetByIdAsync(id)
        +GetAllAsync()
        +FindAsync(predicate)
        +AddAsync(entity)
        +UpdateAsync(entity)
        +DeleteAsync(id)
        +SaveChangesAsync()
    }
    
    class IUserRepository {
        <<interface>>
        +GetByUsernameAsync(username)
        +GetUserWithRolesAsync(userId)
        +GetUserPermissionsAsync(userId)
    }
    
    class UserRepository {
        +GetByUsernameAsync(username)
        +GetUserWithRolesAsync(userId)
        +GetUserPermissionsAsync(userId)
    }
    
    class ICustomerRepository {
        <<interface>>
        +GetCustomersWithOrdersAsync(filters)
        +GetCustomersByTransporterAsync(transporterId)
    }
    
    class CustomerRepository {
        +GetCustomersWithOrdersAsync(filters)
        +GetCustomersByTransporterAsync(transporterId)
    }

    IRepository <|-- BaseRepository
    BaseRepository <|-- UserRepository
    BaseRepository <|-- CustomerRepository
    IRepository <|-- IUserRepository
    IRepository <|-- ICustomerRepository
    IUserRepository <|-- UserRepository
    ICustomerRepository <|-- CustomerRepository
```

### **Event Publishing Architecture**

```mermaid
classDiagram
    class IEventPublisher {
        <<interface>>
        +PublishAsync~T~(event)
        +PublishBatchAsync~T~(events)
    }
    
    class KafkaEventPublisher {
        -IProducer~string, string~ producer
        -ILogger logger
        +PublishAsync~T~(event)
        +PublishBatchAsync~T~(events)
        -SerializeEvent~T~(event)
        -GetTopicName~T~()
    }
    
    class IEventHandler~T~ {
        <<interface>>
        +HandleAsync(event)
    }
    
    class WeightDataEventHandler {
        -IAnalyticsService analyticsService
        -IComplianceService complianceService
        +HandleAsync(weightDataEvent)
    }
    
    class TransactionEventHandler {
        -INotificationService notificationService
        -IAuditService auditService
        +HandleAsync(transactionEvent)
    }
    
    class DomainEvent {
        <<abstract>>
        +Id: Guid
        +Timestamp: DateTime
        +EventType: string
        +Version: int
        +CorrelationId: Guid?
    }
    
    class WeightDataEvent {
        +WeighbridgeId: Guid
        +Weight: decimal
        +IsStable: bool
        +SessionId: Guid?
    }
    
    class TransactionEvent {
        +TransactionId: Guid
        +Status: TransactionStatus
        +CustomerId: Guid
        +NetWeight: decimal?
    }

    IEventPublisher <|-- KafkaEventPublisher
    IEventHandler <|-- WeightDataEventHandler
    IEventHandler <|-- TransactionEventHandler
    DomainEvent <|-- WeightDataEvent
    DomainEvent <|-- TransactionEvent
```

### **Service Client Pattern**

```mermaid
classDiagram
    class IServiceClient {
        <<interface>>
        +GetAsync~T~(endpoint)
        +PostAsync~T~(endpoint, data)
        +PutAsync~T~(endpoint, data)
        +DeleteAsync(endpoint)
    }
    
    class BaseServiceClient {
        <<abstract>>
        #HttpClient httpClient
        #ILogger logger
        #string baseUrl
        +GetAsync~T~(endpoint)
        +PostAsync~T~(endpoint, data)
        +HandleHttpErrorAsync(response)
        #SerializeRequest~T~(data)
        #DeserializeResponse~T~(content)
    }
    
    class ICustomerServiceClient {
        <<interface>>
        +ValidateCustomerAsync(customerId)
        +GetCustomerDetailsAsync(customerId)
        +GetCustomerOrdersAsync(customerId)
    }
    
    class CustomerServiceClient {
        +ValidateCustomerAsync(customerId)
        +GetCustomerDetailsAsync(customerId)
        +GetCustomerOrdersAsync(customerId)
    }
    
    class IProductServiceClient {
        <<interface>>
        +ValidateProductAsync(productId)
        +GetProductPricingAsync(productId)
        +GetProductSpecificationsAsync(productId)
    }
    
    class ProductServiceClient {
        +ValidateProductAsync(productId)
        +GetProductPricingAsync(productId)
        +GetProductSpecificationsAsync(productId)
    }
    
    class ServiceClientException {
        +StatusCode: HttpStatusCode
        +ServiceName: string
        +ErrorMessage: string
        +RequestId: string
    }

    IServiceClient <|-- BaseServiceClient
    BaseServiceClient <|-- CustomerServiceClient
    BaseServiceClient <|-- ProductServiceClient
    ICustomerServiceClient <|-- CustomerServiceClient
    IProductServiceClient <|-- ProductServiceClient
    BaseServiceClient --> ServiceClientException
```

## 🔒 **Security Architecture Components**

### **JWT Token Service Architecture**

```mermaid
classDiagram
    class IJwtTokenService {
        <<interface>>
        +GenerateTokenAsync(user, roles)
        +ValidateTokenAsync(token)
        +RefreshTokenAsync(refreshToken)
        +RevokeTokenAsync(token)
    }
    
    class JwtTokenService {
        -JwtSecurityTokenHandler tokenHandler
        -IConfiguration configuration
        -IRefreshTokenRepository refreshTokenRepository
        +GenerateTokenAsync(user, roles)
        +ValidateTokenAsync(token)
        +RefreshTokenAsync(refreshToken)
        +CreateJwtToken(claims, expiry)
        +ExtractClaimsFromToken(token)
    }
    
    class TokenValidationResult {
        +IsValid: bool
        +UserId: Guid?
        +Username: string
        +Roles: List~string~
        +Permissions: List~string~
        +ExpiresAt: DateTime?
        +ErrorMessage: string
    }
    
    class RefreshToken {
        +Id: Guid
        +UserId: Guid
        +Token: string
        +ExpiresAt: DateTime
        +CreatedAt: DateTime
        +IsRevoked: bool
        +RevokedAt: DateTime?
    }
    
    class IRefreshTokenRepository {
        <<interface>>
        +CreateAsync(refreshToken)
        +GetByTokenAsync(token)
        +RevokeAsync(token)
        +RevokeAllUserTokensAsync(userId)
        +DeleteExpiredTokensAsync()
    }

    IJwtTokenService <|-- JwtTokenService
    JwtTokenService --> TokenValidationResult
    JwtTokenService --> RefreshToken
    JwtTokenService --> IRefreshTokenRepository
```

### **Authorization Middleware Architecture**

```mermaid
classDiagram
    class IAuthorizationMiddleware {
        <<interface>>
        +InvokeAsync(context, next)
    }
    
    class RoleAuthorizationMiddleware {
        -RequestDelegate next
        -IJwtTokenService tokenService
        -IAuthorizationCacheService cacheService
        +InvokeAsync(context, next)
        -ExtractTokenFromRequest(request)
        -ValidateTokenAndExtractClaims(token)
        -CheckRoutePermissions(route, userRoles)
        -SetUserContextHeaders(context, userInfo)
    }
    
    class IAuthorizationCacheService {
        <<interface>>
        +GetUserPermissionsAsync(userId)
        +CacheUserPermissionsAsync(userId, permissions)
        +InvalidateUserCacheAsync(userId)
    }
    
    class AuthorizationCacheService {
        -IMemoryCache memoryCache
        -IDistributedCache distributedCache
        +GetUserPermissionsAsync(userId)
        +CacheUserPermissionsAsync(userId, permissions)
        -GetCacheKey(userId)
    }
    
    class UserContext {
        +UserId: Guid
        +Username: string
        +Email: string
        +Roles: List~string~
        +Permissions: List~string~
        +OrganizationId: Guid?
        +IsAuthenticated: bool
    }
    
    class RoutePermission {
        +Route: string
        +HttpMethod: string
        +RequiredRoles: List~string~
        +RequiredPermissions: List~string~
        +IsPublic: bool
    }

    IAuthorizationMiddleware <|-- RoleAuthorizationMiddleware
    RoleAuthorizationMiddleware --> IAuthorizationCacheService
    IAuthorizationCacheService <|-- AuthorizationCacheService
    RoleAuthorizationMiddleware --> UserContext
    RoleAuthorizationMiddleware --> RoutePermission
```

## 📊 **Data Access Architecture**

### **Database Context Architecture**

```mermaid
classDiagram
    class IApplicationDbContext {
        <<interface>>
        +Users: DbSet~User~
        +Customers: DbSet~Customer~
        +Products: DbSet~Product~
        +Transactions: DbSet~Transaction~
        +SaveChangesAsync(cancellationToken)
    }
    
    class ApplicationDbContext {
        +Users: DbSet~User~
        +Customers: DbSet~Customer~
        +Products: DbSet~Product~
        +Transactions: DbSet~Transaction~
        +WeightReadings: DbSet~WeightReading~
        +OnModelCreating(modelBuilder)
        +SaveChangesAsync(cancellationToken)
        -ApplyEntityConfigurations(modelBuilder)
        -SetAuditFields()
    }
    
    class BaseEntity {
        <<abstract>>
        +Id: Guid
        +CreatedAt: DateTime
        +CreatedBy: Guid?
        +UpdatedAt: DateTime?
        +UpdatedBy: Guid?
        +IsDeleted: bool
        +DeletedAt: DateTime?
        +DeletedBy: Guid?
    }
    
    class IAuditableEntity {
        <<interface>>
        +CreatedAt: DateTime
        +CreatedBy: Guid?
        +UpdatedAt: DateTime?
        +UpdatedBy: Guid?
    }
    
    class ISoftDeletable {
        <<interface>>
        +IsDeleted: bool
        +DeletedAt: DateTime?
        +DeletedBy: Guid?
    }

    IApplicationDbContext <|-- ApplicationDbContext
    IAuditableEntity <|-- BaseEntity
    ISoftDeletable <|-- BaseEntity
    ApplicationDbContext --> BaseEntity
```

---

**Previous Level**: [← Component Flows](03-component-flows.md) | **Next Level**: [Business Processes →](05-business-processes.md)