# Order Management

The Order Management module handles customer orders, status tracking, and order lifecycle management within the Customer Service.

## Overview

Orders represent customer requests for products or services. The system tracks order status, manages order history, and provides comprehensive order lifecycle management.

## Features

### Order Operations
- **Create Order**: Generate new customer orders
- **Update Order**: Modify order details and status
- **Track Orders**: Monitor order progress and status
- **Order History**: Maintain complete order audit trail

### Status Management
- **Status Updates**: Track order through various stages
- **Status History**: Maintain complete status change log
- **Automated Transitions**: Handle status changes based on business rules
- **Manual Overrides**: Allow authorized status modifications

## API Endpoints

### Order CRUD
```
GET    /api/customers/{id}/orders            # Get customer orders
POST   /api/customers/{id}/orders            # Create new order
GET    /api/orders/{orderId}                 # Get specific order
PUT    /api/orders/{orderId}                 # Update order
DELETE /api/orders/{orderId}                # Cancel order
```

### Status Management
```
POST /api/orders/{orderId}/status           # Update order status
GET  /api/orders/{orderId}/status-history   # Get status history
GET  /api/orders/by-status/{status}         # Get orders by status
```

### Order Tracking
```
GET /api/customers/{id}/orders/active       # Get active orders
GET /api/customers/{id}/orders/completed    # Get completed orders
GET /api/orders/pending                     # Get all pending orders
```

## Data Models

### Order Entity
- **OrderId**: Unique identifier
- **CustomerId**: Associated customer
- **OrderNumber**: Human-readable order number
- **Status**: Current order status
- **OrderDate**: When order was placed
- **RequestedDate**: Customer requested completion date
- **CompletedDate**: Actual completion date
- **TotalAmount**: Order total value
- **Notes**: Order notes and comments
- **Priority**: Order priority level

### OrderStatusHistory Entity
- **HistoryId**: Unique identifier
- **OrderId**: Associated order
- **FromStatus**: Previous status
- **ToStatus**: New status
- **ChangedDate**: When status changed
- **ChangedBy**: User who changed status
- **Reason**: Reason for status change
- **Notes**: Additional notes

## Order Status Flow

```
Pending → Confirmed → Processing → Shipped → Delivered
   ↓         ↓           ↓          ↓         ↓
Cancelled  Cancelled   On Hold   Returned  Completed
```

### Status Descriptions
- **Pending**: Order placed, awaiting confirmation
- **Confirmed**: Order confirmed and accepted
- **Processing**: Order is being processed/fulfilled
- **On Hold**: Order temporarily paused
- **Shipped**: Order has been shipped
- **Delivered**: Order successfully delivered
- **Completed**: Order fully completed
- **Cancelled**: Order cancelled
- **Returned**: Order returned by customer

## Business Rules

1. **Status Validation**: Orders can only transition to valid next statuses
2. **Customer Association**: Orders must belong to active customers
3. **Amount Validation**: Order amounts must be positive values
4. **Date Validation**: Requested dates must be in the future
5. **Cancellation Rules**: Orders can only be cancelled in certain statuses
6. **History Tracking**: All status changes must be logged

## Status Transition Rules

| From Status | Valid Next Statuses |
|-------------|-------------------|
| Pending | Confirmed, Cancelled |
| Confirmed | Processing, Cancelled |
| Processing | Shipped, On Hold, Cancelled |
| On Hold | Processing, Cancelled |
| Shipped | Delivered, Returned |
| Delivered | Completed, Returned |
| Returned | Processing, Completed |

## Integration Points

- **Customer Service**: Orders are tied to specific customers
- **Product Service**: Validates products and pricing
- **Inventory Service**: Checks product availability
- **Shipping Service**: Handles order shipment
- **Payment Service**: Processes order payments
- **Notification Service**: Sends order status updates