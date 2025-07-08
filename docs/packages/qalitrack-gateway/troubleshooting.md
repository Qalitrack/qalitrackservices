# QaliTrack API Gateway - Troubleshooting Guide

Comprehensive troubleshooting guide for common issues, error resolution, and diagnostic procedures for the QaliTrack API Gateway.

## 📋 Table of Contents

- [Quick Diagnostics](#quick-diagnostics)
- [Authentication Issues](#authentication-issues)
- [Authorization Problems](#authorization-problems)
- [Service Discovery Issues](#service-discovery-issues)
- [Performance Problems](#performance-problems)
- [Configuration Issues](#configuration-issues)
- [Network and Connectivity](#network-and-connectivity)
- [Debugging Tools](#debugging-tools)

## Quick Diagnostics

### Health Check Commands

Start with these commands to assess system health:

```bash
# 1. Gateway health
curl http://localhost:7000/health

# 2. Service discovery
curl http://localhost:7000/api/gateway/services

# 3. Gateway info
curl http://localhost:7000/api/gateway/info

# 4. Test authentication
curl -X POST http://localhost:7000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"password"}'

# 5. Check logs
docker logs qalitrack-gateway
```

### Common Error Indicators

| Status Code | Common Cause | Quick Fix |
|-------------|--------------|-----------|
| **401** | Token expired or invalid | Re-authenticate |
| **403** | Insufficient role privileges | Check user role assignment |
| **404** | Service not found | Verify service deployment |
| **502** | Downstream service unavailable | Check service health |
| **503** | Gateway overloaded | Reduce request rate |

## Authentication Issues

### Problem: 401 Unauthorized

#### Symptoms
```http
HTTP/1.1 401 Unauthorized
{
  "error": {
    "code": "TOKEN_EXPIRED",
    "message": "JWT token has expired"
  }
}
```

#### Diagnosis Steps
```bash
# 1. Check token expiration
echo $JWT_TOKEN | cut -d. -f2 | base64 -d | jq .exp

# 2. Verify current time
date +%s

# 3. Test with fresh token
curl -X POST http://localhost:7000/api/MockAuth/mock-login \
  -H "Content-Type: application/json" \
  -d '{"username":"testuser","role":"User"}'
```

#### Solutions

**Solution 1: Token Expired**
```bash
# Get new token
TOKEN=$(curl -X POST http://localhost:7000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"your-username","password":"your-password"}' \
  | jq -r .token)

# Use new token
curl -H "Authorization: Bearer $TOKEN" http://localhost:7000/api/products
```

**Solution 2: Invalid Token Format**
```bash
# Verify token structure
echo $TOKEN | grep -E '^[A-Za-z0-9_-]+\.[A-Za-z0-9_-]+\.[A-Za-z0-9_-]+$'

# If invalid, regenerate token
curl -X POST http://localhost:7000/api/MockAuth/mock-login \
  -H "Content-Type: application/json" \
  -d '{"username":"testuser","role":"User"}'
```

**Solution 3: Wrong Issuer/Audience**
```bash
# Check JWT configuration
grep -A 5 -B 5 "Jwt" appsettings.json

# Verify issuer in token
echo $TOKEN | cut -d. -f2 | base64 -d | jq .iss
```

### Problem: Invalid JWT Signature

#### Symptoms
```
Authentication failed: Invalid signature
```

#### Diagnosis
```bash
# 1. Check secret key configuration
echo $JWT_SECRET_KEY

# 2. Verify token hasn't been tampered with
echo $ORIGINAL_TOKEN
echo $CURRENT_TOKEN
diff <(echo $ORIGINAL_TOKEN) <(echo $CURRENT_TOKEN)
```

#### Solutions

**Solution 1: Regenerate Token**
```bash
# Use mock authentication for testing
curl -X POST http://localhost:7001/api/MockAuth/mock-login \
  -H "Content-Type: application/json" \
  -d '{"username":"testuser","role":"Admin"}' \
  | jq -r .token
```

**Solution 2: Verify Secret Key**
```csharp
// Check appsettings.json
{
  "Jwt": {
    "SecretKey": "YourSuperSecretKeyForJWTWhichShouldBeAtLeast32CharactersLong!",
    "Issuer": "UserService",
    "Audience": "UserService"
  }
}
```

## Authorization Problems

### Problem: 403 Forbidden

#### Symptoms
```http
HTTP/1.1 403 Forbidden
{
  "error": {
    "code": "INSUFFICIENT_PRIVILEGES",
    "message": "User role 'User' does not meet minimum requirement 'Operator'"
  }
}
```

#### Diagnosis Steps
```bash
# 1. Check user role in token
echo $TOKEN | cut -d. -f2 | base64 -d | jq .role

# 2. Verify endpoint requirements
curl http://localhost:7000/api/gateway/info | jq .services

# 3. Test with higher role
curl -X POST http://localhost:7001/api/MockAuth/mock-login \
  -H "Content-Type: application/json" \
  -d '{"username":"testuser","role":"Admin"}'
```

#### Solutions

**Solution 1: Role Upgrade Required**
```bash
# Contact administrator for role assignment
# Or use mock service for testing:
curl -X POST http://localhost:7001/api/MockAuth/mock-login \
  -H "Content-Type: application/json" \
  -d '{"username":"testuser","role":"Operator"}'
```

**Solution 2: Verify Role Hierarchy**
```
Role Levels:
- SuperAdmin (5) - Full access
- Admin (4) - Administrative access
- SiteManager (3) - Site management
- Operator (2) - Operations
- User (1) - Basic access
- Guest (0) - Public only
```

**Solution 3: Check Service-Specific Permissions**
```bash
# Some services have special role requirements
# Check documentation for specific endpoint requirements

# Example: Compliance service requires Auditor role
curl -H "Authorization: Bearer $AUDITOR_TOKEN" \
     http://localhost:7000/api/compliance/reports
```

### Problem: Role Not Recognized

#### Symptoms
```
User has no valid roles for authorization
```

#### Diagnosis
```bash
# 1. Check roles in token
echo $TOKEN | cut -d. -f2 | base64 -d | jq .role

# 2. Check available roles
curl http://localhost:7001/api/MockAuth/roles
```

#### Solutions

**Solution 1: Update Token with Valid Role**
```bash
# Valid roles: Guest, User, Operator, Auditor, SiteManager, Admin, SuperAdmin
curl -X POST http://localhost:7001/api/MockAuth/mock-login \
  -H "Content-Type: application/json" \
  -d '{"username":"testuser","role":"Operator"}'
```

## Service Discovery Issues

### Problem: 502 Bad Gateway

#### Symptoms
```http
HTTP/1.1 502 Bad Gateway
{
  "error": {
    "code": "SERVICE_UNAVAILABLE",
    "message": "Downstream service is temporarily unavailable"
  }
}
```

#### Diagnosis Steps
```bash
# 1. Check service health
curl http://localhost:7000/health

# 2. Check specific service
curl http://localhost:7005/health  # Product service

# 3. Check Docker containers
docker ps | grep qalitrack

# 4. Check service logs
docker logs user-service
docker logs product-service
```

#### Solutions

**Solution 1: Start Missing Services**
```bash
# Start all services
make start-mock

# Or start specific service
docker-compose -f docker-compose.testing.yml up user-service -d
```

**Solution 2: Service Recovery**
```bash
# Restart unhealthy service
docker restart user-service

# Wait for service to be ready
sleep 10
curl http://localhost:7001/health
```

**Solution 3: Use Mock Services**
```bash
# Switch to mock mode for testing
export USE_MOCK_SERVICES=true
make start-mock
```

### Problem: Service Not Found (404)

#### Symptoms
```http
HTTP/1.1 404 Not Found
```

#### Diagnosis
```bash
# 1. Check Ocelot routing configuration
cat ocelot.json | jq .Routes

# 2. Verify service is configured
curl http://localhost:7000/api/gateway/services

# 3. Check client configuration
cat configs/clients/testing.yml
```

#### Solutions

**Solution 1: Add Service to Configuration**
```yaml
# In configs/clients/testing.yml
services:
  product-service:
    enabled: true
    port: 7005
```

**Solution 2: Update Ocelot Configuration**
```json
{
  "Routes": [
    {
      "DownstreamPathTemplate": "/api/products/{everything}",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [
        {"Host": "product-service", "Port": 7005}
      ],
      "UpstreamPathTemplate": "/api/products/{everything}"
    }
  ]
}
```

## Performance Problems

### Problem: Slow Response Times

#### Symptoms
- Response times > 1000ms
- Intermittent timeouts
- High CPU/memory usage

#### Diagnosis
```bash
# 1. Check gateway performance
curl -w "@curl-format.txt" http://localhost:7000/api/products

# Create curl-format.txt:
# time_namelookup:  %{time_namelookup}\n
# time_connect:     %{time_connect}\n
# time_appconnect:  %{time_appconnect}\n
# time_pretransfer: %{time_pretransfer}\n
# time_redirect:    %{time_redirect}\n
# time_starttransfer: %{time_starttransfer}\n
# time_total:       %{time_total}\n

# 2. Check resource usage
docker stats qalitrack-gateway

# 3. Check service health response times
curl http://localhost:7000/health | jq .entries
```

#### Solutions

**Solution 1: Increase Timeout Values**
```json
// appsettings.json
{
  "HttpClient": {
    "TimeoutSeconds": 60
  },
  "HealthChecks": {
    "TimeoutSeconds": 30
  }
}
```

**Solution 2: Optimize Cache Settings**
```csharp
// Increase cache size and TTL
services.AddMemoryCache(options =>
{
    options.SizeLimit = 2000;  // Increase from 1000
    options.CompactionPercentage = 0.1;  // Reduce compaction
});
```

**Solution 3: Scale Services**
```bash
# Scale up specific service
docker-compose -f docker-compose.testing.yml up --scale product-service=3 -d
```

### Problem: Memory Leaks

#### Symptoms
- Continuously increasing memory usage
- OutOfMemory exceptions
- Degraded performance over time

#### Diagnosis
```bash
# 1. Monitor memory usage
docker stats --no-stream qalitrack-gateway

# 2. Check for memory leaks in logs
docker logs qalitrack-gateway | grep -i "memory\|gc\|heap"

# 3. Use .NET diagnostic tools
dotnet-dump collect -p $(pgrep -f QaliTrack.Gateway)
```

#### Solutions

**Solution 1: Configure Memory Limits**
```yaml
# docker-compose.yml
services:
  gateway:
    mem_limit: 256m
    memswap_limit: 256m
```

**Solution 2: Optimize Cache Management**
```csharp
// Set appropriate cache limits
services.AddMemoryCache(options =>
{
    options.SizeLimit = 1000;
    options.CompactionPercentage = 0.25;
    options.ExpirationScanFrequency = TimeSpan.FromMinutes(1);
});
```

## Configuration Issues

### Problem: Configuration Not Loading

#### Symptoms
```
Configuration section 'Jwt' not found
```

#### Diagnosis
```bash
# 1. Check configuration files exist
ls -la appsettings*.json
ls -la configs/clients/

# 2. Check environment variables
env | grep QALITRACK
env | grep JWT

# 3. Verify file permissions
ls -la configs/clients/testing.yml
```

#### Solutions

**Solution 1: Create Missing Configuration**
```json
// appsettings.json
{
  "Jwt": {
    "SecretKey": "YourSuperSecretKeyForJWTWhichShouldBeAtLeast32CharactersLong!",
    "Issuer": "UserService",
    "Audience": "UserService"
  }
}
```

**Solution 2: Set Environment Variables**
```bash
export JWT_SECRET_KEY="YourSuperSecretKeyForJWTWhichShouldBeAtLeast32CharactersLong!"
export CLIENT_CODE="testing"
export USE_MOCK_SERVICES="true"
```

**Solution 3: Fix File Permissions**
```bash
chmod 644 configs/clients/testing.yml
chown $USER:$USER configs/clients/testing.yml
```

### Problem: Ocelot Configuration Errors

#### Symptoms
```
Could not start Ocelot, errors are: Unable to start Ocelot
```

#### Diagnosis
```bash
# 1. Validate JSON syntax
jq . ocelot.json

# 2. Check for required fields
cat ocelot.json | jq .Routes[0]

# 3. Verify environment-specific files
ls -la ocelot*.json
```

#### Solutions

**Solution 1: Fix JSON Syntax**
```bash
# Use jq to validate and format
jq . ocelot.json > ocelot.json.tmp && mv ocelot.json.tmp ocelot.json
```

**Solution 2: Minimal Working Configuration**
```json
{
  "Routes": [
    {
      "DownstreamPathTemplate": "/api/users/{everything}",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [
        {"Host": "user-service", "Port": 7001}
      ],
      "UpstreamPathTemplate": "/api/users/{everything}",
      "UpstreamHttpMethod": ["GET", "POST", "PUT", "DELETE"]
    }
  ],
  "GlobalConfiguration": {
    "BaseUrl": "https://localhost:7000"
  }
}
```

## Network and Connectivity

### Problem: DNS Resolution Issues

#### Symptoms
```
Name or service not known: user-service
```

#### Diagnosis
```bash
# 1. Check Docker network
docker network ls
docker network inspect qalitrack-network

# 2. Test connectivity between containers
docker exec qalitrack-gateway ping user-service

# 3. Check service names in docker-compose
grep -A 5 -B 5 "container_name\|service" docker-compose.yml
```

#### Solutions

**Solution 1: Use Correct Service Names**
```yaml
# Ensure service names match in docker-compose.yml
services:
  gateway:
    container_name: qalitrack-gateway
  user-service:
    container_name: user-service
```

**Solution 2: Add Network Configuration**
```yaml
# docker-compose.yml
networks:
  qalitrack-network:
    driver: bridge

services:
  gateway:
    networks:
      - qalitrack-network
  user-service:
    networks:
      - qalitrack-network
```

### Problem: Port Conflicts

#### Symptoms
```
Port 7000 is already in use
```

#### Diagnosis
```bash
# 1. Check what's using the port
netstat -tulpn | grep :7000
lsof -i :7000

# 2. Check Docker port mappings
docker ps --format "table {{.Names}}\t{{.Ports}}"
```

#### Solutions

**Solution 1: Stop Conflicting Process**
```bash
# Kill process using the port
sudo kill $(lsof -t -i:7000)

# Or stop specific Docker container
docker stop $(docker ps -q --filter "expose=7000")
```

**Solution 2: Use Different Port**
```yaml
# docker-compose.yml
services:
  gateway:
    ports:
      - "7100:7000"  # Use port 7100 instead
```

## Debugging Tools

### Debug Commands

```bash
# 1. Gateway logs with timestamp
docker logs -f --timestamps qalitrack-gateway

# 2. Filter logs by level
docker logs qalitrack-gateway 2>&1 | grep -E "(ERROR|WARN)"

# 3. Follow logs from multiple services
docker-compose -f docker-compose.testing.yml logs -f gateway user-service

# 4. Check container resources
docker stats --no-stream --format "table {{.Container}}\t{{.CPUPerc}}\t{{.MemUsage}}"

# 5. Inspect container configuration
docker inspect qalitrack-gateway | jq '.[0].Config'
```

### Advanced Debugging

#### Enable Debug Logging
```json
// appsettings.Development.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Warning",
      "Ocelot": "Debug"
    }
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Debug",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    }
  }
}
```

#### JWT Token Debugging
```bash
# Decode JWT token payload
decode_jwt() {
    local token=$1
    echo $token | cut -d. -f2 | base64 -d | jq .
}

# Usage
decode_jwt $YOUR_JWT_TOKEN
```

#### Network Debugging
```bash
# Test connectivity from gateway container
docker exec qalitrack-gateway curl -I http://user-service:7001/health

# Check DNS resolution
docker exec qalitrack-gateway nslookup user-service

# Test specific endpoints
docker exec qalitrack-gateway curl -v http://user-service:7001/api/MockAuth/roles
```

### Monitoring and Alerting

#### Health Check Monitoring
```bash
#!/bin/bash
# health-monitor.sh

GATEWAY_URL="http://localhost:7000"
ALERT_EMAIL="admin@company.com"

check_health() {
    local response=$(curl -s -o /dev/null -w "%{http_code}" $GATEWAY_URL/health)
    
    if [ "$response" != "200" ]; then
        echo "ALERT: Gateway health check failed (HTTP $response)" | \
            mail -s "QaliTrack Gateway Alert" $ALERT_EMAIL
        return 1
    fi
    
    return 0
}

# Run health check
if ! check_health; then
    echo "Health check failed, attempting recovery..."
    docker restart qalitrack-gateway
    sleep 30
    check_health
fi
```

#### Performance Monitoring
```bash
#!/bin/bash
# performance-monitor.sh

monitor_performance() {
    local endpoint="$1"
    local max_response_time="$2"
    
    local response_time=$(curl -o /dev/null -s -w "%{time_total}" $endpoint)
    local response_time_ms=$(echo "$response_time * 1000" | bc)
    
    if (( $(echo "$response_time_ms > $max_response_time" | bc -l) )); then
        echo "ALERT: Slow response time: ${response_time_ms}ms for $endpoint"
        return 1
    fi
    
    echo "Response time OK: ${response_time_ms}ms for $endpoint"
    return 0
}

# Monitor key endpoints
monitor_performance "http://localhost:7000/health" 1000
monitor_performance "http://localhost:7000/api/gateway/info" 2000
```

### Common Debugging Scenarios

#### Scenario 1: Gateway Won't Start

```bash
# Checklist:
1. Check port availability: netstat -tulpn | grep :7000
2. Verify configuration: jq . appsettings.json
3. Check dependencies: docker ps
4. Review logs: docker logs qalitrack-gateway
5. Test minimal config: Use basic ocelot.json
```

#### Scenario 2: Authentication Works But Authorization Fails

```bash
# Checklist:
1. Decode JWT token: echo $TOKEN | cut -d. -f2 | base64 -d | jq .
2. Check role in token: echo $TOKEN | cut -d. -f2 | base64 -d | jq .role
3. Verify role hierarchy: Check user guide
4. Test with higher role: Use mock auth with Admin role
5. Check endpoint requirements: Review API documentation
```

#### Scenario 3: Services Not Reachable

```bash
# Checklist:
1. Check service health: curl http://localhost:700X/health
2. Verify Docker network: docker network inspect qalitrack-network
3. Test connectivity: docker exec gateway ping service-name
4. Check service discovery: curl http://localhost:7000/api/gateway/services
5. Review routing config: jq .Routes ocelot.json
```

---

*This troubleshooting guide provides systematic approaches to diagnose and resolve common issues with the QaliTrack API Gateway, ensuring reliable operation and quick problem resolution.*