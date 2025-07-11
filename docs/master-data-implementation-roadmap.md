# Master Data Services Implementation Roadmap

## 🎯 **Current Status & Business Requirements**

Based on the refined business requirements, here's the updated Master Data architecture and implementation priority:

### **Business Model Clarification**
- **Customer** can use a **Transporter** OR can be a **Transporter** themselves
- **Transporter** has **Vehicles** and **Drivers**
- **Transporter** can have vehicles registered in multiple **SACCOs** (external management)
- **Route** is simple: Plant-to-Plant with Gate-level tracking and timing
- **Customer/Supplier** has **Orders** for specific **Products**

## 🗺️ **Revised Master Data Architecture**

### **Entity Relationships Visual**
```
┌─────────────────────────────────────────────────────────────────────────┐
│                     CORE BUSINESS ENTITIES                             │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  ┌─────────────────┐    ┌─────────────────────────────────────────┐    │
│  │    CUSTOMER     │◄──►│            ORDER                        │    │
│  │    :7008        │    │         (Customer/Supplier)             │    │
│  │ • Company Info  │    │ • Product Requirements                  │    │
│  │ • Contracts     │    │ • Quantities                           │    │
│  │ • Contacts      │    │ • Delivery Schedule                    │    │
│  │ • Payment Terms │    │ • Status Tracking                      │    │
│  └─────────┬───────┘    └─────────────────┬───────────────────────┘    │
│            │                              │                            │
│            │ (can be)                     │ (references)               │
│            ▼                              ▼                            │
│  ┌─────────────────┐    ┌─────────────────────────────────────────┐    │
│  │  TRANSPORTER    │    │            PRODUCT                      │    │
│  │    :7010        │    │            :7005                        │    │
│  │ • Fleet Mgmt    │    │ • Catalog                              │    │
│  │ • Business Info │    │ • Specifications                       │    │
│  │ • Licenses      │    │ • Categories                           │    │
│  │ • Insurance     │    │ • Quality Standards                    │    │
│  └─────────┬───────┘    └─────────────────────────────────────────┘    │
│            │                                                           │
│            │ (owns/manages)                                            │
│            ▼                                                           │
│  ┌─────────────────┐    ┌─────────────────────────────────────────┐    │
│  │    VEHICLE      │    │            DRIVER                       │    │
│  │    :7003        │    │            :7004                        │    │
│  │ • Registration  │    │ • License Info                         │    │
│  │ • Specifications│    │ • Certifications                       │    │
│  │ • Insurance     │    │ • Violations                           │    │
│  │ • SACCO Links   │    │ • Employment History                   │    │
│  └─────────────────┘    └─────────────────────────────────────────┘    │
│            │                              │                            │
│            └──────────────┬───────────────┘                            │
│                           │ (uses)                                     │
│                           ▼                                             │
│  ┌─────────────────────────────────────────────────────────────────┐    │
│  │                    ROUTE                                        │    │
│  │                    :7006                                        │    │
│  │ • Origin Plant (e.g., Mombasa Plant)                          │    │
│  │ • Destination Plant (e.g., Nairobi Plant)                     │    │
│  │ • Gate Mapping (Gate A In → Gate B Out)                       │    │
│  │ • Expected Times vs Actual Times                              │    │
│  │ • Distance & Duration                                          │    │
│  └─────────────────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────────────────┘
```

## 📋 **Implementation Priority & Logical Order**

### **Phase 1: Foundation Services** ✅ *Completed*
1. **User Service** ✅ - Authentication & authorization foundation
2. **Organization Service** ✅ - Multi-tenant structure

### **Phase 2: Business Entity Services** 🎯 *Next Priority*

#### **Option A: Customer-First Approach** (Recommended)
```
Priority 1: Customer Service (7008)
┌─────────────────────────────────────────────────────────────────────────┐
│                         CUSTOMER SERVICE                               │
├─────────────────────────────────────────────────────────────────────────┤
│  Business Justification:                                               │
│  • Customers are the revenue source - they place orders               │
│  • Customer data drives all transactions                              │
│  • Establishes the "who" for business relationships                   │
│  • Can be both customer AND transporter (dual role)                  │
│                                                                        │
│  Core Entities:                                                       │
│  • Customer/Supplier (dual purpose)                                   │
│  • Contracts & Agreements                                             │
│  • Contact Information                                                │
│  • Payment Terms & Credit Limits                                      │
│  • Order History & Preferences                                        │
│                                                                        │
│  API Endpoints:                                                       │
│  • GET /api/customers - List all customers                           │
│  • POST /api/customers - Create new customer                         │
│  • GET /api/customers/{id}/orders - Customer order history           │
│  • GET /api/customers/{id}/contracts - Customer contracts            │
│  • PUT /api/customers/{id}/credit-limit - Update credit terms        │
└─────────────────────────────────────────────────────────────────────────┘
```

