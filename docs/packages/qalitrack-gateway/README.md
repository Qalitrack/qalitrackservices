# QaliTrack API Gateway

A high-performance, secure API Gateway for the QaliTrack weighbridge management system, built with .NET 8 and Ocelot. The gateway serves as the central entry point for all QaliTrack microservices, providing unified authentication, authorization, service discovery, and request routing.

## 🚀 Quick Start

### Prerequisites
- .NET 8 SDK
- Docker (for containerized deployment)
- Valid JWT configuration

### Development Setup

```bash
# 1. Navigate to gateway source
cd packages/qalitrack-gateway/src

# 2. Restore dependencies
dotnet restore

# 3. Configure JWT settings (appsettings.json)
{
  "Jwt": {
    "SecretKey": "YourSuperSecretKeyForJWTWhichShouldBeAtLeast32CharactersLong!",
    "Issuer": "UserService",
    "Audience": "UserService"
  }
}

# 4. Run the gateway
dotnet run

# 5. Access the gateway
# Gateway: https://localhost:7000
# Swagger: https://localhost:7000/swagger
# Health: https://localhost:7000/health
```

### Docker Deployment

```bash
# Build gateway container
docker build -t qalitrack/gateway .

# Run with environment configuration
docker run -p 7000:7000 \
  -e Jwt__SecretKey="YourSecretKey" \
  -e CLIENT_CODE="testing" \
  qalitrack/gateway
```

## 🏗️ Architecture Overview

The QaliTrack API Gateway implements a sophisticated **hybrid authorization model** that combines:

- **Coarse-grained control** at the gateway level (service access)
- **Fine-grained control** within individual microservices (data access)
- **Role hierarchy** with inheritance (Guest < User < Operator < Admin < SuperAdmin)

### Core Components

| Component | Purpose | Technology |
|-----------|---------|------------|
| **Ocelot Gateway** | Request routing and load balancing | Ocelot 22.0.1 |
| **JWT Authentication** | Token-based security | Microsoft.AspNetCore.Authentication.JwtBearer |
| **Role Authorization Middleware** | Hierarchical role enforcement | Custom middleware |
| **Service Discovery** | Dynamic health monitoring | ASP.NET Core Health Checks |
| **Swagger Aggregation** | Unified API documentation | MMLib.SwaggerForOcelot |
| **Logging & Monitoring** | Comprehensive observability | Serilog |

### Service Routing Matrix

The gateway routes requests to 17+ microservices based on path patterns:

```
/api/auth/*           → User Service (Port 7001)
/api/users/*          → User Service (Port 7001)
/api/organizations/*  → Organization Service (Port 7002)
/api/vehicles/*       → Vehicle Service (Port 7003)
/api/drivers/*        → Driver Service (Port 7004)
/api/products/*       → Product Service (Port 7005)
/api/customers/*      → Customer Service (Port 7008)
/api/compliance/*     → Compliance Service (Port 7013)
/api/analytics/*      → Analytics Service (Port 7016)
# ... and more
```

## 🔐 Authorization System

### Role Hierarchy

```
SuperAdmin (Level 5) - System-wide administrative access
    ↓
Admin (Level 4) - Full organizational administrative access  
    ↓
SiteManager (Level 3) - Site-level management access
    ↓
Operator (Level 2) - Daily operational tasks
Auditor (Level 2) - Read-only compliance access
ClientAdmin (Level 2) - Organization-specific admin
    ↓
User (Level 1) - Basic authenticated access
    ↓
Guest (Level 0) - Public access only
```

### Authorization Flow

1. **Request arrives** at gateway with JWT token
2. **Authentication verification** validates token signature and claims
3. **Route analysis** determines required role for the target service
4. **Role hierarchy check** verifies user role meets minimum requirements
5. **User context forwarding** adds user headers for downstream services
6. **Request routing** forwards authenticated request to target service

### Example Authorization Rules

```yaml
# Products Service - User level access
/api/products/*:
  required_role: "User"
  description: "Product catalog access"

# Customer Management - Operator level access  
/api/customers/*:
  required_role: "Operator"
  description: "Customer management operations"

# System Administration - Admin level access
/api/organizations/*:
  required_role: "Admin" 
  description: "Organization management"
```

## 🧪 Testing & Quality Assurance

The gateway includes comprehensive testing with **104+ test cases** covering:

### Unit Tests (Role Matrix Testing)
- **Authentication Testing**: JWT validation, expiration, tampering detection
- **Authorization Matrix**: All roles vs all endpoints (104+ combinations)
- **Role Hierarchy**: Privilege level validation
- **Security Headers**: User context forwarding verification

