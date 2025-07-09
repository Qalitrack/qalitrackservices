# QaliTrack Microservices - Visual Architecture Guide

## 🏗️ **System Overview**

QaliTrack is a **weighbridge management system** with 19 microservices organized around the **WHO vs HOW** principle:

```
┌─────────────────────────────────────────────────────────────────────────┐
│                        QaliTrack Ecosystem                             │
│                    Industrial Weighbridge Management                    │
└─────────────────────────────────────────────────────────────────────────┘
                                    │
                ┌───────────────────┼───────────────────┐
                │                   │                   │
        ┌───────▼────────┐ ┌───────▼────────┐ ┌────────▼────────┐
        │ Master Data    │ │ DataManager    │ │ Infrastructure  │
        │ Services       │ │ Services       │ │ Services        │
        │ (WHO/WHAT)     │ │ (HOW/WHEN)     │ │ (SUPPORT)       │
        └────────────────┘ └────────────────┘ └─────────────────┘
```

## 🎯 **Service Categories**

### 🏢 **Infrastructure Services** (2 services)
```
┌─────────────────────────────────────────────────────────────────────────┐
│                        INFRASTRUCTURE LAYER                            │
├─────────────────────────┬───────────────────────────────────────────────┤
│  API Gateway :7000      │  Service Discovery :7019                     │
│  ┌─────────────────┐   │  ┌─────────────────────────────────────────┐  │
│  │ • JWT Auth      │   │  │ • Service Registry                      │  │
│  │ • Role Check    │   │  │ • Health Aggregation                   │  │
│  │ • Route Guard   │   │  │ • Load Balancing                       │  │
│  │ • Rate Limiting │   │  │ • Service Discovery                    │  │
│  └─────────────────┘   │  └─────────────────────────────────────────┘  │
└─────────────────────────┴───────────────────────────────────────────────┘
```

### 👥 **Master Data Services** (11 services) - "The WHO/WHAT/WHERE"
```
┌─────────────────────────────────────────────────────────────────────────┐
│                           MASTER DATA LAYER                            │
├─────────────────────────────────────────────────────────────────────────┤
│  Identity & Organization                                                │
│  ┌─────────────────┐    ┌─────────────────────────────────────────┐    │
│  │ User Service    │    │ Organization Service                    │    │
│  │ :7001           │    │ :7002                                   │    │
│  │ • Authentication│    │ • Multi-tenant                         │    │
│  │ • Authorization │    │ • Sites & Departments                  │    │
│  │ • JWT Tokens    │    │ • Organizational Hierarchy             │    │
│  │ • Role Management│   │ • Company Management                   │    │
│  └─────────────────┘    └─────────────────────────────────────────┘    │
├─────────────────────────────────────────────────────────────────────────┤
│  Transportation Fleet                                                   │
│  ┌─────────────────┐ ┌─────────────────┐ ┌─────────────────────────┐  │
│  │ Vehicle Service │ │ Driver Service  │ │ Route Service           │  │
│  │ :7003           │ │ :7004           │ │ :7006                   │  │
│  │ • Registration  │ │ • Licenses      │ │ • Path Planning         │  │
│  │ • Specifications│ │ • Violations    │ │ • Route Optimization    │  │
│  │ • Insurance     │ │ • Certifications│ │ • Transportation Logic  │  │
│  │ • Inspections   │ │ • Driver Profiles│ │ • Waypoints & Schedules│  │
│  └─────────────────┘ └─────────────────┘ └─────────────────────────┘  │
├─────────────────────────────────────────────────────────────────────────┤
│  Business Entities                                                      │
│  ┌─────────────────┐ ┌─────────────────┐ ┌─────────────────────────┐  │
│  │ Customer Service│ │ Supplier Service│ │ Product Service         │  │
│  │ :7008 ✅        │ │ :7009 ✅        │ │ :7005 ✅               │  │
│  │ • CRM           │ │ • Vendor Mgmt   │ │ • Catalog Management    │  │
│  │ • Order Mgmt    │ │ • Procurement   │ │ • Product Specifications│  │
│  │ • Customer Data │ │ • Contracts     │ │ • Categories & Types    │  │
│  │ • Relationships │ │ • Performance   │ │ • Pricing Management    │  │
│  └─────────────────┘ └─────────────────┘ └─────────────────────────┘  │
├─────────────────────────────────────────────────────────────────────────┤
│  Transportation Partners                                                │
│  ┌─────────────────┐ ┌─────────────────┐ ┌─────────────────────────┐  │
│  │ Transporter     │ │ Sacco Service   │ │ Weighbridge Service     │  │
│  │ Service :7010   │ │ :7011           │ │ :7007                   │  │
│  │ • Fleet Mgmt    │ │ • Cooperative   │ │ • Equipment Config      │  │
│  │ • Transport Co. │ │ • Member Mgmt   │ │ • Calibration          │  │
│  │ • Logistics     │ │ • SACCO Services│ │ • Maintenance Records  │  │
│  └─────────────────┘ └─────────────────┘ └─────────────────────────┘  │
└─────────────────────────────────────────────────────────────────────────┘
```

