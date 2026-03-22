#!/bin/bash
set -e

echo "📊 Installing Prometheus + Grafana monitoring stack..."

# Add Prometheus community Helm repository
echo "📦 Adding prometheus-community Helm repository..."
helm repo add prometheus-community https://prometheus-community.github.io/helm-charts
helm repo update

# Install kube-prometheus-stack
echo "🚀 Installing kube-prometheus-stack..."
helm upgrade --install prometheus prometheus-community/kube-prometheus-stack \
  --namespace qalitrack-monitoring \
  --create-namespace \
  --values values-prometheus-stack.yaml \
  --wait \
  --timeout 10m

echo "✅ Prometheus stack installed successfully"

# Wait for pods to be ready
echo "⏳ Waiting for monitoring pods to be ready..."
kubectl wait --for=condition=ready pod \
  -l "app.kubernetes.io/part-of=kube-prometheus-stack" \
  -n qalitrack-monitoring \
  --timeout=300s

# Install ServiceMonitors for Qalitrack services
echo "📡 Installing ServiceMonitors for Qalitrack services..."
kubectl apply -f servicemonitors.yaml

echo ""
echo "🎉 Monitoring setup complete!"
echo ""
echo "Access Grafana:"
echo "  kubectl port-forward -n qalitrack-monitoring svc/prometheus-grafana 3000:80"
echo "  Then open: http://localhost:3000"
echo "  Username: admin"
echo "  Password: admin (change this in values-prometheus-stack.yaml)"
echo ""
echo "Access Prometheus:"
echo "  kubectl port-forward -n qalitrack-monitoring svc/prometheus-kube-prometheus-prometheus 9090:9090"
echo "  Then open: http://localhost:9090"
echo ""
echo "Access AlertManager:"
echo "  kubectl port-forward -n qalitrack-monitoring svc/prometheus-kube-prometheus-alertmanager 9093:9093"
echo "  Then open: http://localhost:9093"
echo ""
echo "Check monitoring status:"
echo "  kubectl get pods -n qalitrack-monitoring"
echo "  kubectl get servicemonitors -n qalitrack-prod"
