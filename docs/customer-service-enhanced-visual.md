# Customer Service - Enhanced Business Model Implementation

## 🎯 **Enhanced Customer Service Overview**

The Customer Service has been enhanced to support the complete business model requirements, including **dual-role customers** (customer + transporter) and **order-driven workflows**.

## 🏗️ **Enhanced Entity Relationship Model**

### **Core Business Entities Visual**
```
┌─────────────────────────────────────────────────────────────────────────┐
│                     ENHANCED CUSTOMER SERVICE                          │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  ┌─────────────────────────────────────────────────────────────────┐    │
│  │                    CUSTOMER (Enhanced)                         │    │
│  │                                                                 │    │
│  │  Core Properties:                                               │    │
│  │  • Name, Email, Phone, Address                                  │    │
│  │  • TaxNumber, RegistrationNumber                                │    │
│  │  • CustomerType (Individual/Corporate/Government)               │    │
│  │  • Status (Active/Inactive/Suspended/Pending)                  │    │
│  │                                                                 │    │
│  │  🚛 Transporter Capabilities (NEW):                           │    │
│  │  • IsTransporter (boolean)                                     │    │
│  │  • TransporterLicenseNumber                                     │    │
│  │  • PreferredTransporterId (external transporter)               │    │
│  │  • PreferredTransporterName                                     │    │
│  │                                                                 │    │
│  │  🏢 Business Classification (NEW):                            │    │
│  │  • IsSupplier (can supply products)                            │    │
│  │  • IsBuyer (can purchase products)                             │    │
│  │  • PaymentTermsDays (30/60/90 days)                           │    │
│  │  • Currency (KES, USD, etc.)                                   │    │
│  └─────────────────┬───────────────────────────────────────────────┘    │
│                    │                                                     │
│                    │ (places/receives)                                   │
│                    ▼                                                     │
│  ┌─────────────────────────────────────────────────────────────────┐    │
│  │                    ORDER (NEW)                                  │    │
│  │                                                                 │    │
│  │  Order Details:                                                 │    │
│  │  • OrderNumber (unique)                                         │    │
│  │  • CustomerId, SupplierId                                       │    │
│  │  • ProductId, ProductName                                       │    │
│  │  • Quantity, UnitOfMeasure, UnitPrice, TotalAmount             │    │
│  │  • OrderDate, ExpectedDeliveryDate, ActualDeliveryDate         │    │
│  │  • Status (Pending → InTransit → Completed)                    │    │
│  │  • OrderType (Purchase/Sale/Transfer/Sample)                   │    │
│  │                                                                 │    │
│  │  🚚 Transportation Details (NEW):                              │    │
│  │  • TransporterId, TransporterName                              │    │
│  │  • IsCustomerTransporting (customer handles own transport)     │    │
│  │  • RouteId (planned route)                                     │    │
│  │  • OriginLocation → DestinationLocation                        │    │
│  │                                                                 │    │
│  │  📋 Quality & Specifications:                                  │    │
│  │  • QualitySpecifications                                       │    │
│  │  • SpecialInstructions                                         │    │
│  │  • TolerancePercentage                                         │    │
│  │                                                                 │    │
│  │  📄 References & Tracking:                                     │    │
│  │  • CustomerOrderReference                                      │    │
│  │  • SupplierOrderReference                                      │    │
│  │  • WeighingTransactionId (links to weight data)               │    │
│  └─────────────────┬───────────────────────────────────────────────┘    │
│                    │                                                     │
│                    │ (tracks changes)                                    │
│                    ▼                                                     │
│  ┌─────────────────────────────────────────────────────────────────┐    │
│  │                ORDER STATUS HISTORY (NEW)                      │    │
│  │                                                                 │    │
│  │  • FromStatus → ToStatus                                        │    │
│  │  • ChangedAt, ChangedBy                                         │    │
│  │  • Reason, Notes                                                │    │
│  │  • Complete audit trail of order lifecycle                     │    │
│  └─────────────────────────────────────────────────────────────────┘    │
│                                                                         │
│  Supporting Entities (Existing):                                       │
│  ┌─────────────────┐ ┌─────────────────┐ ┌─────────────────────────┐  │
│  │ CustomerContact │ │ CustomerContract│ │ CustomerBilling         │  │
│  │ • Multiple      │ │ • Terms & Conds │ │ • Payment Details       │  │
│  │ • Contact Points│ │ • Service Levels│ │ • Credit Management     │  │
│  └─────────────────┘ └─────────────────┘ └─────────────────────────┘  │
└─────────────────────────────────────────────────────────────────────────┘
```

