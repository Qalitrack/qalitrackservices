# Deployment Guide

This guide covers deploying the QaliTrack Data Manager Service in various environments from development to production.

## Overview

The service supports multiple deployment strategies:
- **Local Development**: Direct .NET execution or Docker Compose
- **Container Deployment**: Docker with orchestration platforms
- **Cloud Platforms**: Azure, AWS, Google Cloud
- **On-Premise**: Traditional server deployment

## Prerequisites

### System Requirements

#### Minimum Requirements
- **CPU**: 2 cores
- **Memory**: 4 GB RAM
- **Storage**: 20 GB available space
- **OS**: Linux (Ubuntu 18.04+), Windows Server 2019+, macOS 10.15+

#### Recommended Production
- **CPU**: 4+ cores
- **Memory**: 8+ GB RAM
- **Storage**: 100+ GB SSD
- **Network**: 1 Gbps
- **OS**: Linux (Ubuntu 20.04+ LTS)

#### Database Requirements
- **SQLite**: Built-in (development only)
- **PostgreSQL**: 12+ (recommended for production)
- **SQL Server**: 2017+ (enterprise environments)

### Software Dependencies

- **.NET 8.0 Runtime**
- **Docker** (for containerized deployment)
- **Reverse Proxy** (Nginx, Apache, or cloud load balancer)

## Environment Configuration

### Environment Variables

Create a `.env` file with required configuration:

```bash
# Database Configuration
DATABASE_PROVIDER=PostgreSQL  # SQLite, PostgreSQL, SqlServer
CONNECTION_STRING="Host=localhost;Database=qalitrack_datamanager;Username=app;Password=secure_password"

# Master Data Service Integration
MASTERDATA_SERVICE_URL=https://masterdata.qalitrack.com
MASTERDATA_SERVICE_TIMEOUT=30

# Authentication & Security
JWT_SECRET_KEY=your-256-bit-secret-key-here
JWT_ISSUER=https://auth.qalitrack.com
JWT_AUDIENCE=qalitrack-datamanager

# Multi-Tenancy
DEFAULT_ORGANIZATION_ID=00000000-0000-0000-0000-000000000000

# Caching
REDIS_CONNECTION_STRING=localhost:6379

# Monitoring & Logging
SERILOG_MINIMUM_LEVEL=Information
ELASTIC_SEARCH_URL=https://elasticsearch.example.com:9200

# Performance
MAX_PAGE_SIZE=100
DEFAULT_PAGE_SIZE=20
CACHE_DURATION_MINUTES=5

# Health Checks
HEALTH_CHECK_PATH=/health
HEALTH_CHECK_PORT=5000

# CORS
ALLOWED_ORIGINS=https://app.qalitrack.com,https://admin.qalitrack.com
```

### Configuration Files

#### Production appsettings.json

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "Console"
      },
      {
        "Name": "File",
        "Args": {
          "path": "/var/log/qalitrack/datamanager-.log",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 30
        }
      }
    ]
  },
  "AllowedHosts": "*",
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://0.0.0.0:5000"
      }
    }
  },
  "HealthChecks": {
    "UI": {
      "MaximumHistoryEntriesPerEndpoint": 50,
      "EvaluationTimeInSeconds": 10,
      "ApiPath": "/health",
      "UIPath": "/health-ui"
    }
  }
}
```

## Docker Deployment

### Dockerfile

The service includes a multi-stage Dockerfile for optimized production builds:

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5000
EXPOSE 5001

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/QaliTrack.DataManager.Api/QaliTrack.DataManager.Api.csproj", "src/QaliTrack.DataManager.Api/"]
COPY ["src/QaliTrack.DataManager.Core/QaliTrack.DataManager.Core.csproj", "src/QaliTrack.DataManager.Core/"]
COPY ["src/QaliTrack.DataManager.Infrastructure/QaliTrack.DataManager.Infrastructure.csproj", "src/QaliTrack.DataManager.Infrastructure/"]
RUN dotnet restore "src/QaliTrack.DataManager.Api/QaliTrack.DataManager.Api.csproj"
COPY . .
WORKDIR "/src/src/QaliTrack.DataManager.Api"
RUN dotnet build "QaliTrack.DataManager.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "QaliTrack.DataManager.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create non-root user for security
RUN addgroup --system --gid 1001 dotnetuser
RUN adduser --system --uid 1001 --ingroup dotnetuser dotnetuser
USER dotnetuser

ENTRYPOINT ["dotnet", "QaliTrack.DataManager.Api.dll"]
```

