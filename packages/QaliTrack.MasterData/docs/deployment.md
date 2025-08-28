# Deployment

## Overview

This document provides comprehensive guidance for deploying the QaliTrack Master Data Service in various environments. The service is designed to be deployed as a containerized application using Docker and Kubernetes, with support for both cloud and on-premises deployments.

## Deployment Architecture

### Container-Based Deployment
The service is packaged as Docker containers with the following components:
- **API Service**: Main application container
- **Database**: PostgreSQL or SQL Server container
- **Documentation**: DocFX documentation server
- **Reverse Proxy**: Nginx for load balancing and SSL termination

### Deployment Environments
- **Development**: Local development environment
- **Testing**: Automated testing and QA environment
- **Staging**: Pre-production environment
- **Production**: Live production environment

## Prerequisites

### System Requirements
- **CPU**: Minimum 2 vCPUs (4 vCPUs recommended for production)
- **Memory**: Minimum 4GB RAM (8GB recommended for production)
- **Storage**: Minimum 20GB (SSD recommended)
- **Network**: HTTPS/TLS support, port 80/443 access

### Software Requirements
- **Docker**: Version 20.10 or later
- **Docker Compose**: Version 2.0 or later
- **Kubernetes**: Version 1.21 or later (for K8s deployments)
- **.NET Runtime**: 8.0 (included in container)

### Database Requirements
- **PostgreSQL**: Version 13 or later (recommended)
- **SQL Server**: Version 2019 or later
- **SQLite**: For development/testing only

## Configuration

### Environment Variables
```bash
# Database Configuration
DB_CONNECTION_STRING="Server=localhost;Database=QaliTrackMasterData;User Id=qalitrack;Password=***;"
DB_PROVIDER="PostgreSQL" # PostgreSQL, SqlServer, or Sqlite

# API Configuration
ASPNETCORE_ENVIRONMENT="Production"
ASPNETCORE_URLS="http://+:8080"
API_BASE_PATH="/api/masterdata"

# Security Configuration
JWT_SECRET="your-jwt-secret-key"
JWT_ISSUER="qalitrack-masterdata"
JWT_AUDIENCE="qalitrack-clients"
JWT_EXPIRY_MINUTES=60

# Logging Configuration
SERILOG_MINIMUM_LEVEL="Information"
SERILOG_WRITE_TO_CONSOLE=true
SERILOG_WRITE_TO_FILE=true

# Multi-tenancy Configuration
ENABLE_MULTI_TENANCY=true
DEFAULT_ORGANIZATION_ID="default-org"

# Documentation Configuration
ENABLE_SWAGGER=true
ENABLE_DOCS_SERVE=true
DOCS_BASE_PATH="/docs"
```

### Configuration Files

#### appsettings.Production.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "${DB_CONNECTION_STRING}"
  },
  "DatabaseProvider": "${DB_PROVIDER}",
  "Authentication": {
    "Jwt": {
      "Key": "${JWT_SECRET}",
      "Issuer": "${JWT_ISSUER}",
      "Audience": "${JWT_AUDIENCE}",
      "ExpiryInMinutes": "${JWT_EXPIRY_MINUTES}"
    }
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "HealthChecks": {
    "UI": {
      "HealthCheckUiOptions": {
        "UIPath": "/health-ui",
        "ApiPath": "/health-json"
      }
    }
  }
}
```

## Docker Deployment

### Dockerfile
The service includes an optimized multi-stage Dockerfile:

```dockerfile
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/QaliTrack.MasterData.Api/QaliTrack.MasterData.Api.csproj", "src/QaliTrack.MasterData.Api/"]
COPY ["src/QaliTrack.MasterData.Core/QaliTrack.MasterData.Core.csproj", "src/QaliTrack.MasterData.Core/"]
COPY ["src/QaliTrack.MasterData.Infrastructure/QaliTrack.MasterData.Infrastructure.csproj", "src/QaliTrack.MasterData.Infrastructure/"]
RUN dotnet restore "src/QaliTrack.MasterData.Api/QaliTrack.MasterData.Api.csproj"
COPY . .
WORKDIR "/src/src/QaliTrack.MasterData.Api"
RUN dotnet build "QaliTrack.MasterData.Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "QaliTrack.MasterData.Api.csproj" -c Release -o /app/publish

# Documentation build stage
FROM build AS docs-build
WORKDIR /src
RUN dotnet tool install -g docfx
ENV PATH="${PATH}:/root/.dotnet/tools"
RUN docfx
COPY _site /app/docs

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=docs-build /app/docs ./wwwroot/docs
EXPOSE 8080
ENTRYPOINT ["dotnet", "QaliTrack.MasterData.Api.dll"]
```

### Docker Compose

#### docker-compose.yml
```yaml
version: '3.8'

