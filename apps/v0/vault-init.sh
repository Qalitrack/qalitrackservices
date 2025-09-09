#!/bin/bash

# Wait for Vault to be ready
echo "Waiting for Vault to be ready..."
while ! curl -s http://vault:8200/v1/sys/health > /dev/null; do
  sleep 2
done

export VAULT_ADDR=http://vault:8200
export VAULT_TOKEN=myroot

echo "Vault is ready, initializing secrets and policies..."

# Enable AppRole authentication method
vault auth enable approle

# Create policies for different services
cat << EOF | vault policy write qalitrack-services -
path "secret/data/database/*" {
  capabilities = ["read"]
}
path "secret/data/email/*" {
  capabilities = ["read"]
}
path "secret/data/services/*" {
  capabilities = ["read"]
}
EOF

# Create AppRole for services
vault write auth/approle/role/qalitrack-services \
    token_policies="qalitrack-services" \
    token_ttl=1h \
    token_max_ttl=4h

# Get role-id and secret-id
ROLE_ID=$(vault read -field=role_id auth/approle/role/qalitrack-services/role-id)
SECRET_ID=$(vault write -field=secret_id -f auth/approle/role/qalitrack-services/secret-id)

echo "Role ID: $ROLE_ID"
echo "Secret ID: $SECRET_ID"

# Store database secrets
vault kv put secret/database/masterdata \
    connection_string="Host=postgres;Database=masterdata;Username=masterdata;Password=masterdata123;Pooling=true;MinPoolSize=5;MaxPoolSize=100;Include Error Detail=true;Command Timeout=60"

vault kv put secret/database/backup \
    connection_string="Host=postgres;Database=backupservicedb;Username=backupservice;Password=backupservice123;Pooling=true;MinPoolSize=5;MaxPoolSize=100;Include Error Detail=true;Command Timeout=60"

vault kv put secret/database/postgres \
    admin_user="admin" \
    admin_password="password" \
    database="microservices"

# Store email configuration
vault kv put secret/email/smtp \
    from_email="noreply@qalitrack.com" \
    from_name="QaliTrack System" \
    smtp_host="mailpit" \
    smtp_port="1025" \
    smtp_username="" \
    smtp_password="" \
    enable_ssl="false"

# Store service configuration
vault kv put secret/services/rabbitmq \
    default_user="admin" \
    default_password="password"

vault kv put secret/services/minio \
    root_user="admin" \
    root_password="password123"

vault kv put secret/services/zincsearch \
    admin_user="admin" \
    admin_password="password123"

# Store AppRole credentials for services to use
vault kv put secret/auth/approle \
    role_id="$ROLE_ID" \
    secret_id="$SECRET_ID"

echo "Vault initialization complete!"
echo "Services can authenticate using:"
echo "  Role ID: $ROLE_ID"
echo "  Secret ID: $SECRET_ID"