# QaliTrack Entity Relationship Diagram

## System Overview
This document shows the complete entity model for QaliTrack weighbridge management system, adapted for single organization with multiple sites/plants. 

## Legend
- ✅ **Existing** - Currently implemented in QaliTrack MasterData/DataManager
- 🔶 **Partial** - Exists but needs modification  
- ❌ **Missing** - Needs to be developed
- 🆕 **New** - New entity for Bamburi requirements

---

## Core Master Data Entities

```mermaid
erDiagram
    %% Organization and Sites
    ORGANIZATION {
        uuid id PK "✅ Existing"
        string name "✅ Existing"
        string code "✅ Existing"
        string description "✅ Existing"
        datetime created_at "✅ Existing"
        datetime updated_at "✅ Existing"
    }
    
    ORGANIZATION_LOCATION {
        uuid id PK "✅ Existing (Site/Plant)"
        uuid organization_id FK "✅ Existing"
        string name "✅ Existing - Plant Name"
        string location_type "✅ Existing - Plant, Warehouse, Terminal"
        string address "✅ Existing"
        string contact_person "✅ Existing"
        boolean is_active "✅ Existing"
    }
    
    %% Products and Materials
    PRODUCT {
        uuid id PK "✅ Existing"
        string name "✅ Existing"
        string code "✅ Existing"
        string category "✅ Existing - Cement, Clinker, Concrete"
        string unit_of_measure "✅ Existing"
        decimal unit_weight "🔶 Partial - Add for cement bags/bulk"
        string specifications "✅ Existing"
        boolean is_active "✅ Existing"
    }
    
    %% Business Entities
    BUSINESS_ENTITY {
        uuid id PK "✅ Existing"
        string name "✅ Existing"
        string code "✅ Existing"
        string entity_type "✅ Existing - Customer, Supplier, Transporter"
        string contact_email "✅ Existing"
        string contact_phone "✅ Existing"
        string address "✅ Existing"
        boolean is_active "✅ Existing"
    }
    
    CUSTOMER_PROFILE {
        uuid id PK "✅ Existing"
        uuid business_entity_id FK "✅ Existing"
        decimal credit_limit "✅ Existing"
        string payment_terms "✅ Existing"
        string preferred_contact_method "✅ Existing"
    }
    
    SUPPLIER_PROFILE {
        uuid id PK "✅ Existing"
        uuid business_entity_id FK "✅ Existing"
        string supplier_type "✅ Existing"
        string quality_rating "✅ Existing"
        int lead_time_days "✅ Existing"
    }
    
    %% Fleet Management
    VEHICLE {
        uuid id PK "✅ Existing"
        string registration_number "✅ Existing"
        string make "✅ Existing"
        string model "✅ Existing"
        string fuel_type "✅ Existing"
        decimal max_weight "✅ Existing"
        decimal tare_weight "✅ Existing"
        string status "✅ Existing"
    }
    
    DRIVER {
        uuid id PK "✅ Existing"
        string first_name "✅ Existing"
        string last_name "✅ Existing"
        string phone_number "✅ Existing"
        string email "✅ Existing"
        string employee_id "✅ Existing"
        string status "✅ Existing"
    }
    
    DRIVER_VEHICLE_ASSIGNMENT {
        uuid id PK "✅ Existing (Relationships)"
        uuid driver_id FK "✅ Existing"
        uuid vehicle_id FK "✅ Existing"
        datetime assigned_date "✅ Existing"
        boolean is_primary "✅ Existing"
        boolean is_active "✅ Existing"
    }
    
    %% Weighbridge Infrastructure
    WEIGHBRIDGE {
        uuid id PK "✅ Existing"
        uuid site_id FK "🔶 Partial - Link to OrganizationLocation"
        string name "✅ Existing"
        string code "✅ Existing"
        decimal max_capacity "✅ Existing"
        string direction_capability "🆕 New - OneWay, TwoWay"
        string weighbridge_role "🆕 New - Entry, Exit, Both"
        string manufacturer "✅ Existing"
        string model "✅ Existing"
        boolean is_active "✅ Existing"
    }

    %% Relationships
    ORGANIZATION ||--o{ ORGANIZATION_LOCATION : "has sites"
    ORGANIZATION_LOCATION ||--o{ WEIGHBRIDGE : "contains"
    BUSINESS_ENTITY ||--o| CUSTOMER_PROFILE : "extends"
    BUSINESS_ENTITY ||--o| SUPPLIER_PROFILE : "extends"
    DRIVER ||--o{ DRIVER_VEHICLE_ASSIGNMENT : "assigned to"
    VEHICLE ||--o{ DRIVER_VEHICLE_ASSIGNMENT : "assigned from"
```

