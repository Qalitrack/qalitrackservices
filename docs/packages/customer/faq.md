# Customer Service FAQ

## General Questions

### What is the Customer Service?

The Customer Service is a microservice within the QaliTrack ecosystem that provides comprehensive customer relationship management (CRM) capabilities. It handles customer registration, profile management, contact information, contracts, billing details, and credit management for weighbridge operations and compliance tracking.

### What technology stack does the Customer Service use?

The Customer Service is built using:
- **.NET 8** with ASP.NET Core for the API framework
- **Entity Framework Core** for data access
- **SQLite** for development and **SQL Server/PostgreSQL** for production
- **AutoMapper** for object mapping
- **FluentValidation** for input validation
- **Swagger/OpenAPI** for API documentation

### How does the Customer Service integrate with other QaliTrack services?

The Customer Service integrates through:
- **REST API endpoints** for synchronous communication
- **Message bus events** for asynchronous notifications
- **Shared authentication** using organization and user context headers
- **Common data models** and consistent API patterns across services

## Authentication and Security

### How do I authenticate requests to the Customer Service?

All requests require two mandatory headers:
```http
X-Organization-Id: {your-organization-uuid}
X-User-Id: {your-user-uuid}
```

These headers provide:
- **Organization context** for multi-tenant data isolation
- **User context** for audit trails and permissions
- **Access control** based on user roles and organization membership

### What happens if I don't provide authentication headers?

Requests without required headers will receive:
- **HTTP 401 Unauthorized** response
- Error message indicating missing authentication
- No access to any customer data or operations

### How is customer data secured?

Customer data security includes:
- **Encryption in transit** using HTTPS/TLS
- **Organization-based data isolation** preventing cross-tenant access
- **Input validation** to prevent injection attacks
- **Audit logging** for all data access and modifications
- **GDPR compliance** for data protection and privacy

## Customer Management

### How do I register a new customer?

Use the POST `/api/customers` endpoint with required information:

```json
{
  "name": "Customer Company Name",
  "contactEmail": "contact@customer.com",
  "billingAddress": "123 Business Street, City, State 12345",
  "customerType": "Corporate",
  "creditLimit": 25000.00
}
```

**Required fields:**
- `name`: Customer name (max 500 characters)
- `contactEmail`: Unique email address
- `billingAddress`: Complete billing address
- `customerType`: Individual, Corporate, Government, or NonProfit
- `creditLimit`: Non-negative decimal value

### Can I have duplicate customer names?

Yes, customer names can be duplicated, but certain fields must be unique:
- **Contact email**: Must be unique across all customers
- **Tax number**: Must be unique when provided
- **Registration number**: Must be unique when provided

### How do I search for customers?

Use the GET `/api/customers` endpoint with search parameters:

```http
GET /api/customers?search=Acme&page=1&pageSize=20
```

**Search capabilities:**
- **Name matching**: Partial name searches
- **Email lookup**: Exact email matches
- **Tax number**: Exact tax number matches
- **Pagination**: Control result size with page and pageSize

### What customer types are supported?

Four customer types are available:
- **Individual**: Personal accounts for individual customers
- **Corporate**: Business entities and corporations
- **Government**: Government agencies and departments
- **NonProfit**: Non-profit organizations and charities

### How do I update customer information?

Use the PUT `/api/customers/{id}` endpoint with updated data:

```json
{
  "name": "Updated Customer Name",
  "contactEmail": "new-email@customer.com",
  "billingAddress": "New Address",
  "customerType": "Corporate",
  "creditLimit": 30000.00,
  "status": "Active"
}
```

**Note**: Email uniqueness is still enforced during updates.

### Can I delete customers?

Yes, use DELETE `/api/customers/{id}`. Customer deletion:
- **Soft delete**: Data is preserved for audit purposes
- **Cascade effects**: Related contacts, contracts, and billing data are also deleted
- **Irreversible**: Deleted customers cannot be restored through the API
- **Data retention**: May be subject to GDPR and business retention policies

## Contact Management

### How many contacts can a customer have?

