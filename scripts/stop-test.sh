#!/bin/bash

# Stop testing services
set -e

echo "🛑 Stopping test services..."

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
COMPOSE_FILE="docker-compose.gateway-user.yml"

while [[ $# -gt 0 ]]; do
    case $1 in
        --remove-data|-v)
            REMOVE_VOLUMES=true
            print_warning "Data volumes will be removed!"
            shift
            ;;
        --file|-f)
            COMPOSE_FILE="$2"
            shift 2
            ;;
        *)
            echo "Unknown option: $1"
            echo "Usage: $0 [--remove-data|-v] [--file|-f compose-file]"
            exit 1
            ;;
    esac
done

# Check if docker-compose is available
if ! command -v docker-compose &> /dev/null; then
    print_error "docker-compose is not installed."
    exit 1
fi

print_status "Stopping services from $COMPOSE_FILE..."

if [ "$REMOVE_VOLUMES" = true ]; then
    print_warning "Removing all data volumes..."
    if docker-compose -f "$COMPOSE_FILE" down -v; then
        print_success "Services stopped and data removed!"
    else
        print_error "Failed to stop services"
        exit 1
    fi
else
    if docker-compose -f "$COMPOSE_FILE" down; then
        print_success "Services stopped!"
        print_status "Data volumes preserved. Use '$0 --remove-data' to remove all data."
    else
        print_error "Failed to stop services"
        exit 1
    fi
fi

print_status "Cleanup completed ✓"