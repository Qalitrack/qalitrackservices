# NGINX Ingress Controller

NGINX Ingress Controller for SSL/TLS termination and domain routing to your Qalitrack Gateway Service.

## Architecture

```
Internet (HTTPS port 443)
   ↓
NGINX Ingress Controller (SSL termination)
   ↓ (Plain HTTP)
Your Gateway Service (JWT auth + routing)
   ↓
Microservices (user, masterdata, transaction, backup)
```

## How It Works with Your Custom Gateway

**NGINX Ingress** handles:
- SSL/TLS termination (HTTPS certificates)
- Domain routing (`qalibrated.co.ke` → Gateway Service)
- Load balancing at network layer
- Standard ports (80/443)

**Your Gateway Service** handles:
- JWT authentication
- API routing to microservices
- Business logic
- Request validation

**They complement each other - NO conflict!**

## Installation

### Quick Install

```bash
cd kubernetes/ingress-nginx
./install.sh
```

### Manual Installation

```bash
# Add Helm repository
helm repo add ingress-nginx https://kubernetes.github.io/ingress-nginx
helm repo update

# Install
helm upgrade --install ingress-nginx ingress-nginx/ingress-nginx \
  --namespace ingress-nginx \
  --create-namespace \
  --values values-ingress.yaml \
  --wait
```

## Configuration

### Get External IP

```bash
kubectl get svc -n ingress-nginx ingress-nginx-controller
```

Output:
```
NAME                       TYPE           EXTERNAL-IP      PORT(S)
ingress-nginx-controller   LoadBalancer   <PENDING>        80:31234/TCP,443:31567/TCP
```

Wait for `EXTERNAL-IP` to change from `<PENDING>` to an actual IP address.

### Point DNS to Ingress

Once you have the external IP, update your DNS:

```
qalibrated.co.ke          A    <EXTERNAL-IP>
staging.qalibrated.co.ke  A    <EXTERNAL-IP>
dev.qalibrated.co.ke      A    <EXTERNAL-IP>
```

## Enable Ingress for Qalitrack

### Option 1: With Ingress (HTTPS on port 443)

Edit `kubernetes/helm-charts/qalitrack-platform/values.yaml`:

```yaml
# Change Gateway Service to ClusterIP (internal only)
gateway:
  service:
    type: ClusterIP  # Change from LoadBalancer
    port: 7000

# Enable Ingress
ingress:
  enabled: true  # Change from false
  className: "nginx"
  annotations:
    cert-manager.io/cluster-issuer: "letsencrypt-prod"  # For SSL
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

Deploy:
```bash
cd kubernetes/helm-charts/qalitrack-platform
helm upgrade qalitrack . --namespace qalitrack-prod --values values.yaml
```

Access: `https://qalibrated.co.ke/qalitrack/api`

### Option 2: Without Ingress (Direct access)

Keep current configuration:

```yaml
gateway:
  service:
    type: LoadBalancer  # Direct internet access
    port: 7000

ingress:
  enabled: false  # No Ingress
```

Access: `http://qalibrated.co.ke:7000/qalitrack/api`

## Verify Ingress

### Check Ingress Resources

```bash
# List all Ingress resources
kubectl get ingress -A

# Describe Ingress
kubectl describe ingress qalitrack-ingress -n qalitrack-prod
```

### Check Controller Logs

```bash
kubectl logs -n ingress-nginx deployment/ingress-nginx-controller -f
```

### Test Ingress

```bash
# Get Ingress IP
INGRESS_IP=$(kubectl get svc ingress-nginx-controller -n ingress-nginx \
  -o jsonpath='{.status.loadBalancer.ingress[0].ip}')

# Test HTTP (should redirect to HTTPS if configured)
curl -H "Host: qalibrated.co.ke" http://$INGRESS_IP/qalitrack/api/health

# Test HTTPS (after cert-manager issues certificate)
curl https://qalibrated.co.ke/qalitrack/api/health
```

## SSL/TLS with cert-manager

NGINX Ingress works seamlessly with cert-manager for automatic SSL certificates.

### Prerequisites

1. Install cert-manager first:
   ```bash
   cd ../cert-manager
   ./install.sh
   ```

2. Ensure DNS is pointing to Ingress IP

### Enable SSL

Add annotation to Ingress in `values.yaml`:

```yaml
ingress:
  annotations:
    cert-manager.io/cluster-issuer: "letsencrypt-prod"
```

cert-manager will automatically:
1. Request certificate from Let's Encrypt
2. Complete HTTP-01 challenge via Ingress
3. Store certificate in Kubernetes Secret
4. Auto-renew before expiration

## Monitoring

### Prometheus Metrics

NGINX Ingress exports Prometheus metrics on port 10254.

Create ServiceMonitor:

```yaml
apiVersion: monitoring.coreos.com/v1
kind: ServiceMonitor
metadata:
  name: ingress-nginx
  namespace: qalitrack-monitoring
spec:
  selector:
    matchLabels:
      app.kubernetes.io/name: ingress-nginx
  namespaceSelector:
    matchNames:
    - ingress-nginx
  endpoints:
  - port: metrics
    interval: 30s
```

Apply:
```bash
kubectl apply -f servicemonitor-ingress.yaml
```

### Key Metrics

- `nginx_ingress_controller_requests` - Total requests
- `nginx_ingress_controller_request_duration_seconds` - Latency
- `nginx_ingress_controller_response_size` - Response sizes
- `nginx_ingress_controller_ssl_expire_time_seconds` - SSL cert expiry

