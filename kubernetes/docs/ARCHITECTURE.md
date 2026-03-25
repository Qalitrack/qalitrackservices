# Qalitrack Platform Architecture

System architecture and design decisions for the Kubernetes-based Qalitrack platform.

## System Overview

Qalitrack is a microservices-based weighbridge management system designed for high availability, scalability, and operational simplicity. The platform utilizes Kubernetes for orchestration, Helm for package management, and implements industry-standard patterns for service mesh, observability, and security.

## Logical Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                        Client Layer                              │
│  (Web Browsers, Mobile Apps, External Integrations)             │
└────────────────────┬────────────────────────────────────────────┘
                     │
                     │ HTTPS/TLS
                     ▼
┌─────────────────────────────────────────────────────────────────┐
│                   Ingress Layer                                  │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │ NGINX Ingress Controller                                  │  │
│  │ - L7 Load Balancing                                       │  │
│  │ - SSL/TLS Termination (cert-manager)                      │  │
│  │ - Path-based Routing                                      │  │
│  │ - Rate Limiting                                            │  │
│  └──────────────────────────────────────────────────────────┘  │
└────────────────────┬────────────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────────────┐
│                    API Gateway Layer                             │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │ Gateway Service (YARP)                                    │  │
│  │ - Request Routing                                         │  │
│  │ - JWT Authentication                                      │  │
│  │ - Request/Response Transformation                         │  │
│  │ - Circuit Breaking                                        │  │
│  └──────────────────────────────────────────────────────────┘  │
└────────────────────┬────────────────────────────────────────────┘
                     │
      ┌──────────────┼──────────────┬─────────────┬──────────────┐
      │              │              │             │              │
      ▼              ▼              ▼             ▼              ▼
┌─────────┐    ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐
│  User   │    │MasterData│  │Transaction│  │ Backup   │  │Technician│
│ Service │    │ Service  │  │  Service  │  │ Service  │  │ Service  │
└────┬────┘    └────┬─────┘  └────┬─────┘  └────┬─────┘  └────┬─────┘
     │              │              │             │              │
     ├──────────────┴──────────────┴─────────────┴──────────────┤
     │                                                           │
     ▼                                                           ▼
