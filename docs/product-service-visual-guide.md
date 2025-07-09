# Product Service Visual Guide

## 🎯 **Service Overview**

### **Business Purpose**
The Product Service manages QaliTrack's product catalog, including cement products, aggregates, and related materials. It provides centralized product information, specifications, pricing, and quality standards for the entire logistics ecosystem.

### **Key Business Problems Solved**
- **Product Standardization**: Unified product catalog across all plants and operations
- **Pricing Management**: Centralized pricing with regional variations
- **Quality Control**: Product specifications and quality standards enforcement
- **Order Validation**: Ensures ordered products exist and meet specifications
- **Inventory Planning**: Product information for inventory management decisions

### **Integration Role**
Central product reference service that validates product information for orders, inventory, and quality control processes.

---

## 🏗️ **Architecture Diagram**

```
┌─────────────────────────────────────────────────────────────────┐
│                        Product Service                          │
│                         Port: 7005                              │
└─────────────────────────────────────────────────────────────────┘
┌─────────────────────────────────────────────────────────────────┐
│                          API Layer                              │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐   │
│  │   ProductsController   │  │  CategoriesController │  │  PricingController │   │
│  │  - CRUD operations     │  │  - Category management │  │  - Price management │   │
│  │  - Search & filter     │  │  - Hierarchy operations │  │  - Region-based     │   │
│  │  - Bulk operations     │  │  - Product assignments  │  │  - Bulk pricing     │   │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘   │
└─────────────────────────────────────────────────────────────────┘
┌─────────────────────────────────────────────────────────────────┐
│                         Core Layer                              │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐   │
│  │   ProductService   │  │  CategoryService    │  │  PricingService     │   │
│  │  - Product logic   │  │  - Category logic   │  │  - Price calculation │   │
│  │  - Validation      │  │  - Hierarchy mgmt   │  │  - Region handling   │   │
│  │  - Business rules  │  │  - Assignment logic │  │  - Bulk operations   │   │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘   │
│                                                                 │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐   │
│  │     Product        │  │     Category        │  │     ProductPrice    │   │
│  │  - Id, Name        │  │  - Id, Name         │  │  - Id, ProductId    │   │
│  │  - Code, SKU       │  │  - Code, Parent     │  │  - Region, Price    │   │
│  │  - CategoryId      │  │  - Level, Active    │  │  - Currency, Dates  │   │
│  │  - Specifications  │  │  - CreatedAt        │  │  - CreatedAt        │   │
│  │  - IsActive        │  │                     │  │                     │   │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘   │
└─────────────────────────────────────────────────────────────────┘
┌─────────────────────────────────────────────────────────────────┐
│                    Infrastructure Layer                         │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐   │
│  │  ProductRepository │  │ CategoryRepository │  │ PricingRepository   │   │
│  │  - CRUD operations │  │  - CRUD operations │  │  - CRUD operations  │   │
│  │  - Search queries  │  │  - Hierarchy queries │  │  - Price queries    │   │
│  │  - Bulk operations │  │  - Parent/child ops │  │  - Region filtering │   │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘   │
│                                                                 │
│  ┌─────────────────────────────────────────────────────────────┐  │
│  │                ProductDbContext                              │  │
│  │  - Products, Categories, ProductPrices Tables               │  │
│  │  - Entity configurations and relationships                  │  │
│  │  - Database migrations and seeding                          │  │
│  └─────────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
```

---

## 📊 **Entity Model**

### **Core Entities**

#### **Product Entity**
```csharp
public class Product : BaseEntity
{
    public string Name { get; set; }                    // "Portland Cement Grade 42.5"
    public string Code { get; set; }                    // "PC-425"
    public string SKU { get; set; }                     // "QT-CEM-PC425-50KG"
    public string CategoryId { get; set; }              // References Category
    public string Description { get; set; }             // Detailed description
    public ProductSpecifications Specifications { get; set; }
    public string Unit { get; set; }                    // "Bags", "Tons", "Cubic Meters"
    public decimal StandardWeight { get; set; }         // 50.0 (kg per bag)
    public string QualityGrade { get; set; }            // "Grade 42.5", "Grade 32.5"
    public bool IsActive { get; set; }                  // Product availability
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedBy { get; set; }
    public string UpdatedBy { get; set; }
}

public class ProductSpecifications
{
    public string CompressiveStrength { get; set; }     // "42.5 MPa"
    public string ChemicalComposition { get; set; }     // "CaO: 60-67%, SiO2: 20-25%"
    public string PhysicalProperties { get; set; }      // "Fineness: 225-275 m²/kg"
    public string QualityStandards { get; set; }        // "KEBS KS 02-1055:2017"
    public string StorageRequirements { get; set; }     // "Dry, covered storage"
    public string PackagingOptions { get; set; }        // "50kg bags, 1.5T bulk bags"
}
```

