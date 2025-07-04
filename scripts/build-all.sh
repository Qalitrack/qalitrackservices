#!/bin/bash

# Build script for all QaliTrack services
set -e

echo "🚀 Building QaliTrack Microservices..."

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

print_status "Docker is running ✓"

# Build Gateway
print_status "Building API Gateway..."
cd packages/qalitrack-gateway
if dotnet build QaliTrack.Gateway.sln -c Release; then
    print_success "Gateway build completed"
else
    print_error "Gateway build failed"
    exit 1
fi
cd ../..

# Build Master Data Services
print_status "Building Master Data Services..."
master_services=(
    "packages/microservices/masterdata/user-service"
    "packages/microservices/masterdata/organization-service"
    "packages/microservices/masterdata/vehicle-service"
    "packages/microservices/masterdata/driver-service"
    "packages/microservices/masterdata/product-service"
    "packages/microservices/masterdata/route-service"
    "packages/microservices/masterdata/weighbridge-service"
    "packages/microservices/masterdata/customer-service"
    "packages/microservices/masterdata/supplier-service"
    "packages/microservices/masterdata/transporter-service"
    "packages/microservices/masterdata/sacco-service"
)

for service in "${master_services[@]}"; do
    service_name=$(basename "$service")
    print_status "Building $service_name..."
    
    cd "$service"
    if ls *.sln 1> /dev/null 2>&1; then
        if dotnet build *.sln -c Release; then
            print_success "$service_name build completed"
        else
            print_error "$service_name build failed"
            cd - > /dev/null
            exit 1
        fi
    else
        print_warning "$service_name: No solution file found, skipping..."
    fi
    cd - > /dev/null
done

# Build Data Manager Services
print_status "Building Data Manager Services..."
data_services=(
    "packages/microservices/datamanager/weight-data-service"
    "packages/microservices/datamanager/compliance-service"
    "packages/microservices/datamanager/operational-data-service"
    "packages/microservices/datamanager/transaction-service"
    "packages/microservices/datamanager/analytics-service"
    "packages/microservices/datamanager/data-sync-service"
    "packages/microservices/datamanager/archive-service"
)

for service in "${data_services[@]}"; do
    service_name=$(basename "$service")
    print_status "Building $service_name..."
    
    cd "$service"
    if ls *.sln 1> /dev/null 2>&1; then
        if dotnet build *.sln -c Release; then
            print_success "$service_name build completed"
        else
            print_error "$service_name build failed"
            cd - > /dev/null
            exit 1
        fi
    else
        print_warning "$service_name: No solution file found, skipping..."
    fi
    cd - > /dev/null
done

print_success "All services built successfully! 🎉"

print_status "Next steps:"
echo "  1. Run 'docker-compose up --build' to build and start all services"
echo "  2. Access the API Gateway at https://localhost:7000"
echo "  3. View API documentation at https://localhost:7000/swagger"
echo "  4. Check service health at https://localhost:7000/health"