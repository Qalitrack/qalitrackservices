# Master Data Service Documentation

## Overview
This service manages all master data entities required for operational and regulatory processes in industries, factories, and national weighbridges. It centralizes and standardizes data for vehicles, suppliers, transporters, drivers, products, routes, Saccos, and weighbridges, ensuring consistency and reliability across the system.

## Features
- **Vehicle Management:** Register, update, and deactivate vehicles
- **Supplier Management:** Maintain supplier records and contact information  
- **Customer Management:** Maintain customer records and contact information
- **Transporter Management:** Manage transporter companies and their details
- **Driver Management:** Register and manage driver profiles and licenses, including expiry dates
- **Product Management:** Define and update product master data
- **Route Management:** Create and manage transport routes
- **Sacco Management:** Register and manage Saccos (Savings and Credit Cooperative Organizations)
- **Company/Organisation Management:** Define company management/organisation details
- **Weighbridge Management:** Register and maintain weighbridge locations and details

## Architecture

### Service Structure
- **Service Layer:** Handles business logic for each entity (VehicleService, SupplierService, etc.)
- **Repository Layer:** Handles database operations (VehicleRepository, etc.)
- **Controller Layer:** Exposes RESTful endpoints (VehicleController, etc.)
- **DTOs/Models:** Data transfer objects and validation schemas

### Database Schema (PostgreSQL)

#### Core Entities

**Vehicle**
- id (UUID, PK)
- registration_number (VARCHAR, unique)
- type (VARCHAR)
- owner_id (FK to Supplier/Transporter/Driver)
- status (active/inactive)
- color, model
- axle_configuration_id (FK)
- created_at, updated_at

**Supplier**
- id (UUID, PK)
- name (VARCHAR)
- contact_info (JSONB)
- status (active/inactive)
- logo
- created_at, updated_at

**Driver**
- id (UUID, PK)
- full_name (VARCHAR)
- license_number (VARCHAR)
- transporter_id (FK)
- supplier_id (FK)
- status (active/inactive)
- created_at, updated_at

**Product**
- id (UUID, PK)
- code (VARCHAR, unique)
- name (VARCHAR)
- description (TEXT)
- image
- created_at, updated_at

**Route**
- id (UUID, PK)
- name (VARCHAR)
- start_point (VARCHAR)
- end_point (VARCHAR)
- status (active/inactive)
- created_at, updated_at

## API Integration
- **API-First:** All master data entities expose RESTful APIs
- **Event-Driven:** Optional message queue for master data change notifications
- **Authentication:** JWT or OAuth2 secured APIs

## Reporting Features
- Generate reports from all master data entities
- Filterable reports (status, date range, entity type)
- Exportable formats (CSV, PDF)
- Authorization-based access control

### Report Endpoints
- `GET /reports/vehicles?status=active`
- `GET /reports/suppliers?export=csv`
- `GET /reports/weighbridges?from=2024-01-01&to=2024-06-30`

## Technology Stack
- **Backend:** C#/.NET
- **Database:** PostgreSQL
- **Authentication:** JWT
- **Documentation:** Swagger/OpenAPI
- **Containerization:** Docker
- **CI/CD:** GitHub Actions/Azure DevOps

## Testing & Deployment
- **Unit Testing:** xUnit for service and repository layers
- **Integration Testing:** API endpoints and database interactions
- **Monitoring:** Logging and monitoring for all services
- **Automation:** Build, test, and deployment pipelines