## 🔄 **Business Process Flow**

### **Customer-Order-Transportation Flow**
```
┌─────────────────────────────────────────────────────────────────────────┐
│                    ENHANCED BUSINESS WORKFLOW                           │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  1. Customer Places Order                                               │
│     ┌─────────────────────────────────────────────────────────────┐    │
│     │ POST /api/orders                                            │    │
│     │ {                                                           │    │
│     │   "customerId": "cust-123",                                 │    │
│     │   "productId": "cement-grade42",                           │    │
│     │   "quantity": 1000,                                        │    │
│     │   "unitOfMeasure": "tons",                                 │    │
│     │   "expectedDeliveryDate": "2025-01-15",                   │    │
│     │   "isCustomerTransporting": true,                         │    │
│     │   "originLocation": "Mombasa Plant",                      │    │
│     │   "destinationLocation": "Nairobi Site Gate A"           │    │
│     │ }                                                           │    │
│     └─────────────────┬───────────────────────────────────────────┘    │
│                       │                                                 │
│  2. Transportation Assignment Logic                                     │
│     ┌─────────────────▼───────────────────────────────────────────┐    │
│     │ IF customer.IsTransporter = TRUE                           │    │
│     │   → Set order.IsCustomerTransporting = true               │    │
│     │   → Customer handles own vehicles & drivers               │    │
│     │   → Use customer's fleet management                       │    │
│     │ ELSE                                                       │    │
│     │   → Set order.TransporterId = customer.PreferredTransporter│   │
│     │   → External transporter manages transport                │    │
│     │   → Create transport contract                              │    │
│     └─────────────────┬───────────────────────────────────────────┘    │
│                       │                                                 │
│  3. Route Planning                                                      │
│     ┌─────────────────▼───────────────────────────────────────────┐    │
│     │ Route Service Integration:                                  │    │
│     │ • Origin Plant (e.g., "Mombasa Plant")                    │    │
│     │ • Destination Plant (e.g., "Nairobi Plant")               │    │
│     │ • Gate Mapping: "Gate A In" → "Gate B Out"                │    │
│     │ • Expected Times vs Actual Times                          │    │
│     │ • Vehicle and driver assignment                           │    │
│     │ • Distance, duration, route optimization                  │    │
│     └─────────────────┬───────────────────────────────────────────┘    │
│                       │                                                 │
│  4. Order Status Tracking                                               │
│     ┌─────────────────▼───────────────────────────────────────────┐    │
│     │ Status Progression:                                         │    │
│     │ Pending → Confirmed → InProduction → ReadyForShipment      │    │
│     │ → InTransit → AtDestination → WeighingInProgress           │    │
│     │ → Completed                                                 │    │
│     │                                                             │    │
│     │ Each status change creates OrderStatusHistory entry        │    │
│     │ • Timestamp, user, reason, notes                          │    │
│     │ • Complete audit trail for compliance                     │    │
│     └─────────────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────────────────┘
```

## 📊 **Enhanced API Endpoints**

