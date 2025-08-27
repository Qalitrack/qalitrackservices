# Error Handling

## Overview

The QaliTrack Master Data Service implements comprehensive error handling to provide consistent, informative error responses across all API endpoints. This document outlines the error handling patterns, status codes, and response formats used throughout the service.

## Standard Error Response Format

All API endpoints return errors in a consistent format:

```json
{
  "success": false,
  "message": "Human-readable error message",
  "errorCode": "SPECIFIC_ERROR_CODE",
  "timestamp": "2024-01-15T10:30:00Z",
  "path": "/api/vehicles/123",
  "details": {
    "field": "Additional error details",
    "validationErrors": []
  }
}
```

## HTTP Status Codes

### 2xx Success
- **200 OK**: Request successful
- **201 Created**: Resource created successfully
- **204 No Content**: Successful request with no response body (typically for DELETE operations)

### 4xx Client Errors
- **400 Bad Request**: Invalid request format or parameters
- **401 Unauthorized**: Authentication required or failed
- **403 Forbidden**: Insufficient permissions
- **404 Not Found**: Resource not found
- **409 Conflict**: Resource conflict (e.g., duplicate key)
- **422 Unprocessable Entity**: Validation errors
- **429 Too Many Requests**: Rate limit exceeded

### 5xx Server Errors
- **500 Internal Server Error**: Unexpected server error
- **503 Service Unavailable**: Service temporarily unavailable
- **504 Gateway Timeout**: Request timeout

## Error Categories

### Validation Errors (422)

Validation errors occur when request data fails business rules or data constraints:

```json
{
  "success": false,
  "message": "Validation failed",
  "errorCode": "VALIDATION_ERROR",
  "timestamp": "2024-01-15T10:30:00Z",
  "path": "/api/drivers",
  "details": {
    "validationErrors": [
      {
        "field": "licenseNumber",
        "message": "License number is required",
        "code": "REQUIRED_FIELD"
      },
      {
        "field": "email",
        "message": "Invalid email format",
        "code": "INVALID_FORMAT"
      }
    ]
  }
}
```

### Resource Not Found (404)

When requested resources don't exist:

```json
{
  "success": false,
  "message": "Vehicle with ID '123' not found",
  "errorCode": "RESOURCE_NOT_FOUND",
  "timestamp": "2024-01-15T10:30:00Z",
  "path": "/api/vehicles/123",
  "details": {
    "resourceType": "Vehicle",
    "resourceId": "123"
  }
}
```

### Authorization Errors (401, 403)

Authentication and authorization failures:

```json
{
  "success": false,
  "message": "Access denied. Insufficient permissions",
  "errorCode": "INSUFFICIENT_PERMISSIONS",
  "timestamp": "2024-01-15T10:30:00Z",
  "path": "/api/organizations/456",
  "details": {
    "requiredPermission": "manage_organizations",
    "currentRole": "viewer"
  }
}
```

### Conflict Errors (409)

Resource conflicts, typically during create or update operations:

```json
{
  "success": false,
  "message": "Vehicle with license plate 'ABC123' already exists",
  "errorCode": "DUPLICATE_RESOURCE",
  "timestamp": "2024-01-15T10:30:00Z",
  "path": "/api/vehicles",
  "details": {
    "conflictField": "licensePlate",
    "conflictValue": "ABC123",
    "existingResourceId": "vehicle-789"
  }
}
```

## Common Error Codes

### General Errors
- **INVALID_REQUEST**: Malformed request
- **RESOURCE_NOT_FOUND**: Requested resource doesn't exist
- **UNAUTHORIZED**: Authentication required
- **INSUFFICIENT_PERMISSIONS**: Insufficient access rights
- **RATE_LIMIT_EXCEEDED**: Too many requests

### Validation Errors
- **REQUIRED_FIELD**: Required field missing
- **INVALID_FORMAT**: Invalid data format
- **OUT_OF_RANGE**: Value outside acceptable range
- **INVALID_RELATIONSHIP**: Invalid foreign key relationship
- **DUPLICATE_VALUE**: Unique constraint violation

