# Customer Service - Clean Architecture with Service Separation

## 🎯 **Clean Service Architecture Overview**

The Customer Service has been refactored to maintain **clean separation of concerns** with other microservices, avoiding duplication and following proper microservice principles.

## 🏗️ **Clean Entity Relationship Model**

### **Service Separation Visual**
```
┌─────────────────────────────────────────────────────────────────────────┐
│                    CLEAN MICROSERVICE ARCHITECTURE                     │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  ┌─────────────────────────────────────────────────────────────────┐    │
│  │               CUSTOMER SERVICE (:7008)                         │    │
│  │                                                                 │    │
│  │  Customer Entity:                                               │    │
│  │  • Name, Email, Phone, Address                                  │    │
│  │  • TaxNumber, RegistrationNumber                                │    │
│  │  • CustomerType, Status, CreditLimit                           │    │
│  │  • IsSupplier, IsBuyer                                          │    │
│  │  • PaymentTermsDays, Currency                                   │    │
│  │                                                                 │    │
│  │  🔗 Service References (Clean):                                │    │
│  │  • TransporterId → References Transporter Service              │    │
│  │  • PreferredTransporterId → External transporter preference    │    │
│  │                                                                 │    │
│  │  Order Entity:                                                  │    │
│  │  • Order lifecycle management                                   │    │
│  │  • Product requirements                                         │    │
│  │  • Quality specifications                                       │    │
│  │  • TransporterId → References Transporter Service              │    │
│  │  • RouteId → References Route Service                          │    │
│  └─────────────────┬───────────────────────────────────────────────┘    │
│                    │                                                     │
│                    │ (references via ID)                                 │
│                    ▼                                                     │
│  ┌─────────────────────────────────────────────────────────────────┐    │
│  │              TRANSPORTER SERVICE (:7010)                       │    │
│  │                                                                 │    │
│  │  Transporter Entity:                                            │    │
│  │  • CompanyName, LicenseNumber                                   │    │
│  │  • Fleet management capabilities                                │    │
│  │  • Service areas, specializations                              │    │
│  │  • Insurance, certifications                                    │    │
│  │  • Contact information                                          │    │
│  │                                                                 │    │
│  │  🔗 Business Relationship:                                     │    │
│  │  • CustomerId → Link back to Customer (if customer-transporter)│    │
│  │  • Independent transporters (no customer link)                 │    │
│  │  • Vehicle fleet management                                     │    │
│  │  • Driver management                                            │    │
│  └─────────────────┬───────────────────────────────────────────────┘    │
│                    │                                                     │
│                    │ (manages)                                           │
│                    ▼                                                     │
│  ┌─────────────────────────────────────────────────────────────────┐    │
│  │               VEHICLE SERVICE (:7003)                          │    │
│  │                                                                 │    │
│  │  Vehicle Entity:                                                │    │
│  │  • Registration, specifications                                 │    │
│  │  • TransporterId → Owner transporter                           │    │
│  │  • Current status, location                                     │    │
│  │  • Maintenance records                                          │    │
│  │  • SACCO registrations (external references)                   │    │
│  └─────────────────┬───────────────────────────────────────────────┘    │
│                    │                                                     │
│                    │ (operated by)                                       │
│                    ▼                                                     │
│  ┌─────────────────────────────────────────────────────────────────┐    │
│  │               DRIVER SERVICE (:7004)                           │    │
│  │                                                                 │    │
│  │  Driver Entity:                                                 │    │
│  │  • Personal information, licenses                               │    │
│  │  • TransporterId → Employer transporter                        │    │
│  │  • Certifications, violations                                   │    │
│  │  • Current assignments                                          │    │
│  └─────────────────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────────────────┘
```

## 🔄 **Clean Business Process Flow**

