# Weight Data Service API Documentation

## Authentication & Headers

All API requests require the following headers:

```http
X-Organization-Id: {organization-id}
X-User-Id: {user-id}
```

## Response Format

All API responses follow a consistent format:

```json
{
  "success": true,
  "message": "Operation completed successfully",
  "data": { /* response data */ },
  "errors": []
}
```

## Error Handling

### HTTP Status Codes

- `200 OK` - Successful operation
- `201 Created` - Resource created successfully
- `400 Bad Request` - Invalid request data
- `403 Forbidden` - Insufficient permissions
- `404 Not Found` - Resource not found
- `500 Internal Server Error` - Server error

### Error Response

```json
{
  "success": false,
  "message": "Error description",
  "data": null,
  "errors": ["Detailed error message"]
}
```

## Weight Measurements API

### GET /api/measurements

Get paginated list of weight measurements with optional filtering.

**Query Parameters:**
- `page` (int): Page number (default: 1)
- `pageSize` (int): Items per page (default: 50)
- `searchTerm` (string): Search in vehicle registration, ticket reference, or customer reference
- `fromDate` (datetime): Filter measurements from this date
- `toDate` (datetime): Filter measurements to this date
- `sortBy` (string): Sort field
- `sortDescending` (bool): Sort direction (default: true)

**Response:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "guid",
        "weighbridgeId": "WB001",
        "vehicleRegistration": "ABC123",
        "driverId": "D001",
        "weight": 15000.50,
        "type": "In",
        "status": "Pending",
        "measurementDateTime": "2024-01-15T10:30:00Z",
        "ticketReference": "T001",
        "notes": "Initial weighing",
        "organizationId": "org1",
        "tareWeight": 2000.00,
        "netWeight": 13000.50,
        "productType": "Coal",
        "customerReference": "CUST001",
        "createdAt": "2024-01-15T10:30:00Z",
        "createdBy": "user1",
        "corrections": []
      }
    ],
    "totalCount": 150,
    "page": 1,
    "pageSize": 50,
    "totalPages": 3,
    "hasNextPage": true,
    "hasPreviousPage": false
  }
}
```

### GET /api/measurements/{id}

Get specific weight measurement by ID.

**Response:**
```json
{
  "success": true,
  "data": {
    "id": "guid",
    "weighbridgeId": "WB001",
    "vehicleRegistration": "ABC123",
    // ... other measurement fields
    "corrections": [
      {
        "id": "guid",
        "originalWeight": 15000.50,
        "correctedWeight": 15100.00,
        "reason": "Scale calibration adjustment",
        "authorizedBy": "supervisor1",
        "correctionDateTime": "2024-01-15T11:00:00Z",
        "isApproved": true,
        "approvedBy": "manager1",
        "approvalDateTime": "2024-01-15T11:30:00Z"
      }
    ]
  }
}
```

### POST /api/measurements

Create new weight measurement.

**Request Body:**
```json
{
  "weighbridgeId": "WB001",
  "vehicleRegistration": "ABC123",
  "driverId": "D001",
  "weight": 15000.50,
  "type": "In",
  "ticketReference": "T001",
  "notes": "Initial weighing",
  "tareWeight": 2000.00,
  "productType": "Coal",
  "customerReference": "CUST001"
}
```

**Response:** Returns created measurement with generated ID and timestamps.

### PUT /api/measurements/{id}

Update existing weight measurement.

**Request Body:**
```json
{
  "weight": 15100.00,
  "status": "Validated",
  "notes": "Updated after verification",
  "netWeight": 13100.00
}
```

**Response:** Returns updated measurement.

### DELETE /api/measurements/{id}

Soft delete weight measurement (marks as deleted).

**Response:**
```json
{
  "success": true,
  "data": true,
  "message": "Measurement deleted successfully"
}
```

## Weighbridge Status API

### GET /api/weighbridges

Get all weighbridges for the organization.

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": "guid",
      "weighbridgeId": "WB001",
      "name": "Main Gate Weighbridge",
      "location": "North Entrance",
      "status": "Active",
      "maxCapacity": 50000.00,
      "minCapacity": 0.00,
      "currentWeight": 15000.50,
      "lastCalibrationDate": "2024-01-01T00:00:00Z",
      "nextCalibrationDate": "2024-07-01T00:00:00Z",
      "isOnline": true,
      "serialNumber": "WB001-2024",
      "manufacturer": "ACME Scales",
      "model": "AS-5000",
      "accuracyTolerance": 0.1
    }
  ]
}
```

