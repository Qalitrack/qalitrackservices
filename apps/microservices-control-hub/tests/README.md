# 🧪 Testing Microservices Control Hub

This directory contains test fixtures and sample services to verify that the microservices control hub correctly:

1. **Performs health checks** on both HTTP and TCP services
2. **Aggregates Swagger documentation** from multiple services  
3. **Applies API root prefixes** correctly in the unified documentation
4. **Handles different service groups** (application, infrastructure)

## 🚀 Quick Test

Run the complete test environment:

```bash
# Start test services
docker-compose -f tests/docker-compose.test.yml up -d

# Wait for all services to be ready
docker-compose -f tests/docker-compose.test.yml logs -f microservices-control-hub

# Access the control hub
open http://localhost:3000
```

## 📋 Test Scenarios

### Health Check Testing

The test environment includes:

- **3 Sample HTTP Services** with `/health` endpoints
- **1 Redis instance** for TCP health check testing  
- **1 PostgreSQL instance** for TCP health check testing

**Expected Results:**
- All services should show as "healthy" in the dashboard
- HTTP services should respond to GET /health with 200 status
- TCP services should accept connections on their respective ports

### Swagger Aggregation Testing

Each sample service exposes:
- Swagger specification at `/swagger/v1/swagger.json`
- Different API root prefixes (`/api/service1`, `/api/service2`, `/api/service3`)
- Sample REST endpoints for items management

**Expected Results:**
- Unified Swagger UI at http://localhost:3000/docs
- All 3 services should appear with proper tags
- API paths should be prefixed correctly with their respective API roots
- No schema conflicts between services

### Configuration Testing

The test configuration demonstrates:
- **Service grouping** (application vs infrastructure)
- **Selective monitoring** (enabled/disabled services)  
- **API root configuration** for different path prefixes
- **Mixed service types** (HTTP health checks vs TCP health checks)

## 🔍 Manual Testing Steps

### 1. Verify Dashboard

Visit http://localhost:3000 and confirm:

- [ ] Overall system health shows all services
- [ ] Service cards display correct status (healthy/unhealthy)
- [ ] Services are grouped properly (Application, Infrastructure)
- [ ] Real-time updates work (status changes reflect quickly)

### 2. Test Health API

```bash
# Check overall health
curl http://localhost:3000/api/health | jq

# Check specific service health  
curl http://localhost:3000/api/health/sample-service-1 | jq

# Check health statistics
curl http://localhost:3000/api/health/stats/summary | jq
```

### 3. Test Documentation API

```bash
# Get aggregated Swagger spec
curl http://localhost:3000/api/docs/swagger.json | jq

# List documented services
curl http://localhost:3000/api/docs/services | jq

# Get service-specific documentation
curl http://localhost:3000/api/docs/services/sample-service-1/swagger.json | jq
```

### 4. Test Service Discovery

```bash
# List all services with status
curl http://localhost:3000/api/services | jq

# Get current configuration  
curl http://localhost:3000/api/services/config | jq

# Get services grouped by type
curl http://localhost:3000/api/services/groups | jq
```

### 5. Test Sample Service APIs

```bash
# Test sample service endpoints (through external URLs)
curl http://localhost:3000/api/service1/items | jq
curl http://localhost:3000/api/service2/items | jq  
curl http://localhost:3000/api/service3/items | jq

# Test creating items
curl -X POST http://localhost:3000/api/service1/items \
  -H "Content-Type: application/json" \
  -d '{"name":"Test Item","description":"Created via API"}' | jq
```

## 🧹 Cleanup

Stop and remove the test environment:

```bash
docker-compose -f tests/docker-compose.test.yml down
docker-compose -f tests/docker-compose.test.yml down --volumes --rmi all
```

## 📁 Test Files

- `sample-service/` - Simple Node.js service with health and swagger endpoints
- `config/test-services.json` - Service configuration with API root examples  
- `docker-compose.test.yml` - Complete test environment setup
- `README.md` - This testing guide

## 🔧 Customizing Tests

### Adding New Test Services

1. Create a new service directory under `tests/`
2. Add the service to `config/test-services.json` 
3. Include the service in `docker-compose.test.yml`
4. Configure appropriate `apiRoot` and health check paths

### Testing Different Configurations

1. Copy `config/test-services.json` to a new file
2. Modify service configurations (hosts, ports, API roots, etc.)
3. Update the Docker compose volume mount to use the new config
4. Restart the control hub service

This allows testing different service discovery patterns and configuration scenarios.