### **Customer-Transporter Relationship Scenarios**
```
┌─────────────────────────────────────────────────────────────────────────┐
│                    CLEAN BUSINESS RELATIONSHIP FLOW                    │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  Scenario 1: Customer with Own Transport Capability                    │
│  ┌─────────────────────────────────────────────────────────────────┐    │
│  │ 1. Customer "Bamburi Cement" exists in Customer Service        │    │
│  │    • Name: "Bamburi Cement Ltd"                                │    │
│  │    • IsSupplier: false, IsBuyer: true                          │    │
│  │    • TransporterId: "transport-bamburi-001"                    │    │
│  │                                                                 │    │
│  │ 2. Separate Transporter "transport-bamburi-001" in Transporter │    │
│  │    Service:                                                     │    │
│  │    • CompanyName: "Bamburi Transport Division"                 │    │
│  │    • LicenseNumber: "TL-2024-001"                             │    │
│  │    • CustomerId: "customer-bamburi-001" (back-reference)       │    │
│  │    • Fleet: 50 vehicles                                        │    │
│  │                                                                 │    │
│  │ 3. Order Processing:                                            │    │
│  │    • Customer places order                                      │    │
│  │    • System checks Customer.TransporterId                      │    │
│  │    • Assigns transport to customer's own transporter           │    │
│  │    • Transporter Service manages vehicle/driver assignment     │    │
│  └─────────────────────────────────────────────────────────────────┘    │
│                                                                         │
│  Scenario 2: Customer Using External Transporter                       │
│  ┌─────────────────────────────────────────────────────────────────┐    │
│  │ 1. Customer "Ujenzi Builders" exists in Customer Service       │    │
│  │    • Name: "Ujenzi Builders Ltd"                               │    │
│  │    • TransporterId: null (no own transport)                    │    │
│  │    • PreferredTransporterId: "transport-kentrans-001"          │    │
│  │                                                                 │    │
│  │ 2. External Transporter "transport-kentrans-001":              │    │
│  │    • CompanyName: "KenTrans Logistics"                         │    │
│  │    • CustomerId: null (independent transporter)                │    │
│  │    • Services multiple customers                                │    │
│  │                                                                 │    │
│  │ 3. Order Processing:                                            │    │
│  │    • Customer places order                                      │    │
│  │    • System uses PreferredTransporterId                        │    │
│  │    • External transporter handles transport                    │    │
│  └─────────────────────────────────────────────────────────────────┘    │
│                                                                         │
│  Scenario 3: Independent Transporter (No Customer Link)                │
│  ┌─────────────────────────────────────────────────────────────────┐    │
│  │ 1. Transporter "Haulage Masters Ltd" exists only in            │    │
│  │    Transporter Service:                                         │    │
│  │    • CompanyName: "Haulage Masters Ltd"                        │    │
│  │    • CustomerId: null (no customer relationship)               │    │
│  │    • Provides transport services to multiple customers         │    │
│  │                                                                 │    │
│  │ 2. Multiple customers can reference this transporter           │    │
│  │    as PreferredTransporterId                                    │    │
│  └─────────────────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────────────────┘
```

## 📊 **Clean API Design**

### **Customer Service APIs** (Focused)
```
┌─────────────────────────────────────────────────────────────────────────┐
│                      CUSTOMER SERVICE APIs (CLEAN)                     │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  Core Customer Management:                                              │
│  ├─ GET    /api/customers                    # List customers           │
│  ├─ POST   /api/customers                    # Create customer          │
│  ├─ GET    /api/customers/{id}               # Get customer details     │
│  ├─ PUT    /api/customers/{id}               # Update customer          │
│  ├─ DELETE /api/customers/{id}               # Soft delete customer     │
│  └─ GET    /api/customers/search?q={term}    # Search customers         │
│                                                                         │
│  Business Relationship Management:                                      │
│  ├─ PUT    /api/customers/{id}/transporter   # Link to transporter      │
│  ├─ DELETE /api/customers/{id}/transporter   # Unlink transporter       │
│  ├─ PUT    /api/customers/{id}/preferred-transporter # Set preference   │
│  └─ GET    /api/customers/{id}/transport-options # Get transport info   │
│                                                                         │
│  Order Management:                                                      │
│  ├─ GET    /api/orders                       # List all orders          │
│  ├─ POST   /api/orders                       # Create order             │
│  ├─ GET    /api/orders/{id}                  # Order details            │
│  ├─ PUT    /api/orders/{id}                  # Update order             │
│  ├─ POST   /api/orders/{id}/assign-transport # Assign transporter       │
│  └─ GET    /api/orders/customer/{customerId} # Customer orders          │
│                                                                         │
│  ❌ REMOVED: Transport-specific operations                              │
│  ❌ No fleet management APIs                                            │
│  ❌ No vehicle/driver management                                        │
│  ❌ No transport licensing                                              │
└─────────────────────────────────────────────────────────────────────────┘
```