#### **Option B: Product-First Approach**
```
Priority 1: Product Service (7005)
┌─────────────────────────────────────────────────────────────────────────┐
│                         PRODUCT SERVICE                                │
├─────────────────────────────────────────────────────────────────────────┤
│  Business Justification:                                               │
│  • Products define what is being weighed/transported                  │
│  • Product specifications drive quality control                       │
│  • Establishes the "what" for all transactions                        │
│  • Required for order management and pricing                          │
│                                                                        │
│  Core Entities:                                                       │
│  • Products & SKUs                                                    │
│  • Categories & Classifications                                       │
│  • Specifications & Quality Standards                                 │
│  • Pricing & Units of Measure                                         │
│  • Regulatory Requirements                                             │
└─────────────────────────────────────────────────────────────────────────┘
```

### **Recommended Next Service: Customer Service** 🎯

**Why Customer Service First:**
1. **Business Priority**: Customers generate revenue through orders
2. **Data Foundation**: Customer information drives transaction requirements
3. **Dual Role Support**: Can handle both customer and transporter roles
4. **Order Management**: Sets up foundation for order-driven operations
5. **Contract Management**: Establishes pricing and terms

## 🏗️ **Customer Service Detailed Design**

### **Core Entities & Relationships**
```
┌─────────────────────────────────────────────────────────────────────────┐
│                      CUSTOMER SERVICE ENTITIES                         │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  Customer/Supplier (Main Entity)                                       │
│  ┌─────────────────────────────────────────────────────────────────┐    │
│  │ • ID, Name, Type (Customer/Supplier/Both)                      │    │
│  │ • Business Registration, Tax Info                              │    │
│  │ • Address, Contact Information                                 │    │
│  │ • Credit Rating, Payment Terms                                 │    │
│  │ • IsTransporter (boolean - can they transport?)               │    │
│  │ • PreferredTransporter (if they use external)                 │    │
│  │ • Status (Active/Inactive/Suspended)                          │    │
│  └─────────────────────────────────────────────────────────────────┘    │
│                               │                                         │
│                               │ (has)                                   │
│                               ▼                                         │
│  Order (Business Transaction)                                          │
│  ┌─────────────────────────────────────────────────────────────────┐    │
│  │ • OrderNumber, CustomerID, SupplierID                          │    │
│  │ • ProductID, Quantity, UnitPrice                               │    │
│  │ • ExpectedDeliveryDate, ActualDeliveryDate                     │    │
│  │ • Status (Pending/InTransit/Delivered/Cancelled)               │    │
│  │ • TransporterID (who will transport)                           │    │
│  │ • RouteID (planned route)                                      │    │
│  │ • SpecialInstructions, QualityRequirements                     │    │
│  └─────────────────────────────────────────────────────────────────┘    │
│                               │                                         │
│                               │ (governed by)                           │
│                               ▼                                         │
│  Contract (Business Agreement)                                         │
│  ┌─────────────────────────────────────────────────────────────────┐    │
│  │ • ContractNumber, CustomerID                                   │    │
│  │ • StartDate, EndDate, RenewalTerms                             │    │
│  │ • PaymentTerms (30/60/90 days)                                 │    │
│  │ • CreditLimit, SecurityDeposit                                 │    │
│  │ • ProductCategories, Volume Commitments                        │    │
│  │ • PricingStructure, Discounts                                  │    │
│  │ • PenaltyTerms, DeliveryTerms                                  │    │
│  └─────────────────────────────────────────────────────────────────┘    │
│                               │                                         │
│                               │ (has)                                   │
│                               ▼                                         │
│  Contact (People & Communication)                                      │
│  ┌─────────────────────────────────────────────────────────────────┐    │
│  │ • ContactID, CustomerID                                        │    │
│  │ • Name, Title, Department                                      │    │
│  │ • Phone, Email, Address                                        │    │
│  │ • IsPrimary, ContactType (Sales/Operations/Finance)           │    │
│  │ • PreferredCommunication, Languages                            │    │
│  └─────────────────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────────────────┘
```

