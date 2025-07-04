# Transaction Service - Data Models

## Overview
The Transaction Service manages complete weighing transactions, linking vehicles, drivers, suppliers, customers, transporters, products, and routes in a comprehensive business workflow.

## SQLite Schema (Development)

### Core Tables

#### WeighingTransactions
```sql
CREATE TABLE weighing_transactions (
    id TEXT PRIMARY KEY, -- UUID
    transaction_number TEXT UNIQUE NOT NULL, -- Business identifier
    organization_id TEXT NOT NULL,
    
    -- Entities
    vehicle_id TEXT NOT NULL,
    driver_id TEXT NOT NULL,
    supplier_id TEXT,
    customer_id TEXT,
    transporter_id TEXT,
    product_id TEXT,
    route_id TEXT,
    weighbridge_id TEXT NOT NULL,
    
    -- Transaction Details
    transaction_type TEXT NOT NULL CHECK (transaction_type IN ('inbound', 'outbound', 'internal')),
    transaction_status TEXT DEFAULT 'initiated' CHECK (transaction_status IN ('initiated', 'in_progress', 'completed', 'cancelled', 'disputed')),
    
    -- Weight Information
    entry_weight_id INTEGER, -- FK to weight_measurements
    exit_weight_id INTEGER,
    gross_weight DECIMAL(10,2),
    tare_weight DECIMAL(10,2),
    net_weight DECIMAL(10,2),
    weight_unit TEXT DEFAULT 'kg',
    
    -- Product Information
    product_quantity DECIMAL(10,3),
    quantity_unit TEXT,
    product_price DECIMAL(10,2),
    currency TEXT DEFAULT 'KES',
    total_value DECIMAL(12,2),
    
    -- Timing
    entry_time DATETIME,
    exit_time DATETIME,
    duration_minutes INTEGER,
    
    -- Documentation
    reference_number TEXT, -- Customer/supplier reference
    delivery_note_number TEXT,
    invoice_number TEXT,
    comments TEXT,
    
    -- Audit Fields
    created_by TEXT NOT NULL,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    updated_by TEXT,
    updated_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    completed_at DATETIME,
    
    -- Foreign Key References
    FOREIGN KEY (vehicle_id) REFERENCES vehicles(id),
    FOREIGN KEY (driver_id) REFERENCES drivers(id),
    FOREIGN KEY (supplier_id) REFERENCES suppliers(id),
    FOREIGN KEY (customer_id) REFERENCES customers(id),
    FOREIGN KEY (transporter_id) REFERENCES transporters(id),
    FOREIGN KEY (product_id) REFERENCES products(id),
    FOREIGN KEY (route_id) REFERENCES routes(id),
    FOREIGN KEY (weighbridge_id) REFERENCES weighbridges(id),
    FOREIGN KEY (organization_id) REFERENCES organizations(id),
    FOREIGN KEY (entry_weight_id) REFERENCES weight_measurements(id),
    FOREIGN KEY (exit_weight_id) REFERENCES weight_measurements(id)
);
```

#### TransactionWorkflow
```sql
CREATE TABLE transaction_workflow (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    transaction_id TEXT NOT NULL,
    
    -- Workflow State
    workflow_step TEXT NOT NULL CHECK (workflow_step IN (
        'vehicle_arrival', 'document_check', 'entry_weighing', 
        'loading_unloading', 'exit_weighing', 'payment_processing', 
        'completion', 'dispute_resolution'
    )),
    step_status TEXT NOT NULL CHECK (step_status IN ('pending', 'in_progress', 'completed', 'skipped', 'failed')),
    
    -- Timing
    step_started_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    step_completed_at DATETIME,
    step_duration_minutes INTEGER,
    
    -- Step Details
    step_data TEXT, -- JSON data specific to each step
    performed_by TEXT,
    comments TEXT,
    
    -- Validation
    validation_passed BOOLEAN,
    validation_errors TEXT, -- JSON array of validation issues
    
    FOREIGN KEY (transaction_id) REFERENCES weighing_transactions(id)
);
```

