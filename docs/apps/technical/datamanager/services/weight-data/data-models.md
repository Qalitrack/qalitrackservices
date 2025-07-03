# Weight Data Service - Data Models

## Overview
The Weight Data Service captures real-time weight measurements from weighbridge hardware and manages weight-related data.

## SQLite Schema (Development)

### Core Tables

#### WeightMeasurements
```sql
CREATE TABLE weight_measurements (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    weighbridge_id TEXT NOT NULL,
    vehicle_id TEXT NOT NULL,
    driver_id TEXT NOT NULL,
    organization_id TEXT NOT NULL,
    
    -- Weight Data
    gross_weight DECIMAL(10,2) NOT NULL,
    tare_weight DECIMAL(10,2),
    net_weight DECIMAL(10,2),
    weight_unit TEXT DEFAULT 'kg' CHECK (weight_unit IN ('kg', 'tons', 'lbs')),
    
    -- Measurement Context
    measurement_type TEXT NOT NULL CHECK (measurement_type IN ('entry', 'exit', 'single')),
    measurement_status TEXT DEFAULT 'active' CHECK (measurement_status IN ('active', 'void', 'corrected')),
    
    -- Hardware Information
    load_cell_readings TEXT, -- JSON array of individual load cell values
    calibration_factor DECIMAL(8,6),
    temperature DECIMAL(5,2),
    
    -- Audit Fields
    measured_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    measured_by TEXT NOT NULL,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    
    -- Foreign Key References (to Master Data Services)
    FOREIGN KEY (weighbridge_id) REFERENCES weighbridges(id),
    FOREIGN KEY (vehicle_id) REFERENCES vehicles(id),
    FOREIGN KEY (driver_id) REFERENCES drivers(id),
    FOREIGN KEY (organization_id) REFERENCES organizations(id)
);
```

#### WeighbridgeStatus
```sql
CREATE TABLE weighbridge_status (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    weighbridge_id TEXT NOT NULL UNIQUE,
    
    -- Operational Status
    is_operational BOOLEAN DEFAULT true,
    current_load DECIMAL(10,2) DEFAULT 0,
    max_capacity DECIMAL(10,2) NOT NULL,
    
    -- Health Metrics
    last_calibration DATETIME,
    calibration_due DATETIME,
    maintenance_status TEXT DEFAULT 'ok' CHECK (maintenance_status IN ('ok', 'warning', 'critical')),
    
    -- Environmental
    temperature DECIMAL(5,2),
    humidity DECIMAL(5,2),
    
    -- System Information
    firmware_version TEXT,
    last_heartbeat DATETIME DEFAULT CURRENT_TIMESTAMP,
    
    -- Audit Fields
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    
    FOREIGN KEY (weighbridge_id) REFERENCES weighbridges(id)
);
```

#### WeightCorrections
```sql
CREATE TABLE weight_corrections (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    original_measurement_id INTEGER NOT NULL,
    
    -- Correction Details
    corrected_gross_weight DECIMAL(10,2),
    corrected_tare_weight DECIMAL(10,2),
    corrected_net_weight DECIMAL(10,2),
    correction_reason TEXT NOT NULL,
    correction_type TEXT NOT NULL CHECK (correction_type IN ('manual', 'system', 'calibration')),
    
    -- Authorization
    corrected_by TEXT NOT NULL,
    authorized_by TEXT,
    correction_timestamp DATETIME DEFAULT CURRENT_TIMESTAMP,
    
    -- Audit Trail
    original_values TEXT, -- JSON of original weight values
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    
    FOREIGN KEY (original_measurement_id) REFERENCES weight_measurements(id)
);
```

### Indexes for Performance
```sql
-- Primary query indexes
CREATE INDEX idx_weight_measurements_weighbridge_date ON weight_measurements(weighbridge_id, measured_at);
CREATE INDEX idx_weight_measurements_vehicle ON weight_measurements(vehicle_id, measured_at);
CREATE INDEX idx_weight_measurements_organization ON weight_measurements(organization_id, measured_at);
CREATE INDEX idx_weight_measurements_status ON weight_measurements(measurement_status);

-- Status monitoring indexes
CREATE INDEX idx_weighbridge_status_operational ON weighbridge_status(is_operational);
CREATE INDEX idx_weighbridge_status_heartbeat ON weighbridge_status(last_heartbeat);

-- Correction tracking indexes
CREATE INDEX idx_weight_corrections_original ON weight_corrections(original_measurement_id);
CREATE INDEX idx_weight_corrections_corrected_by ON weight_corrections(corrected_by, correction_timestamp);
```

