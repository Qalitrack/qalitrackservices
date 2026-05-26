#!/bin/bash

# Start script for QaliTrack services
set -e

echo "🚀 Starting QaliTrack Microservices..."

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Function to print colored output
print_status() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

print_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Check if Docker is running
if ! docker info > /dev/null 2>&1; then
    print_error "Docker is not running. Please start Docker first."
    exit 1
fi

# Check if docker-compose is available
if ! command -v docker-compose &> /dev/null; then
    print_error "docker-compose is not installed. Please install Docker Compose."
    exit 1
fi

print_status "Starting services with Docker Compose..."

# Start services
if docker-compose up -d --build; then
    print_success "All services started successfully!"
    
    print_status "Service URLs:"
    echo "  🌐 API Gateway:           https://localhost:7000"
    echo "  📚 API Documentation:    https://localhost:7000/swagger"
    echo "  💓 Health Check:         https://localhost:7000/health"
    echo "  🔐 User Service:         https://localhost:7001"
    echo "  🏢 Organization Service: https://localhost:7002"
    echo "  🚗 Vehicle Service:      https://localhost:7003"
    echo "  👤 Driver Service:       https://localhost:7004"
    echo "  📦 Product Service:      https://localhost:7005"
    echo "  🛣️  Route Service:        https://localhost:7006"
    echo "  ⚖️  Weighbridge Service:  https://localhost:7007"
    echo "  👥 Customer Service:     https://localhost:7008"
    echo "  🏭 Supplier Service:     https://localhost:7009"
    echo "  🚛 Transporter Service:  https://localhost:7010"
    echo "  🏦 Sacco Service:        https://localhost:7011"
    echo "  📊 Weight Data Service:  https://localhost:7012"
    echo "  ✅ Compliance Service:   https://localhost:7013"
    echo "  📈 Operational Data:     https://localhost:7014"
    echo "  💳 Transaction Service:  https://localhost:7015"
    echo "  📊 Analytics Service:    https://localhost:7016"
    echo "  🔄 Data Sync Service:    https://localhost:7017"
    echo "  📁 Archive Service:      https://localhost:7018"
    
    echo ""
    print_status "Useful commands:"
    echo "  📋 View logs:            docker-compose logs -f [service-name]"
    echo "  🔄 Restart service:      docker-compose restart [service-name]"
    echo "  🛑 Stop all services:    docker-compose down"
    echo "  🗑️  Remove all data:      docker-compose down -v"
    
else
    print_error "Failed to start services"
    exit 1
fi