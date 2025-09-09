#!/bin/bash

# Comprehensive QaliTrack API Test Suite
# Tests all HTTP methods, error cases, and edge cases

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# API URLs
MASTERDATA_URL="http://localhost:5004"
DATAMANAGER_URL="http://localhost:5005"

# Test counters
TEST_COUNT=0
PASS_COUNT=0
FAIL_COUNT=0

# Arrays to store created IDs for cleanup
declare -a LOCATION_TYPE_IDS=()
declare -a ZONE_IDS=()
declare -a SITE_IDS=()
declare -a BUSINESS_ENTITY_IDS=()
declare -a VEHICLE_IDS=()
declare -a DRIVER_IDS=()

# Function to run a test
run_test() {
    local test_name="$1"
    local method="$2"
    local url="$3"
    local data="$4"
    local expected_status="$5"
    local description="$6"
    
    TEST_COUNT=$((TEST_COUNT + 1))
    echo -e "\n${BLUE}Test $TEST_COUNT: $test_name${NC}"
    if [ ! -z "$description" ]; then
        echo "Description: $description"
    fi
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
        
        # Extract and store ID from successful creation
        if [ "$method" = "POST" ] && [ "$http_status" = "201" ]; then
            extract_and_store_id "$url" "$body"
        fi
    else
        echo -e "${RED}❌ FAIL${NC} (Expected: $expected_status, Got: $http_status)"
        echo "Response Body:"
        echo "$body" | head -c 500
        echo ""
        FAIL_COUNT=$((FAIL_COUNT + 1))
    fi
}

# Function to extract ID from response and store it
extract_and_store_id() {
    local url="$1"
    local response_body="$2"
    
    # Extract ID from JSON response (assuming format: "id":"guid-here")
    local id=$(echo "$response_body" | grep -o '"[iI]d":"[^"]*"' | head -1 | cut -d'"' -f4)
    
    if [ ! -z "$id" ]; then
        if [[ "$url" == *"location-types"* ]]; then
            LOCATION_TYPE_IDS+=("$id")
        elif [[ "$url" == *"zones"* ]]; then
            ZONE_IDS+=("$id")
        elif [[ "$url" == *"sites"* ]]; then
            SITE_IDS+=("$id")
        elif [[ "$url" == *"business-entities"* ]]; then
            BUSINESS_ENTITY_IDS+=("$id")
        elif [[ "$url" == *"vehicles"* ]]; then
            VEHICLE_IDS+=("$id")
        elif [[ "$url" == *"drivers"* ]]; then
            DRIVER_IDS+=("$id")
        fi
        echo "Stored ID: $id"
    fi
}

echo "🚀 Starting Comprehensive QaliTrack API Test Suite"
echo "================================================="
echo "This test will systematically test all HTTP methods and error cases"

# Phase 1: Health Checks
echo -e "\n${YELLOW}=== PHASE 1: Health Checks ===${NC}"
run_test "MasterData Health Check" "GET" "$MASTERDATA_URL/../health" "" 200 "Verify MasterData API is running"
run_test "DataManager Health Check" "GET" "$DATAMANAGER_URL/../health" "" 200 "Verify DataManager API is running"

# Phase 2: Initial GET requests (should be empty)
echo -e "\n${YELLOW}=== PHASE 2: Initial Empty State Tests ===${NC}"
run_test "Get LocationTypes (Empty)" "GET" "$MASTERDATA_URL/location-types" "" 200 "Should return empty list initially"
run_test "Get Zones (Empty)" "GET" "$MASTERDATA_URL/zones" "" 200 "Should return empty list initially"
run_test "Get Sites (Empty)" "GET" "$MASTERDATA_URL/sites" "" 200 "Should return empty list initially"
run_test "Get BusinessEntities (Empty)" "GET" "$MASTERDATA_URL/business-entities" "" 200 "Should return empty list initially"
run_test "Get vehicles (Empty)" "GET" "$MASTERDATA_URL/vehicles" "" 200 "Should return empty list initially"
run_test "Get drivers (Empty)" "GET" "$MASTERDATA_URL/drivers" "" 200 "Should return empty list initially"