#### **Category Entity**
```csharp
public class Category : BaseEntity
{
    public string Name { get; set; }                    // "Cement Products"
    public string Code { get; set; }                    // "CEM"
    public string? ParentId { get; set; }               // Hierarchical structure
    public int Level { get; set; }                      // 0=Root, 1=Sub, 2=Sub-sub
    public string Description { get; set; }             // Category description
    public bool IsActive { get; set; }                  // Category availability
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedBy { get; set; }
    public string UpdatedBy { get; set; }
    
    // Navigation properties
    public Category? Parent { get; set; }
    public List<Category> Children { get; set; }
    public List<Product> Products { get; set; }
}
```

#### **ProductPrice Entity**
```csharp
public class ProductPrice : BaseEntity
{
    public string ProductId { get; set; }               // References Product
    public string Region { get; set; }                  // "Nairobi", "Mombasa", "Kisumu"
    public decimal Price { get; set; }                  // 650.00
    public string Currency { get; set; }                // "KES"
    public decimal? MinQuantity { get; set; }           // Minimum order quantity
    public decimal? MaxQuantity { get; set; }           // Maximum order quantity
    public DateTime EffectiveDate { get; set; }         // Price effective from
    public DateTime? ExpiryDate { get; set; }           // Price valid until
    public bool IsActive { get; set; }                  // Price availability
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedBy { get; set; }
    public string UpdatedBy { get; set; }
    
    // Navigation properties
    public Product Product { get; set; }
}
```

### **Entity Relationships**

```
┌─────────────────┐         ┌─────────────────┐         ┌─────────────────┐
│    Category     │◄─────┐  │     Product     │         │   ProductPrice  │
│                 │      │  │                 │         │                 │
│ • Id            │      └──┤ • CategoryId    │◄────────┤ • ProductId     │
│ • Name          │         │ • Name          │         │ • Region        │
│ • Code          │         │ • Code          │         │ • Price         │
│ • ParentId      │         │ • SKU           │         │ • Currency      │
│ • Level         │         │ • Description   │         │ • EffectiveDate │
│ • Description   │         │ • Unit          │         │ • ExpiryDate    │
│ • IsActive      │         │ • StandardWeight│         │ • IsActive      │
└─────────────────┘         │ • QualityGrade  │         └─────────────────┘
                            │ • Specifications│
                            │ • IsActive      │
                            └─────────────────┘
```

---

## 🔌 **API Endpoints**

### **Product Management**

#### **GET /api/products**
**Purpose**: Retrieve paginated product list with filtering
```json
// Request
GET /api/products?page=1&size=10&category=CEM&search=cement&active=true

// Response
{
  "data": [
    {
      "id": "prod-001",
      "name": "Portland Cement Grade 42.5",
      "code": "PC-425",
      "sku": "QT-CEM-PC425-50KG",
      "categoryId": "cat-001",
      "categoryName": "Cement Products",
      "description": "High-quality Portland cement for construction",
      "unit": "Bags",
      "standardWeight": 50.0,
      "qualityGrade": "Grade 42.5",
      "specifications": {
        "compressiveStrength": "42.5 MPa",
        "chemicalComposition": "CaO: 60-67%, SiO2: 20-25%",
        "physicalProperties": "Fineness: 225-275 m²/kg",
        "qualityStandards": "KEBS KS 02-1055:2017",
        "storageRequirements": "Dry, covered storage",
        "packagingOptions": "50kg bags, 1.5T bulk bags"
      },
      "isActive": true,
      "createdAt": "2024-01-15T10:30:00Z",
      "updatedAt": "2024-01-20T14:45:00Z"
    }
  ],
  "pagination": {
    "page": 1,
    "size": 10,
    "total": 45,
    "totalPages": 5
  }
}
```

