# QaliTrack Master Data Service

A consolidated master data service built with Django-style modular architecture for the QaliTrack weighbridge management system.

## 🏗️ Architecture

This service consolidates 12 individual microservices into a single, well-structured service using Django-style patterns:

### Django-Style Modules (Apps)
- **Organization** - Multi-tenancy foundation
- **BusinessEntities** - Customer, Supplier, Transporter management
- **Driver** - Driver profiles, licensing, and performance
- **Vehicle** - Vehicle management with maintenance tracking
- **SACCO** - SACCO organization management
- **Product** - Product catalog and specifications
- **Route** - Route planning and optimization
- **Weighbridge** - Weighbridge operations and calibration
- **Relationships** - Cross-module relationship management

## 🚀 Quick Start

### Prerequisites
- .NET 8.0 SDK
- PostgreSQL (optional, SQLite is default)

### 1. Clone and Setup
```bash
cd packages/QaliTrack.MasterData
cp .env.sample .env
# Edit .env with your configuration
```

### 2. Database Setup (Django-style migrations)
```bash
# Create migration
dotnet ef migrations add InitialCreate --project src/QaliTrack.MasterData.Infrastructure --startup-project src/QaliTrack.MasterData.Api

# Apply migration
dotnet ef database update --project src/QaliTrack.MasterData.Infrastructure --startup-project src/QaliTrack.MasterData.Api
```

### 3. Run the Service
```bash
dotnet build -m:1 --verbosity minimal --no-restore -p:UseSharedCompilation=false
dotnet run --project src/QaliTrack.MasterData.Api
```

### 4. Access the Service
- **Swagger UI**: `http://localhost:5000/`
- **Health Check**: `http://localhost:5000/health`
- **API Endpoints**: `http://localhost:5000/{module}/` (Django-style URLs)

## 🌍 Environment Configuration

### Database Configuration (Django-style)

#### SQLite (Default)
```env
DB_PROVIDER=SQLite
DB_PATH=masterdata.db
```

#### PostgreSQL
```env
DB_PROVIDER=PostgreSQL
DB_HOST=localhost
DB_PORT=5432
DB_NAME=qalitrack_masterdata
DB_USER=postgres
DB_PASSWORD=your_password
```

### Module Configuration (Django INSTALLED_APPS equivalent)