services:
  qalitrack-masterdata:
    build: .
    ports:
      - "8080:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - DB_CONNECTION_STRING=Server=db;Database=QaliTrackMasterData;User Id=qalitrack;Password=SecurePassword123!;
      - DB_PROVIDER=PostgreSQL
      - JWT_SECRET=your-super-secret-jwt-key-here-make-it-long-and-secure
      - JWT_ISSUER=qalitrack-masterdata
      - JWT_AUDIENCE=qalitrack-clients
    depends_on:
      - db
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:8080/health"]
      interval: 30s
      timeout: 10s
      retries: 3
    restart: unless-stopped

  db:
    image: postgres:15
    environment:
      POSTGRES_DB: QaliTrackMasterData
      POSTGRES_USER: qalitrack
      POSTGRES_PASSWORD: SecurePassword123!
    volumes:
      - postgres_data:/var/lib/postgresql/data
    ports:
      - "5432:5432"
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U qalitrack"]
      interval: 10s
      timeout: 5s
      retries: 5
    restart: unless-stopped

  nginx:
    image: nginx:alpine
    ports:
      - "80:80"
      - "443:443"
    volumes:
      - ./nginx.conf:/etc/nginx/nginx.conf
      - ./ssl:/etc/nginx/ssl
    depends_on:
      - qalitrack-masterdata
    restart: unless-stopped

volumes:
  postgres_data:
```

### Running with Docker Compose
```bash
# Clone the repository
git clone https://github.com/yourusername/qalitrackservices.git
cd qalitrackservices/packages/QaliTrack.MasterData

# Set environment variables
cp .env.example .env
# Edit .env with your configuration

# Build and start services
docker-compose up -d

# View logs
docker-compose logs -f

# Stop services
docker-compose down
```

## Kubernetes Deployment

### Namespace and ConfigMap
```yaml
apiVersion: v1
kind: Namespace
metadata:
  name: qalitrack-masterdata
---
apiVersion: v1
kind: ConfigMap
metadata:
  name: masterdata-config
  namespace: qalitrack-masterdata
data:
  ASPNETCORE_ENVIRONMENT: "Production"
  DB_PROVIDER: "PostgreSQL"
  JWT_ISSUER: "qalitrack-masterdata"
  JWT_AUDIENCE: "qalitrack-clients"
  JWT_EXPIRY_MINUTES: "60"
  SERILOG_MINIMUM_LEVEL: "Information"
```

### Secret Management
```yaml
apiVersion: v1
kind: Secret
metadata:
  name: masterdata-secrets
  namespace: qalitrack-masterdata
type: Opaque
stringData:
  DB_CONNECTION_STRING: "Server=postgres-service;Database=QaliTrackMasterData;User Id=qalitrack;Password=SecurePassword123!;"
  JWT_SECRET: "your-super-secret-jwt-key-here-make-it-long-and-secure"
```

### Deployment
```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: masterdata-api
  namespace: qalitrack-masterdata
spec:
  replicas: 3
  selector:
    matchLabels:
      app: masterdata-api
  template:
    metadata:
      labels:
        app: masterdata-api
    spec:
      containers:
      - name: api
        image: qalitrack/masterdata:latest
        ports:
        - containerPort: 8080
        envFrom:
        - configMapRef:
            name: masterdata-config
        - secretRef:
            name: masterdata-secrets
        livenessProbe:
          httpGet:
            path: /health
            port: 8080
          initialDelaySeconds: 30
          periodSeconds: 10
        readinessProbe:
          httpGet:
            path: /health/ready
            port: 8080
          initialDelaySeconds: 5
          periodSeconds: 5
        resources:
          requests:
            memory: "512Mi"
            cpu: "500m"
          limits:
            memory: "1Gi"
            cpu: "1000m"
```

### Service and Ingress
```yaml
apiVersion: v1
kind: Service
metadata:
  name: masterdata-service
  namespace: qalitrack-masterdata
spec:
  selector:
    app: masterdata-api
  ports:
  - protocol: TCP
    port: 80
    targetPort: 8080
  type: ClusterIP
---
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: masterdata-ingress
  namespace: qalitrack-masterdata
  annotations:
    kubernetes.io/ingress.class: nginx
    cert-manager.io/cluster-issuer: letsencrypt-prod
    nginx.ingress.kubernetes.io/rewrite-target: /
