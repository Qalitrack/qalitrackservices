# Secrets Management

Secure generation and management of Qalitrack application secrets.

## Quick Start

```bash
cd kubernetes/secrets

# 1. Generate secrets
./generate-secrets.sh

# 2. Review and update secrets.env (especially EMAIL settings)
nano secrets.env

# 3. Create Kubernetes secrets
./create-k8s-secrets.sh

# 4. Backup secrets.env to secure location
# Then delete local copy or keep in password manager
```

## What Gets Generated

The `generate-secrets.sh` script creates:

1. **JWT_SECRET_KEY** - 64-character random string for JWT signing
2. **USER_SERVICE_DB_PASSWORD** - PostgreSQL password for user service database
3. **MASTER_DATA_DB_PASSWORD** - PostgreSQL password for masterdata database
4. **TRANSACTION_DB_PASSWORD** - PostgreSQL password for transaction database
5. **USER_SERVICE_REDIS_PASSWORD** - Redis password for caching
6. **RABBITMQ_PASSWORD** - RabbitMQ password
7. **RABBITMQ_ERLANG_COOKIE** - RabbitMQ Erlang cookie for clustering
8. **EMAIL_* settings** - SMTP configuration (needs manual update)

## Generated Files

### secrets.env

```bash
# Example format (DO NOT commit!)
JWT_SECRET_KEY=abc123...xyz (64 chars)
USER_SERVICE_DB_PASSWORD=def456...uvw (32 chars)
# ... etc
```

**⚠️ NEVER commit this file to Git!**

## Manual Steps

### Update Email Configuration

Edit `secrets.env` after generation:

```bash
# For Gmail
EMAIL_SMTP_HOST=smtp.gmail.com
EMAIL_SMTP_PORT=587
EMAIL_SMTP_USERNAME=your-email@gmail.com
EMAIL_SMTP_PASSWORD=your-app-password  # Get from Google Account settings
EMAIL_FROM_EMAIL=noreply@qalibrated.co.ke
EMAIL_FROM_NAME=Qalitrack System
EMAIL_ENABLE_SSL=true
```

**Gmail App Password:**
1. Go to Google Account → Security
2. Enable 2-Factor Authentication
3. Generate App Password for "Mail"
4. Use that password in EMAIL_SMTP_PASSWORD

### For Other Email Providers

```bash
# Office 365
EMAIL_SMTP_HOST=smtp.office365.com
EMAIL_SMTP_PORT=587

# SendGrid
EMAIL_SMTP_HOST=smtp.sendgrid.net
EMAIL_SMTP_PORT=587
EMAIL_SMTP_USERNAME=apikey
EMAIL_SMTP_PASSWORD=<your-sendgrid-api-key>

# AWS SES
EMAIL_SMTP_HOST=email-smtp.us-east-1.amazonaws.com
EMAIL_SMTP_PORT=587
EMAIL_SMTP_USERNAME=<your-smtp-username>
EMAIL_SMTP_PASSWORD=<your-smtp-password>
```

## Applying Secrets to Kubernetes

### Production

```bash
./create-k8s-secrets.sh
# Creates secret in qalitrack-prod namespace
```

### Staging

```bash
NAMESPACE=qalitrack-staging ./create-k8s-secrets.sh
```

### Development

```bash
NAMESPACE=qalitrack-dev ./create-k8s-secrets.sh
```

## Verify Secrets

```bash
# List secrets
kubectl get secrets -n qalitrack-prod

# Describe secret (shows keys, not values)
kubectl describe secret qalitrack-secrets -n qalitrack-prod

# View secret values (base64 encoded)
kubectl get secret qalitrack-secrets -n qalitrack-prod -o yaml

# Decode a specific value
kubectl get secret qalitrack-secrets -n qalitrack-prod \
  -o jsonpath='{.data.JWT_SECRET_KEY}' | base64 -d
```

## Update Secrets

### Update Single Value

```bash
# Update JWT secret
kubectl create secret generic qalitrack-secrets \
  --from-literal=JWT_SECRET_KEY="new-secret-here" \
  --dry-run=client -o yaml | kubectl apply -f -
```

### Replace All Secrets

```bash
# Delete existing
kubectl delete secret qalitrack-secrets -n qalitrack-prod

# Recreate from updated secrets.env
./create-k8s-secrets.sh
```

### Rotate Secrets

```bash
# 1. Generate new secrets
./generate-secrets.sh

# This creates a NEW secrets.env, backing up the old one
mv secrets.env secrets.env.old
./generate-secrets.sh

# 2. Apply new secrets
./create-k8s-secrets.sh

# 3. Restart all services to pick up new secrets
kubectl rollout restart deployment -n qalitrack-prod

# 4. Verify services are healthy
kubectl get pods -n qalitrack-prod
```

## Security Best Practices

### 1. Never Commit Secrets

The `.gitignore` in this directory prevents committing:
- `secrets.env`
- `*.env`
- `*.key`
- `*.pem`

Always verify before committing:
```bash
git status
# Should NOT show secrets.env
```

### 2. Backup Secrets Securely

Store `secrets.env` in:
- **Password Manager** (1Password, LastPass, Bitwarden)
- **HashiCorp Vault**
- **AWS Secrets Manager**
- **Azure Key Vault**
- **Google Secret Manager**

**DO NOT** store in:
- Git repository
- Slack/Discord
- Email
- Unencrypted cloud storage

### 3. Limit Access

