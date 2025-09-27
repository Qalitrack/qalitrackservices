#!/bin/bash

echo "Starting QaliTrack Services (Production)"
echo "========================================"

# Create qalitrack-proxy network if it doesn't exist
if ! docker network ls | grep -q qalitrack-proxy; then
    echo "Creating qalitrack-proxy network..."
    docker network create qalitrack-proxy
fi

# Start QaliTrack Traefik first
echo "Starting QaliTrack Traefik..."
docker compose up -d

# Wait for Traefik to be ready
echo "Waiting for Traefik to be ready..."
sleep 5

# Start User Service (Production)
echo "Starting User Service (Production)..."
cd services/user-service/prod
docker compose up -d

echo ""
echo "✅ QaliTrack Production services started successfully!"
echo ""
echo "🌐 Access URLs:"
echo "   - User Service API: https://qalitrack.cseco.co.ke/api/users"
echo "   - Auth API: https://qalitrack.cseco.co.ke/api/auth"
echo "   - Health Check: https://qalitrack.cseco.co.ke/health"
echo "   - Traefik Dashboard: https://qalitrack.cseco.co.ke/traefik"
echo ""
echo "📊 To check status:"
echo "   - Traefik: docker compose ps"
echo "   - User Service: cd services/user-service/prod && docker compose ps"
echo "📋 To view logs: docker compose logs -f [service-name]"