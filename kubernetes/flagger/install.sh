#!/bin/bash
set -e

echo "🚦 Installing Flagger for Progressive Delivery..."

# Add Flagger Helm repository
echo "📦 Adding Flagger Helm repository..."
helm repo add flagger https://flagger.app
helm repo update

# Install Flagger
echo "🚀 Installing Flagger..."
helm upgrade --install flagger flagger/flagger \
  --namespace flagger-system \
  --create-namespace \
  --set meshProvider=kubernetes \
  --set metricsServer=http://prometheus-kube-prometheus-prometheus.qalitrack-monitoring:9090 \
  --wait

# Install Flagger Loadtester (for automated testing)
echo "🧪 Installing Flagger Loadtester..."
helm upgrade --install flagger-loadtester flagger/loadtester \
  --namespace flagger-system \
  --set cmd.timeout=1h

echo "✅ Flagger installed successfully"

# Verify installation
echo ""
echo "📋 Checking Flagger status..."
kubectl get pods -n flagger-system

echo ""
echo "🎉 Flagger setup complete!"
echo ""
echo "Next steps:"
echo "1. Apply canary configurations for your services:"
echo "   kubectl apply -f gateway-canary.yaml"
echo "   kubectl apply -f user-service-canary.yaml"
echo ""
echo "2. Deploy a new version to trigger canary release:"
echo "   kubectl set image deployment/gateway-service gateway-service=ghcr.io/qalitrack/qalitrackservices/gateway-service:v2.0.0 -n qalitrack-prod"
echo ""
echo "3. Watch canary progress:"
echo "   kubectl get canary -n qalitrack-prod -w"
echo ""
echo "Monitor canary metrics:"
echo "  kubectl logs -n flagger-system deployment/flagger -f"