#### Enabling/Disabling Modules
Control which modules are loaded at runtime (similar to Django's `INSTALLED_APPS`):

```env
# Default: All modules enabled
ENABLED_MODULES=Organization,Driver,Vehicle,BusinessEntities,SACCO,Product,Route,Weighbridge

# Minimal setup: Core modules only
ENABLED_MODULES=Organization,BusinessEntities,Driver

# Custom deployment: Specific business needs
ENABLED_MODULES=Organization,Driver,Vehicle,SACCO
```

#### Module Dependencies
Some modules have dependencies that are automatically included:

```bash
# Organization module: Always required (multi-tenancy foundation)
# - Automatically included regardless of ENABLED_MODULES setting
# - Provides OrganizationId context for all other modules

# Relationships module: Auto-enabled when needed
# - Automatically included when modules with cross-relationships are enabled
# - Example: Driver + SACCO modules = DriverSaccoMembership relationships enabled

# BusinessEntities: Replaces Customer, Supplier, Transporter
# - Single module providing Customer/Supplier/Transporter profiles
# - More efficient than separate microservices
```

#### Module Control Features
- **Runtime Loading**: Only enabled modules are registered with dependency injection
- **Controller Registration**: Disabled modules' controllers are not registered
- **Database Tables**: All tables created, but only enabled modules serve data
- **API Documentation**: Swagger only shows enabled modules' endpoints
- **Cross-Module Relationships**: Automatically handled between enabled modules

#### Module Configuration Examples

```env
# Transport-focused deployment
ENABLED_MODULES=Organization,Driver,Vehicle,SACCO,Route
# Enables: Driver-Vehicle assignments, Driver-SACCO memberships, Route planning

# Business operations focus  
ENABLED_MODULES=Organization,BusinessEntities,Product,Weighbridge
# Enables: Customer/Supplier management, Product catalogs, Weighbridge operations

# SACCO management system
ENABLED_MODULES=Organization,Driver,Vehicle,SACCO
# Enables: Complete SACCO ecosystem with member and fleet management

# Minimal API surface
ENABLED_MODULES=Organization,BusinessEntities
# Enables: Basic business entity management only
```

## 📡 API Endpoints

### Django-Style URL Structure
```
/{module}/                    # List resources (Django-style, no /api prefix)
/{module}/{id}               # Get/Update/Delete resource
/{module}/{id}/{relationship} # Cross-module relationships
```

### Available Modules
- `/organization/` - Organization management
- `/driver/` - Driver management
- `/vehicle/` - Vehicle management
- `/businessentity/` - Customer/Supplier/Transporter management
- `/sacco/` - SACCO organization management
- `/product/` - Product catalog management
- `/route/` - Route planning
- `/weighbridge/` - Weighbridge operations

### Cross-Module Relationships
- `/driver/{id}/sacco-memberships` - Driver's SACCO memberships
- `/driver/{id}/vehicle-assignments` - Driver's vehicle assignments
- `/vehicle/{id}/ownership-history` - Vehicle ownership tracking

## 🔍 Django-Style Filtering & Pagination

### Query Parameters (Django REST Framework Style)

#### Pagination
```bash
# Basic pagination
GET /organization/?page=2&page_size=10

# Response format (Django REST style)
{
  "success": true,
  "data": {
    "count": 150,                    # Total number of records
    "next": "http://localhost:5000/organization/?page=3&page_size=10",
    "previous": "http://localhost:5000/organization/?page=1&page_size=10", 
    "results": [...],                # Array of actual data
    "pagination": {
      "page": 2,
      "pageSize": 10,
      "totalPages": 15,
      "totalItems": 150,
      "hasNext": true,
      "hasPrevious": true
    }
  }
}
```

#### Search (Django-style global search)
```bash
# Search across searchable text fields
GET /driver/?search=john
GET /organization/?search=transport
```

#### Filtering (Django field lookups)
```bash
# Filter by exact match
GET /driver/?status=Active
GET /vehicle/?fuelType=Diesel

# Multiple filters (AND logic)
GET /driver/?status=Active&nationality=Kenya

# Organization-specific filtering (multi-tenancy)
GET /driver/?organizationId=123e4567-e89b-12d3-a456-426614174000
```

#### Ordering (Django-style)
```bash
# Ascending order
GET /driver/?ordering=lastName

# Descending order (prefix with -)
GET /driver/?ordering=-createdAt

# Multiple fields
GET /vehicle/?ordering=make,-year
```

#### Combined Query Examples
```bash
# Search + filter + sort + paginate
GET /driver/?search=john&status=Active&ordering=-createdAt&page=1&page_size=20

# Filter vehicles by type and fuel, sorted by year
GET /vehicle/?vehicleType=Truck&fuelType=Diesel&ordering=-year&page_size=50

# Search organizations and paginate
GET /organization/?search=logistics&ordering=name&page=2&page_size=15
```

### Advanced Filtering Features

#### Date Filtering (Django-style)
```bash
# Filter by date ranges (implementation ready)
GET /driver/?createdAt__gte=2024-01-01
GET /vehicle/?lastInspectionDate__lte=2024-12-31
```

#### Cross-Module Filtering
```bash
# Filter drivers by their SACCO membership status
GET /driver/{id}/sacco-memberships?status=Active

# Filter vehicles by transporter ownership
GET /vehicle/{id}/transporter-ownership?isActive=true
```

### Response Formats

#### Success Response
```json
{
  "success": true,
  "message": "Operation successful",
  "data": {
    "count": 25,
    "next": "http://localhost:5000/organization/?page=2&page_size=10",
    "previous": null,
    "results": [
      {
        "id": "123e4567-e89b-12d3-a456-426614174000",
        "name": "ABC Transport Ltd",
        "status": "Active",
        // ... other fields
      }
    ],
    "pagination": {
      "page": 1,
      "pageSize": 10,
      "totalPages": 3,
      "totalItems": 25,
      "hasNext": true,
      "hasPrevious": false
    }
  },
  "errors": [],
  "timestamp": "2025-08-27T06:17:44.0171071Z"
}
```

#### Error Response
```json
{
  "success": false,
  "message": "Operation failed",
  "data": null,
  "errors": [
    "Driver not found",
    "Invalid organization ID format"
  ],
  "timestamp": "2025-08-27T06:17:44.0171071Z"
}
```

## 🗄️ Database Schema & Architecture

### Database Support
- **Primary**: SQLite (development, lightweight deployments)
- **Production**: PostgreSQL (recommended for production environments)
- **Configuration**: Django-style environment variables in `.env` file

### UUID-Based Architecture
All entities use **UUID primary keys** for:
- **Scalability**: No auto-increment bottlenecks
- **Security**: Non-sequential, non-predictable IDs
- **Distributed Systems**: Collision-free across services
- **Data Migration**: Simplified merging and replication

```sql
-- Example: All IDs are UUIDs
Id: 123e4567-e89b-12d3-a456-426614174000 (Guid)
OrganizationId: 987fcdeb-51a2-43d6-bf89-123456789012 (Guid)
```

### Cross-Module Relationships (Implemented)

#### Core Business Relationships
- **Driver ↔ SACCO**: `DriverSaccoMembership`
  - Membership tracking, share contributions, benefits
  - Status tracking (Active, Inactive, Suspended)
- **Vehicle ↔ Transporter**: `VehicleTransporterOwnership`  
  - Fleet ownership, purchase details, financing info
  - Ownership history with start/end dates
- **Driver ↔ Vehicle**: `DriverVehicleAssignment`
  - Current and historical assignments
  - Primary driver designation per vehicle
- **Product ↔ Supplier**: `ProductSupplierCatalog`
  - Pricing, lead times, quality ratings
  - Preferred supplier relationships

#### Operational Relationships  
- **Route ↔ Weighbridge**: `RouteWeighbridgeAssociation`
  - Mandatory/optional stops, sequence order
  - Distance and duration estimates
- **Organization ↔ Weighbridge**: `OrganizationWeighbridgeOwnership`
  - Asset ownership, maintenance contracts
  - Purchase and insurance details
- **Vehicle ↔ SACCO**: `VehicleSaccoRegistration`
  - Regulatory compliance tracking
  - Registration fees and certificate management

#### Access Control
- **User ↔ Organization ↔ Role**: `UserOrganizationRole`
  - Multi-tenant role assignment matrix
  - Scope limitations and specific permissions

### Entity Features

#### BaseEntity Pattern (Django-style Model)
```csharp
public abstract class BaseEntity 
{
    public Guid Id { get; set; } = Guid.NewGuid();          // UUID primary key
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Auto-timestamp
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow; // Auto-timestamp  
    public string CreatedBy { get; set; } = "system";        // Audit trail
    public string UpdatedBy { get; set; } = "system";        // Audit trail
    public bool IsDeleted { get; set; } = false;             // Soft delete flag
}
```

#### Global Query Filters (Django-style Soft Delete)
All entities automatically filter out soft-deleted records:
```csharp
// Automatic filtering - deleted records never returned
modelBuilder.Entity<Driver>().HasQueryFilter(d => !d.IsDeleted);
modelBuilder.Entity<Vehicle>().HasQueryFilter(v => !v.IsDeleted);
// Applied to all entities inheriting BaseEntity
```

#### Multi-Tenancy Support
Organization-based data isolation:
```csharp
// All tenant-specific entities include OrganizationId
public Guid OrganizationId { get; set; }  // Tenant isolation

// Automatic filtering by organization context (implementation ready)
// query.Where(entity => entity.OrganizationId == currentUserOrganizationId)
```

### Database Schema Highlights

#### Module Tables (Core Entities)
```sql
-- Organization module
Organizations, OrganizationUsers, OrganizationLocations, OrganizationSettings, OrganizationSubscriptions

-- Driver module  
Drivers, DriverLicenses, DriverProfiles, DriverDocuments, DriverTrainings, DriverMedical, DriverViolations, DriverPerformance

-- Vehicle module
Vehicles, VehicleTypes, VehicleRegistrations, VehicleSpecifications, VehicleDocuments, VehicleInspections, VehicleInsurance, VehicleMaintenance

-- BusinessEntity module (replaces Customer/Supplier/Transporter)
BusinessEntities, CustomerProfiles, SupplierProfiles, TransporterProfiles, BusinessEntityContacts, BusinessEntityLocations, BusinessEntityDocuments

-- SACCO module
Saccos, SaccoMembers, SaccoCommittees, SaccoMeetings, SaccoFinancials, SaccoShares, SaccoLoans, SaccoServices
```

#### Relationship Tables (Junction Tables)
```sql
-- Cross-module relationship tracking
DriverSaccoMemberships           -- Driver ↔ SACCO relationships
VehicleTransporterOwnerships     -- Vehicle ↔ Transporter fleet management  
DriverVehicleAssignments         -- Driver ↔ Vehicle assignments
ProductSupplierCatalogs          -- Product ↔ Supplier sourcing
RouteWeighbridgeAssociations     -- Route ↔ Weighbridge stops
OrganizationWeighbridgeOwnerships -- Organization ↔ Weighbridge assets
VehicleSaccoRegistrations        -- Vehicle ↔ SACCO compliance
UserOrganizationRoles            -- User ↔ Organization ↔ Role matrix
```

#### Index Strategy
```sql
-- Performance indexes on frequently queried fields
CREATE INDEX IX_Drivers_OrganizationId ON Drivers(OrganizationId);
CREATE INDEX IX_Vehicles_Status_OrganizationId ON Vehicles(Status, OrganizationId);
CREATE INDEX IX_BusinessEntities_EntityType ON BusinessEntities(EntityType);
CREATE INDEX IX_DriverVehicleAssignments_IsActive ON DriverVehicleAssignments(IsActive);
-- UUID indexes for foreign key relationships
CREATE INDEX IX_DriverSaccoMemberships_DriverId ON DriverSaccoMemberships(DriverId);
CREATE INDEX IX_VehicleTransporterOwnerships_VehicleId ON VehicleTransporterOwnerships(VehicleId);
```

## 🔧 Development

### Project Structure
```
QaliTrack.MasterData/
├── src/
│   ├── QaliTrack.MasterData.Api/           # Web API layer
│   │   ├── Controllers/                    # Module controllers
│   │   └── Infrastructure/                 # Configuration & utilities
│   ├── QaliTrack.MasterData.Core/          # Business logic & entities
│   │   ├── Common/                         # Shared interfaces & base classes
│   │   └── Modules/                        # Django-style modules
│   │       ├── Organization/
│   │       ├── Driver/
│   │       ├── Vehicle/
│   │       └── Relationships/              # Cross-module relationships
│   └── QaliTrack.MasterData.Infrastructure/ # Data access layer
│       ├── Data/                           # DbContext & configuration
│       └── Migrations/                     # EF Core migrations
├── tests/
│   └── QaliTrack.MasterData.Tests/         # Unit & integration tests
├── .env.sample                             # Environment configuration template
└── README.md
```

### Adding New Modules (Django Apps equivalent)
1. Create module folder in `Core/Modules/{ModuleName}/`
2. Add entities in `{ModuleName}/Entities/`
3. Update `MasterDataDbContext` with new DbSets
4. Create controller in `Api/Controllers/`
5. Add to `ENABLED_MODULES` in `.env`

### Database Migrations (Django-style)
```bash
# Add new migration
dotnet ef migrations add {MigrationName} --project src/QaliTrack.MasterData.Infrastructure --startup-project src/QaliTrack.MasterData.Api

# Update database
dotnet ef database update --project src/QaliTrack.MasterData.Infrastructure --startup-project src/QaliTrack.MasterData.Api

# Rollback migration
dotnet ef database update {PreviousMigration} --project src/QaliTrack.MasterData.Infrastructure --startup-project src/QaliTrack.MasterData.Api
```

## 🧪 Testing

### Run Tests
```bash
dotnet test
```

### Test Categories
- **Unit Tests**: Individual module logic
- **Integration Tests**: Database and API integration
- **Relationship Tests**: Cross-module relationship validation

### Manual API Testing

#### Quick Service Validation
```bash
# Start the service
dotnet run --project src/QaliTrack.MasterData.Api --urls http://localhost:5001

# Test health endpoint
curl http://localhost:5001/health

# Test Django-style pagination
curl "http://localhost:5001/organization?page=1&page_size=10" | jq .

# Test search functionality  
curl "http://localhost:5001/organization?search=transport" | jq .

# Test filtering
curl "http://localhost:5001/driver?status=Active" | jq .

# Test ordering
curl "http://localhost:5001/driver?ordering=-createdAt" | jq .
```

#### Database Verification
```bash
# Check database was created (SQLite)
ls -la masterdata.db

# Check database schema (if SQLite installed)
sqlite3 masterdata.db ".schema" | head -20

# Check table creation
sqlite3 masterdata.db ".tables"
```

#### Module Configuration Testing
```bash
# Test with minimal modules
export ENABLED_MODULES=Organization,Driver
dotnet run --project src/QaliTrack.MasterData.Api --urls http://localhost:5002

# Check Swagger shows only enabled modules
curl -s http://localhost:5002/ | grep -o 'Driver\|Vehicle\|SACCO'
# Should only show 'Driver' (Vehicle and SACCO disabled)
```

#### Cross-Module Relationship Testing
```bash
# Create a test driver (replace with actual POST data)
curl -X POST http://localhost:5001/driver \
  -H "Content-Type: application/json" \
  -d '{"firstName":"John","lastName":"Doe","email":"john@example.com"}'

# Test cross-module endpoints  
curl http://localhost:5001/driver/{driver-id}/sacco-memberships
curl http://localhost:5001/driver/{driver-id}/vehicle-assignments
```

## 🔍 Health Monitoring

### Health Check Endpoints
- **Basic Health**: `GET /health`
- **Detailed Health**: `GET /health/detailed` (includes database connectivity)

### Monitoring
- All endpoints include correlation IDs for request tracking
- Structured logging with module context
- Performance metrics for cross-module queries

## 🚢 Deployment

### Environment Preparation
1. Set up PostgreSQL database (recommended for production)
2. Configure environment variables
3. Run database migrations
4. Deploy application

### Docker Deployment
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0
COPY . /app
WORKDIR /app
EXPOSE 5000
ENTRYPOINT ["dotnet", "QaliTrack.MasterData.Api.dll"]
```

## 📋 Migration from Microservices

This service consolidates the following original microservices:
- ✅ customer-service → BusinessEntities module
- ✅ driver-service → Driver module  
- ✅ product-service → Product module
- ✅ vehicle-service → Vehicle module
- ✅ supplier-service → BusinessEntities module
- ✅ transporter-service → BusinessEntities module
- ✅ sacco-service → SACCO module
- ✅ organization-service → Organization module
- ✅ route-service → Route module
- ✅ weighbridge-service → Weighbridge module
- ✅ user-service → User module (simplified)
- ✅ report-service → Reporting module

### Benefits of Consolidation
- **Reduced Complexity**: Single deployment unit
- **Better Performance**: Eliminated inter-service calls
- **Simplified Development**: Django-style modular architecture
- **Enhanced Relationships**: Proper cross-module data integrity
- **Unified Migrations**: Single database schema management

## 🤝 Contributing

1. Follow Django-style module patterns
2. Implement proper relationships using the Relationships module
3. Add comprehensive tests for new features
4. Update migrations for schema changes
5. Document API endpoints and relationships

## 📄 License

This project is part of the QaliTrack weighbridge management system.