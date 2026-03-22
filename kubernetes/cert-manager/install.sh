#!/bin/bash
set -e

echo "🔒 Installing cert-manager for automatic SSL/TLS certificates..."

# Install cert-manager CRDs and components
echo "📦 Installing cert-manager v1.13.0..."
kubectl apply -f https://github.com/cert-manager/cert-manager/releases/download/v1.13.0/cert-manager.yaml

# Wait for cert-manager to be ready
echo "⏳ Waiting for cert-manager pods to be ready..."
kubectl wait --for=condition=ready pod \
  -l app.kubernetes.io/instance=cert-manager \
  -n cert-manager \
  --timeout=300s

echo "✅ cert-manager installed successfully"

# Create ClusterIssuers
echo "🌐 Creating Let's Encrypt ClusterIssuers..."

# Apply staging issuer first (for testing)
kubectl apply -f cluster-issuer-staging.yaml

# Apply production issuer
kubectl apply -f cluster-issuer-prod.yaml

echo "✅ ClusterIssuers created:"
kubectl get clusterissuers

echo ""
echo "🎉 Setup complete!"
echo ""
echo "Next steps:"
echo "1. Update your Helm values to enable ingress with TLS"
echo "2. Deploy/upgrade your application"
echo "3. cert-manager will automatically request and manage SSL certificates"
echo ""
echo "Check certificate status with:"
echo "  kubectl get certificate -A"
echo "  kubectl describe certificate <cert-name> -n <namespace>"
