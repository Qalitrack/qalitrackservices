#!/bin/bash

# Start script for Kungu Cement
set -e

echo "🚀 Starting Kungu Cement QaliTrack Services..."

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

print_status "Starting 10 services for Kungu Cement..."

# Set client configuration
export CLIENT_CODE=kungu

# Start services
if docker compose -f docker-compose.kungu.yml up -d --build; then
    print_success "All services started successfully!"
    
    echo ""
    print_status "Service URLs:"
    echo "  🌐 API Gateway:           http://localhost:7000"
    echo "  📚 API Documentation:    http://localhost:8000"
    echo "  💓 Health Check:         http://localhost:7000/health"
    echo "  🔧 Gateway: http://localhost:7000"
    echo "  🔧 User Service: http://localhost:7001"
    echo "  🔧 Organization Service: http://localhost:7002"
    echo "  🔧 Vehicle Service: http://localhost:7003"
    echo "  🔧 Driver Service: http://localhost:7004"
    echo "  🔧 Product Service: http://localhost:7005"
    echo "  🔧 Weighbridge Service: http://localhost:7007"
    echo "  🔧 Customer Service: http://localhost:7008"
    echo "  🔧 Weight Data Service: http://localhost:7012"
    echo "  🔧 Transaction Service: http://localhost:7015"

    
    echo ""
    print_status "Enabled Services (10):"
    echo "  ✅ gateway"
    echo "  ✅ user-service"
    echo "  ✅ organization-service"
    echo "  ✅ vehicle-service"
    echo "  ✅ driver-service"
    echo "  ✅ product-service"
    echo "  ✅ weighbridge-service"
    echo "  ✅ customer-service"
    echo "  ✅ weight-data-service"
    echo "  ✅ transaction-service"

    
    echo ""
    print_status "Useful commands:"
    echo "  📋 View logs:            docker compose -f docker-compose.kungu.yml logs -f"
    echo "  🔄 Restart service:      docker compose -f docker-compose.kungu.yml restart [service-name]"
    echo "  🛑 Stop all services:    docker compose -f docker-compose.kungu.yml down"
    echo "  🗑️  Remove all data:      docker compose -f docker-compose.kungu.yml down -v"
    
else
    print_error "Failed to start services"
    exit 1
fi