## Entity Relationships

### Weight Measurement Entity
```csharp
public class WeightMeasurement
{
    public int Id { get; set; }
    public string WeighbridgeId { get; set; }
    public string VehicleId { get; set; }
    public string DriverId { get; set; }
    public string OrganizationId { get; set; }
    
    // Weight Data
    public decimal GrossWeight { get; set; }
    public decimal? TareWeight { get; set; }
    public decimal? NetWeight { get; set; }
    public string WeightUnit { get; set; } = "kg";
    
    // Measurement Context
    public MeasurementType MeasurementType { get; set; }
    public MeasurementStatus MeasurementStatus { get; set; } = MeasurementStatus.Active;
    
    // Hardware Information
    public string LoadCellReadings { get; set; } // JSON
    public decimal? CalibrationFactor { get; set; }
    public decimal? Temperature { get; set; }
    
    // Audit Fields
    public DateTime MeasuredAt { get; set; }
    public string MeasuredBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Navigation Properties
    public virtual ICollection<WeightCorrection> Corrections { get; set; }
}
```

### Weighbridge Status Entity
```csharp
public class WeighbridgeStatus
{
    public int Id { get; set; }
    public string WeighbridgeId { get; set; }
    
    // Operational Status
    public bool IsOperational { get; set; } = true;
    public decimal CurrentLoad { get; set; }
    public decimal MaxCapacity { get; set; }
    
    // Health Metrics
    public DateTime? LastCalibration { get; set; }
    public DateTime? CalibrationDue { get; set; }
    public MaintenanceStatus MaintenanceStatus { get; set; } = MaintenanceStatus.Ok;
    
    // Environmental
    public decimal? Temperature { get; set; }
    public decimal? Humidity { get; set; }
    
    // System Information
    public string FirmwareVersion { get; set; }
    public DateTime LastHeartbeat { get; set; }
    
    // Audit Fields
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

## Enums

```csharp
public enum MeasurementType
{
    Entry,
    Exit,
    Single
}

public enum MeasurementStatus
{
    Active,
    Void,
    Corrected
}

public enum MaintenanceStatus
{
    Ok,
    Warning,
    Critical
}
```

## Data Validation Rules

### Weight Measurement Validation
- **GrossWeight**: Must be > 0 and <= weighbridge max capacity
- **TareWeight**: Must be > 0 and < GrossWeight (if provided)
- **NetWeight**: Must equal GrossWeight - TareWeight (if both provided)
- **WeighbridgeId**: Must exist in Master Data Service
- **VehicleId**: Must exist and be active in Master Data Service
- **DriverId**: Must exist and have valid license in Master Data Service

### Business Rules
- **Single Measurement**: Only GrossWeight required
- **Entry/Exit Pair**: Entry must have GrossWeight, Exit should calculate NetWeight
- **Weight Corrections**: Only authorized users can make corrections
- **Calibration**: Measurements cannot be taken if calibration is overdue

## Production Migration Path

### PostgreSQL Schema
```sql
-- Partitioned by date for performance
CREATE TABLE weight_measurements (
    -- Same schema but with partitioning
    measured_at TIMESTAMP NOT NULL
) PARTITION BY RANGE (measured_at);

-- Monthly partitions
CREATE TABLE weight_measurements_y2024m01 PARTITION OF weight_measurements
    FOR VALUES FROM ('2024-01-01') TO ('2024-02-01');
```

### InfluxDB Time-Series Data
```sql
-- Real-time hardware metrics
measurement: weighbridge_metrics
tags: weighbridge_id, location
fields: current_load, temperature, humidity, load_cell_1, load_cell_2, etc.
time: measurement_timestamp
```