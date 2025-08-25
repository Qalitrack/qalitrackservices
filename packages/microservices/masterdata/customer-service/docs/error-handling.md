# Error Handling

The Customer Service implements comprehensive error handling to ensure robust operation and clear error reporting.

## Error Response Format

All API endpoints return errors in a consistent format:

```json
{
  "success": false,
  "data": null,
  "message": "High-level error description",
  "errors": [
    "Specific error detail 1",
    "Specific error detail 2"
  ],
  "timestamp": "2024-01-01T12:00:00Z",
  "traceId": "unique-trace-identifier"
}
```

## HTTP Status Codes

| Status Code | Description | When Used |
|-------------|-------------|-----------|
| 200 OK | Request successful | Successful GET, PUT operations |
| 201 Created | Resource created | Successful POST operations |
| 204 No Content | Request successful, no content | Successful DELETE operations |
| 400 Bad Request | Invalid request data | Validation failures, malformed JSON |
| 401 Unauthorized | Authentication required | Missing or invalid JWT token |
| 403 Forbidden | Access denied | Insufficient permissions |
| 404 Not Found | Resource not found | Customer/Contact/Contract/Order not found |
| 409 Conflict | Resource conflict | Duplicate email, contract number, etc. |
| 422 Unprocessable Entity | Validation errors | Business rule violations |
| 500 Internal Server Error | Server error | Unexpected server errors |
| 503 Service Unavailable | Service temporarily unavailable | Database connection issues |

## Error Categories

### Validation Errors (400 Bad Request)

Returned when request data fails validation:

```json
{
  "success": false,
  "message": "Validation failed",
  "errors": [
    "Name is required",
    "Email must be a valid email address",
    "Phone number format is invalid"
  ]
}
```

**Common validation scenarios:**
- Missing required fields
- Invalid email format
- Phone number format validation
- Date range validations
- String length violations

### Authentication Errors (401 Unauthorized)

```json
{
  "success": false,
  "message": "Authentication required",
  "errors": [
    "JWT token is missing or invalid"
  ]
}
```

### Authorization Errors (403 Forbidden)

```json
{
  "success": false,
  "message": "Access denied",
  "errors": [
    "Insufficient permissions to access this resource"
  ]
}
```

### Not Found Errors (404 Not Found)

```json
{
  "success": false,
  "message": "Resource not found",
  "errors": [
    "Customer with ID 'abc-123' not found"
  ]
}
```

### Conflict Errors (409 Conflict)

```json
{
  "success": false,
  "message": "Resource conflict",
  "errors": [
    "A customer with this email already exists"
  ]
}
```

### Business Logic Errors (422 Unprocessable Entity)

```json
{
  "success": false,
  "message": "Business rule violation",
  "errors": [
    "Cannot delete customer with active contracts",
    "Contract end date must be after start date"
  ]
}
```

### Server Errors (500 Internal Server Error)

```json
{
  "success": false,
  "message": "An internal server error occurred",
  "errors": [
    "Please contact support with trace ID: xyz-789"
  ],
  "traceId": "xyz-789"
}
```

## Field-Level Validation Rules

### Customer Validation
- **Name**: Required, 2-100 characters
- **Email**: Required, valid email format, unique
- **Phone**: Optional, valid phone format
- **Address**: Optional, max 500 characters

### Contact Validation
- **Customer ID**: Required, must exist
- **Name**: Required, 2-100 characters
- **Email**: Optional, valid email format
- **Phone**: Optional, valid phone format
- **Position**: Optional, max 100 characters
- **IsPrimary**: Boolean, only one primary contact per customer

### Contract Validation
- **Customer ID**: Required, must exist
- **Contract Number**: Required, unique, 5-50 characters
- **Start Date**: Required, cannot be in the past
- **End Date**: Required, must be after start date
- **Terms**: Optional, max 2000 characters
- **Value**: Optional, must be positive
- **Status**: Required, valid enum value

### Order Validation
- **Customer ID**: Required, must exist
- **Contract ID**: Optional, must exist if provided
- **Order Number**: Required, unique, 5-50 characters
- **Description**: Optional, max 1000 characters
- **Quantity**: Required, must be positive
- **Unit Price**: Required, must be positive
- **Status**: Required, valid enum value

## Error Logging

All errors are logged with different levels:

### Error Levels
- **Information**: Successful operations, normal flow
- **Warning**: Validation failures, business rule violations
- **Error**: Unexpected errors, database issues
- **Critical**: System failures, security breaches

### Log Format
```
[Timestamp] [Level] [TraceId] [Source] Message
2024-01-01T12:00:00Z [ERROR] abc-123 CustomerService.Api.Controllers.CustomersController Database connection failed: timeout expired
```

## Global Exception Handling

The service uses global exception middleware to handle unhandled exceptions:

```csharp
public class GlobalExceptionMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }
}
```

## Retry Policies

For transient failures, the service implements retry policies:

### Database Operations
- **Retry Count**: 3
- **Delay**: Exponential backoff (1s, 2s, 4s)
- **Conditions**: Connection timeout, deadlock

### External Service Calls
- **Retry Count**: 2
- **Delay**: Linear backoff (1s, 2s)
- **Conditions**: Network timeout, service unavailable

## Circuit Breaker Pattern

For external dependencies, circuit breaker pattern is implemented:

- **Failure Threshold**: 5 consecutive failures
- **Timeout**: 30 seconds
- **Recovery**: Gradual recovery with health checks

## Troubleshooting Common Errors

### Database Connection Issues
```
Error: Unable to connect to database
Solution: 
1. Check database connection string
2. Verify database server is running
3. Check network connectivity
4. Validate credentials
```

### Validation Failures
```
Error: Name is required
Solution: Ensure all required fields are provided in the request body
```

### Duplicate Resource Errors
```
Error: A customer with this email already exists
Solution: Use a different email address or update the existing customer
```

### Authentication Issues
```
Error: JWT token is missing or invalid
Solution: 
1. Include valid JWT token in Authorization header
2. Ensure token hasn't expired
3. Verify token signing key
```

## Monitoring and Alerting

### Error Rate Monitoring
- Monitor error rates per endpoint
- Alert when error rate exceeds 5%
- Dashboard showing error trends

### Critical Error Alerts
- Immediate alerts for 500 errors
- Database connection failures
- Security-related errors

### Error Metrics
- Total error count
- Error rate by endpoint
- Response time percentiles
- Error categories breakdown

## Best Practices

1. **Always return consistent error format**
2. **Include correlation/trace IDs for tracking**
3. **Log errors with appropriate detail level**
4. **Don't expose internal implementation details**
5. **Provide actionable error messages**
6. **Implement proper retry mechanisms**
7. **Monitor error rates and patterns**
8. **Have escalation procedures for critical errors**