# Phase 3: Invalid GET requests (should return 404)
echo -e "\n${YELLOW}=== PHASE 3: Invalid Resource Tests ===${NC}"
run_test "Get Non-existent LocationType" "GET" "$MASTERDATA_URL/location-types/00000000-0000-0000-0000-000000000001" "" 404 "Should return 404 for non-existent resource"
run_test "Get Non-existent Zone" "GET" "$MASTERDATA_URL/zones/00000000-0000-0000-0000-000000000001" "" 404 "Should return 404 for non-existent resource"
run_test "Get Non-existent Site" "GET" "$MASTERDATA_URL/sites/00000000-0000-0000-0000-000000000001" "" 404 "Should return 404 for non-existent resource"
run_test "Get Non-existent businessentity" "GET" "$MASTERDATA_URL/business-entities/00000000-0000-0000-0000-000000000001" "" 404 "Should return 404 for non-existent resource"
run_test "Get Non-existent vehicle" "GET" "$MASTERDATA_URL/vehicles/00000000-0000-0000-0000-000000000001" "" 404 "Should return 404 for non-existent resource"
run_test "Get Non-existent driver" "GET" "$MASTERDATA_URL/drivers/00000000-0000-0000-0000-000000000001" "" 404 "Should return 404 for non-existent resource"

# Phase 4: Invalid POST requests (should return 400)
echo -e "\n${YELLOW}=== PHASE 4: Invalid Creation Tests ===${NC}"
run_test "Create LocationType - Missing Name" "POST" "$MASTERDATA_URL/location-types" \
    '{"code":"PLT","description":"Missing name","isActive":true}' 400 "Should return 400 for missing required field"
run_test "Create LocationType - Missing Code" "POST" "$MASTERDATA_URL/location-types" \
    '{"name":"Plant","description":"Missing code","isActive":true}' 400 "Should return 400 for missing required field"
run_test "Create LocationType - Empty JSON" "POST" "$MASTERDATA_URL/location-types" \
    '{}' 400 "Should return 400 for empty request body"

run_test "Create Zone - Missing Name" "POST" "$MASTERDATA_URL/zones" \
    '{"code":"COAST","description":"Missing name","isActive":true}' 400 "Should return 400 for missing required field"

run_test "Create businessentity - Missing Name" "POST" "$MASTERDATA_URL/business-entities" \
    '{"code":"ABC-001","registrationNumber":"REG-001","contactEmail":"test@test.com","contactPhone":"+1234567890","address":"Test Address","entityType":"Customer"}' 400 "Should return 400 for missing required field"

run_test "Create vehicle - Missing Registration" "POST" "$MASTERDATA_URL/vehicles" \
    '{"make":"Toyota","model":"Truck","fuelType":"Diesel","maxWeight":10000,"status":"Active"}' 400 "Should return 400 for missing required field"

run_test "Create driver - Missing Name" "POST" "$MASTERDATA_URL/drivers" \
    '{"lastName":"driver","phoneNumber":"+1234567890","email":"driver@test.com","employeeId":"EMP-001","licenseNumber":"LIC-001","status":"Active"}' 400 "Should return 400 for missing required field"

# Phase 5: Valid creation tests
echo -e "\n${YELLOW}=== PHASE 5: Valid Creation Tests ===${NC}"
run_test "Create LocationType - Plant" "POST" "$MASTERDATA_URL/location-types" \
    '{"name":"Plant","code":"PLT","description":"Manufacturing facility","isActive":true}' 201 "Should successfully create LocationType"

run_test "Create LocationType - Warehouse" "POST" "$MASTERDATA_URL/location-types" \
    '{"name":"Warehouse","code":"WHR","description":"Storage facility","isActive":true}' 201 "Should successfully create second LocationType"

run_test "Create Zone - Coastal" "POST" "$MASTERDATA_URL/zones" \
    '{"name":"Coastal Region","code":"COAST","description":"Coastal operations","isActive":true}' 201 "Should successfully create Zone"

run_test "Create Zone - Eastern" "POST" "$MASTERDATA_URL/zones" \
    '{"name":"Eastern Region","code":"EAST","description":"Eastern operations","isActive":true}' 201 "Should successfully create second Zone"

