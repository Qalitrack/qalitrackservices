# QaliTrack Infrastructure - Modular Docker Compose Setup

This directory contains modular Docker Compose files for QaliTrack infrastructure services, organized by startup order and dependencies.

## 📂 File Organization

The infrastructure is split into numbered YAML files that must be started in order:

### 00-volumes-networks.yml
- **Purpose**: Shared Docker volumes and networks
- **Dependencies**: None
- **Contains**: All volume definitions and the `microservices` network

### 01-databases.yml
- **Purpose**: PostgreSQL database services
- **Dependencies**: volumes-networks
- **Contains**: 
  - `postgres` (main database on port 5432)
  - `postgres-backup` (backup service database on port 5435)

### 02-cache-messaging.yml
- **Purpose**: Cache and messaging services
- **Dependencies**: volumes-networks
- **Contains**:
  - `redis` (cache on port 6379)
  - `rabbitmq` (message broker on ports 5672/15672)

### 03-storage-search.yml
- **Purpose**: Object storage and search services
- **Dependencies**: volumes-networks
- **Contains**:
  - `minio` (S3-compatible storage on ports 9000/9001)
  - `zincsearch` (search engine on port 4080)

### 04-secrets.yml
- **Purpose**: HashiCorp Vault secret management
- **Dependencies**: volumes-networks
- **Contains**:
  - `vault` (secrets management on port 8200)

### 05-email.yml
- **Purpose**: Email testing service
- **Dependencies**: volumes-networks
- **Contains**:
  - `mailpit` (SMTP testing on ports 1025/8025)

### 99-secrets-init.yml
- **Purpose**: Vault initialization and configuration
- **Dependencies**: ALL other services must be healthy
- **Contains**:
  - `vault-init` (one-time setup service)

## 🚀 Startup Instructions

### Option 1: Manual Sequential Startup (Recommended for First Time)

```bash
# 1. Create volumes and networks first
docker-compose -f infrastructure/00-volumes-networks.yml up -d

# 2. Start databases
docker-compose -f infrastructure/01-databases.yml up -d

# 3. Wait for databases to be healthy, then start cache/messaging
docker-compose -f infrastructure/02-cache-messaging.yml up -d

# 4. Start storage and search services
docker-compose -f infrastructure/03-storage-search.yml up -d

# 5. Start Vault
docker-compose -f infrastructure/04-secrets.yml up -d

# 6. Start email service
docker-compose -f infrastructure/05-email.yml up -d

# 7. Wait for all services to be healthy, then initialize Vault
docker-compose -f infrastructure/99-secrets-init.yml up

# 8. Check vault-init logs to get Role ID and Secret ID
docker-compose -f infrastructure/99-secrets-init.yml logs vault-init
```

### Option 2: Automated Startup Script (After Testing)

```bash
# Use the provided startup script
./infrastructure/start-all.sh
```

### Option 3: Combined Single Command (Quick Start)

```bash
# Start all services in dependency order
docker-compose \
  -f infrastructure/00-volumes-networks.yml \
  -f infrastructure/01-databases.yml \
  -f infrastructure/02-cache-messaging.yml \
  -f infrastructure/03-storage-search.yml \
  -f infrastructure/04-secrets.yml \
  -f infrastructure/05-email.yml \
  up -d

# Then manually run vault initialization
docker-compose -f infrastructure/99-secrets-init.yml up
```

## 🔍 Service Health Checks

All services include health checks. Monitor status with:

```bash
# Check all services
docker-compose \
  -f infrastructure/00-volumes-networks.yml \
  -f infrastructure/01-databases.yml \
  -f infrastructure/02-cache-messaging.yml \
  -f infrastructure/03-storage-search.yml \
  -f infrastructure/04-secrets.yml \
  -f infrastructure/05-email.yml \
  ps

# Check specific service health
docker-compose -f infrastructure/01-databases.yml ps postgres
```

## 🛑 Shutdown Instructions

### Graceful Shutdown (Reverse Order)

```bash
# Stop vault-init first (if running)
docker-compose -f infrastructure/99-secrets-init.yml down

# Stop services in reverse dependency order
docker-compose -f infrastructure/05-email.yml down
docker-compose -f infrastructure/04-secrets.yml down
docker-compose -f infrastructure/03-storage-search.yml down
docker-compose -f infrastructure/02-cache-messaging.yml down
docker-compose -f infrastructure/01-databases.yml down

# Remove volumes and networks last (optional - will destroy data)
# docker-compose -f infrastructure/00-volumes-networks.yml down
```

### Quick Shutdown (All at Once)

```bash
# Stop all infrastructure services
docker-compose \
  -f infrastructure/00-volumes-networks.yml \
  -f infrastructure/01-databases.yml \
  -f infrastructure/02-cache-messaging.yml \
  -f infrastructure/03-storage-search.yml \
  -f infrastructure/04-secrets.yml \
  -f infrastructure/05-email.yml \
  -f infrastructure/99-secrets-init.yml \
  down
```

