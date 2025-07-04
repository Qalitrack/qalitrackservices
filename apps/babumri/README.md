# Babumri Cement - QaliTrack Deployment

## Overview
- **Client**: Babumri Cement
- **Code**: babumri
- **Description**: Babumri Cement Factory - Full weighbridge operations
- **Environment**: production
- **Domain**: babumri.qalitrack.com

## Services (16 enabled)

### Enabled Services
- **gateway** (Port 7000) - 2 replicas
- **user-service** (Port 7001)
- **organization-service** (Port 7002)
- **vehicle-service** (Port 7003)
- **driver-service** (Port 7004)
- **product-service** (Port 7005)
- **route-service** (Port 7006)
- **weighbridge-service** (Port 7007) - 2 replicas
- **customer-service** (Port 7008)
- **supplier-service** (Port 7009)
- **transporter-service** (Port 7010)
- **weight-data-service** (Port 7012) - 2 replicas
- **compliance-service** (Port 7013)
- **operational-data-service** (Port 7014)
- **transaction-service** (Port 7015) - 2 replicas
- **analytics-service** (Port 7016)

### Disabled Services (3)
- **sacco-service** - Not applicable for cement factory
- **data-sync-service** - Single location, no sync needed
- **archive-service** - Manual archival process

## Quick Start

```bash
# Start all services
./start-babumri.sh

# View logs
docker-compose -f docker-compose.babumri.yml logs -f

# Stop services
docker-compose -f docker-compose.babumri.yml down
```

## Service URLs

- **API Gateway**: http://localhost:7000
- **Swagger Documentation**: http://localhost:8000

## Features
- ✅ Multi Tenant
- ✅ Advanced Analytics
- ✅ Compliance Monitoring
- ✅ Real Time Weighing
- ✅ Automated Reporting
- ❌ Mobile Access
