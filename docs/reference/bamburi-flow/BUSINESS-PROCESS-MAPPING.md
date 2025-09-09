# Business Process Mapping: QaliTrack System Design Coverage

## Overview
This document maps Bamburi Cement's business processes to QaliTrack system components, showing how our database design handles each workflow step.

**Process Order**: Admin Setup → Order Creation → Weighing Operations → Quality Control → Analytics

---

## 1. Foundation Data Setup (Level 0-1 Dependencies)

### 1.1 Pure Reference Tables (No Dependencies)

#### Process: Create foundational lookup tables first
**Description**: These tables have no foreign key dependencies and must be created first.

```sql
-- Step 1A: Create Location Types (Pure Reference)
INSERT INTO LOCATION_TYPE (id, name, code, description, is_active) VALUES
  ('f47ac10b-58cc-4372-a567-0e02b2c3d479', 'Plant', 'PLT', 'Manufacturing facility', true),
  ('6ba7b810-9dad-11d1-80b4-00c04fd430c8', 'Warehouse', 'WHR', 'Storage and distribution center', true),
  ('6ba7b811-9dad-11d1-80b4-00c04fd430c8', 'Terminal', 'TRM', 'Loading/offloading terminal', true),
  ('6ba7b812-9dad-11d1-80b4-00c04fd430c8', 'Branch', 'BRC', 'Sales office', true);

-- Step 1A2: Create Zones (Pure Reference)  
INSERT INTO ZONE (id, name, code, description, is_active) VALUES
  ('z1a7b810-9dad-11d1-80b4-00c04fd430c8', 'Coastal Region', 'COAST', 'Mombasa, Mbaraki, BSP Plants, Matuga operations', true),
  ('z2a7b810-9dad-11d1-80b4-00c04fd430c8', 'Eastern Region', 'EAST', 'Nairobi Grinding, Marimbeti, Kitui Road operations', true),
  ('z3a7b810-9dad-11d1-80b4-00c04fd430c8', 'Central Region', 'CENTRAL', 'Central Kenya operations', true);

-- Step 1B: Create Root Product Categories (No Parents)
INSERT INTO PRODUCT_CATEGORY (id, name, code, description, parent_category_id, is_active) VALUES
  ('a47ac10b-58cc-4372-a567-0e02b2c3d479', 'Cement', 'CEM', 'Finished cement products', NULL, true),
  ('b47ac10b-58cc-4372-a567-0e02b2c3d479', 'Clinker', 'CLK', 'Intermediate cement product', NULL, true),
  ('c47ac10b-58cc-4372-a567-0e02b2c3d479', 'Raw Materials', 'RAW', 'Input materials', NULL, true);

-- Step 1C: Create Packaging Types (Pure Reference)
INSERT INTO PACKAGING_TYPE (id, name, code, unit_of_measure, requires_container, handling_equipment, is_active) VALUES
  ('d47ac10b-58cc-4372-a567-0e02b2c3d479', '50kg Bags', 'BAG50', 'bags', false, 'Forklift, Conveyor', true),
  ('e47ac10b-58cc-4372-a567-0e02b2c3d479', '25kg Bags', 'BAG25', 'bags', false, 'Manual, Forklift', true),
  ('f47ac10b-58cc-4372-a567-0e02b2c3d479', 'Bulk', 'BULK', 'tons', false, 'Conveyor, Pneumatic', true);

-- Step 1D: Create Basic Business Entities (No Profiles Yet)
INSERT INTO BUSINESS_ENTITY (id, name, code, entity_type, contact_email, contact_phone, address, is_active) VALUES
  ('be1ac10b-58cc-4372-a567-0e02b2c3d479', 'ABC Construction Ltd', 'ABC-CONST', 'Customer', 'orders@abcconstruction.co.ke', '+254700123456', 'Westlands, Nairobi', true),
  ('be2ac10b-58cc-4372-a567-0e02b2c3d479', 'Kenya Limestone Quarries', 'KLQ-SUPP', 'Supplier', 'supply@klq.co.ke', '+254722987654', 'Voi, Taita Taveta', true),
  ('be3ac10b-58cc-4372-a567-0e02b2c3d479', 'Mombasa Rail Transport', 'MRT-TRANS', 'Transporter', 'dispatch@mrt.co.ke', '+254733456789', 'Mombasa Port', true);
```