### Integration Tests (Deployed Services)
- **Service Discovery**: Health check validation
- **End-to-End Authorization**: Real service communication
- **Mock vs Real Services**: Environment switching validation
- **Performance Testing**: Response time and throughput

```bash
# Run comprehensive gateway tests
make test-gateway                # Unit tests (104+ test cases)
make test-gateway-integration    # Integration tests with deployed services

# Test role matrix
make test-role-matrix           # Authorization matrix validation
```

## 📊 Service Discovery & Health Monitoring

### Dynamic Health Checks
The gateway automatically discovers and monitors services based on client configuration:

```csharp
// Health check endpoints
/health                    // Gateway health
/health/ready             // Readiness probe
/health/live              // Liveness probe
```

### Service Information API
```http
GET /api/gateway/info      # Gateway and service information
GET /api/gateway/services  # Service discovery data
```

## 🔧 Configuration

### Environment-Based Configuration

| Environment | Config File | Purpose |
|-------------|-------------|---------|
| **Development** | `ocelot.development.json` | Local development |
| **Testing** | `ocelot.json` | Integration testing |
| **Production** | Client-specific configs | Multi-tenant deployment |

### JWT Configuration
```json
{
  "Jwt": {
    "SecretKey": "32+ character secret key",
    "Issuer": "UserService",           // or "MockUserService" for testing
    "Audience": "UserService",         // or "MockUserService" for testing
    "ExpirationMinutes": 60
  }
}
```

### Mock vs Real Services
The gateway automatically detects service mode:

```bash
# Mock Mode (Fast testing)
USE_MOCK_SERVICES=true
# → JWT Issuer: MockUserService
# → Instant role switching
# → No database required

# Real Mode (Production-like)  
USE_MOCK_SERVICES=false
# → JWT Issuer: UserService
# → Database-backed authentication
# → Full user management
```

## 📈 Performance & Scalability

### Performance Characteristics
- **Response Time**: < 50ms for authenticated requests
- **Throughput**: 1000+ requests/second
- **Memory Usage**: < 100MB baseline
- **Role Authorization Caching**: 5-minute TTL for route requirements

### Scaling Considerations
- **Horizontal Scaling**: Stateless design enables multiple instances
- **Load Balancing**: Ocelot supports multiple downstream instances
- **Caching Strategy**: In-memory caching for authorization rules
- **Health Check Optimization**: Configurable check intervals

## 🚦 Getting Started Examples

### 1. Authentication
```bash
# Login to get JWT token
curl -X POST https://localhost:7000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username": "admin", "password": "password"}'

# Response
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {...},
  "expiresAt": "2025-07-08T15:30:00Z"
}
```

### 2. Using Gateway APIs
```bash
# Use JWT token for subsequent requests
curl -X GET https://localhost:7000/api/products \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."

# Gateway forwards with user context headers:
# X-User-ID: user-uuid
# X-User-Roles: Admin
# X-User-Email: user@example.com
# X-Gateway-Authorized: true
```

### 3. Health Monitoring
```bash
# Check gateway health
curl https://localhost:7000/health

# Check service discovery
curl https://localhost:7000/api/gateway/services
```

## 📚 Related Documentation

- [User Guide](./user-guide.md) - Business user documentation
- [API Reference](./api-reference.md) - Complete endpoint documentation  
- [Technical Architecture](./technical-architecture.md) - Implementation details
- [Integration Guide](./integration-guide.md) - Service integration patterns
- [Testing Guide](./testing-guide.md) - Testing strategies and examples
- [Troubleshooting](./troubleshooting.md) - Common issues and solutions

## 🛠️ Development & Contribution

### Project Structure
```
packages/qalitrack-gateway/
├── src/
│   ├── Controllers/          # Gateway endpoints
│   ├── Middleware/           # Authorization middleware
│   ├── Services/             # Configuration services
│   ├── Models/               # Data models
│   └── Program.cs            # Application bootstrap
├── tests/
│   ├── Authorization/        # Role matrix tests
│   ├── Integration/          # Deployed service tests
│   └── Services/             # Service validation tests
└── docs/                     # Documentation
```

### Contributing
1. Follow existing code patterns and conventions
2. Add comprehensive tests for new features
3. Update documentation for changes
4. Ensure security best practices

---

*QaliTrack API Gateway provides secure, scalable, and efficient access to all QaliTrack microservices, enabling comprehensive weighbridge management operations with enterprise-grade security and monitoring.*