run_test "Create businessentity - Customer" "POST" "$MASTERDATA_URL/business-entities" \
    '{"name":"ABC Construction Ltd","code":"ABC-CONST","registrationNumber":"REG-ABC-2020","entityType":"Customer","contactEmail":"orders@abcconstruction.co.ke","contactPhone":"+254700123456","address":"Westlands, Nairobi","city":"Nairobi","country":"Kenya"}' 201 "Should successfully create businessentity"

run_test "Create businessentity - Supplier" "POST" "$MASTERDATA_URL/business-entities" \
    '{"name":"Kenya Limestone Quarries","code":"KLQ-SUPP","registrationNumber":"REG-KLQ-2018","entityType":"Supplier","contactEmail":"supply@klq.co.ke","contactPhone":"+254722987654","address":"Voi, Taita Taveta","city":"Voi","country":"Kenya"}' 201 "Should successfully create second businessentity"

run_test "Create vehicle - Truck" "POST" "$MASTERDATA_URL/vehicles" \
    '{"registrationNumber":"KCA 123A","vehicleType":"Truck","make":"Isuzu","model":"FVZ","fuelType":"Diesel","maxWeight":35000,"maxLegalLoad":35000,"maxSafeLoad":32000,"tareWeight":15000,"hasContainer":true,"containerType":"Tarpaulin","status":"Active"}' 201 "Should successfully create vehicle"

run_test "Create vehicle - Canter" "POST" "$MASTERDATA_URL/vehicles" \
    '{"registrationNumber":"KBZ 456B","vehicleType":"Truck","make":"Mitsubishi","model":"Canter","fuelType":"Diesel","maxWeight":25000,"maxLegalLoad":25000,"maxSafeLoad":22000,"tareWeight":8000,"hasContainer":true,"containerType":"Sealed_Box","status":"Active"}' 201 "Should successfully create second vehicle"

run_test "Create driver - John" "POST" "$MASTERDATA_URL/drivers" \
    '{"firstName":"John","lastName":"Kamau","phoneNumber":"+254701234567","email":"j.kamau@transport.co.ke","employeeId":"DRV001","licenseNumber":"DL-123456789","licenseClass":"CDL-A","licenseExpiry":"2025-12-31","status":"Active"}' 201 "Should successfully create driver"

run_test "Create driver - Mary" "POST" "$MASTERDATA_URL/drivers" \
    '{"firstName":"Mary","lastName":"Wanjiku","phoneNumber":"+254722334567","email":"m.wanjiku@transport.co.ke","employeeId":"DRV002","licenseNumber":"DL-987654321","licenseClass":"CDL-B","licenseExpiry":"2025-08-15","status":"Active"}' 201 "Should successfully create second driver"

# Phase 6: Duplicate creation tests (should return 400)
echo -e "\n${YELLOW}=== PHASE 6: Duplicate Creation Tests ===${NC}"
run_test "Create LocationType - Duplicate Code" "POST" "$MASTERDATA_URL/location-types" \
    '{"name":"Plant Copy","code":"PLT","description":"Duplicate code","isActive":true}' 400 "Should return 400 for duplicate code"

run_test "Create Zone - Duplicate Code" "POST" "$MASTERDATA_URL/zones" \
    '{"name":"Coastal Copy","code":"COAST","description":"Duplicate code","isActive":true}' 400 "Should return 400 for duplicate code"

run_test "Create businessentity - Duplicate Code" "POST" "$MASTERDATA_URL/business-entities" \
    '{"name":"ABC Construction Copy","code":"ABC-CONST","registrationNumber":"REG-ABC-2021","entityType":"Customer","contactEmail":"orders2@abcconstruction.co.ke","contactPhone":"+254700123457","address":"Different Address","city":"Nairobi","country":"Kenya"}' 400 "Should return 400 for duplicate code"

run_test "Create vehicle - Duplicate Registration" "POST" "$MASTERDATA_URL/vehicles" \
    '{"registrationNumber":"KCA 123A","vehicleType":"Truck","make":"Toyota","model":"Different","fuelType":"Diesel","maxWeight":30000,"status":"Active"}' 400 "Should return 400 for duplicate registration"