#### TransactionCharges
```sql
CREATE TABLE transaction_charges (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    transaction_id TEXT NOT NULL,
    
    -- Charge Details
    charge_type TEXT NOT NULL CHECK (charge_type IN (
        'weighing_fee', 'storage_fee', 'overtime_fee', 'penalty_fee', 
        'handling_fee', 'documentation_fee', 'service_tax'
    )),
    charge_description TEXT,
    
    -- Pricing
    base_amount DECIMAL(10,2) NOT NULL,
    quantity DECIMAL(10,3) DEFAULT 1,
    unit_price DECIMAL(10,2),
    tax_rate DECIMAL(5,2) DEFAULT 0,
    tax_amount DECIMAL(10,2),
    total_amount DECIMAL(10,2) NOT NULL,
    currency TEXT DEFAULT 'KES',
    
    -- Status
    charge_status TEXT DEFAULT 'pending' CHECK (charge_status IN ('pending', 'paid', 'waived', 'disputed')),
    
    -- Audit
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    paid_at DATETIME,
    paid_by TEXT,
    
    FOREIGN KEY (transaction_id) REFERENCES weighing_transactions(id)
);
```

#### TransactionDocuments
```sql
CREATE TABLE transaction_documents (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    transaction_id TEXT NOT NULL,
    
    -- Document Details
    document_type TEXT NOT NULL CHECK (document_type IN (
        'weight_ticket', 'delivery_note', 'invoice', 'receipt', 
        'inspection_certificate', 'quality_report', 'photo', 'signature'
    )),
    document_name TEXT NOT NULL,
    document_path TEXT, -- File storage path
    document_url TEXT,  -- CDN/blob storage URL
    
    -- Metadata
    file_size INTEGER,
    file_type TEXT,
    mime_type TEXT,
    
    -- Status
    document_status TEXT DEFAULT 'active' CHECK (document_status IN ('active', 'archived', 'deleted')),
    
    -- Audit
    uploaded_by TEXT NOT NULL,
    uploaded_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    
    FOREIGN KEY (transaction_id) REFERENCES weighing_transactions(id)
);
```

### Indexes for Performance
```sql
-- Transaction queries
CREATE INDEX idx_transactions_status ON weighing_transactions(transaction_status, created_at);
CREATE INDEX idx_transactions_vehicle ON weighing_transactions(vehicle_id, created_at);
CREATE INDEX idx_transactions_weighbridge ON weighing_transactions(weighbridge_id, created_at);
CREATE INDEX idx_transactions_organization ON weighing_transactions(organization_id, created_at);
CREATE INDEX idx_transactions_number ON weighing_transactions(transaction_number);
CREATE INDEX idx_transactions_reference ON weighing_transactions(reference_number);

-- Workflow tracking
CREATE INDEX idx_workflow_transaction ON transaction_workflow(transaction_id, workflow_step);
CREATE INDEX idx_workflow_status ON transaction_workflow(step_status, step_started_at);

-- Financial queries
CREATE INDEX idx_charges_transaction ON transaction_charges(transaction_id, charge_type);
CREATE INDEX idx_charges_status ON transaction_charges(charge_status, created_at);

-- Document management
CREATE INDEX idx_documents_transaction ON transaction_documents(transaction_id, document_type);
CREATE INDEX idx_documents_status ON transaction_documents(document_status, uploaded_at);
```

## Entity Relationships