---

## Order Management System (🆕 NEW - Critical Missing Component)

```mermaid
erDiagram
    %% Customer Orders (Outbound)
    CUSTOMER_ORDER {
        uuid id PK "❌ Missing - CRITICAL"
        string order_number "❌ Missing - Unique identifier"
        uuid customer_id FK "❌ Missing - Link to BusinessEntity"
        uuid from_site_id FK "❌ Missing - Source plant/site"
        uuid to_site_id FK "❌ Missing - Delivery destination"
        datetime order_date "❌ Missing"
        datetime required_date "❌ Missing"
        string status "❌ Missing - Pending, Confirmed, InProgress, Completed"
        decimal total_amount "❌ Missing"
        string payment_terms "❌ Missing"
        string special_instructions "❌ Missing"
        boolean is_visible_at_gate "❌ Missing - For CSC confirmation"
        string created_by "❌ Missing"
    }
    
    CUSTOMER_ORDER_LINE {
        uuid id PK "❌ Missing - CRITICAL"
        uuid customer_order_id FK "❌ Missing"
        uuid product_id FK "❌ Missing"
        decimal quantity_ordered "❌ Missing"
        decimal quantity_delivered "❌ Missing - Track fulfillment"
        decimal unit_price "❌ Missing"
        string unit_of_measure "❌ Missing"
        string line_status "❌ Missing - Pending, Loaded, Delivered"
    }
    
    %% Purchase Orders / Schedule Agreements (Inbound)
    PURCHASE_ORDER {
        uuid id PK "❌ Missing - CRITICAL"
        string po_number "❌ Missing - References HODIM"
        uuid supplier_id FK "❌ Missing - Link to BusinessEntity"
        uuid to_site_id FK "❌ Missing - Destination plant/site"
        datetime order_date "❌ Missing"
        datetime expected_delivery_date "❌ Missing"
        string status "❌ Missing - Open, InTransit, Received, Closed"
        decimal total_amount "❌ Missing"
        string terms_conditions "❌ Missing"
        string hodim_reference "❌ Missing - External system link"
    }
    
    PURCHASE_ORDER_LINE {
        uuid id PK "❌ Missing - CRITICAL"
        uuid purchase_order_id FK "❌ Missing"
        uuid product_id FK "❌ Missing"
        decimal quantity_ordered "❌ Missing"
        decimal quantity_received "❌ Missing"
        decimal unit_price "❌ Missing"
        string quality_specifications "❌ Missing - For moisture testing"
    }
    
    %% Schedule Agreements (HODIM Integration)
    SCHEDULE_AGREEMENT {
        uuid id PK "❌ Missing - CRITICAL for HODIM"
        string agreement_number "❌ Missing - HODIM reference"
        uuid supplier_id FK "❌ Missing"
        uuid product_id FK "❌ Missing"
        datetime valid_from "❌ Missing"
        datetime valid_to "❌ Missing"
        decimal price_per_unit "❌ Missing"
        string delivery_terms "❌ Missing"
        boolean is_active "❌ Missing"
    }

    %% Delivery Notes (Physical Documents)
    DELIVERY_NOTE {
        uuid id PK "❌ Missing - CRITICAL"
        string delivery_note_number "❌ Missing - Physical document ref"
        uuid purchase_order_id FK "❌ Missing - Optional for inbound"
        uuid customer_order_id FK "❌ Missing - Optional for outbound"
        uuid supplier_id FK "❌ Missing - For inbound"
        uuid customer_id FK "❌ Missing - For outbound"
        uuid vehicle_id FK "❌ Missing"
        uuid driver_id FK "❌ Missing"
        datetime issue_date "❌ Missing"
        string delivery_type "❌ Missing - Inbound, Outbound"
        string status "❌ Missing - Created, InTransit, Delivered"
        string qr_code "❌ Missing - For automation"
        string notes "❌ Missing"
    }
    
    DELIVERY_NOTE_LINE {
        uuid id PK "❌ Missing - CRITICAL"
        uuid delivery_note_id FK "❌ Missing"
        uuid product_id FK "❌ Missing"
        decimal quantity "❌ Missing"
        string unit_of_measure "❌ Missing"
        string batch_number "❌ Missing - For traceability"
        string quality_certificate "❌ Missing - For compliance"
    }

    %% Relationships
    CUSTOMER_ORDER ||--o{ CUSTOMER_ORDER_LINE : "contains"
    PURCHASE_ORDER ||--o{ PURCHASE_ORDER_LINE : "contains"
    DELIVERY_NOTE ||--o{ DELIVERY_NOTE_LINE : "contains"
    BUSINESS_ENTITY ||--o{ CUSTOMER_ORDER : "places order"
    BUSINESS_ENTITY ||--o{ PURCHASE_ORDER : "supplies to"
    PRODUCT ||--o{ CUSTOMER_ORDER_LINE : "ordered"
    PRODUCT ||--o{ PURCHASE_ORDER_LINE : "ordered"
    PRODUCT ||--o{ DELIVERY_NOTE_LINE : "delivered"
    ORGANIZATION_LOCATION ||--o{ CUSTOMER_ORDER : "ships from"
    ORGANIZATION_LOCATION ||--o{ PURCHASE_ORDER : "receives at"
```

