# Customer Service

## Overview

The Customer Service is a comprehensive microservice within the QaliTrack ecosystem designed to manage all customer-related data and operations. It provides a complete customer lifecycle management system including customer registration, contact management, contract administration, billing information, and credit management.

## Features

### Core Customer Management
- **Customer Registration & Profiles**: Register new customers with comprehensive business information
- **Customer Status Management**: Activate, deactivate, and manage customer lifecycle states
- **Search & Discovery**: Advanced search capabilities across customer database
- **Customer Details**: Comprehensive customer profile management with full relationship data

### Contact Management
- **Multiple Contacts**: Support for multiple contacts per customer with role-based categorization
- **Contact Types**: Business, Technical, Billing, and Emergency contact classification
- **Contact Hierarchy**: Primary and secondary contact designation
- **Contact Lifecycle**: Active/inactive contact status management

### Contract Administration
- **Contract Creation**: Comprehensive contract management with terms and conditions
- **Contract Types**: Support for Service, Maintenance, and Lease contract types
- **Contract Lifecycle**: Draft, Active, Expired, and Terminated status tracking
- **Auto-Renewal**: Configurable automatic contract renewal capabilities
- **Contract Compliance**: Track contract terms, signatures, and renewal dates

### Billing & Financial Management
- **Billing Profiles**: Complete billing contact and address management
- **Payment Methods**: Track preferred payment methods and terms
- **Tax Management**: Tax exemption handling and tax number validation
- **Multi-Currency**: Support for international customers with currency preferences
- **Discount Management**: Configurable customer-specific discount percentages

### Credit Management
- **Credit Limits**: Configurable credit limits with real-time usage tracking
- **Credit Monitoring**: Available vs. used credit calculations
- **Payment History**: Comprehensive payment tracking and analysis
- **Credit References**: Multiple credit reference management
- **Security Deposits**: Track and manage customer security deposits

## Technology Stack

- **.NET 8**: Modern C# framework for high-performance APIs
- **ASP.NET Core**: RESTful API framework with built-in dependency injection
- **Entity Framework Core**: ORM with SQLite database for development
- **AutoMapper**: Object-to-object mapping for DTOs
- **FluentValidation**: Comprehensive input validation framework
- **Swagger/OpenAPI**: API documentation and testing interface
- **xUnit**: Unit and integration testing framework

## Project Structure

```
CustomerService/
├── src/
│   ├── CustomerService.Api/          # Web API layer
│   │   ├── Controllers/              # REST API controllers
│   │   │   ├── CustomersController.cs
│   │   │   ├── ContactsController.cs
│   │   │   └── ContractsController.cs
│   │   ├── Program.cs               # Application startup
│   │   └── appsettings.json         # Configuration
│   ├── CustomerService.Core/        # Business logic layer
│   │   ├── Entities/                # Domain entities
│   │   ├── DTOs/                    # Data transfer objects
│   │   ├── Interfaces/              # Service contracts
│   │   ├── Services/                # Business logic services
│   │   ├── Validators/              # Input validation rules
│   │   └── Mappings/                # AutoMapper profiles
│   └── CustomerService.Infrastructure/ # Data access layer
│       ├── Data/                    # Database context
│       └── Repositories/            # Data access repositories
└── tests/
    └── CustomerService.Tests/       # Comprehensive test suite
```

## Core Entities

### Customer
Central customer entity with business information, contact details, and relationships to all subsidiary entities.

### CustomerContact
Individual contact persons associated with customers, supporting multiple contact types and role-based access.

### CustomerContract
Contract management entities supporting various contract types, terms, and lifecycle management.

### CustomerBilling
Billing-specific information including addresses, payment preferences, and tax details.

### CustomerCredit
Credit management with limits, usage tracking, and payment history.

## Key API Endpoints

### Customer Management
- `GET /api/customers` - Retrieve customers with pagination and search
- `POST /api/customers` - Register new customer
- `GET /api/customers/{id}` - Get specific customer details
- `PUT /api/customers/{id}` - Update customer information
- `DELETE /api/customers/{id}` - Delete customer record
- `POST /api/customers/{id}/activate` - Activate customer account
- `POST /api/customers/{id}/deactivate` - Deactivate customer account
- `GET /api/customers/{id}/details` - Get comprehensive customer details

### Contact Management
- `GET /api/customers/{id}/contacts` - Get customer contacts
- `POST /api/customers/{id}/contacts` - Add new contact
- `PUT /api/contacts/{id}` - Update contact information
- `DELETE /api/contacts/{id}` - Remove contact