### ⚙️ **DataManager Services** (6 services) - "The HOW/WHEN/WHY"
```
┌─────────────────────────────────────────────────────────────────────────┐
│                         OPERATIONAL DATA LAYER                         │
├─────────────────────────────────────────────────────────────────────────┤
│  Real-time Operations                                                   │
│  ┌─────────────────┐    ┌─────────────────────────────────────────┐    │
│  │ Weight Data     │    │ Transaction Service                     │    │
│  │ Service :7012   │    │ :7015                                   │    │
│  │ • Live Weighing │    │ • End-to-end Transactions              │    │
│  │ • Hardware Link │    │ • Workflow Orchestration               │    │
│  │ • Corrections   │    │ • Document Management                  │    │
│  │ • Validation    │    │ • Business Process Engine             │    │
│  └─────────────────┘    └─────────────────────────────────────────┘    │
├─────────────────────────────────────────────────────────────────────────┤
│  Business Intelligence & Compliance                                     │
│  ┌─────────────────┐ ┌─────────────────┐ ┌─────────────────────────┐  │
│  │ Analytics       │ │ Compliance      │ │ Operational Data        │  │
│  │ Service :7016   │ │ Service :7013   │ │ Service :7014           │  │
│  │ • BI Reports    │ │ • Regulatory    │ │ • Daily Operations      │  │
│  │ • KPIs          │ │ • License Track │ │ • Coordination          │  │
│  │ • Dashboards    │ │ • Violations    │ │ • Product/Route Mgmt    │  │
│  │ • Performance   │ │ • Audit Trails  │ │ • Operational Control   │  │
│  └─────────────────┘ └─────────────────┘ └─────────────────────────┘  │
├─────────────────────────────────────────────────────────────────────────┤
│  Data Management                                                        │
│  ┌─────────────────┐    ┌─────────────────────────────────────────┐    │
│  │ Data Sync       │    │ Archive Service                         │    │
│  │ Service :7017   │    │ :7018                                   │    │
│  │ • Multi-site    │    │ • Long-term Storage                    │    │
│  │ • Synchronization│   │ • Compliance Archive                   │    │
│  │ • Conflict Res. │    │ • Data Retention                       │    │
│  │ • Distributed   │    │ • Historical Data                      │    │
│  └─────────────────┘    └─────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────────────────┘
```

## 🔄 **Customer-Supplier Integration Pattern**

### **Business Relationship Model**
```
┌─────────────────────────────────────────────────────────────────────────┐
│                    CUSTOMER-SUPPLIER INTEGRATION                        │
├─────────────────────────────────────────────────────────────────────────┤
│  Customer Service (Buyers)        │  Supplier Service (Vendors)         │
│  ┌─────────────────────────────┐  │  ┌─────────────────────────────────┐ │
│  │ • Customer as BUYER         │  │  │ • Supplier as VENDOR            │ │
│  │ • Orders cement/materials   │  │  │ • Supplies cement/materials     │ │
│  │ • References SupplierId     │  │  │ • References CustomerId         │ │
│  │ • Procurement from suppliers│  │  │ • Sales to customers            │ │
│  │ • Preferred suppliers list  │  │  │ • Customer contracts           │ │
│  └─────────────────────────────┘  │  └─────────────────────────────────┘ │
│                                   │                                     │
│  Same Entity, Different Roles:    │  Integration Points:                │
│  • Companies can be BOTH          │  • Customer orders → Supplier      │
│  • Bamburi: Customer AND Supplier │  • Supplier delivers → Customer    │
│  • Lafarge: Customer AND Supplier │  • Cross-reference validation      │
│  • Clean separation of roles      │  • Avoid data duplication          │
│                                   │  • Maintain service boundaries     │
└─────────────────────────────────────────────────────────────────────────┘
```

