# Customer Service

Customer Service microservice for the QaliTrack platform.

## Service Type: Masterdata

This service manages customer relationships, contacts, contracts, and related business operations following clean architecture principles.

## Features

- **Clean Architecture**: API, Core, and Infrastructure layers
- **Entity Framework**: SQLite database with EF Core
- **AutoMapper**: Object-to-object mapping
- **Swagger/OpenAPI**: API documentation
- **Logging**: Structured logging with Serilog
- **Health Checks**: Built-in monitoring

## Main Entities

The primary entities for this service are:
- `Customer` - Core customer information and relationships
- `Contact` - Customer contact persons and communication
- `Contract` - Customer contracts and renewals
- `Order` - Customer orders and status tracking

## Getting Started

### Prerequisites

- .NET 8.0 SDK
- SQLite (included)

### Running the Service

```bash
# Build the service
dotnet build

# Run the service
dotnet run --project src/CustomerService.Api

# Access the API
# Swagger UI: http://localhost:5001
# Health Check: http://localhost:5001/health
```

### Development

1. **Customize Entities**: Update entities in `src/CustomerService.Core/Entities/`
2. **Add Business Logic**: Implement services in `src/CustomerService.Core/Services/`
3. **Configure Database**: Modify `src/CustomerService.Infrastructure/Data/CustomerServiceDbContext.cs`
4. **Add Controllers**: Create API endpoints in `src/CustomerService.Api/Controllers/`

### API Endpoints

The service provides RESTful endpoints for Customer management:

- `GET /api/customers` - Get all customers
- `GET /api/customers/{id}` - Get customer by ID
- `POST /api/customers` - Create new customer
- `PUT /api/customers/{id}` - Update customer
- `DELETE /api/customers/{id}` - Delete customer
- `POST /api/customers/{id}/activate` - Activate customer
- `POST /api/customers/{id}/deactivate` - Deactivate customer

### Contact Management

- `GET /api/customers/{id}/contacts` - Get customer contacts
- `POST /api/customers/{id}/contacts` - Add contact to customer
- `GET /api/customers/{id}/contacts/primary` - Get primary contact

### Contract Management

- `GET /api/customers/{id}/contracts` - Get customer contracts
- `POST /api/customers/{id}/contracts` - Create contract for customer
- `GET /api/customers/{id}/contracts/active` - Get active contracts

### Integration with Gateway

This service is designed to work with the QaliTrack API Gateway:

- Routes are automatically configured via `/api/customers`
- Health checks are exposed for monitoring
- CORS is configured for cross-origin requests

## Project Structure

```
CustomerService/
├── src/
│   ├── CustomerService.Api/           # Web API layer
│   ├── CustomerService.Core/          # Business logic
│   └── CustomerService.Infrastructure/ # Data access
├── tests/
│   └── CustomerService.Tests/         # Unit & integration tests
├── Dockerfile                         # Container configuration
└── CustomerService.sln               # Solution file
```

## Testing

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test tests/CustomerService.Tests
```

## Docker

```bash
# Build Docker image
docker build -t customer-service .

# Run container
docker run -p 5001:80 customer-service
```

## Contributing

1. Follow clean architecture principles
2. Add comprehensive tests
3. Update documentation
4. Follow naming conventions

Generated with QaliTrack Service Template 🚀