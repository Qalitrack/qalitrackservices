#!/bin/bash
set -e

# Check if secrets.env exists
if [ ! -f secrets.env ]; then
    echo "❌ Error: secrets.env not found!"
    echo ""
    echo "Please generate secrets first:"
    echo "  ./generate-secrets.sh"
    exit 1
fi

echo "🔐 Creating Kubernetes Secrets from secrets.env..."
echo ""

# Load secrets from file
source secrets.env

# Namespace
NAMESPACE="${NAMESPACE:-qalitrack-prod}"

echo "Target namespace: $NAMESPACE"
echo ""

# Check if namespace exists
if ! kubectl get namespace $NAMESPACE &> /dev/null; then
    echo "❌ Namespace $NAMESPACE does not exist!"
    echo ""
    echo "Create namespace first:"
    echo "  kubectl create namespace $NAMESPACE"
    echo "Or:"
    echo "  cd ../namespaces && ./apply-all.sh"
    exit 1
fi

# Delete existing secret if it exists
if kubectl get secret qalitrack-secrets -n $NAMESPACE &> /dev/null; then
    echo "⚠️  Secret 'qalitrack-secrets' already exists in namespace $NAMESPACE"
    read -p "Do you want to replace it? (y/N): " -n 1 -r
    echo
    if [[ $REPLY =~ ^[Yy]$ ]]; then
        kubectl delete secret qalitrack-secrets -n $NAMESPACE
        echo "✅ Deleted existing secret"
    else
        echo "❌ Aborted. Keeping existing secret."
        exit 0
    fi
fi

echo "Creating secret 'qalitrack-secrets' in namespace $NAMESPACE..."

# Create Kubernetes secret
kubectl create secret generic qalitrack-secrets \
  --from-literal=JWT_SECRET_KEY="$JWT_SECRET_KEY" \
  --from-literal=JWT_ISSUER="$JWT_ISSUER" \
  --from-literal=JWT_AUDIENCE="$JWT_AUDIENCE" \
  --from-literal=JWT_EXPIRATION_MINUTES="$JWT_EXPIRATION_MINUTES" \
  --from-literal=USER_SERVICE_DB_PASSWORD="$USER_SERVICE_DB_PASSWORD" \
  --from-literal=MASTER_DATA_DB_PASSWORD="$MASTER_DATA_DB_PASSWORD" \
  --from-literal=TRANSACTION_DB_PASSWORD="$TRANSACTION_DB_PASSWORD" \
  --from-literal=TECHNICIAN_DB_PASSWORD="$TECHNICIAN_DB_PASSWORD" \
  --from-literal=POSTGRES_ADMIN_PASSWORD="$POSTGRES_ADMIN_PASSWORD" \
  --from-literal=USER_SERVICE_REDIS_PASSWORD="$USER_SERVICE_REDIS_PASSWORD" \
  --from-literal=USER_SERVICE_REDIS_CONNECTION_STRING="$USER_SERVICE_REDIS_CONNECTION_STRING" \
  --from-literal=USER_SERVICE_DB_CONNECTION_STRING="$USER_SERVICE_DB_CONNECTION_STRING" \
  --from-literal=MASTER_DATA_DB_CONNECTION_STRING="$MASTER_DATA_DB_CONNECTION_STRING" \
  --from-literal=TRANSACTION_DB_CONNECTION_STRING="$TRANSACTION_DB_CONNECTION_STRING" \
  --from-literal=TECHNICIAN_DB_CONNECTION_STRING="$TECHNICIAN_DB_CONNECTION_STRING" \
  --from-literal=TECHNICIAN_USER_SERVICE_DB_CONNECTION_STRING="$TECHNICIAN_USER_SERVICE_DB_CONNECTION_STRING" \
  --from-literal=TECHNICIAN_REDIS_CONNECTION_STRING="$TECHNICIAN_REDIS_CONNECTION_STRING" \
  --from-literal=BACKUP_SERVICE_DB_CONNECTION_STRING="$BACKUP_SERVICE_DB_CONNECTION_STRING" \
  --from-literal=BACKUP_SERVICE_REDIS_CONNECTION_STRING="$BACKUP_SERVICE_REDIS_CONNECTION_STRING" \
  --from-literal=RABBITMQ_PASSWORD="$RABBITMQ_PASSWORD" \
  --from-literal=RABBITMQ_ERLANG_COOKIE="$RABBITMQ_ERLANG_COOKIE" \
  --from-literal=EMAIL_SMTP_HOST="$EMAIL_SMTP_HOST" \
  --from-literal=EMAIL_SMTP_PORT="$EMAIL_SMTP_PORT" \
  --from-literal=EMAIL_SMTP_USERNAME="$EMAIL_SMTP_USERNAME" \
  --from-literal=EMAIL_SMTP_PASSWORD="$EMAIL_SMTP_PASSWORD" \
  --from-literal=EMAIL_FROM_EMAIL="$EMAIL_FROM_EMAIL" \
  --from-literal=EMAIL_FROM_NAME="$EMAIL_FROM_NAME" \
  --from-literal=EMAIL_ENABLE_SSL="$EMAIL_ENABLE_SSL" \
  -n $NAMESPACE

echo "✅ Secret created successfully!"
echo ""
echo "Verify secret:"
echo "  kubectl get secret qalitrack-secrets -n $NAMESPACE"
echo "  kubectl describe secret qalitrack-secrets -n $NAMESPACE"
echo ""
echo "⚠️  Security reminder:"
echo "  - Secrets are base64 encoded (NOT encrypted) in Kubernetes"
echo "  - Consider using Sealed Secrets or External Secrets Operator for production"
echo "  - Enable RBAC to restrict access to secrets"
echo "  - Rotate secrets periodically"
