#!/bin/bash
set -e

echo "🌐 Installing NGINX Ingress Controller..."

# Add ingress-nginx Helm repository
echo "📦 Adding ingress-nginx Helm repository..."
helm repo add ingress-nginx https://kubernetes.github.io/ingress-nginx
helm repo update

# Install NGINX Ingress Controller
echo "🚀 Installing NGINX Ingress Controller..."
helm upgrade --install ingress-nginx ingress-nginx/ingress-nginx \
  --namespace ingress-nginx \
  --create-namespace \
  --values values-ingress.yaml \
  --wait \
  --timeout 5m

echo "✅ NGINX Ingress Controller installed successfully"

# Wait for LoadBalancer to get external IP
echo "⏳ Waiting for LoadBalancer to get external IP..."
kubectl wait --for=jsonpath='{.status.loadBalancer.ingress}' \
  service/ingress-nginx-controller \
  -n ingress-nginx \
  --timeout=300s || true

# Get the external IP
echo ""
echo "📋 Ingress Controller Status:"
kubectl get svc -n ingress-nginx ingress-nginx-controller

EXTERNAL_IP=$(kubectl get svc ingress-nginx-controller -n ingress-nginx \
  -o jsonpath='{.status.loadBalancer.ingress[0].ip}' 2>/dev/null || echo "pending")

echo ""
echo "🎉 NGINX Ingress Controller is ready!"
echo ""
echo "External IP: $EXTERNAL_IP"
echo ""
echo "Next steps:"
echo "1. Point your DNS to the external IP:"
echo "   qalibrated.co.ke  A  $EXTERNAL_IP"
echo ""
echo "2. Enable ingress in your Helm values:"
echo "   ingress:"
echo "     enabled: true"
echo ""
echo "3. Deploy/upgrade your application"
echo ""
echo "4. Install cert-manager for SSL (if not already installed):"
echo "   cd ../cert-manager && ./install.sh"
echo ""
echo "Check ingress status:"
echo "  kubectl get ingress -A"
echo "  kubectl logs -n ingress-nginx deployment/ingress-nginx-controller"
