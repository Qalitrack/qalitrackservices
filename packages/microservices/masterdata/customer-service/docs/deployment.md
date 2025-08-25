# Deployment Guide

This guide covers deploying the Customer Service microservice using Docker and various orchestration platforms.

## Prerequisites

- Docker Engine 20.10 or later
- .NET 8.0 SDK (for local development)
- Access to container registry (for production deployments)

## Docker Deployment

### Building the Image

```bash
# Navigate to the service directory
cd packages/microservices/masterdata/customer-service

# Build the Docker image
docker build -t qalitrack/customer-service:latest .

# Tag for production
docker tag qalitrack/customer-service:latest qalitrack/customer-service:1.0.0
```

### Running the Container

#### Development Environment
```bash
# Run with development settings
docker run -d \
  --name customer-service-dev \
  -p 8080:80 \
  -e ASPNETCORE_ENVIRONMENT=Development \
  -e ConnectionStrings__DefaultConnection="Data Source=customers.db" \
  qalitrack/customer-service:latest
```

#### Production Environment
```bash
# Run with production settings
docker run -d \
  --name customer-service-prod \
  -p 80:80 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e ConnectionStrings__DefaultConnection="Server=prod-db;Database=CustomerService;Trusted_Connection=true;" \
  -e Logging__LogLevel__Default=Information \
  --restart unless-stopped \
  qalitrack/customer-service:latest
```

### Environment Variables

| Variable | Description | Default | Required |
|----------|-------------|---------|----------|
| `ASPNETCORE_ENVIRONMENT` | Application environment | Production | Yes |
| `ASPNETCORE_URLS` | URLs the app listens on | http://+:80 | No |
| `ConnectionStrings__DefaultConnection` | Database connection string | SQLite in-memory | Yes |
| `Logging__LogLevel__Default` | Logging level | Information | No |
| `JWT__Secret` | JWT signing secret | - | Yes (Production) |
| `JWT__Issuer` | JWT issuer | QaliTrack | No |
| `JWT__Audience` | JWT audience | QaliTrack | No |

## Docker Compose

Create a `docker-compose.yml` file for the complete setup:

```yaml
version: '3.8'
services:
  customer-service:
    image: qalitrack/customer-service:latest
    container_name: customer-service
    ports:
      - "8080:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=Data Source=/app/data/customers.db
      - JWT__Secret=${JWT_SECRET}
    volumes:
      - customer-data:/app/data
      - customer-logs:/app/logs
    healthcheck:
      test: ["CMD", "wget", "--no-verbose", "--tries=1", "--spider", "http://localhost/health"]
      interval: 30s
      timeout: 10s
      retries: 3
      start_period: 40s
    restart: unless-stopped
    networks:
      - qalitrack-network

volumes:
  customer-data:
    driver: local
  customer-logs:
    driver: local

networks:
  qalitrack-network:
    external: true
```

Run with:
```bash
# Set environment variables
export JWT_SECRET="your-super-secret-jwt-key-here"

# Start the service
docker-compose up -d

# Check status
docker-compose ps

# View logs
docker-compose logs -f customer-service
```

## Kubernetes Deployment

### Deployment Manifest

