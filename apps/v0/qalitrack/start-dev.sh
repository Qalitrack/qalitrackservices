#!/bin/bash

echo "Starting QaliTrack Services (Development)"
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

# Start User Service (Development)
echo "Starting User Service (Development)..."
cd services/user-service/dev
docker compose up -d

echo ""
echo "✅ QaliTrack Development services started successfully!"
echo ""
echo "🌐 Access URLs:"
echo "   - User Service API: http://qalitrack.localhost/api/users"
echo "   - Auth API: http://qalitrack.localhost/api/auth"
echo "   - Health Check: http://qalitrack.localhost/health"
echo "   - Traefik Dashboard: http://qalitrack.localhost/traefik"
echo "   - RabbitMQ Management: http://qalitrack.localhost/rabbitmq-dev"
echo ""
echo "📊 To check status:"
echo "   - Traefik: docker compose ps"
echo "   - User Service: cd services/user-service/dev && docker compose ps"
echo "📋 To view logs: docker compose logs -f [service-name]"