### Weighing Transaction Entity
```csharp
public class WeighingTransaction
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string TransactionNumber { get; set; }
    public string OrganizationId { get; set; }
    
    // Entity References
    public string VehicleId { get; set; }
    public string DriverId { get; set; }
    public string SupplierId { get; set; }
    public string CustomerId { get; set; }
    public string TransporterId { get; set; }
    public string ProductId { get; set; }
    public string RouteId { get; set; }
    public string WeighbridgeId { get; set; }
    
    // Transaction Details
    public TransactionType TransactionType { get; set; }
    public TransactionStatus TransactionStatus { get; set; } = TransactionStatus.Initiated;
    
    // Weight Information
    public int? EntryWeightId { get; set; }
    public int? ExitWeightId { get; set; }
    public decimal? GrossWeight { get; set; }
    public decimal? TareWeight { get; set; }
    public decimal? NetWeight { get; set; }
    public string WeightUnit { get; set; } = "kg";
    
    // Product Information
    public decimal? ProductQuantity { get; set; }
    public string QuantityUnit { get; set; }
    public decimal? ProductPrice { get; set; }
    public string Currency { get; set; } = "KES";
    public decimal? TotalValue { get; set; }
    
    // Timing
    public DateTime? EntryTime { get; set; }
    public DateTime? ExitTime { get; set; }
    public int? DurationMinutes { get; set; }
    
    // Documentation
    public string ReferenceNumber { get; set; }
    public string DeliveryNoteNumber { get; set; }
    public string InvoiceNumber { get; set; }
    public string Comments { get; set; }
    
    // Audit Fields
    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string UpdatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    
    // Navigation Properties
    public virtual ICollection<TransactionWorkflow> WorkflowSteps { get; set; } = new List<TransactionWorkflow>();
    public virtual ICollection<TransactionCharge> Charges { get; set; } = new List<TransactionCharge>();
    public virtual ICollection<TransactionDocument> Documents { get; set; } = new List<TransactionDocument>();
}
```

### Transaction Workflow Entity
```csharp
public class TransactionWorkflow
{
    public int Id { get; set; }
    public string TransactionId { get; set; }
    
    // Workflow State
    public WorkflowStep WorkflowStep { get; set; }
    public StepStatus StepStatus { get; set; } = StepStatus.Pending;
    
    // Timing
    public DateTime StepStartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? StepCompletedAt { get; set; }
    public int? StepDurationMinutes { get; set; }
    
    // Step Details
    public string StepData { get; set; } // JSON
    public string PerformedBy { get; set; }
    public string Comments { get; set; }
    
    // Validation
    public bool? ValidationPassed { get; set; }
    public string ValidationErrors { get; set; } // JSON
    
    // Navigation Properties
    public virtual WeighingTransaction Transaction { get; set; }
}
```

## Enums

```csharp
public enum TransactionType
{
    Inbound,    // Material/product coming in
    Outbound,   // Material/product going out
    Internal    // Internal movement within facility
}

public enum TransactionStatus
{
    Initiated,     // Transaction created
    InProgress,    // Actively being processed
    Completed,     // Successfully completed
    Cancelled,     // Cancelled before completion
    Disputed       // Under dispute resolution
}

public enum WorkflowStep
{
    VehicleArrival,      // Vehicle arrives at facility
    DocumentCheck,       // Check delivery notes, permits
    EntryWeighing,       // First weight measurement
    LoadingUnloading,    // Product loading/unloading
    ExitWeighing,        // Second weight measurement
    PaymentProcessing,   // Process charges and payments
    Completion,          // Finalize transaction
    DisputeResolution    // Handle disputes
}

public enum StepStatus
{
    Pending,     // Step not yet started
    InProgress,  // Step currently active
    Completed,   // Step finished successfully
    Skipped,     // Step bypassed
    Failed       // Step failed validation
}

public enum ChargeType
{
    WeighingFee,       // Basic weighing service fee
    StorageFee,        // Storage charges for delays
    OvertimeFee,       // After-hours service charges
    PenaltyFee,        // Overweight or violation penalties
    HandlingFee,       // Special handling charges
    DocumentationFee,  // Document processing fees
    ServiceTax         // Government taxes
}
```

## Business Rules and Validation

### Transaction Creation Rules
- **Vehicle Status**: Must be active and not currently in another transaction
- **Driver License**: Must be valid and not expired
- **Weighbridge**: Must be operational and available
- **Organization**: User must have access to specified organization
- **Product**: Must be active if specified
- **Route**: Must be valid if specified

### Workflow Progression Rules
- **Sequential Steps**: Steps must follow defined order
- **Completion Requirements**: Each step must meet completion criteria
- **Validation Gates**: Certain steps require validation before proceeding
- **Skip Conditions**: Some steps can be skipped based on transaction type

### Weight Calculation Rules
- **Entry/Exit Pair**: Net weight = Gross weight (entry) - Gross weight (exit)
- **Single Weighing**: Net weight = Gross weight - Known tare weight
- **Weight Validation**: Net weight must be positive and reasonable
- **Unit Consistency**: All weights must use same unit system