```yaml
# customer-service-deployment.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: customer-service
  namespace: qalitrack
spec:
  replicas: 3
  selector:
    matchLabels:
      app: customer-service
  template:
    metadata:
      labels:
        app: customer-service
    spec:
      containers:
      - name: customer-service
        image: qalitrack/customer-service:1.0.0
        ports:
        - containerPort: 80
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: "Production"
        - name: ConnectionStrings__DefaultConnection
          valueFrom:
            secretKeyRef:
              name: customer-service-secrets
              key: db-connection
        - name: JWT__Secret
          valueFrom:
            secretKeyRef:
              name: customer-service-secrets
              key: jwt-secret
        livenessProbe:
          httpGet:
            path: /health
            port: 80
          initialDelaySeconds: 30
          periodSeconds: 10
        readinessProbe:
          httpGet:
            path: /health/ready
            port: 80
          initialDelaySeconds: 5
          periodSeconds: 5
        resources:
          requests:
            memory: "128Mi"
            cpu: "100m"
          limits:
            memory: "256Mi"
            cpu: "500m"
        volumeMounts:
        - name: customer-data
          mountPath: /app/data
        - name: customer-logs
          mountPath: /app/logs
      volumes:
      - name: customer-data
        persistentVolumeClaim:
          claimName: customer-service-data
      - name: customer-logs
        persistentVolumeClaim:
          claimName: customer-service-logs
---
apiVersion: v1
kind: Service
metadata:
  name: customer-service
  namespace: qalitrack
spec:
  selector:
    app: customer-service
  ports:
  - protocol: TCP
    port: 80
    targetPort: 80
  type: ClusterIP
```

### Secrets Configuration

```yaml
# customer-service-secrets.yaml
apiVersion: v1
kind: Secret
metadata:
  name: customer-service-secrets
  namespace: qalitrack
type: Opaque
data:
  db-connection: <base64-encoded-connection-string>
  jwt-secret: <base64-encoded-jwt-secret>
```

Deploy to Kubernetes:
```bash
# Apply secrets
kubectl apply -f customer-service-secrets.yaml

# Apply deployment
kubectl apply -f customer-service-deployment.yaml

# Check status
kubectl get pods -n qalitrack -l app=customer-service
kubectl get services -n qalitrack
```

## Health Checks

The service includes comprehensive health checks:

- **Basic Health**: `GET /health` - Returns 200 if service is running
- **Detailed Health**: `GET /health/detailed` - Includes database connectivity
- **Readiness**: `GET /health/ready` - Service is ready to receive traffic

### Health Check Response
```json
{
  "status": "Healthy",
  "totalDuration": "00:00:00.0012345",
  "entries": {
    "database": {
      "status": "Healthy",
      "duration": "00:00:00.0010000",
      "data": {}
    }
  }
}
```

## Monitoring and Logging

### Application Logs
Logs are written to `/app/logs/` directory inside the container:
- `application-{date}.log` - Application logs
- `errors-{date}.log` - Error logs

### Metrics
The service exposes metrics at `/metrics` endpoint (when enabled):
- Request counts and durations
- Database connection pool metrics
- Custom business metrics

### Log Aggregation
For production deployments, configure log aggregation:

#### Fluentd Configuration
```yaml
# fluentd-customer-service.conf
<source>
  @type tail
  path /app/logs/*.log
  pos_file /var/log/fluentd-customer-service.log.pos
  tag qalitrack.customer-service
  format json
</source>

<match qalitrack.customer-service>
  @type elasticsearch
  host elasticsearch.logging.svc.cluster.local
  port 9200
  index_name qalitrack-customer-service
</match>
```

## Troubleshooting

### Common Issues

1. **Container fails to start**
   - Check environment variables are correctly set
   - Verify database connection string
   - Check container logs: `docker logs customer-service`

2. **Health check failures**
   - Ensure database is accessible
   - Check network connectivity
   - Verify health check endpoints are working

3. **Performance issues**
   - Monitor resource usage: `docker stats customer-service`
   - Check database query performance
   - Review application logs for errors

### Useful Commands

```bash
# Check container status
docker ps -a

# View live logs
docker logs -f customer-service

# Execute commands in container
docker exec -it customer-service /bin/bash

# Check resource usage
docker stats customer-service

# Backup database
docker exec customer-service cp /app/data/customers.db /tmp/backup.db
```

## Scaling Considerations

- The service is stateless and can be horizontally scaled
- Database should be externalized for multiple instances
- Consider using a load balancer for high availability
- Implement database connection pooling for better performance
- Use caching (Redis) for frequently accessed data