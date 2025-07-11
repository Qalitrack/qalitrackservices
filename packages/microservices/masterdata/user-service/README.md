# UserService

UserService microservice for the QaliTrack platform.

## Service Type: Masterdata

This service was generated from the QaliTrack service template and follows clean architecture principles.

## Features

- **Clean Architecture**: API, Core, and Infrastructure layers
- **Entity Framework**: SQLite database with EF Core
- **AutoMapper**: Object-to-object mapping
- **Swagger/OpenAPI**: API documentation
- **Logging**: Structured logging with Serilog
- **Health Checks**: Built-in monitoring

## Main Entity

The primary entity for this service is `User`.

## Getting Started

### Prerequisites

- .NET 8.0 SDK
- SQLite (included)

### Running the Service

```bash
# Build the service
dotnet build

# Run the service
dotnet run --project src/UserService.Api

# Access the API
# Swagger UI: http://localhost:5000
# Health Check: http://localhost:5000/health
```

### Development

1. **Customize Entities**: Update entities in `src/UserService.Core/Entities/`
2. **Add Business Logic**: Implement services in `src/UserService.Core/Services/`
3. **Configure Database**: Modify `src/UserService.Infrastructure/Data/UserServiceDbContext.cs`
4. **Add Controllers**: Create API endpoints in `src/UserService.Api/Controllers/`

### API Endpoints

The service provides RESTful endpoints for User management:

- `GET /api/users` - Get all users
- `GET /api/users/{id}` - Get user by ID
- `POST /api/users` - Create new user
- `PUT /api/users/{id}` - Update user
- `DELETE /api/users/{id}` - Delete user

### Integration with Gateway

This service is designed to work with the QaliTrack API Gateway:

- Routes are automatically configured
- Health checks are exposed for monitoring
- CORS is configured for cross-origin requests

## Project Structure

```
UserService/
├── src/
│   ├── UserService.Api/           # Web API layer
│   ├── UserService.Core/          # Business logic
│   └── UserService.Infrastructure/ # Data access
├── tests/
│   └── UserService.Tests/         # Unit & integration tests
├── Dockerfile                          # Container configuration
└── UserService.sln               # Solution file
```

## Testing

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test tests/UserService.Tests
```

## Docker

```bash
# Build Docker image
docker build -t userservice .

# Run container
docker run -p 5000:8080 userservice
```

## Contributing

1. Follow clean architecture principles
2. Add comprehensive tests
3. Update documentation
4. Follow naming conventions

Generated with QaliTrack Service Template 🚀