┌─────────────────────────────────────────────────────────────────┐
│                      Data Layer                                  │
│  ┌───────────┐  ┌───────────┐  ┌───────────┐  ┌───────────┐   │
│  │PostgreSQL │  │PostgreSQL │  │PostgreSQL │  │PostgreSQL │   │
│  │  (User)   │  │(MasterData)│  │  (Trans)  │  │ (Backup)  │   │
│  └───────────┘  └───────────┘  └───────────┘  └───────────┘   │
│                                                                  │
│  ┌───────────────────────┐  ┌──────────────────────────────┐  │
│  │  Redis (Cache)        │  │  RabbitMQ (Message Queue)    │  │
│  └───────────────────────┘  └──────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
```

## Service Layer Architecture

### Gateway Service

**Technology:** YARP (Yet Another Reverse Proxy)
**Port:** 7000
**Responsibilities:**
- Request routing to backend microservices
- JWT token validation and authentication
- CORS policy enforcement
- Request/response logging
- Circuit breaking and retry policies

**Routing Configuration:**
```
/api/user/*        → user-service:7001
/api/masterdata/*  → masterdata-service:7002
/api/transaction/* → transaction-service:7003
/api/backup/*      → backup-service:7004
/api/technician/*  → technician-service:7006
/api/qtruck/*      → qtruck-api:7007
```

**Scaling:** 2-5 replicas (HPA enabled)
**Health Checks:** /health endpoint
**Metrics:** Prometheus metrics at /metrics

### User Service

**Technology:** ASP.NET Core 8.0
**Port:** 7001
**Database:** PostgreSQL (user_service_db)
**Responsibilities:**
- User authentication and authorization
- User profile management
- Role-based access control (RBAC)
- JWT token issuance
- Password management and reset

**Database Schema:**
- Users table
- Roles table
- UserRoles junction table
- AuditLogs table

**Dependencies:**
- PostgreSQL (user-service-postgresql)
- Redis (session storage, optional)

### MasterData Service

**Technology:** ASP.NET Core 8.0
**Port:** 7002
**Database:** PostgreSQL (masterdata_db)
**Responsibilities:**
- Vehicle registration and management
- Product catalog management
- Supplier/customer management
- Reference data maintenance

**Database Schema:**
- Vehicles table
- Products table
- Suppliers table
- Customers table
- Categories table

**Dependencies:**
- PostgreSQL (masterdata-service-postgresql)
- Redis (caching)

### Transaction Service

**Technology:** ASP.NET Core 8.0
**Port:** 7003
**Database:** PostgreSQL (transaction_db)
**Responsibilities:**
- Weighbridge transaction processing
- Weight recording (tare, gross)
- Transaction lifecycle management
- Reweigh request handling
- Receipt generation

**Database Schema:**
- Transactions table
- WeighingRecords table
- ReweighRecords table
- AuditLogs table

**Dependencies:**
- PostgreSQL (transaction-service-postgresql)
- Redis (transaction state caching)
- RabbitMQ (event publishing)

### Backup Service

**Technology:** ASP.NET Core 8.0
**Port:** 7004
**Database:** PostgreSQL (backup_metadata_db)
**Responsibilities:**
- Database backup orchestration
- Backup metadata tracking
- On-demand backup API
- Backup verification

**Dependencies:**
- PostgreSQL (backup-service-postgresql, backup metadata)
- All service PostgreSQL instances (backup targets)
- RabbitMQ (backup job queue)

## Data Layer Architecture

### PostgreSQL Database Strategy

**Deployment Model:** Database-per-service pattern

**Rationale:**
- Service isolation and independence
- Independent scaling
- Fault isolation
- Schema evolution flexibility

**Configuration:**
- Version: PostgreSQL 15
- Bitnami Helm chart
- Persistent storage via PVC
- Connection pooling: 50-500 connections per instance

**Performance Tuning:**
```
max_connections = 500
shared_buffers = 2GB
effective_cache_size = 6GB
work_mem = 16MB
maintenance_work_mem = 512MB
```

**Backup Strategy:**
- Automated pg_dump daily via CronJob
- 30-day retention
- Stored on PersistentVolume
- Checksums for integrity verification

### Redis Caching Layer

**Deployment:** Single master (standalone mode)
**Configuration:**
- Bitnami Redis Helm chart
- Maxmemory: 512MB
- Eviction policy: allkeys-lru
- Persistence: RDB snapshots

**Use Cases:**
- Session storage
- Transaction state caching
- Query result caching
- Rate limiting counters

### RabbitMQ Message Queue

**Deployment:** Single instance
**Configuration:**
- Bitnami RabbitMQ Helm chart
- Queues:
  - backup.jobs (durable)
  - transaction.events (durable)
  - notifications (non-durable)

**Patterns:**
- Work queues for backup jobs
- Publish/subscribe for transaction events
- RPC for synchronous inter-service communication

## Network Architecture

### Kubernetes Networking

**CNI:** Cluster-dependent (Calico, Flannel, or cloud provider CNI)
**Service Mesh:** Not implemented (planned: Istio/Linkerd)

**Network Segmentation:**

```
ingress-nginx namespace (isolated)
│
├─> qalitrack-prod namespace
│   ├─ Services (ClusterIP)
│   ├─ Databases (ClusterIP)
│   └─ Infrastructure (Redis, RabbitMQ)
│
└─> qalitrack-monitoring namespace
    ├─ Prometheus
    ├─ Grafana
    └─ Loki
```

**NetworkPolicy Enforcement:**
- Default deny all ingress
- Explicit allow rules:
  - Ingress controller → Gateway
  - Gateway → Backend services
  - Services → Databases (port 5432)
  - Services → Redis (port 6379)
  - Services → RabbitMQ (ports 5672, 15672)
  - Prometheus → All services (metrics)
- Egress: Allowed to DNS, internet (controlled)

### DNS Resolution

**Internal DNS:** CoreDNS (Kubernetes default)

**Service Discovery Pattern:**
```
<service-name>.<namespace>.svc.cluster.local
```

Examples:
- `user-service.qalitrack-prod.svc.cluster.local`
- `user-service-postgresql.qalitrack-prod.svc.cluster.local`

Short names work within namespace:
- `user-service` (same namespace)
- `user-service.qalitrack-prod` (cross-namespace)

### Ingress Architecture

**Controller:** NGINX Ingress Controller
**TLS:** cert-manager with Let's Encrypt
**Annotations:**
- `cert-manager.io/cluster-issuer: letsencrypt-prod`
- `nginx.ingress.kubernetes.io/ssl-redirect: "true"`
- `nginx.ingress.kubernetes.io/force-ssl-redirect: "true"`

**Path Routing:**
```
https://qalibrated.co.ke/qalitrack/api/*     → gateway-service
https://qalibrated.co.ke/grafana/*           → grafana
https://qalibrated.co.ke/prometheus/*        → prometheus
```

## Storage Architecture

### PersistentVolume Strategy

**StorageClass:** Cluster default (cloud provider or local provisioner)
**Access Modes:**
- ReadWriteOnce (RWO) for databases
- ReadWriteMany (RWX) for shared logs (if applicable)

**PersistentVolumeClaims:**

| Service | PVC Name | Size | Access Mode |
|---------|----------|------|-------------|
| User PostgreSQL | user-service-postgresql | 10Gi | RWO |
| MasterData PostgreSQL | masterdata-service-postgresql | 10Gi | RWO |
| Transaction PostgreSQL | transaction-service-postgresql | 20Gi | RWO |
| Backup PostgreSQL | backup-service-postgresql | 10Gi | RWO |
| Backup Storage | backup-storage-pvc | 100Gi | RWO |
| Prometheus | prometheus-storage | 50Gi | RWO |
| Grafana | grafana-storage | 10Gi | RWO |
| Loki | loki-storage | 30Gi | RWO |

**Total Storage:** ~250Gi

### Backup Storage

**Location:** Kubernetes PersistentVolume (backup-storage-pvc)
**Retention:** 30 days (automated cleanup)
**Directory Structure:**
```
/backups/
├── user-service/
│   ├── backup-2026-03-01.tar.gz
│   └── backup-2026-03-02.tar.gz
├── masterdata-service/
├── transaction-service/
└── backup-service/
```

**Future:** Extend to S3/GCS for off-cluster backups

## Security Architecture

### Authentication and Authorization

**Authentication:** JWT (RS256)
**Token Lifetime:** 1 hour (configurable)
**Refresh Tokens:** Supported (24-hour lifetime)

**JWT Payload:**
```json
{
  "sub": "user-id",
  "name": "User Name",
  "email": "user@example.com",
  "roles": ["Admin", "Operator"],
  "exp": 1234567890,
  "iss": "qalitrack-gateway"
}
```

**Authorization:** Role-based access control (RBAC)
- Roles: Admin, Operator, Viewer, Technician
- Permissions checked at service level

### Network Security

**TLS/SSL:**
- All external traffic: HTTPS (TLS 1.2+)
- Internal traffic: HTTP (within cluster)
- Certificate management: cert-manager + Let's Encrypt

**NetworkPolicies:**
- Ingress: Explicit allow rules
- Egress: Default allow (DNS, internet), deny private networks
- Isolation: Cross-namespace communication blocked

### Secrets Management

**Storage:** Kubernetes Secrets (base64 encoded)
**Secrets:**
- JWT private/public keys (RS256 key pair)
- Database passwords
- Redis password
- RabbitMQ credentials
- SMTP credentials

**Rotation:** Manual (planned: External Secrets Operator)

**Access:** Mounted as environment variables or files in pods

### Pod Security

**Pod Security Standards:** Restricted (planned)
**Current Configuration:**
- Non-root user (UID 1000)
- Read-only root filesystem (where applicable)
- No privilege escalation
- Capabilities dropped

### RBAC (Kubernetes)

**ServiceAccounts:**
- Default service account per namespace
- ArgoCD service account (cluster-admin for GitOps)
- Prometheus service account (metrics reader)

**Roles and RoleBindings:** Standard Kubernetes RBAC

## Observability Architecture

### Metrics Collection

**Technology:** Prometheus (kube-prometheus-stack)
**Scrape Interval:** 30 seconds
**Retention:** 15 days
**Storage:** 50GB PVC

**ServiceMonitors:** Auto-discovery of metrics endpoints
- All microservices expose `/metrics` (Prometheus format)
- PostgreSQL metrics (postgres_exporter)
- Redis metrics (redis_exporter)
- NGINX Ingress metrics

**Key Metrics:**
- HTTP request rate, latency, errors (RED metrics)
- CPU, memory, disk usage (USE metrics)
- Database connections, query performance
- Cache hit rate

### Visualization

**Technology:** Grafana
**Dashboards:**
- Platform Overview (all services)
- Transaction Service (business metrics)
- Kubernetes Cluster (node/pod metrics)
- PostgreSQL Performance
- Redis Performance

**Alerting:** Alertmanager (configured via Prometheus)

### Log Aggregation

**Technology:** Loki
**Collection:** Promtail (DaemonSet on all nodes)
**Retention:** 30 days
**Storage:** 30GB PVC

**Log Format:** JSON structured logs
```json
{
  "timestamp": "2026-03-25T10:30:00Z",
  "level": "INFO",
  "service": "transaction-service",
  "message": "Transaction created",
  "transactionId": "TX-12345",
  "userId": "user-123"
}
```

**Querying:** LogQL via Grafana Explore

### Tracing (Planned)

**Technology:** OpenTelemetry + Jaeger
**Implementation:** Future enhancement

## Scalability Architecture

### Horizontal Scaling

**HorizontalPodAutoscaler (HPA):**
- Metric: CPU utilization
- Target: 70%
- Min replicas: 2
- Max replicas: 5

**Services with HPA:**
- Gateway Service
- User Service
- MasterData Service
- Transaction Service
- Technician Service
- QTruck API

### Vertical Scaling

**Resource Limits:**
- Request: Minimum guaranteed resources
- Limit: Maximum allowed resources

**Example (Transaction Service):**
```yaml
resources:
  requests:
    memory: "512Mi"
    cpu: "250m"
  limits:
    memory: "2Gi"
    cpu: "1000m"
```

### Database Scaling

**Current:** Single instance per service
**Future:**
- PostgreSQL replication (1 primary + 2 read replicas)
- Connection pooling (PgBouncer)
- Read/write splitting

### Caching Strategy

**Layers:**
1. Application-level caching (in-memory)
2. Redis distributed cache
3. Database query cache

**Cache Invalidation:**
- Time-based (TTL)
- Event-based (RabbitMQ events)

## High Availability

### Pod Replicas

**Production Configuration:**
- Gateway: 2 replicas (critical path)
- Services: 2 replicas each
- Databases: 1 replica (planned: 3 with replication)
- Redis: 1 instance (planned: Sentinel 3 nodes)
- RabbitMQ: 1 instance (planned: cluster 3 nodes)

### Pod Disruption Budgets

```yaml
apiVersion: policy/v1
kind: PodDisruptionBudget
metadata:
  name: gateway-service-pdb
spec:
  minAvailable: 1
  selector:
    matchLabels:
      app.kubernetes.io/name: gateway-service
```

### Health Checks

**Liveness Probe:** Detects dead pods (restart)
**Readiness Probe:** Detects unready pods (remove from service)

**Example:**
```yaml
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
  initialDelaySeconds: 10
  periodSeconds: 5
```

### Rolling Updates

**Strategy:** RollingUpdate
**Configuration:**
```yaml
strategy:
  type: RollingUpdate
  rollingUpdate:
    maxSurge: 1
    maxUnavailable: 0
```

Ensures zero downtime during updates.

## Disaster Recovery

### Backup Strategy

**Databases:**
- Daily automated backups (CronJob at 02:00)
- Stored on PersistentVolume
- 30-day retention
- Integrity verification (checksums)

**Configuration:**
- Helm charts stored in Git
- Kubernetes manifests in Git
- Secrets documented (not in Git)

### Recovery Procedures

**Database Restore:**
```bash
# Restore from backup
kubectl cp /backups/user-service/backup-2026-03-25.tar.gz \
  user-service-postgresql-0:/tmp/backup.tar.gz -n qalitrack-prod

kubectl exec -it user-service-postgresql-0 -n qalitrack-prod -- \
  psql -U postgres -d user_service_db -f /tmp/backup.sql
```

**Infrastructure Rebuild:**
```bash
# Re-deploy entire platform
cd kubernetes/helm-charts/qalitrack-platform
helm install qalitrack . -n qalitrack-prod
```

**RTO (Recovery Time Objective):** < 2 hours
**RPO (Recovery Point Objective):** < 24 hours (daily backups)

## Design Decisions

### Microservices vs Monolith

**Decision:** Microservices architecture
**Rationale:**
- Independent scaling
- Technology flexibility
- Team autonomy
- Fault isolation

**Trade-offs:**
- Increased complexity
- Distributed system challenges
- Network latency

### Database-per-Service

**Decision:** Separate PostgreSQL instance per service
**Rationale:**
- Data ownership and isolation
- Independent schema evolution
- Fault isolation
- Service autonomy

**Trade-offs:**
- Resource overhead (multiple DB instances)
- No cross-service transactions
- Data duplication (acceptable)

### Kubernetes Native

**Decision:** Kubernetes-first design
**Rationale:**
- Industry standard orchestration
- Declarative infrastructure
- Built-in scaling and self-healing
- Ecosystem tooling (Helm, Prometheus, etc.)

**Trade-offs:**
- Learning curve
- Operational complexity
- Infrastructure requirements

### Helm for Package Management

**Decision:** Helm charts for all components
**Rationale:**
- Templating and reusability
- Dependency management
- Versioned releases
- Rollback capability

**Trade-offs:**
- Template complexity
- Helm-specific knowledge required

### YARP for API Gateway

**Decision:** YARP (ASP.NET-based) vs NGINX/Kong
**Rationale:**
- .NET ecosystem alignment
- Configuration flexibility
- JWT integration
- Performance

**Trade-offs:**
- Less mature than NGINX/Kong
- Fewer community plugins

## Future Enhancements

### Service Mesh (Istio/Linkerd)

**Benefits:**
- mTLS for internal traffic
- Advanced traffic management
- Distributed tracing
- Circuit breaking

**Timeline:** Q3 2026

### Multi-Region Deployment

**Benefits:**
- Geographic redundancy
- Lower latency for global users
- Disaster recovery

**Timeline:** Q4 2026

### Event-Driven Architecture

**Enhancement:** Expand RabbitMQ usage
**Benefits:**
- Asynchronous processing
- Event sourcing
- CQRS pattern

**Timeline:** Q2 2026

### External Secrets Operator

**Benefits:**
- Integration with HashiCorp Vault
- Automated secret rotation
- Centralized secret management

**Timeline:** Q3 2026

**Version:** 1.0.0
**Last Updated:** 2026-03
