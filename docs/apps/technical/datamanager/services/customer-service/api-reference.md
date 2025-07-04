# Customer Service API Reference

## Base Information

- **Base URL**: `http://localhost:7003/api` (Development)
- **Content Type**: `application/json`
- **Authentication**: Organization-based headers
- **API Version**: v1

## Authentication

All endpoints require the following headers:

```http
X-Organization-Id: {organization-id}
X-User-Id: {user-id}
Content-Type: application/json
```

## Response Format

All API responses follow a consistent format:

```json
{
  "success": true,
  "message": "Operation completed successfully",
  "data": { /* Response data */ },
  "errors": null,
  "timestamp": "2024-07-04T10:30:00.000Z"
}
```

## Customer Management Endpoints

### GET /api/customers

Retrieve customers with pagination and optional search functionality.

**Parameters:**
- `page` (query, integer, optional): Page number (default: 1)
- `pageSize` (query, integer, optional): Items per page (default: 20)
- `search` (query, string, optional): Search term for name, email, or tax number

**Example Request:**
```http
GET /api/customers?page=1&pageSize=10&search=acme
X-Organization-Id: org-123
X-User-Id: user-456
```

**Example Response:**
```json
{
  "success": true,
  "message": "Customers retrieved successfully",
  "data": [
    {
      "id": "customer-uuid-123",
      "name": "Acme Corporation",
      "taxNumber": "TAX123456",
      "registrationNumber": "REG789012",
      "contactEmail": "billing@acme.com",
      "contactPhone": "+1-555-0123",
      "billingAddress": "123 Business St, City, State 12345",
      "customerType": "Corporate",
      "creditLimit": 50000.00,
      "status": "Active",
      "notes": "Key enterprise customer",
      "createdAt": "2024-01-15T08:30:00.000Z",
      "updatedAt": "2024-06-20T14:45:00.000Z"
    }
  ],
  "errors": null,
  "timestamp": "2024-07-04T10:30:00.000Z"
}
```

---

### POST /api/customers

Register a new customer in the system.

**Request Body:**
```json
{
  "name": "New Customer Corp",
  "taxNumber": "TAX987654",
  "registrationNumber": "REG456789",
  "contactEmail": "contact@newcustomer.com",
  "contactPhone": "+1-555-0199",
  "billingAddress": "456 Corporate Ave, Business City, State 54321",
  "customerType": "Corporate",
  "creditLimit": 25000.00,
  "notes": "New enterprise customer - high volume expected"
}
```

**Example Response (201 Created):**
```json
{
  "success": true,
  "message": "Customer registered successfully",
  "data": {
    "id": "customer-uuid-789",
    "name": "New Customer Corp",
    "taxNumber": "TAX987654",
    "registrationNumber": "REG456789",
    "contactEmail": "contact@newcustomer.com",
    "contactPhone": "+1-555-0199",
    "billingAddress": "456 Corporate Ave, Business City, State 54321",
    "customerType": "Corporate",
    "creditLimit": 25000.00,
    "status": "Active",
    "notes": "New enterprise customer - high volume expected",
    "createdAt": "2024-07-04T10:30:00.000Z",
    "updatedAt": "2024-07-04T10:30:00.000Z"
  },
  "errors": null,
  "timestamp": "2024-07-04T10:30:00.000Z"
}
```

**Error Response (400 Bad Request):**
```json
{
  "success": false,
  "message": "Validation failed",
  "data": null,
  "errors": [
    "Name is required",
    "Contact email must be valid",
    "Credit limit must be greater than 0"
  ],
  "timestamp": "2024-07-04T10:30:00.000Z"
}
```

---

### GET /api/customers/{id}

Retrieve a specific customer by ID.

**Parameters:**
- `id` (path, string, required): Customer ID

**Example Request:**
```http
GET /api/customers/customer-uuid-123
X-Organization-Id: org-123
X-User-Id: user-456
```

