# Weight Data Service - API Endpoints

## Base URL
`/api/v1/weight-data`

## Authentication
All endpoints require JWT authentication via `Authorization: Bearer {token}` header.
User context is provided via API Gateway headers: `X-User-ID`, `X-User-Roles`, `X-Organization-ID`.

## Weight Measurements

### POST /measurements
Create a new weight measurement from weighbridge hardware.

**Request:**
```json
{
  "weighbridgeId": "wb-001",
  "vehicleId": "v-12345",
  "driverId": "d-67890",
  "grossWeight": 45000.50,
  "tareWeight": 15000.00,
  "measurementType": "entry",
  "loadCellReadings": [11250.12, 11250.13, 11250.12, 11250.13],
  "temperature": 25.5,
  "calibrationFactor": 1.0000
}
```

**Response (201):**
```json
{
  "id": 123,
  "weighbridgeId": "wb-001",
  "vehicleId": "v-12345",
  "driverId": "d-67890",
  "grossWeight": 45000.50,
  "tareWeight": 15000.00,
  "netWeight": 30000.50,
  "weightUnit": "kg",
  "measurementType": "entry",
  "measurementStatus": "active",
  "loadCellReadings": "[11250.12, 11250.13, 11250.12, 11250.13]",
  "temperature": 25.5,
  "calibrationFactor": 1.0000,
  "measuredAt": "2024-01-15T10:30:00Z",
  "measuredBy": "system",
  "organizationId": "org-001"
}
```

**Validation Errors (400):**
```json
{
  "error": "Validation failed",
  "details": [
    {
      "field": "grossWeight",
      "message": "Gross weight exceeds weighbridge capacity"
    },
    {
      "field": "vehicleId",
      "message": "Vehicle not found or inactive"
    }
  ]
}
```

### GET /measurements
Retrieve weight measurements with filtering and pagination.

**Query Parameters:**
- `weighbridgeId` (optional): Filter by weighbridge
- `vehicleId` (optional): Filter by vehicle
- `driverId` (optional): Filter by driver
- `measurementType` (optional): Filter by type (entry, exit, single)
- `status` (optional): Filter by status (active, void, corrected)
- `startDate` (optional): Start date filter (ISO 8601)
- `endDate` (optional): End date filter (ISO 8601)
- `page` (optional): Page number (default: 1)
- `pageSize` (optional): Records per page (default: 50, max: 200)
- `sortBy` (optional): Sort field (measuredAt, grossWeight, netWeight)
- `sortOrder` (optional): Sort direction (asc, desc, default: desc)

**Request:**
```
GET /api/v1/weight-data/measurements?weighbridgeId=wb-001&startDate=2024-01-01T00:00:00Z&endDate=2024-01-31T23:59:59Z&page=1&pageSize=50
```

**Response (200):**
```json
{
  "data": [
    {
      "id": 123,
      "weighbridgeId": "wb-001",
      "vehicleId": "v-12345",
      "driverId": "d-67890",
      "grossWeight": 45000.50,
      "tareWeight": 15000.00,
      "netWeight": 30000.50,
      "weightUnit": "kg",
      "measurementType": "entry",
      "measurementStatus": "active",
      "measuredAt": "2024-01-15T10:30:00Z",
      "measuredBy": "system"
    }
  ],
  "pagination": {
    "page": 1,
    "pageSize": 50,
    "totalRecords": 1250,
    "totalPages": 25,
    "hasNext": true,
    "hasPrevious": false
  },
  "filters": {
    "weighbridgeId": "wb-001",
    "startDate": "2024-01-01T00:00:00Z",
    "endDate": "2024-01-31T23:59:59Z"
  }
}
```

### GET /measurements/{id}
Retrieve a specific weight measurement by ID.

**Response (200):**
```json
{
  "id": 123,
  "weighbridgeId": "wb-001",
  "vehicleId": "v-12345",
  "driverId": "d-67890",
  "grossWeight": 45000.50,
  "tareWeight": 15000.00,
  "netWeight": 30000.50,
  "weightUnit": "kg",
  "measurementType": "entry",
  "measurementStatus": "active",
  "loadCellReadings": "[11250.12, 11250.13, 11250.12, 11250.13]",
  "temperature": 25.5,
  "calibrationFactor": 1.0000,
  "measuredAt": "2024-01-15T10:30:00Z",
  "measuredBy": "system",
  "organizationId": "org-001",
  "corrections": [
    {
      "id": 45,
      "correctedGrossWeight": 45001.00,
      "correctionReason": "Calibration adjustment",
      "correctedBy": "admin-user",
      "correctionTimestamp": "2024-01-15T11:00:00Z"
    }
  ]
}
```

### PUT /measurements/{id}/correct
Apply a correction to an existing weight measurement.

**Request:**
```json
{
  "correctedGrossWeight": 45001.00,
  "correctedTareWeight": 15000.50,
  "correctionReason": "Manual calibration adjustment after inspection",
  "correctionType": "manual"
}
```

**Response (200):**
```json
{
  "id": 45,
  "originalMeasurementId": 123,
  "correctedGrossWeight": 45001.00,
  "correctedTareWeight": 15000.50,
  "correctedNetWeight": 30000.50,
  "correctionReason": "Manual calibration adjustment after inspection",
  "correctionType": "manual",
  "correctedBy": "user-123",
  "authorizedBy": "admin-456",
  "correctionTimestamp": "2024-01-15T11:00:00Z",
  "originalValues": "{\"grossWeight\": 45000.50, \"tareWeight\": 15000.00, \"netWeight\": 30000.50}"
}
```

### DELETE /measurements/{id}
Void a weight measurement (soft delete).