## 🔧 Service Access Information

### Database Services (Admin Credentials - For Vault Management Only)
- **PostgreSQL Main**: `localhost:5432` 
  - Username: `vault-admin`
  - Password: `vault-postgres-admin-pass`
  - Database: `microservices`
  
- **PostgreSQL Backup**: `localhost:5435`
  - Username: `vault-backup-admin` 
  - Password: `vault-backup-admin-pass`
  - Database: `backupservicedb`

### Cache & Messaging (Admin Credentials - For Vault Management Only)
- **Redis**: `localhost:6379`
  - Password: `vault-redis-admin-pass`
  
- **RabbitMQ Management**: `http://localhost:15672`
  - Username: `vault-rabbitmq-admin`
  - Password: `vault-rabbitmq-admin-pass`

### Storage & Search (Admin Credentials - For Vault Management Only)
- **MinIO Console**: `http://localhost:9001`
  - Username: `vault-minio-admin`
  - Password: `vault-minio-admin-pass`
  
- **MinIO API**: `http://localhost:9000`

- **ZincSearch**: `http://localhost:4080`
  - Username: `vault-zinc-admin`
  - Password: `vault-zinc-admin-pass`

### Secrets Management
- **Vault UI**: `http://localhost:8200`
  - Token: `myroot` (development mode)

### Email Testing  
- **Mailpit Web UI**: `http://localhost:8025`
- **SMTP Server**: `localhost:1025` (no authentication required)

## ⚠️ Important Security Notes

1. **Admin Credentials**: The credentials listed above are **ADMIN ONLY** credentials used by Vault to manage dynamic user creation.

2. **Application Access**: Applications should **NEVER** use these admin credentials directly. Instead, they should:
   - Authenticate to Vault using AppRole (Role ID + Secret ID)
   - Get dynamic credentials from Vault's secrets engines
   - Use the dynamic credentials to connect to services

3. **Dynamic Credentials**: 
   - PostgreSQL: `vault read database/creds/postgres-main-role`
   - RabbitMQ: `vault read rabbitmq/creds/rabbitmq-role`
   - Redis/MinIO/ZincSearch: `vault kv get secret/services/redis` (admin credentials via KV)

## 🧪 Testing Infrastructure

### Test All Services Are Running
```bash
# Check PostgreSQL
docker-compose -f infrastructure/01-databases.yml exec postgres pg_isready -U vault-admin -d microservices

# Check Redis
docker-compose -f infrastructure/02-cache-messaging.yml exec redis redis-cli --no-auth-warning -a vault-redis-admin-pass ping

# Check Vault
curl -s http://localhost:8200/v1/sys/health | jq

# Test dynamic credential generation
docker-compose -f infrastructure/04-secrets.yml exec vault vault read database/creds/postgres-main-role
```

### Get AppRole Credentials for Applications
```bash
# Get Role ID and Secret ID from vault-init logs
docker-compose -f infrastructure/99-secrets-init.yml logs vault-init

# Or get them directly from Vault
docker-compose -f infrastructure/04-secrets.yml exec vault vault read auth/approle/role/qalitrack-services/role-id
docker-compose -f infrastructure/04-secrets.yml exec vault vault write -field=secret_id -f auth/approle/role/qalitrack-services/secret-id
```

## 🔄 Maintenance

### Update Services
```bash
# Pull latest images
docker-compose -f infrastructure/01-databases.yml pull
docker-compose -f infrastructure/02-cache-messaging.yml pull
# ... repeat for all files

# Restart with new images
docker-compose -f infrastructure/01-databases.yml up -d
# ... repeat in proper order
```

### Reset Vault (Development Only)
```bash
# Stop vault and vault-init
docker-compose -f infrastructure/99-secrets-init.yml down
docker-compose -f infrastructure/04-secrets.yml down

# Remove vault data
docker volume rm v0_vault_data v0_vault_logs

# Restart vault and re-initialize
docker-compose -f infrastructure/04-secrets.yml up -d
# Wait for vault to be ready...
docker-compose -f infrastructure/99-secrets-init.yml up
```

## 🐛 Troubleshooting

### Common Issues

1. **Services Not Starting**: Check startup order, volumes must be created first
2. **vault-init Failing**: Ensure all other services are healthy before running
3. **Connection Refused**: Check if services are on the `microservices` network
4. **Permission Denied**: Ensure Docker has proper permissions for volume mounts

### Debug Commands
```bash
# Check service logs
docker-compose -f infrastructure/01-databases.yml logs postgres

# Check service health
docker-compose -f infrastructure/01-databases.yml ps

# Enter service container
docker-compose -f infrastructure/01-databases.yml exec postgres bash

# Check network connectivity
docker-compose -f infrastructure/01-databases.yml exec postgres ping redis
```