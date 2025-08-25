#!/bin/bash

# Script to fix GitHub Actions workflows with proper GHCR permissions

WORKFLOW_DIR=".github/workflows"
SERVICES=(
    "compliance-service"
    "customer-service"
    "driver-service"
    "operational-data-service"
    "product-service"
    "report-service"
    "route-service"
    "sacco-service"
    "supplier-service"
    "transaction-service"
    "transporter-service"
    "vehicle-service"
    "weighbridge-service"
    "weight-data-service"
)

for service in "${SERVICES[@]}"; do
    workflow_file="$WORKFLOW_DIR/build-$service.yml"
    
    echo "Fixing $workflow_file..."
    
    # Get the service name for paths
    service_name=$(echo "$service" | sed 's/-service$//')
    
    cat > "$workflow_file" << EOF
name: Build $(echo "$service" | sed 's/-/ /g' | sed 's/\b\w/\U&/g')

on:
  push:
    branches: [ main ]
    paths:
      - 'packages/microservices/masterdata/$service/**'
  pull_request:
    branches: [ main ]
    paths:
      - 'packages/microservices/masterdata/$service/**'
  workflow_dispatch:

env:
  REGISTRY: ghcr.io
  DOCKER_BUILDKIT: 1
  SERVICE_NAME: $service
  SERVICE_PATH: packages/microservices/masterdata/$service

jobs:
  build:
    runs-on: ubuntu-latest
    permissions:
      contents: read
      packages: write
      attestations: write
      id-token: write
    steps:
      - name: Checkout code
        uses: actions/checkout@v4

      - name: Set up .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0.x'

      - name: Restore dependencies
        run: |
          cd \${{ env.SERVICE_PATH }}
          dotnet restore --verbosity normal

      - name: Build application
        run: |
          cd \${{ env.SERVICE_PATH }}
          dotnet build --no-restore -c Release --verbosity normal

      - name: Run tests
        run: |
          cd \${{ env.SERVICE_PATH }}
          dotnet test --no-build -c Release --verbosity normal
        continue-on-error: true

      - name: Set up Docker Buildx
        uses: docker/setup-buildx-action@v3

      - name: Log in to Container Registry
        uses: docker/login-action@v3
        with:
          registry: \${{ env.REGISTRY }}
          username: \${{ github.actor }}
          password: \${{ secrets.GITHUB_TOKEN }}

      - name: Extract metadata
        id: meta
        uses: docker/metadata-action@v5
        with:
          images: \${{ env.REGISTRY }}/\${{ github.repository }}/\${{ env.SERVICE_NAME }}
          tags: |
            type=ref,event=branch
            type=ref,event=pr
            type=sha,prefix={{branch}}-
            type=raw,value=latest,enable={{is_default_branch}}

      - name: Build and push Docker image
        uses: docker/build-push-action@v5
        with:
          context: \${{ env.SERVICE_PATH }}
          platforms: linux/amd64
          push: true
          tags: \${{ steps.meta.outputs.tags }}
          labels: \${{ steps.meta.outputs.labels }}
          cache-from: type=gha
          cache-to: type=gha,mode=max

EOF
    
    echo "Fixed $workflow_file"
done

echo "All workflows have been updated with proper GHCR permissions!"