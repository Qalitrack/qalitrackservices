#!/bin/bash

echo "⚠️  WARNING: This will delete ALL Qalitrack namespaces and their resources!"
echo ""
read -p "Are you sure you want to continue? (yes/no): " confirm

if [ "$confirm" != "yes" ]; then
    echo "❌ Aborted."
    exit 0
fi

echo ""
echo "🗑️  Deleting Qalitrack Kubernetes Namespaces..."
echo ""

echo "Deleting Development namespace..."
kubectl delete -f qalitrack-dev.yaml --ignore-not-found=true

echo ""
echo "Deleting Staging namespace..."
kubectl delete -f qalitrack-staging.yaml --ignore-not-found=true

echo ""
echo "Deleting Production namespace..."
kubectl delete -f qalitrack-prod.yaml --ignore-not-found=true

echo ""
echo "Deleting Monitoring namespace..."
kubectl delete -f qalitrack-monitoring.yaml --ignore-not-found=true

echo ""
echo "✅ All namespaces deleted."
echo ""
echo "Verifying deletion..."
kubectl get namespaces -l managed-by=qalitrack
