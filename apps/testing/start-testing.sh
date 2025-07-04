#!/bin/bash

# Start script for QaliTrack Testing Environment
set -e

echo "🚀 Starting QaliTrack Testing Environment QaliTrack Services..."

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

print_status() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

print_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Check if Docker is running
if ! docker info > /dev/null 2>&1; then
    print_error "Docker is not running. Please start Docker first."
    exit 1
fi

print_status "Starting 3 services for QaliTrack Testing Environment..."

# Set client configuration
export CLIENT_CODE=testing

# Start services
if docker compose -f docker-compose.testing.yml up -d --build; then
    print_success "All services started successfully!"
    
    echo ""
    print_status "Service URLs:"
    echo "  🌐 API Gateway:           http://localhost:7000"
    echo "  📚 API Documentation:    http://localhost:8000"
    echo "  💓 Health Check:         http://localhost:7000/health"
    echo "  🔧 Gateway: http://localhost:7000"
    echo "  🔧 User Service: http://localhost:7001"
    echo "  🔧 Service Discovery: http://localhost:7019"

    
    echo ""
    print_status "Enabled Services (3):"
    echo "  ✅ gateway"
    echo "  ✅ user-service"
    echo "  ✅ service-discovery"

    
    echo ""
    print_status "Useful commands:"
    echo "  📋 View logs:            docker compose -f docker-compose.testing.yml logs -f"
    echo "  🔄 Restart service:      docker compose -f docker-compose.testing.yml restart [service-name]"
    echo "  🛑 Stop all services:    docker compose -f docker-compose.testing.yml down"
    echo "  🗑️  Remove all data:      docker compose -f docker-compose.testing.yml down -v"
    
else
    print_error "Failed to start services"
    exit 1
fi