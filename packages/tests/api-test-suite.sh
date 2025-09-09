#!/bin/bash

# QaliTrack API Test Suite
# Comprehensive testing of all endpoints to identify issues systematically

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# API URLs
MASTERDATA_URL="http://localhost:5004/api"
DATAMANAGER_URL="http://localhost:5005/api"

# Test counter
TEST_COUNT=0
PASS_COUNT=0
FAIL_COUNT=0

# Function to run a test
run_test() {
    local test_name="$1"
    local method="$2"
    local url="$3"
    local data="$4"
    local expected_status="$5"
    
    TEST_COUNT=$((TEST_COUNT + 1))
    echo -e "\n${BLUE}Test $TEST_COUNT: $test_name${NC}"
    echo "Method: $method"
    echo "URL: $url"
    if [ ! -z "$data" ]; then
        echo "Data: $data"
    fi
    
    # Prepare curl command
    if [ "$method" = "GET" ]; then
        response=$(curl -s -w "HTTPSTATUS:%{http_code}" "$url")
    elif [ "$method" = "POST" ] || [ "$method" = "PUT" ]; then
        response=$(curl -s -w "HTTPSTATUS:%{http_code}" -X "$method" -H "Content-Type: application/json" -d "$data" "$url")
    elif [ "$method" = "DELETE" ]; then
        response=$(curl -s -w "HTTPSTATUS:%{http_code}" -X DELETE "$url")
    fi
    
    # Extract HTTP status and body
    http_status=$(echo $response | grep -o "HTTPSTATUS:[0-9]*" | cut -d: -f2)
    body=$(echo $response | sed -E 's/HTTPSTATUS:[0-9]*$//')
    
    # Check result
    if [ "$http_status" -eq "$expected_status" ]; then
        echo -e "${GREEN}✅ PASS${NC} (Status: $http_status)"
        PASS_COUNT=$((PASS_COUNT + 1))
    else
        echo -e "${RED}❌ FAIL${NC} (Expected: $expected_status, Got: $http_status)"
        echo "Response: $body"
        FAIL_COUNT=$((FAIL_COUNT + 1))
    fi
}

echo "🚀 Starting QaliTrack API Test Suite"
echo "==================================="

# Test API Health
echo -e "\n${YELLOW}=== API Health Tests ===${NC}"
run_test "MasterData Health Check" "GET" "$MASTERDATA_URL/../health" "" 200
run_test "DataManager Health Check" "GET" "$DATAMANAGER_URL/../health" "" 200

# Test MasterData API - SiteManagement
echo -e "\n${YELLOW}=== MasterData API - SiteManagement ===${NC}"

# Location Types
run_test "Get LocationTypes (Empty)" "GET" "$MASTERDATA_URL/SiteManagement/location-types" "" 200
run_test "Create LocationType - Plant" "POST" "$MASTERDATA_URL/SiteManagement/location-types" \
    '{"name":"Plant","code":"PLT","description":"Manufacturing facility","isActive":true}' 201
run_test "Create LocationType - Warehouse" "POST" "$MASTERDATA_URL/SiteManagement/location-types" \
    '{"name":"Warehouse","code":"WHR","description":"Storage facility","isActive":true}' 201
run_test "Create LocationType - Duplicate" "POST" "$MASTERDATA_URL/SiteManagement/location-types" \
    '{"name":"Plant","code":"PLT","description":"Manufacturing facility","isActive":true}' 400
run_test "Get LocationTypes (With Data)" "GET" "$MASTERDATA_URL/SiteManagement/location-types" "" 200

# Zones
run_test "Get Zones (Empty)" "GET" "$MASTERDATA_URL/SiteManagement/zones" "" 200
run_test "Create Zone - Coastal" "POST" "$MASTERDATA_URL/SiteManagement/zones" \
    '{"name":"Coastal Region","code":"COAST","description":"Coastal operations","isActive":true}' 201
run_test "Create Zone - Eastern" "POST" "$MASTERDATA_URL/SiteManagement/zones" \
    '{"name":"Eastern Region","code":"EAST","description":"Eastern operations","isActive":true}' 201
run_test "Create Zone - Duplicate" "POST" "$MASTERDATA_URL/SiteManagement/zones" \
    '{"name":"Coastal Region","code":"COAST","description":"Coastal operations","isActive":true}' 400
run_test "Get Zones (With Data)" "GET" "$MASTERDATA_URL/SiteManagement/zones" "" 200

# Sites (will need actual IDs from previous responses)
run_test "Get Sites (Empty)" "GET" "$MASTERDATA_URL/SiteManagement/sites" "" 200

