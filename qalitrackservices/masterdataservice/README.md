# Master Data Service (v1.0.0)

## Overview
This service manages all master data entities required for operational and regulatory processes in industries, factories, and national weighbridges. It centralizes and standardizes data for vehicles, suppliers, transporters, drivers, products, routes, Saccos, and weighbridges, ensuring consistency and reliability across the system.


## Features
- **Vehicle Management:** Register, update, and deactivate vehicles.
- **Supplier Management:** Maintain supplier records and contact information.
- **Customer Management:** Maintain customer records and contact information.
- **Transporter Management:** Manage transporter companies and their details.
- **Driver Management:** Register and manage driver profiles and licenses, including expiry dates
- **Product Management:** Define and update product master data.
- **Route Management:** Create and manage transport routes.
- **Sacco Management:** Register and manage Saccos (Savings and Credit Cooperative Organizations). Include a way to add other affiliations like organisations they might be in that particular country 
-**Company/Organisation Management** Define company management/organisation details
- **Weighbridge Management:** Register and maintain weighbridge locations and details.

---

## Detailed Requirements & User Stories

### 1. Vehicle Management
**Requirement:**
- The system must allow registration, update, and deactivation of vehicles.
- Vehicles must be uniquely identifiable (e.g., by registration number).

**User Stories:**
- As an admin, I want to register a new vehicle so it can be tracked in the system.
- As an admin, I want to update vehicle details to keep records current.


### 2. Supplier Management
**Requirement:**
- The system must store supplier information, including contact details and status.

**User Stories:**
- As an admin, I want to add new suppliers to the system for procurement and logistics.
- As an admin, I want to update supplier information as needed.
- As an admin, I want to deactivate suppliers who are no longer active.

### 3. Transporter Management
**Requirement:**
- The system must manage transporter companies and their associated vehicles and drivers.

**User Stories:**
- As an admin, I want to register transporter companies for logistics operations.
- As an admin, I want to link vehicles and drivers to transporters.
- As an admin, I want to update transporter details.

### 4. Driver Management
**Requirement:**
- The system must register and manage driver profiles, including license details and status.

**User Stories:**
- As an admin, I want to register drivers to assign them to vehicles and transporters.
- As an admin, I want to update driver information and license status.
- As an admin, I want to deactivate drivers who are no longer active.

### 5. Product Management
**Requirement:**
- The system must define and manage product master data, including product codes and descriptions.

**User Stories:**
- As an admin, I want to add new products to the master data for inventory and logistics.
- As an admin, I want to update product details.

### 6. Route Management
**Requirement:**
- The system must allow creation and management of transport routes, including start and end points.

**User Stories:**
- As an admin, I want to define routes for transport planning and tracking.
- As an admin, I want to update or deactivate routes as needed.

### 7. Sacco Management
**Requirement:**
- The system must register and manage Saccos, including their members and contact information.

**User Stories:**
- As an admin, I want to register Saccos for regulatory and operational purposes.
- As an admin, I want to update Sacco details.

### 8. Weighbridge Management
**Requirement:**
- The system must register and maintain weighbridge locations and operational details.

**User Stories:**
- As an admin, I want to add new weighbridges to the system for compliance and tracking.
- As an admin, I want to update weighbridge information.

---



## Extensibility
- Modular design for easy addition of new master data entities.
- API-first approach for integration with other services.

## Implementation Steps

### 1. Database Schema Design

**Entities & Example Schema (PostgreSQL):**

- **Vehicle**
  - id (UUID, PK)
  - registration_number (VARCHAR, unique)
  - type (VARCHAR)
  - owner_id (FK to Supplier/Transporter/Driver) should track its registered under whom
  - status (active/inactive)
  - created_at, updated_at
  - color
  - model
  - axle configuration (FK to axle configurations available)

- **Supplier**
  - id (UUID, PK)
  - name (VARCHAR)
  - contact_info (JSONB)
  - status (active/inactive)
  - created_at, updated_at
  - logo 