There is no hard limit on the number of contacts per customer. However:
- **Best practice**: 5-10 contacts per customer for performance
- **Contact types**: Each type (Business, Technical, Billing, Emergency) can have one primary contact
- **Active status**: Inactive contacts don't count toward operational limits

### What contact types are available?

Four contact types are supported:
- **Business**: General business operations and coordination
- **Technical**: Technical support and system integration
- **Billing**: Financial operations and invoice processing
- **Emergency**: After-hours and urgent situation contacts

### Can I have multiple primary contacts?

You can have **one primary contact per contact type**:
- One primary Business contact
- One primary Technical contact
- One primary Billing contact
- One primary Emergency contact

### How do I add a contact to a customer?

Use POST `/api/customers/{customerId}/contacts`:

```json
{
  "firstName": "John",
  "lastName": "Smith",
  "email": "j.smith@customer.com",
  "phone": "+1-555-0123",
  "contactType": "Business",
  "isPrimary": true
}
```

## Contract Management

### What contract types are supported?

Three contract types are available:
- **Service**: Ongoing weighbridge services and support
- **Maintenance**: Equipment maintenance and repair contracts
- **Lease**: Equipment and facility lease agreements

### How does auto-renewal work?

Auto-renewal contracts:
- **Automatic extension**: Contracts extend automatically at expiration
- **Renewal period**: Specified in months (typically 12 months)
- **Notifications**: System sends alerts 90, 60, and 30 days before expiration
- **Terms preservation**: Original terms and conditions carry forward

### Can I modify active contracts?

Yes, active contracts can be updated:
- **Contract details**: Title, description, value can be modified
- **Terms changes**: Update payment terms and conditions
- **Status updates**: Change from Draft to Active, or terminate early
- **Signature information**: Add or update signatory details

### How do I track contract expiration?

The system provides:
- **Expiration alerts**: Automatic notifications before contract end dates
- **Status tracking**: Monitor Active, Expired, and Terminated contracts
- **Dashboard views**: Summary of contracts requiring attention
- **Renewal workflows**: Streamlined contract renewal processes

## Billing and Credit Management

### How is billing information managed?

Each customer can have one billing profile containing:
- **Billing contact**: Name, email, and phone for invoice delivery
- **Billing address**: Complete mailing address for invoices
- **Payment terms**: Net 15, Net 30, or custom payment periods
- **Discount settings**: Customer-specific discount percentages
- **Tax information**: Tax exemption numbers and handling

### How does credit management work?

Credit management includes:
- **Credit limits**: Maximum outstanding amount allowed
- **Usage tracking**: Real-time calculation of available vs. used credit
- **Payment history**: Track payment performance and reliability
- **Credit ratings**: Excellent, Good, Fair, Poor based on payment history
- **Security deposits**: Optional deposits for high-risk customers

### Can credit limits be changed?

Yes, credit limits can be updated:
- **Increase requests**: Based on payment history and business growth
- **Decrease/suspension**: For payment issues or financial concerns
- **Automatic recalculation**: Available credit updates with limit changes
- **Approval workflows**: May require management approval for large increases

### What happens when credit limits are exceeded?

When credit limits are exceeded:
- **Transaction blocking**: New transactions may be declined
- **Alerts**: Automatic notifications to billing contacts
- **Exception handling**: Manual override capabilities for urgent situations
- **Resolution options**: Payment or credit limit increase required

## API Integration

### What response format does the API use?

All API responses follow a consistent format:

```json
{
  "success": true,
  "message": "Operation completed successfully",
  "data": { /* Response data */ },
  "errors": null,
  "timestamp": "2024-07-04T10:30:00.000Z"
}
```

### How do I handle API errors?

API errors include:
- **HTTP status codes**: 400 (Bad Request), 404 (Not Found), 409 (Conflict), etc.
- **Error messages**: Clear description of the problem
- **Error details**: Specific validation errors when applicable
- **Timestamp**: When the error occurred

**Example error response:**
```json
{
  "success": false,
  "message": "Validation failed",
  "data": null,
  "errors": [
    "Name is required",
    "Email format is invalid"
  ],
  "timestamp": "2024-07-04T10:30:00.000Z"
}
```

### How do I implement pagination?

Use query parameters for pagination:

