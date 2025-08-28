# Weighbridge Module

## Overview

The Weighbridge module manages weighing station infrastructure and operations within the QaliTrack ecosystem. This module provides comprehensive management for weighbridge equipment, calibration, maintenance, and transaction processing.

## Key Features

### Core Entities
- **Weighbridge**: Main weighbridge equipment management
- **WeighbridgeCalibration**: Calibration records and schedules
- **WeighbridgeMaintenance**: Maintenance tracking and history
- **WeighbridgeDocument**: Document management
- **WeighbridgeTransaction**: Transaction processing and records

### Functionality
- **Equipment Management**: Complete weighbridge lifecycle management
- **Calibration Tracking**: Regular calibration scheduling and compliance
- **Maintenance Planning**: Preventive and corrective maintenance
- **Transaction Processing**: Weight measurement and data recording
- **Document Management**: Certificates, manuals, and compliance documents
- **Status Monitoring**: Real-time equipment status tracking

## API Endpoints

### Weighbridge Management
- `GET /api/weighbridges` - List all weighbridges
- `GET /api/weighbridges/{id}` - Get weighbridge details
- `POST /api/weighbridges` - Create new weighbridge
- `PUT /api/weighbridges/{id}` - Update weighbridge
- `PATCH /api/weighbridges/{id}` - Partial update weighbridge
- `DELETE /api/weighbridges/{id}` - Delete weighbridge

### Calibration Management
- `GET /api/weighbridges/{id}/calibrations` - List calibrations
- `POST /api/weighbridges/{id}/calibrations` - Create calibration record
- `PUT /api/weighbridges/{weighbridgeId}/calibrations/{id}` - Update calibration
- `DELETE /api/weighbridges/{weighbridgeId}/calibrations/{id}` - Delete calibration

### Maintenance Management
- `GET /api/weighbridges/{id}/maintenance` - List maintenance records
- `POST /api/weighbridges/{id}/maintenance` - Create maintenance record
- `PUT /api/weighbridges/{weighbridgeId}/maintenance/{id}` - Update maintenance
- `DELETE /api/weighbridges/{weighbridgeId}/maintenance/{id}` - Delete maintenance

## Data Models

### Weighbridge
- **Id**: Unique identifier
- **Name**: Weighbridge name
- **Location**: Physical location
- **Capacity**: Maximum weight capacity
- **Accuracy**: Measurement accuracy specifications
- **SerialNumber**: Equipment serial number
- **Manufacturer**: Equipment manufacturer
- **Model**: Equipment model
- **InstallationDate**: Installation date
- **Status**: Current operational status
- **LastCalibration**: Last calibration date
- **NextCalibration**: Next calibration due date
- **OrganizationId**: Organization ownership

### WeighbridgeCalibration
- **Id**: Unique identifier
- **WeighbridgeId**: Associated weighbridge
- **CalibrationDate**: Date of calibration
- **CalibratedBy**: Technician/company performing calibration
- **CertificateNumber**: Calibration certificate number
- **StandardWeights**: Standard weights used
- **Results**: Calibration test results
- **Status**: Calibration status
- **NextDueDate**: Next calibration due date
- **Notes**: Additional calibration notes

### WeighbridgeMaintenance
- **Id**: Unique identifier
- **WeighbridgeId**: Associated weighbridge
- **MaintenanceDate**: Date of maintenance
- **MaintenanceType**: Type of maintenance (Preventive, Corrective)
- **PerformedBy**: Technician/company performing maintenance
- **Description**: Maintenance description
- **PartsReplaced**: Parts replaced during maintenance
- **Cost**: Maintenance cost
- **Status**: Maintenance status
- **NextScheduled**: Next scheduled maintenance

## Business Rules

### Calibration Requirements
- Weighbridges must be calibrated at regular intervals
- Calibration certificates must be valid and up-to-date
- Failed calibrations require immediate attention
- Calibration history must be maintained for compliance

### Maintenance Scheduling
- Preventive maintenance should be scheduled based on usage
- Critical maintenance issues must be addressed immediately
- Maintenance history affects weighbridge reliability scoring
- Spare parts inventory should be monitored

### Transaction Processing
- All transactions must be recorded with timestamps
- Weight measurements must be within calibrated accuracy
- Transaction integrity must be maintained
- Audit trails are required for all transactions

## Integration Points

### Related Modules
- **Routes**: Weighbridges are associated with transportation routes
- **Organizations**: Weighbridge ownership and management
- **Relationships**: Route-weighbridge associations

### External Systems
- Weight measurement systems integration
- Calibration management systems
- Maintenance scheduling systems
- Compliance reporting systems

## Usage Examples

### Creating a New Weighbridge
```json
{
  "name": "Main Gate Weighbridge",
  "location": "Gate 1, Industrial Area",
  "capacity": 60000,
  "accuracy": 20,
  "serialNumber": "WB-001-2023",
  "manufacturer": "ScaleTech",
  "model": "ST-6000",
  "installationDate": "2023-01-15",
  "organizationId": "org-123"
}
```

### Scheduling Calibration
```json
{
  "weighbridgeId": "wb-001",
  "scheduledDate": "2024-03-15",
  "calibrationType": "Annual",
  "technician": "Certified Calibration Services",
  "notes": "Annual compliance calibration"
}
```

## API Reference

For detailed API documentation including request/response schemas, see the [auto-generated API reference](xref:QaliTrack.MasterData.Core.Modules.Weighbridge.DTOs).