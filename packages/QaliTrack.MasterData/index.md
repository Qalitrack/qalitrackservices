---
_layout: landing
---

# QaliTrack Master Data Service

## Overview

The QaliTrack Master Data Service is a comprehensive microservice that manages all master data entities within the QaliTrack ecosystem. This service provides centralized management for business entities, vehicles, drivers, routes, weighbridges, and their complex relationships.

## Architecture

The service follows a clean architecture pattern with the following layers:

- **API Layer**: RESTful controllers providing HTTP endpoints
- **Core Layer**: Business logic, entities, DTOs, and interfaces
- **Infrastructure Layer**: Data access, repositories, and external service integrations

## Key Features

### Core Entities
- **Business Entities**: Companies, customers, suppliers, transporters
- **Vehicles**: Fleet management and vehicle tracking
- **Drivers**: Driver management and certification tracking
- **Routes**: Transportation route definitions
- **Weighbridges**: Weighing station management
- **Organizations**: Company and organization hierarchy
- **Products**: Product catalog and specifications
- **SACCOs**: Savings and Credit Cooperative Organizations

### Cross-Module Relationships
- Driver-SACCO memberships
- Vehicle-Transporter ownership
- Driver-Vehicle assignments
- Driver-Transporter employment
- Product-Supplier catalogs
- Route-Weighbridge associations
- Vehicle-SACCO registrations

### API Features
- Full CRUD operations for all entities
- Comprehensive relationship management
- Django-style pagination and filtering
- Multi-tenancy support with OrganizationId
- Standardized API response format
- Complete HTTP method support (GET, POST, PUT, PATCH, DELETE, HEAD, OPTIONS)

## Getting Started

### Prerequisites
- .NET 8.0 or later
- SQL Server, PostgreSQL, or SQLite for development
- Docker (optional)

### Installation

1. Clone the repository
2. Navigate to the QaliTrack.MasterData directory
3. Restore dependencies: `dotnet restore`
4. Run migrations: `dotnet ef database update`
5. Start the service: `dotnet run --project src/QaliTrack.MasterData.Api`

### API Documentation

The service exposes a comprehensive REST API with Swagger documentation available at `/swagger` when running in development mode.

## Modules

- [Business Entities](xref:QaliTrack.MasterData.Core.Modules.BusinessEntities) - Company and organization management
- [Vehicles](xref:QaliTrack.MasterData.Core.Modules.Vehicle) - Fleet management
- [Drivers](xref:QaliTrack.MasterData.Core.Modules.Driver) - Driver management
- [Routes](xref:QaliTrack.MasterData.Core.Modules.Route) - Transportation routes
- [Weighbridges](xref:QaliTrack.MasterData.Core.Modules.Weighbridge) - Weighing stations
- [Products](xref:QaliTrack.MasterData.Core.Modules.Product) - Product catalog
- [SACCOs](xref:QaliTrack.MasterData.Core.Modules.Sacco) - Cooperative organizations
- [Organizations](xref:QaliTrack.MasterData.Core.Modules.Organization) - Organizational hierarchy
- [Relationships](xref:QaliTrack.MasterData.Core.Modules.Relationships) - Cross-module relationships

## API Reference

For detailed API documentation, please refer to the [API Reference](api/index.md) section.

## Support

For support and questions, please refer to the project documentation or contact the development team.