#!/bin/bash

# QaliTrack Services Test Runner Script
# This script runs all tests for the QaliTrack microservices implementation

echo "🧪 Starting QaliTrack Services Test Suite..."
echo "================================================"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Test results tracking
TOTAL_TESTS=0
PASSED_TESTS=0
FAILED_TESTS=0
SERVICES_TESTED=0

# Function to run tests for a specific service
run_service_tests() {
    local service_name=$1
    local service_path=$2
    
    echo ""
    echo -e "${YELLOW}Testing $service_name...${NC}"
    echo "----------------------------------------"
    
    if [ -f "$service_path/tests/${service_name}Tests.cs" ]; then
        cd "$service_path" || exit 1
        
        # Run tests and capture output
        test_output=$(dotnet test --verbosity quiet 2>&1)
        test_exit_code=$?
        
        if [ $test_exit_code -eq 0 ]; then
            echo -e "${GREEN}✅ $service_name tests PASSED${NC}"
            PASSED_TESTS=$((PASSED_TESTS + 1))
        else
            echo -e "${RED}❌ $service_name tests FAILED${NC}"
            echo "$test_output"
            FAILED_TESTS=$((FAILED_TESTS + 1))
        fi
        
        SERVICES_TESTED=$((SERVICES_TESTED + 1))
        cd - > /dev/null || exit 1
    else
        echo -e "${YELLOW}⚠️  No test file found for $service_name${NC}"
    fi
}

# Function to run gateway tests
run_gateway_tests() {
    echo ""
    echo -e "${YELLOW}Testing QaliTrack Gateway...${NC}"
    echo "----------------------------------------"
    
    if [ -d "packages/qalitrack-gateway/tests" ]; then
        cd "packages/qalitrack-gateway/tests" || exit 1
        
        # Run tests and capture output
        test_output=$(dotnet test --verbosity quiet 2>&1)
        test_exit_code=$?
        
        if [ $test_exit_code -eq 0 ]; then
            echo -e "${GREEN}✅ Gateway tests PASSED${NC}"
            PASSED_TESTS=$((PASSED_TESTS + 1))
        else
            echo -e "${RED}❌ Gateway tests FAILED${NC}"
            echo "$test_output"
            FAILED_TESTS=$((FAILED_TESTS + 1))
        fi
        
        SERVICES_TESTED=$((SERVICES_TESTED + 1))
        cd - > /dev/null || exit 1
    else
        echo -e "${YELLOW}⚠️  No gateway tests found${NC}"
    fi
}

# Start testing
echo "Starting comprehensive test suite for QaliTrack Services..."
echo ""

# Test individual microservices
run_service_tests "VehicleService" "packages/microservices/operations/vehicle-service"
run_service_tests "DriverService" "packages/microservices/operations/driver-service"
run_service_tests "TransporterService" "packages/microservices/operations/transporter-service"
run_service_tests "RouteService" "packages/microservices/operations/route-service"
run_service_tests "WeighbridgeService" "packages/microservices/operations/weighbridge-service"
run_service_tests "SACCOService" "packages/microservices/operations/sacco-service"

run_service_tests "CustomerService" "packages/microservices/masterdata/customer-service"
run_service_tests "ProductService" "packages/microservices/masterdata/product-service"
run_service_tests "SupplierService" "packages/microservices/masterdata/supplier-service"

run_service_tests "TransactionService" "packages/microservices/transactions/transaction-service"
run_service_tests "WeightDataService" "packages/microservices/transactions/weight-data-service"
run_service_tests "OperationalDataService" "packages/microservices/transactions/operational-data-service"
run_service_tests "ComplianceService" "packages/microservices/transactions/compliance-service"

run_service_tests "AnalyticsService" "packages/microservices/analytics/analytics-service"
run_service_tests "ReportService" "packages/microservices/analytics/report-service"

run_service_tests "DataSyncService" "packages/microservices/datamanager/data-sync-service"
run_service_tests "ArchiveService" "packages/microservices/datamanager/archive-service"

# Test gateway
run_gateway_tests

# Test summary
echo ""
echo "================================================"
echo -e "${YELLOW}📊 Test Summary${NC}"
echo "================================================"
echo "Services Tested: $SERVICES_TESTED"
echo -e "Passed: ${GREEN}$PASSED_TESTS${NC}"
echo -e "Failed: ${RED}$FAILED_TESTS${NC}"
echo ""

# Final result
if [ $FAILED_TESTS -eq 0 ]; then
    echo -e "${GREEN}🎉 All tests PASSED! QaliTrack Services are ready for deployment.${NC}"
    exit 0
else
    echo -e "${RED}❌ Some tests FAILED. Please review the failures above.${NC}"
    exit 1
fi