# Test MasterData API - BusinessEntity  
echo -e "\n${YELLOW}=== MasterData API - BusinessEntity ===${NC}"
run_test "Get BusinessEntities (Empty)" "GET" "$MASTERDATA_URL/BusinessEntity" "" 200
run_test "Create BusinessEntity - Customer" "POST" "$MASTERDATA_URL/BusinessEntity" \
    '{"name":"ABC Construction Ltd","code":"ABC-CONST","registrationNumber":"REG-ABC-2020","entityType":"Customer","contactEmail":"orders@abcconstruction.co.ke","contactPhone":"+254700123456","address":"Westlands, Nairobi","city":"Nairobi","country":"Kenya"}' 201
run_test "Create BusinessEntity - Duplicate Code" "POST" "$MASTERDATA_URL/BusinessEntity" \
    '{"name":"ABC Construction Ltd","code":"ABC-CONST","registrationNumber":"REG-ABC-2020","entityType":"Customer","contactEmail":"orders@abcconstruction.co.ke","contactPhone":"+254700123456","address":"Westlands, Nairobi","city":"Nairobi","country":"Kenya"}' 400
run_test "Get BusinessEntities (With Data)" "GET" "$MASTERDATA_URL/BusinessEntity" "" 200

# Test MasterData API - Vehicle
echo -e "\n${YELLOW}=== MasterData API - Vehicle ===${NC}"
run_test "Get Vehicles (Empty)" "GET" "$MASTERDATA_URL/Vehicle" "" 200
run_test "Create Vehicle" "POST" "$MASTERDATA_URL/Vehicle" \
    '{"registrationNumber":"KCA 123A","make":"Isuzu","model":"FVZ","fuelType":"Diesel","maxWeight":35000,"maxLegalLoad":35000,"maxSafeLoad":32000,"tareWeight":15000,"hasContainer":true,"containerType":"Tarpaulin","status":"Active"}' 201
run_test "Create Vehicle - Duplicate Registration" "POST" "$MASTERDATA_URL/Vehicle" \
    '{"registrationNumber":"KCA 123A","make":"Isuzu","model":"FVZ","fuelType":"Diesel","maxWeight":35000,"maxLegalLoad":35000,"maxSafeLoad":32000,"tareWeight":15000,"hasContainer":true,"containerType":"Tarpaulin","status":"Active"}' 400
run_test "Get Vehicles (With Data)" "GET" "$MASTERDATA_URL/Vehicle" "" 200

# Test MasterData API - Driver
echo -e "\n${YELLOW}=== MasterData API - Driver ===${NC}"
run_test "Get Drivers (Empty)" "GET" "$MASTERDATA_URL/Driver" "" 200
run_test "Create Driver" "POST" "$MASTERDATA_URL/Driver" \
    '{"firstName":"John","lastName":"Kamau","phoneNumber":"+254701234567","email":"j.kamau@transport.co.ke","employeeId":"DRV001","licenseNumber":"DL-123456789","licenseClass":"CDL-A","licenseExpiry":"2025-12-31","status":"Active"}' 201
run_test "Create Driver - Duplicate Employee ID" "POST" "$MASTERDATA_URL/Driver" \
    '{"firstName":"John","lastName":"Kamau","phoneNumber":"+254701234567","email":"j.kamau@transport.co.ke","employeeId":"DRV001","licenseNumber":"DL-123456789","licenseClass":"CDL-A","licenseExpiry":"2025-12-31","status":"Active"}' 400
run_test "Get Drivers (With Data)" "GET" "$MASTERDATA_URL/Driver" "" 200

# Test DataManager API - Orders
echo -e "\n${YELLOW}=== DataManager API - Orders ===${NC}"
run_test "Get Customer Orders (Empty)" "GET" "$DATAMANAGER_URL/Orders/customer-orders" "" 200
run_test "Get Purchase Orders (Empty)" "GET" "$DATAMANAGER_URL/Orders/purchase-orders" "" 200

# Test DataManager API - Transactions
echo -e "\n${YELLOW}=== DataManager API - Transactions ===${NC}"
run_test "Get Transactions (Empty)" "GET" "$DATAMANAGER_URL/Transactions" "" 200

# Test DataManager API - Quality
echo -e "\n${YELLOW}=== DataManager API - Quality ===${NC}"
run_test "Get Quality Test Results (Empty)" "GET" "$DATAMANAGER_URL/Quality/test-results" "" 200

# Test DataManager API - Operations
echo -e "\n${YELLOW}=== DataManager API - Operations ===${NC}"
run_test "Get Operational Alerts (Empty)" "GET" "$DATAMANAGER_URL/Operations/alerts" "" 200

# Summary
echo -e "\n${YELLOW}=== Test Summary ===${NC}"
echo "Total Tests: $TEST_COUNT"
echo -e "Passed: ${GREEN}$PASS_COUNT${NC}"
echo -e "Failed: ${RED}$FAIL_COUNT${NC}"

if [ $FAIL_COUNT -eq 0 ]; then
    echo -e "\n${GREEN}🎉 All tests passed!${NC}"
    exit 0
else
    echo -e "\n${RED}❌ Some tests failed. Review the errors above.${NC}"
    exit 1
fi