```bash
# Only specific service accounts can read secrets
kubectl create rolebinding secret-reader \
  --clusterrole=view \
  --serviceaccount=qalitrack-prod:gateway-service \
  -n qalitrack-prod
```

### 4. Encrypt at Rest

Enable encryption at rest in Kubernetes:

```yaml
# kube-apiserver configuration
apiVersion: apiserver.config.k8s.io/v1
kind: EncryptionConfiguration
resources:
  - resources:
    - secrets
    providers:
    - aescbc:
        keys:
        - name: key1
          secret: <base64-encoded-32-byte-key>
    - identity: {}
```

### 5. Use External Secrets (Advanced)

For production, consider:

#### External Secrets Operator

```bash
# Install External Secrets Operator
helm repo add external-secrets https://charts.external-secrets.io
helm install external-secrets external-secrets/external-secrets -n external-secrets-system --create-namespace

# Create SecretStore pointing to AWS Secrets Manager
kubectl apply -f - <<EOF
apiVersion: external-secrets.io/v1beta1
kind: SecretStore
metadata:
  name: aws-secrets
  namespace: qalitrack-prod
spec:
  provider:
    aws:
      service: SecretsManager
      region: us-east-1
      auth:
        jwt:
          serviceAccountRef:
            name: external-secrets
EOF

# Create ExternalSecret
kubectl apply -f - <<EOF
apiVersion: external-secrets.io/v1beta1
kind: ExternalSecret
metadata:
  name: qalitrack-secrets
  namespace: qalitrack-prod
spec:
  refreshInterval: 1h
  secretStoreRef:
    name: aws-secrets
    kind: SecretStore
  target:
    name: qalitrack-secrets
  data:
  - secretKey: JWT_SECRET_KEY
    remoteRef:
      key: qalitrack/jwt-secret
EOF
```

#### Sealed Secrets

```bash
# Install Sealed Secrets controller
kubectl apply -f https://github.com/bitnami-labs/sealed-secrets/releases/download/v0.24.0/controller.yaml

# Install kubeseal CLI
wget https://github.com/bitnami-labs/sealed-secrets/releases/download/v0.24.0/kubeseal-linux-amd64 -O kubeseal
sudo install -m 755 kubeseal /usr/local/bin/kubeseal

# Seal your secret
kubectl create secret generic qalitrack-secrets \
  --from-env-file=secrets.env \
  --dry-run=client -o yaml | \
  kubeseal -o yaml > sealed-secret.yaml

# Now you can commit sealed-secret.yaml safely to Git!
kubectl apply -f sealed-secret.yaml
```

## Troubleshooting

### Secret Not Found

```bash
# Check if secret exists
kubectl get secret qalitrack-secrets -n qalitrack-prod

# If not, create it
./create-k8s-secrets.sh
```

### Pod Can't Read Secret

```bash
# Check pod events
kubectl describe pod <pod-name> -n qalitrack-prod

# Common causes:
# 1. Secret doesn't exist in same namespace
# 2. Secret name mismatch in deployment
# 3. RBAC permissions issue
```

### Invalid Secret Values

```bash
# Check what's in the secret
kubectl get secret qalitrack-secrets -n qalitrack-prod -o json | jq '.data | map_values(@base64d)'

# Verify each value
kubectl get secret qalitrack-secrets -n qalitrack-prod \
  -o jsonpath='{.data.JWT_SECRET_KEY}' | base64 -d | wc -c
# Should output: 64 (for JWT secret)
```

### Services Not Using New Secrets

After updating secrets, restart deployments:

```bash
# Restart all services
kubectl rollout restart deployment -n qalitrack-prod

# Restart specific service
kubectl rollout restart deployment/gateway-service -n qalitrack-prod

# Wait for rollout
kubectl rollout status deployment/gateway-service -n qalitrack-prod
```

## Secret Lifecycle

### Development

```bash
# Use simpler secrets for dev
JWT_SECRET_KEY=dev-secret-not-for-production
USER_SERVICE_DB_PASSWORD=postgres
```

### Staging

```bash
# Use generated secrets, but different from production
./generate-secrets.sh
NAMESPACE=qalitrack-staging ./create-k8s-secrets.sh
```

### Production

```bash
# Use strong generated secrets
./generate-secrets.sh
# Store in vault immediately
./create-k8s-secrets.sh
# Delete local copy after storing in vault
shred -u secrets.env  # Securely delete
```

## Compliance

### Audit Secret Access

```bash
# Enable audit logging in Kubernetes
# Check who accessed secrets
kubectl get events -n qalitrack-prod --field-selector involvedObject.kind=Secret

# Check pod service accounts
kubectl get pods -n qalitrack-prod -o custom-columns=NAME:.metadata.name,SA:.spec.serviceAccountName
```

### Secret Rotation Policy

Recommended rotation schedule:
- **JWT Secret:** Every 90 days
- **Database Passwords:** Every 180 days
- **API Keys:** Every 90 days
- **Email Passwords:** When employees leave
- **After Security Incident:** Immediately

## Resources

- [Kubernetes Secrets](https://kubernetes.io/docs/concepts/configuration/secret/)
- [External Secrets Operator](https://external-secrets.io/)
- [Sealed Secrets](https://github.com/bitnami-labs/sealed-secrets)
- [Vault by HashiCorp](https://www.vaultproject.io/)

---

**Last Updated:** March 2026
**Version:** 1.0.0
**Security Level:** Production-ready with best practices
