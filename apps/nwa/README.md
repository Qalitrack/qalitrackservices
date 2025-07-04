# National Weighing Authority - QaliTrack Deployment

## Overview
- **Client**: National Weighing Authority
- **Code**: nwa
- **Description**: National Weighing Authority - Regulatory oversight and compliance
- **Environment**: government
- **Domain**: weighing.gov

## Services (10 enabled)

### Enabled Services
- **gateway** (Port 7000) - 3 replicas
- **user-service** (Port 7001) - 2 replicas
- **organization-service** (Port 7002) - 2 replicas
- **weighbridge-service** (Port 7007) - 2 replicas
- **compliance-service** (Port 7013) - 3 replicas
- **analytics-service** (Port 7016) - 2 replicas
- **data-sync-service** (Port 7017) - 2 replicas
- **archive-service** (Port 7018) - 2 replicas
- **weight-data-service** (Port 7012) - 2 replicas
- **transaction-service** (Port 7015)

### Disabled Services (9)
- **vehicle-service** - Not within regulatory scope
- **driver-service** - Not within regulatory scope
- **product-service** - Product classification only, not management
- **route-service** - Not within regulatory scope
- **customer-service** - Not within regulatory scope
- **supplier-service** - Not within regulatory scope
- **transporter-service** - Not within regulatory scope
- **sacco-service** - Not within regulatory scope
- **operational-data-service** - Oversight only, not operations

## Quick Start

```bash
# Start all services
./start-nwa.sh

# View logs
docker-compose -f docker-compose.nwa.yml logs -f

# Stop services
docker-compose -f docker-compose.nwa.yml down
```

## Service URLs

- **API Gateway**: http://localhost:7000
- **Swagger Documentation**: http://localhost:8000

## Features
- ✅ Multi Tenant
- ✅ Advanced Analytics
- ✅ Compliance Monitoring
- ❌ Real Time Weighing
- ✅ Automated Reporting
- ✅ Mobile Access
- ✅ Regulatory Dashboard
- ✅ Audit Trails
- ✅ Data Aggregation
