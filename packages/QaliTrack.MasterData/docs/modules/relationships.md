# Relationships Module

The Relationships module manages complex cross-module relationships within the QaliTrack system. This module addresses the critical missing relationships identified during the system consolidation analysis.

## Overview

This module provides comprehensive relationship management between different entities across various modules, enabling proper data integrity and business process support.

## Supported Relationships

### Driver-SACCO Membership
Manages the membership relationship between drivers and SACCO organizations.

**Key Features:**
- Membership tracking with dates and status
- Share contribution management
- Monthly contribution tracking
- Membership benefits management
- Expiry date handling

**Endpoints:**
- `GET /relationships/driver-sacco-memberships` - List all memberships
- `POST /relationships/driver-sacco-memberships` - Create new membership
- `GET /relationships/driver-sacco-memberships/{id}` - Get specific membership
- `PUT /relationships/driver-sacco-memberships/{id}` - Update membership
- `PATCH /relationships/driver-sacco-memberships/{id}` - Partially update membership
- `DELETE /relationships/driver-sacco-memberships/{id}` - Delete membership

### Vehicle-Transporter Ownership
Manages ownership relationships between vehicles and transporter organizations.

**Key Features:**
- Ownership period tracking
- Purchase price and current value management
- Financing details tracking
- Insurance information management
- Ownership type classification

### Driver-Vehicle Assignment
Manages current and historical vehicle assignments for drivers.

**Key Features:**
- Primary driver designation
- Assignment period tracking
- Assignment type classification
- Assignment reasoning
- Active assignment status

### Driver-Transporter Employment
Manages employment relationships between drivers and transporter companies.

**Key Features:**
- Employment period tracking
- Salary and position management
- Department and reporting structure
- Employment status tracking
- Termination reason recording

### Product-Supplier Catalog
Manages product availability and pricing from different suppliers.

**Key Features:**
- Pricing and currency management
- Minimum/maximum order quantities
- Lead time tracking
- Quality rating system
- Discount and payment terms

### Route-Weighbridge Association
Manages the association between transportation routes and weighbridge stations.

**Key Features:**
- Sequence order management
- Mandatory vs optional weighbridge stops
- Distance and duration estimation
- Special conditions and instructions
- Association type classification

### Vehicle-SACCO Registration
Manages vehicle registration with SACCO organizations.

**Key Features:**
- Registration period tracking
- Registration fee management
- Certificate number tracking
- Registration conditions
- Expiry date management

## API Patterns

All relationship endpoints follow consistent patterns:

### CRUD Operations
- **Create**: POST with Create*Dto
- **Read**: GET with filtering and pagination
- **Update**: PUT with Update*Dto (full update)
- **Patch**: PATCH with Patch*Dto (partial update)
- **Delete**: DELETE (soft delete with IsDeleted flag)

### Response Format
All endpoints return data in the standardized ApiResponse format:
```json
{
  "success": true,
  "message": "Operation successful",
  "data": { ... },
  "errors": null,
  "timestamp": "2024-01-01T00:00:00Z"
}
```

### Pagination
List endpoints support Django-style pagination:
```
GET /relationships/driver-sacco-memberships?page=1&page_size=10&sort_by=created_at&sort_order=desc
```

## Entity Relationships

The module uses Entity Framework Core navigation properties to maintain referential integrity:

```csharp
public class DriverSaccoMembership : BaseEntity
{
    public Guid DriverId { get; set; }
    public Guid SaccoId { get; set; }
    
    // Navigation properties
    public virtual Driver Driver { get; set; }
    public virtual Sacco Sacco { get; set; }
}
```

## Integration Points

### Cross-Module Endpoints
The relationships are also exposed through contextual endpoints on related modules:

- `GET /saccos/{id}/drivers` - Get all drivers in a SACCO
- `GET /saccos/{id}/vehicles` - Get all vehicles registered with a SACCO
- `GET /drivers/{id}/vehicles` - Get driver's vehicle assignments
- `GET /vehicles/{id}/drivers` - Get vehicle's driver assignments

## Business Rules

### Data Integrity
- Foreign key relationships are enforced through Entity Framework
- Soft deletes maintain historical data integrity
- Audit trails track all relationship changes

### Multi-Tenancy
- All relationships respect organization boundaries
- OrganizationId filtering ensures data isolation

## Future Enhancements

- Workflow approvals for relationship changes
- Automated relationship expiry notifications
- Bulk relationship management operations
- Advanced reporting and analytics