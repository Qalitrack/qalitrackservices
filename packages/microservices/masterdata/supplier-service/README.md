# Supplier Service

The Supplier Service is a microservice that manages all supplier-related data and operations within the QaliTrack platform.

## Features

### Core Supplier Management
- Supplier registration and authentication
- Supplier profile management (name, code, contact information, type, status)
- Supplier verification system
- Search and filtering capabilities

### Address Management
- Multiple addresses per supplier (Main, Warehouse, Billing, Shipping, Manufacturing)
- Default address management
- Geographic coordinates support

### Product Catalog Management
- Supplier-specific product catalog
- Product availability status tracking
- Stock level management
- Preferred product designation
- Lead time and minimum order quantity tracking

### Dynamic Pricing System
- Multiple pricing types (Standard, Volume, Contract, Promotional, Seasonal, Special)
- Quantity-based pricing tiers
- Time-based pricing validity
- Discount management (percentage and fixed amount)
- Active pricing calculation

### Performance Tracking
- Multiple performance metrics (Delivery Time, Quality, Response Time, Reliability, Price Competitiveness, Communication)
- Period-based performance tracking (Week, Month, Quarter, Year)
- Performance scoring (0-100 scale)
- Performance summary and grading system

## Architecture

The service follows Clean Architecture principles with the following layers:

### Core Layer (`SupplierService.Core`)
- **Entities**: Domain models (Supplier, Address, SupplierProduct, SupplierPricing, SupplierPerformance)
- **Interfaces**: Repository and service contracts
- **DTOs**: Data transfer objects for API communication
- **Services**: Business logic implementation
- **Mappings**: AutoMapper profiles for entity-DTO mapping

### Infrastructure Layer (`SupplierService.Infrastructure`)
- **Data**: Entity Framework DbContext and database configuration
- **Repositories**: Data access implementations

### API Layer (`SupplierService.Api`)
- **Controllers**: REST API endpoints
- **Program.cs**: Application configuration and dependency injection

### Tests (`SupplierService.Tests`)
- Unit tests for services and repositories
- Integration tests for API endpoints

## API Endpoints

### Suppliers
- `GET /api/suppliers` - Get all suppliers
- `GET /api/suppliers/{id}` - Get supplier by ID
- `GET /api/suppliers/code/{code}` - Get supplier by code
- `POST /api/suppliers` - Create new supplier
- `PUT /api/suppliers/{id}` - Update supplier
- `DELETE /api/suppliers/{id}` - Delete supplier
- `GET /api/suppliers/search?term={term}` - Search suppliers
- `GET /api/suppliers/status/{status}` - Get suppliers by status
- `GET /api/suppliers/type/{type}` - Get suppliers by type
- `POST /api/suppliers/{id}/verify` - Verify supplier

### Addresses
- `GET /api/suppliers/{id}/addresses` - Get supplier addresses
- `POST /api/suppliers/{id}/addresses` - Create supplier address
- `PUT /api/suppliers/{supplierId}/addresses/{addressId}` - Update address
- `DELETE /api/suppliers/{supplierId}/addresses/{addressId}` - Delete address
- `GET /api/addresses/{id}` - Get address by ID
- `POST /api/addresses/{id}/set-default` - Set address as default

### Supplier Products
- `GET /api/suppliers/{id}/products` - Get supplier products
- `POST /api/suppliers/{id}/products` - Create supplier product
- `PUT /api/suppliers/{supplierId}/products/{productId}` - Update supplier product
- `DELETE /api/suppliers/{supplierId}/products/{productId}` - Delete supplier product
- `GET /api/supplierproducts/{id}` - Get supplier product by ID
- `POST /api/supplierproducts/{id}/stock` - Update product stock
- `POST /api/supplierproducts/{id}/preferred` - Set product as preferred

### Pricing
- `GET /api/supplierproducts/{id}/pricing` - Get product pricing
- `POST /api/supplierproducts/{id}/pricing` - Create product pricing
- `GET /api/supplierproducts/{id}/pricing/active` - Get active pricing
- `GET /api/supplierproducts/{id}/pricing/calculate` - Calculate effective price
- `GET /api/pricing/{id}` - Get pricing by ID
- `PUT /api/pricing/{id}` - Update pricing
- `DELETE /api/pricing/{id}` - Delete pricing
- `GET /api/pricing/promotional` - Get promotional pricing
- `GET /api/pricing/expiring` - Get expiring pricing

### Performance
- `GET /api/suppliers/{id}/performance` - Get supplier performance
- `POST /api/suppliers/{id}/performance` - Create performance record
- `GET /api/suppliers/{id}/performance/summary` - Get performance summary

## Database Schema

