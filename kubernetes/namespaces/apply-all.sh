#!/bin/bash

echo "🚀 Creating Qalitrack Kubernetes Namespaces..."
echo ""

echo "📦 Creating Production namespace..."
kubectl apply -f qalitrack-prod.yaml

echo ""
echo "🧪 Creating Staging namespace..."
kubectl apply -f qalitrack-staging.yaml

echo ""
echo "💻 Creating Development namespace..."
kubectl apply -f qalitrack-dev.yaml

echo ""
echo "📊 Creating Monitoring namespace..."
kubectl apply -f qalitrack-monitoring.yaml

echo ""
echo "✅ All namespaces created successfully!"
echo ""
echo "Verifying namespaces..."
kubectl get namespaces -l managed-by=qalitrack

echo ""
echo "📋 Resource Quotas:"
echo ""
echo "Production:"
kubectl get resourcequota -n qalitrack-prod

echo ""
echo "Staging:"
kubectl get resourcequota -n qalitrack-staging

echo ""
echo "Development:"
kubectl get resourcequota -n qalitrack-dev

echo ""
echo "Monitoring:"
kubectl get resourcequota -n qalitrack-monitoring

echo ""
echo "✨ Setup complete! Ready for service deployments."