**Example Response:**
```json
{
  "success": true,
  "message": "Customer retrieved successfully",
  "data": {
    "id": "customer-uuid-123",
    "name": "Acme Corporation",
    "taxNumber": "TAX123456",
    "registrationNumber": "REG789012",
    "contactEmail": "billing@acme.com",
    "contactPhone": "+1-555-0123",
    "billingAddress": "123 Business St, City, State 12345",
    "customerType": "Corporate",
    "creditLimit": 50000.00,
    "status": "Active",
    "notes": "Key enterprise customer",
    "createdAt": "2024-01-15T08:30:00.000Z",
    "updatedAt": "2024-06-20T14:45:00.000Z"
  },
  "errors": null,
  "timestamp": "2024-07-04T10:30:00.000Z"
}
```

**Error Response (404 Not Found):**
```json
{
  "success": false,
  "message": "Customer with ID customer-uuid-999 not found",
  "data": null,
  "errors": null,
  "timestamp": "2024-07-04T10:30:00.000Z"
}
```

---

### PUT /api/customers/{id}

Update an existing customer's information.

**Parameters:**
- `id` (path, string, required): Customer ID

**Request Body:**
```json
{
  "name": "Updated Company Name",
  "taxNumber": "TAX123456",
  "registrationNumber": "REG789012",
  "contactEmail": "newcontact@acme.com",
  "contactPhone": "+1-555-0124",
  "billingAddress": "123 Business St, Suite 200, City, State 12345",
  "customerType": "Corporate",
  "creditLimit": 75000.00,
  "status": "Active",
  "notes": "Updated customer information - increased credit limit"
}
```

**Example Response:**
```json
{
  "success": true,
  "message": "Customer updated successfully",
  "data": {
    "id": "customer-uuid-123",
    "name": "Updated Company Name",
    "taxNumber": "TAX123456",
    "registrationNumber": "REG789012",
    "contactEmail": "newcontact@acme.com",
    "contactPhone": "+1-555-0124",
    "billingAddress": "123 Business St, Suite 200, City, State 12345",
    "customerType": "Corporate",
    "creditLimit": 75000.00,
    "status": "Active",
    "notes": "Updated customer information - increased credit limit",
    "createdAt": "2024-01-15T08:30:00.000Z",
    "updatedAt": "2024-07-04T10:30:00.000Z"
  },
  "errors": null,
  "timestamp": "2024-07-04T10:30:00.000Z"
}
```

---

### DELETE /api/customers/{id}

Delete a customer from the system.

**Parameters:**
- `id` (path, string, required): Customer ID

**Example Request:**
```http
DELETE /api/customers/customer-uuid-123
X-Organization-Id: org-123
X-User-Id: user-456
```

**Example Response (200 OK):**
```json
{
  "success": true,
  "message": "Customer deleted successfully",
  "data": null,
  "errors": null,
  "timestamp": "2024-07-04T10:30:00.000Z"
}
```

---

### POST /api/customers/{id}/activate

Activate a customer account.

**Parameters:**
- `id` (path, string, required): Customer ID

**Example Response:**
```json
{
  "success": true,
  "message": "Customer activated successfully",
  "data": null,
  "errors": null,
  "timestamp": "2024-07-04T10:30:00.000Z"
}
```

---

### POST /api/customers/{id}/deactivate

Deactivate a customer account.

**Parameters:**
- `id` (path, string, required): Customer ID

**Example Response:**
```json
{
  "success": true,
  "message": "Customer deactivated successfully",
  "data": null,
  "errors": null,
  "timestamp": "2024-07-04T10:30:00.000Z"
}
```

---

### GET /api/customers/{id}/details

Retrieve comprehensive customer details including contacts, contracts, billing, and credit information.

**Parameters:**
- `id` (path, string, required): Customer ID

