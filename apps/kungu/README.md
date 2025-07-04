# Kungu Cement - QaliTrack Deployment

## Overview
- **Client**: Kungu Cement
- **Code**: kungu
- **Description**: Kungu Cement Factory - Simplified weighbridge operations
- **Environment**: production
- **Domain**: kungu.qalitrack.com

## Services (10 enabled)

### Enabled Services
- **gateway** (Port 7000)
- **user-service** (Port 7001)
- **organization-service** (Port 7002)
- **vehicle-service** (Port 7003)
- **driver-service** (Port 7004)
- **product-service** (Port 7005)
- **weighbridge-service** (Port 7007)
- **customer-service** (Port 7008)
- **weight-data-service** (Port 7012)
- **transaction-service** (Port 7015)

### Disabled Services (9)
- **route-service** - Simple local operations, no complex routing
- **supplier-service** - Limited supplier relationships
- **transporter-service** - No external transporters managed
- **sacco-service** - Not applicable for cement factory
- **compliance-service** - Manual compliance processes
- **operational-data-service** - Basic operations only
- **analytics-service** - Basic reporting sufficient
- **data-sync-service** - Single location, no sync needed
- **archive-service** - Manual archival process

## Quick Start

```bash
# Start all services
./start-kungu.sh

# View logs
docker-compose -f docker-compose.kungu.yml logs -f

# Stop services
docker-compose -f docker-compose.kungu.yml down
```

## Service URLs

- **API Gateway**: http://localhost:7000
- **Swagger Documentation**: http://localhost:8000

## Features
- ❌ Multi Tenant
- ❌ Advanced Analytics
- ❌ Compliance Monitoring
- ✅ Real Time Weighing
- ❌ Automated Reporting
- ❌ Mobile Access
