# API Guidelines

This document outlines the Django-style REST API conventions used throughout the QaliTrack Data Manager Service.

## General Principles

### RESTful Design
- Use HTTP methods appropriately (GET, POST, PUT, DELETE)
- Resource-oriented URLs with meaningful hierarchies
- Consistent response formats across all endpoints
- Proper HTTP status codes for all responses

### Django-Style Patterns
- Consistent pagination with `page` and `page_size` parameters
- Field-specific filtering with query parameters
- Ordering support with `ordering` parameter
- Standardized response structure with metadata

## URL Structure

### Base Patterns

```
GET    /[module]/[resource]                 # List with pagination
GET    /[module]/[resource]/{id}            # Get single item
POST   /[module]/[resource]                 # Create new item
PUT    /[module]/[resource]/{id}            # Update existing item
DELETE /[module]/[resource]/{id}            # Delete item
```

### Examples

```
GET    /weightdata/measurements             # List weight measurements
GET    /transactions                        # List transactions
GET    /compliance/violations               # List compliance violations
POST   /operations/alerts                   # Create new alert
PUT    /datasync/sessions/{id}              # Update sync session
DELETE /archive/policies/{id}               # Delete archive policy
```

## Pagination

All list endpoints support consistent pagination:

### Request Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| page | int | 1 | Page number (1-based) |
| page_size | int | 20 | Items per page (max 100) |

### Request Examples

```http
GET /transactions?page=2&page_size=50
GET /weightdata/measurements?page=1&page_size=10
```

### Response Format

```json
{
  "success": true,
  "data": [...],
  "pagination": {
    "page": 2,
    "page_size": 20,
    "total_count": 150,
    "total_pages": 8,
    "has_next": true,
    "has_previous": true
  }
}
```

## Filtering

### Query Parameters

Most list endpoints support field-specific filtering:

```http
GET /transactions?status=Completed&transaction_type=Inbound
GET /compliance/violations?severity=High&status=Open
GET /weightdata/measurements?weighbridge_id={guid}&quality_status=PASS
```

### Search Parameters

Many endpoints support text search across multiple fields:

```http
GET /transactions?search=truck123
GET /operations/alerts?search=maintenance
```

### Date Range Filtering

Date fields typically support range filtering:

```http
GET /weightdata/measurements?start_date=2024-01-01&end_date=2024-01-31
GET /transactions?planned_date_after=2024-01-15&planned_date_before=2024-02-15
```

## Ordering

### Parameter Format

Use the `ordering` parameter with field names:

```http
GET /transactions?ordering=planned_date        # Ascending
GET /transactions?ordering=-planned_date       # Descending (prefix with -)
```

### Multiple Fields

Separate multiple fields with commas:

```http
GET /transactions?ordering=-priority,planned_date
```

### Examples

```http
GET /weightdata/measurements?ordering=-measurement_time
GET /compliance/violations?ordering=severity,-detected_at
GET /operations/alerts?ordering=-alert_time
```

## Response Format

### Success Responses

All successful responses follow this structure:

```json
{
  "success": true,
  "data": [object or array],
  "message": "Optional success message",
  "pagination": {...}  // Only for list endpoints
}
```

### Single Item Response

```json
{
  "success": true,
  "data": {
    "id": "guid",
    "field1": "value1",
    "field2": "value2",
    "created_at": "2024-01-15T10:30:00Z",
    "updated_at": "2024-01-15T10:30:00Z"
  }
}
```

### List Response

```json
{
  "success": true,
  "data": [
    {
      "id": "guid1",
      "field1": "value1"
    },
    {
      "id": "guid2",
      "field1": "value2"
    }
  ],
  "pagination": {
    "page": 1,
    "page_size": 20,
    "total_count": 45,
    "total_pages": 3,
    "has_next": true,
    "has_previous": false
  }
}
```

### Error Responses

Error responses use appropriate HTTP status codes:

#### Validation Error (400)

```json
{
  "success": false,
  "error": "Validation failed",
  "details": {
    "field1": ["This field is required"],
    "field2": ["Invalid value provided"]
  }
}
```

#### Not Found (404)

```json
{
  "success": false,
  "error": "Resource not found",
  "message": "Transaction with ID {guid} not found"
}
```

#### Server Error (500)

```json
{
  "success": false,
  "error": "Internal server error",
  "message": "An unexpected error occurred"
}
```

## Authentication & Authorization

### Headers

All requests require authentication headers:

```http
Authorization: Bearer <jwt-token>
X-Organization-Id: <organization-guid>  // Optional if in JWT claims
```