**Example Response:**
```json
{
  "success": true,
  "message": "Customer details retrieved successfully",
  "data": {
    "id": "customer-uuid-123",
    "name": "Acme Corporation",
    "taxNumber": "TAX123456",
    "registrationNumber": "REG789012",
    "contactEmail": "billing@acme.com",
    "contactPhone": "+1-555-0123",
    "billingAddress": "123 Business St, City, State 12345",
    "customerType": "Corporate",
    "creditLimit": 50000.00,
    "status": "Active",
    "notes": "Key enterprise customer",
    "createdAt": "2024-01-15T08:30:00.000Z",
    "updatedAt": "2024-06-20T14:45:00.000Z",
    "contacts": [
      {
        "id": "contact-uuid-456",
        "customerId": "customer-uuid-123",
        "firstName": "John",
        "lastName": "Smith",
        "email": "j.smith@acme.com",
        "phone": "+1-555-0123",
        "mobile": "+1-555-0124",
        "position": "Operations Manager",
        "department": "Operations",
        "contactType": "Business",
        "isPrimary": true,
        "isActive": true,
        "createdAt": "2024-01-15T08:30:00.000Z",
        "updatedAt": "2024-01-15T08:30:00.000Z"
      }
    ],
    "contracts": [
      {
        "id": "contract-uuid-789",
        "customerId": "customer-uuid-123",
        "contractNumber": "CON-2024-001",
        "title": "Annual Service Agreement",
        "description": "Comprehensive weighbridge services",
        "startDate": "2024-01-01T00:00:00.000Z",
        "endDate": "2024-12-31T23:59:59.999Z",
        "contractValue": 120000.00,
        "status": "Active",
        "contractType": "Service",
        "terms": "Net 30 payment terms",
        "signedByCustomer": "John Smith",
        "signedByCompany": "QaliTrack Representative",
        "signedDate": "2023-12-15T10:00:00.000Z",
        "autoRenew": true,
        "renewalPeriodMonths": 12,
        "createdAt": "2023-12-10T08:30:00.000Z",
        "updatedAt": "2023-12-15T10:00:00.000Z"
      }
    ],
    "locations": [],
    "billing": {
      "id": "billing-uuid-321",
      "customerId": "customer-uuid-123",
      "billingContactName": "Jane Doe",
      "billingEmail": "billing@acme.com",
      "billingPhone": "+1-555-0125",
      "billingAddress": "123 Business St, Accounting Dept",
      "billingCity": "Business City",
      "billingState": "State",
      "billingCountry": "USA",
      "billingPostalCode": "12345",
      "preferredPaymentMethod": "NET30",
      "taxExemptNumber": null,
      "currency": "USD",
      "discountPercentage": 5.00,
      "paymentTermsDays": 30,
      "invoiceDeliveryMethod": "Email",
      "createdAt": "2024-01-15T08:30:00.000Z",
      "updatedAt": "2024-01-15T08:30:00.000Z"
    },
    "credit": {
      "id": "credit-uuid-654",
      "customerId": "customer-uuid-123",
      "creditLimit": 50000.00,
      "availableCredit": 35000.00,
      "usedCredit": 15000.00,
      "creditTerms": "Net 30 payment terms with 2% early payment discount",
      "creditRating": "Excellent",
      "lastCreditReview": "2024-01-01T00:00:00.000Z",
      "nextCreditReview": "2025-01-01T00:00:00.000Z",
      "securityDeposit": 0.00,
      "creditReference1": "Reference Bank 1",
      "creditReference2": "Supplier Reference 1",
      "creditReference3": "Trade Reference 1",
      "notes": "Excellent payment history, approved for increased limit",
      "createdAt": "2024-01-15T08:30:00.000Z",
      "updatedAt": "2024-06-01T08:30:00.000Z"
    },
    "preferences": null
  },
  "errors": null,
  "timestamp": "2024-07-04T10:30:00.000Z"
}
```

## Contact Management Endpoints

### GET /api/customers/{id}/contacts

Retrieve all contacts for a specific customer.

**Parameters:**
- `id` (path, string, required): Customer ID

**Example Response:**
```json
{
  "success": true,
  "message": "Customer contacts retrieved successfully",
  "data": [
    {
      "id": "contact-uuid-456",
      "customerId": "customer-uuid-123",
      "firstName": "John",
      "lastName": "Smith",
      "email": "j.smith@acme.com",
      "phone": "+1-555-0123",
      "mobile": "+1-555-0124",
      "position": "Operations Manager",
      "department": "Operations",
      "contactType": "Business",
      "isPrimary": true,
      "isActive": true,
      "createdAt": "2024-01-15T08:30:00.000Z",
      "updatedAt": "2024-01-15T08:30:00.000Z"
    }
  ],
  "errors": null,
  "timestamp": "2024-07-04T10:30:00.000Z"
}
```