run_test "Create driver - Duplicate Employee ID" "POST" "$MASTERDATA_URL/drivers" \
    '{"firstName":"Jane","lastName":"Doe","phoneNumber":"+254701234568","email":"jane@transport.co.ke","employeeId":"DRV001","licenseNumber":"DL-999888777","licenseClass":"CDL-A","licenseExpiry":"2025-12-31","status":"Active"}' 400 "Should return 400 for duplicate employee ID"

# Phase 7: GET requests with data
echo -e "\n${YELLOW}=== PHASE 7: Retrieve Data Tests ===${NC}"
run_test "Get LocationTypes (With Data)" "GET" "$MASTERDATA_URL/location-types" "" 200 "Should return list with created LocationTypes"
run_test "Get Zones (With Data)" "GET" "$MASTERDATA_URL/zones" "" 200 "Should return list with created Zones"
run_test "Get BusinessEntities (With Data)" "GET" "$MASTERDATA_URL/business-entities" "" 200 "Should return list with created BusinessEntities"
run_test "Get vehicles (With Data)" "GET" "$MASTERDATA_URL/vehicles" "" 200 "Should return list with created vehicles"
run_test "Get drivers (With Data)" "GET" "$MASTERDATA_URL/drivers" "" 200 "Should return list with created drivers"

