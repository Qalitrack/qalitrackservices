---
_layout: landing
---

# QaliTrack Gateway Service

## Overview

The QaliTrack Gateway Service is a comprehensive API Gateway built on Ocelot that serves as the single entry point for all QaliTrack microservices. It provides centralized routing, authentication, authorization, monitoring, and service discovery capabilities.

## Architecture

The Gateway Service implements the API Gateway pattern with the following components:

- **Ocelot Framework**: Core routing and middleware engine
- **Authentication & Authorization**: JWT token validation and role-based access control
- **Service Discovery**: Dynamic service routing and health monitoring
- **Audit & Monitoring**: Comprehensive logging and performance tracking
- **Rate Limiting**: Request throttling and abuse protection
- **Load Balancing**: Intelligent request distribution

## Key Features

### Core Capabilities
- **Unified API Entry Point**: Single endpoint for all client applications
- **Dynamic Service Routing**: Route configuration based on service discovery
- **Health-Aware Routing**: Automatic failover to healthy service instances
- **Request/Response Transformation**: Data modification and enrichment
- **Protocol Translation**: REST to gRPC conversion support

### Security Features
- **JWT Authentication**: Token-based authentication with refresh support
- **Role-Based Authorization**: Fine-grained permission control
- **Rate Limiting**: Per-user and per-endpoint throttling
- **Request Validation**: Input sanitization and validation
- **CORS Support**: Cross-origin resource sharing configuration

### Monitoring & Observability
- **Comprehensive Audit Logging**: All requests and responses logged
- **Performance Metrics**: Response times, error rates, throughput
- **Health Checks**: Service availability monitoring
- **Distributed Tracing**: Request correlation across services
- **Real-time Dashboards**: Service status and performance visibility

### Service Management
- **Service Discovery**: Automatic service registration and discovery
- **Circuit Breaker**: Fault tolerance and resilience patterns
- **Retry Logic**: Automatic retry with backoff strategies
- **Caching**: Response caching for improved performance
- **Configuration Management**: Dynamic configuration updates

## Getting Started

### Prerequisites
- .NET 8.0 or later
- Docker (optional)
- Access to QaliTrack microservices

### Installation

1. Clone the repository
2. Navigate to the qalitrack-gateway directory
3. Restore dependencies: `dotnet restore`
4. Configure services in `ocelot.json`
5. Start the gateway: `dotnet run --project src/QaliTrack.Gateway.csproj`

### Configuration

The gateway uses Ocelot configuration files:
- `ocelot.json` - Main routing configuration
- `ocelot.development.json` - Development environment overrides
- `appsettings.json` - Application settings

## Components

### Controllers
- **GatewayController**: Main routing and proxy functionality
- **MonitoringController**: Health checks and service status
- **ServiceDiscoveryController**: Service registration and discovery
- **SwaggerController**: API documentation aggregation

### Services
- **AuditService**: Request/response logging and audit trails
- **AuthorizationCacheService**: Permission caching and validation
- **HealthAwareRoutingService**: Dynamic routing based on service health
- **ServiceHealthMonitor**: Continuous service health monitoring

### Middleware
- **RoleAuthorizationMiddleware**: Custom authorization logic
- **Global Exception Middleware**: Error handling and logging
- **Rate Limiting Middleware**: Request throttling
- **Audit Middleware**: Request/response audit logging

## Service Discovery

The gateway supports multiple service discovery mechanisms:

```json
{
  "Routes": [
    {
      "DownstreamPathTemplate": "/masterdata/{everything}",
      "DownstreamScheme": "https",
      "DownstreamHostAndPorts": [
        {
          "Host": "masterdata-service",
          "Port": 80
        }
      ],
      "UpstreamPathTemplate": "/api/masterdata/{everything}",
      "UpstreamHttpMethod": [ "GET", "POST", "PUT", "PATCH", "DELETE" ]
    }
  ]
}
```

## Authentication & Authorization

The gateway implements a comprehensive security model:

### JWT Authentication
- Token validation and verification
- Automatic token refresh
- Multi-tenant token support

### Role-Based Access Control
- Hierarchical role management
- Permission-based authorization
- Resource-level access control

### Security Headers
- CORS configuration
- Content Security Policy
- X-Frame-Options and security headers

## Monitoring & Logging

### Audit Logging
All requests are logged with:
- Request/response details
- User context and permissions
- Performance metrics
- Error tracking

### Health Monitoring
- Service availability checks
- Performance threshold monitoring
- Automatic alerting
- Health dashboard

## Deployment

The gateway supports multiple deployment scenarios:

### Docker Deployment
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0
COPY . /app
WORKDIR /app
EXPOSE 80
ENTRYPOINT ["dotnet", "QaliTrack.Gateway.dll"]
```

### Kubernetes Deployment
- Helm charts available
- Auto-scaling configuration
- Service mesh integration
- Load balancer configuration

## API Reference

For detailed API documentation, please refer to the [API Reference](api/index.md) section.

## Configuration Reference

- [Ocelot Configuration](docs/configuration/ocelot.md)
- [Service Discovery](docs/configuration/service-discovery.md)
- [Authentication](docs/configuration/authentication.md)
- [Rate Limiting](docs/configuration/rate-limiting.md)

## Support

For support and questions, please refer to the project documentation or contact the development team.