---

### POST /api/customers/{id}/contacts

Add a new contact to a customer.

**Parameters:**
- `id` (path, string, required): Customer ID

**Request Body:**
```json
{
  "firstName": "Sarah",
  "lastName": "Johnson",
  "email": "s.johnson@acme.com",
  "phone": "+1-555-0126",
  "mobile": "+1-555-0127",
  "position": "Technical Manager",
  "department": "Engineering",
  "contactType": "Technical",
  "isPrimary": false
}
```

**Example Response (201 Created):**
```json
{
  "success": true,
  "message": "Contact added successfully",
  "data": {
    "id": "contact-uuid-789",
    "customerId": "customer-uuid-123",
    "firstName": "Sarah",
    "lastName": "Johnson",
    "email": "s.johnson@acme.com",
    "phone": "+1-555-0126",
    "mobile": "+1-555-0127",
    "position": "Technical Manager",
    "department": "Engineering",
    "contactType": "Technical",
    "isPrimary": false,
    "isActive": true,
    "createdAt": "2024-07-04T10:30:00.000Z",
    "updatedAt": "2024-07-04T10:30:00.000Z"
  },
  "errors": null,
  "timestamp": "2024-07-04T10:30:00.000Z"
}
```

---

### PUT /api/contacts/{id}

Update an existing contact's information.

**Parameters:**
- `id` (path, string, required): Contact ID

**Request Body:**
```json
{
  "firstName": "Sarah",
  "lastName": "Johnson-Williams",
  "email": "s.johnson-williams@acme.com",
  "phone": "+1-555-0126",
  "mobile": "+1-555-0127",
  "position": "Senior Technical Manager",
  "department": "Engineering",
  "contactType": "Technical",
  "isPrimary": false,
  "isActive": true
}
```

**Example Response:**
```json
{
  "success": true,
  "message": "Contact updated successfully",
  "data": {
    "id": "contact-uuid-789",
    "customerId": "customer-uuid-123",
    "firstName": "Sarah",
    "lastName": "Johnson-Williams",
    "email": "s.johnson-williams@acme.com",
    "phone": "+1-555-0126",
    "mobile": "+1-555-0127",
    "position": "Senior Technical Manager",
    "department": "Engineering",
    "contactType": "Technical",
    "isPrimary": false,
    "isActive": true,
    "createdAt": "2024-07-04T10:30:00.000Z",
    "updatedAt": "2024-07-04T11:15:00.000Z"
  },
  "errors": null,
  "timestamp": "2024-07-04T11:15:00.000Z"
}
```

---

### DELETE /api/contacts/{id}

Remove a contact from the system.

**Parameters:**
- `id` (path, string, required): Contact ID

**Example Response:**
```json
{
  "success": true,
  "message": "Contact deleted successfully",
  "data": null,
  "errors": null,
  "timestamp": "2024-07-04T10:30:00.000Z"
}
```

## Contract Management Endpoints

### GET /api/customers/{id}/contracts

Retrieve all contracts for a specific customer.

**Parameters:**
- `id` (path, string, required): Customer ID

**Example Response:**
```json
{
  "success": true,
  "message": "Customer contracts retrieved successfully",
  "data": [
    {
      "id": "contract-uuid-789",
      "customerId": "customer-uuid-123",
      "contractNumber": "CON-2024-001",
      "title": "Annual Service Agreement",
      "description": "Comprehensive weighbridge services",
      "startDate": "2024-01-01T00:00:00.000Z",
      "endDate": "2024-12-31T23:59:59.999Z",
      "contractValue": 120000.00,
      "status": "Active",
      "contractType": "Service",
      "terms": "Net 30 payment terms",
      "signedByCustomer": "John Smith",
      "signedByCompany": "QaliTrack Representative",
      "signedDate": "2023-12-15T10:00:00.000Z",
      "autoRenew": true,
      "renewalPeriodMonths": 12,
      "createdAt": "2023-12-10T08:30:00.000Z",
      "updatedAt": "2023-12-15T10:00:00.000Z"
    }
  ],
  "errors": null,
  "timestamp": "2024-07-04T10:30:00.000Z"
}
```