### Docker Compose

#### Development

```yaml
version: '3.8'

services:
  datamanager:
    build: .
    ports:
      - "5000:5000"
      - "5001:5001"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - DATABASE_PROVIDER=SQLite
      - CONNECTION_STRING=Data Source=datamanager.db
    volumes:
      - ./data:/app/data
    depends_on:
      - masterdata

  masterdata:
    image: qalitrack/masterdata:latest
    ports:
      - "3000:3000"
    environment:
      - NODE_ENV=development
```

#### Production

```yaml
version: '3.8'

services:
  datamanager:
    image: qalitrack/datamanager:latest
    ports:
      - "5000:5000"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - DATABASE_PROVIDER=PostgreSQL
      - CONNECTION_STRING=${DATABASE_CONNECTION_STRING}
      - MASTERDATA_SERVICE_URL=http://masterdata:3000
    volumes:
      - /var/log/qalitrack:/var/log/qalitrack
    restart: unless-stopped
    depends_on:
      - postgres
      - redis
      - masterdata

  postgres:
    image: postgres:15
    environment:
      - POSTGRES_DB=qalitrack_datamanager
      - POSTGRES_USER=app
      - POSTGRES_PASSWORD=${DATABASE_PASSWORD}
    volumes:
      - postgres_data:/var/lib/postgresql/data
    restart: unless-stopped

  redis:
    image: redis:7-alpine
    volumes:
      - redis_data:/data
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
      - datamanager
    restart: unless-stopped

volumes:
  postgres_data:
  redis_data:
```

## Database Setup

### Initial Migration

Run database migrations before first deployment:

```bash
# Using Docker
docker-compose exec datamanager dotnet ef database update

# Direct execution
dotnet ef database update --project src/QaliTrack.DataManager.Infrastructure --startup-project src/QaliTrack.DataManager.Api

# With specific connection string
dotnet ef database update --connection "Host=prod-db;Database=qalitrack_datamanager;Username=app;Password=prod_password"
```

### Migration Scripts

Generate SQL scripts for production deployment:

```bash
dotnet ef migrations script --project src/QaliTrack.DataManager.Infrastructure --startup-project src/QaliTrack.DataManager.Api --output migrate.sql
```

### Seed Data

Initialize with default data:

```bash
# Development seed data
dotnet run --project src/QaliTrack.DataManager.Api -- --seed-data

# Production seed data (minimal)
dotnet run --project src/QaliTrack.DataManager.Api -- --seed-data --environment Production
```

## Reverse Proxy Configuration

### Nginx Configuration

```nginx
upstream datamanager_backend {
    server datamanager:5000;
    # Add more servers for load balancing
    # server datamanager2:5000;
}

server {
    listen 80;
    server_name api.qalitrack.com;
    return 301 https://$server_name$request_uri;
}

server {
    listen 443 ssl http2;
    server_name api.qalitrack.com;

    ssl_certificate /etc/nginx/ssl/qalitrack.com.crt;
    ssl_certificate_key /etc/nginx/ssl/qalitrack.com.key;
    ssl_protocols TLSv1.2 TLSv1.3;
    ssl_ciphers HIGH:!aNULL:!MD5;

    location / {
        proxy_pass http://datamanager_backend;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        
        # WebSocket support
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "upgrade";
        
        # Timeouts
        proxy_connect_timeout 300;
        proxy_send_timeout 300;
        proxy_read_timeout 300;
    }

    # Health check endpoint
    location /health {
        proxy_pass http://datamanager_backend/health;
        access_log off;
    }

    # Static file caching
    location ~* \.(js|css|png|jpg|jpeg|gif|ico|svg)$ {
        proxy_pass http://datamanager_backend;
        expires 1y;
        add_header Cache-Control "public, immutable";
    }
}
```

