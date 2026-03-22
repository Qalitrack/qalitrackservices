# Qalitrack Kubernetes Namespaces

This directory contains namespace definitions for the Qalitrack microservices platform.

## Namespace Structure

### 🏭 Production Environment: `qalitrack-prod`
**Purpose**: Production workloads serving live customers

**Resources**:
- CPU Requests: 20 cores, Limits: 40 cores
- Memory Requests: 40GB, Limits: 80GB
- Persistent Volume Claims: 20
- Services: 50

**Services Deployed**:
- Gateway Service (JWT Auth)
- User Service
- MasterData Service
- Transaction Service
- Backup Service
- PostgreSQL databases
- Redis cache
- RabbitMQ messaging

---

### 🧪 Staging Environment: `qalitrack-staging`
**Purpose**: Pre-production testing and validation

**Resources**:
- CPU Requests: 10 cores, Limits: 20 cores
- Memory Requests: 20GB, Limits: 40GB
- Persistent Volume Claims: 15
- Services: 30

**Usage**: Test new features before production deployment

---

### 💻 Development Environment: `qalitrack-dev`
**Purpose**: Active development and testing

**Resources**:
- CPU Requests: 5 cores, Limits: 10 cores
- Memory Requests: 10GB, Limits: 20GB
- Persistent Volume Claims: 10
- Services: 20

**Usage**: Developer testing and experimentation

---

### 📊 Monitoring: `qalitrack-monitoring`
**Purpose**: Observability stack (Prometheus, Grafana, Loki)

**Resources**:
- CPU Requests: 4 cores, Limits: 8 cores
- Memory Requests: 8GB, Limits: 16GB
- Persistent Volume Claims: 10
- Services: 15

**Future Services**:
- Prometheus (metrics)
- Grafana (dashboards)
- Loki (logs)
- AlertManager (alerts)

---

## Deployment Instructions

### Create All Namespaces
```bash
kubectl apply -f qalitrack-prod.yaml
kubectl apply -f qalitrack-staging.yaml
kubectl apply -f qalitrack-dev.yaml
kubectl apply -f qalitrack-monitoring.yaml
```

Or use the provided script:
```bash
./apply-all.sh
```

### Verify Namespaces
```bash
kubectl get namespaces -l managed-by=qalitrack
```

### View Resource Quotas
```bash
kubectl get resourcequota -n qalitrack-prod
kubectl get resourcequota -n qalitrack-staging
kubectl get resourcequota -n qalitrack-dev
kubectl get resourcequota -n qalitrack-monitoring
```

### View Limit Ranges
```bash
kubectl describe limitrange -n qalitrack-prod
```

---

## Resource Limits per Container

| Environment | Default CPU | Default Memory | Max CPU | Max Memory |
|------------|-------------|----------------|---------|------------|
| Production | 250m        | 256Mi          | 4 cores | 8Gi        |
| Staging    | 100m        | 128Mi          | 2 cores | 4Gi        |
| Development| 100m        | 128Mi          | 1 core  | 2Gi        |
| Monitoring | 100m        | 256Mi          | 2 cores | 4Gi        |

---

## Labels

All namespaces use consistent labels:
- `name`: namespace name
- `environment`: prod/staging/dev/shared
- `managed-by`: qalitrack
- `app.kubernetes.io/part-of`: qalitrack

---

## Best Practices

1. **Always deploy to dev first**, then staging, then production
2. **Use resource requests** to ensure pods get minimum resources
3. **Set resource limits** to prevent resource exhaustion
4. **Monitor quota usage** regularly
5. **Use network policies** to isolate environments
6. **Keep secrets separate** per namespace
7. **Use separate service accounts** per environment

---

## Next Steps

1. ✅ Create namespaces (this step)
2. ⏭️ Set up persistent volumes
3. ⏭️ Create ConfigMaps and Secrets
4. ⏭️ Deploy databases (PostgreSQL, Redis, RabbitMQ)
5. ⏭️ Deploy microservices
6. ⏭️ Configure ingress and load balancing
7. ⏭️ Set up monitoring and logging