---

### POST /api/customers/{id}/contracts

Create a new contract for a customer.

**Parameters:**
- `id` (path, string, required): Customer ID

**Request Body:**
```json
{
  "contractNumber": "CON-2024-002",
  "title": "Equipment Maintenance Contract",
  "description": "Annual maintenance for weighbridge equipment",
  "startDate": "2024-07-01T00:00:00.000Z",
  "endDate": "2025-06-30T23:59:59.999Z",
  "contractValue": 25000.00,
  "contractType": "Maintenance",
  "terms": "Quarterly payments, 24/7 support included",
  "autoRenew": true,
  "renewalPeriodMonths": 12
}
```

**Example Response (201 Created):**
```json
{
  "success": true,
  "message": "Contract created successfully",
  "data": {
    "id": "contract-uuid-012",
    "customerId": "customer-uuid-123",
    "contractNumber": "CON-2024-002",
    "title": "Equipment Maintenance Contract",
    "description": "Annual maintenance for weighbridge equipment",
    "startDate": "2024-07-01T00:00:00.000Z",
    "endDate": "2025-06-30T23:59:59.999Z",
    "contractValue": 25000.00,
    "status": "Draft",
    "contractType": "Maintenance",
    "terms": "Quarterly payments, 24/7 support included",
    "signedByCustomer": null,
    "signedByCompany": null,
    "signedDate": null,
    "autoRenew": true,
    "renewalPeriodMonths": 12,
    "createdAt": "2024-07-04T10:30:00.000Z",
    "updatedAt": "2024-07-04T10:30:00.000Z"
  },
  "errors": null,
  "timestamp": "2024-07-04T10:30:00.000Z"
}
```

---

### PUT /api/contracts/{id}

Update an existing contract.

**Parameters:**
- `id` (path, string, required): Contract ID

**Request Body:**
```json
{
  "contractNumber": "CON-2024-002",
  "title": "Equipment Maintenance Contract - Updated",
  "description": "Annual maintenance for weighbridge equipment with extended coverage",
  "startDate": "2024-07-01T00:00:00.000Z",
  "endDate": "2025-06-30T23:59:59.999Z",
  "contractValue": 28000.00,
  "status": "Active",
  "contractType": "Maintenance",
  "terms": "Quarterly payments, 24/7 support included, parts warranty extended",
  "signedByCustomer": "John Smith",
  "signedByCompany": "QaliTrack Service Manager",
  "signedDate": "2024-07-04T09:00:00.000Z",
  "autoRenew": true,
  "renewalPeriodMonths": 12
}
```

**Example Response:**
```json
{
  "success": true,
  "message": "Contract updated successfully",
  "data": {
    "id": "contract-uuid-012",
    "customerId": "customer-uuid-123",
    "contractNumber": "CON-2024-002",
    "title": "Equipment Maintenance Contract - Updated",
    "description": "Annual maintenance for weighbridge equipment with extended coverage",
    "startDate": "2024-07-01T00:00:00.000Z",
    "endDate": "2025-06-30T23:59:59.999Z",
    "contractValue": 28000.00,
    "status": "Active",
    "contractType": "Maintenance",
    "terms": "Quarterly payments, 24/7 support included, parts warranty extended",
    "signedByCustomer": "John Smith",
    "signedByCompany": "QaliTrack Service Manager",
    "signedDate": "2024-07-04T09:00:00.000Z",
    "autoRenew": true,
    "renewalPeriodMonths": 12,
    "createdAt": "2024-07-04T10:30:00.000Z",
    "updatedAt": "2024-07-04T11:45:00.000Z"
  },
  "errors": null,
  "timestamp": "2024-07-04T11:45:00.000Z"
}
```

---

### DELETE /api/contracts/{id}

Remove a contract from the system.

**Parameters:**
- `id` (path, string, required): Contract ID

**Example Response:**
```json
{
  "success": true,
  "message": "Contract deleted successfully",
  "data": null,
  "errors": null,
  "timestamp": "2024-07-04T10:30:00.000Z"
}
```

