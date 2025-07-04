#!/bin/bash

# Stop script for QaliTrack services
set -e

echo "🛑 Stopping QaliTrack Microservices..."

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

# Parse command line arguments
REMOVE_VOLUMES=false
if [[ "$1" == "--remove-data" || "$1" == "-v" ]]; then
    REMOVE_VOLUMES=true
    print_warning "Data volumes will be removed!"
fi

# Check if docker-compose is available
if ! command -v docker-compose &> /dev/null; then
    print_error "docker-compose is not installed."
    exit 1
fi

# Stop services
print_status "Stopping all services..."

if [ "$REMOVE_VOLUMES" = true ]; then
    print_warning "Removing all data volumes..."
    if docker-compose down -v; then
        print_success "All services stopped and data removed!"
    else
        print_error "Failed to stop services"
        exit 1
    fi
else
    if docker-compose down; then
        print_success "All services stopped!"
        print_status "Data volumes preserved. Use '$0 --remove-data' to remove all data."
    else
        print_error "Failed to stop services"
        exit 1
    fi
fi

print_status "Cleanup completed ✓"