### Contract Management
- `GET /api/customers/{id}/contracts` - Get customer contracts
- `POST /api/customers/{id}/contracts` - Create new contract
- `PUT /api/contracts/{id}` - Update contract details
- `DELETE /api/contracts/{id}` - Remove contract

### Billing & Credit Management
- `GET /api/customers/{id}/billing` - Get billing information
- `PUT /api/customers/{id}/billing` - Update billing details
- `GET /api/customers/{id}/credit` - Get credit information
- `PUT /api/customers/{id}/credit` - Update credit settings

## Configuration

### Database Connection
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=customerservice.db"
  }
}
```

### Service Registration
The service uses dependency injection for all components:
- Repository pattern for data access
- Service layer for business logic
- AutoMapper for DTO mapping
- FluentValidation for input validation

## Running the Service

### Development Environment
```bash
cd src/CustomerService.Api
dotnet run
```

### Access Points
- **API**: http://localhost:5000
- **Swagger UI**: http://localhost:5000/swagger
- **Health Check**: http://localhost:5000/health

### Database
- **Type**: SQLite (development)
- **File**: `customerservice.db`
- **Auto-Creation**: Database is created automatically on first run

## Authentication & Authorization

The service implements QaliTrack's standard authentication patterns:
- **Organization Context**: `X-Organization-Id` header required
- **User Context**: `X-User-Id` header for audit trails
- **Base Controller**: Provides authentication utilities

## Integration Points

### Upstream Services
- **User Service**: Customer user account linkage
- **Organization Service**: Multi-tenant organization context

### Downstream Consumers
- **Weight Data Service**: Customer identification for weighbridge operations
- **Transaction Service**: Customer billing and invoice generation
- **Compliance Service**: Customer-specific compliance requirements

## Default Data & Roles

### Customer Types
- Individual
- Corporate
- Government
- NonProfit

### Customer Status
- Active: Fully operational customer
- Inactive: Temporarily disabled
- Suspended: Compliance or payment issues
- Pending: Awaiting activation approval

### Contact Types
- Business: General business contact
- Technical: Technical point of contact
- Billing: Financial and billing contact
- Emergency: Emergency contact person

### Contract Types
- Service: Ongoing service agreements
- Maintenance: Equipment maintenance contracts
- Lease: Equipment or facility lease agreements

### Contract Status
- Draft: Contract in preparation
- Active: Currently valid contract
- Expired: Contract past end date
- Terminated: Contract cancelled before end date

## Security Considerations

### Data Protection
- Customer PII is handled according to data protection regulations
- Credit information requires elevated permissions
- Contact data is encrypted in transit and at rest

### Input Validation
- Comprehensive validation using FluentValidation
- SQL injection prevention through parameterized queries
- XSS protection through input sanitization

### Audit Trail
- All customer modifications are logged with user context
- Timestamp tracking for all operations
- Integration with QaliTrack's centralized audit system

## Development Guidelines

### Code Standards
- Follow established QaliTrack C# coding conventions
- Implement comprehensive error handling
- Use async/await for all database operations
- Maintain high test coverage (>90%)

### API Design
- RESTful endpoint design
- Consistent error response formats
- Comprehensive API documentation
- Version management for breaking changes

## Testing

### Test Categories
- **Unit Tests**: Business logic validation
- **Integration Tests**: Database and API testing
- **Security Tests**: Authentication and authorization
- **Performance Tests**: Load and stress testing

### Running Tests
```bash
cd tests/CustomerService.Tests
dotnet test
```

## Performance Considerations

### Database Optimization
- Indexed fields: TaxNumber, RegistrationNumber, ContactEmail
- Efficient pagination for large customer datasets
- Optimized queries for customer search functionality

### Caching Strategy
- Customer profile caching for frequently accessed data
- Contract status caching for compliance checks
- Credit limit caching for real-time validation

### Scalability
- Stateless service design for horizontal scaling
- Efficient database connection pooling
- Asynchronous processing for non-critical operations

## Monitoring & Health Checks

### Health Endpoints
- `/health`: Basic service health
- Database connectivity verification
- Integration with QaliTrack monitoring infrastructure

### Logging
- Structured logging using Serilog
- Integration with centralized log aggregation
- Performance metrics and error tracking

## Related Documentation

- [API Reference](../../apps/technical/datamanager/services/customer-service/api-reference.md)
- [Database Schema](../../apps/technical/datamanager/services/customer-service/database-schema.md)
- [Business Requirements](../../requirements/customer-service/README.md)
- [Integration Guide](../../apps/technical/datamanager/services/customer-service/integration-guide.md)
- [Testing Guide](../../apps/technical/datamanager/services/customer-service/testing-guide.md)