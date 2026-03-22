# cert-manager - Automatic SSL/TLS Certificate Management

Automatic HTTPS certificates for your Qalitrack deployment using Let's Encrypt.

## What is cert-manager?

cert-manager automatically provisions and manages TLS certificates in Kubernetes. It integrates with Let's Encrypt to provide free, automatically-renewed SSL/TLS certificates.

## Features

- **Automatic certificate issuance** from Let's Encrypt
- **Automatic renewal** before expiration (no manual intervention)
- **HTTP-01 challenge** validation via Ingress
- **Staging and production** issuers for testing

## Installation

### Quick Install

```bash
cd kubernetes/cert-manager
./install.sh
```

### Manual Installation

```bash
# Install cert-manager
kubectl apply -f https://github.com/cert-manager/cert-manager/releases/download/v1.13.0/cert-manager.yaml

# Wait for cert-manager to be ready
kubectl wait --for=condition=ready pod \
  -l app.kubernetes.io/instance=cert-manager \
  -n cert-manager \
  --timeout=300s

# Create ClusterIssuers
kubectl apply -f cluster-issuer-staging.yaml
kubectl apply -f cluster-issuer-prod.yaml
```

## Configuration

### ClusterIssuers

Two ClusterIssuers are available:

#### 1. Staging (for testing)
```yaml
# cluster-issuer-staging.yaml
metadata:
  name: letsencrypt-staging
spec:
  acme:
    server: https://acme-staging-v02.api.letsencrypt.org/directory
```

**Use this first** to test your setup. Staging has higher rate limits and won't count against production limits.

#### 2. Production (for real certificates)
```yaml
# cluster-issuer-prod.yaml
metadata:
  name: letsencrypt-prod
spec:
  acme:
    server: https://acme-v02.api.letsencrypt.org/directory
```

**Switch to this** after testing with staging.

## Enable HTTPS for Qalitrack

### Step 1: Enable Ingress in Helm

Edit `kubernetes/helm-charts/qalitrack-platform/values.yaml`:

```yaml
ingress:
  enabled: true
  className: "nginx"
  annotations:
    cert-manager.io/cluster-issuer: "letsencrypt-prod"  # or letsencrypt-staging for testing
    nginx.ingress.kubernetes.io/ssl-redirect: "true"
  hosts:
    - host: qalibrated.co.ke
      paths:
        - path: /qalitrack/api
          pathType: Prefix
          service: gateway-service
          port: 7000
  tls:
    - secretName: qalitrack-tls
      hosts:
        - qalibrated.co.ke
```

### Step 2: Install/Upgrade Helm Chart

```bash
cd kubernetes/helm-charts/qalitrack-platform

helm upgrade --install qalitrack . \
  --namespace qalitrack-prod \
  --values values.yaml
```

### Step 3: Verify Certificate

```bash
# Check certificate status
kubectl get certificate -n qalitrack-prod

# Check detailed status
kubectl describe certificate qalitrack-tls -n qalitrack-prod

# Check cert-manager logs if there are issues
kubectl logs -n cert-manager deployment/cert-manager
```

## Testing with Staging

**Always test with staging first** to avoid hitting Let's Encrypt rate limits:

```yaml
annotations:
  cert-manager.io/cluster-issuer: "letsencrypt-staging"  # Start with staging
```

1. Deploy with staging issuer
2. Verify certificate is issued successfully
3. Delete the certificate: `kubectl delete certificate qalitrack-tls -n qalitrack-prod`
4. Change to `letsencrypt-prod`
5. Redeploy

## How It Works

```
1. Ingress created with cert-manager annotation
   ↓
2. cert-manager sees the annotation
   ↓
3. cert-manager creates a Certificate resource
   ↓
4. Let's Encrypt issues HTTP-01 challenge
   ↓
5. cert-manager creates temporary Ingress for validation
   ↓
6. Let's Encrypt validates domain ownership
   ↓
7. Certificate issued and stored as Kubernetes Secret
   ↓
8. Ingress uses the certificate for TLS
   ↓
9. cert-manager auto-renews before expiration (every 60 days)
```

## Troubleshooting

### Certificate Not Issued

```bash
# Check certificate status
kubectl describe certificate qalitrack-tls -n qalitrack-prod

# Check challenge status
kubectl get challenge -A

# Check cert-manager logs
kubectl logs -n cert-manager deployment/cert-manager -f
```

### Common Issues

1. **DNS not pointing to cluster**
   - Ensure `qalibrated.co.ke` resolves to your Ingress IP
   - Check: `nslookup qalibrated.co.ke`

2. **Ingress Controller not installed**
   - Install NGINX Ingress Controller first
   - See: `kubernetes/helm-charts/DEPLOYMENT-GUIDE.md`

3. **Rate limit exceeded**
   - Use staging issuer for testing
   - Production has limit of 50 certs/week per domain

4. **HTTP-01 challenge fails**
   - Ensure port 80 is accessible
   - Check firewall rules
   - Verify Ingress is working

### Check Rate Limits

Let's Encrypt rate limits:
- **Production:** 50 certificates per registered domain per week
- **Staging:** Much higher limits, but certificates are not trusted

## Certificate Renewal

cert-manager automatically renews certificates **30 days before expiration**.

Check renewal status:
```bash
# View all certificates and expiration dates
kubectl get certificate -A

# Force renewal (if needed)
kubectl delete secret qalitrack-tls -n qalitrack-prod
# cert-manager will automatically recreate it
```

## Monitoring

### Certificate Status

```bash
# List all certificates
kubectl get certificate -A

# Watch certificate events
kubectl get events -n qalitrack-prod --field-selector involvedObject.kind=Certificate

# Check secret creation
kubectl get secret qalitrack-tls -n qalitrack-prod
```

### Prometheus Metrics (if monitoring enabled)

cert-manager exports Prometheus metrics on port 9402:
- `certmanager_certificate_expiration_timestamp_seconds`
- `certmanager_certificate_ready_status`

## Cleanup

```bash
# Delete ClusterIssuers
kubectl delete -f cluster-issuer-staging.yaml
kubectl delete -f cluster-issuer-prod.yaml

# Uninstall cert-manager
kubectl delete -f https://github.com/cert-manager/cert-manager/releases/download/v1.13.0/cert-manager.yaml
```

## Additional Resources

- [cert-manager Documentation](https://cert-manager.io/docs/)
- [Let's Encrypt Rate Limits](https://letsencrypt.org/docs/rate-limits/)
- [HTTP-01 Challenge](https://letsencrypt.org/docs/challenge-types/#http-01-challenge)

---

**Last Updated:** March 2026
**Version:** 1.0.0
