#!/bin/bash

# QaliTrack Multi-Client Manager
set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

print_header() {
    echo -e "${CYAN}"
    echo "██████╗  █████╗ ██╗     ██╗████████╗██████╗  █████╗  ██████╗██╗  ██╗"
    echo "██╔═══██╗██╔══██╗██║     ██║╚══██╔══╝██╔══██╗██╔══██╗██╔════╝██║ ██╔╝"
    echo "██║   ██║███████║██║     ██║   ██║   ██████╔╝███████║██║     █████╔╝ "
    echo "██║▄▄ ██║██╔══██║██║     ██║   ██║   ██╔══██╗██╔══██║██║     ██╔═██╗ "
    echo "╚██████╔╝██║  ██║███████╗██║   ██║   ██║  ██║██║  ██║╚██████╗██║  ██╗"
    echo " ╚══▀▀═╝ ╚═╝  ╚═╝╚══════╝╚═╝   ╚═╝   ╚═╝  ╚═╝╚═╝  ╚═╝ ╚═════╝╚═╝  ╚═╝"
    echo -e "${NC}"
    echo -e "${BLUE}Multi-Client Microservices Manager${NC}"
    echo ""
}

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

list_clients() {
    echo -e "${CYAN}Available Client Configurations:${NC}"
    echo ""
    
    if [ ! -d "configs/clients" ]; then
        print_error "No client configurations found in configs/clients/"
        return 1
    fi
    
    for config_file in configs/clients/*.yml; do
        if [ -f "$config_file" ]; then
            client_code=$(basename "$config_file" .yml)
            
            # Extract client name and description from YAML
            client_name=$(grep "name:" "$config_file" | head -1 | sed 's/.*name: *"*\\([^"]*\\)"*.*/\\1/')
            client_desc=$(grep "description:" "$config_file" | head -1 | sed 's/.*description: *"*\\([^"]*\\)"*.*/\\1/')
            
            echo -e "📋 ${YELLOW}${client_code}${NC}"
            echo "   Name: ${client_name}"
            echo "   Description: ${client_desc}"
            
            # Count enabled services
            enabled_count=$(grep -c "enabled: true" "$config_file" || echo "0")
            echo "   Services: ${enabled_count} enabled"
            echo ""
        fi
    done
}

generate_deployment() {
    local client_code=$1
    
    if [ -z "$client_code" ]; then
        print_error "Client code is required"
        echo "Usage: $0 generate <client-code>"
        return 1
    fi
    
    local config_file="configs/clients/${client_code}.yml"
    
    if [ ! -f "$config_file" ]; then
        print_error "Configuration file not found: $config_file"
        return 1
    fi
    
    print_status "Generating deployment for client: $client_code"
    
    if python3 scripts/generate-deployment.py "$config_file"; then
        print_success "Deployment generated successfully!"
        echo ""
        print_status "Generated files:"
        echo "  📁 Directory: apps/$client_code/"
        echo "  🐳 Compose: apps/$client_code/docker-compose.$client_code.yml"
        echo "  🚀 Script: apps/$client_code/start-$client_code.sh"
        echo ""
        print_status "To start the deployment:"
        echo "  cd apps/$client_code && ./start-$client_code.sh"
    else
        print_error "Failed to generate deployment"
        return 1
    fi
}

start_client() {
    local client_code=$1
    
    if [ -z "$client_code" ]; then
        print_error "Client code is required"
        echo "Usage: $0 start <client-code>"
        return 1
    fi
    
    local deployment_dir="apps/$client_code"
    local start_script="$deployment_dir/start-$client_code.sh"
    
    if [ ! -f "$start_script" ]; then
        print_warning "Deployment not found. Generating..."
        generate_deployment "$client_code"
    fi
    
    if [ -f "$start_script" ]; then
        print_status "Starting $client_code deployment..."
        cd "$deployment_dir"
        ./start-$client_code.sh
        cd - > /dev/null
    else
        print_error "Failed to find or generate start script"
        return 1
    fi
}

