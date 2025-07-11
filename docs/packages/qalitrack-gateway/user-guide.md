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
- **Hybrid Authorization** - Role-based + permission-based security
- **Service Discovery** - Automatic routing to available services  
- **Health Monitoring** - Real-time service status information

## User Roles and Permissions

The gateway uses a **hybrid authorization model** that combines role-based access control with fine-grained permissions. Your access to services depends on both your role level and specific permissions granted to that role.

### Role Hierarchy

Understanding your role determines what services and data you can access:

#### 🔴 Guest (Level 0)
- **Access**: Public information only
- **Permissions**: `read:public`
- **Use Cases**: Product catalogs, public documentation
- **Limitations**: Cannot access operational data

#### 🟡 User (Level 1) 
- **Access**: Basic authenticated services
- **Permissions**: `read:public`, `read:products`, `read:profile`
- **Services**: Products, personal profile
- **Use Cases**: Viewing product information, updating personal details
- **Business Context**: General system users, customers

#### 🟠 Operator (Level 2)
- **Access**: Daily operational tasks
- **Permissions**: User permissions + `write:products`, `read:customers`, `write:customers`, `read:vehicles`, `write:vehicles`, `read:drivers`, `write:drivers`, `read:suppliers`, `write:suppliers`, `read:weight-data`, `write:weight-data`, `read:transactions`, `write:transactions`
- **Services**: Products, customers, vehicles, drivers, suppliers, weight data, transactions
- **Use Cases**: Creating transactions, managing master data
- **Business Context**: Weighbridge operators, data entry staff

#### 🔵 Auditor (Level 2)
- **Access**: Read-only compliance and analytics
- **Permissions**: `read:public`, `read:compliance`, `read:analytics`, `read:reports`, `read:transactions`, `read:archive`
- **Services**: Compliance monitoring, analytics reports
- **Use Cases**: Compliance verification, audit reporting
- **Business Context**: Compliance officers, auditors

#### 🟢 Site Manager (Level 3)
- **Access**: Site-level management
- **Permissions**: Operator permissions + `read:analytics`, `read:compliance`, `manage:site`
- **Services**: All operational services + analytics + site management
- **Use Cases**: Site performance monitoring, operational oversight
- **Business Context**: Site supervisors, regional managers

#### 🟣 Admin (Level 4)
- **Access**: Organizational administration
- **Permissions**: Operator permissions + all delete permissions + `read:users`, `write:users`, `manage:users`, `read:organizations`, `write:organizations`, `manage:organizations`, `read:compliance`, `manage:compliance`, `read:analytics`, `read:archive`, `write:archive`
- **Services**: All services including user management
- **Use Cases**: User administration, system configuration
- **Business Context**: IT administrators, business managers

#### ⚫ SuperAdmin (Level 5)
- **Access**: System-wide control
- **Permissions**: All Admin permissions + `delete:users`, `delete:organizations`, `delete:transactions`, `delete:archive`, `manage:system`, `admin:system`
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

## Permission Management Workflows

### Understanding Your Permissions

#### Checking Your Current Permissions

When you authenticate, your JWT token contains your role and permissions:

```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "role": "Operator",
  "roles": ["Operator"],
  "user": {
    "role": "Operator",
    "roles": ["Operator"],
    "permissions": [
      "read:public", "read:products", "read:profile",
      "write:products", "read:customers", "write:customers",
      "read:vehicles", "write:vehicles", "read:drivers",
      "write:drivers", "read:suppliers", "write:suppliers",
      "read:weight-data", "write:weight-data",
      "read:transactions", "write:transactions"
    ]
  }
}
```

#### Verifying Access to Services

Use the gateway info endpoint to check which services you can access:

```bash
curl -H "Authorization: Bearer YOUR_TOKEN" \
  https://localhost:7000/api/gateway/info
```

### Access Control Scenarios

#### Scenario 1: User Trying to Access Customer Data

**User Role**: User (Level 1)
**Attempting**: `GET /api/customers`
**Required**: Role `Operator` + Permission `read:customers`
**Result**: ❌ **403 Forbidden** - Insufficient role and missing permission

**Error Response**:
```json
{
  "error": {
    "code": "INSUFFICIENT_PRIVILEGES",
    "message": "User does not meet both role and permission requirements",
    "details": {
      "roleCheck": {
        "userRole": "User",
        "requiredRole": "Operator", 
        "passed": false
      },
      "permissionCheck": {
        "userPermissions": ["read:products", "read:profile"],
        "requiredPermissions": ["read:customers"],
        "passed": false
      }
    }
  }
}
```

#### Scenario 2: Operator Accessing Product Management

**User Role**: Operator (Level 2)
**Attempting**: `POST /api/products`
**Required**: Role `Operator` + Permission `write:products`
**Result**: ✅ **200 OK** - Both role and permission requirements met

#### Scenario 3: Auditor Accessing Analytics

**User Role**: Auditor (Level 2)
**Attempting**: `GET /api/analytics/reports`
**Required**: Role `SiteManager` + Permission `read:analytics`
**Result**: ❌ **403 Forbidden** - Has permission but insufficient role level

### Permission Best Practices

#### For Users
1. **Know Your Role**: Understand what permissions your role includes
2. **Request Appropriately**: Don't attempt to access services outside your permissions
3. **Report Issues**: If you need access to something, contact your administrator
4. **Security Awareness**: Never share your authentication tokens

#### For Administrators
1. **Principle of Least Privilege**: Assign minimum required permissions
2. **Regular Audits**: Review user roles and permissions quarterly
3. **Permission Documentation**: Keep role-permission mappings up to date
4. **Access Requests**: Establish clear process for permission escalation

#### For Developers
1. **Handle Authorization Errors**: Implement proper error handling for 403 responses
2. **Permission Checking**: Validate permissions before making requests
3. **Token Inspection**: Parse JWT tokens to understand user permissions
4. **Graceful Degradation**: Hide unavailable features based on user permissions

### Permission Escalation Process

If you need additional permissions:

1. **Identify Required Permission**: Determine exactly what permission you need
2. **Business Justification**: Prepare justification for the access request
3. **Contact Administrator**: Submit request through proper channels
4. **Temporary vs Permanent**: Specify if access is temporary or permanent
5. **Verification**: Test new permissions once granted

### Troubleshooting Permission Issues

#### Common Permission Errors

| Error Code | Meaning | Solution |
|------------|---------|----------|
| `INSUFFICIENT_ROLE` | Role level too low | Contact admin for role upgrade |
| `MISSING_PERMISSIONS` | Specific permission missing | Request specific permission |
| `INSUFFICIENT_PRIVILEGES` | Both role and permission issues | Need both role and permission updates |

#### Permission Debugging

1. **Check JWT Token**: Verify your current permissions in the token
2. **Test with Mock Services**: Use mock endpoints to verify permission logic
3. **Review Documentation**: Confirm required permissions for endpoints
4. **Contact Support**: Escalate complex permission issues

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