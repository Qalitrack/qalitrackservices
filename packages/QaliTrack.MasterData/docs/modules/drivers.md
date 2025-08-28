# Drivers Module

The Drivers module manages driver profiles, licensing, training, and performance tracking within the QaliTrack system.

## Overview

This module provides comprehensive driver management capabilities including:
- Driver profile management
- License tracking and renewals
- Training and certification records
- Medical certification tracking
- Performance monitoring
- Document management

## Key Features

- **Complete Driver Profiles**: Personal and professional information
- **License Management**: Track multiple license types and renewals
- **Training Records**: Certification and training completion tracking
- **Medical Records**: Health certification and medical exam tracking
- **Performance Analytics**: Safety scores and performance metrics
- **Document Storage**: Driver-related document management
- **Compliance Tracking**: Regulatory compliance monitoring

## API Endpoints

### Drivers
- `GET /api/masterdata/drivers` - List all drivers
- `GET /api/masterdata/drivers/{id}` - Get specific driver
- `POST /api/masterdata/drivers` - Create new driver
- `PUT /api/masterdata/drivers/{id}` - Update driver information
- `PATCH /api/masterdata/drivers/{id}` - Partial driver update
- `DELETE /api/masterdata/drivers/{id}` - Remove driver

### Driver Licenses
- `GET /api/masterdata/drivers/{id}/licenses` - Get driver licenses
- `POST /api/masterdata/drivers/{id}/licenses` - Add license
- `PUT /api/masterdata/drivers/{id}/licenses/{licenseId}` - Update license
- `DELETE /api/masterdata/drivers/{id}/licenses/{licenseId}` - Remove license

### Driver Training
- `GET /api/masterdata/drivers/{id}/training` - Get training records
- `POST /api/masterdata/drivers/{id}/training` - Add training record
- `PUT /api/masterdata/drivers/{id}/training/{trainingId}` - Update training
- `DELETE /api/masterdata/drivers/{id}/training/{trainingId}` - Remove training

### Driver Medical Records
- `GET /api/masterdata/drivers/{id}/medical` - Get medical records
- `POST /api/masterdata/drivers/{id}/medical` - Add medical record
- `PUT /api/masterdata/drivers/{id}/medical/{medicalId}` - Update medical
- `DELETE /api/masterdata/drivers/{id}/medical/{medicalId}` - Remove medical

### Driver Documents
- `GET /api/masterdata/drivers/{id}/documents` - Get driver documents
- `POST /api/masterdata/drivers/{id}/documents` - Upload document
- `PUT /api/masterdata/drivers/{id}/documents/{docId}` - Update document
- `DELETE /api/masterdata/drivers/{id}/documents/{docId}` - Remove document

## Data Models

### Driver
Main driver entity containing personal and professional information.

**Key Properties:**
- Personal details (name, contact, address)
- Employment information
- Emergency contacts
- Status and availability

### DriverLicense
License information and tracking.

**Key Properties:**
- License type and class
- Issue and expiration dates
- License number
- Issuing authority
- Status and restrictions

### DriverTraining
Training and certification records.

**Key Properties:**
- Training type and provider
- Completion date and certificate
- Expiration date
- Training status
- Renewal requirements

### DriverMedical
Medical certification and health records.

**Key Properties:**
- Medical exam date and results
- Medical certificate expiration
- Medical examiner information
- Medical restrictions
- Fitness status

## Driver Status Types

- **Active**: Available for assignments
- **Inactive**: Not available for assignments
- **Suspended**: License or medical issues
- **Terminated**: No longer employed
- **On Leave**: Temporary unavailability

## License Types

- **Commercial Driver's License (CDL)**
  - Class A: Heavy trucks and tractor-trailers
  - Class B: Large trucks and buses
  - Class C: Small hazmat vehicles

- **Endorsements**
  - Hazmat (H)
  - Passenger (P)
  - School Bus (S)
  - Tank Vehicles (N)

## Training Types

- **Initial Certification**: Basic driver training
- **Defensive Driving**: Safety and accident prevention
- **Hazmat Training**: Hazardous materials handling
- **Customer Service**: Professional interaction skills
- **Vehicle Inspection**: Pre-trip and safety inspections
- **Emergency Response**: Accident and emergency procedures

## Business Rules

### License Requirements
- All drivers must have valid licenses
- License expiration alerts 30 days in advance
- Suspended licenses prevent driver assignments

### Medical Requirements
- Medical certificates required for commercial drivers
- Medical exams every 2 years (or as required)
- Medical alerts 60 days before expiration

### Training Requirements
- Mandatory safety training annually
- Specialized training for hazmat drivers
- Refresher training for incidents or violations

## Integration Points

### Vehicle Assignments
- Driver-vehicle compatibility checks
- Vehicle access permissions
- Assignment history tracking

### Performance Tracking
- Safety incident recording
- Performance metrics collection
- Compliance scoring

### Payroll Integration
- Driver employment verification
- Training completion tracking
- Medical certification status