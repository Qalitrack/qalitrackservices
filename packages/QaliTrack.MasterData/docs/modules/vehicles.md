# Vehicles Module

The Vehicles module manages all vehicle-related information in the QaliTrack system.

## Overview

This module provides comprehensive vehicle management capabilities including:
- Vehicle registration and documentation
- Maintenance scheduling and tracking
- Insurance management
- Inspection records
- Specification management

## Key Features

- Complete vehicle lifecycle management
- Document and compliance tracking
- Maintenance scheduling
- Insurance policy management
- Technical specifications
- Registration and licensing
- Inspection history

## API Endpoints

### Vehicles
- `GET /api/masterdata/vehicles` - List all vehicles
- `GET /api/masterdata/vehicles/{id}` - Get specific vehicle
- `POST /api/masterdata/vehicles` - Register new vehicle
- `PUT /api/masterdata/vehicles/{id}` - Update vehicle information
- `PATCH /api/masterdata/vehicles/{id}` - Partial vehicle update
- `DELETE /api/masterdata/vehicles/{id}` - Remove vehicle

### Vehicle Registration
- `GET /api/masterdata/vehicles/{id}/registrations` - Get registration records
- `POST /api/masterdata/vehicles/{id}/registrations` - Add registration
- `PUT /api/masterdata/vehicles/{id}/registrations/{regId}` - Update registration
- `DELETE /api/masterdata/vehicles/{id}/registrations/{regId}` - Remove registration

### Vehicle Insurance
- `GET /api/masterdata/vehicles/{id}/insurance` - Get insurance policies
- `POST /api/masterdata/vehicles/{id}/insurance` - Add insurance policy
- `PUT /api/masterdata/vehicles/{id}/insurance/{insuranceId}` - Update insurance
- `DELETE /api/masterdata/vehicles/{id}/insurance/{insuranceId}` - Remove insurance

### Vehicle Maintenance
- `GET /api/masterdata/vehicles/{id}/maintenance` - Get maintenance records
- `POST /api/masterdata/vehicles/{id}/maintenance` - Schedule maintenance
- `PUT /api/masterdata/vehicles/{id}/maintenance/{maintenanceId}` - Update maintenance
- `DELETE /api/masterdata/vehicles/{id}/maintenance/{maintenanceId}` - Cancel maintenance

### Vehicle Inspections
- `GET /api/masterdata/vehicles/{id}/inspections` - Get inspection records
- `POST /api/masterdata/vehicles/{id}/inspections` - Record inspection
- `PUT /api/masterdata/vehicles/{id}/inspections/{inspectionId}` - Update inspection
- `DELETE /api/masterdata/vehicles/{id}/inspections/{inspectionId}` - Remove inspection

### Vehicle Documents
- `GET /api/masterdata/vehicles/{id}/documents` - Get vehicle documents
- `POST /api/masterdata/vehicles/{id}/documents` - Upload document
- `PUT /api/masterdata/vehicles/{id}/documents/{docId}` - Update document
- `DELETE /api/masterdata/vehicles/{id}/documents/{docId}` - Remove document

### Vehicle Specifications
- `GET /api/masterdata/vehicles/{id}/specifications` - Get vehicle specs
- `POST /api/masterdata/vehicles/{id}/specifications` - Add specification
- `PUT /api/masterdata/vehicles/{id}/specifications/{specId}` - Update specification
- `DELETE /api/masterdata/vehicles/{id}/specifications/{specId}` - Remove specification

## Vehicle Types

The system supports various vehicle types:
- **Trucks**: Heavy duty vehicles for cargo transport
- **Trailers**: Attachable cargo units
- **Tankers**: Liquid transport vehicles
- **Flatbeds**: Open platform vehicles
- **Containers**: Standardized cargo units

## Data Models

### Vehicle
Main vehicle entity containing identification and basic information.

### VehicleRegistration
Official registration and licensing information.

### VehicleInsurance
Insurance policy and coverage details.

### VehicleMaintenance
Maintenance schedules, records, and service history.

### VehicleInspection
Safety and compliance inspection records.

### VehicleDocument
Associated documents (certificates, permits, etc.).

### VehicleSpecification
Technical specifications and capabilities.