- **Transporter**
  - id (UUID, PK)
  - name (VARCHAR)
  - contact_info (JSONB)
  - status (active/inactive)
  - created_at, updated_at
  -logo
   

- **Driver**
  - id (UUID, PK)
  - full name (VARCHAR)
  - license_number (VARCHAR)
  - transporter_id (FK)
  - supplier_id(FK)
  - status (active/inactive)
  - created_at, updated_at

- **Product**
  - id (UUID, PK)
  - code (VARCHAR, unique)
  - name (VARCHAR)
  - description (TEXT)
  - created_at, updated_at
  - image


- **Route**
  - id (UUID, PK)
  - name (VARCHAR)
  - start_point (VARCHAR)
  - end_point (VARCHAR)
  - status (active/inactive)
  - created_at, updated_at

- **Sacco**
  - id (UUID, PK)
  - name (VARCHAR)
  - contact_info (JSONB)
  - created_at, updated_at
  - other details

- **Weighbridge**
  - id (UUID, PK)
  - location (VARCHAR)
  - description (TEXT)
  - status (active/inactive)
  - created_at, updated_at


### 3. Entity Management Module Structure

- **Service Layer:** Handles business logic for each entity (e.g., VehicleService, SupplierService).
- **Repository Layer:** Handles database operations (e.g., VehicleRepository).
- **Controller Layer:** Exposes RESTful endpoints (e.g., VehicleController).
- **DTOs/Models:** Data transfer objects and validation schemas.

**Example (C#/.NET):**
- `/Controllers/VehicleController.cs`
- `/Services/VehicleService.cs`
- `/Repositories/VehicleRepository.cs`
- `/Models/Vehicle.cs`

### 4. Integration Approach

- **API-First:** All master data entities expose RESTful APIs for use by other services (e.g., user management, audit, logistics).
- **Event-Driven (Optional):** Use events (e.g., via message queue) to notify other systems of master data changes.
- **Authentication:** Secure APIs with JWT or OAuth2.

### 5. Testing & Deployment Plan

- **Unit Testing:** Write tests for service and repository layers (e.g., using xUnit for .NET).
- **Integration Testing:** Test API endpoints and database interactions.
- **Documentation:** Use Swagger/OpenAPI for API docs.
- **CI/CD:** Automate build, test, and deployment (e.g., GitHub Actions, Azure DevOps).
- **Containerization:** Use Docker for consistent deployment environments.
- **Monitoring:** Set up logging and monitoring for all services.

---

_This document provides a blueprint for implementing the Master Data Service. Each step can be expanded into detailed technical tasks as development progresses._


## Reporting

### Requirements
- The system must support generating reports from all master data entities (vehicles, suppliers, transporters, customers,drivers, products, routes, Saccos, weighbridges).
- Reports should be filterable (e.g., by status, date range, entity type).
- Reports should be exportable (e.g., CSV, PDF).
- Only authorized users can generate and access reports.

### User Stories
- As an admin, I want to generate a list of all active vehicles for compliance checks.
- As a manager, I want to export supplier and transporter lists for operational planning.

- As a user, I want to filter reports by date, status, or other attributes.

### Implementation Steps
1. **Reporting Service**: Implement a dedicated service/module for report generation (e.g., `ReportingService`).
2. **API Endpoints**: Expose endpoints such as:
   - `GET /reports/vehicles?status=active`
   - `GET /reports/suppliers?export=csv`
   - `GET /reports/weighbridges?from=2024-01-01&to=2024-06-30`
3. **Filtering & Export**: Support query parameters for filtering and export format selection (CSV, PDF, etc.).
4. **Authorization**: Ensure only users with the correct permissions can access reporting endpoints. //handshake with the userservices and also audit logs
5. **Integration**: Reporting service can aggregate data from all master data entities, using direct DB queries or service APIs.
6. **Frontend**: (If applicable) Provide UI components for report selection, filtering, and export.