#### **GET /api/products/{id}**
**Purpose**: Retrieve specific product details
```json
// Request
GET /api/products/prod-001

// Response
{
  "id": "prod-001",
  "name": "Portland Cement Grade 42.5",
  "code": "PC-425",
  "sku": "QT-CEM-PC425-50KG",
  "categoryId": "cat-001",
  "description": "High-quality Portland cement for construction",
  "unit": "Bags",
  "standardWeight": 50.0,
  "qualityGrade": "Grade 42.5",
  "specifications": {
    "compressiveStrength": "42.5 MPa",
    "chemicalComposition": "CaO: 60-67%, SiO2: 20-25%",
    "physicalProperties": "Fineness: 225-275 m²/kg",
    "qualityStandards": "KEBS KS 02-1055:2017",
    "storageRequirements": "Dry, covered storage",
    "packagingOptions": "50kg bags, 1.5T bulk bags"
  },
  "isActive": true,
  "createdAt": "2024-01-15T10:30:00Z",
  "updatedAt": "2024-01-20T14:45:00Z"
}
```

#### **POST /api/products**
**Purpose**: Create new product
```json
// Request
POST /api/products
{
  "name": "Portland Cement Grade 32.5",
  "code": "PC-325",
  "sku": "QT-CEM-PC325-50KG",
  "categoryId": "cat-001",
  "description": "Standard Portland cement for general construction",
  "unit": "Bags",
  "standardWeight": 50.0,
  "qualityGrade": "Grade 32.5",
  "specifications": {
    "compressiveStrength": "32.5 MPa",
    "chemicalComposition": "CaO: 58-65%, SiO2: 20-25%",
    "physicalProperties": "Fineness: 225-275 m²/kg",
    "qualityStandards": "KEBS KS 02-1055:2017",
    "storageRequirements": "Dry, covered storage",
    "packagingOptions": "50kg bags, 1.5T bulk bags"
  },
  "isActive": true
}

// Response
{
  "id": "prod-002",
  "name": "Portland Cement Grade 32.5",
  "code": "PC-325",
  "sku": "QT-CEM-PC325-50KG",
  "categoryId": "cat-001",
  "description": "Standard Portland cement for general construction",
  "unit": "Bags",
  "standardWeight": 50.0,
  "qualityGrade": "Grade 32.5",
  "specifications": {
    "compressiveStrength": "32.5 MPa",
    "chemicalComposition": "CaO: 58-65%, SiO2: 20-25%",
    "physicalProperties": "Fineness: 225-275 m²/kg",
    "qualityStandards": "KEBS KS 02-1055:2017",
    "storageRequirements": "Dry, covered storage",
    "packagingOptions": "50kg bags, 1.5T bulk bags"
  },
  "isActive": true,
  "createdAt": "2024-01-22T09:15:00Z",
  "updatedAt": "2024-01-22T09:15:00Z"
}
```

#### **PUT /api/products/{id}**
**Purpose**: Update existing product
```json
// Request
PUT /api/products/prod-002
{
  "name": "Portland Cement Grade 32.5 - Updated",
  "code": "PC-325",
  "sku": "QT-CEM-PC325-50KG",
  "categoryId": "cat-001",
  "description": "Standard Portland cement for general construction - Updated formula",
  "unit": "Bags",
  "standardWeight": 50.0,
  "qualityGrade": "Grade 32.5",
  "specifications": {
    "compressiveStrength": "32.5 MPa",
    "chemicalComposition": "CaO: 58-65%, SiO2: 20-25%",
    "physicalProperties": "Fineness: 225-275 m²/kg",
    "qualityStandards": "KEBS KS 02-1055:2017",
    "storageRequirements": "Dry, covered storage",
    "packagingOptions": "50kg bags, 1.5T bulk bags"
  },
  "isActive": true
}

// Response: Updated product object
```

#### **DELETE /api/products/{id}**
**Purpose**: Soft delete product (mark as inactive)
```json
// Request
DELETE /api/products/prod-002

// Response
{
  "message": "Product deactivated successfully",
  "productId": "prod-002"
}
```

### **Category Management**

#### **GET /api/categories**
**Purpose**: Retrieve category hierarchy
```json
// Request
GET /api/categories?includeHierarchy=true

// Response
{
  "data": [
    {
      "id": "cat-001",
      "name": "Cement Products",
      "code": "CEM",
      "parentId": null,
      "level": 0,
      "description": "All cement-related products",
      "isActive": true,
      "children": [
        {
          "id": "cat-002",
          "name": "Portland Cement",
          "code": "PC",
          "parentId": "cat-001",
          "level": 1,
          "description": "Portland cement varieties",
          "isActive": true,
          "children": [
            {
              "id": "cat-003",
              "name": "Grade 42.5",
              "code": "PC-425",
              "parentId": "cat-002",
              "level": 2,
              "description": "High-strength Portland cement",
              "isActive": true,
              "children": []
            }
          ]
        }
      ]
    }
  ]
}
```

