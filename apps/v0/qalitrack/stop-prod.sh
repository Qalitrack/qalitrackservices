#!/bin/bash

echo "Stopping QaliTrack Production Services"
echo "======================================"

# Stop User Service (Production)
echo "Stopping User Service (Production)..."
cd services/user-service/prod
docker compose down

# Stop QaliTrack Traefik
echo "Stopping QaliTrack Traefik..."
cd ../../../
docker compose down

echo ""
echo "✅ QaliTrack Production services stopped successfully!"