### Apache Configuration

```apache
<VirtualHost *:443>
    ServerName api.qalitrack.com
    
    SSLEngine on
    SSLCertificateFile /etc/ssl/certs/qalitrack.com.crt
    SSLCertificateKeyFile /etc/ssl/private/qalitrack.com.key
    
    ProxyPreserveHost On
    ProxyRequests Off
    
    ProxyPass /health http://localhost:5000/health
    ProxyPassReverse /health http://localhost:5000/health
    
    ProxyPass / http://localhost:5000/
    ProxyPassReverse / http://localhost:5000/
    
    # WebSocket support
    RewriteEngine On
    RewriteCond %{HTTP:Upgrade} =websocket [NC]
    RewriteRule /(.*) ws://localhost:5000/$1 [P,L]
</VirtualHost>
```

## Cloud Platform Deployment

### Azure App Service

#### ARM Template

```json
{
  "$schema": "https://schema.management.azure.com/schemas/2019-04-01/deploymentTemplate.json#",
  "contentVersion": "1.0.0.0",
  "parameters": {
    "appName": {
      "type": "string",
      "defaultValue": "qalitrack-datamanager"
    },
    "location": {
      "type": "string",
      "defaultValue": "[resourceGroup().location]"
    }
  },
  "resources": [
    {
      "type": "Microsoft.Web/serverfarms",
      "apiVersion": "2020-06-01",
      "name": "[concat(parameters('appName'), '-plan')]",
      "location": "[parameters('location')]",
      "sku": {
        "name": "P1v2",
        "capacity": 1
      },
      "properties": {
        "reserved": true
      }
    },
    {
      "type": "Microsoft.Web/sites",
      "apiVersion": "2020-06-01",
      "name": "[parameters('appName')]",
      "location": "[parameters('location')]",
      "dependsOn": [
        "[resourceId('Microsoft.Web/serverfarms', concat(parameters('appName'), '-plan'))]"
      ],
      "properties": {
        "serverFarmId": "[resourceId('Microsoft.Web/serverfarms', concat(parameters('appName'), '-plan'))]",
        "siteConfig": {
          "linuxFxVersion": "DOTNETCORE|8.0",
          "appSettings": [
            {
              "name": "ASPNETCORE_ENVIRONMENT",
              "value": "Production"
            }
          ]
        }
      }
    }
  ]
}
```

#### Azure CLI Deployment

```bash
# Create resource group
az group create --name qalitrack-rg --location eastus2

# Deploy App Service
az deployment group create \
  --resource-group qalitrack-rg \
  --template-file deploy/azure-appservice.json \
  --parameters appName=qalitrack-datamanager

# Configure connection strings
az webapp config connection-string set \
  --name qalitrack-datamanager \
  --resource-group qalitrack-rg \
  --connection-string-type SQLAzure \
  --settings DefaultConnection="Server=tcp:qalitrack-sql.database.windows.net,1433;Database=datamanager;User ID=appuser;Password=SecurePassword123!;Encrypt=True;"
```

### AWS ECS

#### Task Definition

```json
{
  "family": "qalitrack-datamanager",
  "networkMode": "awsvpc",
  "requiresCompatibilities": ["FARGATE"],
  "cpu": "256",
  "memory": "512",
  "executionRoleArn": "arn:aws:iam::123456789012:role/ecsTaskExecutionRole",
  "containerDefinitions": [
    {
      "name": "datamanager",
      "image": "qalitrack/datamanager:latest",
      "portMappings": [
        {
          "containerPort": 5000,
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
          "name": "CONNECTION_STRING",
          "valueFrom": "arn:aws:secretsmanager:us-east-1:123456789012:secret:datamanager/db-connection"
        }
      ],
      "logConfiguration": {
        "logDriver": "awslogs",
        "options": {
          "awslogs-group": "/ecs/qalitrack-datamanager",
          "awslogs-region": "us-east-1",
          "awslogs-stream-prefix": "ecs"
        }
      },
      "healthCheck": {
        "command": ["CMD-SHELL", "curl -f http://localhost:5000/health || exit 1"],
        "interval": 30,
        "timeout": 5,
        "retries": 3
      }
    }
  ]
}
```