**Dependencies**: ✅ **NONE** - These are foundation tables

---

### 1.2 Level 1 Dependencies (Reference Tables with Simple FKs)

#### Process: Create entities that depend only on Level 0 tables
**Description**: Sites, base products, and profiles that need the foundation tables.

```sql
-- Step 2A: Create Sites (Depends on LOCATION_TYPE + ZONE)
INSERT INTO SITE (id, location_type_id, zone_id, name, address, contact_person, is_active) VALUES
  ('site1c10b-58cc-4372-a567-0e02b2c3d479', 'f47ac10b-58cc-4372-a567-0e02b2c3d479', 'z1a7b810-9dad-11d1-80b4-00c04fd430c8', 'Mombasa Plant', 'Bamburi, Mombasa', 'Plant Manager', true),
  ('site2c10b-58cc-4372-a567-0e02b2c3d479', 'f47ac10b-58cc-4372-a567-0e02b2c3d479', 'z2a7b810-9dad-11d1-80b4-00c04fd430c8', 'Nairobi Grinding Station', 'Industrial Area, Nairobi', 'Operations Manager', true);

-- Step 2B: Create Child Product Categories (Depends on Parent Categories)
INSERT INTO PRODUCT_CATEGORY (id, name, code, description, parent_category_id, is_active) VALUES
  ('opc1c10b-58cc-4372-a567-0e02b2c3d479', 'OPC', 'OPC', 'Ordinary Portland Cement', 'a47ac10b-58cc-4372-a567-0e02b2c3d479', true),
  ('ppc1c10b-58cc-4372-a567-0e02b2c3d479', 'PPC', 'PPC', 'Portland Pozzolan Cement', 'a47ac10b-58cc-4372-a567-0e02b2c3d479', true);

-- Step 2C: Create Base Products (Depends on PRODUCT_CATEGORY)
INSERT INTO PRODUCT_BASE (id, name, code, category_id, grade, chemical_formula, standard_density, description, is_active) VALUES
  ('prod1c10b-58cc-4372-a567-0e02b2c3d479', 'Ordinary Portland Cement 42.5N', 'OPC-42-5N', 'opc1c10b-58cc-4372-a567-0e02b2c3d479', '42.5N', 'Ca3SiO5', 1.5, 'Standard OPC cement', true),
  ('prod2c10b-58cc-4372-a567-0e02b2c3d479', 'Clinker Grade A', 'CLK-GRA', 'b47ac10b-58cc-4372-a567-0e02b2c3d479', 'Grade A', 'Ca3SiO5+Ca2SiO4', 1.6, 'High quality clinker', true);

-- Step 2D: Create Customer/Supplier Profiles (Depends on BUSINESS_ENTITY)
INSERT INTO CUSTOMER_PROFILE (id, business_entity_id, credit_limit, payment_terms, preferred_contact_method) VALUES
  ('cp1ac10b-58cc-4372-a567-0e02b2c3d479', 'be1ac10b-58cc-4372-a567-0e02b2c3d479', 500000.00, 'NET30', 'Email');

INSERT INTO SUPPLIER_PROFILE (id, business_entity_id, supplier_type, quality_rating, lead_time_days) VALUES
  ('sp1ac10b-58cc-4372-a567-0e02b2c3d479', 'be2ac10b-58cc-4372-a567-0e02b2c3d479', 'Raw Material', 'A+', 3);

-- Step 2E: Create Vehicles and Drivers (Mostly Standalone)
INSERT INTO VEHICLE (id, number_plate, vehicle_type, make, model, fuel_type, max_weight, max_legal_load, max_safe_load, tare_weight, has_container, container_type, status, is_blocked, blocked_reason) VALUES
  ('veh1c10b-58cc-4372-a567-0e02b2c3d479', 'KCA 123A', 'Truck', 'Isuzu', 'FVZ', 'Diesel', 35000, 35000, 32000, 15000, true, 'Tarpaulin', 'Active', false, NULL);

INSERT INTO DRIVER (id, first_name, last_name, phone_number, email, employee_id, license_number, license_class, license_expiry, status) VALUES
  ('drv1c10b-58cc-4372-a567-0e02b2c3d479', 'John', 'Kamau', '+254701234567', 'j.kamau@transport.co.ke', 'DRV001', 'DL-123456789', 'CDL-A', '2025-12-31', 'Active');
```