**Response (200):**
```json
{
  "id": 123,
  "measurementStatus": "void",
  "voidedBy": "user-123",
  "voidedAt": "2024-01-15T12:00:00Z",
  "voidReason": "Hardware malfunction detected"
}
```

## Weighbridge Status

### GET /weighbridges/status
Get status of all weighbridges user has access to.

**Response (200):**
```json
{
  "data": [
    {
      "weighbridgeId": "wb-001",
      "isOperational": true,
      "currentLoad": 0,
      "maxCapacity": 80000,
      "lastCalibration": "2024-01-01T08:00:00Z",
      "calibrationDue": "2024-04-01T08:00:00Z",
      "maintenanceStatus": "ok",
      "temperature": 24.5,
      "humidity": 65.2,
      "firmwareVersion": "v2.1.3",
      "lastHeartbeat": "2024-01-15T10:29:45Z"
    }
  ]
}
```

### GET /weighbridges/{weighbridgeId}/status
Get status of specific weighbridge.

**Response (200):**
```json
{
  "weighbridgeId": "wb-001",
  "isOperational": true,
  "currentLoad": 15230.50,
  "maxCapacity": 80000,
  "lastCalibration": "2024-01-01T08:00:00Z",
  "calibrationDue": "2024-04-01T08:00:00Z",
  "maintenanceStatus": "ok",
  "temperature": 24.5,
  "humidity": 65.2,
  "firmwareVersion": "v2.1.3",
  "lastHeartbeat": "2024-01-15T10:29:45Z",
  "loadCellReadings": {
    "cell1": 3807.63,
    "cell2": 3807.62,
    "cell3": 3807.63,
    "cell4": 3807.62
  }
}
```

### PUT /weighbridges/{weighbridgeId}/status
Update weighbridge status (typically from hardware heartbeat).

**Request:**
```json
{
  "isOperational": true,
  "currentLoad": 15230.50,
  "temperature": 24.5,
  "humidity": 65.2,
  "loadCellReadings": {
    "cell1": 3807.63,
    "cell2": 3807.62,
    "cell3": 3807.63,
    "cell4": 3807.62
  },
  "firmwareVersion": "v2.1.3"
}
```

**Response (200):**
```json
{
  "weighbridgeId": "wb-001",
  "isOperational": true,
  "currentLoad": 15230.50,
  "temperature": 24.5,
  "humidity": 65.2,
  "lastHeartbeat": "2024-01-15T10:30:00Z",
  "updated": true
}
```

## Real-time Endpoints

### GET /weighbridges/{weighbridgeId}/live
Get real-time weight readings (WebSocket or Server-Sent Events).

**WebSocket Connection:**
```javascript
ws://api.domain.com/api/v1/weight-data/weighbridges/wb-001/live?token={jwt_token}
```

**Real-time Data Stream:**
```json
{
  "weighbridgeId": "wb-001",
  "timestamp": "2024-01-15T10:30:15.123Z",
  "currentLoad": 15230.50,
  "loadCellReadings": [3807.63, 3807.62, 3807.63, 3807.62],
  "temperature": 24.5,
  "isStable": true,
  "varianceThreshold": 0.1
}
```

## Analytics Endpoints

### GET /analytics/summary
Get weight measurement summary for dashboard.

**Query Parameters:**
- `period` (required): time period (today, week, month, quarter, year)
- `weighbridgeId` (optional): Filter by weighbridge
- `organizationId` (optional): Filter by organization

**Response (200):**
```json
{
  "period": "today",
  "summary": {
    "totalMeasurements": 156,
    "totalVehicles": 89,
    "totalWeight": 2456789.50,
    "averageWeight": 15748.65,
    "peakHour": {
      "hour": 14,
      "count": 23
    },
    "weighbridgeUtilization": {
      "wb-001": 78.5,
      "wb-002": 45.2
    }
  },
  "trends": {
    "measurementsGrowth": 12.5,
    "weightGrowth": 8.3,
    "utilizationChange": -2.1
  }
}
```

## Error Responses

### 400 Bad Request
```json
{
  "error": "Bad Request",
  "message": "Invalid input data",
  "details": [
    {
      "field": "grossWeight",
      "message": "Must be greater than 0"
    }
  ],
  "timestamp": "2024-01-15T10:30:00Z",
  "path": "/api/v1/weight-data/measurements"
}
```

### 401 Unauthorized
```json
{
  "error": "Unauthorized",
  "message": "Invalid or missing authentication token",
  "timestamp": "2024-01-15T10:30:00Z"
}
```

### 403 Forbidden
```json
{
  "error": "Forbidden",
  "message": "Insufficient permissions to access this weighbridge",
  "timestamp": "2024-01-15T10:30:00Z"
}
```

### 404 Not Found
```json
{
  "error": "Not Found",
  "message": "Weight measurement with ID 123 not found",
  "timestamp": "2024-01-15T10:30:00Z"
}
```

### 409 Conflict
```json
{
  "error": "Conflict",
  "message": "Vehicle is currently being weighed on another weighbridge",
  "timestamp": "2024-01-15T10:30:00Z"
}
```

### 422 Unprocessable Entity
```json
{
  "error": "Unprocessable Entity",
  "message": "Business rule validation failed",
  "details": [
    {
      "rule": "weighbridge_capacity",
      "message": "Weight exceeds maximum capacity of 80,000 kg"
    }
  ],
  "timestamp": "2024-01-15T10:30:00Z"
}
```

### 500 Internal Server Error
```json
{
  "error": "Internal Server Error",
  "message": "An unexpected error occurred",
  "errorId": "WDS-2024-0115-001",
  "timestamp": "2024-01-15T10:30:00Z"
}
```