## Billing Management Endpoints

### GET /api/customers/{id}/billing

Retrieve billing information for a customer.

**Parameters:**
- `id` (path, string, required): Customer ID

**Example Response:**
```json
{
  "success": true,
  "message": "Customer billing retrieved successfully",
  "data": {
    "id": "billing-uuid-321",
    "customerId": "customer-uuid-123",
    "billingContactName": "Jane Doe",
    "billingEmail": "billing@acme.com",
    "billingPhone": "+1-555-0125",
    "billingAddress": "123 Business St, Accounting Dept",
    "billingCity": "Business City",
    "billingState": "State",
    "billingCountry": "USA",
    "billingPostalCode": "12345",
    "preferredPaymentMethod": "NET30",
    "taxExemptNumber": null,
    "currency": "USD",
    "discountPercentage": 5.00,
    "paymentTermsDays": 30,
    "invoiceDeliveryMethod": "Email",
    "createdAt": "2024-01-15T08:30:00.000Z",
    "updatedAt": "2024-01-15T08:30:00.000Z"
  },
  "errors": null,
  "timestamp": "2024-07-04T10:30:00.000Z"
}
```

---

### PUT /api/customers/{id}/billing

Update billing information for a customer.

**Parameters:**
- `id` (path, string, required): Customer ID

**Request Body:**
```json
{
  "billingContactName": "Jane Doe",
  "billingEmail": "billing@acme.com",
  "billingPhone": "+1-555-0125",
  "billingAddress": "123 Business St, Accounting Dept, Floor 3",
  "billingCity": "Business City",
  "billingState": "State",
  "billingCountry": "USA",
  "billingPostalCode": "12345",
  "preferredPaymentMethod": "NET15",
  "taxExemptNumber": "TAX-EXEMPT-789",
  "currency": "USD",
  "discountPercentage": 7.50,
  "paymentTermsDays": 15,
  "invoiceDeliveryMethod": "Email"
}
```

**Example Response:**
```json
{
  "success": true,
  "message": "Customer billing updated successfully",
  "data": {
    "id": "billing-uuid-321",
    "customerId": "customer-uuid-123",
    "billingContactName": "Jane Doe",
    "billingEmail": "billing@acme.com",
    "billingPhone": "+1-555-0125",
    "billingAddress": "123 Business St, Accounting Dept, Floor 3",
    "billingCity": "Business City",
    "billingState": "State",
    "billingCountry": "USA",
    "billingPostalCode": "12345",
    "preferredPaymentMethod": "NET15",
    "taxExemptNumber": "TAX-EXEMPT-789",
    "currency": "USD",
    "discountPercentage": 7.50,
    "paymentTermsDays": 15,
    "invoiceDeliveryMethod": "Email",
    "createdAt": "2024-01-15T08:30:00.000Z",
    "updatedAt": "2024-07-04T10:30:00.000Z"
  },
  "errors": null,
  "timestamp": "2024-07-04T10:30:00.000Z"
}
```

## Credit Management Endpoints

### GET /api/customers/{id}/credit

Retrieve credit information for a customer.

**Parameters:**
- `id` (path, string, required): Customer ID

**Example Response:**
```json
{
  "success": true,
  "message": "Customer credit retrieved successfully",
  "data": {
    "id": "credit-uuid-654",
    "customerId": "customer-uuid-123",
    "creditLimit": 50000.00,
    "availableCredit": 35000.00,
    "usedCredit": 15000.00,
    "creditTerms": "Net 30 payment terms with 2% early payment discount",
    "creditRating": "Excellent",
    "lastCreditReview": "2024-01-01T00:00:00.000Z",
    "nextCreditReview": "2025-01-01T00:00:00.000Z",
    "securityDeposit": 0.00,
    "creditReference1": "Reference Bank 1",
    "creditReference2": "Supplier Reference 1",
    "creditReference3": "Trade Reference 1",
    "notes": "Excellent payment history, approved for increased limit",
    "createdAt": "2024-01-15T08:30:00.000Z",
    "updatedAt": "2024-06-01T08:30:00.000Z"
  },
  "errors": null,
  "timestamp": "2024-07-04T10:30:00.000Z"
}
```