### Charge Calculation Rules
- **Base Charges**: Apply standard weighing fees
- **Time-based Charges**: Storage fees for extended duration
- **Weight-based Penalties**: Overweight charges
- **Tax Calculation**: Apply applicable tax rates
- **Discount Application**: Apply organization-specific discounts

## Data Integrity Constraints

### Foreign Key Constraints
```sql
-- Ensure weight measurements exist
ALTER TABLE weighing_transactions 
ADD CONSTRAINT fk_entry_weight 
FOREIGN KEY (entry_weight_id) REFERENCES weight_measurements(id);

-- Ensure workflow belongs to transaction
ALTER TABLE transaction_workflow 
ADD CONSTRAINT fk_workflow_transaction 
FOREIGN KEY (transaction_id) REFERENCES weighing_transactions(id) 
ON DELETE CASCADE;
```

### Check Constraints
```sql
-- Ensure positive weights
ALTER TABLE weighing_transactions 
ADD CONSTRAINT chk_positive_weights 
CHECK (gross_weight > 0 AND (tare_weight IS NULL OR tare_weight > 0) 
       AND (net_weight IS NULL OR net_weight > 0));

-- Ensure logical weight relationships
ALTER TABLE weighing_transactions 
ADD CONSTRAINT chk_weight_logic 
CHECK (tare_weight IS NULL OR gross_weight > tare_weight);

-- Ensure positive charges
ALTER TABLE transaction_charges 
ADD CONSTRAINT chk_positive_charges 
CHECK (base_amount >= 0 AND total_amount >= 0);
```

### Triggers for Data Consistency
```sql
-- Auto-update transaction status based on workflow
CREATE TRIGGER update_transaction_status 
AFTER UPDATE ON transaction_workflow
WHEN NEW.workflow_step = 'completion' AND NEW.step_status = 'completed'
BEGIN
    UPDATE weighing_transactions 
    SET transaction_status = 'completed', 
        completed_at = CURRENT_TIMESTAMP,
        updated_at = CURRENT_TIMESTAMP
    WHERE id = NEW.transaction_id;
END;

-- Calculate net weight when both entry and exit weights available
CREATE TRIGGER calculate_net_weight 
AFTER UPDATE ON weighing_transactions
WHEN NEW.entry_weight_id IS NOT NULL AND NEW.exit_weight_id IS NOT NULL
BEGIN
    UPDATE weighing_transactions 
    SET net_weight = (
        SELECT entry.gross_weight - exit.gross_weight 
        FROM weight_measurements entry, weight_measurements exit
        WHERE entry.id = NEW.entry_weight_id AND exit.id = NEW.exit_weight_id
    ),
    updated_at = CURRENT_TIMESTAMP
    WHERE id = NEW.id;
END;
```

## Production Migration Considerations

### PostgreSQL Enhancements
```sql
-- Partitioning by date and organization
CREATE TABLE weighing_transactions (
    -- Same schema
    created_at TIMESTAMP NOT NULL,
    organization_id TEXT NOT NULL
) PARTITION BY RANGE (created_at);

-- Create monthly partitions
CREATE TABLE weighing_transactions_y2024m01 PARTITION OF weighing_transactions
    FOR VALUES FROM ('2024-01-01') TO ('2024-02-01');

-- Organization-based sub-partitioning
CREATE TABLE weighing_transactions_y2024m01_org1 PARTITION OF weighing_transactions_y2024m01
    FOR VALUES IN ('org-001');
```

### JSON Data Storage
```sql
-- Store complex step data as JSONB in PostgreSQL
ALTER TABLE transaction_workflow 
ALTER COLUMN step_data TYPE JSONB;

-- Create GIN index for JSON queries
CREATE INDEX idx_workflow_step_data_gin ON transaction_workflow 
USING GIN (step_data);

-- Example JSON data structure
-- Document check step data:
{
  "documents_checked": ["delivery_note", "permit"],
  "validation_results": {
    "delivery_note": {"valid": true, "number": "DN-12345"},
    "permit": {"valid": false, "reason": "expired"}
  },
  "inspector_notes": "Permit renewal required"
}
```