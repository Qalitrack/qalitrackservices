# Customer Management

The Customer Service provides comprehensive customer relationship management capabilities.

## Core Features

### Customer CRUD Operations
- **Create Customer**: Register new customers with validation
- **Read Customer**: Retrieve customer information and relationships
- **Update Customer**: Modify customer details with audit tracking  
- **Delete Customer**: Remove customer records with safety checks

### Customer Status Management
- **Activate Customer**: Enable customer for business operations
- **Deactivate Customer**: Temporarily disable customer access
- **Status Tracking**: Monitor customer lifecycle states

### Unique Constraint Validation
- **Email Uniqueness**: Prevent duplicate customer email addresses
- **Tax Number Validation**: Ensure unique tax identification numbers
- **Name Availability**: Check customer name availability

## API Endpoints

### Basic Operations
```
GET    /api/customers           # List all customers
GET    /api/customers/{id}      # Get specific customer
POST   /api/customers           # Create new customer  
PUT    /api/customers/{id}      # Update customer
DELETE /api/customers/{id}      # Remove customer
```

### Status Management
```
POST /api/customers/{id}/activate     # Activate customer
POST /api/customers/{id}/deactivate   # Deactivate customer
```

### Validation
```
GET /api/customers/check-name/{name}  # Check name availability
```

## Data Transfer Objects

### CreateCustomerDto
- Name (required)
- ContactEmail (optional, must be unique)
- TaxNumber (optional, must be unique)  
- Address information
- Contact details

### UpdateCustomerDto
- Partial update support
- Validation for unique constraints
- Audit timestamp updates

### CustomerReadDto
- Complete customer information
- Related entity counts
- Status information
- Audit timestamps

## Business Rules

1. **Email Uniqueness**: Customer email addresses must be unique across the system
2. **Tax Number Validation**: Tax numbers must be unique and follow format requirements
3. **Status Transitions**: Customers can be activated/deactivated with proper authorization
4. **Soft Delete**: Customer deletion maintains referential integrity
5. **Audit Trail**: All customer changes are tracked with timestamps

## Integration Points

- **Contact Service**: Manages customer contact persons
- **Contract Service**: Handles customer contracts and agreements
- **Order Service**: Processes customer orders and transactions
- **Gateway**: Routes customer endpoints via `/api/customers`