### **Transporter Service APIs** (Specialized)
```
┌─────────────────────────────────────────────────────────────────────────┐
│                     TRANSPORTER SERVICE APIs                           │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  Core Transporter Management:                                           │
│  ├─ GET    /api/transporters                 # List transporters        │
│  ├─ POST   /api/transporters                 # Create transporter       │
│  ├─ GET    /api/transporters/{id}            # Transporter details      │
│  ├─ PUT    /api/transporters/{id}            # Update transporter       │
│  └─ DELETE /api/transporters/{id}            # Delete transporter       │
│                                                                         │
│  Customer-Transporter Relationships:                                    │
│  ├─ POST   /api/transporters/customer-owned  # Create customer transport│
│  ├─ GET    /api/transporters/customer/{id}   # Get customer's transporter│
│  └─ GET    /api/transporters/independent     # Independent transporters │
│                                                                         │
│  Fleet & Operations:                                                    │
│  ├─ GET    /api/transporters/{id}/vehicles   # Transporter's vehicles   │
│  ├─ GET    /api/transporters/{id}/drivers    # Transporter's drivers    │
│  ├─ GET    /api/transporters/{id}/capacity   # Available capacity       │
│  ├─ POST   /api/transporters/{id}/assign     # Assign to order          │
│  └─ GET    /api/transporters/{id}/assignments # Current assignments     │
│                                                                         │
│  Service Areas & Capabilities:                                          │
│  ├─ GET    /api/transporters/by-route/{routeId} # Transporters for route│
│  ├─ GET    /api/transporters/by-area/{area}   # Transporters by area    │
│  └─ GET    /api/transporters/availability     # Real-time availability  │
└─────────────────────────────────────────────────────────────────────────┘
```

## 🔗 **Clean Service Integration**

### **Inter-Service Communication Pattern**
```
┌─────────────────────────────────────────────────────────────────────────┐
│                     CLEAN SERVICE INTEGRATION                          │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  Order Creation Flow:                                                   │
│  ┌─────────────────────────────────────────────────────────────────┐    │
│  │ 1. Customer Service receives order creation request            │    │
│  │                                                                 │    │
│  │ 2. Transport Assignment Logic:                                  │    │
│  │    IF Customer.TransporterId != null                           │    │
│  │      → Call Transporter Service: GET /transporters/{id}        │    │
│  │      → Verify transporter belongs to customer                  │    │
│  │      → Assign customer's own transporter                       │    │
│  │    ELSE IF Customer.PreferredTransporterId != null             │    │
│  │      → Call Transporter Service: GET /transporters/{id}        │    │
│  │      → Check availability                                       │    │
│  │      → Assign preferred transporter                            │    │
│  │    ELSE                                                         │    │
│  │      → Call Transporter Service: GET /transporters/by-route    │    │
│  │      → Select best available transporter                       │    │
│  │                                                                 │    │
│  │ 3. Create order with TransporterId reference                   │    │
│  │                                                                 │    │
│  │ 4. Call Transporter Service: POST /transporters/{id}/assign    │    │
│  │    → Notify transporter of new assignment                      │    │
│  └─────────────────────────────────────────────────────────────────┘    │
│                                                                         │
│  Service Lookup Pattern:                                                │
│  ┌─────────────────────────────────────────────────────────────────┐    │
│  │ Customer Service stores only IDs:                              │    │
│  │ • Order.TransporterId = "transport-123"                        │    │
│  │ • Order.RouteId = "route-456"                                  │    │
│  │ • Order.ProductId = "product-789"                              │    │
│  │                                                                 │    │
│  │ When returning order details:                                   │    │
│  │ • Call Transporter Service for transporter name               │    │
│  │ • Call Route Service for route details                        │    │
│  │ • Call Product Service for product details                    │    │
│  │ • Aggregate response with full details                        │    │
│  └─────────────────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────────────────┘
```

