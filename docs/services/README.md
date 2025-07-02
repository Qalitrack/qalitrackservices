# Microservices Documentation

This directory contains technical documentation for all QaliTrack microservices.

## Services

### User Service
- **Location**: `/services/userservice/`
- **Documentation**: [User Service Docs](./userservice.md)
- **Purpose**: User authentication, authorization, and profile management

### Master Data Service
- **Location**: `/services/masterdataservice/`
- **Documentation**: [Master Data Service Docs](./masterdataservice.md)
- **Purpose**: Central management of reference data and configurations

## Documentation Template

Each service should have documentation covering:

1. **Overview**
   - Service purpose and responsibilities
   - Key features and capabilities

2. **Architecture**
   - Service architecture diagram
   - Database schema
   - External dependencies

3. **API Reference**
   - Endpoint documentation
   - Request/response schemas
   - Authentication requirements

4. **Configuration**
   - Environment variables
   - Configuration files
   - Default settings

5. **Deployment**
   - Docker configuration
   - Environment setup
   - Health checks

6. **Development**
   - Local development setup
   - Testing procedures
   - Debugging guides

## Inter-Service Communication

Document how services communicate:
- REST APIs
- Message queues
- Event streaming
- Database sharing patterns