**Dependencies**: ✅ **LEVEL 0 ONLY** - All IDs reference tables created in Step 1

---

## 2. Complex Dependencies Setup (Level 2-3)

### 2.1 Level 2 Dependencies (Needs Level 0 + 1)

#### Process: Create entities that need multiple foundation tables
**Description**: Product variants, site capabilities, weighbridges, and driver assignments.

```sql
-- Step 3A: Create Product Variants (Depends on PRODUCT_BASE + PACKAGING_TYPE)
INSERT INTO PRODUCT_VARIANT (id, product_base_id, packaging_type_id, variant_name, variant_code, unit_weight, price_per_unit, is_active) VALUES
  ('pv1ac10b-58cc-4372-a567-0e02b2c3d479', 'prod1c10b-58cc-4372-a567-0e02b2c3d479', 'd47ac10b-58cc-4372-a567-0e02b2c3d479', 'OPC 42.5N Cement 50kg Bags', 'OPC-42-5N-50-BAG', 50.0, 125.00, true),
  ('pv2ac10b-58cc-4372-a567-0e02b2c3d479', 'prod1c10b-58cc-4372-a567-0e02b2c3d479', 'e47ac10b-58cc-4372-a567-0e02b2c3d479', 'OPC 42.5N Cement 25kg Bags', 'OPC-42-5N-25-BAG', 25.0, 85.00, true),
  ('pv3ac10b-58cc-4372-a567-0e02b2c3d479', 'prod2c10b-58cc-4372-a567-0e02b2c3d479', 'f47ac10b-58cc-4372-a567-0e02b2c3d479', 'Clinker Grade A Bulk', 'CLK-GRA-BULK', NULL, 300.00, true);

-- Step 3B: Create Site Capabilities (Depends on SITE + PRODUCT_CATEGORY)
INSERT INTO SITE_CAPABILITY (id, site_id, capability_type, product_category_id, max_capacity_tons, operational_hours, requires_special_equipment, equipment_required, is_active) VALUES
  ('sc1ac10b-58cc-4372-a567-0e02b2c3d479', 'site1c10b-58cc-4372-a567-0e02b2c3d479', 'Production', 'a47ac10b-58cc-4372-a567-0e02b2c3d479', 50000, '24/7', true, 'Kiln, Grinding Mill', true),
  ('sc2ac10b-58cc-4372-a567-0e02b2c3d479', 'site1c10b-58cc-4372-a567-0e02b2c3d479', 'Production', 'b47ac10b-58cc-4372-a567-0e02b2c3d479', 30000, '24/7', true, 'Kiln', true),
  ('sc3ac10b-58cc-4372-a567-0e02b2c3d479', 'site2c10b-58cc-4372-a567-0e02b2c3d479', 'Grinding', 'a47ac10b-58cc-4372-a567-0e02b2c3d479', 25000, '6AM-10PM', true, 'Grinding Mill, Silos', true);

-- Step 3C: Create Weighbridges (Depends on SITE)
INSERT INTO WEIGHBRIDGE (id, site_id, name, code, max_capacity, direction_capability, weighbridge_role, manufacturer, model, is_active) VALUES
  ('wb1ac10b-58cc-4372-a567-0e02b2c3d479', 'site1c10b-58cc-4372-a567-0e02b2c3d479', 'Main Cement Weighbridge', 'MOM-CEM-WB01', 80000, 'Two_Way', 'Both', 'DiniArgeo', 'DFWLKI', true),
  ('wb2ac10b-58cc-4372-a567-0e02b2c3d479', 'site2c10b-58cc-4372-a567-0e02b2c3d479', 'Entry Weighbridge', 'NAI-ENT-WB01', 60000, 'One_Way', 'Entry', 'Data', 'D2008', true);

-- Step 3D: Create Driver Vehicle Assignments (Depends on DRIVER + VEHICLE)
INSERT INTO DRIVER_VEHICLE_ASSIGNMENT (id, driver_id, vehicle_id, assigned_date, is_primary, is_active) VALUES
  ('dva1c10b-58cc-4372-a567-0e02b2c3d479', 'drv1c10b-58cc-4372-a567-0e02b2c3d479', 'veh1c10b-58cc-4372-a567-0e02b2c3d479', '2024-01-01', true, true);
```