---

### PUT /api/customers/{id}/credit

Update credit information for a customer.

**Parameters:**
- `id` (path, string, required): Customer ID

**Request Body:**
```json
{
  "creditLimit": 75000.00,
  "creditTerms": "Net 30 payment terms with 3% early payment discount for increased limit",
  "creditRating": "Excellent",
  "lastCreditReview": "2024-07-04T10:30:00.000Z",
  "nextCreditReview": "2025-07-04T10:30:00.000Z",
  "securityDeposit": 0.00,
  "creditReference1": "Reference Bank 1",
  "creditReference2": "Supplier Reference 1",
  "creditReference3": "Trade Reference 1",
  "notes": "Credit limit increased due to excellent payment history and business growth"
}
```

**Example Response:**
```json
{
  "success": true,
  "message": "Customer credit updated successfully",
  "data": {
    "id": "credit-uuid-654",
    "customerId": "customer-uuid-123",
    "creditLimit": 75000.00,
    "availableCredit": 60000.00,
    "usedCredit": 15000.00,
    "creditTerms": "Net 30 payment terms with 3% early payment discount for increased limit",
    "creditRating": "Excellent",
    "lastCreditReview": "2024-07-04T10:30:00.000Z",
    "nextCreditReview": "2025-07-04T10:30:00.000Z",
    "securityDeposit": 0.00,
    "creditReference1": "Reference Bank 1",
    "creditReference2": "Supplier Reference 1",
    "creditReference3": "Trade Reference 1",
    "notes": "Credit limit increased due to excellent payment history and business growth",
    "createdAt": "2024-01-15T08:30:00.000Z",
    "updatedAt": "2024-07-04T10:30:00.000Z"
  },
  "errors": null,
  "timestamp": "2024-07-04T10:30:00.000Z"
}
```

## Enumerations

### CustomerType
- `Individual` - Individual person
- `Corporate` - Corporation or business entity
- `Government` - Government agency or department
- `NonProfit` - Non-profit organization

### CustomerStatus
- `Active` - Fully operational customer
- `Inactive` - Temporarily disabled customer
- `Suspended` - Customer suspended due to compliance or payment issues
- `Pending` - Customer awaiting activation approval

### ContactType
- `Business` - General business contact
- `Technical` - Technical point of contact
- `Billing` - Financial and billing contact
- `Emergency` - Emergency contact person

### ContractStatus
- `Draft` - Contract in preparation
- `Active` - Currently valid contract
- `Expired` - Contract past end date
- `Terminated` - Contract cancelled before end date

### ContractType
- `Service` - Ongoing service agreements
- `Maintenance` - Equipment maintenance contracts
- `Lease` - Equipment or facility lease agreements

## Error Codes

### HTTP Status Codes

- **200 OK**: Request succeeded
- **201 Created**: Resource created successfully
- **400 Bad Request**: Invalid request data or validation error
- **401 Unauthorized**: Authentication required
- **403 Forbidden**: Insufficient permissions
- **404 Not Found**: Resource not found
- **409 Conflict**: Resource conflict (e.g., duplicate tax number)
- **422 Unprocessable Entity**: Validation failed
- **500 Internal Server Error**: Server error

### Common Error Messages

- `"Customer with ID {id} not found"`
- `"Contact with ID {id} not found"`
- `"Contract with ID {id} not found"`
- `"Validation failed"` (with specific validation errors in `errors` array)
- `"Tax number already exists"`
- `"Registration number already exists"`
- `"Email address already exists"`

## Rate Limiting

The API implements standard rate limiting:
- **Rate**: 1000 requests per hour per organization
- **Headers**: Rate limit information included in response headers
- **429 Response**: When rate limit exceeded

## Health Check

### GET /health

Check service health and dependencies.

**Example Response:**
```json
{
  "status": "Healthy",
  "totalDuration": "00:00:00.0234567",
  "entries": {
    "database": {
      "data": {},
      "description": null,
      "duration": "00:00:00.0123456",
      "status": "Healthy",
      "tags": []
    }
  }
}
```