### **Integration Example**
```
┌─────────────────────────────────────────────────────────────────────────┐
│                        BAMBURI CEMENT EXAMPLE                           │
├─────────────────────────────────────────────────────────────────────────┤
│  Customer Service Record:         │  Supplier Service Record:           │
│  ┌─────────────────────────────┐  │  ┌─────────────────────────────────┐ │
│  │ ID: customer-bamburi-001    │  │  │ ID: supplier-bamburi-001        │ │
│  │ Name: Bamburi Cement Ltd    │  │  │ Name: Bamburi Cement Ltd        │ │
│  │ Type: Buyer                 │  │  │ Type: RawMaterial Supplier      │ │
│  │ Orders: [cement, aggregates]│  │  │ Supplies: [cement, aggregates]  │ │
│  │ PreferredSuppliers: [...]   │  │  │ CustomerContracts: [...]        │ │
│  └─────────────────────────────┘  │  └─────────────────────────────────┘ │
│                                   │                                     │
│  Same company, different business contexts and service boundaries       │
└─────────────────────────────────────────────────────────────────────────┘
```

## 🔄 **Service Interaction Flow**

### **Typical Weighing Transaction Flow**
```
┌─────────────────────────────────────────────────────────────────────────┐
│                    WEIGHING TRANSACTION WORKFLOW                        │
└─────────────────────────────────────────────────────────────────────────┘
                                    │
                            ┌───────▼───────┐
                            │ API Gateway   │
                            │ :7000         │
                            │ • Auth Check  │
                            │ • Route Guard │
                            └───────┬───────┘
                                    │
                        ┌───────────┼───────────┐
                        │           │           │
                ┌───────▼───┐ ┌─────▼─────┐ ┌─▼──────────┐
                │ User      │ │ Vehicle   │ │ Product    │
                │ Service   │ │ Service   │ │ Service    │
                │ :7001     │ │ :7003     │ │ :7005      │
                └───────────┘ └───────────┘ └────────────┘
                                    │
                            ┌───────▼───────┐
                            │ Weight Data   │
                            │ Service :7012 │
                            │ • Live Weight │
                            │ • Hardware    │
                            └───────┬───────┘
                                    │
                            ┌───────▼───────┐
                            │ Transaction   │
                            │ Service :7015 │
                            │ • Orchestrate │
                            │ • Document    │
                            └───────┬───────┘
                                    │
                        ┌───────────┼───────────┐
                        │           │           │
                ┌───────▼───┐ ┌─────▼─────┐ ┌─▼──────────┐
                │ Compliance│ │ Analytics │ │ Archive    │
                │ :7013     │ │ :7016     │ │ :7018      │
                │ • Validate│ │ • Report  │ │ • Store    │
                └───────────┘ └───────────┘ └────────────┘
```

## 🎯 **Business Domain Mapping**

### **WHO Services** (Identity & Entities)
```
┌─────────────────────────────────────────────────────────────────────────┐
│                              WHO                                        │
├─────────────────────────────────────────────────────────────────────────┤
│  👤 User Service        - Who can access the system?                   │
│  🏢 Organization Service - Which companies/sites are involved?          │
│  🚛 Vehicle Service     - What vehicles are in the system?             │
│  👷 Driver Service      - Who are the drivers?                         │
│  📦 Product Service     - What products are being weighed?             │
│  🏭 Customer Service    - Who are the customers/buyers?                │
│  🏭 Supplier Service    - Who are the suppliers/vendors?               │
│  🚚 Transporter Service - Who handles transportation?                  │
│  🏪 Sacco Service       - What cooperatives are involved?              │
│  ⚖️ Weighbridge Service - What weighing equipment exists?              │
│  🗺️ Route Service       - What routes are available?                   │
└─────────────────────────────────────────────────────────────────────────┘
```

### **HOW Services** (Operations & Processes)
```
┌─────────────────────────────────────────────────────────────────────────┐
│                              HOW                                        │
├─────────────────────────────────────────────────────────────────────────┤
│  ⚖️ Weight Data Service   - How is weight measured?                     │
│  📋 Transaction Service   - How are transactions processed?             │
│  ⚖️ Compliance Service    - How are regulations enforced?               │
│  ⚙️ Operational Data      - How are daily operations managed?           │
│  📊 Analytics Service     - How is performance measured?                │
│  🔄 Data Sync Service     - How is data synchronized?                   │
│  📚 Archive Service       - How is historical data stored?              │
└─────────────────────────────────────────────────────────────────────────┘
```

