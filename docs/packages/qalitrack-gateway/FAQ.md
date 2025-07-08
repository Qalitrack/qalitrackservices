# QaliTrack API Gateway - Frequently Asked Questions

Comprehensive FAQ covering common questions, best practices, and solutions for the QaliTrack API Gateway.

## 📋 Table of Contents

- [General Questions](#general-questions)
- [Authentication & Authorization](#authentication--authorization)
- [Service Configuration](#service-configuration)
- [Performance & Scalability](#performance--scalability)
- [Troubleshooting](#troubleshooting)
- [Development & Testing](#development--testing)
- [Deployment & Operations](#deployment--operations)
- [Integration](#integration)

## General Questions

### Q: What is the QaliTrack API Gateway?

**A:** The QaliTrack API Gateway is a centralized entry point for all QaliTrack microservices, providing unified authentication, authorization, service discovery, and request routing. It's built with .NET 8 and Ocelot, offering enterprise-grade security and scalability for weighbridge management operations.

### Q: How many microservices does the gateway support?

**A:** The gateway currently supports 17+ microservices organized into two categories:
- **Master Data Services (11)**: User, Organization, Vehicle, Driver, Product, Route, Weighbridge, Customer, Supplier, Transporter, Sacco
- **Data Management Services (8)**: Weight Data, Compliance, Operational Data, Transaction, Analytics, Data Sync, Archive

### Q: What are the system requirements?

**A:** 
- **.NET 8 Runtime** (for source deployment)
- **Docker** (for containerized deployment)
- **Minimum 256MB RAM** (512MB recommended for production)
- **1 CPU core** (2+ cores recommended for production)
- **Network access** to downstream services

### Q: Is the gateway stateless?

**A:** Yes, the gateway is completely stateless. It uses:
- JWT tokens for authentication (no session storage)
- In-memory caching with TTL (can be rebuilt from source)
- File-based configuration (externally mounted)
- No persistent database requirements

---

## Authentication & Authorization

### Q: How does authentication work?

**A:** The gateway uses JWT (JSON Web Token) authentication:

1. **Login**: Client authenticates with User Service via gateway
2. **Token**: Receives JWT token with user info and roles
3. **Requests**: Include token in `Authorization: Bearer <token>` header
4. **Validation**: Gateway validates token signature and expiration
5. **Forwarding**: Adds user context headers to downstream requests

```bash
# Example authentication flow
curl -X POST http://localhost:7000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"operator1","password":"password123"}'

# Use received token
curl -H "Authorization: Bearer <token>" \
     http://localhost:7000/api/products
```

### Q: What user roles are supported?

**A:** The gateway supports hierarchical roles with inheritance:

| Role | Level | Access | Use Case |
|------|-------|--------|----------|
| **Guest** | 0 | Public data only | Catalog browsing |
| **User** | 1 | Basic authenticated access | General system users |
| **Operator** | 2 | Daily operations | Weighbridge operators |
| **Auditor** | 2 | Compliance & analytics | Compliance officers |
| **SiteManager** | 3 | Site management | Site supervisors |
| **Admin** | 4 | Organization administration | IT administrators |
| **SuperAdmin** | 5 | System-wide control | System administrators |

### Q: How does role inheritance work?

**A:** Higher-level roles automatically inherit permissions from lower levels:
- **Admin** (Level 4) can access everything **Operator** (Level 2) can access
- **SuperAdmin** (Level 5) has complete system access
- **Auditor** (Level 2) has special read-only access to compliance services

### Q: Can I test different roles without creating users?

**A:** Yes! Use the mock authentication service for testing:

```bash
# Test as different roles instantly
curl -X POST http://localhost:7001/api/MockAuth/mock-login \
  -H "Content-Type: application/json" \
  -d '{"username":"testuser","role":"Admin"}'

# Available roles: Guest, User, Operator, Auditor, SiteManager, Admin, SuperAdmin
```

### Q: How long do tokens last?

**A:** 
- **Default expiration**: 1 hour
- **Configurable** in JWT settings
- **No automatic refresh** - clients must re-authenticate
- **Expired tokens** return 401 Unauthorized

### Q: What happens if my token expires?

**A:** 
1. Gateway returns `401 Unauthorized`
2. Client receives error with expiration details
3. Client must re-authenticate to get new token
4. Retry original request with new token

---

## Service Configuration

### Q: How do I add a new microservice to the gateway?

**A:** Follow these steps:

1. **Add to Client Configuration** (`configs/clients/your-client.yml`):
```yaml
services:
  new-service:
    enabled: true
    port: 7019
```

2. **Add Ocelot Route** (`ocelot.json`):
```json
{
  "DownstreamPathTemplate": "/api/newservice/{everything}",
  "DownstreamScheme": "http", 
  "DownstreamHostAndPorts": [{"Host": "new-service", "Port": 7019}],
  "UpstreamPathTemplate": "/api/newservice/{everything}",
  "Metadata": {
    "RequiredRoles": ["User"],
    "ServiceName": "NewService"
  }
}
```

3. **Deploy Service** with correct Docker network and port

### Q: How does service discovery work?

**A:** The gateway uses multiple discovery mechanisms:

- **Static Configuration**: Services defined in client YAML files
- **Health Checks**: Automatic health monitoring of configured services
- **Docker Networking**: Service name resolution in Docker environments
- **Consul Support**: Optional advanced service discovery (if configured)

### Q: Can I disable specific services?

**A:** Yes, set `enabled: false` in client configuration:

```yaml
# configs/clients/testing.yml
services:
  analytics-service:
    enabled: false  # This service won't be available
  product-service:
    enabled: true   # This service will be available
```

### Q: How do I configure different environments?

**A:** Use environment-specific configuration files:

```bash
# Development
export CLIENT_CODE=testing
export USE_MOCK_SERVICES=true

# Production  
export CLIENT_CODE=production
export USE_MOCK_SERVICES=false
```

---

## Performance & Scalability

### Q: What are the performance characteristics?

**A:** Current performance metrics:

- **Response Time**: ~25ms average, <50ms 95th percentile
- **Throughput**: 850+ requests/second sustained
- **Memory Usage**: 128MB baseline, 180MB peak
- **CPU Usage**: 35% average, 65% peak
- **Error Rate**: <0.1%

### Q: How do I scale the gateway?

**A:** Multiple scaling options:

1. **Horizontal Scaling**:
```yaml
# Docker Compose
services:
  gateway:
    deploy:
      replicas: 3  # Multiple instances
```

2. **Vertical Scaling**:
```yaml
services:
  gateway:
    deploy:
      resources:
        limits:
          memory: 512M  # Increase memory
          cpus: '1.0'   # Increase CPU
```

3. **Load Balancing**: Use nginx or cloud load balancer

### Q: How does caching work?

**A:** The gateway uses multi-level caching:

- **L1 Cache** (Memory): Route authorization rules (5-minute TTL)
- **L2 Cache** (Distributed): Service configuration (10-minute TTL)  
- **Health Check Cache**: Service status (30-second TTL)
- **JWT Validation Cache**: Token validation results (1-minute TTL)

### Q: What causes slow performance?

**A:** Common performance issues:

1. **Downstream Service Latency**: Slow microservices affect gateway response
2. **Network Issues**: Connectivity problems between services
3. **Cache Misses**: Cold cache requires more processing
4. **Resource Constraints**: Insufficient CPU/memory allocation
5. **Configuration Issues**: Inefficient routing or middleware setup

**Solutions**: Check [Performance Tuning Guide](./performance-tuning.md)

---

## Troubleshooting

### Q: I'm getting 401 Unauthorized errors. What's wrong?

**A:** Check these common causes:

1. **Missing Token**: Include `Authorization: Bearer <token>` header
2. **Expired Token**: Re-authenticate to get fresh token
3. **Invalid Token**: Verify token format and signature
4. **Wrong Issuer**: Check JWT configuration matches token issuer

```bash
# Debug token
echo $TOKEN | cut -d. -f2 | base64 -d | jq .

# Test with mock token
curl -X POST http://localhost:7001/api/MockAuth/mock-login \
  -d '{"username":"testuser","role":"User"}'
```

### Q: I'm getting 403 Forbidden errors. What's wrong?

**A:** This indicates insufficient role privileges:

1. **Check User Role**: Verify role in JWT token
2. **Check Endpoint Requirements**: Review API documentation
3. **Role Hierarchy**: Ensure user role meets minimum requirements
4. **Test with Higher Role**: Try Admin or SuperAdmin role

```bash
# Check role in token
echo $TOKEN | cut -d. -f2 | base64 -d | jq .role

# Test with admin role
curl -X POST http://localhost:7001/api/MockAuth/mock-login \
  -d '{"username":"testuser","role":"Admin"}'
```

### Q: Services are returning 502 Bad Gateway. What's wrong?

**A:** This indicates downstream service issues:

1. **Service Health**: Check if target service is running
2. **Network Connectivity**: Verify service can be reached
3. **Port Configuration**: Ensure correct port mapping
4. **Service Discovery**: Confirm service is properly configured

```bash
# Check service health
curl http://localhost:7000/health

# Check specific service
curl http://localhost:7005/health  # Product service

# Check Docker containers
docker ps | grep qalitrack
```

### Q: How do I enable debug logging?

**A:** Update logging configuration:

```json
// appsettings.Development.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "QaliTrack": "Debug",
      "Ocelot": "Debug"
    }
  }
}
```

```bash
# View logs with timestamps
docker logs -f --timestamps qalitrack-gateway

# Filter by level
docker logs qalitrack-gateway 2>&1 | grep -E "(ERROR|WARN)"
```

---

## Development & Testing

### Q: How do I set up a development environment?

**A:** Follow these steps:

1. **Clone Repository**:
```bash
git clone <repository-url>
cd qalitrackservices
```

2. **Start Mock Services**:
```bash
make start-mock
```

3. **Run Gateway**:
```bash
cd packages/qalitrack-gateway/src
dotnet run
```

4. **Test Endpoint**:
```bash
curl http://localhost:7000/health
```

### Q: How do I run the test suite?

**A:** Multiple test categories available:

```bash
# All tests
make test-gateway

# Specific categories
dotnet test --filter Category=Unit          # 104+ unit tests
dotnet test --filter Category=Integration   # 57 integration tests  
dotnet test --filter Category=Security      # 25 security tests

# Performance tests
dotnet test --filter Category=Performance
```

### Q: What's the difference between mock and real services?

**A:** 

| Aspect | Mock Services | Real Services |
|--------|---------------|---------------|
| **Startup Time** | ~10 seconds | ~30 seconds |
| **Authentication** | Instant role switching | Full user registration/login |
| **Database** | None (stateless) | SQLite/PostgreSQL |
| **Use Case** | Development, testing | Production, integration testing |
| **Data Persistence** | No | Yes |

### Q: How do I test role-based authorization?

**A:** Use the comprehensive role matrix testing:

```bash
# Test authorization matrix (104+ test combinations)
make test-role-matrix

# Test specific role
curl -X POST http://localhost:7001/api/MockAuth/mock-login \
  -d '{"username":"testuser","role":"Operator"}' | jq -r .token

# Use token to test endpoints
curl -H "Authorization: Bearer $TOKEN" \
     http://localhost:7000/api/customers
```

---

## Deployment & Operations

### Q: How do I deploy to production?

**A:** Use Docker Compose for production deployment:

1. **Build Images**:
```bash
docker build -t qalitrack/gateway packages/qalitrack-gateway
```

2. **Configure Environment**:
```bash
export CLIENT_CODE=production
export USE_MOCK_SERVICES=false
export JWT_SECRET_KEY="YourProductionSecretKey"
```

3. **Deploy with Compose**:
```bash
docker-compose -f docker-compose.production.yml up -d
```

4. **Verify Health**:
```bash
curl https://your-domain.com/health
```

### Q: How do I monitor the gateway in production?

**A:** Multiple monitoring approaches:

1. **Health Endpoints**:
```bash
curl https://your-domain.com/health        # Overall health
curl https://your-domain.com/health/ready  # Readiness probe
curl https://your-domain.com/health/live   # Liveness probe
```

2. **Metrics Collection**:
- Request counts by endpoint and role
- Response time histograms  
- Error rate tracking
- Resource utilization

3. **Logging**:
- Structured JSON logging with Serilog
- Request/response correlation IDs
- Performance metrics
- Security events

### Q: How do I handle gateway updates?

**A:** Use rolling deployment strategy:

1. **Zero-Downtime Updates**:
```bash
# Deploy new version alongside existing
docker-compose -f docker-compose.yml up --scale gateway=2 -d

# Health check new instance
curl http://new-gateway:7000/health

# Switch traffic and remove old instance
docker-compose -f docker-compose.yml up --scale gateway=1 -d
```

2. **Configuration Updates**:
- Update configuration files
- Restart gateway service
- Verify health checks pass

### Q: What backup and recovery procedures should I follow?

**A:** Gateway-specific backup considerations:

1. **Configuration Backup**:
```bash
# Backup configuration files
tar -czf gateway-config-$(date +%Y%m%d).tar.gz configs/ *.json

# Backup to cloud storage
aws s3 cp gateway-config-$(date +%Y%m%d).tar.gz s3://backups/
```

2. **Recovery Procedures**:
- Gateway is stateless - no data backup needed
- Restore configuration files
- Redeploy container with correct config
- Verify health and service connectivity

---

## Integration

### Q: How do I integrate my application with the gateway?

**A:** Use the provided SDKs or HTTP clients:

**JavaScript/TypeScript**:
```typescript
import { QaliTrackSDK } from '@qalitrack/sdk';

const sdk = new QaliTrackSDK('https://localhost:7000');
await sdk.login('username', 'password');
const products = await sdk.getProducts();
```

**Python**:
```python
from qalitrack_sdk import QaliTrackSDK

sdk = QaliTrackSDK('https://localhost:7000')
sdk.login('username', 'password')
products = sdk.get_products()
```

**C#**:
```csharp
using var sdk = new QaliTrackSDK("https://localhost:7000");
await sdk.LoginAsync("username", "password");
var products = await sdk.GetProductsAsync();
```

### Q: How do I handle authentication in my client application?

**A:** Implement proper token management:

1. **Login and Store Token**:
```javascript
const auth = await gateway.login(username, password);
localStorage.setItem('token', auth.token);
localStorage.setItem('expires', auth.expiresAt);
```

2. **Include Token in Requests**:
```javascript
const token = localStorage.getItem('token');
const response = await fetch('/api/products', {
  headers: { 'Authorization': `Bearer ${token}` }
});
```

3. **Handle Token Expiration**:
```javascript
if (response.status === 401) {
  // Token expired - redirect to login
  window.location.href = '/login';
}
```

### Q: Can I use the gateway with mobile applications?

**A:** Yes, the gateway supports all HTTP clients:

**iOS (Swift)**:
```swift
let token = "your-jwt-token"
var request = URLRequest(url: URL(string: "https://localhost:7000/api/products")!)
request.setValue("Bearer \(token)", forHTTPHeaderField: "Authorization")
```

**Android (Kotlin)**:
```kotlin
val client = OkHttpClient()
val request = Request.Builder()
    .url("https://localhost:7000/api/products")
    .addHeader("Authorization", "Bearer $token")
    .build()
```

### Q: How do I implement error handling in my client?

**A:** Handle standard HTTP status codes:

```typescript
class APIClient {
  async callAPI(endpoint: string): Promise<any> {
    try {
      const response = await fetch(endpoint);
      
      switch (response.status) {
        case 200: return response.json();
        case 401: throw new AuthenticationError('Token expired');
        case 403: throw new AuthorizationError('Insufficient privileges');
        case 404: throw new NotFoundError('Service not found');
        case 502: throw new ServiceUnavailableError('Service down');
        default: throw new APIError(`Unexpected error: ${response.status}`);
      }
    } catch (error) {
      // Handle network errors
      throw new NetworkError('Network connection failed');
    }
  }
}
```

### Q: How do I implement retry logic for resilient client applications?

**A:** Use exponential backoff for retries:

```typescript
class ResilientAPIClient {
  async callWithRetry<T>(apiCall: () => Promise<T>, maxRetries = 3): Promise<T> {
    for (let attempt = 1; attempt <= maxRetries; attempt++) {
      try {
        return await apiCall();
      } catch (error) {
        // Don't retry client errors (4xx) except 429
        if (error.status >= 400 && error.status < 500 && error.status !== 429) {
          throw error;
        }
        
        if (attempt === maxRetries) throw error;
        
        // Exponential backoff: 1s, 2s, 4s
        const delay = Math.min(1000 * Math.pow(2, attempt - 1), 10000);
        await new Promise(resolve => setTimeout(resolve, delay));
      }
    }
  }
}
```

---

**Additional Resources:**
- [User Guide](./user-guide.md) - Comprehensive user documentation
- [API Reference](./api-reference.md) - Complete endpoint documentation
- [Technical Architecture](./technical-architecture.md) - Implementation details
- [Integration Guide](./integration-guide.md) - Client integration examples
- [Troubleshooting Guide](./troubleshooting.md) - Problem resolution
- [Performance Tuning](./performance-tuning.md) - Optimization strategies

*This FAQ covers the most common questions and scenarios encountered when working with the QaliTrack API Gateway. For specific technical issues, consult the detailed guides referenced above.*