#### **POST /api/categories**
**Purpose**: Create new category
```json
// Request
POST /api/categories
{
  "name": "Aggregates",
  "code": "AGG",
  "parentId": null,
  "description": "Construction aggregates and materials",
  "isActive": true
}

// Response: Created category object
```

### **Pricing Management**

#### **GET /api/products/{id}/prices**
**Purpose**: Retrieve product pricing by region
```json
// Request
GET /api/products/prod-001/prices?region=Nairobi

// Response
{
  "data": [
    {
      "id": "price-001",
      "productId": "prod-001",
      "region": "Nairobi",
      "price": 650.00,
      "currency": "KES",
      "minQuantity": 1,
      "maxQuantity": 1000,
      "effectiveDate": "2024-01-01T00:00:00Z",
      "expiryDate": "2024-12-31T23:59:59Z",
      "isActive": true,
      "createdAt": "2024-01-15T10:30:00Z",
      "updatedAt": "2024-01-20T14:45:00Z"
    }
  ]
}
```

#### **POST /api/products/{id}/prices**
**Purpose**: Add new price for product
```json
// Request
POST /api/products/prod-001/prices
{
  "region": "Mombasa",
  "price": 630.00,
  "currency": "KES",
  "minQuantity": 1,
  "maxQuantity": 1000,
  "effectiveDate": "2024-01-01T00:00:00Z",
  "expiryDate": "2024-12-31T23:59:59Z",
  "isActive": true
}

// Response: Created price object
```

### **Business-Specific Endpoints**

#### **GET /api/products/by-category/{categoryId}**
**Purpose**: Retrieve products by category
```json
// Request
GET /api/products/by-category/cat-001?includeSubcategories=true

// Response: List of products in category and subcategories
```

#### **GET /api/products/search**
**Purpose**: Advanced product search
```json
// Request
GET /api/products/search?q=cement&category=CEM&priceRange=500-800&region=Nairobi

// Response: Filtered product list with pricing
```

#### **POST /api/products/validate**
**Purpose**: Validate product for order
```json
// Request
POST /api/products/validate
{
  "productId": "prod-001",
  "quantity": 100,
  "region": "Nairobi"
}

// Response
{
  "isValid": true,
  "product": {
    "id": "prod-001",
    "name": "Portland Cement Grade 42.5",
    "unit": "Bags",
    "standardWeight": 50.0
  },
  "pricing": {
    "region": "Nairobi",
    "price": 650.00,
    "currency": "KES",
    "totalPrice": 65000.00
  },
  "availability": {
    "isAvailable": true,
    "message": "Product available for order"
  }
}
```

---

## 🔗 **Integration Points**

### **Service Dependencies**

#### **Customer Service Integration**
```
┌─────────────────┐         ┌─────────────────┐
│ Customer Service│────────►│ Product Service │
│                 │         │                 │
│ • Orders        │         │ • Product Info  │
│ • ProductId     │         │ • Price Lookup  │
│ • Validation    │         │ • Availability  │
└─────────────────┘         └─────────────────┘
```

**Integration Flow:**
1. Customer Service validates ProductId during order creation
2. Product Service returns product details and pricing
3. Customer Service stores ProductId reference (no duplication)
4. Order processing uses ProductId for inventory and fulfillment

#### **Future Integrations**

**Inventory Service** (Future)
```
┌─────────────────┐         ┌─────────────────┐
│ Product Service │────────►│ Inventory Service│
│                 │         │                 │
│ • Product Info  │         │ • Stock Levels  │
│ • Specifications│         │ • Availability  │
│ • Categories    │         │ • Reservations  │
└─────────────────┘         └─────────────────┘
```

**Quality Service** (Future)
```
┌─────────────────┐         ┌─────────────────┐
│ Product Service │────────►│ Quality Service │
│                 │         │                 │
│ • Quality Grade │         │ • Test Results  │
│ • Standards     │         │ • Certificates │
│ • Specifications│         │ • Compliance    │
└─────────────────┘         └─────────────────┘
```

### **Integration Patterns**

#### **Product Validation Pattern**
```csharp
// Customer Service -> Product Service
public async Task<bool> ValidateProductAsync(string productId, int quantity, string region)
{
    var productResponse = await _productService.ValidateProductAsync(new ProductValidationRequest
    {
        ProductId = productId,
        Quantity = quantity,
        Region = region
    });
    
    return productResponse.IsValid;
}
```

#### **Price Lookup Pattern**
```csharp
// Customer Service -> Product Service
public async Task<decimal> GetProductPriceAsync(string productId, string region)
{
    var priceResponse = await _productService.GetProductPriceAsync(productId, region);
    return priceResponse.Price;
}
```