## 🔒 **Security & Authorization Model**

### **Role Hierarchy**
```
┌─────────────────────────────────────────────────────────────────────────┐
│                        ROLE HIERARCHY                                   │
├─────────────────────────────────────────────────────────────────────────┤
│  SuperAdmin (Level 5)  ┌─────────────────────────────────────────────┐ │
│  ├─ System-wide access │ • Full system control                      │ │
│  └─ All permissions    │ • All microservices access                │ │
│                        └─────────────────────────────────────────────┘ │
│  Admin (Level 4)       ┌─────────────────────────────────────────────┐ │
│  ├─ Organization-wide  │ • Organization management                  │ │
│  └─ Most permissions   │ • User management                          │ │
│                        └─────────────────────────────────────────────┘ │
│  SiteManager (Level 3) ┌─────────────────────────────────────────────┐ │
│  ├─ Site-level access  │ • Site operations                          │ │
│  └─ Operational perms  │ • Analytics & reporting                    │ │
│                        └─────────────────────────────────────────────┘ │
│  Operator (Level 2)    ┌─────────────────────────────────────────────┐ │
│  ├─ Daily operations   │ • Weight data entry                        │ │
│  └─ Basic permissions  │ • Transaction processing                   │ │
│                        └─────────────────────────────────────────────┘ │
│  User (Level 1)        ┌─────────────────────────────────────────────┐ │
│  ├─ Read-only access   │ • View own data                            │ │
│  └─ Minimal permissions│ • Basic profile management                 │ │
│                        └─────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────────────┘
```

## 🏭 **Client Deployment Examples**

### **Babumri Cement** (Full Production)
```
┌─────────────────────────────────────────────────────────────────────────┐
│                        BABUMRI CEMENT DEPLOYMENT                        │
├─────────────────────────────────────────────────────────────────────────┤
│  Infrastructure: Gateway + Service Discovery                            │
│  Master Data: All 11 services (Full identity & entity management)      │
│  DataManager: All 6 services (Complete operational suite)              │
│  Database: PostgreSQL High Availability                                 │
│  Features: Real-time weighing, Analytics, Compliance, Multi-site       │
│  Capacity: High volume, Multiple weighbridges                          │
└─────────────────────────────────────────────────────────────────────────┘
```

### **Kungu Cement** (Simplified)
```
┌─────────────────────────────────────────────────────────────────────────┐
│                        KUNGU CEMENT DEPLOYMENT                          │
├─────────────────────────────────────────────────────────────────────────┤
│  Infrastructure: Gateway + Service Discovery                            │
│  Master Data: 6 essential services (Core entities only)                │
│  DataManager: 2 services (Weight Data + Transactions)                  │
│  Database: SQLite (Local storage)                                      │
│  Features: Basic weighing, Simple transactions                         │
│  Capacity: Single site, Cost-effective                                 │
└─────────────────────────────────────────────────────────────────────────┘
```

## 🎯 **Key Integration Points**

### **Hardware Integration**
- **Weight Data Service** connects directly to weighbridge sensors
- **Real-time data capture** from industrial weighing equipment
- **Calibration management** through Weighbridge Service

### **ERP Integration**
- **Transaction Service** interfaces with factory management systems
- **Analytics Service** provides business intelligence to ERP
- **Compliance Service** ensures regulatory data flows to corporate systems

### **Multi-site Operations**
- **Data Sync Service** handles distributed site coordination
- **Organization Service** manages site hierarchy and permissions
- **Archive Service** provides centralized compliance data storage

## 🔍 **Service Communication Patterns**

### **Gateway-First Architecture**
```
Client → API Gateway → Service (with User Context)
```

### **Service-to-Service Communication**
```
Service A → API Gateway → Service B (authenticated)
```

### **Data Flow Patterns**
```
Master Data → DataManager → Analytics → Archive
```

This architecture provides a robust, scalable foundation for industrial weighbridge management with clear separation of concerns between identity/entities (WHO/WHAT) and operational processes (HOW/WHEN).

---

*Visual representation of QaliTrack's 19-microservice architecture*  
*Organized by business domain and operational responsibility*