---

## Transaction Processing System

```mermaid
erDiagram
    %% Core Transaction Entity (🔶 NEEDS MODIFICATION)
    WEIGHING_TRANSACTION {
        uuid id PK "✅ Existing"
        string transaction_number "✅ Existing"
        uuid site_id FK "🔶 Modify - Add site reference"
        uuid weighbridge_id FK "✅ Existing"
        uuid vehicle_id FK "✅ Existing"
        uuid driver_id FK "✅ Existing"
        uuid customer_order_id FK "❌ Missing - CRITICAL link"
        uuid purchase_order_id FK "❌ Missing - CRITICAL link"
        uuid delivery_note_id FK "❌ Missing - CRITICAL link"
        string transaction_type "✅ Existing - Purchase, Sale, Transfer"
        string direction "🆕 New - Inbound, Outbound, Transfer"
        string status "✅ Existing"
        datetime transaction_date "✅ Existing"
        decimal gross_weight "✅ Existing"
        decimal tare_weight "✅ Existing"
        decimal net_weight "✅ Existing"
        datetime entry_weighing_time "✅ Existing"
        datetime exit_weighing_time "✅ Existing"
        string current_state "✅ Existing"
    }
    
    %% Transaction Lines (🆕 NEW - For Multi-Product Transactions)
    TRANSACTION_LINE {
        uuid id PK "❌ Missing - For mixed loads"
        uuid transaction_id FK "❌ Missing"
        uuid product_id FK "❌ Missing"
        decimal quantity "❌ Missing"
        string unit_of_measure "❌ Missing"
        decimal unit_weight "❌ Missing"
        string batch_number "❌ Missing"
        string quality_grade "❌ Missing"
    }
    
    %% Weight Measurements (✅ EXISTS BUT ENHANCE)
    WEIGHT_MEASUREMENT {
        uuid id PK "✅ Existing"
        uuid transaction_id FK "✅ Existing"
        uuid weighbridge_id FK "✅ Existing"
        decimal weight "✅ Existing"
        datetime measurement_time "✅ Existing"
        string measurement_type "✅ Existing - Entry, Exit"
        string status "✅ Existing"
        decimal temperature "✅ Existing"
        decimal humidity "✅ Existing"
        boolean is_calibrated "✅ Existing"
        boolean is_stable "🔶 Add - For automation"
        decimal stability_variance "🔶 Add - For automation"
    }
    
    %% Inter-Plant Transfers (🔶 ENHANCE EXISTING)
    INTER_PLANT_TRANSFER {
        uuid id PK "❌ Missing - CRITICAL"
        uuid source_transaction_id FK "❌ Missing - Origin transaction"
        uuid target_transaction_id FK "❌ Missing - Destination transaction" 
        uuid from_site_id FK "❌ Missing"
        uuid to_site_id FK "❌ Missing"
        string transfer_number "❌ Missing - QR Code reference"
        string status "❌ Missing - Initiated, InTransit, Received, Completed"
        datetime dispatch_time "❌ Missing"
        datetime arrival_time "❌ Missing"
        string qr_code "❌ Missing - LOGON number"
        decimal quantity_sent "❌ Missing"
        decimal quantity_received "❌ Missing"
        string transfer_notes "❌ Missing"
    }

    %% Relationships
    WEIGHING_TRANSACTION ||--o{ WEIGHT_MEASUREMENT : "has measurements"
    WEIGHING_TRANSACTION ||--o{ TRANSACTION_LINE : "contains products"
    CUSTOMER_ORDER ||--o{ WEIGHING_TRANSACTION : "fulfilled by"
    PURCHASE_ORDER ||--o{ WEIGHING_TRANSACTION : "received through"
    DELIVERY_NOTE ||--o{ WEIGHING_TRANSACTION : "documented by"
    PRODUCT ||--o{ TRANSACTION_LINE : "included in"
    ORGANIZATION_LOCATION ||--o{ WEIGHING_TRANSACTION : "processed at"
    INTER_PLANT_TRANSFER ||--|| WEIGHING_TRANSACTION : "originates from"
    INTER_PLANT_TRANSFER ||--|| WEIGHING_TRANSACTION : "completes with"
```

