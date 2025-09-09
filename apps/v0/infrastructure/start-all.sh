#!/bin/bash

# QaliTrack Infrastructure Startup Script
# Starts all infrastructure services in the correct dependency order

set -e  # Exit on any error

echo "🚀 Starting QaliTrack Infrastructure Services"
echo "================================================"

# Change to the parent directory so docker-compose can find the files
cd "$(dirname "$0")/.."

# Function to wait for services to be healthy
wait_for_healthy() {
    local compose_file=$1
    local service_name=$2
    local max_attempts=30
    local attempt=1
    
    echo "⏳ Waiting for $service_name to be healthy..."
    
    while [ $attempt -le $max_attempts ]; do
        if docker-compose -f "$compose_file" ps "$service_name" | grep -q "healthy\|Up"; then
            echo "✅ $service_name is healthy"
            return 0
        fi
        
        echo "   Attempt $attempt/$max_attempts - $service_name not ready yet..."
        sleep 5
        attempt=$((attempt + 1))
    done
    
    echo "❌ $service_name failed to become healthy within timeout"
    return 1
}

# Function to check if service is running
is_service_running() {
    local compose_file=$1
    local service_name=$2
    docker-compose -f "$compose_file" ps "$service_name" | grep -q "Up"
}

echo "📦 Step 1: Creating volumes and networks..."
docker-compose -f infrastructure/00-volumes-networks.yml up -d
echo "✅ Volumes and networks created"

echo ""
echo "💾 Step 2: Starting database services..."
docker-compose -f infrastructure/01-databases.yml up -d

# Wait for databases to be ready
wait_for_healthy "infrastructure/01-databases.yml" "postgres"
wait_for_healthy "infrastructure/01-databases.yml" "postgres-backup"

echo ""
echo "🗄️  Step 3: Starting cache and messaging services..."
docker-compose -f infrastructure/02-cache-messaging.yml up -d

wait_for_healthy "infrastructure/02-cache-messaging.yml" "redis"
wait_for_healthy "infrastructure/02-cache-messaging.yml" "rabbitmq"

echo ""
echo "📦 Step 4: Starting storage and search services..."
docker-compose -f infrastructure/03-storage-search.yml up -d

wait_for_healthy "infrastructure/03-storage-search.yml" "minio"
wait_for_healthy "infrastructure/03-storage-search.yml" "zincsearch"

echo ""
echo "🔐 Step 5: Starting Vault secrets management..."
docker-compose -f infrastructure/04-secrets.yml up -d

wait_for_healthy "infrastructure/04-secrets.yml" "vault"

echo ""
echo "📧 Step 6: Starting email testing service..."
docker-compose -f infrastructure/05-email.yml up -d

wait_for_healthy "infrastructure/05-email.yml" "mailpit"

echo ""
echo "⚙️  Step 7: Initializing Vault with dynamic secrets engines..."
echo "   This may take a few moments as Vault configures PostgreSQL and RabbitMQ secrets engines..."

# Run vault-init and capture output
docker-compose -f infrastructure/99-secrets-init.yml up > vault-init.log 2>&1 &
VAULT_INIT_PID=$!

# Wait for vault-init to complete
wait $VAULT_INIT_PID

echo ""
echo "📋 Vault initialization completed! Check the logs:"
echo "   docker-compose -f infrastructure/99-secrets-init.yml logs vault-init"

echo ""
echo "🎉 All infrastructure services are now running!"
echo ""
echo "📊 Service Status:"
echo "=================="

# Show status of all services
docker-compose \
  -f infrastructure/01-databases.yml \
  -f infrastructure/02-cache-messaging.yml \
  -f infrastructure/03-storage-search.yml \
  -f infrastructure/04-secrets.yml \
  -f infrastructure/05-email.yml \
  ps

echo ""
echo "🔗 Access URLs:"
echo "==============="
echo "• Vault UI:           http://localhost:8200 (token: myroot)"
echo "• RabbitMQ Mgmt:      http://localhost:15672 (vault-rabbitmq-admin/vault-rabbitmq-admin-pass)"
echo "• MinIO Console:      http://localhost:9001 (vault-minio-admin/vault-minio-admin-pass)"
echo "• ZincSearch:         http://localhost:4080 (vault-zinc-admin/vault-zinc-admin-pass)"
echo "• Mailpit:            http://localhost:8025"
echo ""
echo "🗂️  Database Connections (Admin - for Vault management only):"
echo "• PostgreSQL Main:    localhost:5432 (vault-admin/vault-postgres-admin-pass)"
echo "• PostgreSQL Backup:  localhost:5435 (vault-backup-admin/vault-backup-admin-pass)"
echo "• Redis:              localhost:6379 (password: vault-redis-admin-pass)"
echo ""
echo "🔑 Next Steps:"
echo "============="
echo "1. Get AppRole credentials for applications:"
echo "   docker-compose -f infrastructure/99-secrets-init.yml logs vault-init"
echo ""
echo "2. Test dynamic credential generation:"
echo "   docker-compose -f infrastructure/04-secrets.yml exec vault vault read database/creds/postgres-main-role"
echo ""
echo "3. Set environment variables for applications:"
echo "   export VAULT_ROLE_ID=\"your-role-id\""
echo "   export VAULT_SECRET_ID=\"your-secret-id\""
echo ""
echo "✅ Infrastructure startup complete!"