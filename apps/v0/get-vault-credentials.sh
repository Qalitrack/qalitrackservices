#!/bin/bash

# Script to retrieve Vault AppRole credentials for services
# This should be run by services at startup to get their authentication credentials

VAULT_ADDR=${VAULT_ADDR:-"http://vault:8200"}
ROOT_TOKEN=${VAULT_ROOT_TOKEN:-"myroot"}

echo "Retrieving Vault AppRole credentials..."

# Get the role-id and secret-id from Vault using root token
export VAULT_TOKEN="$ROOT_TOKEN"

ROLE_ID=$(vault read -address="$VAULT_ADDR" -field=role_id auth/approle/role/qalitrack-services/role-id 2>/dev/null)
SECRET_ID=$(vault write -address="$VAULT_ADDR" -field=secret_id -f auth/approle/role/qalitrack-services/secret-id 2>/dev/null)

if [ -z "$ROLE_ID" ] || [ -z "$SECRET_ID" ]; then
    echo "Failed to retrieve Vault credentials. Using fallback authentication."
    exit 1
fi

# Export credentials for the service to use
export VAULT_ROLE_ID="$ROLE_ID"
export VAULT_SECRET_ID="$SECRET_ID"

echo "Vault credentials retrieved successfully"
echo "Role ID: $ROLE_ID"
echo "Secret ID: [REDACTED]"

# Execute the main service command
exec "$@"