## 📁 **Roles and Permissions Configuration**

### **Configuration Approach Question Response:**

For roles and permissions, I recommend a **hybrid approach**:

#### **Option 1: Gateway Configuration** 📁 `configs/auth/`
```
configs/auth/
├── roles.json                 # Role definitions
├── permissions.json           # Permission definitions
├── role-permissions.json      # Role-permission mappings
└── service-permissions.json   # Service-specific permissions
```

**Pros:**
- Centralized authorization at gateway level
- Consistent across all services
- Easy to manage and audit

**Cons:**
- Gateway becomes a bottleneck
- Less flexible for service-specific permissions

#### **Option 2: User Service Configuration** 📁 `UserService.Infrastructure/Data/SeedData/`
```
UserService.Infrastructure/Data/SeedData/
├── DefaultRoles.json
├── DefaultPermissions.json
├── RolePermissions.json
└── ServicePermissions.json
```

**Pros:**
- Database-driven, can be modified at runtime
- User Service manages identity completely
- Scalable and flexible

**Cons:**
- Services must call User Service for permissions
- More complex authorization flow

#### **Recommended Approach: User Service + Gateway Cache**

1. **Roles & Permissions** defined in **User Service database**
2. **Gateway caches** permissions for performance
3. **Service-specific permissions** in each service's configuration

```
┌─────────────────────────────────────────────────────────────────────────┐
│                   RECOMMENDED PERMISSIONS ARCHITECTURE                  │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  User Service (Source of Truth):                                        │
│  ├─ Database tables: Roles, Permissions, RolePermissions               │
│  ├─ JWT tokens include user roles                                       │
│  └─ APIs for role/permission management                                 │
│                                                                         │
│  API Gateway (Cached Authorization):                                    │
│  ├─ configs/auth/gateway-permissions.json                              │
│  ├─ Cache role-permission mappings                                      │
│  ├─ Validate JWT and extract roles                                      │
│  └─ Route-level permission checking                                     │
│                                                                         │
│  Individual Services (Fine-grained):                                    │
│  ├─ appsettings.json: service-specific permissions                     │
│  ├─ Resource-level authorization                                        │
│  └─ Business logic permission checks                                    │
└─────────────────────────────────────────────────────────────────────────┘
```

**Would you like me to implement this permissions approach?**

## 🎯 **Customer Service Implementation - COMPLETED** ✅

### **✅ Fully Implemented and Working:**

1. **Complete Implementation:**
   - ✅ **Customer CRUD** with clean service separation
   - ✅ **Order Management** with full lifecycle tracking
   - ✅ **Database Schema** with proper relationships and constraints
   - ✅ **API Endpoints** fully functional and tested
   - ✅ **Service Architecture** following Clean Architecture principles

2. **Clean Service Separation:**
   - 🎯 **Customer Service**: Customer management + Orders (✅ COMPLETE)
   - 🚛 **Transporter Service**: Transport operations + Fleet management (🔜 NEXT)
   - 🚗 **Vehicle Service**: Vehicle management under Transporters (🔜 PENDING)
   - 👨‍💼 **Driver Service**: Driver management under Transporters (🔜 PENDING)

3. **Production Ready:**
   - ✅ **Builds successfully** with no errors
   - ✅ **Runs on port 7008** as configured
   - ✅ **Database creation** working properly
   - ✅ **Health checks** functional
   - ✅ **API endpoints** responding correctly

4. **Business Model Support:**
   - ✅ **Customer-Transporter relationships** via TransporterId references
   - ✅ **Order-driven workflows** with comprehensive order management
   - ✅ **Multi-role support** (customer, supplier, buyer flags)
   - ✅ **Quality management** with specifications and tolerances
   - ✅ **Audit trail** for compliance and tracking

### **🚀 Ready for Integration:**
- ✅ **Service boundaries** properly defined
- ✅ **No duplication** with other services
- ✅ **Integration points** clearly identified
- ✅ **Next service** (Transporter Service) can be developed

**Customer Service Status: ✅ COMPLETED and FUNCTIONAL**

---

*Clean Customer Service Architecture - Proper Service Separation*  
*QaliTrack Master Data Services with Clear Boundaries*