### **Customer Management APIs** (Enhanced)
```
┌─────────────────────────────────────────────────────────────────────────┐
│                      CUSTOMER MANAGEMENT APIs                          │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  Basic Customer Operations:                                             │
│  ├─ GET    /api/customers                    # List customers           │
│  ├─ POST   /api/customers                    # Create customer          │
│  ├─ GET    /api/customers/{id}               # Get customer details     │
│  ├─ PUT    /api/customers/{id}               # Update customer          │
│  ├─ DELETE /api/customers/{id}               # Soft delete customer     │
│  └─ GET    /api/customers/search?q={term}    # Search customers         │
│                                                                         │
│  🚛 Transporter-Specific APIs (NEW):                                   │
│  ├─ GET    /api/customers/transporters       # List customer-transporters│
│  ├─ POST   /api/customers/{id}/make-transporter # Enable transporter   │
│  ├─ DELETE /api/customers/{id}/remove-transporter # Disable transporter│
│  └─ GET    /api/customers/{id}/fleet          # Get customer's fleet    │
│                                                                         │
│  Contact & Contract Management:                                         │
│  ├─ GET    /api/customers/{id}/contacts       # Customer contacts       │
│  ├─ POST   /api/customers/{id}/contacts       # Add contact             │
│  ├─ GET    /api/customers/{id}/contracts      # Customer contracts      │
│  └─ POST   /api/customers/{id}/contracts      # Create contract         │
└─────────────────────────────────────────────────────────────────────────┘
```

### **Order Management APIs** (NEW)
```
┌─────────────────────────────────────────────────────────────────────────┐
│                        ORDER MANAGEMENT APIs                           │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  Core Order Operations:                                                 │
│  ├─ GET    /api/orders                       # List all orders          │
│  ├─ POST   /api/orders                       # Create new order         │
│  ├─ GET    /api/orders/{id}                  # Get order details        │
│  ├─ PUT    /api/orders/{id}                  # Update order             │
│  ├─ POST   /api/orders/{id}/status           # Update order status      │
│  └─ DELETE /api/orders/{id}                  # Cancel order             │
│                                                                         │
│  Business Intelligence:                                                 │
│  ├─ GET    /api/orders/customer/{customerId} # Customer's orders        │
│  ├─ GET    /api/orders/supplier/{supplierId} # Supplier's orders        │
│  ├─ GET    /api/orders/pending               # All pending orders       │
│  ├─ GET    /api/orders/in-transit            # Orders in transport      │
│  └─ GET    /api/orders/search?q={query}      # Search orders            │
│                                                                         │
│  Audit & Tracking:                                                     │
│  ├─ GET    /api/orders/{id}/history          # Order status history     │
│  ├─ GET    /api/orders/{id}/documents        # Order documents          │
│  └─ POST   /api/orders/{id}/documents        # Upload order documents   │
│                                                                         │
│  Transportation Integration:                                            │
│  ├─ GET    /api/orders/customer-transport    # Self-transported orders  │
│  ├─ GET    /api/orders/external-transport    # External-transported     │
│  └─ POST   /api/orders/{id}/assign-transport # Assign transporter       │
└─────────────────────────────────────────────────────────────────────────┘
```

## 🔗 **Service Integration Points**

