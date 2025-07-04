# Weight Data Service - Functional Tests

## Test Categories

### 1. Weight Measurement Creation Tests

#### Test Case: WDS-001 - Valid Single Weight Measurement
**Given:** A vehicle approaches weighbridge wb-001
**When:** System captures weight measurement with valid data
**Then:** Measurement should be created successfully

**Test Data:**
```json
{
  "weighbridgeId": "wb-001",
  "vehicleId": "v-12345",
  "driverId": "d-67890",
  "grossWeight": 45000.50,
  "measurementType": "single"
}
```

**Expected Result:**
- Status: 201 Created
- Net weight calculated automatically if tare weight available
- Measurement status: active
- Audit fields populated correctly

#### Test Case: WDS-002 - Entry/Exit Pair Measurement
**Given:** A vehicle enters weighbridge for entry measurement
**When:** Entry measurement is recorded
**And:** Vehicle exits with exit measurement
**Then:** Both measurements should be linked and net weight calculated

**Test Sequence:**
1. **Entry:**
   ```json
   {
     "weighbridgeId": "wb-001",
     "vehicleId": "v-12345",
     "driverId": "d-67890",
     "grossWeight": 45000.50,
     "measurementType": "entry"
   }
   ```

2. **Exit:**
   ```json
   {
     "weighbridgeId": "wb-001",
     "vehicleId": "v-12345",
     "driverId": "d-67890",
     "grossWeight": 15000.00,
     "measurementType": "exit"
   }
   ```

**Expected Result:**
- Net weight = 45000.50 - 15000.00 = 30000.50 kg
- Both measurements linked via transaction

#### Test Case: WDS-003 - Weight Exceeds Capacity
**Given:** Weighbridge has maximum capacity of 80,000 kg
**When:** Vehicle weight exceeds this limit
**Then:** Measurement should be rejected

**Test Data:**
```json
{
  "weighbridgeId": "wb-001",
  "vehicleId": "v-heavy",
  "driverId": "d-67890",
  "grossWeight": 85000.00,
  "measurementType": "single"
}
```

**Expected Result:**
- Status: 422 Unprocessable Entity
- Error: "Weight exceeds maximum capacity"

#### Test Case: WDS-004 - Invalid Vehicle ID
**Given:** Non-existent vehicle ID
**When:** Attempting to create measurement
**Then:** Should return validation error

**Test Data:**
```json
{
  "weighbridgeId": "wb-001",
  "vehicleId": "invalid-vehicle",
  "driverId": "d-67890",
  "grossWeight": 45000.50,
  "measurementType": "single"
}
```

**Expected Result:**
- Status: 400 Bad Request
- Error: "Vehicle not found or inactive"

### 2. Weight Measurement Retrieval Tests

#### Test Case: WDS-005 - Get Measurements with Pagination
**Given:** 150 measurements exist for weighbridge wb-001
**When:** Requesting page 2 with page size 50
**Then:** Should return correct page with navigation info

**Request:**
```
GET /measurements?weighbridgeId=wb-001&page=2&pageSize=50
```

**Expected Result:**
- 50 records returned
- Page 2 of 3 total pages
- hasNext: true, hasPrevious: true

#### Test Case: WDS-006 - Filter by Date Range
**Given:** Measurements from January 1-31, 2024
**When:** Filtering by January 15-20, 2024
**Then:** Only measurements in that range returned

**Request:**
```
GET /measurements?startDate=2024-01-15T00:00:00Z&endDate=2024-01-20T23:59:59Z
```

**Expected Result:**
- Only measurements between Jan 15-20 returned
- Sorted by measuredAt descending

#### Test Case: WDS-007 - Get Measurement by ID with Corrections
**Given:** Measurement ID 123 has been corrected
**When:** Retrieving measurement details
**Then:** Should include correction history

**Expected Result:**
- Original measurement data
- Array of corrections with timestamps
- Correction reasons and authorized users

### 3. Weight Correction Tests

#### Test Case: WDS-008 - Valid Manual Correction
**Given:** Measurement with ID 123 needs correction
**When:** Authorized user applies correction
**Then:** Correction should be saved and measurement updated

