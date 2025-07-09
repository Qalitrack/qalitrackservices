# Product Service API Reference

## Table of Contents

- [Overview](#overview)
- [Authentication](#authentication)
- [Base URLs](#base-urls)
- [Response Format](#response-format)
- [Error Handling](#error-handling)
- [Products API](#products-api)
- [Product Categories API](#product-categories-api)
- [Product Specifications API](#product-specifications-api)
- [Product Pricing API](#product-pricing-api)
- [Product Compliance API](#product-compliance-api)
- [Data Models](#data-models)
- [Error Codes](#error-codes)

## Overview

The Product Service API provides comprehensive product master data management capabilities. It follows REST principles and uses JSON for data exchange. All endpoints support standard HTTP methods and return consistent response formats.

### API Version
- **Current Version**: v1
- **Release Date**: 2024-01-15
- **Status**: Stable

### Supported Features
- Product CRUD operations
- Hierarchical category management
- Hazardous material classification
- Multi-tier pricing management
- Product specifications
- Compliance tracking
- Advanced search and filtering

## Authentication

All API requests require authentication using JWT tokens passed in the Authorization header.

### Authentication Header
```http
Authorization: Bearer <jwt-token>
```

### Token Requirements
- **Format**: JWT (JSON Web Token)
- **Validity**: Tokens must be valid and not expired
- **Scope**: Appropriate permissions for the requested operation

### Example Request
```bash
curl -X GET "http://localhost:7005/api/products" \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

## Base URLs

### Development
```
http://localhost:7005
```

### Production (via Gateway)
```
http://localhost:7000/api/products
```

### Swagger Documentation
```
http://localhost:7005/swagger
```

## Response Format

All API responses follow a consistent format with success indicators, data payload, and error information.

### Success Response
```json
{
  "success": true,
  "message": "Operation completed successfully",
  "data": { ... },
  "timestamp": "2024-01-15T10:30:00Z"
}
```

### Error Response
```json
{
  "success": false,
  "message": "Operation failed",
  "errors": [
    "Specific error message 1",
    "Specific error message 2"
  ],
  "timestamp": "2024-01-15T10:30:00Z"
}
```

### Pagination Response
```json
{
  "success": true,
  "data": [...],
  "pagination": {
    "currentPage": 1,
    "pageSize": 50,
    "totalItems": 150,
    "totalPages": 3
  }
}
```

## Error Handling

### HTTP Status Codes
- **200 OK**: Successful GET, PUT operations
- **201 Created**: Successful POST operations
- **400 Bad Request**: Invalid request data
- **401 Unauthorized**: Authentication required
- **403 Forbidden**: Insufficient permissions
- **404 Not Found**: Resource not found
- **409 Conflict**: Resource conflict (e.g., duplicate code)
- **422 Unprocessable Entity**: Validation errors
- **500 Internal Server Error**: Server-side errors

### Error Response Examples

#### Validation Error (400)
```json
{
  "success": false,
  "message": "Validation failed",
  "errors": [
    "Product name is required",
    "Product code must be unique",
    "Category ID must be valid"
  ]
}
```

#### Not Found Error (404)
```json
{
  "success": false,
  "message": "Product not found",
  "errors": ["Product with ID 'invalid-id' does not exist"]
}
```

## Products API

### Get All Products
Retrieve a list of all products.

**Endpoint**: `GET /api/products`

**Parameters**: None

**Response**:
```json
{
  "success": true,
  "data": [
    {
      "id": "prod-12345",
      "name": "Premium Diesel Fuel",
      "code": "DIESEL-001",
      "description": "High-quality diesel fuel for commercial vehicles",
      "categoryId": "cat-fuel-001",
      "categoryName": "Diesel Fuels",
      "unitOfMeasure": "Liters",
      "weight": 0.85,
      "density": 0.832,
      "isHazardous": true,
      "hazmatClass": "3",
      "requiresSpecialHandling": true,
      "status": "Active",
      "notes": "Requires temperature monitoring",
      "createdAt": "2024-01-15T10:30:00Z",
      "updatedAt": "2024-01-15T10:30:00Z"
    }
  ]
}
```

### Get Product by ID
Retrieve a specific product by its ID.

**Endpoint**: `GET /api/products/{id}`

**Parameters**:
- `id` (path, required): Product ID

**Response**:
```json
{
  "success": true,
  "data": {
    "id": "prod-12345",
    "name": "Premium Diesel Fuel",
    "code": "DIESEL-001",
    "description": "High-quality diesel fuel for commercial vehicles",
    "categoryId": "cat-fuel-001",
    "categoryName": "Diesel Fuels",
    "unitOfMeasure": "Liters",
    "weight": 0.85,
    "density": 0.832,
    "isHazardous": true,
    "hazmatClass": "3",
    "requiresSpecialHandling": true,
    "status": "Active",
    "notes": "Requires temperature monitoring",
    "createdAt": "2024-01-15T10:30:00Z",
    "updatedAt": "2024-01-15T10:30:00Z"
  }
}
```

### Register New Product
Create a new product.

**Endpoint**: `POST /api/products`

**Request Body**:
```json
{
  "name": "Premium Diesel Fuel",
  "code": "DIESEL-001",
  "description": "High-quality diesel fuel for commercial vehicles",
  "categoryId": "cat-fuel-001",
  "unitOfMeasure": "Liters",
  "weight": 0.85,
  "density": 0.832,
  "isHazardous": true,
  "hazmatClass": "3",
  "requiresSpecialHandling": true,
  "notes": "Requires temperature monitoring during transport"
}
```

**Response** (201 Created):
```json
{
  "success": true,
  "message": "Product registered successfully",
  "data": {
    "id": "prod-12345",
    "name": "Premium Diesel Fuel",
    "code": "DIESEL-001",
    "status": "Active",
    "createdAt": "2024-01-15T10:30:00Z",
    "updatedAt": "2024-01-15T10:30:00Z"
  }
}
```

### Update Product
Update an existing product.

**Endpoint**: `PUT /api/products/{id}`

**Parameters**:
- `id` (path, required): Product ID

**Request Body**:
```json
{
  "name": "Updated Premium Diesel Fuel",
  "description": "Updated high-quality diesel fuel for commercial vehicles",
  "categoryId": "cat-fuel-001",
  "unitOfMeasure": "Liters",
  "weight": 0.86,
  "density": 0.835,
  "isHazardous": true,
  "hazmatClass": "3",
  "requiresSpecialHandling": true,
  "status": "Active",
  "notes": "Updated handling requirements"
}
```

**Response**:
```json
{
  "success": true,
  "message": "Product updated successfully",
  "data": {
    "id": "prod-12345",
    "name": "Updated Premium Diesel Fuel",
    "code": "DIESEL-001",
    "status": "Active",
    "updatedAt": "2024-01-15T11:30:00Z"
  }
}
```

### Delete Product
Delete a product (soft delete).

**Endpoint**: `DELETE /api/products/{id}`

**Parameters**:
- `id` (path, required): Product ID

**Response**:
```json
{
  "success": true,
  "message": "Product deleted successfully"
}
```

### Search Products
Search products by name, code, or description.

**Endpoint**: `GET /api/products/search`

**Parameters**:
- `searchTerm` (query, required): Search term

**Example**: `GET /api/products/search?searchTerm=diesel`

**Response**:
```json
{
  "success": true,
  "data": [
    {
      "id": "prod-12345",
      "name": "Premium Diesel Fuel",
      "code": "DIESEL-001",
      "description": "High-quality diesel fuel",
      "categoryName": "Diesel Fuels",
      "isHazardous": true,
      "hazmatClass": "3"
    }
  ]
}
```

### Get Products by Category
Retrieve products belonging to a specific category.

**Endpoint**: `GET /api/products/category/{categoryId}`

**Parameters**:
- `categoryId` (path, required): Category ID

**Response**:
```json
{
  "success": true,
  "data": [
    {
      "id": "prod-12345",
      "name": "Premium Diesel Fuel",
      "code": "DIESEL-001",
      "categoryId": "cat-fuel-001",
      "categoryName": "Diesel Fuels"
    }
  ]
}
```

### Get Hazardous Products
Retrieve all products classified as hazardous.

**Endpoint**: `GET /api/products/hazmat`

**Response**:
```json
{
  "success": true,
  "data": [
    {
      "id": "prod-12345",
      "name": "Premium Diesel Fuel",
      "code": "DIESEL-001",
      "isHazardous": true,
      "hazmatClass": "3",
      "requiresSpecialHandling": true
    }
  ]
}
```

## Product Categories API

### Get All Categories
Retrieve all product categories.

**Endpoint**: `GET /api/products/categories`

**Response**:
```json
{
  "success": true,
  "data": [
    {
      "id": "cat-fuel-001",
      "name": "Petroleum Products",
      "code": "PETRO",
      "description": "All petroleum-based products",
      "parentCategoryId": null,
      "sortOrder": 1,
      "isActive": true,
      "subCategories": [
        {
          "id": "cat-diesel-001",
          "name": "Diesel Fuels",
          "code": "DIESEL",
          "parentCategoryId": "cat-fuel-001"
        }
      ]
    }
  ]
}
```

### Get Root Categories
Retrieve top-level categories (categories without parent).

**Endpoint**: `GET /api/products/categories/root`

**Response**:
```json
{
  "success": true,
  "data": [
    {
      "id": "cat-fuel-001",
      "name": "Petroleum Products",
      "code": "PETRO",
      "description": "All petroleum-based products",
      "parentCategoryId": null,
      "sortOrder": 1,
      "isActive": true
    }
  ]
}
```

### Get Subcategories
Retrieve subcategories for a parent category.

**Endpoint**: `GET /api/products/categories/{parentCategoryId}/subcategories`

**Parameters**:
- `parentCategoryId` (path, required): Parent category ID

**Response**:
```json
{
  "success": true,
  "data": [
    {
      "id": "cat-diesel-001",
      "name": "Diesel Fuels",
      "code": "DIESEL",
      "description": "Various types of diesel fuel",
      "parentCategoryId": "cat-fuel-001",
      "sortOrder": 1,
      "isActive": true
    }
  ]
}
```

### Create Category
Create a new product category.

**Endpoint**: `POST /api/products/categories`

**Request Body**:
```json
{
  "name": "Diesel Fuels",
  "code": "DIESEL",
  "description": "Various types of diesel fuel",
  "parentCategoryId": "cat-fuel-001",
  "sortOrder": 1
}
```

**Response** (201 Created):
```json
{
  "success": true,
  "message": "Category created successfully",
  "data": {
    "id": "cat-diesel-001",
    "name": "Diesel Fuels",
    "code": "DIESEL",
    "isActive": true,
    "createdAt": "2024-01-15T10:30:00Z"
  }
}
```

## Product Specifications API

### Get Product Specifications
Retrieve specifications for a specific product.

**Endpoint**: `GET /api/products/{id}/specifications`

**Parameters**:
- `id` (path, required): Product ID

**Response**:
```json
{
  "success": true,
  "data": [
    {
      "id": "spec-1",
      "productId": "prod-12345",
      "name": "Octane Rating",
      "value": "87",
      "unit": "RON",
      "description": "Research Octane Number",
      "isRequired": true,
      "sortOrder": 1
    },
    {
      "id": "spec-2",
      "productId": "prod-12345",
      "name": "Density",
      "value": "0.832",
      "unit": "g/cm³",
      "description": "Density at 15°C",
      "isRequired": true,
      "sortOrder": 2
    }
  ]
}
```

### Update Product Specifications
Update specifications for a specific product.

**Endpoint**: `PUT /api/products/{id}/specifications`

**Parameters**:
- `id` (path, required): Product ID

**Request Body**:
```json
{
  "specifications": [
    {
      "id": "spec-1",
      "name": "Octane Rating",
      "value": "91",
      "unit": "RON",
      "description": "Research Octane Number",
      "isRequired": true,
      "sortOrder": 1
    },
    {
      "name": "Sulfur Content",
      "value": "15",
      "unit": "ppm",
      "description": "Maximum sulfur content",
      "isRequired": true,
      "sortOrder": 3
    }
  ]
}
```

**Response**:
```json
{
  "success": true,
  "message": "Product specifications updated successfully"
}
```

## Product Pricing API

### Get Product Pricing
Retrieve pricing information for a specific product.

**Endpoint**: `GET /api/products/{id}/pricing`

**Parameters**:
- `id` (path, required): Product ID

**Response**:
```json
{
  "success": true,
  "data": [
    {
      "id": "price-1",
      "productId": "prod-12345",
      "unitPrice": 2.85,
      "currency": "USD",
      "minimumQuantity": 1000,
      "maximumQuantity": 5000,
      "customerGroup": "Retail",
      "region": "US-West",
      "effectiveDate": "2024-01-01T00:00:00Z",
      "expirationDate": "2024-12-31T23:59:59Z",
      "isActive": true
    },
    {
      "id": "price-2",
      "productId": "prod-12345",
      "unitPrice": 2.75,
      "currency": "USD",
      "minimumQuantity": 5001,
      "maximumQuantity": 10000,
      "customerGroup": "Retail",
      "region": "US-West",
      "effectiveDate": "2024-01-01T00:00:00Z",
      "expirationDate": "2024-12-31T23:59:59Z",
      "isActive": true
    }
  ]
}
```

### Update Product Pricing
Update pricing information for a specific product.

**Endpoint**: `PUT /api/products/{id}/pricing`

**Parameters**:
- `id` (path, required): Product ID

**Request Body**:
```json
{
  "pricingRules": [
    {
      "id": "price-1",
      "unitPrice": 2.90,
      "currency": "USD",
      "minimumQuantity": 1000,
      "maximumQuantity": 5000,
      "customerGroup": "Retail",
      "region": "US-West",
      "effectiveDate": "2024-02-01T00:00:00Z",
      "expirationDate": "2024-12-31T23:59:59Z"
    },
    {
      "unitPrice": 2.80,
      "currency": "USD",
      "minimumQuantity": 5001,
      "maximumQuantity": 10000,
      "customerGroup": "Retail",
      "region": "US-West",
      "effectiveDate": "2024-02-01T00:00:00Z",
      "expirationDate": "2024-12-31T23:59:59Z"
    }
  ]
}
```

**Response**:
```json
{
  "success": true,
  "message": "Product pricing updated successfully"
}
```

## Product Compliance API

### Get Product Compliance
Retrieve compliance information for a specific product.

**Endpoint**: `GET /api/products/{id}/compliance`

**Parameters**:
- `id` (path, required): Product ID

**Response**:
```json
{
  "success": true,
  "data": {
    "productId": "prod-12345",
    "isHazardous": true,
    "hazmatClass": "3",
    "transportRequirements": "UN1202 - Diesel fuel, flash point > 38°C",
    "storageRequirements": "Store in cool, dry place away from ignition sources",
    "handlingInstructions": "Use appropriate PPE and ensure adequate ventilation",
    "emergencyProcedures": "In case of spill: evacuate area, contact emergency services",
    "requirements": [
      {
        "id": "comp-1",
        "complianceType": "DOT",
        "regulation": "49 CFR 173.150",
        "authority": "US Department of Transportation",
        "description": "Transportation of diesel fuel",
        "certificationNumber": "DOT-2024-001",
        "issuedDate": "2024-01-01T00:00:00Z",
        "expirationDate": "2024-12-31T23:59:59Z",
        "status": "Active",
        "isRequired": true
      },
      {
        "id": "comp-2",
        "complianceType": "EPA",
        "regulation": "40 CFR 80.510",
        "authority": "Environmental Protection Agency",
        "description": "Diesel fuel quality standards",
        "certificationNumber": "EPA-2024-002",
        "issuedDate": "2024-01-01T00:00:00Z",
        "expirationDate": "2024-12-31T23:59:59Z",
        "status": "Active",
        "isRequired": true
      }
    ]
  }
}
```

## Data Models

### Product
```typescript
interface Product {
  id: string;
  name: string;
  code: string;
  description?: string;
  categoryId: string;
  categoryName?: string;
  unitOfMeasure: string;
  weight?: number;
  density?: number;
  isHazardous: boolean;
  hazmatClass?: string;
  requiresSpecialHandling: boolean;
  status: ProductStatus;
  notes?: string;
  createdAt: Date;
  updatedAt: Date;
}
```

### ProductCategory
```typescript
interface ProductCategory {
  id: string;
  name: string;
  code: string;
  description?: string;
  parentCategoryId?: string;
  sortOrder: number;
  isActive: boolean;
  createdAt: Date;
  updatedAt: Date;
  subCategories?: ProductCategory[];
}
```

### ProductSpecification
```typescript
interface ProductSpecification {
  id: string;
  productId: string;
  name: string;
  value: string;
  unit?: string;
  description?: string;
  isRequired: boolean;
  sortOrder: number;
  createdAt: Date;
  updatedAt: Date;
}
```

### ProductPricing
```typescript
interface ProductPricing {
  id: string;
  productId: string;
  unitPrice: number;
  currency: string;
  minimumQuantity?: number;
  maximumQuantity?: number;
  customerGroup?: string;
  region?: string;
  effectiveDate: Date;
  expirationDate?: Date;
  isActive: boolean;
  createdAt: Date;
  updatedAt: Date;
}
```

### ProductCompliance
```typescript
interface ProductCompliance {
  id: string;
  productId: string;
  complianceType: string;
  regulation: string;
  authority?: string;
  description?: string;
  certificationNumber?: string;
  issuedDate?: Date;
  expirationDate?: Date;
  status: ComplianceStatus;
  isRequired: boolean;
  createdAt: Date;
  updatedAt: Date;
}
```

### Request Models

#### RegisterProductRequest
```typescript
interface RegisterProductRequest {
  name: string;
  code: string;
  description?: string;
  categoryId: string;
  unitOfMeasure: string;
  weight?: number;
  density?: number;
  isHazardous: boolean;
  hazmatClass?: string;
  requiresSpecialHandling: boolean;
  notes?: string;
}
```

#### UpdateProductRequest
```typescript
interface UpdateProductRequest {
  name: string;
  description?: string;
  categoryId: string;
  unitOfMeasure: string;
  weight?: number;
  density?: number;
  isHazardous: boolean;
  hazmatClass?: string;
  requiresSpecialHandling: boolean;
  status: string;
  notes?: string;
}
```

#### CreateProductCategoryRequest
```typescript
interface CreateProductCategoryRequest {
  name: string;
  code: string;
  description?: string;
  parentCategoryId?: string;
  sortOrder: number;
}
```

### Enums

#### ProductStatus
```typescript
enum ProductStatus {
  Active = "Active",
  Inactive = "Inactive",
  Discontinued = "Discontinued",
  Pending = "Pending"
}
```

#### ComplianceStatus
```typescript
enum ComplianceStatus {
  Active = "Active",
  Expired = "Expired",
  Pending = "Pending",
  Revoked = "Revoked"
}
```

## Error Codes

### Validation Errors (400)
- **PROD001**: Product name is required
- **PROD002**: Product code is required and must be unique
- **PROD003**: Category ID must be valid
- **PROD004**: Unit of measure is required
- **PROD005**: Invalid hazmat class (must be 1-9)
- **PROD006**: Hazmat class required for hazardous products
- **PROD007**: Invalid product status

### Business Logic Errors (422)
- **PROD101**: Cannot delete product with active inventory
- **PROD102**: Cannot change product code after creation
- **PROD103**: Category cannot be deleted with assigned products
- **PROD104**: Circular reference in category hierarchy
- **PROD105**: Invalid pricing rule configuration

### System Errors (500)
- **PROD501**: Database connection error
- **PROD502**: External service unavailable
- **PROD503**: Data consistency error
- **PROD504**: Cache invalidation failed

### Usage Examples

#### Complete Product Creation Workflow
```bash
# 1. Create category
curl -X POST "http://localhost:7005/api/products/categories" \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Diesel Fuels",
    "code": "DIESEL",
    "description": "Various types of diesel fuel",
    "parentCategoryId": null,
    "sortOrder": 1
  }'

# 2. Create product
curl -X POST "http://localhost:7005/api/products" \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Premium Diesel Fuel",
    "code": "DIESEL-001",
    "description": "High-quality diesel fuel",
    "categoryId": "cat-diesel-001",
    "unitOfMeasure": "Liters",
    "weight": 0.85,
    "isHazardous": true,
    "hazmatClass": "3",
    "requiresSpecialHandling": true
  }'

# 3. Add specifications
curl -X PUT "http://localhost:7005/api/products/prod-12345/specifications" \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{
    "specifications": [
      {
        "name": "Octane Rating",
        "value": "87",
        "unit": "RON"
      }
    ]
  }'

# 4. Add pricing
curl -X PUT "http://localhost:7005/api/products/prod-12345/pricing" \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{
    "pricingRules": [
      {
        "unitPrice": 2.85,
        "currency": "USD",
        "minimumQuantity": 1000,
        "customerGroup": "Retail"
      }
    ]
  }'
```

---

*This API reference is automatically generated from the Product Service OpenAPI specification and is updated with each release.*