---

## Quality Control and Compliance (🔶 ENHANCE EXISTING)

```mermaid
erDiagram
    %% Quality Control (🆕 NEW - Critical for Inbound)
    QUALITY_TEST {
        uuid id PK "❌ Missing - CRITICAL for inbound"
        uuid transaction_id FK "❌ Missing"
        uuid product_id FK "❌ Missing"
        string test_type "❌ Missing - Moisture, Strength, Chemical"
        datetime test_time "❌ Missing"
        string tested_by "❌ Missing - Lab Technician ID"
        decimal test_value "❌ Missing"
        decimal acceptable_min "❌ Missing"
        decimal acceptable_max "❌ Missing"
        string test_result "❌ Missing - Pass, Fail, Warning"
        string test_method "❌ Missing"
        string certificate_number "❌ Missing"
        string notes "❌ Missing"
    }
    
    %% Enhanced Compliance (🔶 MODIFY EXISTING)
    COMPLIANCE_CHECK {
        uuid id PK "✅ Existing"
        uuid transaction_id FK "✅ Existing"
        uuid quality_test_id FK "❌ Missing - Link to quality results"
        string compliance_type "✅ Existing"
        string status "✅ Existing"
        string check_result "✅ Existing"
        datetime check_date "✅ Existing"
        string regulatory_reference "✅ Existing"
        boolean auto_generated "🔶 Add - System vs Manual"
    }

    %% Relationships
    WEIGHING_TRANSACTION ||--o{ QUALITY_TEST : "tested for quality"
    QUALITY_TEST ||--o{ COMPLIANCE_CHECK : "generates compliance check"
    PRODUCT ||--o{ QUALITY_TEST : "has quality standards"
```

---

## Automation and Hardware Integration (❌ MISSING - CRITICAL)

```mermaid
erDiagram
    %% Hardware Controllers
    WEIGHBRIDGE_CONTROLLER {
        uuid id PK "❌ Missing - Hardware integration"
        uuid weighbridge_id FK "❌ Missing"
        string controller_type "❌ Missing - PLC, Indicator, PC"
        string ip_address "❌ Missing"
        int port "❌ Missing"
        string communication_protocol "❌ Missing - Modbus, TCP/IP"
        string status "❌ Missing - Online, Offline, Error"
        datetime last_heartbeat "❌ Missing"
        string firmware_version "❌ Missing"
    }
    
    %% PLC Integration (❌ MISSING - CRITICAL for Automation)
    PLC_CONFIGURATION {
        uuid id PK "❌ Missing - Siemens Logo 8"
        uuid weighbridge_id FK "❌ Missing"
        string plc_type "❌ Missing - Siemens_Logo8"
        string plc_address "❌ Missing"
        int input_count "❌ Missing - 5 inputs for sensors"
        int output_count "❌ Missing - 10 outputs for controls"
        string configuration_data "❌ Missing - JSON config"
        boolean is_active "❌ Missing"
    }
    
    PLC_IO_POINT {
        uuid id PK "❌ Missing - Individual I/O points"
        uuid plc_configuration_id FK "❌ Missing"
        string io_type "❌ Missing - Input, Output"
        string io_name "❌ Missing - VehicleDetectorA, BoomBarrierB"
        int io_address "❌ Missing - Q1, Q2, i1, i2"
        string device_type "❌ Missing - TrafficLight, BoomBarrier, VehicleDetector"
        string status "❌ Missing - OK, Error"
        datetime last_tested "❌ Missing"
    }
    
    %% ANPR Cameras (❌ MISSING)
    ANPR_CAMERA {
        uuid id PK "❌ Missing - License plate recognition"
        uuid weighbridge_id FK "❌ Missing"
        string camera_name "❌ Missing - Camera A, Camera B"
        string model "❌ Missing - HIKVision iDS-2CD7A46G0-IZHS"
        string ip_address "❌ Missing"
        string position "❌ Missing - Entry, Exit"
        boolean is_active "❌ Missing"
        string status "❌ Missing"
    }
    
    VEHICLE_DETECTION_EVENT {
        uuid id PK "❌ Missing - RFID/Camera events"
        uuid weighbridge_id FK "❌ Missing"
        uuid transaction_id FK "❌ Missing"
        string detection_method "❌ Missing - RFID, ANPR, Manual"
        string vehicle_registration "❌ Missing - From ANPR"
        datetime detection_time "❌ Missing"
        string detection_status "❌ Missing - Confirmed, Mismatch"
        string raw_data "❌ Missing - Image path, RFID data"
    }

    %% Relationships
    WEIGHBRIDGE ||--|| WEIGHBRIDGE_CONTROLLER : "controlled by"
    WEIGHBRIDGE ||--o| PLC_CONFIGURATION : "automated by"
    WEIGHBRIDGE ||--o{ ANPR_CAMERA : "monitored by"
    PLC_CONFIGURATION ||--o{ PLC_IO_POINT : "contains"
    WEIGHING_TRANSACTION ||--o{ VEHICLE_DETECTION_EVENT : "detected during"
```