### Google Cloud Run

#### Deployment Command

```bash
# Build and push container
docker build -t gcr.io/your-project-id/qalitrack-datamanager .
docker push gcr.io/your-project-id/qalitrack-datamanager

# Deploy to Cloud Run
gcloud run deploy qalitrack-datamanager \
  --image gcr.io/your-project-id/qalitrack-datamanager \
  --platform managed \
  --region us-central1 \
  --allow-unauthenticated \
  --port 5000 \
  --memory 1Gi \
  --cpu 1 \
  --max-instances 10 \
  --set-env-vars ASPNETCORE_ENVIRONMENT=Production \
  --set-env-vars DATABASE_PROVIDER=PostgreSQL
```

## Kubernetes Deployment

### Namespace and ConfigMap

```yaml
apiVersion: v1
kind: Namespace
metadata:
  name: qalitrack
---
apiVersion: v1
kind: ConfigMap
metadata:
  name: datamanager-config
  namespace: qalitrack
data:
  ASPNETCORE_ENVIRONMENT: "Production"
  DATABASE_PROVIDER: "PostgreSQL"
  MASTERDATA_SERVICE_URL: "http://masterdata-service:3000"
```

### Secret

```yaml
apiVersion: v1
kind: Secret
metadata:
  name: datamanager-secrets
  namespace: qalitrack
type: Opaque
stringData:
  CONNECTION_STRING: "Host=postgres-service;Database=qalitrack_datamanager;Username=app;Password=SecurePassword123!"
  JWT_SECRET_KEY: "your-256-bit-secret-key-here"
```

### Deployment

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: datamanager
  namespace: qalitrack
spec:
  replicas: 3
  selector:
    matchLabels:
      app: datamanager
  template:
    metadata:
      labels:
        app: datamanager
    spec:
      containers:
      - name: datamanager
        image: qalitrack/datamanager:latest
        ports:
        - containerPort: 5000
        envFrom:
        - configMapRef:
            name: datamanager-config
        - secretRef:
            name: datamanager-secrets
        livenessProbe:
          httpGet:
            path: /health
            port: 5000
          initialDelaySeconds: 30
          periodSeconds: 10
        readinessProbe:
          httpGet:
            path: /health
            port: 5000
          initialDelaySeconds: 5
          periodSeconds: 5
        resources:
          requests:
            memory: "256Mi"
            cpu: "250m"
          limits:
            memory: "512Mi"
            cpu: "500m"
```

### Service and Ingress

```yaml
apiVersion: v1
kind: Service
metadata:
  name: datamanager-service
  namespace: qalitrack
spec:
  selector:
    app: datamanager
  ports:
  - port: 80
    targetPort: 5000
  type: ClusterIP
---
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: datamanager-ingress
  namespace: qalitrack
  annotations:
    kubernetes.io/ingress.class: nginx
    cert-manager.io/cluster-issuer: letsencrypt-prod
spec:
  tls:
  - hosts:
    - api.qalitrack.com
    secretName: datamanager-tls
  rules:
  - host: api.qalitrack.com
    http:
      paths:
      - path: /
        pathType: Prefix
        backend:
          service:
            name: datamanager-service
            port:
              number: 80