**Dependencies**: ✅ **LEVEL 0 + 1** - Uses IDs from previous steps

---

### 2.2 Level 3 Dependencies (Business Rules and Permissions)

#### Process: Create business rules, permissions, and constraints
**Description**: Product permissions, site constraints, and hardware configurations.

```sql
-- Step 4A: Create Product Usage Permissions (Depends on PRODUCT_VARIANT + SITE)
INSERT INTO PRODUCT_USAGE_PERMISSION (id, product_variant_id, usage_type, site_id, is_permitted, effective_from, effective_to) VALUES
  ('pup1c10b-58cc-4372-a567-0e02b2c3d479', 'pv1ac10b-58cc-4372-a567-0e02b2c3d479', 'Sale', 'site1c10b-58cc-4372-a567-0e02b2c3d479', true, '2024-01-01', '2025-12-31'),
  ('pup2c10b-58cc-4372-a567-0e02b2c3d479', 'pv1ac10b-58cc-4372-a567-0e02b2c3d479', 'Sale', 'site2c10b-58cc-4372-a567-0e02b2c3d479', true, '2024-01-01', '2025-12-31'),
  ('pup3c10b-58cc-4372-a567-0e02b2c3d479', 'pv3ac10b-58cc-4372-a567-0e02b2c3d479', 'InterPlant', 'site1c10b-58cc-4372-a567-0e02b2c3d479', true, '2024-01-01', '2025-12-31');

-- Step 4B: Create Site Product Constraints (Depends on SITE + PRODUCT_CATEGORY)  
INSERT INTO SITE_PRODUCT_CONSTRAINT (id, from_site_id, to_site_id, product_category_id, constraint_type, reason, min_quantity, max_quantity, requires_approval, effective_from, effective_to, is_active) VALUES
  ('spc1c10b-58cc-4372-a567-0e02b2c3d479', 'site1c10b-58cc-4372-a567-0e02b2c3d479', 'site2c10b-58cc-4372-a567-0e02b2c3d479', 'b47ac10b-58cc-4372-a567-0e02b2c3d479', 'Allowed', 'Clinker for grinding operations', 500.0, 5000.0, false, '2024-01-01', '2025-12-31', true);

-- Step 4C: Create Product Specifications (Depends on PRODUCT_BASE)
INSERT INTO PRODUCT_SPECIFICATION (id, product_base_id, spec_category, spec_name, spec_unit, min_value, max_value, typical_value, test_method, is_mandatory) VALUES
  ('ps1ac10b-58cc-4372-a567-0e02b2c3d479', 'prod1c10b-58cc-4372-a567-0e02b2c3d479', 'Physical', 'Compressive Strength 28-day', 'MPa', 42.5, 55.0, 48.0, 'ASTM C109', true),
  ('ps2ac10b-58cc-4372-a567-0e02b2c3d479', 'prod1c10b-58cc-4372-a567-0e02b2c3d479', 'Physical', 'Fineness', 'm²/kg', 280, 400, 340, 'IS 4031', true);

-- Step 4D: Create Hardware Configurations (Depends on WEIGHBRIDGE)
INSERT INTO PLC_CONFIGURATION (id, weighbridge_id, plc_type, plc_address, input_count, output_count, configuration_data, is_active) VALUES
  ('plc1c10b-58cc-4372-a567-0e02b2c3d479', 'wb1ac10b-58cc-4372-a567-0e02b2c3d479', 'Siemens Logo 8', '192.168.1.100', 5, 10, '{"traffic_lights": ["Q1","Q2","Q3","Q4"], "boom_barriers": ["Q9","Q10"]}', true);

INSERT INTO ANPR_CAMERA (id, weighbridge_id, camera_name, model, ip_address, position, is_active, status) VALUES
  ('cam1c10b-58cc-4372-a567-0e02b2c3d479', 'wb1ac10b-58cc-4372-a567-0e02b2c3d479', 'Entry Camera A', 'HIKVision iDS-2CD7A46G0-IZHS', '192.168.1.201', 'Entry', true, 'Online');
```

