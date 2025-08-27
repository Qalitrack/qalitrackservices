# API Overview

The **QaliTrack Master Data Service** provides a comprehensive REST API for managing all master data entities in the QaliTrack ecosystem. This consolidated service replaces 10+ individual microservices with a unified, efficient solution.

## Base URL

```
http://localhost:7001/api/masterdata
```

## Authentication

All API endpoints require JWT authentication via the `Authorization` header:

```http
Authorization: Bearer <jwt-token>
```

## Response Format

All API responses follow a consistent format:

```json
{
  "success": true,
  "data": { ... },
  "message": "Optional message",
  "pagination": { ... } // For paginated responses
}
```

## HTTP Methods

All modules support the complete range of HTTP methods:

| Method | Purpose | Example |
|--------|---------|---------|
| `GET` | Retrieve entities | `GET /api/masterdata/vehicles` |
| `POST` | Create new entity | `POST /api/masterdata/vehicles` |
| `PUT` | Full update | `PUT /api/masterdata/vehicles/123` |
| `PATCH` | Partial update | `PATCH /api/masterdata/vehicles/123` |
| `DELETE` | Remove entity | `DELETE /api/masterdata/vehicles/123` |
| `HEAD` | Check existence | `HEAD /api/masterdata/vehicles/123` |
| `OPTIONS` | Get allowed methods | `OPTIONS /api/masterdata/vehicles` |

## Module Endpoints

### Business Entities (`/business-entities`)
Manages customers, suppliers, and transporters with unified business entity profiles.

```http
GET    /api/masterdata/business-entities           # List all entities
GET    /api/masterdata/business-entities/123       # Get specific entity
POST   /api/masterdata/business-entities           # Create entity
PUT    /api/masterdata/business-entities/123       # Update entity
DELETE /api/masterdata/business-entities/123       # Delete entity

# Sub-resources
GET    /api/masterdata/business-entities/123/contacts          # Entity contacts
GET    /api/masterdata/business-entities/123/customer-profile  # Customer profile
GET    /api/masterdata/business-entities/123/supplier-profile  # Supplier profile
GET    /api/masterdata/business-entities/123/transporter-profile # Transporter profile
```

### Vehicles (`/vehicles`)
Complete fleet management with maintenance, insurance, and compliance tracking.

```http
GET    /api/masterdata/vehicles                    # List vehicles
GET    /api/masterdata/vehicles/123                # Get vehicle
POST   /api/masterdata/vehicles                    # Register vehicle
PUT    /api/masterdata/vehicles/123                # Update vehicle

# Sub-resources
GET    /api/masterdata/vehicles/123/registrations  # Vehicle registrations
GET    /api/masterdata/vehicles/123/insurance      # Insurance policies
GET    /api/masterdata/vehicles/123/maintenance    # Maintenance records
GET    /api/masterdata/vehicles/123/inspections    # Inspection history
GET    /api/masterdata/vehicles/123/documents      # Vehicle documents
```

### Drivers (`/drivers`)
Personnel management with licensing, training, and performance tracking.

```http
GET    /api/masterdata/drivers                     # List drivers
GET    /api/masterdata/drivers/123                 # Get driver
POST   /api/masterdata/drivers                     # Create driver
PUT    /api/masterdata/drivers/123                 # Update driver

# Sub-resources
GET    /api/masterdata/drivers/123/licenses        # Driver licenses
GET    /api/masterdata/drivers/123/training        # Training records
GET    /api/masterdata/drivers/123/medical         # Medical records
GET    /api/masterdata/drivers/123/documents       # Driver documents
```

### Other Modules

- **Routes** (`/routes`) - Route planning and logistics management
- **Products** (`/products`) - Product catalog and specifications  
- **Weighbridges** (`/weighbridges`) - Equipment management
- **SACCOs** (`/saccos`) - Financial cooperative integration
- **Organizations** (`/organizations`) - Multi-tenant structure
- **Relationships** (`/relationships`) - Cross-module associations

## Query Parameters

### Pagination

```http
GET /api/masterdata/vehicles?page=1&pageSize=20&sortBy=registrationDate&sortOrder=desc
```

### Filtering

```http
GET /api/masterdata/vehicles?status=active&vehicleType=truck&organizationId=123
```

### Search

```http
GET /api/masterdata/drivers?search=john&searchFields=firstName,lastName,licenseNumber
```

## Error Handling

The API returns consistent error responses:

```json
{
  "success": false,
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Invalid input data",
    "details": [
      {
        "field": "email",
        "message": "Invalid email format"
      }
    ]
  }
}
```

Common HTTP status codes:

- `200` - Success
- `201` - Created
- `204` - No Content (for successful DELETE)
- `400` - Bad Request
- `401` - Unauthorized
- `403` - Forbidden
- `404` - Not Found
- `409` - Conflict
- `422` - Unprocessable Entity
- `500` - Internal Server Error

## Rate Limiting

API requests are rate-limited to prevent abuse:

- **Standard users**: 1000 requests per hour
- **System integrations**: 10000 requests per hour

Rate limit headers are included in responses:

```http
X-RateLimit-Limit: 1000
X-RateLimit-Remaining: 999
X-RateLimit-Reset: 1640995200
```

## Interactive Documentation

For detailed API exploration and testing, visit:

- **Swagger UI**: http://localhost:7001/ 
- **API Reference**: http://localhost:7001/docs/api/