```

## Monitoring and Observability

### Health Checks

The service provides comprehensive health checks:

```http
GET /health
GET /health-ui  # Visual health check dashboard
```

### Prometheus Metrics

Enable metrics collection:

```bash
# Add to appsettings.json
{
  "Prometheus": {
    "Enabled": true,
    "Port": 9090,
    "Endpoint": "/metrics"
  }
}
```

### Application Insights (Azure)

```json
{
  "ApplicationInsights": {
    "InstrumentationKey": "your-key-here"
  },
  "Logging": {
    "ApplicationInsights": {
      "LogLevel": {
        "Default": "Information"
      }
    }
  }
}
```

## Security Considerations

### SSL/TLS Configuration

- Use strong cipher suites (TLS 1.2+)
- Implement HTTP Strict Transport Security (HSTS)
- Configure proper certificate management

### Network Security

- Implement network segmentation
- Use private subnets for database access
- Configure security groups/firewall rules
- Enable VPN or private connectivity for sensitive environments

### Secrets Management

- Use cloud-native secret management services
- Rotate secrets regularly
- Never store secrets in configuration files or environment variables in plain text

### Container Security

- Use non-root users in containers
- Scan images for vulnerabilities
- Implement runtime security monitoring
- Use minimal base images (distroless when possible)

## Backup and Disaster Recovery

### Database Backups

```bash
# PostgreSQL backup
pg_dump -h localhost -U app -d qalitrack_datamanager > backup.sql

# Automated backup script
#!/bin/bash
DATE=$(date +%Y%m%d_%H%M%S)
pg_dump -h $DB_HOST -U $DB_USER -d $DB_NAME | gzip > /backups/datamanager_$DATE.sql.gz

# Retain only last 30 days
find /backups -name "datamanager_*.sql.gz" -mtime +30 -delete
```

### Cross-Region Replication

Configure database replication for disaster recovery:

- **PostgreSQL**: Streaming replication
- **SQL Server**: Always On Availability Groups
- **Cloud**: Use managed service replication features

## Performance Optimization

### Database Optimization

- Regular maintenance (VACUUM, REINDEX)
- Query performance monitoring
- Connection pooling configuration
- Proper indexing strategies

### Application Performance

- Enable response compression
- Configure output caching
- Implement connection pooling
- Use async/await patterns consistently

### Caching Strategy

```json
{
  "Caching": {
    "Redis": {
      "ConnectionString": "localhost:6379",
      "DefaultExpiration": "00:05:00"
    },
    "Memory": {
      "SizeLimit": 100,
      "CompactionPercentage": 0.8
    }
  }
}
```

## Troubleshooting

### Common Issues

#### Database Connection Failures
```bash
# Check connection string
dotnet ef database update --verbose

# Test database connectivity
psql -h hostname -U username -d database_name -c "SELECT 1;"
```

#### High Memory Usage
```bash
# Monitor container memory
docker stats container_name

# Check for memory leaks
dotnet-counters monitor --process-id <pid> --counters System.Runtime
```

#### Performance Issues
```bash
# Enable detailed logging
export ASPNETCORE_ENVIRONMENT=Development
export Serilog__MinimumLevel__Default=Debug

# Profile with dotnet-trace
dotnet-trace collect --process-id <pid> --format speedscope
```

### Log Analysis

Key log patterns to monitor:

```bash
# Database deadlocks
grep -i "deadlock" /var/log/qalitrack/datamanager.log

# High response times
grep -i "elapsed.*[5-9][0-9][0-9][0-9]ms" /var/log/qalitrack/datamanager.log

# Authentication failures
grep -i "unauthorized\|forbidden" /var/log/qalitrack/datamanager.log
```

## Scaling Strategies

### Horizontal Scaling

- Use load balancers
- Implement sticky sessions if needed
- Consider database read replicas
- Cache frequently accessed data

### Vertical Scaling

- Monitor CPU and memory usage
- Scale containers/pods resources
- Optimize database instance sizes

### Auto-scaling Configuration

```yaml
# Kubernetes HPA
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: datamanager-hpa
spec:
  scaleTargetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: datamanager
  minReplicas: 2
  maxReplicas: 10
  metrics:
  - type: Resource
    resource:
      name: cpu
      target:
        type: Utilization
        averageUtilization: 70
```