stop_client() {
    local client_code=$1
    local remove_data=${2:-false}
    
    if [ -z "$client_code" ]; then
        print_error "Client code is required"
        echo "Usage: $0 stop <client-code> [--remove-data]"
        return 1
    fi
    
    local deployment_dir="apps/$client_code"
    local compose_file="$deployment_dir/docker-compose.$client_code.yml"
    
    if [ ! -f "$compose_file" ]; then
        print_error "Deployment not found: $compose_file"
        return 1
    fi
    
    print_status "Stopping $client_code deployment..."
    
    cd "$deployment_dir"
    
    if [ "$remove_data" = "true" ]; then
        print_warning "Removing all data volumes..."
        docker compose -f "docker-compose.$client_code.yml" down -v
        print_success "Services stopped and data removed!"
    else
        docker compose -f "docker-compose.$client_code.yml" down
        print_success "Services stopped! (data preserved)"
    fi
    
    cd - > /dev/null
}

show_status() {
    local client_code=$1
    
    if [ -z "$client_code" ]; then
        print_error "Client code is required"
        echo "Usage: $0 status <client-code>"
        return 1
    fi
    
    local deployment_dir="apps/$client_code"
    local compose_file="$deployment_dir/docker-compose.$client_code.yml"
    
    if [ ! -f "$compose_file" ]; then
        print_error "Deployment not found: $compose_file"
        return 1
    fi
    
    print_status "Status for $client_code deployment:"
    echo ""
    
    cd "$deployment_dir"
    docker compose -f "docker-compose.$client_code.yml" ps
    cd - > /dev/null
}

show_logs() {
    local client_code=$1
    local service_name=$2
    
    if [ -z "$client_code" ]; then
        print_error "Client code is required"
        echo "Usage: $0 logs <client-code> [service-name]"
        return 1
    fi
    
    local deployment_dir="apps/$client_code"
    local compose_file="$deployment_dir/docker-compose.$client_code.yml"
    
    if [ ! -f "$compose_file" ]; then
        print_error "Deployment not found: $compose_file"
        return 1
    fi
    
    cd "$deployment_dir"
    
    if [ -n "$service_name" ]; then
        print_status "Showing logs for $client_code/$service_name..."
        docker compose -f "docker-compose.$client_code.yml" logs -f "$service_name"
    else
        print_status "Showing logs for all $client_code services..."
        docker compose -f "docker-compose.$client_code.yml" logs -f
    fi
    
    cd - > /dev/null
}

show_help() {
    echo "QaliTrack Multi-Client Manager"
    echo ""
    echo "Usage: $0 <command> [options]"
    echo ""
    echo "Commands:"
    echo "  list                     List all available client configurations"
    echo "  generate <client>        Generate deployment files for a client"
    echo "  start <client>           Start services for a client"
    echo "  stop <client> [--remove] Stop services for a client (--remove to delete data)"
    echo "  status <client>          Show service status for a client"
    echo "  logs <client> [service]  Show logs for a client (optionally specific service)"
    echo "  help                     Show this help message"
    echo ""
    echo "Examples:"
    echo "  $0 list                              # List all clients"
    echo "  $0 generate testing                  # Generate testing deployment"
    echo "  $0 start babumri-cement             # Start Babumri Cement services"
    echo "  $0 stop kungu-cement                # Stop Kungu Cement services"
    echo "  $0 stop testing --remove-data       # Stop testing and remove data"
    echo "  $0 status national-weighing         # Show status"
    echo "  $0 logs testing user-service        # Show user-service logs for testing"
    echo ""
}

# Main script logic
case "$1" in
    "list")
        print_header
        list_clients
        ;;
    "generate")
        print_header
        generate_deployment "$2"
        ;;
    "start")
        print_header
        start_client "$2"
        ;;
    "stop")
        print_header
        remove_data="false"
        if [ "$3" = "--remove-data" ] || [ "$3" = "--remove" ]; then
            remove_data="true"
        fi
        stop_client "$2" "$remove_data"
        ;;
    "status")
        print_header
        show_status "$2"
        ;;
    "logs")
        print_header
        show_logs "$2" "$3"
        ;;
    "help"|"--help"|"-h"|"")
        print_header
        show_help
        ;;
    *)
        print_header
        print_error "Unknown command: $1"
        echo ""
        show_help
        exit 1
        ;;
esac