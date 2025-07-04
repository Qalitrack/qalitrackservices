# Product Service Database Schema

## Table of Contents

- [Overview](#overview)
- [Entity Relationship Diagram](#entity-relationship-diagram)
- [Database Tables](#database-tables)
- [Relationships](#relationships)
- [Indexes and Constraints](#indexes-and-constraints)
- [Data Types and Constraints](#data-types-and-constraints)
- [Migration Scripts](#migration-scripts)
- [Performance Considerations](#performance-considerations)

## Overview

The Product Service database schema is designed to support comprehensive product master data management within the QaliTrack ecosystem. The schema follows Entity Framework Core conventions and implements Clean Architecture principles with proper separation of concerns.

### Database Configuration
- **Primary Database**: SQLite (development), PostgreSQL/SQL Server (production)
- **ORM**: Entity Framework Core 8.0
- **Migration Strategy**: Code-first migrations
- **Naming Convention**: Pascal case for entities, camelCase for properties
- **Soft Delete**: Implemented across all entities with `IsDeleted` flag

### Schema Features
- **Hierarchical Categories**: Self-referencing category structure
- **Hazmat Support**: Comprehensive dangerous goods classification
- **Multi-tier Pricing**: Customer group and quantity-based pricing
- **Specifications**: Flexible key-value product attributes
- **Compliance Tracking**: Regulatory compliance management
- **Audit Fields**: Creation and modification timestamps

## Entity Relationship Diagram

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│  ProductCategory│    │     Product     │    │ProductSpecifica-│
│                 │    │                 │    │tion             │
├─────────────────┤    ├─────────────────┤    ├─────────────────┤
│ Id (PK)         │◄───┤ CategoryId (FK) │    │ Id (PK)         │
│ Name            │    │ Id (PK)         ├───►│ ProductId (FK)  │
│ Code (UK)       │    │ Name            │    │ Name            │
│ Description     │    │ Code (UK)       │    │ Value           │
│ ParentCategoryId│    │ Description     │    │ Unit            │
│ SortOrder       │    │ UnitOfMeasure   │    │ Description     │
│ IsActive        │    │ Weight          │    │ IsRequired      │
│ CreatedAt       │    │ Density         │    │ SortOrder       │
│ UpdatedAt       │    │ IsHazardous     │    │ IsDeleted       │
│ IsDeleted       │    │ HazmatClass     │    │ CreatedAt       │
└─────────────────┘    │ RequiresSpecial │    │ UpdatedAt       │
         ▲             │ Handling        │    └─────────────────┘
         │             │ Status          │    
         │             │ Notes           │    ┌─────────────────┐
         │             │ IsDeleted       │    │ ProductPricing  │
         │             │ CreatedAt       │    │                 │
         │             │ UpdatedAt       │    ├─────────────────┤
         │             └─────────────────┘    │ Id (PK)         │
         │                      │             │ ProductId (FK)  │◄──┐
         │                      │             │ UnitPrice       │   │
         │                      │             │ Currency        │   │
         │                      │             │ MinimumQuantity │   │
         │                      │             │ MaximumQuantity │   │
         │                      │             │ CustomerGroup   │   │
         │                      │             │ Region          │   │
         │                      │             │ EffectiveDate   │   │
         │                      │             │ ExpirationDate  │   │
         │                      │             │ IsActive        │   │
         │                      │             │ IsDeleted       │   │
         │                      │             │ CreatedAt       │   │
         │                      │             │ UpdatedAt       │   │
         │                      │             └─────────────────┘   │
         │                      │                                   │
         └──────────────────────┼───────────────────────────────────┘
                                │
                                │             ┌─────────────────┐
                                │             │  ProductHazmat  │
                                │             │                 │
                                │             ├─────────────────┤
                                │             │ Id (PK)         │
                                │             │ ProductId (FK)  │◄──┐
                                │             │ HazmatClass     │   │
                                │             │ PackingGroup    │   │
                                │             │ UnNumber        │   │
                                │             │ ProperShipping  │   │
                                │             │ Name            │   │
                                │             │ Transport       │   │
                                │             │ Requirements    │   │
                                │             │ Storage         │   │
                                │             │ Requirements    │   │
                                │             │ Handling        │   │
                                │             │ Instructions    │   │
                                │             │ Emergency       │   │
                                │             │ Procedures      │   │
                                │             │ EmergencyContact│   │
                                │             │ EmergencyPhone  │   │
                                │             │ IsDeleted       │   │
                                │             │ CreatedAt       │   │
                                │             │ UpdatedAt       │   │
                                │             └─────────────────┘   │
                                │                                   │
                                └───────────────────────────────────┘
                                │
                                │             ┌─────────────────┐
                                │             │ProductCompliance│
                                │             │                 │
                                │             ├─────────────────┤
                                │             │ Id (PK)         │
                                │             │ ProductId (FK)  │◄──┐
                                │             │ ComplianceType  │   │
                                │             │ Regulation      │   │
                                │             │ Authority       │   │
                                │             │ Description     │   │
                                │             │ Certification   │   │
                                │             │ Number          │   │
                                │             │ IssuedDate      │   │
                                │             │ ExpirationDate  │   │
                                │             │ Status          │   │
                                │             │ IsRequired      │   │
                                │             │ IsDeleted       │   │
                                │             │ CreatedAt       │   │
                                │             │ UpdatedAt       │   │
                                │             └─────────────────┘   │
                                │                                   │
                                └───────────────────────────────────┘
                                │
                                │             ┌─────────────────┐
                                │             │ProductInventory │
                                │             │                 │
                                │             ├─────────────────┤
                                │             │ Id (PK)         │
                                │             │ ProductId (FK)  │◄──┐
                                │             │ CurrentStock    │   │
                                │             │ MinimumStock    │   │
                                │             │ MaximumStock    │   │
                                │             │ ReorderPoint    │   │
                                │             │ ReorderQuantity │   │
                                │             │ Location        │   │
                                │             │ Warehouse       │   │
                                │             │ LastStockUpdate │   │
                                │             │ IsDeleted       │   │
                                │             │ CreatedAt       │   │
                                │             │ UpdatedAt       │   │
                                │             └─────────────────┘   │
                                │                                   │
                                └───────────────────────────────────┘
                                │
                                │             ┌─────────────────┐
                                │             │ ProductVariant  │
                                │             │                 │
                                │             ├─────────────────┤
                                │             │ Id (PK)         │
                                │             │ ProductId (FK)  │◄──┘
                                │             │ Name            │
                                │             │ Code            │
                                │             │ VariantType     │
                                │             │ VariantValue    │
                                │             │ PriceAdjustment │
                                │             │ IsActive        │
                                │             │ IsDeleted       │
                                │             │ CreatedAt       │
                                │             │ UpdatedAt       │
                                │             └─────────────────┘
```

## Database Tables

### Products

The main product entity containing core product information.

```sql
CREATE TABLE Products (
    Id              NVARCHAR(450)   PRIMARY KEY,
    Name            NVARCHAR(100)   NOT NULL,
    Code            NVARCHAR(50)    NOT NULL UNIQUE,
    Description     NVARCHAR(MAX)   NULL,
    CategoryId      NVARCHAR(450)   NOT NULL,
    UnitOfMeasure   NVARCHAR(20)    NOT NULL,
    Weight          DECIMAL(18,4)   NULL,
    Density         DECIMAL(18,4)   NULL,
    IsHazardous     BIT             NOT NULL DEFAULT 0,
    HazmatClass     NVARCHAR(20)    NULL,
    RequiresSpecialHandling BIT     NOT NULL DEFAULT 0,
    Status          INT             NOT NULL DEFAULT 0,
    Notes           NVARCHAR(MAX)   NULL,
    IsDeleted       BIT             NOT NULL DEFAULT 0,
    CreatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT FK_Products_ProductCategories_CategoryId 
        FOREIGN KEY (CategoryId) REFERENCES ProductCategories(Id)
);
```

**Columns**:
- `Id`: Unique identifier (GUID)
- `Name`: Product name (max 100 characters)
- `Code`: Unique product code (max 50 characters)
- `Description`: Optional product description
- `CategoryId`: Foreign key to ProductCategories
- `UnitOfMeasure`: Base unit of measurement (max 20 characters)
- `Weight`: Product weight in base units (decimal 18,4)
- `Density`: Product density (decimal 18,4)
- `IsHazardous`: Boolean flag for hazardous classification
- `HazmatClass`: UN hazmat class (1-9)
- `RequiresSpecialHandling`: Boolean flag for special handling requirements
- `Status`: Product status enum (0=Active, 1=Inactive, 2=Discontinued, 3=Pending)
- `Notes`: Optional notes and comments
- `IsDeleted`: Soft delete flag
- `CreatedAt`: Creation timestamp
- `UpdatedAt`: Last modification timestamp

### ProductCategories

Hierarchical product categorization system.

```sql
CREATE TABLE ProductCategories (
    Id              NVARCHAR(450)   PRIMARY KEY,
    Name            NVARCHAR(50)    NOT NULL,
    Code            NVARCHAR(20)    NOT NULL UNIQUE,
    Description     NVARCHAR(MAX)   NULL,
    ParentCategoryId NVARCHAR(450)  NULL,
    SortOrder       INT             NOT NULL DEFAULT 0,
    IsActive        BIT             NOT NULL DEFAULT 1,
    IsDeleted       BIT             NOT NULL DEFAULT 0,
    CreatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT FK_ProductCategories_ProductCategories_ParentCategoryId 
        FOREIGN KEY (ParentCategoryId) REFERENCES ProductCategories(Id)
);
```

**Columns**:
- `Id`: Unique identifier (GUID)
- `Name`: Category name (max 50 characters)
- `Code`: Unique category code (max 20 characters)
- `Description`: Optional category description
- `ParentCategoryId`: Self-referencing foreign key for hierarchy
- `SortOrder`: Display order within parent category
- `IsActive`: Boolean flag for active categories
- `IsDeleted`: Soft delete flag
- `CreatedAt`: Creation timestamp
- `UpdatedAt`: Last modification timestamp

### ProductSpecifications

Technical specifications and attributes for products.

```sql
CREATE TABLE ProductSpecifications (
    Id              NVARCHAR(450)   PRIMARY KEY,
    ProductId       NVARCHAR(450)   NOT NULL,
    Name            NVARCHAR(50)    NOT NULL,
    Value           NVARCHAR(200)   NOT NULL,
    Unit            NVARCHAR(20)    NULL,
    Description     NVARCHAR(MAX)   NULL,
    IsRequired      BIT             NOT NULL DEFAULT 0,
    SortOrder       INT             NOT NULL DEFAULT 0,
    IsDeleted       BIT             NOT NULL DEFAULT 0,
    CreatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT FK_ProductSpecifications_Products_ProductId 
        FOREIGN KEY (ProductId) REFERENCES Products(Id) ON DELETE CASCADE
);
```

**Columns**:
- `Id`: Unique identifier (GUID)
- `ProductId`: Foreign key to Products
- `Name`: Specification name (max 50 characters)
- `Value`: Specification value (max 200 characters)
- `Unit`: Optional unit of measurement (max 20 characters)
- `Description`: Optional specification description
- `IsRequired`: Boolean flag for required specifications
- `SortOrder`: Display order for specifications
- `IsDeleted`: Soft delete flag
- `CreatedAt`: Creation timestamp
- `UpdatedAt`: Last modification timestamp

### ProductPricing

Multi-tier pricing rules for products.

```sql
CREATE TABLE ProductPricing (
    Id              NVARCHAR(450)   PRIMARY KEY,
    ProductId       NVARCHAR(450)   NOT NULL,
    UnitPrice       DECIMAL(18,4)   NOT NULL,
    Currency        NVARCHAR(3)     NOT NULL DEFAULT 'USD',
    MinimumQuantity DECIMAL(18,4)   NULL,
    MaximumQuantity DECIMAL(18,4)   NULL,
    CustomerGroup   NVARCHAR(50)    NULL,
    Region          NVARCHAR(50)    NULL,
    EffectiveDate   DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    ExpirationDate  DATETIME2       NULL,
    IsActive        BIT             NOT NULL DEFAULT 1,
    IsDeleted       BIT             NOT NULL DEFAULT 0,
    CreatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT FK_ProductPricing_Products_ProductId 
        FOREIGN KEY (ProductId) REFERENCES Products(Id) ON DELETE CASCADE
);
```

**Columns**:
- `Id`: Unique identifier (GUID)
- `ProductId`: Foreign key to Products
- `UnitPrice`: Price per unit (decimal 18,4)
- `Currency`: ISO currency code (3 characters)
- `MinimumQuantity`: Minimum quantity for this price tier
- `MaximumQuantity`: Maximum quantity for this price tier
- `CustomerGroup`: Target customer group (max 50 characters)
- `Region`: Geographic region (max 50 characters)
- `EffectiveDate`: When pricing becomes effective
- `ExpirationDate`: When pricing expires (optional)
- `IsActive`: Boolean flag for active pricing
- `IsDeleted`: Soft delete flag
- `CreatedAt`: Creation timestamp
- `UpdatedAt`: Last modification timestamp

### ProductHazmat

Hazardous material information for dangerous goods.

```sql
CREATE TABLE ProductHazmat (
    Id                  NVARCHAR(450)   PRIMARY KEY,
    ProductId           NVARCHAR(450)   NOT NULL UNIQUE,
    HazmatClass         NVARCHAR(20)    NOT NULL,
    PackingGroup        NVARCHAR(10)    NULL,
    UnNumber            NVARCHAR(10)    NULL,
    ProperShippingName  NVARCHAR(200)   NULL,
    TransportRequirements NVARCHAR(MAX) NULL,
    StorageRequirements NVARCHAR(MAX)   NULL,
    HandlingInstructions NVARCHAR(MAX)  NULL,
    EmergencyProcedures NVARCHAR(MAX)   NULL,
    EmergencyContact    NVARCHAR(100)   NULL,
    EmergencyPhone      NVARCHAR(20)    NULL,
    IsDeleted           BIT             NOT NULL DEFAULT 0,
    CreatedAt           DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt           DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT FK_ProductHazmat_Products_ProductId 
        FOREIGN KEY (ProductId) REFERENCES Products(Id) ON DELETE CASCADE
);
```

**Columns**:
- `Id`: Unique identifier (GUID)
- `ProductId`: Foreign key to Products (one-to-one)
- `HazmatClass`: UN hazmat class (max 20 characters)
- `PackingGroup`: UN packing group (I, II, III)
- `UnNumber`: UN identification number
- `ProperShippingName`: Official shipping name
- `TransportRequirements`: Transportation requirements and restrictions
- `StorageRequirements`: Storage conditions and requirements
- `HandlingInstructions`: Safe handling procedures
- `EmergencyProcedures`: Emergency response procedures
- `EmergencyContact`: Emergency contact name
- `EmergencyPhone`: Emergency contact phone number
- `IsDeleted`: Soft delete flag
- `CreatedAt`: Creation timestamp
- `UpdatedAt`: Last modification timestamp

### ProductCompliance

Regulatory compliance requirements and certifications.

```sql
CREATE TABLE ProductCompliance (
    Id                  NVARCHAR(450)   PRIMARY KEY,
    ProductId           NVARCHAR(450)   NOT NULL,
    ComplianceType      NVARCHAR(50)    NOT NULL,
    Regulation          NVARCHAR(100)   NOT NULL,
    Authority           NVARCHAR(100)   NULL,
    Description         NVARCHAR(MAX)   NULL,
    CertificationNumber NVARCHAR(50)    NULL,
    IssuedDate          DATETIME2       NULL,
    ExpirationDate      DATETIME2       NULL,
    Status              INT             NOT NULL DEFAULT 0,
    IsRequired          BIT             NOT NULL DEFAULT 1,
    IsDeleted           BIT             NOT NULL DEFAULT 0,
    CreatedAt           DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt           DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT FK_ProductCompliance_Products_ProductId 
        FOREIGN KEY (ProductId) REFERENCES Products(Id) ON DELETE CASCADE
);
```

**Columns**:
- `Id`: Unique identifier (GUID)
- `ProductId`: Foreign key to Products
- `ComplianceType`: Type of compliance (DOT, EPA, OSHA, etc.)
- `Regulation`: Specific regulation reference
- `Authority`: Regulatory authority
- `Description`: Compliance description
- `CertificationNumber`: Certificate or permit number
- `IssuedDate`: When certification was issued
- `ExpirationDate`: When certification expires
- `Status`: Compliance status (0=Active, 1=Expired, 2=Pending, 3=Revoked)
- `IsRequired`: Boolean flag for required compliance
- `IsDeleted`: Soft delete flag
- `CreatedAt`: Creation timestamp
- `UpdatedAt`: Last modification timestamp

### ProductInventory

Inventory tracking and management information.

```sql
CREATE TABLE ProductInventory (
    Id              NVARCHAR(450)   PRIMARY KEY,
    ProductId       NVARCHAR(450)   NOT NULL UNIQUE,
    CurrentStock    DECIMAL(18,4)   NOT NULL DEFAULT 0,
    MinimumStock    DECIMAL(18,4)   NULL,
    MaximumStock    DECIMAL(18,4)   NULL,
    ReorderPoint    DECIMAL(18,4)   NULL,
    ReorderQuantity DECIMAL(18,4)   NULL,
    Location        NVARCHAR(50)    NULL,
    Warehouse       NVARCHAR(50)    NULL,
    LastStockUpdate DATETIME2       NULL,
    IsDeleted       BIT             NOT NULL DEFAULT 0,
    CreatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT FK_ProductInventory_Products_ProductId 
        FOREIGN KEY (ProductId) REFERENCES Products(Id) ON DELETE CASCADE
);
```

**Columns**:
- `Id`: Unique identifier (GUID)
- `ProductId`: Foreign key to Products (one-to-one)
- `CurrentStock`: Current stock quantity
- `MinimumStock`: Minimum stock threshold
- `MaximumStock`: Maximum stock capacity
- `ReorderPoint`: Automatic reorder trigger point
- `ReorderQuantity`: Standard reorder quantity
- `Location`: Specific storage location
- `Warehouse`: Warehouse identifier
- `LastStockUpdate`: Last inventory update timestamp
- `IsDeleted`: Soft delete flag
- `CreatedAt`: Creation timestamp
- `UpdatedAt`: Last modification timestamp

### ProductVariants

Product variations and alternative configurations.

```sql
CREATE TABLE ProductVariants (
    Id              NVARCHAR(450)   PRIMARY KEY,
    ProductId       NVARCHAR(450)   NOT NULL,
    Name            NVARCHAR(100)   NOT NULL,
    Code            NVARCHAR(50)    NOT NULL,
    VariantType     NVARCHAR(50)    NOT NULL,
    VariantValue    NVARCHAR(100)   NOT NULL,
    PriceAdjustment DECIMAL(18,4)   NULL DEFAULT 0,
    IsActive        BIT             NOT NULL DEFAULT 1,
    IsDeleted       BIT             NOT NULL DEFAULT 0,
    CreatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT FK_ProductVariants_Products_ProductId 
        FOREIGN KEY (ProductId) REFERENCES Products(Id) ON DELETE CASCADE
);
```

**Columns**:
- `Id`: Unique identifier (GUID)
- `ProductId`: Foreign key to Products
- `Name`: Variant name (max 100 characters)
- `Code`: Unique variant code (max 50 characters)
- `VariantType`: Type of variation (Size, Grade, Packaging, etc.)
- `VariantValue`: Specific variant value
- `PriceAdjustment`: Price adjustment from base product
- `IsActive`: Boolean flag for active variants
- `IsDeleted`: Soft delete flag
- `CreatedAt`: Creation timestamp
- `UpdatedAt`: Last modification timestamp

## Relationships

### Primary Relationships

1. **Product ↔ ProductCategory** (Many-to-One)
   - Each product belongs to exactly one category
   - Categories can contain multiple products
   - Foreign key: `Products.CategoryId → ProductCategories.Id`

2. **ProductCategory ↔ ProductCategory** (Self-Referencing)
   - Categories support hierarchical structure
   - Parent categories can have multiple subcategories
   - Foreign key: `ProductCategories.ParentCategoryId → ProductCategories.Id`

3. **Product ↔ ProductSpecification** (One-to-Many)
   - Each product can have multiple specifications
   - Specifications belong to exactly one product
   - Foreign key: `ProductSpecifications.ProductId → Products.Id`

4. **Product ↔ ProductPricing** (One-to-Many)
   - Each product can have multiple pricing rules
   - Pricing rules belong to exactly one product
   - Foreign key: `ProductPricing.ProductId → Products.Id`

5. **Product ↔ ProductHazmat** (One-to-One)
   - Each product can have optional hazmat information
   - Hazmat information belongs to exactly one product
   - Foreign key: `ProductHazmat.ProductId → Products.Id`

6. **Product ↔ ProductCompliance** (One-to-Many)
   - Each product can have multiple compliance requirements
   - Compliance requirements belong to exactly one product
   - Foreign key: `ProductCompliance.ProductId → Products.Id`

7. **Product ↔ ProductInventory** (One-to-One)
   - Each product can have optional inventory information
   - Inventory information belongs to exactly one product
   - Foreign key: `ProductInventory.ProductId → Products.Id`

8. **Product ↔ ProductVariant** (One-to-Many)
   - Each product can have multiple variants
   - Variants belong to exactly one product
   - Foreign key: `ProductVariants.ProductId → Products.Id`

### Relationship Constraints

- **Cascade Delete**: All child entities are deleted when parent product is deleted
- **Referential Integrity**: Foreign key constraints ensure data consistency
- **Soft Delete**: All entities support soft deletion to maintain audit trails
- **Unique Constraints**: Prevent duplicate codes and ensure data integrity

## Indexes and Constraints

### Primary Indexes

```sql
-- Primary Keys (automatically indexed)
CREATE UNIQUE INDEX IX_Products_Id ON Products(Id);
CREATE UNIQUE INDEX IX_ProductCategories_Id ON ProductCategories(Id);
CREATE UNIQUE INDEX IX_ProductSpecifications_Id ON ProductSpecifications(Id);
CREATE UNIQUE INDEX IX_ProductPricing_Id ON ProductPricing(Id);
CREATE UNIQUE INDEX IX_ProductHazmat_Id ON ProductHazmat(Id);
CREATE UNIQUE INDEX IX_ProductCompliance_Id ON ProductCompliance(Id);
CREATE UNIQUE INDEX IX_ProductInventory_Id ON ProductInventory(Id);
CREATE UNIQUE INDEX IX_ProductVariants_Id ON ProductVariants(Id);
```

### Unique Constraints

```sql
-- Unique business keys
CREATE UNIQUE INDEX IX_Products_Code ON Products(Code) WHERE IsDeleted = 0;
CREATE UNIQUE INDEX IX_ProductCategories_Code ON ProductCategories(Code) WHERE IsDeleted = 0;
CREATE UNIQUE INDEX IX_ProductHazmat_ProductId ON ProductHazmat(ProductId) WHERE IsDeleted = 0;
CREATE UNIQUE INDEX IX_ProductInventory_ProductId ON ProductInventory(ProductId) WHERE IsDeleted = 0;
```

### Foreign Key Indexes

```sql
-- Foreign key relationships
CREATE INDEX IX_Products_CategoryId ON Products(CategoryId);
CREATE INDEX IX_ProductCategories_ParentCategoryId ON ProductCategories(ParentCategoryId);
CREATE INDEX IX_ProductSpecifications_ProductId ON ProductSpecifications(ProductId);
CREATE INDEX IX_ProductPricing_ProductId ON ProductPricing(ProductId);
CREATE INDEX IX_ProductHazmat_ProductId ON ProductHazmat(ProductId);
CREATE INDEX IX_ProductCompliance_ProductId ON ProductCompliance(ProductId);
CREATE INDEX IX_ProductInventory_ProductId ON ProductInventory(ProductId);
CREATE INDEX IX_ProductVariants_ProductId ON ProductVariants(ProductId);
```

### Performance Indexes

```sql
-- Search and filtering indexes
CREATE INDEX IX_Products_Name ON Products(Name) WHERE IsDeleted = 0;
CREATE INDEX IX_Products_IsHazardous ON Products(IsHazardous) WHERE IsDeleted = 0;
CREATE INDEX IX_Products_Status ON Products(Status) WHERE IsDeleted = 0;
CREATE INDEX IX_ProductCategories_IsActive ON ProductCategories(IsActive) WHERE IsDeleted = 0;
CREATE INDEX IX_ProductPricing_Currency ON ProductPricing(Currency) WHERE IsDeleted = 0;
CREATE INDEX IX_ProductPricing_CustomerGroup ON ProductPricing(CustomerGroup) WHERE IsDeleted = 0;
CREATE INDEX IX_ProductCompliance_ComplianceType ON ProductCompliance(ComplianceType) WHERE IsDeleted = 0;
```

### Composite Indexes

```sql
-- Complex query optimization
CREATE INDEX IX_Products_Category_Status ON Products(CategoryId, Status) WHERE IsDeleted = 0;
CREATE INDEX IX_ProductPricing_Product_Active ON ProductPricing(ProductId, IsActive) WHERE IsDeleted = 0;
CREATE INDEX IX_ProductSpecifications_Product_Required ON ProductSpecifications(ProductId, IsRequired) WHERE IsDeleted = 0;
```

## Data Types and Constraints

### String Data Types

| Field Type | SQL Type | Max Length | Description |
|------------|----------|------------|-------------|
| ID | NVARCHAR(450) | 450 | GUID identifiers |
| Name | NVARCHAR(100) | 100 | Entity names |
| Code | NVARCHAR(50) | 50 | Business codes |
| Short Text | NVARCHAR(20) | 20 | Units, currencies |
| Medium Text | NVARCHAR(200) | 200 | Descriptions |
| Long Text | NVARCHAR(MAX) | Unlimited | Notes, procedures |

### Numeric Data Types

| Field Type | SQL Type | Precision | Description |
|------------|----------|-----------|-------------|
| Decimal | DECIMAL(18,4) | 18,4 | Weights, prices, quantities |
| Integer | INT | - | Status codes, sort orders |
| Boolean | BIT | - | Flags and switches |

### Date/Time Data Types

| Field Type | SQL Type | Description |
|------------|----------|-------------|
| Timestamp | DATETIME2 | High precision timestamps |
| Date | DATE | Date-only values |

### Validation Constraints

```sql
-- Product constraints
ALTER TABLE Products ADD CONSTRAINT CK_Products_Weight 
    CHECK (Weight IS NULL OR Weight >= 0);

ALTER TABLE Products ADD CONSTRAINT CK_Products_Density 
    CHECK (Density IS NULL OR Density > 0);

ALTER TABLE Products ADD CONSTRAINT CK_Products_HazmatClass 
    CHECK (HazmatClass IS NULL OR HazmatClass IN ('1','2','3','4','5','6','7','8','9'));

-- Category constraints
ALTER TABLE ProductCategories ADD CONSTRAINT CK_ProductCategories_SortOrder 
    CHECK (SortOrder >= 0);

-- Pricing constraints
ALTER TABLE ProductPricing ADD CONSTRAINT CK_ProductPricing_UnitPrice 
    CHECK (UnitPrice > 0);

ALTER TABLE ProductPricing ADD CONSTRAINT CK_ProductPricing_Quantities 
    CHECK (MinimumQuantity IS NULL OR MaximumQuantity IS NULL OR MinimumQuantity <= MaximumQuantity);

-- Inventory constraints
ALTER TABLE ProductInventory ADD CONSTRAINT CK_ProductInventory_Stock_Levels 
    CHECK (CurrentStock >= 0 AND 
           (MinimumStock IS NULL OR MinimumStock >= 0) AND 
           (MaximumStock IS NULL OR MaximumStock >= 0) AND
           (MinimumStock IS NULL OR MaximumStock IS NULL OR MinimumStock <= MaximumStock));
```

## Migration Scripts

### Initial Migration

```sql
-- Create database schema
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'ProductService')
BEGIN
    CREATE DATABASE ProductService;
END

USE ProductService;

-- Enable features
EXEC sp_configure 'contained database authentication', 1;
RECONFIGURE;

-- Create tables in dependency order
-- 1. ProductCategories (no dependencies)
-- 2. Products (depends on ProductCategories)
-- 3. All other tables (depend on Products)
```

### Sample Data Migration

```sql
-- Insert sample categories
INSERT INTO ProductCategories (Id, Name, Code, Description, SortOrder, IsActive)
VALUES 
    ('cat-petro-001', 'Petroleum Products', 'PETRO', 'All petroleum-based products', 1, 1),
    ('cat-diesel-001', 'Diesel Fuels', 'DIESEL', 'Various types of diesel fuel', 1, 1),
    ('cat-chem-001', 'Chemicals', 'CHEM', 'Chemical products', 2, 1);

-- Set up category hierarchy
UPDATE ProductCategories 
SET ParentCategoryId = 'cat-petro-001' 
WHERE Id = 'cat-diesel-001';

-- Insert sample products
INSERT INTO Products (Id, Name, Code, Description, CategoryId, UnitOfMeasure, Weight, IsHazardous, HazmatClass, Status)
VALUES 
    ('prod-diesel-001', 'Premium Diesel Fuel', 'DIESEL-001', 'High-quality diesel fuel', 'cat-diesel-001', 'Liters', 0.85, 1, '3', 0),
    ('prod-chem-001', 'Sulfuric Acid 98%', 'ACID-H2SO4-98', '98% concentrated sulfuric acid', 'cat-chem-001', 'Liters', 1.84, 1, '8', 0);
```

### Version Upgrade Scripts

```sql
-- Version 1.1: Add new columns
ALTER TABLE Products ADD EnvironmentalRating NVARCHAR(10) NULL;
ALTER TABLE Products ADD CarbonFootprint DECIMAL(18,4) NULL;

-- Version 1.2: Add new table
CREATE TABLE ProductCertifications (
    Id NVARCHAR(450) PRIMARY KEY,
    ProductId NVARCHAR(450) NOT NULL,
    CertificationType NVARCHAR(50) NOT NULL,
    CertificationBody NVARCHAR(100) NOT NULL,
    CertificateNumber NVARCHAR(50) NOT NULL,
    IssuedDate DATETIME2 NOT NULL,
    ExpirationDate DATETIME2 NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CONSTRAINT FK_ProductCertifications_Products 
        FOREIGN KEY (ProductId) REFERENCES Products(Id)
);
```

## Performance Considerations

### Query Optimization

1. **Index Usage**
   - Ensure queries use appropriate indexes
   - Monitor index usage statistics
   - Remove unused indexes to improve write performance

2. **Query Patterns**
   ```sql
   -- Efficient category hierarchy query
   WITH CategoryHierarchy AS (
       SELECT Id, Name, ParentCategoryId, 0 as Level
       FROM ProductCategories 
       WHERE ParentCategoryId IS NULL AND IsDeleted = 0
       
       UNION ALL
       
       SELECT c.Id, c.Name, c.ParentCategoryId, h.Level + 1
       FROM ProductCategories c
       INNER JOIN CategoryHierarchy h ON c.ParentCategoryId = h.Id
       WHERE c.IsDeleted = 0
   )
   SELECT * FROM CategoryHierarchy;
   ```

3. **Pagination**
   ```sql
   -- Efficient pagination
   SELECT * FROM Products 
   WHERE IsDeleted = 0
   ORDER BY Name
   OFFSET @PageSize * (@PageNumber - 1) ROWS
   FETCH NEXT @PageSize ROWS ONLY;
   ```

### Database Tuning

1. **Statistics Management**
   ```sql
   -- Update statistics regularly
   UPDATE STATISTICS Products;
   UPDATE STATISTICS ProductCategories;
   ```

2. **Maintenance Tasks**
   ```sql
   -- Rebuild indexes periodically
   ALTER INDEX ALL ON Products REBUILD;
   
   -- Clean up soft-deleted records
   DELETE FROM Products WHERE IsDeleted = 1 AND UpdatedAt < DATEADD(YEAR, -1, GETUTCDATE());
   ```

3. **Monitoring Queries**
   ```sql
   -- Check for missing indexes
   SELECT 
       migs.avg_total_user_cost * (migs.avg_user_impact / 100.0) * (migs.user_seeks + migs.user_scans) AS improvement_measure,
       'CREATE INDEX [missing_index_' + CONVERT(VARCHAR, mig.index_group_handle) + '_' + CONVERT(VARCHAR, mid.index_handle) + ']'
       + ' ON ' + mid.statement + ' (' + ISNULL(mid.equality_columns,'')
       + CASE WHEN mid.equality_columns IS NOT NULL AND mid.inequality_columns IS NOT NULL THEN ',' ELSE '' END
       + ISNULL(mid.inequality_columns, '') + ')' + ISNULL(' INCLUDE (' + mid.included_columns + ')', '') AS create_index_statement
   FROM sys.dm_db_missing_index_groups mig
   INNER JOIN sys.dm_db_missing_index_group_stats migs ON migs.group_handle = mig.index_group_handle
   INNER JOIN sys.dm_db_missing_index_details mid ON mig.index_handle = mid.index_handle
   WHERE migs.avg_total_user_cost * (migs.avg_user_impact / 100.0) * (migs.user_seeks + migs.user_scans) > 10
   ORDER BY improvement_measure DESC;
   ```

---

*This database schema documentation is maintained alongside the Product Service codebase and reflects the current Entity Framework Core model definitions.*