---

## System Integration Points

```mermaid
erDiagram
    %% External System Integration (❌ MISSING - CRITICAL)
    EXTERNAL_SYSTEM {
        uuid id PK "❌ Missing - ERP integration"
        string system_name "❌ Missing - SAP, HODIM"
        string system_type "❌ Missing - ERP, WMS, CRM"
        string api_endpoint "❌ Missing"
        string authentication_method "❌ Missing"
        boolean is_active "❌ Missing"
        datetime last_sync "❌ Missing"
    }
    
    INTEGRATION_LOG {
        uuid id PK "❌ Missing - Sync tracking"
        uuid external_system_id FK "❌ Missing"
        string entity_type "❌ Missing - Order, Product, Customer"
        uuid entity_id FK "❌ Missing"
        string operation "❌ Missing - Create, Update, Delete, Sync"
        string status "❌ Missing - Success, Failed, Pending"
        datetime operation_time "❌ Missing"
        string error_message "❌ Missing"
        string request_data "❌ Missing"
        string response_data "❌ Missing"
    }

    %% Support System (🔶 ENHANCE EXISTING)
    SUPPORT_TICKET {
        uuid id PK "❌ Missing - For order issues"
        uuid customer_order_id FK "❌ Missing"
        uuid reported_by FK "❌ Missing - User/Driver"
        string ticket_type "❌ Missing - OrderNotFound, SystemIssue"
        string priority "❌ Missing - High, Medium, Low"
        string status "❌ Missing - Open, InProgress, Resolved"
        string description "❌ Missing"
        datetime created_at "❌ Missing"
        datetime resolved_at "❌ Missing"
        string resolution_notes "❌ Missing"
    }

    %% Relationships
    CUSTOMER_ORDER ||--o{ SUPPORT_TICKET : "may generate"
    EXTERNAL_SYSTEM ||--o{ INTEGRATION_LOG : "logs operations"
```

---

## Summary: Implementation Priority

### 🔴 CRITICAL (Phase 1 - Must Have)
1. **Order Management System** - Customer orders, Purchase orders, Delivery notes
2. **Transaction-Order Linking** - Link transactions to orders/deliveries  
3. **Site-Based Processing** - Remove multi-tenancy, add site references
4. **Product Tracking** - Transaction lines for multi-product support

### 🟡 HIGH (Phase 2 - Important)
5. **Quality Control Integration** - Moisture testing, lab workflows
6. **Inter-Plant Transfer Enhancement** - QR codes, stock-in-transit tracking
7. **Hardware Integration Framework** - PLC, ANPR, RFID support
8. **HODIM Integration** - Schedule agreements, delivery note matching

### 🟢 MEDIUM (Phase 3 - Enhancement)  
9. **Full Automation** - Complete PLC/sensor integration
10. **ERP Integration** - SAP S4/HANA connectivity
11. **Support System** - Ticket management, customer service integration
12. **Advanced Analytics** - TTAT tracking, performance optimization