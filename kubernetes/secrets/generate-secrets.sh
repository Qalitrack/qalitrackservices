#!/bin/bash
set -e

echo "🔐 Generating Qalitrack Secrets..."
echo ""
echo "WARNING: Store these secrets securely! Do NOT commit to Git!"
echo ""

# Function to generate random password
generate_password() {
    openssl rand -base64 32 | tr -d "=+/" | cut -c1-32
}

# Function to generate JWT secret (64 characters)
generate_jwt_secret() {
    openssl rand -base64 48 | tr -d "=+/" | cut -c1-64
}

# Generate all secrets
JWT_SECRET=$(generate_jwt_secret)
DB_PASSWORD=$(generate_password)
POSTGRES_ADMIN_PASSWORD=$(generate_password)
REDIS_PASSWORD=$(generate_password)
RABBITMQ_PASSWORD=$(generate_password)
RABBITMQ_ERLANG_COOKIE=$(generate_password)

# Create secrets.env file
cat > secrets.env << EOF
# Qalitrack Production Secrets
# Generated on: $(date)
# DO NOT COMMIT THIS FILE TO GIT!

# JWT Configuration
JWT_SECRET_KEY=$JWT_SECRET
JWT_ISSUER=qalitrack
JWT_AUDIENCE=qalitrack
JWT_EXPIRATION_MINUTES=60

# Database — one shared DB (qalitrackdb), one app user (qalitrack),
# per-service schemas (masterdata/transactions/users/backup)
DB_USER=qalitrack
DB_NAME=qalitrackdb
DB_PASSWORD=$DB_PASSWORD
POSTGRES_ADMIN_PASSWORD=$POSTGRES_ADMIN_PASSWORD

# Redis Password
USER_SERVICE_REDIS_PASSWORD=$REDIS_PASSWORD

# Connection Strings (assembled from components above)
USER_SERVICE_REDIS_CONNECTION_STRING=user-service-redis-master:6379,password=$REDIS_PASSWORD,abortConnect=false
DB_CONNECTION_STRING=Host=qalitrack-postgresql;Port=5432;Database=qalitrackdb;Username=qalitrack;Password=$DB_PASSWORD;Pooling=true

# RabbitMQ
RABBITMQ_PASSWORD=$RABBITMQ_PASSWORD
RABBITMQ_ERLANG_COOKIE=$RABBITMQ_ERLANG_COOKIE

# Email Configuration (optional - update with your SMTP details)
EMAIL_SMTP_HOST=smtp.gmail.com
EMAIL_SMTP_PORT=587
EMAIL_SMTP_USERNAME=your-email@gmail.com
EMAIL_SMTP_PASSWORD=your-app-password
EMAIL_FROM_EMAIL=noreply@qalibrated.co.ke
EMAIL_FROM_NAME=Qalitrack System
EMAIL_ENABLE_SSL=true
EOF

echo "✅ Secrets generated and saved to: secrets.env"
echo ""
echo "📋 Generated Secrets Summary:"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "JWT Secret:              ${JWT_SECRET:0:10}... (64 chars)"
echo "DB Password:             ${DB_PASSWORD:0:10}... (32 chars)"
echo "Postgres Admin Password: ${POSTGRES_ADMIN_PASSWORD:0:10}... (32 chars)"
echo "Redis Password:          ${REDIS_PASSWORD:0:10}... (32 chars)"
echo "RabbitMQ Password:       ${RABBITMQ_PASSWORD:0:10}... (32 chars)"
echo "RabbitMQ Erlang Cookie:  ${RABBITMQ_ERLANG_COOKIE:0:10}... (32 chars)"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo ""
echo "Next steps:"
echo "1. Review and update EMAIL settings in secrets.env"
echo "2. Create Kubernetes secrets:"
echo "   ./create-k8s-secrets.sh"
echo ""
echo "3. Backup secrets.env to a secure location (password manager, vault, etc.)"
echo "4. Add secrets.env to .gitignore (already done if using provided .gitignore)"
echo ""
echo "⚠️  IMPORTANT: Keep secrets.env file safe and never commit it to Git!"
