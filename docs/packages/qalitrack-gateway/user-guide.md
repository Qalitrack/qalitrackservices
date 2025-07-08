# QaliTrack API Gateway - User Guide

This guide provides comprehensive instructions for business users, system administrators, and integrators working with the QaliTrack API Gateway.

## 📋 Table of Contents

- [Overview](#overview)
- [User Roles and Permissions](#user-roles-and-permissions)
- [Getting Started](#getting-started)
- [Authentication Guide](#authentication-guide)
- [Service Access Patterns](#service-access-patterns)
- [Environment Management](#environment-management)
- [Monitoring and Health Checks](#monitoring-and-health-checks)
- [Troubleshooting Common Issues](#troubleshooting-common-issues)

## Overview

The QaliTrack API Gateway is your central access point to all weighbridge management services. It provides:

- **Unified Authentication** - Single login for all services
- **Role-Based Security** - Appropriate access levels for different user types
- **Service Discovery** - Automatic routing to available services
- **Health Monitoring** - Real-time service status information

## User Roles and Permissions

### Role Hierarchy

Understanding your role determines what services and data you can access:

#### 🔴 Guest (Level 0)
- **Access**: Public information only
- **Use Cases**: Product catalogs, public documentation
- **Limitations**: Cannot access operational data

#### 🟡 User (Level 1) 
- **Access**: Basic authenticated services
- **Services**: Products, personal profile
- **Use Cases**: Viewing product information, updating personal details
- **Business Context**: General system users, customers

#### 🟠 Operator (Level 2)
- **Access**: Daily operational tasks
- **Services**: Products, customers, vehicles, drivers, suppliers
- **Use Cases**: Creating transactions, managing master data
- **Business Context**: Weighbridge operators, data entry staff

#### 🔵 Auditor (Level 2)
- **Access**: Read-only compliance and analytics
- **Services**: Compliance monitoring, analytics reports
- **Use Cases**: Compliance verification, audit reporting
- **Business Context**: Compliance officers, auditors

#### 🟢 Site Manager (Level 3)
- **Access**: Site-level management
- **Services**: All operational services + analytics
- **Use Cases**: Site performance monitoring, operational oversight
- **Business Context**: Site supervisors, regional managers

#### 🟣 Admin (Level 4)
- **Access**: Organizational administration
- **Services**: All services including user management
- **Use Cases**: User administration, system configuration
- **Business Context**: IT administrators, business managers

#### ⚫ SuperAdmin (Level 5)
- **Access**: System-wide control
- **Services**: Complete system access
- **Use Cases**: System maintenance, multi-organization management
- **Business Context**: System administrators, technical support

### Permission Matrix

| Service | Guest | User | Operator | Auditor | SiteManager | Admin | SuperAdmin |
|---------|-------|------|----------|---------|-------------|-------|------------|
| **Authentication** | Login | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ |
| **Products** | View | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ |
| **Customers** | ❌ | ❌ | ✓ | ❌ | ✓ | ✓ | ✓ |
| **Vehicles** | ❌ | ❌ | ✓ | ❌ | ✓ | ✓ | ✓ |
| **Drivers** | ❌ | ❌ | ✓ | ❌ | ✓ | ✓ | ✓ |
| **Weight Data** | ❌ | ❌ | ✓ | View | ✓ | ✓ | ✓ |
| **Compliance** | ❌ | ❌ | ❌ | ✓ | ✓ | ✓ | ✓ |
| **Analytics** | ❌ | ❌ | ❌ | ✓ | ✓ | ✓ | ✓ |
| **User Management** | ❌ | ❌ | ❌ | ❌ | ❌ | ✓ | ✓ |
| **Organizations** | ❌ | ❌ | ❌ | ❌ | ❌ | ✓ | ✓ |

## Getting Started

### 1. Access the Gateway

The gateway is available at:
- **Production**: `https://your-domain.com/`
- **Testing**: `https://localhost:7000/`

### 2. Initial Authentication

#### Web Interface Login
```
1. Navigate to: https://localhost:7000/swagger
2. Click "Authorize" button
3. Enter your credentials
4. Receive JWT token for API access
```

#### API Login
```bash
curl -X POST https://localhost:7000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "username": "your-username",
    "password": "your-password"
  }'
```

### 3. Understanding Your Access

After login, check your permissions:
```bash
# Get your user information
curl -X GET https://localhost:7000/api/users/profile \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"

# Check available services
curl -X GET https://localhost:7000/api/gateway/services \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

## Authentication Guide

### JWT Token Management

Your JWT token contains:
- **User Identity**: ID, username, email
- **Roles**: Your assigned roles and permissions
- **Expiration**: Token validity period (typically 1 hour)

### Token Usage

Always include your token in the Authorization header:
```http
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### Token Refresh

Tokens expire for security. When you receive a 401 Unauthorized:
1. Re-authenticate using the login endpoint
2. Update your application with the new token
3. Retry your original request

### Security Best Practices

- **Never share tokens** with other users
- **Store tokens securely** in your application
- **Use HTTPS** for all communications
- **Log out** when finished to invalidate tokens

## Service Access Patterns

### Master Data Services

#### Product Management
```bash
# View products (User+ role required)
GET /api/products

# Create product (Operator+ role required) 
POST /api/products
{
  "name": "Cement Bags",
  "category": "Building Materials",
  "unitPrice": 850.00
}

# Update product (Operator+ role required)
PUT /api/products/{id}

# Delete product (Admin+ role required)
DELETE /api/products/{id}
```

#### Customer Management
```bash
# View customers (Operator+ role required)
GET /api/customers

# Create customer (Operator+ role required)
POST /api/customers
{
  "name": "ABC Construction Ltd",
  "email": "contact@abcconstruction.com",
  "phone": "+254123456789"
}
```

### Operational Services

#### Weight Data Entry
```bash
# Record weight measurement (Operator+ role required)
POST /api/weight-data
{
  "vehicleId": "vehicle-uuid",
  "grossWeight": 15000,
  "tareWeight": 5000,
  "productId": "product-uuid",
  "customerId": "customer-uuid"
}
```

#### Transaction Processing
```bash
# Create transaction (Operator+ role required)
POST /api/transactions
{
  "customerId": "customer-uuid",
  "productId": "product-uuid",
  "quantity": 100,
  "unitPrice": 850.00,
  "transactionType": "Sale"
}
```

### Analytics and Reporting

#### Compliance Monitoring
```bash
# Get compliance report (Auditor+ role required)
GET /api/compliance/reports?startDate=2025-01-01&endDate=2025-01-31

# Get compliance status (Auditor+ role required)
GET /api/compliance/status
```

#### Business Analytics
```bash
# Get sales analytics (SiteManager+ role required)
GET /api/analytics/sales?period=monthly

# Get operational metrics (SiteManager+ role required)
GET /api/analytics/operations?siteId=site-uuid
```

## Environment Management

### Mock vs Real Services

The gateway supports two operational modes:

#### Mock Services (Testing/Development)
- **Purpose**: Rapid development and testing
- **Authentication**: Instant role switching
- **Data**: Stateless, no persistent storage
- **Use Cases**: Feature development, automated testing

```bash
# Switch to mock mode
export USE_MOCK_SERVICES=true

# Test with different roles instantly
curl -X POST http://localhost:7001/api/MockAuth/mock-login \
  -d '{"username": "testuser", "role": "Admin"}'
```

#### Real Services (Production)
- **Purpose**: Production operations
- **Authentication**: Database-backed user accounts
- **Data**: Persistent storage with full CRUD operations
- **Use Cases**: Live operations, production deployment

```bash
# Switch to real mode
export USE_MOCK_SERVICES=false

# Standard authentication flow
curl -X POST https://localhost:7000/api/auth/login \
  -d '{"username": "admin", "password": "secure-password"}'
```

### Client-Specific Configurations

Different organizations can have customized service configurations:

#### Testing Environment
```yaml
# Minimal services for development
services:
  - gateway
  - user-service
  - product-service
```

#### Cement Factory Configuration
```yaml
# Full operational services
services:
  - gateway
  - user-service
  - customer-service
  - vehicle-service
  - weight-data-service
  - compliance-service
  - analytics-service
```

#### Regulatory Authority Configuration
```yaml
# Compliance-focused services
services:
  - gateway
  - user-service
  - compliance-service
  - analytics-service (read-only)
```

## Monitoring and Health Checks

### Service Health Monitoring

Check system health regularly:

#### Gateway Health
```bash
# Quick health check
curl https://localhost:7000/health

# Detailed service status
curl https://localhost:7000/api/gateway/services
```

#### Individual Service Health
```bash
# User service health
curl https://localhost:7001/health

# Product service health  
curl https://localhost:7005/health
```

### Service Discovery

The gateway automatically discovers available services:

```bash
# Get service information
curl https://localhost:7000/api/gateway/info

# Response includes:
{
  "services": [
    {
      "name": "User Service",
      "path": "/api/users",
      "port": 7001,
      "status": "Available"
    }
  ]
}
```

### Performance Monitoring

Monitor gateway performance:
- **Response Times**: < 50ms for authenticated requests
- **Error Rates**: Monitor 4xx/5xx responses
- **Service Availability**: Health check success rates
- **Authentication Metrics**: Login success/failure rates

## Troubleshooting Common Issues

### Authentication Problems

#### 401 Unauthorized
**Problem**: Token expired or invalid
**Solution**: 
1. Check token expiration
2. Re-authenticate to get new token
3. Verify token format

#### 403 Forbidden
**Problem**: Insufficient role privileges
**Solution**:
1. Check your role assignment
2. Contact administrator for role upgrade
3. Verify endpoint access requirements

### Service Access Issues

#### 404 Not Found
**Problem**: Service not available or wrong endpoint
**Solution**:
1. Check service health: `GET /health`
2. Verify endpoint URL in API documentation
3. Confirm service is enabled for your client

#### 502 Bad Gateway
**Problem**: Downstream service unavailable
**Solution**:
1. Check service health status
2. Wait for service to recover
3. Contact system administrator

### Performance Issues

#### Slow Response Times
**Problem**: High latency or service overload
**Solution**:
1. Check service health metrics
2. Reduce request frequency
3. Use appropriate caching strategies

#### Connection Timeouts
**Problem**: Network or service issues
**Solution**:
1. Verify network connectivity
2. Check service status
3. Implement retry logic with backoff

### Configuration Issues

#### Missing Services
**Problem**: Expected services not available
**Solution**:
1. Check client configuration
2. Verify service deployment
3. Review environment settings

#### Role Permissions
**Problem**: Unexpected access restrictions
**Solution**:
1. Review role hierarchy documentation
2. Check user role assignment
3. Contact administrator for clarification

## Best Practices

### For Business Users
- **Regular Password Updates**: Change passwords every 90 days
- **Role Awareness**: Understand your access level and limitations
- **Data Accuracy**: Ensure data entry accuracy for compliance
- **Security Compliance**: Follow organizational security policies

### For System Administrators
- **Monitor Service Health**: Regular health check monitoring
- **User Management**: Prompt role assignment and deactivation
- **Configuration Management**: Version control for gateway configs
- **Security Auditing**: Regular review of access logs and permissions

### For Developers
- **Token Management**: Implement proper token refresh logic
- **Error Handling**: Handle all HTTP status codes appropriately
- **Testing**: Use mock services for development and testing
- **Documentation**: Keep integration documentation current

---

*This user guide provides comprehensive guidance for effective use of the QaliTrack API Gateway across different user roles and operational scenarios.*