---

## 💼 **Business Use Cases**

### **Use Case 1: Order Product Validation**

**Scenario**: Customer places order for cement products
**Actors**: Customer, Sales Representative, System
**Flow**:
1. Customer selects "Portland Cement Grade 42.5" from catalog
2. System validates product availability in customer's region
3. System retrieves current pricing for the region
4. System confirms product specifications meet order requirements
5. Order is created with valid ProductId reference

**Business Value**: Ensures orders contain valid products with correct pricing

### **Use Case 2: Regional Price Management**

**Scenario**: Sales manager updates product prices for different regions
**Actors**: Sales Manager, Regional Managers, System
**Flow**:
1. Sales manager accesses Product Service pricing interface
2. Manager selects product and region combination
3. System displays current price and validity period
4. Manager updates price with new effective date
5. System validates price changes and updates regional pricing
6. All new orders use updated pricing automatically

**Business Value**: Centralized price management with regional flexibility

### **Use Case 3: Product Catalog Expansion**

**Scenario**: Company introduces new cement grade
**Actors**: Product Manager, Technical Team, System
**Flow**:
1. Product manager creates new product category if needed
2. Technical team defines product specifications and quality standards
3. Product manager adds new product to catalog
4. Regional pricing is set for all operating regions
5. Product becomes available for customer orders
6. Sales team is notified of new product availability

**Business Value**: Systematic product introduction with complete specification management

### **Use Case 4: Quality Standards Compliance**

**Scenario**: Quality team verifies product specifications
**Actors**: Quality Manager, Production Team, System
**Flow**:
1. Quality manager accesses product specifications
2. System displays current quality standards and requirements
3. Manager compares with production batch test results
4. Product specifications are updated if standards change
5. All stakeholders are notified of specification updates
6. Future orders reference updated specifications

**Business Value**: Ensures product quality consistency and compliance

---

## 🛣️ **Implementation Roadmap**

### **Phase 1: Foundation** (Month 1, Week 1-2)
- ✅ **Project Setup**: Create Clean Architecture structure
- ✅ **Core Entities**: Implement Product, Category, ProductPrice entities
- ✅ **Database Layer**: Set up Entity Framework with SQLite
- ✅ **Basic Repository**: Implement repository pattern for data access

### **Phase 2: Core Features** (Month 1, Week 3-4)
- ✅ **Product Management**: Complete CRUD operations for products
- ✅ **Category Management**: Implement hierarchical category system
- ✅ **Pricing System**: Regional pricing with effective dates
- ✅ **API Controllers**: REST API endpoints for all operations

### **Phase 3: Integration** (Month 2, Week 1-2)
- 🔄 **Customer Service Integration**: Product validation endpoints
- 🔄 **Search & Filtering**: Advanced product search capabilities
- 🔄 **Bulk Operations**: Batch product and pricing operations
- 🔄 **API Documentation**: Comprehensive OpenAPI documentation

### **Phase 4: Advanced Features** (Month 2, Week 3-4)
- 🔄 **Quality Standards**: Enhanced specification management
- 🔄 **Audit Trail**: Track all product and pricing changes
- 🔄 **Performance Optimization**: Caching and query optimization
- 🔄 **Monitoring**: Health checks and performance metrics

### **Current Status**: 🔄 **READY FOR IMPLEMENTATION**
- **Priority**: HIGH (Next service to implement)
- **Dependencies**: Customer Service (for integration)
- **Estimated Timeline**: 1 month for complete implementation
- **Resource Requirements**: 1 developer, part-time

---

## 📊 **Success Metrics**

### **Technical Metrics**
- **API Response Time**: < 200ms for product lookups
- **Database Performance**: < 100ms for complex category queries
- **Service Availability**: 99.9% uptime
- **Integration Success**: 100% validation accuracy for orders

### **Business Metrics**
- **Product Catalog**: 100+ products in system
- **Regional Pricing**: 5+ regions with differential pricing
- **Category Hierarchy**: 3+ levels deep for cement products
- **Order Integration**: 100% of orders validate through Product Service

### **Quality Metrics**
- **Data Accuracy**: 99.9% product information accuracy
- **Specification Compliance**: 100% adherence to quality standards
- **Price Consistency**: 0% pricing discrepancies between regions
- **System Reliability**: 0 data loss incidents

---

*Product Service serves as the central product catalog and pricing authority for the entire QaliTrack ecosystem, enabling accurate order processing, pricing management, and quality assurance across all business operations.*