**Dependencies**: ✅ **LEVEL 0-2** - Uses all previously created entities

---

## 3. Operational Data Creation (Level 4-5)

### 3.1 Business Orders (Depends on All Previous Levels)

#### Process: Create orders only after all setup is complete
**Description**: Now we can create customer orders, purchase orders, and transfers with valid references.

```sql
-- Step 5A: Create Schedule Agreements (Depends on SUPPLIER_PROFILE + PRODUCT_VARIANT)
INSERT INTO SCHEDULE_AGREEMENT (id, agreement_number, supplier_id, product_variant_id, valid_from, valid_to, price_per_unit, delivery_terms, is_active) VALUES
  ('sa1ac10b-58cc-4372-a567-0e02b2c3d479', 'HODIM-SA-2024-001', 'sp1ac10b-58cc-4372-a567-0e02b2c3d479', 'pv3ac10b-58cc-4372-a567-0e02b2c3d479', '2024-01-01', '2024-12-31', 300.00, 'FOB Plant', true);

-- Step 5B: Create Customer Orders (Depends on CUSTOMER_PROFILE + SITE + PRODUCT_VARIANT permissions)
INSERT INTO CUSTOMER_ORDER (id, order_number, customer_id, from_site_id, to_site_id, order_date, required_date, status, total_amount, payment_terms, special_instructions, is_visible_at_gate, created_by) VALUES
  ('co1ac10b-58cc-4372-a567-0e02b2c3d479', 'ORD-2024-001234', 'be1ac10b-58cc-4372-a567-0e02b2c3d479', 'site1c10b-58cc-4372-a567-0e02b2c3d479', NULL, '2024-01-15', '2024-01-18', 'Pending', 125000.00, 'NET30', 'Standard delivery', false, 'sales-rep-001');

-- Step 5C: Create Order Lines (Depends on CUSTOMER_ORDER + PRODUCT_VARIANT)
INSERT INTO CUSTOMER_ORDER_LINE (id, customer_order_id, product_variant_id, quantity_ordered, quantity_delivered, unit_price, line_status, special_instructions) VALUES
  ('col1c10b-58cc-4372-a567-0e02b2c3d479', 'co1ac10b-58cc-4372-a567-0e02b2c3d479', 'pv1ac10b-58cc-4372-a567-0e02b2c3d479', 1000, 0, 125.00, 'Pending', 'Load carefully'),
  ('col2c10b-58cc-4372-a567-0e02b2c3d479', 'co1ac10b-58cc-4372-a567-0e02b2c3d479', 'pv2ac10b-58cc-4372-a567-0e02b2c3d479', 500, 0, 85.00, 'Pending', NULL);

-- Now make order visible at gate (after internal approval)
UPDATE CUSTOMER_ORDER 
SET status = 'Confirmed', is_visible_at_gate = true 
WHERE id = 'co1ac10b-58cc-4372-a567-0e02b2c3d479';
```