### **Customer Service ↔ Other Services**
```
┌─────────────────────────────────────────────────────────────────────────┐
│                       SERVICE INTEGRATION MATRIX                       │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  📦 Product Service Integration:                                        │
│  ┌─────────────────────────────────────────────────────────────────┐    │
│  │ • Order.ProductId → Product Service validation                 │    │
│  │ • Product specifications → Order quality requirements          │    │
│  │ • Product pricing → Order unit price calculation               │    │
│  │ • Product availability → Order confirmation                    │    │
│  └─────────────────────────────────────────────────────────────────┘    │
│                                                                         │
│  🚛 Transporter Service Integration:                                    │
│  ┌─────────────────────────────────────────────────────────────────┐    │
│  │ • Customer.IsTransporter → Transporter Service registration    │    │
│  │ • Order.TransporterId → Transporter assignment                 │    │
│  │ • Transport capacity → Order scheduling                        │    │
│  │ • Fleet management → Vehicle/driver assignment                 │    │
│  └─────────────────────────────────────────────────────────────────┘    │
│                                                                         │
│  🗺️ Route Service Integration:                                          │
│  ┌─────────────────────────────────────────────────────────────────┐    │
│  │ • Order.RouteId → Route planning                               │    │
│  │ • Origin/Destination → Route optimization                      │    │
│  │ • Expected delivery times → Route scheduling                   │    │
│  │ • Gate management → Entry/exit tracking                        │    │
│  └─────────────────────────────────────────────────────────────────┘    │
│                                                                         │
│  ⚖️ Weight Data Service Integration:                                     │
│  ┌─────────────────────────────────────────────────────────────────┐    │
│  │ • Order.WeighingTransactionId → Weight measurement linking     │    │
│  │ • Order quantities → Weight validation                         │    │
│  │ • Quality specifications → Weight compliance checking          │    │
│  │ • Tolerance limits → Weight acceptance criteria                │    │
│  └─────────────────────────────────────────────────────────────────┘    │
│                                                                         │
│  📊 Transaction Service Integration:                                    │
│  ┌─────────────────────────────────────────────────────────────────┐    │
│  │ • Order completion → Transaction generation                     │    │
│  │ • Customer billing → Invoice creation                           │    │
│  │ • Payment tracking → Order status updates                      │    │
│  │ • Contract terms → Transaction validation                      │    │
│  └─────────────────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────────────────┘
```

## 📈 **Business Value & Use Cases**

### **Real-World Scenarios**

#### **Scenario 1: Customer as Transporter**
```
📋 Business Case: Cement Company with Own Fleet

Customer: "Bamburi Cement Ltd"
• IsTransporter: true
• TransporterLicenseNumber: "TL-2024-001"
• Own fleet of 50 trucks
• Own drivers and logistics team

Order Flow:
1. Bamburi places order for raw materials
2. System detects IsTransporter = true
3. Sets IsCustomerTransporting = true
4. Bamburi assigns own vehicle and driver
5. Route planned from supplier to Bamburi plant
6. Bamburi handles entire transportation process
```

#### **Scenario 2: Customer Using External Transporter**
```
📋 Business Case: Small Construction Company

Customer: "Ujenzi Builders Ltd"
• IsTransporter: false
• PreferredTransporterId: "transporter-123"
• PreferredTransporterName: "KenTrans Logistics"

Order Flow:
1. Ujenzi places order for cement
2. System detects IsTransporter = false
3. Assigns PreferredTransporter automatically
4. KenTrans receives transport assignment
5. Route planned and vehicle assigned by KenTrans
6. External transporter handles transportation
```

#### **Scenario 3: Dual Role - Customer and Supplier**
```
📋 Business Case: Regional Distributor

Customer: "EastAfrica Cement Distributors"
• IsSupplier: true (supplies to retailers)
• IsBuyer: true (buys from manufacturers)
• IsTransporter: true (own distribution fleet)

Order Flows:
1. Purchase Orders: Buying from manufacturers
2. Sales Orders: Selling to retailers
3. Transfer Orders: Moving stock between warehouses
4. All using own transportation capabilities
```

## 🎯 **Summary of Enhancements**

### **✅ Completed Features**
1. **Dual-Role Customers**: Customers can be transporters
2. **Order Management**: Complete order lifecycle
3. **Status Tracking**: Audit trail with history
4. **Transportation Logic**: Customer vs external transport
5. **Business Classification**: Supplier/buyer/transporter roles
6. **Quality Management**: Specifications and tolerances
7. **Reference Tracking**: Links between orders and weighing

### **🚀 Next Integration Steps**
1. **Product Service**: Product catalog integration
2. **Route Service**: Route planning and optimization
3. **Vehicle/Driver Services**: Fleet management
4. **Weight Data Service**: Weighing transaction linkage
5. **Transaction Service**: End-to-end business process

The enhanced Customer Service now provides the foundation for the complete QaliTrack business model, supporting complex multi-role relationships and order-driven workflows that reflect real-world cement industry operations.

---

*Enhanced Customer Service - Supporting Complex Business Relationships*  
*QaliTrack Master Data Service with Order Management Capabilities*