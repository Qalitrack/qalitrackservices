# Business Entities Module

The Business Entities module manages customers, suppliers, and transporters in the QaliTrack ecosystem.

## Overview

This module provides a unified API for managing different types of business entities:
- **Customers**: Organizations that purchase products/services
- **Suppliers**: Organizations that provide products/services
- **Transporters**: Organizations that handle logistics and transportation

## Key Features

- Unified business entity management
- Contact information tracking
- Document management
- Location management
- Contract and performance tracking
- Profile customization for each entity type

## API Endpoints

### Business Entities
- `GET /api/masterdata/business-entities` - List all business entities
- `GET /api/masterdata/business-entities/{id}` - Get specific entity
- `POST /api/masterdata/business-entities` - Create new entity
- `PUT /api/masterdata/business-entities/{id}` - Update entity
- `DELETE /api/masterdata/business-entities/{id}` - Delete entity

### Contacts
- `GET /api/masterdata/business-entities/{id}/contacts` - Get entity contacts
- `POST /api/masterdata/business-entities/{id}/contacts` - Add contact
- `PUT /api/masterdata/business-entities/{id}/contacts/{contactId}` - Update contact
- `DELETE /api/masterdata/business-entities/{id}/contacts/{contactId}` - Delete contact

### Customer Profiles
- `GET /api/masterdata/business-entities/{id}/customer-profile` - Get customer profile
- `POST /api/masterdata/business-entities/{id}/customer-profile` - Create customer profile
- `PUT /api/masterdata/business-entities/{id}/customer-profile` - Update customer profile

### Supplier Profiles
- `GET /api/masterdata/business-entities/{id}/supplier-profile` - Get supplier profile
- `POST /api/masterdata/business-entities/{id}/supplier-profile` - Create supplier profile
- `PUT /api/masterdata/business-entities/{id}/supplier-profile` - Update supplier profile

### Transporter Profiles  
- `GET /api/masterdata/business-entities/{id}/transporter-profile` - Get transporter profile
- `POST /api/masterdata/business-entities/{id}/transporter-profile` - Create transporter profile
- `PUT /api/masterdata/business-entities/{id}/transporter-profile` - Update transporter profile

## Entity Types

### Customer
Represents organizations that purchase products or services from the system.

**Key Properties:**
- Credit limit management
- Payment terms
- Order history
- Contract management

### Supplier
Represents organizations that provide products or services to customers.

**Key Properties:**
- Product catalogs
- Pricing information
- Performance metrics
- Quality ratings

### Transporter
Represents organizations that handle logistics and transportation services.

**Key Properties:**
- Fleet information
- Insurance details
- Route coverage
- Performance tracking

## Data Models

### BusinessEntity
Base entity containing common properties for all business entity types.

### CustomerProfile
Extended information specific to customer entities.

### SupplierProfile
Extended information specific to supplier entities.

### TransporterProfile
Extended information specific to transporter entities.

### BusinessEntityContact
Contact information associated with business entities.