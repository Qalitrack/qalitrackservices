# SACCO Module

## Overview

The SACCO (Savings and Credit Cooperative Organizations) module manages financial cooperative organizations within the QaliTrack ecosystem. This module provides comprehensive management for SACCO operations, member management, financial services, and governance structures.

## Key Features

### Core Entities
- **Sacco**: Main SACCO organization management
- **SaccoMember**: Member registration and management
- **SaccoCommittee**: Governance and committee structures
- **SaccoFinancial**: Financial records and reporting
- **SaccoLoan**: Loan processing and management
- **SaccoShare**: Share capital management
- **SaccoService**: Service offerings and management
- **SaccoMeeting**: Meeting management and records

### Functionality
- **Organization Management**: Complete SACCO lifecycle management
- **Member Services**: Member registration, profiles, and services
- **Financial Management**: Savings, loans, and share capital
- **Governance**: Committee management and meeting coordination
- **Service Delivery**: Various financial and non-financial services
- **Compliance**: Regulatory compliance and reporting

## API Endpoints

### SACCO Management
- `GET /api/saccos` - List all SACCOs
- `GET /api/saccos/{id}` - Get SACCO details
- `POST /api/saccos` - Create new SACCO
- `PUT /api/saccos/{id}` - Update SACCO
- `PATCH /api/saccos/{id}` - Partial update SACCO
- `DELETE /api/saccos/{id}` - Delete SACCO

### Member Management
- `GET /api/saccos/{id}/members` - List SACCO members
- `POST /api/saccos/{id}/members` - Add new member
- `PUT /api/saccos/{saccoId}/members/{id}` - Update member
- `DELETE /api/saccos/{saccoId}/members/{id}` - Remove member

### Financial Services
- `GET /api/saccos/{id}/loans` - List loans
- `POST /api/saccos/{id}/loans` - Create loan application
- `GET /api/saccos/{id}/shares` - List share transactions
- `POST /api/saccos/{id}/shares` - Record share transaction

### Committee Management
- `GET /api/saccos/{id}/committees` - List committees
- `POST /api/saccos/{id}/committees` - Create committee
- `GET /api/saccos/{id}/meetings` - List meetings
- `POST /api/saccos/{id}/meetings` - Schedule meeting

## Data Models

### Sacco
- **Id**: Unique identifier
- **Name**: SACCO name
- **RegistrationNumber**: Official registration number
- **LicenseNumber**: Operating license number
- **EstablishedDate**: Date established
- **RegisteredAddress**: Official registered address
- **ContactInfo**: Contact information
- **Status**: SACCO operational status
- **SaccoType**: Type of SACCO (Agricultural, Transport, etc.)
- **MemberCount**: Current member count
- **ShareCapital**: Total share capital
- **TotalAssets**: Total assets value
- **OrganizationId**: Parent organization

### SaccoMember
- **Id**: Unique identifier
- **SaccoId**: Associated SACCO
- **MemberNumber**: Unique member number
- **PersonalInfo**: Personal information
- **ContactInfo**: Contact details
- **JoinDate**: Date joined SACCO
- **MembershipType**: Type of membership
- **MembershipStatus**: Current membership status
- **SharesOwned**: Number of shares owned
- **ContributionAmount**: Regular contribution amount
- **GuarantorInfo**: Guarantor information

### SaccoLoan
- **Id**: Unique identifier
- **SaccoId**: Associated SACCO
- **MemberId**: Borrowing member
- **LoanType**: Type of loan
- **LoanAmount**: Principal loan amount
- **InterestRate**: Applied interest rate
- **LoanTerm**: Loan duration in months
- **ApplicationDate**: Date applied
- **ApprovalDate**: Date approved
- **DisbursementDate**: Date disbursed
- **Status**: Current loan status
- **GuarantorsInfo**: Loan guarantors
- **CollateralInfo**: Collateral details

### SaccoCommittee
- **Id**: Unique identifier
- **SaccoId**: Associated SACCO
- **CommitteeName**: Committee name
- **CommitteeType**: Type of committee
- **EstablishedDate**: Date established
- **Status**: Committee status
- **Responsibilities**: Committee responsibilities
- **MeetingFrequency**: How often committee meets
- **Members**: Committee members list

## Business Rules

### Membership Management
- Members must meet eligibility criteria before joining
- Member numbers must be unique within each SACCO
- Share purchases must comply with minimum requirements
- Member status changes must be properly documented

### Financial Services
- Loan amounts cannot exceed predetermined limits
- Interest rates must comply with regulatory requirements
- Loan guarantors must be verified members
- Share transfers require proper authorization

### Governance
- Committees must have minimum required members
- Meeting quorums must be met for decision validity
- Financial decisions require appropriate approvals
- Audit trails must be maintained for all transactions

## Integration Points

### Related Modules
- **Drivers**: Driver-SACCO membership relationships
- **Vehicles**: Vehicle-SACCO registration relationships
- **Organizations**: SACCO organizational hierarchy
- **Relationships**: Cross-module membership tracking

### External Systems
- Banking and payment systems
- Regulatory reporting systems
- Credit bureau integration
- Mobile money platforms

## Usage Examples

### Creating a New SACCO
```json
{
  "name": "Transport Workers SACCO",
  "registrationNumber": "SACCO/REG/2023/001",
  "licenseNumber": "LIC/SACCO/2023/001",
  "establishedDate": "2023-01-15",
  "registeredAddress": "123 Main Street, City Center",
  "saccoType": "Transport",
  "contactInfo": {
    "phone": "+254700000000",
    "email": "info@transportworkerssacco.co.ke"
  },
  "organizationId": "org-123"
}
```

### Registering a New Member
```json
{
  "saccoId": "sacco-001",
  "personalInfo": {
    "firstName": "John",
    "lastName": "Doe",
    "nationalId": "12345678",
    "dateOfBirth": "1985-05-15"
  },
  "contactInfo": {
    "phone": "+254700000001",
    "email": "john.doe@email.com",
    "address": "456 Elm Street"
  },
  "membershipType": "Regular",
  "initialShares": 10,
  "contributionAmount": 5000
}
```

### Creating a Loan Application
```json
{
  "saccoId": "sacco-001",
  "memberId": "member-001",
  "loanType": "Business",
  "loanAmount": 100000,
  "loanTerm": 24,
  "purpose": "Vehicle purchase for transport business",
  "guarantors": [
    {
      "memberId": "member-002",
      "guaranteeAmount": 50000
    }
  ]
}
```

## API Reference

For detailed API documentation including request/response schemas, see the [auto-generated API reference](xref:QaliTrack.MasterData.Core.Modules.Sacco.DTOs).