**Test Data:**
```json
{
  "correctedGrossWeight": 45001.00,
  "correctionReason": "Calibration adjustment after inspection",
  "correctionType": "manual"
}
```

**Expected Result:**
- Status: 200 OK
- Original values preserved in correction record
- Measurement status remains active
- Audit trail created

#### Test Case: WDS-009 - Unauthorized Correction Attempt
**Given:** User without correction permissions
**When:** Attempting to correct measurement
**Then:** Should be rejected

**Expected Result:**
- Status: 403 Forbidden
- Error: "Insufficient permissions for weight corrections"

#### Test Case: WDS-010 - Invalid Correction Data
**Given:** Correction with negative weight
**When:** Submitting correction
**Then:** Should be rejected with validation error

**Test Data:**
```json
{
  "correctedGrossWeight": -1000.00,
  "correctionReason": "Test correction"
}
```

**Expected Result:**
- Status: 400 Bad Request
- Error: "Corrected weight must be positive"

### 4. Weighbridge Status Tests

#### Test Case: WDS-011 - Weighbridge Heartbeat Update
**Given:** Weighbridge wb-001 is operational
**When:** Hardware sends status update
**Then:** Status should be updated with current timestamp

**Test Data:**
```json
{
  "isOperational": true,
  "currentLoad": 15230.50,
  "temperature": 24.5,
  "humidity": 65.2
}
```

**Expected Result:**
- Status updated successfully
- lastHeartbeat timestamp updated
- Current load reflected in status

#### Test Case: WDS-012 - Weighbridge Offline Detection
**Given:** Weighbridge has not sent heartbeat for 5 minutes
**When:** Checking weighbridge status
**Then:** Should be marked as potentially offline

**Expected Result:**
- isOperational: false (if no heartbeat > 5 minutes)
- Warning flag for maintenance team

#### Test Case: WDS-013 - Calibration Due Alert
**Given:** Weighbridge calibration is due in 7 days
**When:** Checking weighbridge status
**Then:** Should show calibration warning

**Expected Result:**
- maintenanceStatus: "warning"
- calibrationDue date in response
- Alert generated for maintenance

### 5. Real-time Data Tests

#### Test Case: WDS-014 - WebSocket Live Data Stream
**Given:** WebSocket connection to weighbridge wb-001
**When:** Weight changes on weighbridge
**Then:** Real-time updates should be pushed

**Test Sequence:**
1. Connect to WebSocket endpoint
2. Place weight on weighbridge
3. Verify real-time data received
4. Remove weight and verify update

**Expected Result:**
- Real-time weight updates
- Load cell readings included
- Stable weight detection

#### Test Case: WDS-015 - Multiple Concurrent Connections
**Given:** 10 clients connected to live data stream
**When:** Weight changes occur
**Then:** All clients should receive updates

**Expected Result:**
- All connections receive same data
- No data loss or corruption
- Proper connection management

### 6. Analytics Tests

#### Test Case: WDS-016 - Daily Summary Generation
**Given:** 100 measurements recorded today
**When:** Requesting daily analytics summary
**Then:** Accurate summary statistics returned

**Expected Result:**
- Total measurements: 100
- Correct weight totals and averages
- Peak hour identification
- Utilization percentages

#### Test Case: WDS-017 - Trend Calculation
**Given:** Historical data for comparison
**When:** Requesting analytics with trends
**Then:** Growth percentages calculated correctly

**Expected Result:**
- Accurate growth calculations
- Proper baseline comparisons
- Positive/negative trend indicators

### 7. Data Validation Tests

#### Test Case: WDS-018 - Tare Weight Validation
**Given:** Gross weight of 45000 kg and tare weight of 50000 kg
**When:** Creating measurement
**Then:** Should reject invalid tare weight

**Expected Result:**
- Status: 400 Bad Request
- Error: "Tare weight cannot exceed gross weight"

#### Test Case: WDS-019 - Load Cell Variance Check
**Given:** Load cell readings with high variance
**When:** Creating measurement
**Then:** Should flag unstable reading

