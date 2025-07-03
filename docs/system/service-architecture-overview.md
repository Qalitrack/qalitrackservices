# Service Architecture Overview

## Master Data vs DataManager Services

This document explains the architectural distinction between Master Data Services and DataManager Services in the QaliTrack system.

## **Master Data Services** (Reference Data)

**Purpose:** Manage **foundational/reference data** that other systems use

**Characteristics:**
- Relatively **static** data that changes infrequently
- **Shared across multiple applications**
- **Single source of truth** for business entities
- **CRUD operations** with validation
- **No complex business workflows**

**Master Data Services in QaliTrack:**
- **Vehicle Service** - Vehicle registrations, specs (data changes when new vehicle registered)
- **Driver Service** - Driver profiles, licenses (data changes when driver hired/license renewed)
- **Product Service** - Product catalog, specifications (data changes when new products added)
- **Route Service** - Transport paths and restrictions (data changes when routes updated)
- **Weighbridge Service** - Weighbridge locations and configurations (data changes when infrastructure updated)
- **Supplier Service** - Vendor management and contracts (data changes when relationships established)
- **Customer Service** - Client management and contracts (data changes when relationships established)
- **Transporter Service** - Fleet companies and operations (data changes when transporters onboarded)
- **Organization Service** - Multi-tenant context and hierarchy (data changes during organizational changes)
- **Sacco Service** - Cooperative organizations (data changes when SACCOs registered)

## **DataManager Services** (Operational Data)

**Purpose:** Manage **operational/transactional data** with complex business logic

**Characteristics:**
- **High-frequency** data that changes constantly
- **Complex business workflows** and state machines
- **Event-driven** with real-time processing
- **Advanced analytics** and reporting
- **Integration** between multiple master data entities

**DataManager Services in QaliTrack:**
- **Weight Data Service** - Real-time weighbridge measurements (hundreds per day)
- **Transaction Service** - Complex weighing workflows involving multiple master data entities
- **Compliance Service** - Real-time rule evaluation using master data
- **Analytics Service** - Processing operational data to generate insights
- **Operational Data Service** - Product & route management with weighbridge orchestration
- **Data Sync Service** - Multi-site synchronization with conflict resolution
- **Archive Service** - Long-term storage with full entity history

## **Key Differences**

| Aspect | Master Data | DataManager |
|--------|-------------|-------------|
| **Data Type** | Reference/Static | Operational/Dynamic |
| **Change Frequency** | Low (weekly/monthly) | High (hourly/daily) |
| **Complexity** | Simple CRUD | Complex workflows |
| **Dependencies** | Minimal (organization context) | Multiple master data services |
| **Purpose** | Data storage & validation | Business logic & processing |
| **Users** | Administrative users | Operational users |
| **Examples** | "What vehicles exist?" | "How much does this truck weigh right now?" |

## **Relationship Example**

**Master Data Context:**
```
Vehicle Service: "Vehicle ABC123 is a truck with max weight 80,000kg"
Driver Service: "Driver D123 has valid license expiring 2025-12-31"
Product Service: "Product P789 is maize with density 1.2kg/L"
Supplier Service: "Supplier S456 is Acme Grain Co. with contract rate $50/ton"
```

**DataManager Operations:**
```
Weight Data Service: "Vehicle ABC123 weighs 75,500kg at Weighbridge 1"
Transaction Service: "Transaction T001 involves Vehicle ABC123, Driver D123, 
                    Supplier S456, Product P789 with entry weight 45,000kg 
                    and exit weight 75,500kg"
Compliance Service: "Vehicle ABC123 is within weight limits, Driver D123 
                    license is valid, route approved for Product P789"
Analytics Service: "Supplier S456 average turnaround time is 45 minutes, 
                   Product P789 shows 15% volume increase this month"
```

## **In QaliTrack Context**

- **Master Data Services** = The **"who, what, where"** (vehicles, drivers, products, routes)
- **DataManager Services** = The **"how, when, why"** (weighing transactions, compliance checks, analytics)

The **DataManager services consume Master Data services** to perform operational business functions. Master Data provides the context, DataManager provides the action!

## **Service Dependencies**

### **Phase 1: Foundation**
- Organization Service (multi-tenant context)
- User Service (authentication)

### **Phase 2: Master Data Layer**
- Vehicle, Driver, Product, Route, Weighbridge Services
- Supplier, Customer, Transporter, Sacco Services

### **Phase 3: Operational Layer**  
- Weight Data, Compliance, Operational Data Services

### **Phase 4: Business Logic Layer**
- Transaction Service (orchestrates all master data)
- Analytics Service (processes all operational data)

### **Phase 5: Infrastructure Layer**
- Data Sync Service (synchronizes across sites)
- Archive Service (manages data lifecycle)

## **Package Architecture**

Both Master Data and DataManager services are implemented as **reusable packages** with:
- **API Controllers** for HTTP endpoints
- **Business Logic** in Core layer
- **Data Access** in Infrastructure layer
- **Dependency Injection** extensions for easy composition

Applications consume these packages to build domain-specific solutions:
- **Weighbridge Management App** (uses Weight Data + Transaction + Master Data)
- **Fleet Management App** (uses Vehicle + Driver + Transporter)
- **Compliance Monitoring App** (uses Compliance + all relevant Master Data)

This architecture ensures **separation of concerns**, **reusability**, and **scalable composition** of business capabilities.