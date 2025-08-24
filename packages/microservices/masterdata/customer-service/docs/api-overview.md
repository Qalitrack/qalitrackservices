# API Overview

The Customer Service API provides comprehensive customer relationship management capabilities through RESTful endpoints. All endpoints return JSON responses and support standard HTTP status codes.

## Base URL
```
https://api.qalitrack.com/customer-service
```

## Authentication
The service supports JWT-based authentication. Include the token in the Authorization header:
```
Authorization: Bearer <your-jwt-token>
```

## API Endpoints

### Customer Management

#### GET /api/customers
Retrieve a paginated list of customers.

**Query Parameters:**
- `page` (optional): Page number (default: 1)
- `pageSize` (optional): Number of items per page (default: 10, max: 100)
- `search` (optional): Search term for customer name or email

**Response:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "uuid",
        "name": "Customer Name",
        "email": "customer@example.com",
        "phone": "+1234567890",
        "address": "Customer Address",
        "createdAt": "2024-01-01T00:00:00Z",
        "updatedAt": "2024-01-01T00:00:00Z"
      }
    ],
    "totalCount": 100,
    "pageNumber": 1,
    "pageSize": 10
  },
  "message": "Customers retrieved successfully"
}
```

#### GET /api/customers/{id}
Retrieve a specific customer by ID.

**Response:**
```json
{
  "success": true,
  "data": {
    "id": "uuid",
    "name": "Customer Name",
    "email": "customer@example.com",
    "phone": "+1234567890",
    "address": "Customer Address",
    "contacts": [],
    "contracts": [],
    "orders": [],
    "createdAt": "2024-01-01T00:00:00Z",
    "updatedAt": "2024-01-01T00:00:00Z"
  },
  "message": "Customer retrieved successfully"
}
```

#### POST /api/customers
Create a new customer.

**Request Body:**
```json
{
  "name": "Customer Name",
  "email": "customer@example.com",
  "phone": "+1234567890",
  "address": "Customer Address"
}
```

#### PUT /api/customers/{id}
Update an existing customer.

**Request Body:**
```json
{
  "name": "Updated Customer Name",
  "email": "updated@example.com",
  "phone": "+0987654321",
  "address": "Updated Address"
}
```

#### DELETE /api/customers/{id}
Delete a customer by ID.

### Contact Management

#### GET /api/contacts
Retrieve all customer contacts.

#### POST /api/contacts
Create a new contact for a customer.

**Request Body:**
```json
{
  "customerId": "customer-uuid",
  "name": "Contact Name",
  "email": "contact@example.com",
  "phone": "+1234567890",
  "position": "Manager",
  "isPrimary": true
}
```

### Contract Management

#### GET /api/contracts
Retrieve all contracts with filtering options.

#### POST /api/contracts
Create a new contract.

**Request Body:**
```json
{
  "customerId": "customer-uuid",
  "contractNumber": "CTR-2024-001",
  "startDate": "2024-01-01",
  "endDate": "2024-12-31",
  "terms": "Contract terms and conditions",
  "value": 100000.00,
  "status": "Active"
}
```

### Order Management

#### GET /api/orders
Retrieve orders with filtering and pagination.

#### POST /api/orders
Create a new order.

**Request Body:**
```json
{
  "customerId": "customer-uuid",
  "contractId": "contract-uuid",
  "orderNumber": "ORD-2024-001",
  "description": "Order description",
  "quantity": 1000,
  "unitPrice": 50.00,
  "totalAmount": 50000.00,
  "status": "Pending"
}
```

## Error Responses

All endpoints return standardized error responses:

```json
{
  "success": false,
  "data": null,
  "message": "Error description",
  "errors": [
    "Detailed error information"
  ]
}
```

## Common HTTP Status Codes
- `200 OK`: Request successful
- `201 Created`: Resource created successfully
- `400 Bad Request`: Invalid request data
- `401 Unauthorized`: Authentication required
- `403 Forbidden`: Insufficient permissions
- `404 Not Found`: Resource not found
- `500 Internal Server Error`: Server error

## Rate Limiting
The API implements rate limiting to ensure fair usage:
- 1000 requests per hour per API key
- 10 requests per second burst limit

## Pagination
List endpoints support pagination with the following parameters:
- `page`: Page number (starting from 1)
- `pageSize`: Items per page (max 100)

Response includes pagination metadata:
```json
{
  "totalCount": 500,
  "pageNumber": 1,
  "pageSize": 10,
  "totalPages": 50,
  "hasPreviousPage": false,
  "hasNextPage": true
}
```