**Test Data:**
```json
{
  "loadCellReadings": [10000, 15000, 8000, 12000],
  "grossWeight": 45000
}
```

**Expected Result:**
- Warning about unstable reading
- Measurement created with variance flag

#### Test Case: WDS-020 - Concurrent Vehicle Weighing
**Given:** Vehicle v-12345 is on weighbridge wb-001
**When:** Same vehicle attempts weighing on wb-002
**Then:** Should prevent concurrent weighing

**Expected Result:**
- Status: 409 Conflict
- Error: "Vehicle currently being weighed elsewhere"

### 8. Performance Tests

#### Test Case: WDS-021 - High Volume Weight Capture
**Given:** 1000 weight measurements per hour
**When:** System under normal load
**Then:** All measurements processed within SLA

**Expected Result:**
- 95% of measurements processed < 200ms
- No data loss
- Proper queue management

#### Test Case: WDS-022 - Large Dataset Retrieval
**Given:** 1 million historical measurements
**When:** Querying with filters and pagination
**Then:** Response time acceptable

**Expected Result:**
- Query response < 2 seconds
- Pagination working correctly
- Memory usage within limits

### 9. Security Tests

#### Test Case: WDS-023 - Organization Data Isolation
**Given:** User from organization A
**When:** Attempting to access organization B data
**Then:** Should be denied access

**Expected Result:**
- Status: 403 Forbidden
- Only organization A data accessible

#### Test Case: WDS-024 - JWT Token Validation
**Given:** Expired JWT token
**When:** Making API request
**Then:** Should reject request

**Expected Result:**
- Status: 401 Unauthorized
- Error: "Token expired"

### 10. Integration Tests

#### Test Case: WDS-025 - Master Data Service Integration
**Given:** Vehicle ID from Master Data Service
**When:** Creating measurement
**Then:** Should validate vehicle exists and is active

**Mock Responses:**
- Valid vehicle: 200 OK with vehicle details
- Invalid vehicle: 404 Not Found

**Expected Result:**
- Valid vehicles accepted
- Invalid vehicles rejected with proper error

#### Test Case: WDS-026 - User Service Integration
**Given:** User ID from API Gateway headers
**When:** Processing request
**Then:** Should validate user permissions

**Expected Result:**
- Authorized users can access data
- Role-based restrictions enforced
- Audit trail includes user information

## Test Environment Setup

### Database Setup
```sql
-- Create test database
CREATE DATABASE qalitrack_weight_test;

-- Insert test data
INSERT INTO weighbridges (id, max_capacity) VALUES ('wb-001', 80000);
INSERT INTO vehicles (id, registration, status) VALUES ('v-12345', 'KXX-001A', 'active');
INSERT INTO drivers (id, name, license_status) VALUES ('d-67890', 'John Doe', 'valid');
```

### Mock Services
- **Master Data Service:** Mock endpoints for vehicle/driver validation
- **User Service:** Mock user context and permissions
- **Hardware Simulator:** Generate weight readings and status updates

### Test Data Generation
```csharp
// Generate test weight measurements
public static WeightMeasurement CreateTestMeasurement(
    string weighbridgeId = "wb-001",
    decimal grossWeight = 45000.50m,
    MeasurementType type = MeasurementType.Single)
{
    return new WeightMeasurement
    {
        WeighbridgeId = weighbridgeId,
        VehicleId = "v-12345",
        DriverId = "d-67890",
        GrossWeight = grossWeight,
        MeasurementType = type,
        MeasuredAt = DateTime.UtcNow,
        MeasuredBy = "system"
    };
}
```

## Continuous Testing

### Automated Test Suite
- **Unit Tests:** Individual method testing
- **Integration Tests:** Service-to-service communication
- **End-to-End Tests:** Complete user workflows
- **Performance Tests:** Load and stress testing
- **Security Tests:** Penetration testing

### Test Execution
```bash
# Run all tests
dotnet test

# Run specific category
dotnet test --filter Category=WeightMeasurement

# Run performance tests
dotnet test --filter Category=Performance

# Generate coverage report
dotnet test --collect:"XPlat Code Coverage"
```