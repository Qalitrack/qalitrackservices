# QaliTrack API Gateway - API Reference

Complete API reference for the QaliTrack API Gateway, including authentication, service discovery, and administrative endpoints.

## 📋 Table of Contents

- [Base URL](#base-url)
- [Authentication](#authentication)
- [Gateway Endpoints](#gateway-endpoints)
- [Service Routing](#service-routing)
- [Status Codes](#status-codes)
- [Error Handling](#error-handling)
- [Rate Limiting](#rate-limiting)

## Base URL

- **Production**: `https://your-domain.com`
- **Testing**: `https://localhost:7000`
- **Development**: `http://localhost:7000`

## Authentication

All protected endpoints require JWT authentication via the Authorization header:

```http
Authorization: Bearer <jwt-token>
```

### Obtain JWT Token

#### Login Endpoint
```http
POST /api/auth/login
Content-Type: application/json

{
  "username": "string",
  "password": "string"
}
```

**Response (200 OK)**:
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": "user-uuid",
    "username": "admin",
    "email": "admin@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "roles": ["Admin"],
    "permissions": ["manage:users", "read:products", "write:products"]
  },
  "expiresAt": "2025-07-08T15:30:00Z"
}
```

#### Mock Authentication (Testing Only)
```http
POST /api/MockAuth/mock-login
Content-Type: application/json

{
  "username": "testuser",
  "role": "Admin"
}
```

**Available Mock Roles**: `Guest`, `User`, `Operator`, `Auditor`, `SiteManager`, `Admin`, `SuperAdmin`

## Gateway Endpoints

### Gateway Information

#### Get Gateway Info
```http
GET /api/gateway/info
```

**Authentication**: None required

**Response (200 OK)**:
```json
{
  "name": "QaliTrack API Gateway",
  "version": "1.0.0",
  "description": "Central entry point for all QaliTrack microservices",
  "services": [
    {
      "name": "User Service",
      "path": "/api/users",
      "port": 7001,
      "description": "Authentication and user management"
    },
    {
      "name": "Product Service", 
      "path": "/api/products",
      "port": 7005,
      "description": "Product master data"
    }
  ],
  "healthChecks": {
    "gateway": "/health",
    "services": "/services/{service-name}/health"
  },
  "documentation": "/swagger"
}
```

#### Get Service Discovery
```http
GET /api/gateway/services
```

**Authentication**: None required

**Response (200 OK)**:
```json
[
  {
    "name": "user-service",
    "url": "https://localhost:7001", 
    "health": "/services/users/health",
    "status": "Available",
    "description": "Authentication and user management service"
  },
  {
    "name": "product-service",
    "url": "https://localhost:7005",
    "health": "/services/products/health", 
    "status": "Available",
    "description": "Product master data service"
  }
]
```

### Health Monitoring

#### Gateway Health Check
```http
GET /health
```

**Authentication**: None required

**Response (200 OK)**:
```json
{
  "status": "Healthy",
  "totalDuration": "00:00:00.0234567",
  "entries": {
    "gateway": {
      "status": "Healthy",
      "duration": "00:00:00.0001234"
    },
    "user-service": {
      "status": "Healthy", 
      "duration": "00:00:00.0123456"
    },
    "product-service": {
      "status": "Degraded",
      "duration": "00:00:00.0098765",
      "description": "Service responding slowly"
    }
  }
}
```

#### Readiness Check
```http
GET /health/ready
```

**Response**: Returns 200 OK when all critical services are ready

#### Liveness Check  
```http
GET /health/live
```

**Response**: Returns 200 OK when gateway is running

### Swagger Documentation

#### Gateway Swagger UI
```http
GET /swagger
```

**Authentication**: None required
**Content-Type**: text/html

**Description**: Interactive API documentation for all services

#### Swagger JSON
```http
GET /swagger/v1/swagger.json
```

**Authentication**: None required
**Content-Type**: application/json

**Response**: OpenAPI 3.0 specification for the gateway

## Service Routing

The gateway routes requests to downstream services based on path patterns:

### Authentication Service
```http
# Route to User Service (Port 7001)
POST /api/auth/login              → http://user-service:7001/api/auth/login
POST /api/auth/register           → http://user-service:7001/api/auth/register
POST /api/auth/refresh            → http://user-service:7001/api/auth/refresh
DELETE /api/auth/logout           → http://user-service:7001/api/auth/logout
```

**Required Role**: None (public endpoints)

### User Management
```http
# Route to User Service (Port 7001)  
GET /api/users                    → http://user-service:7001/api/users
GET /api/users/{id}               → http://user-service:7001/api/users/{id}
POST /api/users                   → http://user-service:7001/api/users
PUT /api/users/{id}               → http://user-service:7001/api/users/{id}
DELETE /api/users/{id}            → http://user-service:7001/api/users/{id}
```

**Required Role**: 
- GET: `User`
- POST, PUT, DELETE: `Admin`

### Master Data Services

#### Product Management
```http
# Route to Product Service (Port 7005)
GET /api/products                 → http://product-service:7005/api/products
GET /api/products/{id}            → http://product-service:7005/api/products/{id}  
POST /api/products                → http://product-service:7005/api/products
PUT /api/products/{id}            → http://product-service:7005/api/products/{id}
DELETE /api/products/{id}         → http://product-service:7005/api/products/{id}
```

**Required Role**:
- GET: `User`
- POST, PUT: `Operator`
- DELETE: `Admin`

#### Customer Management
```http
# Route to Customer Service (Port 7008)
GET /api/customers                → http://customer-service:7008/api/customers
GET /api/customers/{id}           → http://customer-service:7008/api/customers/{id}
POST /api/customers               → http://customer-service:7008/api/customers
PUT /api/customers/{id}           → http://customer-service:7008/api/customers/{id}
DELETE /api/customers/{id}        → http://customer-service:7008/api/customers/{id}
```

**Required Role**: `Operator`

#### Vehicle Management
```http
# Route to Vehicle Service (Port 7003)
GET /api/vehicles                 → http://vehicle-service:7003/api/vehicles
POST /api/vehicles                → http://vehicle-service:7003/api/vehicles
PUT /api/vehicles/{id}            → http://vehicle-service:7003/api/vehicles/{id}
DELETE /api/vehicles/{id}         → http://vehicle-service:7003/api/vehicles/{id}
```

**Required Role**: `Operator`

### Operational Services

#### Weight Data Management
```http
# Route to Weight Data Service (Port 7012)
GET /api/weight-data              → http://weight-data-service:7012/api/weight-data
POST /api/weight-data             → http://weight-data-service:7012/api/weight-data
GET /api/weight-data/{id}         → http://weight-data-service:7012/api/weight-data/{id}
PUT /api/weight-data/{id}         → http://weight-data-service:7012/api/weight-data/{id}
```

**Required Role**: `Operator`

#### Transaction Processing
```http
# Route to Transaction Service (Port 7015)
GET /api/transactions             → http://transaction-service:7015/api/transactions
POST /api/transactions            → http://transaction-service:7015/api/transactions
GET /api/transactions/{id}        → http://transaction-service:7015/api/transactions/{id}
PUT /api/transactions/{id}        → http://transaction-service:7015/api/transactions/{id}
```

**Required Role**: `Operator`

### Analytics and Reporting

#### Compliance Monitoring
```http
# Route to Compliance Service (Port 7013)
GET /api/compliance/reports       → http://compliance-service:7013/api/compliance/reports
GET /api/compliance/status        → http://compliance-service:7013/api/compliance/status
GET /api/compliance/violations    → http://compliance-service:7013/api/compliance/violations
```

**Required Role**: `Auditor`

#### Business Analytics
```http
# Route to Analytics Service (Port 7016)
GET /api/analytics/sales          → http://analytics-service:7016/api/analytics/sales
GET /api/analytics/operations     → http://analytics-service:7016/api/analytics/operations
GET /api/analytics/performance    → http://analytics-service:7016/api/analytics/performance
```

**Required Role**: `SiteManager`

### Organization Management
```http
# Route to Organization Service (Port 7002)
GET /api/organizations            → http://organization-service:7002/api/organizations
POST /api/organizations           → http://organization-service:7002/api/organizations
PUT /api/organizations/{id}       → http://organization-service:7002/api/organizations/{id}
DELETE /api/organizations/{id}    → http://organization-service:7002/api/organizations/{id}
```

**Required Role**: `Admin`

## Status Codes

### Success Codes
| Code | Description | Usage |
|------|-------------|-------|
| **200** | OK | Successful GET, PUT requests |
| **201** | Created | Successful POST requests |
| **204** | No Content | Successful DELETE requests |

### Client Error Codes
| Code | Description | Common Causes |
|------|-------------|---------------|
| **400** | Bad Request | Invalid request format, missing fields |
| **401** | Unauthorized | Missing/invalid JWT token, token expired |
| **403** | Forbidden | Insufficient role privileges |
| **404** | Not Found | Service unavailable, invalid endpoint |
| **409** | Conflict | Duplicate data, business rule violation |
| **422** | Unprocessable Entity | Validation errors |
| **429** | Too Many Requests | Rate limiting triggered |

### Server Error Codes
| Code | Description | Common Causes |
|------|-------------|---------------|
| **500** | Internal Server Error | Gateway processing error |
| **502** | Bad Gateway | Downstream service unavailable |
| **503** | Service Unavailable | Service temporarily down |
| **504** | Gateway Timeout | Downstream service timeout |

## Error Handling

### Standard Error Response Format
```json
{
  "error": {
    "code": "INSUFFICIENT_PRIVILEGES",
    "message": "User does not have required role for this operation",
    "details": {
      "requiredRole": "Admin",
      "userRoles": ["User"],
      "endpoint": "/api/users/admin"
    },
    "timestamp": "2025-07-08T10:30:00Z",
    "traceId": "trace-uuid"
  }
}
```

### Common Error Codes

#### Authentication Errors
```json
{
  "error": {
    "code": "TOKEN_EXPIRED",
    "message": "JWT token has expired",
    "details": {
      "expiredAt": "2025-07-08T09:30:00Z",
      "currentTime": "2025-07-08T10:30:00Z"
    }
  }
}
```

#### Authorization Errors
```json
{
  "error": {
    "code": "INSUFFICIENT_PRIVILEGES", 
    "message": "User role 'User' does not meet minimum requirement 'Operator'",
    "details": {
      "userLevel": 1,
      "requiredLevel": 2,
      "service": "CustomerService"
    }
  }
}
```

#### Service Unavailable Errors
```json
{
  "error": {
    "code": "SERVICE_UNAVAILABLE",
    "message": "Downstream service is temporarily unavailable",
    "details": {
      "service": "product-service",
      "port": 7005,
      "lastHealthCheck": "2025-07-08T10:25:00Z"
    }
  }
}
```

## Rate Limiting

The gateway implements rate limiting to ensure fair usage:

### Rate Limits by Role

| Role | Requests/Minute | Burst Limit |
|------|----------------|-------------|
| **Guest** | 30 | 10 |
| **User** | 60 | 20 |
| **Operator** | 120 | 40 |
| **SiteManager** | 180 | 60 |
| **Admin** | 300 | 100 |
| **SuperAdmin** | Unlimited | Unlimited |

### Rate Limit Headers
```http
X-RateLimit-Limit: 60
X-RateLimit-Remaining: 45
X-RateLimit-Reset: 1625745600
X-RateLimit-Retry-After: 60
```

### Rate Limit Exceeded Response
```http
HTTP/1.1 429 Too Many Requests
Content-Type: application/json

{
  "error": {
    "code": "RATE_LIMIT_EXCEEDED",
    "message": "Rate limit exceeded for role 'User'",
    "details": {
      "limit": 60,
      "window": "1 minute",
      "retryAfter": 45
    }
  }
}
```

## Request/Response Examples

### Authentication Flow
```bash
# 1. Login
curl -X POST https://localhost:7000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "username": "operator1",
    "password": "secure123"
  }'

# Response
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": "uuid",
    "username": "operator1",
    "roles": ["Operator"]
  },
  "expiresAt": "2025-07-08T11:30:00Z"
}

# 2. Use token for subsequent requests
curl -X GET https://localhost:7000/api/products \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

### Service Discovery
```bash
# Check available services
curl -X GET https://localhost:7000/api/gateway/services

# Response
[
  {
    "name": "user-service",
    "url": "https://localhost:7001",
    "status": "Available",
    "health": "/services/users/health"
  },
  {
    "name": "product-service", 
    "url": "https://localhost:7005",
    "status": "Available",
    "health": "/services/products/health"
  }
]
```

### Health Monitoring
```bash
# Gateway health check
curl -X GET https://localhost:7000/health

# Response
{
  "status": "Healthy",
  "totalDuration": "00:00:00.0234567",
  "entries": {
    "user-service": {
      "status": "Healthy",
      "duration": "00:00:00.0123456"
    },
    "product-service": {
      "status": "Healthy", 
      "duration": "00:00:00.0098765"
    }
  }
}
```

## Integration Examples

### JavaScript/Node.js
```javascript
class QaliTrackGateway {
  constructor(baseUrl) {
    this.baseUrl = baseUrl;
    this.token = null;
  }

  async login(username, password) {
    const response = await fetch(`${this.baseUrl}/api/auth/login`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ username, password })
    });
    
    const data = await response.json();
    this.token = data.token;
    return data;
  }

  async getProducts() {
    const response = await fetch(`${this.baseUrl}/api/products`, {
      headers: { 'Authorization': `Bearer ${this.token}` }
    });
    return response.json();
  }
}

// Usage
const gateway = new QaliTrackGateway('https://localhost:7000');
await gateway.login('operator1', 'password');
const products = await gateway.getProducts();
```

### Python
```python
import requests

class QaliTrackGateway:
    def __init__(self, base_url):
        self.base_url = base_url
        self.token = None
    
    def login(self, username, password):
        response = requests.post(
            f"{self.base_url}/api/auth/login",
            json={"username": username, "password": password}
        )
        data = response.json()
        self.token = data["token"]
        return data
    
    def get_products(self):
        headers = {"Authorization": f"Bearer {self.token}"}
        response = requests.get(f"{self.base_url}/api/products", headers=headers)
        return response.json()

# Usage
gateway = QaliTrackGateway("https://localhost:7000")
gateway.login("operator1", "password")
products = gateway.get_products()
```

### C#/.NET
```csharp
public class QaliTrackGateway
{
    private readonly HttpClient _httpClient;
    private string _token;

    public QaliTrackGateway(string baseUrl)
    {
        _httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
    }

    public async Task<LoginResponse> LoginAsync(string username, string password)
    {
        var request = new { username, password };
        var response = await _httpClient.PostAsJsonAsync("/api/auth/login", request);
        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        _token = result.Token;
        _httpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);
        return result;
    }

    public async Task<List<Product>> GetProductsAsync()
    {
        var response = await _httpClient.GetAsync("/api/products");
        return await response.Content.ReadFromJsonAsync<List<Product>>();
    }
}
```

---

*This API reference provides complete documentation for integrating with the QaliTrack API Gateway, including authentication, service routing, error handling, and practical examples.*