### Business Logic Errors
- **BUSINESS_RULE_VIOLATION**: Business rule constraint failed
- **INVALID_STATE_TRANSITION**: Invalid status change
- **DEPENDENCY_EXISTS**: Cannot delete due to dependencies
- **QUOTA_EXCEEDED**: Resource quota exceeded

## Module-Specific Error Handling

### Vehicle Module
- **INVALID_LICENSE_PLATE**: License plate format validation
- **VEHICLE_ALREADY_ASSIGNED**: Vehicle assignment conflicts
- **EXPIRED_REGISTRATION**: Vehicle registration expired

### Driver Module
- **INVALID_LICENSE**: Driver license validation
- **LICENSE_EXPIRED**: Driver license expired
- **MEDICAL_CLEARANCE_REQUIRED**: Medical clearance missing

### Business Entity Module
- **INVALID_BUSINESS_REGISTRATION**: Business registration validation
- **DUPLICATE_BUSINESS_NAME**: Business name already exists
- **INVALID_TAX_NUMBER**: Tax number format validation

### SACCO Module
- **INSUFFICIENT_SHARES**: Insufficient share capital
- **LOAN_LIMIT_EXCEEDED**: Loan amount exceeds limit
- **INVALID_GUARANTOR**: Guarantor validation failed

## Error Handling Best Practices

### For Clients

1. **Check HTTP Status Codes**: Always check the HTTP status code first
2. **Parse Error Response**: Extract error details from the response body
3. **Handle Specific Error Codes**: Implement specific handling for known error codes
4. **Display User-Friendly Messages**: Convert technical errors to user-friendly messages
5. **Implement Retry Logic**: For transient errors (5xx status codes)

### Example Client Error Handling

```javascript
try {
  const response = await fetch('/api/vehicles', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(vehicleData)
  });

  if (!response.ok) {
    const error = await response.json();
    
    switch (error.errorCode) {
      case 'VALIDATION_ERROR':
        handleValidationErrors(error.details.validationErrors);
        break;
      case 'DUPLICATE_RESOURCE':
        showDuplicateResourceError(error.message);
        break;
      case 'INSUFFICIENT_PERMISSIONS':
        redirectToLogin();
        break;
      default:
        showGenericError(error.message);
    }
    return;
  }

  const result = await response.json();
  handleSuccess(result);
} catch (err) {
  showNetworkError('Unable to connect to server');
}
```

## Logging and Monitoring

### Server-Side Logging
- All errors are logged with appropriate severity levels
- Error logs include correlation IDs for tracing
- Sensitive information is never logged
- Performance metrics are collected for error rates

### Error Tracking
- Errors are categorized and tracked for patterns
- Critical errors trigger immediate alerts
- Error trends are monitored for system health
- User impact is assessed for prioritization

## Development Guidelines

### Adding New Error Types

When adding new error handling:

1. **Define Error Code**: Create descriptive error code constants
2. **Standard Response**: Use the standard error response format
3. **Appropriate Status Code**: Select the most appropriate HTTP status code
4. **Helpful Details**: Include relevant details for troubleshooting
5. **Documentation**: Document new error codes and scenarios

### Testing Error Scenarios

Ensure comprehensive testing of:
- Valid and invalid request formats
- Authorization scenarios
- Resource not found cases
- Validation rule violations
- Business logic constraints
- Server error conditions

## Troubleshooting Common Issues

### High Error Rates
1. Check for validation rule changes
2. Verify client integration changes
3. Monitor resource availability
4. Review authentication configuration

### Performance-Related Errors
1. Monitor database connection pools
2. Check query performance
3. Review caching effectiveness
4. Analyze resource utilization

### Integration Issues
1. Verify external service availability
2. Check network connectivity
3. Validate configuration settings
4. Review dependency versions

This comprehensive error handling approach ensures that developers can quickly identify and resolve issues while providing users with clear, actionable error messages.