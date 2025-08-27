# Organization Module

## Overview

The Organization module manages the multi-tenant organizational structure within the QaliTrack ecosystem. This module provides comprehensive management for organizations, their hierarchies, users, subscriptions, and settings.

## Key Features

### Core Entities
- **Organization**: Main organization management
- **OrganizationUser**: User management within organizations
- **OrganizationLocation**: Multi-location management
- **OrganizationSettings**: Organization-specific configurations
- **OrganizationSubscription**: Subscription and billing management

### Functionality
- **Multi-Tenancy**: Complete organization isolation and management
- **User Management**: User roles and permissions within organizations
- **Location Management**: Multiple location support per organization
- **Subscription Management**: Plan management and billing
- **Settings Management**: Organization-specific configurations
- **Hierarchy Management**: Parent-child organization relationships

## API Endpoints

### Organization Management
- `GET /api/organizations` - List organizations
- `GET /api/organizations/{id}` - Get organization details
- `POST /api/organizations` - Create new organization
- `PUT /api/organizations/{id}` - Update organization
- `PATCH /api/organizations/{id}` - Partial update organization
- `DELETE /api/organizations/{id}` - Delete organization

### User Management
- `GET /api/organizations/{id}/users` - List organization users
- `POST /api/organizations/{id}/users` - Add user to organization
- `PUT /api/organizations/{orgId}/users/{id}` - Update user role
- `DELETE /api/organizations/{orgId}/users/{id}` - Remove user

### Location Management
- `GET /api/organizations/{id}/locations` - List locations
- `POST /api/organizations/{id}/locations` - Add location
- `PUT /api/organizations/{orgId}/locations/{id}` - Update location
- `DELETE /api/organizations/{orgId}/locations/{id}` - Remove location

### Settings Management
- `GET /api/organizations/{id}/settings` - Get organization settings
- `PUT /api/organizations/{id}/settings` - Update organization settings

## Data Models

### Organization
- **Id**: Unique identifier
- **Name**: Organization name
- **DisplayName**: Display name for UI
- **Description**: Organization description
- **Industry**: Industry sector
- **CompanySize**: Organization size category
- **RegistrationNumber**: Official registration number
- **TaxNumber**: Tax identification number
- **Website**: Organization website
- **EstablishedDate**: Date established
- **Status**: Organization status
- **ParentOrganizationId**: Parent organization (for hierarchies)
- **TimeZone**: Default timezone
- **Currency**: Default currency
- **Language**: Default language
- **CreatedDate**: Creation timestamp
- **LastModified**: Last modification timestamp

### OrganizationUser
- **Id**: Unique identifier
- **OrganizationId**: Associated organization
- **UserId**: User identifier
- **Email**: User email address
- **FirstName**: User first name
- **LastName**: User last name
- **Role**: User role within organization
- **Status**: User status
- **InvitedDate**: Date user was invited
- **JoinedDate**: Date user joined
- **LastLoginDate**: Last login timestamp
- **Permissions**: Specific permissions granted

### OrganizationLocation
- **Id**: Unique identifier
- **OrganizationId**: Associated organization
- **Name**: Location name
- **LocationType**: Type of location
- **Address**: Physical address
- **City**: City
- **State**: State/province
- **Country**: Country
- **PostalCode**: Postal/ZIP code
- **Coordinates**: GPS coordinates
- **ContactInfo**: Location contact information
- **IsHeadquarters**: Whether this is the main location
- **IsActive**: Whether location is active

### OrganizationSettings
- **Id**: Unique identifier
- **OrganizationId**: Associated organization
- **SettingKey**: Setting identifier
- **SettingValue**: Setting value
- **SettingType**: Data type of setting
- **Category**: Setting category
- **IsEditable**: Whether setting can be modified
- **Description**: Setting description
- **LastModified**: Last modification timestamp

### OrganizationSubscription
- **Id**: Unique identifier
- **OrganizationId**: Associated organization
- **SubscriptionPlan**: Subscription plan type
- **Status**: Subscription status
- **StartDate**: Subscription start date
- **EndDate**: Subscription end date
- **BillingCycle**: Billing frequency
- **MaxUsers**: Maximum users allowed
- **MaxLocations**: Maximum locations allowed
- **Features**: Enabled features
- **PricePerMonth**: Monthly price
- **LastBillingDate**: Last billing date
- **NextBillingDate**: Next billing date

## Business Rules

### Organization Management
- Organization names must be unique within the system
- Parent-child relationships cannot create circular references
- Organization deletion requires all child entities to be handled
- Status changes affect access to all organization resources

### User Management
- Users can belong to multiple organizations with different roles
- At least one admin user must exist per organization
- User permissions are inherited from roles but can be customized
- Inactive users retain data but lose system access

### Subscription Management
- Organizations must have active subscriptions to access paid features
- Feature usage must not exceed subscription limits
- Billing must be processed according to subscription terms
- Plan changes take effect at next billing cycle

## Integration Points

### Related Modules
- **Business Entities**: Organization-owned business entities
- **Vehicles**: Organization fleet management
- **Drivers**: Organization employee management
- **Products**: Organization product catalogs
- **Relationships**: Cross-organization relationships

### External Systems
- Identity management systems
- Billing and payment systems
- Email and notification systems
- Analytics and reporting systems

## Usage Examples

### Creating a New Organization
```json
{
  "name": "Acme Transport Solutions",
  "displayName": "Acme Transport",
  "description": "Leading transport and logistics company",
  "industry": "Transportation",
  "companySize": "Medium",
  "registrationNumber": "REG123456789",
  "taxNumber": "TAX987654321",
  "website": "https://acmetransport.com",
  "establishedDate": "2015-03-20",
  "timeZone": "UTC+3",
  "currency": "USD",
  "language": "en"
}
```

### Adding a User to Organization
```json
{
  "organizationId": "org-001",
  "email": "manager@acmetransport.com",
  "firstName": "Jane",
  "lastName": "Smith",
  "role": "Manager",
  "permissions": [
    "manage_vehicles",
    "manage_drivers",
    "view_reports"
  ]
}
```

### Adding Organization Location
```json
{
  "organizationId": "org-001",
  "name": "Main Depot",
  "locationType": "Depot",
  "address": "123 Industrial Road",
  "city": "Nairobi",
  "country": "Kenya",
  "postalCode": "00100",
  "coordinates": {
    "latitude": -1.2921,
    "longitude": 36.8219
  },
  "isHeadquarters": true,
  "contactInfo": {
    "phone": "+254700000000",
    "email": "depot@acmetransport.com"
  }
}
```

### Configuring Organization Settings
```json
{
  "organizationId": "org-001",
  "settings": [
    {
      "settingKey": "default_currency",
      "settingValue": "KES",
      "category": "Localization"
    },
    {
      "settingKey": "working_hours_start",
      "settingValue": "08:00",
      "category": "Operations"
    },
    {
      "settingKey": "notification_email",
      "settingValue": "admin@acmetransport.com",
      "category": "Notifications"
    }
  ]
}
```

## API Reference

For detailed API documentation including request/response schemas, see the [auto-generated API reference](xref:QaliTrack.MasterData.Core.Modules.Organization.DTOs).