# Phase 8: GET individual resources by ID
echo -e "\n${YELLOW}=== PHASE 8: Retrieve Individual Resources ===${NC}"
if [ ${#LOCATION_TYPE_IDS[@]} -gt 0 ]; then
    run_test "Get LocationType by ID" "GET" "$MASTERDATA_URL/location-types/${LOCATION_TYPE_IDS[0]}" "" 200 "Should return specific LocationType"
fi
if [ ${#ZONE_IDS[@]} -gt 0 ]; then
    run_test "Get Zone by ID" "GET" "$MASTERDATA_URL/zones/${ZONE_IDS[0]}" "" 200 "Should return specific Zone"
fi
if [ ${#BUSINESS_ENTITY_IDS[@]} -gt 0 ]; then
    run_test "Get businessentity by ID" "GET" "$MASTERDATA_URL/business-entities/${BUSINESS_ENTITY_IDS[0]}" "" 200 "Should return specific businessentity"
fi
if [ ${#VEHICLE_IDS[@]} -gt 0 ]; then
    run_test "Get vehicle by ID" "GET" "$MASTERDATA_URL/vehicles/${VEHICLE_IDS[0]}" "" 200 "Should return specific vehicle"
fi
if [ ${#DRIVER_IDS[@]} -gt 0 ]; then
    run_test "Get driver by ID" "GET" "$MASTERDATA_URL/drivers/${DRIVER_IDS[0]}" "" 200 "Should return specific driver"
fi

# Phase 9: PUT (Update) tests
echo -e "\n${YELLOW}=== PHASE 9: Update Tests ===${NC}"
if [ ${#LOCATION_TYPE_IDS[@]} -gt 0 ]; then
    run_test "Update LocationType" "PUT" "$MASTERDATA_URL/location-types/${LOCATION_TYPE_IDS[0]}" \
        '{"name":"Updated Plant","description":"Updated description","isActive":true}' 200 "Should successfully update LocationType"
fi
if [ ${#BUSINESS_ENTITY_IDS[@]} -gt 0 ]; then
    run_test "Update businessentity" "PUT" "$MASTERDATA_URL/business-entities/${BUSINESS_ENTITY_IDS[0]}" \
        '{"name":"Updated ABC Construction Ltd","contactEmail":"updated@abcconstruction.co.ke","contactPhone":"+254700123999","address":"Updated Address","status":"Active"}' 200 "Should successfully update businessentity"
fi

# Phase 10: Update non-existent resources (should return 404)
echo -e "\n${YELLOW}=== PHASE 10: Update Non-existent Resources ===${NC}"
run_test "Update Non-existent LocationType" "PUT" "$MASTERDATA_URL/location-types/00000000-0000-0000-0000-000000000001" \
    '{"name":"Non-existent","description":"Should fail","isActive":true}' 404 "Should return 404 for non-existent resource"

run_test "Update Non-existent businessentity" "PUT" "$MASTERDATA_URL/business-entities/00000000-0000-0000-0000-000000000001" \
    '{"name":"Non-existent","contactEmail":"none@test.com","address":"None","status":"Active"}' 404 "Should return 404 for non-existent resource"

# Phase 11: DataManager API tests (if available)
echo -e "\n${YELLOW}=== PHASE 11: DataManager API Tests ===${NC}"
run_test "Get Customer Orders" "GET" "$DATAMANAGER_URL/orders/customer-orders" "" 200 "Should return customer orders (may be empty)"
run_test "Get Purchase Orders" "GET" "$DATAMANAGER_URL/orders/purchase-orders" "" 200 "Should return purchase orders (may be empty)"
run_test "Get Transactions" "GET" "$DATAMANAGER_URL/transactions" "" 200 "Should return transactions (may be empty)"

# Phase 12: DELETE tests - Clean up created resources
echo -e "\n${YELLOW}=== PHASE 12: DELETE Tests (Cleanup) ===${NC}"
echo "Cleaning up created resources..."

# Delete in reverse order to handle dependencies
for id in "${DRIVER_IDS[@]}"; do
    run_test "Delete driver $id" "DELETE" "$MASTERDATA_URL/drivers/$id" "" 200 "Should successfully delete driver"
done

for id in "${VEHICLE_IDS[@]}"; do
    run_test "Delete vehicle $id" "DELETE" "$MASTERDATA_URL/vehicles/$id" "" 200 "Should successfully delete vehicle"
done

for id in "${BUSINESS_ENTITY_IDS[@]}"; do
    run_test "Delete businessentity $id" "DELETE" "$MASTERDATA_URL/business-entities/$id" "" 200 "Should successfully delete businessentity"
done

for id in "${SITE_IDS[@]}"; do
    run_test "Delete Site $id" "DELETE" "$MASTERDATA_URL/sites/$id" "" 200 "Should successfully delete Site"
done

for id in "${ZONE_IDS[@]}"; do
    run_test "Delete Zone $id" "DELETE" "$MASTERDATA_URL/zones/$id" "" 200 "Should successfully delete Zone"
done

for id in "${LOCATION_TYPE_IDS[@]}"; do
    run_test "Delete LocationType $id" "DELETE" "$MASTERDATA_URL/location-types/$id" "" 200 "Should successfully delete LocationType"
done

# Phase 13: Verify deletion (should return 404 or empty lists)
echo -e "\n${YELLOW}=== PHASE 13: Verify Deletion ===${NC}"
run_test "Get LocationTypes (After Cleanup)" "GET" "$MASTERDATA_URL/location-types" "" 200 "Should return empty list after cleanup"
run_test "Get Zones (After Cleanup)" "GET" "$MASTERDATA_URL/zones" "" 200 "Should return empty list after cleanup"
run_test "Get BusinessEntities (After Cleanup)" "GET" "$MASTERDATA_URL/business-entities" "" 200 "Should return empty list after cleanup"
run_test "Get vehicles (After Cleanup)" "GET" "$MASTERDATA_URL/vehicles" "" 200 "Should return empty list after cleanup"
run_test "Get drivers (After Cleanup)" "GET" "$MASTERDATA_URL/drivers" "" 200 "Should return empty list after cleanup"

# Delete non-existent resources (should return 404)
run_test "Delete Non-existent LocationType" "DELETE" "$MASTERDATA_URL/location-types/00000000-0000-0000-0000-000000000001" "" 404 "Should return 404 for non-existent resource"

# Summary
echo -e "\n${YELLOW}=== COMPREHENSIVE TEST SUMMARY ===${NC}"
echo "========================================="
echo "Total Tests: $TEST_COUNT"
echo -e "Passed: ${GREEN}$PASS_COUNT${NC}"
echo -e "Failed: ${RED}$FAIL_COUNT${NC}"

if [ $FAIL_COUNT -eq 0 ]; then
    echo -e "\n${GREEN}🎉 ALL TESTS PASSED! The API is working correctly.${NC}"
    exit 0
else
    echo -e "\n${RED}❌ $FAIL_COUNT TESTS FAILED. Review the errors above and fix the issues.${NC}"
    echo -e "${YELLOW}💡 Common issues to check:${NC}"
    echo "- Required fields missing in DTOs"
    echo "- Incorrect URL paths"
    echo "- Database constraints not properly configured"
    echo "- Controller methods not implemented"
    echo "- AutoMapper configurations missing"
    exit 1
fi