### **Customer Service API Design**
```
┌─────────────────────────────────────────────────────────────────────────┐
│                      CUSTOMER SERVICE APIs                             │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  Customer Management                                                    │
│  ├─ GET    /api/customers                    # List all customers       │
│  ├─ POST   /api/customers                    # Create new customer      │
│  ├─ GET    /api/customers/{id}               # Get customer details     │
│  ├─ PUT    /api/customers/{id}               # Update customer          │
│  ├─ DELETE /api/customers/{id}               # Soft delete customer     │
│  └─ GET    /api/customers/search?q={term}    # Search customers         │
│                                                                         │
│  Order Management                                                       │
│  ├─ GET    /api/customers/{id}/orders        # Customer orders          │
│  ├─ POST   /api/customers/{id}/orders        # Create order             │
│  ├─ GET    /api/orders/{orderNumber}         # Order details            │
│  ├─ PUT    /api/orders/{orderNumber}/status  # Update order status      │
│  └─ GET    /api/orders/pending               # All pending orders       │
│                                                                         │
│  Contract Management                                                    │
│  ├─ GET    /api/customers/{id}/contracts     # Customer contracts       │
│  ├─ POST   /api/customers/{id}/contracts     # Create contract          │
│  ├─ GET    /api/contracts/{contractNumber}   # Contract details         │
│  ├─ PUT    /api/contracts/{contractNumber}   # Update contract          │
│  └─ GET    /api/contracts/expiring           # Expiring contracts       │
│                                                                         │
│  Contact Management                                                     │
│  ├─ GET    /api/customers/{id}/contacts      # Customer contacts        │
│  ├─ POST   /api/customers/{id}/contacts      # Add contact              │
│  ├─ PUT    /api/contacts/{contactId}         # Update contact           │
│  └─ DELETE /api/contacts/{contactId}         # Remove contact           │
│                                                                         │
│  Business Intelligence                                                  │
│  ├─ GET    /api/customers/{id}/performance   # Customer performance     │
│  ├─ GET    /api/customers/{id}/credit-status # Credit standing          │
│  └─ GET    /api/customers/analytics          # Customer analytics       │
└─────────────────────────────────────────────────────────────────────────┘
```

## 🔄 **Business Flow Integration**

### **Customer-Order-Transport Flow**
```
┌─────────────────────────────────────────────────────────────────────────┐
│                    BUSINESS PROCESS FLOW                               │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  1. Customer Places Order                                               │
│     ┌─────────────────────────────────────────────────────────────┐    │
│     │ Customer Service creates Order                              │    │
│     │ • Product requirements                                      │    │
│     │ • Delivery location & timing                               │    │
│     │ • Quality specifications                                    │    │
│     │ • Transportation requirements                               │    │
│     └─────────────────┬───────────────────────────────────────────┘    │
│                       │                                                 │
│  2. Transportation Assignment                                           │
│     ┌─────────────────▼───────────────────────────────────────────┐    │
│     │ IF Customer.IsTransporter = TRUE                           │    │
│     │   → Customer handles own transport                         │    │
│     │ ELSE                                                       │    │
│     │   → Assign external Transporter                           │    │
│     │   → Create Transport Contract                              │    │
│     └─────────────────┬───────────────────────────────────────────┘    │
│                       │                                                 │
│  3. Route Planning                                                      │
│     ┌─────────────────▼───────────────────────────────────────────┐    │
│     │ Route Service creates route plan                           │    │
│     │ • Origin Plant → Destination Plant                         │    │
│     │ • Gate A (Entry) → Gate B (Exit)                          │    │
│     │ • Expected times vs Actual times                          │    │
│     │ • Vehicle and driver assignment                           │    │
│     └─────────────────┬───────────────────────────────────────────┘    │
│                       │                                                 │
│  4. Execution & Tracking                                                │
│     ┌─────────────────▼───────────────────────────────────────────┐    │
│     │ Weight Data Service captures weighing                      │    │
│     │ Transaction Service orchestrates process                   │    │
│     │ Compliance Service ensures regulations                     │    │
│     │ Customer receives delivery confirmation                    │    │
│     └─────────────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────────────────┘
```

## 🎯 **Implementation Recommendation**

### **Start with Customer Service** because:
1. **Foundation for Revenue**: Customers drive all business transactions
2. **Order Management**: Sets up order-driven workflow
3. **Dual Role Support**: Handles customer-as-transporter scenarios
4. **Contract Framework**: Establishes pricing and terms
5. **Integration Ready**: Easily connects to future Transporter and Product services

### **Customer Service Implementation Plan**
1. **Phase 1**: Core customer CRUD operations
2. **Phase 2**: Order management system
3. **Phase 3**: Contract management
4. **Phase 4**: Contact management and communication
5. **Phase 5**: Business intelligence and analytics

**Next Services After Customer**:
1. **Product Service** (defines what customers order)
2. **Transporter Service** (handles who transports)
3. **Vehicle Service** (transporter's fleet)
4. **Driver Service** (transporter's personnel)
5. **Route Service** (transportation planning)

This approach builds the business foundation first, then adds operational capabilities layer by layer.

---

*Master Data Services Implementation Roadmap*  
*Customer-centric approach for business value delivery*