### Grafana Dashboard

Import dashboard ID `9614` in Grafana for NGINX Ingress metrics.

## Common Ingress Patterns

### Multiple Domains

```yaml
hosts:
  - host: qalibrated.co.ke
    paths:
      - path: /qalitrack/api
        service: gateway-service
        port: 7000
  - host: admin.qalibrated.co.ke
    paths:
      - path: /
        service: admin-frontend
        port: 3000
```

### Path-based Routing

```yaml
hosts:
  - host: qalibrated.co.ke
    paths:
      - path: /api
        service: gateway-service
        port: 7000
      - path: /admin
        service: admin-service
        port: 8080
      - path: /
        service: frontend
        port: 80
```

### Custom Annotations

```yaml
ingress:
  annotations:
    # Rate limiting
    nginx.ingress.kubernetes.io/limit-rps: "100"

    # Request timeout
    nginx.ingress.kubernetes.io/proxy-read-timeout: "120"

    # CORS
    nginx.ingress.kubernetes.io/enable-cors: "true"
    nginx.ingress.kubernetes.io/cors-allow-origin: "*"

    # Whitelist IPs
    nginx.ingress.kubernetes.io/whitelist-source-range: "10.0.0.0/8,192.168.0.0/16"

    # Rewrite target
    nginx.ingress.kubernetes.io/rewrite-target: /$2
```

## Troubleshooting

### Ingress Not Getting External IP

```bash
# Check service status
kubectl get svc -n ingress-nginx

# Check events
kubectl get events -n ingress-nginx --sort-by='.lastTimestamp'

# Check controller logs
kubectl logs -n ingress-nginx deployment/ingress-nginx-controller
```

**Common causes:**
- LoadBalancer not supported (use NodePort or port-forward)
- Cloud provider quota exceeded
- Firewall rules blocking

### 404 Not Found

```bash
# Check Ingress rules
kubectl describe ingress qalitrack-ingress -n qalitrack-prod

# Verify backend service exists
kubectl get svc gateway-service -n qalitrack-prod

# Check Ingress logs
kubectl logs -n ingress-nginx deployment/ingress-nginx-controller | grep qalitrack
```

### SSL Certificate Issues

```bash
# Check certificate status
kubectl get certificate -n qalitrack-prod

# Describe certificate
kubectl describe certificate qalitrack-tls -n qalitrack-prod

# Check cert-manager logs
kubectl logs -n cert-manager deployment/cert-manager

# Check Ingress annotations
kubectl get ingress qalitrack-ingress -n qalitrack-prod -o yaml
```

### 502 Bad Gateway

```bash
# Check if backend pods are running
kubectl get pods -n qalitrack-prod -l app.kubernetes.io/name=gateway-service

# Check pod logs
kubectl logs -n qalitrack-prod -l app.kubernetes.io/name=gateway-service

# Check service endpoints
kubectl get endpoints gateway-service -n qalitrack-prod

# Test backend directly
kubectl port-forward -n qalitrack-prod svc/gateway-service 7000:7000
curl http://localhost:7000/health
```

### Connection Timeout

```bash
# Increase timeouts in Ingress annotation
kubectl annotate ingress qalitrack-ingress -n qalitrack-prod \
  nginx.ingress.kubernetes.io/proxy-read-timeout="120" \
  nginx.ingress.kubernetes.io/proxy-send-timeout="120"

# Or edit values.yaml:
controller:
  config:
    proxy-read-timeout: "120"
    proxy-send-timeout: "120"
```

## Performance Tuning

### Increase Replicas

```yaml
controller:
  replicaCount: 3  # Scale horizontally
```

### Increase Resources

```yaml
controller:
  resources:
    requests:
      cpu: 200m
      memory: 256Mi
    limits:
      cpu: 1000m
      memory: 1Gi
```

### Enable Connection Pooling

```yaml
controller:
  config:
    upstream-keepalive-connections: "100"
    upstream-keepalive-requests: "1000"
    upstream-keepalive-timeout: "60"
```

## Security

### Rate Limiting

```yaml
ingress:
  annotations:
    nginx.ingress.kubernetes.io/limit-rps: "100"
    nginx.ingress.kubernetes.io/limit-connections: "10"
```

### IP Whitelisting

```yaml
ingress:
  annotations:
    nginx.ingress.kubernetes.io/whitelist-source-range: "10.0.0.0/8,192.168.0.0/16"
```

### ModSecurity WAF

```yaml
controller:
  config:
    enable-modsecurity: "true"
    enable-owasp-modsecurity-crs: "true"
```

## Cleanup

```bash
# Uninstall Ingress Controller
helm uninstall ingress-nginx -n ingress-nginx

# Delete namespace
kubectl delete namespace ingress-nginx
```

## Best Practices

1. **Use cert-manager** for automatic SSL certificates
2. **Enable metrics** for monitoring
3. **Set resource limits** to prevent resource exhaustion
4. **Use rate limiting** to prevent abuse
5. **Configure timeouts** based on your application needs
6. **Test failover** by deleting controller pods
7. **Monitor logs** regularly for errors
8. **Keep updated** to latest stable version

## Resources

- [NGINX Ingress Documentation](https://kubernetes.github.io/ingress-nginx/)
- [Ingress Annotations](https://kubernetes.github.io/ingress-nginx/user-guide/nginx-configuration/annotations/)
- [cert-manager Integration](https://cert-manager.io/docs/usage/ingress/)

---

**Last Updated:** March 2026
**Version:** 1.0.0
