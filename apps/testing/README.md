# QaliTrack Testing Environment - QaliTrack Deployment

## Overview
- **Client**: QaliTrack Testing Environment
- **Code**: testing
- **Description**: Development and testing environment with minimal services
- **Environment**: development
- **Domain**: localhost

## Services (3 enabled)

### Enabled Services
- **gateway** (Port 7000)
- **user-service** (Port 7001)
- **service-discovery** (Port 7019)

### Disabled Services (17)
- **organization-service** - Not required for this deployment
- **vehicle-service** - Not required for this deployment
- **driver-service** - Not required for this deployment
- **product-service** - Not required for this deployment
- **route-service** - Not required for this deployment
- **weighbridge-service** - Not required for this deployment
- **customer-service** - Not required for this deployment
- **supplier-service** - Not required for this deployment
- **transporter-service** - Not required for this deployment
- **sacco-service** - Not required for this deployment
- **weight-data-service** - Not required for this deployment
- **compliance-service** - Not required for this deployment
- **operational-data-service** - Not required for this deployment
- **transaction-service** - Not required for this deployment
- **analytics-service** - Not required for this deployment
- **data-sync-service** - Not required for this deployment
- **archive-service** - Not required for this deployment

## Quick Start

```bash
# Start all services
./start-testing.sh

# View logs
docker-compose -f docker-compose.testing.yml logs -f

# Stop services
docker-compose -f docker-compose.testing.yml down
```

## Service URLs

- **API Gateway**: http://localhost:7000
- **Swagger Documentation**: http://localhost:8000

## Features
- ❌ Multi Tenant
- ❌ Advanced Analytics
- ❌ Compliance Monitoring
- ❌ Real Time Weighing
- ❌ Automated Reporting
- ❌ Mobile Access
- ✅ Testing Mode
