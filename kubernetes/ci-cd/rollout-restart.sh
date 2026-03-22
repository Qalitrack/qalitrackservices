#!/bin/bash
set -e

# Script to manually trigger rollout restart for services
# Use this when you want to pull latest images without changing Helm chart

NAMESPACE="${NAMESPACE:-qalitrack-prod}"

show_usage() {
    cat << EOF
Usage: $0 [OPTIONS] <service>

Trigger a rolling restart for a Qalitrack service to pull latest images.

Services:
  gateway       - Gateway Service
  user          - User Service
  masterdata    - MasterData Service
  transaction   - Transaction Service
  backup        - Backup Service
  all           - All services

Options:
  -n, --namespace NAMESPACE    Kubernetes namespace (default: qalitrack-prod)
  -w, --wait                   Wait for rollout to complete
  -h, --help                   Show this help message

Examples:
  # Restart gateway service
  $0 gateway

  # Restart all services and wait
  $0 --wait all

  # Restart user service in staging
  $0 -n qalitrack-staging user
EOF
}

WAIT=false

# Parse arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        -n|--namespace)
            NAMESPACE="$2"
            shift 2
            ;;
        -w|--wait)
            WAIT=true
            shift
            ;;
        -h|--help)
            show_usage
            exit 0
            ;;
        *)
            SERVICE="$1"
            shift
            ;;
    esac
done

if [ -z "$SERVICE" ]; then
    echo "Error: Service name required"
    show_usage
    exit 1
fi

restart_service() {
    local service_name=$1

    echo "🔄 Restarting $service_name..."
    kubectl rollout restart deployment/$service_name -n $NAMESPACE

    if [ "$WAIT" = true ]; then
        echo "⏳ Waiting for rollout to complete..."
        kubectl rollout status deployment/$service_name -n $NAMESPACE --timeout=5m
        echo "✅ $service_name restarted successfully"
    fi
}

case "$SERVICE" in
    gateway)
        restart_service "gateway-service"
        ;;
    user)
        restart_service "user-service"
        ;;
    masterdata)
        restart_service "masterdata-service"
        ;;
    transaction)
        restart_service "transaction-service"
        ;;
    backup)
        restart_service "backup-service"
        ;;
    all)
        echo "🔄 Restarting ALL services..."
        restart_service "gateway-service"
        restart_service "user-service"
        restart_service "masterdata-service"
        restart_service "transaction-service"
        restart_service "backup-service"
        echo "✅ All services restarted"
        ;;
    *)
        echo "Error: Unknown service '$SERVICE'"
        show_usage
        exit 1
        ;;
esac

echo ""
echo "Check pod status with:"
echo "  kubectl get pods -n $NAMESPACE"
