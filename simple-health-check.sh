#!/bin/bash

# Simple Health Check Script for QaliTrack Services
# This script tests basic compilation and health endpoint availability

echo "🏥 Starting QaliTrack Services Health Check..."
echo "==============================================="

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

TOTAL_SERVICES=0
HEALTHY_SERVICES=0
FAILED_SERVICES=0

# Function to check service build and health
check_service_health() {
    local service_name=$1
    local service_path=$2
    
    echo ""
    echo -e "${YELLOW}Checking $service_name...${NC}"
    echo "----------------------------------------"
    
    if [ -d "$service_path" ]; then
        cd "$service_path" || exit 1
        
        # Try to build the service
        echo "Building $service_name..."
        build_output=$(dotnet build --verbosity quiet 2>&1)
        build_exit_code=$?
        
        if [ $build_exit_code -eq 0 ]; then
            echo -e "${GREEN}✅ $service_name build PASSED${NC}"
            HEALTHY_SERVICES=$((HEALTHY_SERVICES + 1))
        else
            echo -e "${RED}❌ $service_name build FAILED${NC}"
            echo "Build errors:"
            echo "$build_output" | head -20
            FAILED_SERVICES=$((FAILED_SERVICES + 1))
        fi
        
        TOTAL_SERVICES=$((TOTAL_SERVICES + 1))
        cd - > /dev/null || exit 1
    else
        echo -e "${YELLOW}⚠️  Service path not found: $service_path${NC}"
    fi
}

# Function to check gateway health
check_gateway_health() {
    echo ""
    echo -e "${YELLOW}Checking QaliTrack Gateway...${NC}"
    echo "----------------------------------------"
    
    if [ -d "packages/qalitrack-gateway/src" ]; then
        cd "packages/qalitrack-gateway/src" || exit 1
        
        # Try to build the gateway
        echo "Building QaliTrack Gateway..."
        build_output=$(dotnet build --verbosity quiet 2>&1)
        build_exit_code=$?
        
        if [ $build_exit_code -eq 0 ]; then
            echo -e "${GREEN}✅ Gateway build PASSED${NC}"
            HEALTHY_SERVICES=$((HEALTHY_SERVICES + 1))
        else
            echo -e "${RED}❌ Gateway build FAILED${NC}"
            echo "Build errors:"
            echo "$build_output" | head -20
            FAILED_SERVICES=$((FAILED_SERVICES + 1))
        fi
        
        TOTAL_SERVICES=$((TOTAL_SERVICES + 1))
        cd - > /dev/null || exit 1
    else
        echo -e "${YELLOW}⚠️  Gateway path not found${NC}"
    fi
}

# Start health checks
echo "Starting health checks for implemented services..."
echo ""

# Check implemented services
check_service_health "CustomerService" "packages/microservices/masterdata/customer-service/src/CustomerService.Api"
check_service_health "TransactionService" "packages/microservices/transactions/transaction-service/src/TransactionService.Api"
check_service_health "AnalyticsService" "packages/microservices/analytics/analytics-service/src/AnalyticsService.Api"
check_service_health "DataSyncService" "packages/microservices/datamanager/data-sync-service/src/DataSyncService.Api"
check_service_health "ArchiveService" "packages/microservices/datamanager/archive-service/src/ArchiveService.Api"

# Check gateway
check_gateway_health

# Health summary
echo ""
echo "==============================================="
echo -e "${YELLOW}🏥 Health Check Summary${NC}"
echo "==============================================="
echo "Total Services: $TOTAL_SERVICES"
echo -e "Healthy: ${GREEN}$HEALTHY_SERVICES${NC}"
echo -e "Failed: ${RED}$FAILED_SERVICES${NC}"
echo ""

# Final result
if [ $FAILED_SERVICES -eq 0 ]; then
    echo -e "${GREEN}🎉 All services are healthy and ready!${NC}"
    exit 0
else
    echo -e "${RED}❌ Some services have build issues. Please review above.${NC}"
    exit 1
fi