### Multi-Tenancy

Data is automatically filtered by organization. The organization ID is extracted from:

1. JWT token claims (`org_id` claim)
2. `X-Organization-Id` request header
3. Query parameter `organization_id` (for system-level operations)

## Status Codes

### Standard HTTP Status Codes

| Code | Meaning | Usage |
|------|---------|-------|
| 200 | OK | Successful GET, PUT requests |
| 201 | Created | Successful POST requests |
| 204 | No Content | Successful DELETE requests |
| 400 | Bad Request | Validation errors, malformed requests |
| 401 | Unauthorized | Missing or invalid authentication |
| 403 | Forbidden | Insufficient permissions |
| 404 | Not Found | Resource does not exist |
| 409 | Conflict | Business logic conflicts |
| 422 | Unprocessable Entity | Semantic validation errors |
| 500 | Internal Server Error | Unexpected server errors |

## Field Naming Conventions

### JSON Field Names
- Use snake_case for consistency with Django patterns
- Dates and times in ISO 8601 format with UTC timezone
- GUIDs as strings in standard format

### Examples

```json
{
  "transaction_number": "TXN-2024-001",
  "planned_date": "2024-01-15T14:30:00Z",
  "vehicle_id": "550e8400-e29b-41d4-a716-446655440000",
  "organization_id": "6ba7b810-9dad-11d1-80b4-00c04fd430c8",
  "is_active": true,
  "created_at": "2024-01-15T10:30:00Z",
  "updated_at": "2024-01-15T10:35:00Z"
}
```

## Bulk Operations

### Bulk Creation

Some endpoints support bulk creation:

```http
POST /transactions/bulk
Content-Type: application/json

{
  "transactions": [
    {"transaction_number": "TXN-001", ...},
    {"transaction_number": "TXN-002", ...}
  ]
}
```

### Bulk Updates

```http
PUT /compliance/violations/bulk
Content-Type: application/json

{
  "updates": [
    {"id": "guid1", "status": "Resolved"},
    {"id": "guid2", "status": "Dismissed"}
  ]
}
```

## Real-Time Endpoints

### Server-Sent Events (SSE)

Real-time data streaming using SSE:

```http
GET /weightdata/realtime/stream
Accept: text/event-stream

# Response
data: {"type": "measurement", "data": {...}}

data: {"type": "heartbeat", "timestamp": "2024-01-15T10:30:00Z"}
```

### WebSocket Endpoints

Some modules provide WebSocket endpoints for bidirectional communication:

```javascript
// JavaScript example
const ws = new WebSocket('wss://api.example.com/datasync/realtime');
ws.onmessage = (event) => {
  const data = JSON.parse(event.data);
  // Handle real-time sync updates
};
```

## Versioning

### URL Versioning

API versions are specified in the URL:

```http
GET /v1/transactions
GET /v2/transactions  # Future version
```

### Header Versioning

Alternative versioning via headers:

```http
GET /transactions
Accept: application/vnd.qalitrack.v1+json
```

## Rate Limiting

### Headers

Rate limit information is provided in response headers:

```http
X-RateLimit-Limit: 1000
X-RateLimit-Remaining: 999
X-RateLimit-Reset: 1642262400
```

### Limits

- Default: 1000 requests per hour per organization
- Burst: 100 requests per minute
- Real-time endpoints: 10 concurrent connections

## Caching

### ETags

Responses include ETags for cache validation:

```http
ETag: "550e8400-e29b-41d4-a716-446655440000"
```

### Cache-Control Headers

```http
Cache-Control: public, max-age=300  # Cache for 5 minutes
Cache-Control: no-cache            # Always validate
Cache-Control: private            # User-specific data
```

## CORS

### Allowed Origins

CORS is configured to allow requests from:
- `https://*.qalitrack.com`
- `https://localhost:3000` (development)
- `https://localhost:4200` (development)

### Allowed Headers

```
Authorization, Content-Type, X-Organization-Id, X-Requested-With
```

### Allowed Methods

```
GET, POST, PUT, DELETE, OPTIONS, HEAD
```

## Documentation

### OpenAPI/Swagger

Complete API documentation is available at:
- Swagger UI: `https://api.example.com/` (root path)
- OpenAPI JSON: `https://api.example.com/swagger/v1/swagger.json`

### Interactive Testing

The Swagger UI provides:
- Complete endpoint documentation
- Interactive request/response testing
- Code examples in multiple languages
- Schema documentation for all models