# QaliTrack API Gateway - Database Schema

Database schema documentation for the QaliTrack API Gateway, including caching mechanisms, configuration storage, and related data structures.

## 📋 Table of Contents

- [Overview](#overview)
- [Data Storage Architecture](#data-storage-architecture)
- [In-Memory Cache Schema](#in-memory-cache-schema)
- [Configuration Storage](#configuration-storage)
- [JWT Token Structure](#jwt-token-structure)
- [Logging and Metrics Schema](#logging-and-metrics-schema)
- [Health Check Data](#health-check-data)

## Overview

The QaliTrack API Gateway is designed as a **stateless service** with minimal persistent data requirements. Instead of a traditional database, the gateway uses:

- **In-Memory Caching** for authorization rules and route mappings
- **File-Based Configuration** for service routing and client settings
- **External Token Validation** via JWT without local user storage
- **Structured Logging** for audit trails and monitoring

## Data Storage Architecture

### Storage Pattern Overview

```
┌─────────────────────────────────────────────────────────┐
│                Gateway Storage Model                    │
├─────────────────────────────────────────────────────────┤
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐      │
│  │ In-Memory   │  │ File-Based  │  │  External   │      │
│  │   Cache     │  │   Config    │  │   Storage   │      │
│  └─────────────┘  └─────────────┘  └─────────────┘      │
│       │                │                │               │
│   • Route Rules    • Ocelot JSON   • User Database      │
│   • Role Mapping   • Client YAML   • Audit Logs        │
│   • Service Info   • Environment   • Metrics Store     │
└─────────────────────────────────────────────────────────┘
```

### Gateway Data Flow

```
External User DB ──→ JWT Token ──→ Gateway ──→ In-Memory Cache
                                     │
                                     ├──→ Config Files
                                     │
                                     └──→ Downstream Services
```

## In-Memory Cache Schema

The gateway uses `IMemoryCache` for high-performance data access with TTL-based expiration.

### Cache Key Patterns

| Cache Key Pattern | Purpose | TTL | Example |
|------------------|---------|-----|---------|
| `route_role_{path}` | Route authorization rules | 5 min | `route_role_/api/products` |
| `service_health_{name}` | Service health status | 30 sec | `service_health_user-service` |
| `config_client_{code}` | Client configuration | 10 min | `config_client_testing` |
| `jwt_validation_{token_hash}` | JWT validation results | 1 min | `jwt_validation_abc123` |

### Route Role Cache Structure

```csharp
// Cache entry for route authorization rules
public class RouteRoleRequirement
{
    public string PathPattern { get; set; }           // "/api/products/{everything}"
    public List<string> RequiredRoles { get; set; }   // ["User", "Operator"]
    public string ServiceName { get; set; }           // "ProductService"
    public string Description { get; set; }           // "Product management endpoints"
    public DateTime CachedAt { get; set; }            // Cache timestamp
    public TimeSpan TTL { get; set; }                 // Time to live
}
```

**Example Cache Entry**:
```json
{
  "pathPattern": "/api/products/{everything}",
  "requiredRoles": ["User"],
  "serviceName": "ProductService", 
  "description": "Product catalog and management",
  "cachedAt": "2025-07-08T10:30:00Z",
  "ttl": "00:05:00"
}
```

### Service Health Cache Structure

```csharp
// Cache entry for service health information
public class ServiceHealthInfo
{
    public string ServiceName { get; set; }           // "user-service"
    public string Status { get; set; }                // "Healthy", "Degraded", "Unhealthy"
    public TimeSpan ResponseTime { get; set; }        // Health check duration
    public string Url { get; set; }                   // "http://user-service:7001"
    public DateTime LastChecked { get; set; }         // Last health check time
    public string ErrorMessage { get; set; }          // Error details if unhealthy
}
```

**Example Cache Entry**:
```json
{
  "serviceName": "user-service",
  "status": "Healthy",
  "responseTime": "00:00:00.0123456",
  "url": "http://user-service:7001",
  "lastChecked": "2025-07-08T10:30:00Z",
  "errorMessage": null
}
```

### Cache Management Operations

```csharp
// Cache access patterns
public class GatewayCacheService
{
    // Set route role requirement with TTL
    public void SetRouteRoleRequirement(string path, RouteRoleRequirement requirement)
    {
        var cacheKey = $"route_role_{path}";
        _cache.Set(cacheKey, requirement, TimeSpan.FromMinutes(5));
    }
    
    // Get cached route requirement
    public RouteRoleRequirement? GetRouteRoleRequirement(string path)
    {
        var cacheKey = $"route_role_{path}";
        return _cache.TryGetValue(cacheKey, out RouteRoleRequirement? cached) ? cached : null;
    }
    
    // Update service health
    public void UpdateServiceHealth(string serviceName, ServiceHealthInfo health)
    {
        var cacheKey = $"service_health_{serviceName}";
        _cache.Set(cacheKey, health, TimeSpan.FromSeconds(30));
    }
}
```

## Configuration Storage

### File-Based Configuration Schema

#### 1. Ocelot Configuration (ocelot.json)

```json
{
  "Routes": [
    {
      "DownstreamPathTemplate": "/api/products/{everything}",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [
        {
          "Host": "product-service",
          "Port": 7005
        }
      ],
      "UpstreamPathTemplate": "/api/products/{everything}",
      "UpstreamHttpMethod": ["GET", "POST", "PUT", "DELETE"],
      "Metadata": {
        "RequiredRoles": ["User"],
        "ServiceName": "ProductService",
        "Description": "Product management endpoints",
        "RateLimit": {
          "RequestsPerMinute": 60,
          "BurstLimit": 20
        }
      }
    }
  ],
  "GlobalConfiguration": {
    "BaseUrl": "https://localhost:7000",
    "RateLimitOptions": {
      "QuotaExceededMessage": "Rate limit exceeded",
      "HttpStatusCode": 429
    }
  }
}
```

#### 2. Client Configuration (YAML)

```yaml
# configs/clients/testing.yml
client:
  code: "testing"
  name: "Testing Environment"
  description: "Development and testing deployment"

services:
  gateway:
    enabled: true
    port: 7000
    replicas: 1
  
  user-service:
    enabled: true
    port: 7001
    replicas: 1
    health_check: "/health"
    
  product-service:
    enabled: true
    port: 7005
    replicas: 1
    health_check: "/health"

environment:
  use_mock_services: true
  log_level: "Debug"
  jwt_issuer: "MockUserService"
  
authorization:
  default_role: "User"
  role_hierarchy:
    Guest: 0
    User: 1
    Operator: 2
    Admin: 4
    SuperAdmin: 5
```

### Configuration Data Models

```csharp
// Client configuration model
public class ClientConfiguration
{
    public ClientInfo Client { get; set; }
    public Dictionary<string, ServiceConfig> Services { get; set; }
    public EnvironmentConfig Environment { get; set; }
    public AuthorizationConfig Authorization { get; set; }
}

public class ServiceConfig
{
    public bool Enabled { get; set; }
    public int Port { get; set; }
    public int Replicas { get; set; }
    public string HealthCheck { get; set; }
    public Dictionary<string, string> Environment { get; set; }
}

public class AuthorizationConfig
{
    public string DefaultRole { get; set; }
    public Dictionary<string, int> RoleHierarchy { get; set; }
    public List<RouteRule> RouteRules { get; set; }
}
```

## JWT Token Structure

### Token Claims Schema

```csharp
// JWT token claims structure
public class JwtTokenClaims
{
    public string Sub { get; set; }              // Subject (User ID)
    public string Name { get; set; }             // Username
    public string Email { get; set; }            // User email
    public List<string> Roles { get; set; }      // User roles
    public List<string> Permissions { get; set; } // Specific permissions
    public string Iss { get; set; }              // Issuer (UserService/MockUserService)
    public string Aud { get; set; }              // Audience
    public long Exp { get; set; }                // Expiration timestamp
    public long Iat { get; set; }                // Issued at timestamp
    public long Nbf { get; set; }                // Not before timestamp
}
```

### Token Validation Cache

```csharp
// Cached JWT validation result
public class JwtValidationResult
{
    public bool IsValid { get; set; }
    public string UserId { get; set; }
    public string Username { get; set; }
    public List<string> Roles { get; set; }
    public List<string> Permissions { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string ErrorMessage { get; set; }
    public DateTime CachedAt { get; set; }
}
```

**Example Token Payload**:
```json
{
  "sub": "550e8400-e29b-41d4-a716-446655440000",
  "name": "john.operator",
  "email": "john.operator@example.com",
  "role": "Operator",
  "permissions": [
    "read:products",
    "write:products", 
    "read:customers",
    "write:customers"
  ],
  "iss": "UserService",
  "aud": "UserService",
  "exp": 1625745600,
  "iat": 1625742000,
  "nbf": 1625742000
}
```

## Logging and Metrics Schema

### Structured Log Entry Format

```csharp
// Serilog structured log entry
public class GatewayLogEntry
{
    public DateTime Timestamp { get; set; }
    public string Level { get; set; }           // Information, Warning, Error
    public string Message { get; set; }
    public string RequestId { get; set; }       // Correlation ID
    public string UserId { get; set; }          // Authenticated user
    public string Method { get; set; }          // HTTP method
    public string Path { get; set; }            // Request path
    public int StatusCode { get; set; }         // Response status
    public long Duration { get; set; }          // Request duration (ms)
    public string UserAgent { get; set; }       // Client user agent
    public string RemoteIP { get; set; }        // Client IP address
    public Dictionary<string, object> Properties { get; set; } // Additional data
}
```

**Example Log Entry (JSON)**:
```json
{
  "timestamp": "2025-07-08T10:30:00.000Z",
  "level": "Information",
  "message": "Request completed successfully",
  "requestId": "req-uuid-123",
  "userId": "user-uuid-456",
  "method": "GET",
  "path": "/api/products",
  "statusCode": 200,
  "duration": 45,
  "userAgent": "Mozilla/5.0...",
  "remoteIP": "192.168.1.100",
  "properties": {
    "service": "ProductService",
    "roles": ["Operator"],
    "cacheHit": true
  }
}
```

### Metrics Collection Schema

```csharp
// Performance metrics structure
public class GatewayMetrics
{
    public string MetricName { get; set; }
    public double Value { get; set; }
    public DateTime Timestamp { get; set; }
    public Dictionary<string, string> Tags { get; set; }
}
```

**Common Metrics**:
```json
[
  {
    "metricName": "gateway.requests.total",
    "value": 1,
    "timestamp": "2025-07-08T10:30:00Z",
    "tags": {
      "service": "product-service",
      "method": "GET",
      "status": "200",
      "role": "Operator"
    }
  },
  {
    "metricName": "gateway.request.duration",
    "value": 45.5,
    "timestamp": "2025-07-08T10:30:00Z", 
    "tags": {
      "service": "product-service",
      "endpoint": "/api/products"
    }
  },
  {
    "metricName": "gateway.auth.failures",
    "value": 1,
    "timestamp": "2025-07-08T10:30:00Z",
    "tags": {
      "reason": "invalid_token",
      "endpoint": "/api/customers"
    }
  }
]
```

## Health Check Data

### Health Check Response Schema

```csharp
// Health check response structure
public class HealthCheckResponse
{
    public string Status { get; set; }           // Healthy, Degraded, Unhealthy
    public TimeSpan TotalDuration { get; set; }  // Total check duration
    public Dictionary<string, HealthEntry> Entries { get; set; }
}

public class HealthEntry
{
    public string Status { get; set; }
    public TimeSpan Duration { get; set; }
    public string Description { get; set; }
    public Dictionary<string, object> Data { get; set; }
    public List<string> Tags { get; set; }
}
```

**Example Health Check Data**:
```json
{
  "status": "Healthy",
  "totalDuration": "00:00:00.0234567",
  "entries": {
    "gateway": {
      "status": "Healthy",
      "duration": "00:00:00.0001234",
      "description": "Gateway is running normally",
      "data": {
        "memoryUsage": "89.5 MB",
        "uptime": "2 days, 3 hours",
        "activeConnections": 15
      }
    },
    "user-service": {
      "status": "Healthy",
      "duration": "00:00:00.0123456",
      "description": "User service is responsive",
      "data": {
        "url": "http://user-service:7001",
        "responseTime": "12ms"
      }
    },
    "product-service": {
      "status": "Degraded", 
      "duration": "00:00:00.0456789",
      "description": "Service responding slowly",
      "data": {
        "url": "http://product-service:7005",
        "responseTime": "456ms",
        "warningThreshold": "200ms"
      }
    }
  }
}
```

## Data Relationships

### Entity Relationship Overview

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│ Client Config   │────│ Service Config  │────│ Route Rules     │
│                 │    │                 │    │                 │
│ • client_code   │    │ • service_name  │    │ • path_pattern  │
│ • environment   │    │ • enabled       │    │ • required_roles│
│ • services[]    │    │ • port          │    │ • service_name  │
└─────────────────┘    └─────────────────┘    └─────────────────┘
         │                       │                       │
         └───────────────────────┼───────────────────────┘
                                 │
                    ┌─────────────────┐
                    │ Cache Entries   │
                    │                 │
                    │ • route_roles   │
                    │ • service_health│
                    │ • jwt_validation│
                    └─────────────────┘
```

### Configuration Inheritance

```yaml
# Configuration precedence (highest to lowest):
1. Environment Variables
   └── JWT_SECRET_KEY, CLIENT_CODE, USE_MOCK_SERVICES

2. Client-Specific YAML
   └── configs/clients/{client_code}.yml
   
3. Ocelot JSON Configuration  
   └── ocelot.json, ocelot.development.json
   
4. Application Defaults
   └── Built-in default values
```

## Performance Considerations

### Cache Optimization

```csharp
// Cache size and performance tuning
public void ConfigureCache(IServiceCollection services)
{
    services.AddMemoryCache(options =>
    {
        options.SizeLimit = 1000;              // Max 1000 entries
        options.CompactionPercentage = 0.25;   // Remove 25% when full
        options.ExpirationScanFrequency = TimeSpan.FromMinutes(1); // Cleanup frequency
    });
}
```

### Data Size Estimates

| Data Type | Entry Size | Count | Total Memory |
|-----------|------------|-------|--------------|
| Route Rules | ~200 bytes | 50 | ~10 KB |
| Service Health | ~150 bytes | 20 | ~3 KB |
| JWT Validation | ~300 bytes | 100 | ~30 KB |
| **Total Cache** | | | **~43 KB** |

### Cleanup and Maintenance

```csharp
// Automatic cache cleanup and monitoring
public class CacheMaintenanceService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // Monitor cache usage
            var cacheStats = GetCacheStatistics();
            _logger.LogInformation("Cache usage: {Stats}", cacheStats);
            
            // Force cleanup if memory usage is high
            if (cacheStats.MemoryUsage > 0.8)
            {
                _cache.Compact(0.5); // Remove 50% of entries
            }
            
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}
```

---

*This database schema documentation provides comprehensive coverage of all data structures, caching mechanisms, and storage patterns used by the QaliTrack API Gateway.*