```http
GET /api/customers?page=2&pageSize=50
```

**Parameters:**
- `page`: Page number (starts at 1)
- `pageSize`: Items per page (default 20, max 100)
- Additional filters can be combined with pagination

### What are the API rate limits?

Current rate limits:
- **1000 requests per hour** per organization
- **Rate limit headers** included in responses
- **429 status code** when limits are exceeded
- **Retry-After header** indicates when to retry

### How do I test API endpoints?

Several testing options:
- **Swagger UI**: Available at `/swagger` for interactive testing
- **Postman**: Import OpenAPI specification for complete API collection
- **curl**: Command-line testing with proper headers
- **Integration tests**: Use provided test examples and patterns

## Performance and Reliability

### What are the service performance targets?

Performance targets include:
- **Customer lookup**: <100ms (95th percentile)
- **Customer search**: <200ms (95th percentile)
- **Customer registration**: <500ms (95th percentile)
- **Service availability**: >99.9% uptime

### How do I monitor service health?

Health monitoring options:
- **Health endpoint**: GET `/health` for service status
- **Application metrics**: Performance and usage statistics
- **Database connectivity**: Automatic database health checks
- **Integration status**: Dependency health verification

### What caching is implemented?

Caching strategies include:
- **Response caching**: HTTP cache headers for static data
- **In-memory caching**: Frequently accessed customer data
- **Distributed caching**: Redis for multi-instance deployments
- **Cache invalidation**: Automatic updates when data changes

### How does the service handle high load?

High load handling:
- **Horizontal scaling**: Multiple service instances
- **Connection pooling**: Efficient database connection management
- **Async operations**: Non-blocking request processing
- **Load balancing**: Traffic distribution across instances

## Troubleshooting

### Why am I getting "Customer not found" errors?

Common causes:
- **Invalid customer ID**: Check the UUID format and existence
- **Organization context**: Ensure you're using the correct organization ID
- **Deleted customers**: Soft-deleted customers won't appear in normal queries
- **Case sensitivity**: Customer IDs are case-sensitive

### Why are my customer searches returning no results?

Check these factors:
- **Search term format**: Ensure proper encoding of special characters
- **Organization context**: Search is scoped to your organization
- **Pagination**: Results may be on different pages
- **Customer status**: Inactive customers may be filtered out

### How do I resolve validation errors?

Validation error resolution:
- **Read error messages**: Specific field requirements are provided
- **Check data types**: Ensure numeric fields contain valid numbers
- **Verify constraints**: Email uniqueness, length limits, etc.
- **Review documentation**: API reference has complete validation rules

### What should I do if the service is unavailable?

Service unavailability steps:
1. **Check health endpoint**: Verify service status at `/health`
2. **Verify network connectivity**: Ensure network access to service
3. **Check authentication**: Confirm headers are correct
4. **Review logs**: Application logs may indicate specific issues
5. **Contact support**: Use provided support channels for assistance

### How do I report bugs or request features?

For bug reports and feature requests:
- **Technical issues**: support@qalitrack.com
- **Bug reports**: Include steps to reproduce, expected vs. actual behavior
- **Feature requests**: Describe business use case and requirements
- **Documentation issues**: Specify unclear or missing information

## Data Management

### How do I backup customer data?

Data backup considerations:
- **Database backups**: Handled at infrastructure level
- **Export functionality**: Use API to export customer data
- **Retention policies**: Data retained per business requirements
- **Recovery procedures**: Contact support for data recovery needs

### Can I import existing customer data?

Data import options:
- **Bulk API operations**: Use multiple POST requests for customer creation
- **Data migration tools**: Contact support for large data migrations
- **Validation requirements**: All imported data must meet validation rules
- **Duplicate handling**: Email uniqueness must be maintained

### How do I comply with GDPR requirements?

GDPR compliance features:
- **Data access**: Customers can request their data through support
- **Data portability**: Export functionality available
- **Data deletion**: Right to be forgotten through deletion APIs
- **Audit trails**: All data access and modifications are logged
- **Privacy controls**: Data access restricted by organization boundaries

This FAQ covers the most common questions about the Customer Service. For additional questions or detailed technical support, please contact the QaliTrack support team.