### PUT /api/weighbridges/{weighbridgeId}/status

Update weighbridge status.

**Request Body:**
```json
{
  "status": "Maintenance",
  "maintenanceNotes": "Scheduled calibration in progress",
  "isOnline": false
}
```

## Weight Corrections API

### POST /api/corrections

Create weight correction.

**Request Body:**
```json
{
  "weightMeasurementId": "guid",
  "correctedWeight": 15100.00,
  "reason": "Scale calibration adjustment"
}
```

**Response:** Returns created correction pending approval.

### PUT /api/corrections/{correctionId}/approve

Approve or reject weight correction.

**Request Body:**
```json
{
  "isApproved": true,
  "approvalNotes": "Correction approved after verification"
}
```

## Analytics API

### GET /api/analytics/summary

Get comprehensive analytics summary.

**Query Parameters:**
- `fromDate` (datetime): Start date for analysis
- `toDate` (datetime): End date for analysis
- `weighbridgeId` (string): Filter by specific weighbridge
- `includeWeighbridgeUsage` (bool): Include weighbridge usage breakdown
- `includeDailyBreakdown` (bool): Include daily weight breakdown

**Response:**
```json
{
  "success": true,
  "data": {
    "totalMeasurements": 1250,
    "pendingMeasurements": 15,
    "validatedMeasurements": 1200,
    "correctedMeasurements": 35,
    "totalWeight": 18750000.00,
    "averageWeight": 15000.00,
    "activeWeighbridges": 3,
    "maintenanceWeighbridges": 1,
    "reportDate": "2024-01-15T12:00:00Z",
    "weighbridgeUsage": [
      {
        "weighbridgeId": "WB001",
        "name": "Main Gate",
        "measurementCount": 750,
        "totalWeight": 11250000.00,
        "averageWeight": 15000.00
      }
    ],
    "dailyWeights": [
      {
        "date": "2024-01-15",
        "measurementCount": 85,
        "totalWeight": 1275000.00,
        "averageWeight": 15000.00
      }
    ]
  }
}
```

## Validation Rules

### Weight Measurement Creation

- `weighbridgeId`: Required, max 50 characters
- `vehicleRegistration`: Required, max 20 characters
- `weight`: Must be greater than 0, less than 1,000,000 kg
- `type`: Must be valid enum value (In, Out, Single, Tare)
- `tareWeight`: Must be >= 0 if provided
- `ticketReference`: Max 50 characters if provided
- `notes`: Max 500 characters if provided

### Weight Correction

- `weightMeasurementId`: Required, must be valid GUID
- `correctedWeight`: Must be greater than 0, less than 1,000,000 kg
- `reason`: Required, max 500 characters

## Business Rules

### Weight Measurements

1. Weight must be within weighbridge capacity limits
2. Weighbridge must be active and online
3. Vehicle registration is validated against master data
4. Net weight is automatically calculated when tare weight is provided

### Weight Corrections

1. Only pending or validated measurements can be corrected
2. Correction must be approved by a different user than the one who created it
3. Approved corrections update the original measurement weight
4. Rejected corrections revert to original weight

### Weighbridge Status

1. Weighbridge capacity cannot be modified once measurements exist
2. Taking a weighbridge offline prevents new measurements
3. Calibration dates are tracked for compliance

## Rate Limiting

Current implementation does not include rate limiting, but consider implementing:

- Per-user: 100 requests per minute
- Per-organization: 1000 requests per minute
- Global: 10,000 requests per minute

## Pagination

All list endpoints support pagination:

- Default page size: 50
- Maximum page size: 100
- Page numbers start at 1

## Caching

Consider implementing caching for:

- Weighbridge status (5 minutes)
- Analytics summaries (15 minutes)
- Configuration data (1 hour)

## Webhooks (Future Enhancement)

Planned webhook events:

- `measurement.created`
- `measurement.corrected`
- `weighbridge.offline`
- `calibration.due`