**Dependencies**: ✅ **ALL PREVIOUS LEVELS** - All referenced entities exist

---

### 3.2 Transaction Processing (Final Level)

#### Process: Process weighing transactions with complete data integrity
**Description**: Create transactions that link to orders, vehicles, weighbridges with all dependencies satisfied.

```sql
-- Step 6A: Create Weighing Transaction (All dependencies satisfied)
INSERT INTO WEIGHING_TRANSACTION (id, transaction_number, site_id, weighbridge_id, vehicle_id, driver_id, customer_order_id, transaction_type, direction, status, transaction_date, is_containerized, seal_number, current_state) VALUES
  ('wt1ac10b-58cc-4372-a567-0e02b2c3d479', 'TXN-2024-001234', 'site1c10b-58cc-4372-a567-0e02b2c3d479', 'wb1ac10b-58cc-4372-a567-0e02b2c3d479', 'veh1c10b-58cc-4372-a567-0e02b2c3d479', 'drv1c10b-58cc-4372-a567-0e02b2c3d479', 'co1ac10b-58cc-4372-a567-0e02b2c3d479', 'Outbound', 'Out', 'WeightEntry', '2024-01-16 08:30:00', false, NULL, 'TareWeighed');

-- Step 6B: Record Weight Measurements
INSERT INTO WEIGHT_MEASUREMENT (id, transaction_id, weighbridge_id, weight, measurement_time, measurement_type, status, temperature, humidity, is_calibrated, is_stable, stability_variance) VALUES
  ('wm1ac10b-58cc-4372-a567-0e02b2c3d479', 'wt1ac10b-58cc-4372-a567-0e02b2c3d479', 'wb1ac10b-58cc-4372-a567-0e02b2c3d479', 15250.5, '2024-01-16 08:35:00', 'Tare', 'Valid', 24.5, 65.2, true, true, 0.5);

-- Step 6C: Create Transaction Lines (Links to PRODUCT_VARIANT from order)
INSERT INTO TRANSACTION_LINE (id, transaction_id, product_variant_id, quantity, actual_unit_weight, batch_number, quality_grade, line_total_weight) VALUES
  ('tl1ac10b-58cc-4372-a567-0e02b2c3d479', 'wt1ac10b-58cc-4372-a567-0e02b2c3d479', 'pv1ac10b-58cc-4372-a567-0e02b2c3d479', 1000, 50.0, 'BATCH-2024-001', 'A', 50000.0),
  ('tl2ac10b-58cc-4372-a567-0e02b2c3d479', 'wt1ac10b-58cc-4372-a567-0e02b2c3d479', 'pv2ac10b-58cc-4372-a567-0e02b2c3d479', 500, 25.0, 'BATCH-2024-001', 'A', 12500.0);

-- Update final weights
UPDATE WEIGHING_TRANSACTION 
SET gross_weight = 77750.5, net_weight = 62500.0, exit_weighing_time = '2024-01-16 10:15:00', status = 'Completed', current_state = 'Completed'
WHERE id = 'wt1ac10b-58cc-4372-a567-0e02b2c3d479';

-- Update order fulfillment
UPDATE CUSTOMER_ORDER_LINE 
SET quantity_delivered = quantity_ordered, line_status = 'Delivered'
WHERE customer_order_id = 'co1ac10b-58cc-4372-a567-0e02b2c3d479';

UPDATE CUSTOMER_ORDER 
SET status = 'Completed' 
WHERE id = 'co1ac10b-58cc-4372-a567-0e02b2c3d479';
```