spec:
  tls:
  - hosts:
    - api.qalitrack.com
    secretName: masterdata-tls
  rules:
  - host: api.qalitrack.com
    http:
      paths:
      - path: /api/masterdata
        pathType: Prefix
        backend:
          service:
            name: masterdata-service
            port:
              number: 80
```

## Cloud Deployments

### Azure Container Apps
```bash
# Create resource group
az group create --name rg-qalitrack --location eastus

# Create container app environment
az containerapp env create \
  --name env-qalitrack \
  --resource-group rg-qalitrack \
  --location eastus

# Deploy container app
az containerapp create \
  --name masterdata-api \
  --resource-group rg-qalitrack \
  --environment env-qalitrack \
  --image qalitrack/masterdata:latest \
  --target-port 8080 \
  --ingress external \
  --env-vars ASPNETCORE_ENVIRONMENT=Production \
  --secrets jwt-secret=your-jwt-secret \
  --min-replicas 1 \
  --max-replicas 10
```

### AWS ECS Fargate
```json
{
  "family": "qalitrack-masterdata",
  "networkMode": "awsvpc",
  "requiresCompatibilities": ["FARGATE"],
  "cpu": "1024",
  "memory": "2048",
  "taskRoleArn": "arn:aws:iam::account:role/ecsTaskRole",
  "executionRoleArn": "arn:aws:iam::account:role/ecsTaskExecutionRole",
  "containerDefinitions": [
    {
      "name": "masterdata-api",
      "image": "qalitrack/masterdata:latest",
      "portMappings": [
        {
          "containerPort": 8080,
          "protocol": "tcp"
        }
      ],
      "environment": [
        {
          "name": "ASPNETCORE_ENVIRONMENT",
          "value": "Production"
        }
      ],
      "secrets": [
        {
          "name": "DB_CONNECTION_STRING",
          "valueFrom": "arn:aws:secretsmanager:region:account:secret:db-connection"
        }
      ],
      "healthCheck": {
        "command": ["CMD-SHELL", "curl -f http://localhost:8080/health || exit 1"],
        "interval": 30,
        "timeout": 5,
        "retries": 3
      },
      "logConfiguration": {
        "logDriver": "awslogs",
        "options": {
          "awslogs-group": "/ecs/qalitrack-masterdata",
          "awslogs-region": "us-east-1",
          "awslogs-stream-prefix": "ecs"
        }
      }
    }
  ]
}
```

## Database Migration

### Production Migration Strategy
```bash
# 1. Backup existing database
pg_dump -h prod-db-host -U username dbname > backup_$(date +%Y%m%d_%H%M%S).sql

# 2. Run migrations in staging environment first
dotnet ef database update --environment Staging

# 3. Validate data integrity
dotnet run --project tools/DataValidator

# 4. Schedule maintenance window
# 5. Run production migration
dotnet ef database update --environment Production

# 6. Verify migration success
dotnet run --project tools/MigrationValidator

# 7. Update application configuration if needed
# 8. Deploy new application version
# 9. Perform smoke tests
# 10. Monitor application health
```

### Zero-Downtime Deployment
```bash
# Blue-green deployment script
./scripts/blue-green-deploy.sh \
  --image qalitrack/masterdata:v2.1.0 \
  --environment production \
  --health-check-timeout 300 \
  --rollback-on-failure true
```

## Monitoring and Health Checks

### Health Check Endpoints
- `/health` - Basic health check
- `/health/ready` - Readiness probe
- `/health/live` - Liveness probe
- `/health-ui` - Health check dashboard

### Monitoring Stack
- **Metrics**: Prometheus and Grafana
- **Logging**: ELK Stack (Elasticsearch, Logstash, Kibana)
- **Tracing**: Jaeger or Zipkin
- **Alerting**: AlertManager

### Sample Monitoring Configuration
```yaml
# prometheus.yml
scrape_configs:
  - job_name: 'masterdata-api'
    static_configs:
      - targets: ['masterdata-service:80']
    metrics_path: '/metrics'
    scrape_interval: 15s
```

## Troubleshooting

### Common Issues

#### Connection Issues
```bash
# Check container connectivity
docker exec -it masterdata-api curl http://localhost:8080/health

# Check database connectivity
docker exec -it masterdata-api dotnet ef database show
```

#### Performance Issues
```bash
# Check resource usage
docker stats masterdata-api

# Check application metrics
curl http://localhost:8080/metrics
```

#### Log Analysis
```bash
# View application logs
docker logs -f masterdata-api

# Search for specific errors
docker logs masterdata-api 2>&1 | grep ERROR
```

This comprehensive deployment guide ensures successful deployment and operation of the QaliTrack Master Data Service across various environments and platforms.