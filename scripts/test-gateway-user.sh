#!/bin/bash

# Test script for Gateway + User Service
set -e

echo "🧪 Testing Gateway + User Service..."

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

print_status "Starting Gateway + User Service for testing..."

# Start services
if docker-compose -f docker-compose.gateway-user.yml up -d --build; then
    print_success "Services started successfully!"
    
    # Wait for services to be ready
    print_status "Waiting for services to be ready..."
    sleep 10
    
    print_status "Service URLs for testing:"
    echo ""
    echo "🌐 API Gateway:"
    echo "  - URL: http://localhost:7000"
    echo "  - Swagger: http://localhost:7000/swagger"
    echo "  - Health: http://localhost:7000/health"
    echo "  - Info: http://localhost:7000/api/gateway/info"
    echo ""
    echo "🔐 User Service (Direct):"
    echo "  - URL: http://localhost:7001"
    echo "  - Swagger: http://localhost:7001/swagger"
    echo "  - Health: http://localhost:7001/health"
    echo ""
    echo "🔗 Through Gateway:"
    echo "  - Auth endpoints: http://localhost:7000/api/auth/*"
    echo "  - User endpoints: http://localhost:7000/api/users/*"
    echo "  - User health: http://localhost:7000/services/users/health"
    echo ""
    print_status "Testing endpoints..."
    echo ""
    
    # Test basic connectivity
    echo "🔍 Testing basic connectivity:"
    
    # Test Gateway health
    if curl -f -s http://localhost:7000/health > /dev/null; then
        print_success "✓ Gateway health check passed"
    else
        print_error "✗ Gateway health check failed"
    fi
    
    # Test User Service health
    if curl -f -s http://localhost:7001/health > /dev/null; then
        print_success "✓ User Service health check passed"
    else
        print_error "✗ User Service health check failed"
    fi
    
    # Test Gateway info
    if curl -f -s http://localhost:7000/api/gateway/info > /dev/null; then
        print_success "✓ Gateway info endpoint accessible"
    else
        print_error "✗ Gateway info endpoint failed"
    fi
    
    # Test User Service through Gateway
    if curl -f -s http://localhost:7000/services/users/health > /dev/null; then
        print_success "✓ User Service accessible through Gateway"
    else
        print_error "✗ User Service not accessible through Gateway"
    fi
    
    echo ""
    print_status "Manual testing commands:"
    echo ""
    echo "# Test Gateway info"
    echo "curl http://localhost:7000/api/gateway/info | jq"
    echo ""
    echo "# Test Gateway health"
    echo "curl http://localhost:7000/health | jq"
    echo ""
    echo "# Register a new user"
    echo "curl -X POST http://localhost:7000/api/auth/register \\"
    echo "  -H 'Content-Type: application/json' \\"
    echo "  -d '{"
    echo "    \"username\": \"testuser\","
    echo "    \"email\": \"test@example.com\","
    echo "    \"password\": \"TestPassword123!\","
    echo "    \"confirmPassword\": \"TestPassword123!\","
    echo "    \"firstName\": \"John\","
    echo "    \"lastName\": \"Doe\""
    echo "  }' | jq"
    echo ""
    echo "# Login"
    echo "curl -X POST http://localhost:7000/api/auth/login \\"
    echo "  -H 'Content-Type: application/json' \\"
    echo "  -d '{"
    echo "    \"username\": \"testuser\","
    echo "    \"password\": \"TestPassword123!\""
    echo "  }' | jq"
    echo ""
    echo "# Check username availability"
    echo "curl http://localhost:7000/api/users/check-username/newuser | jq"
    echo ""
    print_status "Useful commands:"
    echo "  📋 View logs: docker-compose -f docker-compose.gateway-user.yml logs -f"
    echo "  🔄 Restart: docker-compose -f docker-compose.gateway-user.yml restart"
    echo "  🛑 Stop: docker-compose -f docker-compose.gateway-user.yml down"
    echo "  🗑️  Clean: docker-compose -f docker-compose.gateway-user.yml down -v"
    
else
    print_error "Failed to start services"
    exit 1
fi