### Suppliers Table
- Id (Primary Key)
- Name (Required, Max 200 chars)
- Code (Required, Unique, Max 50 chars)
- ContactPerson (Max 200 chars)
- Email (Max 100 chars)
- Phone (Max 20 chars)
- Status (Enum: Active, Inactive, Suspended, Pending)
- Type (Enum: Manufacturer, Distributor, Wholesaler, Retailer, ServiceProvider)
- Description (Max 1000 chars)
- Website (Max 200 chars)
- TaxIdentificationNumber (Max 50 chars)
- IsVerified (Boolean)
- VerificationDate (DateTime, nullable)
- CreatedAt, UpdatedAt, IsDeleted (Base entity fields)

### Addresses Table
- Id (Primary Key)
- SupplierId (Foreign Key)
- Type (Enum: Main, Warehouse, Billing, Shipping, Manufacturing)
- Street (Required, Max 200 chars)
- City (Required, Max 100 chars)
- State (Max 100 chars)
- PostalCode (Max 20 chars)
- Country (Required, Max 100 chars)
- AdditionalInfo (Max 200 chars)
- IsDefault (Boolean)
- Latitude, Longitude (Double, nullable)
- CreatedAt, UpdatedAt, IsDeleted (Base entity fields)

### SupplierProducts Table
- Id (Primary Key)
- SupplierId (Foreign Key)
- ProductId (Required)
- SupplierSKU (Max 100 chars)
- LeadTime (Integer, days)
- MinOrderQuantity (Integer)
- IsPreferred (Boolean)
- Status (Enum: Available, Limited, Unavailable, Discontinued, BackOrdered)
- Notes (Max 1000 chars)
- ReorderLevel (Integer)
- CurrentStock (Integer)
- LastRestockDate (DateTime, nullable)
- NextDeliveryDate (DateTime, nullable)
- CreatedAt, UpdatedAt, IsDeleted (Base entity fields)

### SupplierPricing Table
- Id (Primary Key)
- SupplierProductId (Foreign Key)
- Type (Enum: Standard, Volume, Contract, Promotional, Seasonal, Special)
- Amount (Decimal)
- Currency (Max 3 chars)
- MinQuantity (Integer)
- MaxQuantity (Integer, nullable)
- ValidFrom (DateTime)
- ValidTo (DateTime, nullable)
- IsActive (Boolean)
- Priority (Integer)
- DiscountPercentage (Decimal, nullable)
- DiscountAmount (Decimal, nullable)
- Notes (Max 200 chars)
- CreatedAt, UpdatedAt, IsDeleted (Base entity fields)

### SupplierPerformances Table
- Id (Primary Key)
- SupplierId (Foreign Key)
- MetricType (Enum: DeliveryTime, Quality, ResponseTime, Reliability, PriceCompetitiveness, Communication, Overall)
- Score (Decimal, 0-100 scale)
- Period (Enum: Week, Month, Quarter, Year)
- PeriodStart (DateTime)
- PeriodEnd (DateTime)
- Notes (Max 1000 chars)
- DataPoints (Integer)
- CreatedAt, UpdatedAt, IsDeleted (Base entity fields)

## Configuration

### Database
The service uses Entity Framework Core with SQLite for development and can be configured for other databases in production.

Connection string: `Data Source=supplier.db`

### Dependencies
- ASP.NET Core 8.0
- Entity Framework Core
- AutoMapper
- Swagger/OpenAPI

## Running the Service

1. Ensure .NET 8 SDK is installed
2. Navigate to the service directory
3. Run `dotnet restore` to restore packages
4. Run `dotnet build` to build the solution
5. Run `dotnet run --project src/SupplierService.Api` to start the service
6. Access Swagger UI at `https://localhost:7006/swagger`

## Testing

Run unit tests with:
```bash
dotnet test
```

Use the provided `SupplierService.Api.http` file for API testing with REST clients.

## Integration

The Supplier Service integrates with:
- **Product Service**: For product information and validation
- **Customer Service**: For customer-supplier relationships
- **Order Service**: For order processing and supplier selection

## Performance Considerations

- Database indexes on frequently queried fields (Name, Code, Email, Status, Type)
- Efficient pricing calculation with proper query optimization
- Pagination support for large datasets
- Caching for frequently accessed supplier information

## Security

- Input validation on all API endpoints
- Proper error handling without information leakage
- Audit trail for all supplier modifications
- Role-based access control (to be implemented)

## Future Enhancements

- Real-time notifications for stock level changes
- Integration with external supplier systems
- Advanced analytics and reporting
- Supplier onboarding workflow
- Document management for supplier certifications
- Multi-currency support with exchange rate handling