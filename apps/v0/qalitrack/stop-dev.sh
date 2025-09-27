#!/bin/bash

echo "Stopping QaliTrack Development Services"
echo "========================================"

# Stop User Service (Development)
echo "Stopping User Service (Development)..."
cd services/user-service/dev
docker compose down

# Stop QaliTrack Traefik
echo "Stopping QaliTrack Traefik..."
cd ../../../
docker compose down

echo ""
echo "✅ QaliTrack Development services stopped successfully!"