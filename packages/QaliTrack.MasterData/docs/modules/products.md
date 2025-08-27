# Product Module

## Overview

The Product module manages the complete product catalog and specifications within the QaliTrack ecosystem. This module provides comprehensive management for products, their specifications, pricing, and associated documentation.

## Key Features

### Core Entities
- **Product**: Main product catalog management
- **ProductSpecification**: Technical specifications and attributes
- **ProductPricing**: Pricing models and structures
- **ProductDocument**: Document and media management
- **ProductCategory**: Product categorization and classification

### Functionality
- **Catalog Management**: Complete product lifecycle management
- **Specification Tracking**: Technical specifications and attributes
- **Pricing Management**: Dynamic pricing models and structures
- **Document Management**: Product documentation and media files
- **Category Management**: Hierarchical product categorization
- **Status Monitoring**: Product availability and lifecycle status

## API Endpoints

### Product Management
- `GET /api/products` - List all products
- `GET /api/products/{id}` - Get product details
- `POST /api/products` - Create new product
- `PUT /api/products/{id}` - Update product
- `PATCH /api/products/{id}` - Partial update product
- `DELETE /api/products/{id}` - Delete product

### Specification Management
- `GET /api/products/{id}/specifications` - List product specifications
- `POST /api/products/{id}/specifications` - Create specification
- `PUT /api/products/{productId}/specifications/{id}` - Update specification
- `DELETE /api/products/{productId}/specifications/{id}` - Delete specification

### Document Management
- `GET /api/products/{id}/documents` - List product documents
- `POST /api/products/{id}/documents` - Upload document
- `PUT /api/products/{productId}/documents/{id}` - Update document
- `DELETE /api/products/{productId}/documents/{id}` - Delete document

## Data Models

### Product
- **Id**: Unique identifier
- **Name**: Product name
- **Description**: Product description
- **SKU**: Stock Keeping Unit code
- **Barcode**: Product barcode/UPC
- **Category**: Product category
- **Brand**: Product brand
- **Model**: Product model
- **Weight**: Product weight
- **Dimensions**: Product dimensions
- **Status**: Product status (Active, Inactive, Discontinued)
- **CreatedDate**: Product creation date
- **LastModified**: Last modification date
- **OrganizationId**: Organization ownership

### ProductSpecification
- **Id**: Unique identifier
- **ProductId**: Associated product
- **Name**: Specification name
- **Value**: Specification value
- **Unit**: Unit of measurement
- **SpecificationType**: Type of specification
- **IsRequired**: Whether specification is mandatory
- **DisplayOrder**: Display order for UI
- **Description**: Specification description

### ProductPricing
- **Id**: Unique identifier
- **ProductId**: Associated product
- **PriceType**: Type of pricing (Base, Wholesale, Retail)
- **Price**: Price amount
- **Currency**: Price currency
- **MinQuantity**: Minimum quantity for price tier
- **MaxQuantity**: Maximum quantity for price tier
- **EffectiveDate**: Price effective date
- **ExpiryDate**: Price expiry date
- **IsActive**: Whether pricing is active

### ProductDocument
- **Id**: Unique identifier
- **ProductId**: Associated product
- **DocumentName**: Document name
- **DocumentType**: Document type/category
- **FilePath**: File storage path
- **FileSize**: File size in bytes
- **MimeType**: File MIME type
- **UploadDate**: Document upload date
- **Description**: Document description

## Business Rules

### Product Management
- SKU codes must be unique within organization
- Products must have at least one active pricing record
- Product status changes affect availability in catalogs
- Product hierarchy supports multiple categorization levels

### Specification Management
- Required specifications must have values
- Specification values must match defined data types
- Specifications can be inherited from product categories
- Custom specifications can be added per product

### Pricing Management
- Price tiers cannot overlap in quantity ranges
- At least one base price must be defined
- Historical pricing must be maintained for auditing
- Currency conversions should be supported

## Integration Points

### Related Modules
- **Business Entities**: Supplier catalogs and relationships
- **Relationships**: Product-supplier associations
- **Organizations**: Product ownership and visibility

### External Systems
- Inventory management systems
- E-commerce platforms
- ERP systems
- Pricing management systems

## Usage Examples

### Creating a New Product
```json
{
  "name": "Premium Coffee Beans",
  "description": "High-quality Arabica coffee beans",
  "sku": "COF-001-PREM",
  "barcode": "1234567890123",
  "category": "Beverages",
  "brand": "QualityBeans",
  "model": "Premium Blend",
  "weight": 1000,
  "dimensions": "20x15x10",
  "organizationId": "org-123"
}
```

### Adding Product Specifications
```json
{
  "productId": "prod-001",
  "specifications": [
    {
      "name": "Origin",
      "value": "Colombian Highlands",
      "specificationType": "Text",
      "isRequired": true
    },
    {
      "name": "Roast Level",
      "value": "Medium",
      "specificationType": "Selection",
      "isRequired": true
    },
    {
      "name": "Caffeine Content",
      "value": "95",
      "unit": "mg/100g",
      "specificationType": "Numeric",
      "isRequired": false
    }
  ]
}
```

### Setting Product Pricing
```json
{
  "productId": "prod-001",
  "pricing": [
    {
      "priceType": "Retail",
      "price": 25.99,
      "currency": "USD",
      "minQuantity": 1,
      "maxQuantity": 10,
      "effectiveDate": "2024-01-01"
    },
    {
      "priceType": "Wholesale",
      "price": 20.99,
      "currency": "USD",
      "minQuantity": 11,
      "maxQuantity": 100,
      "effectiveDate": "2024-01-01"
    }
  ]
}
```

## API Reference

For detailed API documentation including request/response schemas, see the [auto-generated API reference](xref:QaliTrack.MasterData.Core.Modules.Product.DTOs).