**Dependencies**: ✅ **COMPLETE** - All entities exist with proper relationships

**QaliTrack Coverage**: 🔶 **ACHIEVABLE** - With proper dependency management, 85%+ coverage possible

---

## 4. Service Architecture: Table Distribution

### 4.1 MasterData Service Tables
**Purpose**: Centralized reference data shared across all operations. These are the foundational entities that define system configuration and business rules.

#### Level 0 - Pure Reference Tables
- `ZONE` - Regional classifications (Coastal, Eastern, Central)
- `LOCATION_TYPE` - Site type classifications (Plant, Warehouse, Terminal, Branch)
- `PACKAGING_TYPE` - Product packaging options (50kg Bags, 25kg Bags, Bulk)
- `PRODUCT_CATEGORY` - Product hierarchies (Cement → OPC/PPC, Clinker, Raw Materials)

#### Level 1 - Business Configuration
- `SITE` - Physical locations with zone and type references
- `PRODUCT_BASE` - Core product definitions with specifications
- `BUSINESS_ENTITY` - Customers, suppliers, transporters master data
- `CUSTOMER_PROFILE` - Customer-specific configurations and credit limits
- `SUPPLIER_PROFILE` - Supplier classifications and quality ratings
- `SCHEDULE_AGREEMENT` - Long-term supply contracts and pricing

#### Level 2 - Operational Configuration
- `PRODUCT_VARIANT` - Product-packaging combinations with pricing
- `SITE_CAPABILITY` - What each site can produce/handle
- `PRODUCT_USAGE_PERMISSION` - Which products can be sold/transferred from each site
- `SITE_PRODUCT_CONSTRAINT` - Business rules for inter-site transfers
- `PRODUCT_SPECIFICATION` - Quality standards and test requirements

#### Level 3 - Hardware Configuration
- `WEIGHBRIDGE` - Physical weighbridge configurations per site
- `PLC_CONFIGURATION` - Siemens Logo 8 control system settings
- `ANPR_CAMERA` - HIKVision camera configurations for plate recognition

**MasterData Total**: 14 tables

### 4.2 DataManager Service Tables  
**Purpose**: Operational data and transactions. These tables handle day-to-day weighing operations and transaction processing.

#### Vehicle & Driver Management
- `VEHICLE` - Truck/transport unit registrations with container capabilities
- `DRIVER` - Driver records with license information
- `DRIVER_VEHICLE_ASSIGNMENT` - Driver-vehicle pairings and scheduling

#### Order Management
- `CUSTOMER_ORDER` - Sales orders from customers
- `CUSTOMER_ORDER_LINE` - Individual line items within orders
- `PURCHASE_ORDER` - Procurement orders to suppliers  
- `PURCHASE_ORDER_LINE` - Line items for purchase orders
- `INTER_PLANT_TRANSFER` - Internal transfers between sites
- `TRANSFER_LINE` - Products being transferred internally

#### Transaction Processing
- `WEIGHING_TRANSACTION` - Core weighing operations linking vehicles, orders, sites
- `WEIGHT_MEASUREMENT` - Individual weight readings from scales
- `TRANSACTION_LINE` - Products and quantities in each transaction

#### Quality & Compliance
- `QUALITY_TEST_RESULT` - Laboratory test results for shipped products
- `SEAL_RECORD` - Container seal tracking for tamper detection
- `VEHICLE_INCIDENT` - Incidents related to seal tampering or violations

**DataManager Total**: 15 tables

### 4.3 Service Communication Pattern

```
MasterData Service (Configuration Hub)
├── Provides reference data to DataManager
├── Manages business rules and permissions
└── Handles product catalogs and site capabilities

DataManager Service (Operations Engine)  
├── Consumes MasterData via APIs/shared DB views
├── Processes weighing transactions
├── Manages order fulfillment
└── Tracks operational incidents and quality
```

**Total System**: 29 tables distributed across 2 microservices
