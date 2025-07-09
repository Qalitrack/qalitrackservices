# Route Service Visual Guide

## 🎯 **Service Overview**

### **Business Purpose**
The Route Service manages QaliTrack's transportation routes, including plant-to-plant routing, gate management, timing optimization, and waypoint tracking. It provides centralized route planning and optimization for efficient cargo movement across the logistics network.

### **Key Business Problems Solved**
- **Route Optimization**: Efficient path planning between plants and destinations
- **Gate Management**: Entry/exit point control and scheduling
- **Timing Optimization**: Reduced transit times and fuel consumption
- **Waypoint Tracking**: Real-time location monitoring and ETA calculations
- **Capacity Planning**: Route capacity and congestion management
- **Compliance**: Regulatory compliance for transportation routes

### **Integration Role**
Central routing authority that provides optimized routes for transportation assignments, validates route feasibility, and tracks route performance across the logistics network.

---

## 🏗️ **Architecture Diagram**

```
┌─────────────────────────────────────────────────────────────────┐
│                        Route Service                            │
│                         Port: 7006                              │
└─────────────────────────────────────────────────────────────────┘
┌─────────────────────────────────────────────────────────────────┐
│                          API Layer                              │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐   │
│  │   RoutesController     │  │   GatesController     │  │ WaypointsController   │   │
│  │  - CRUD operations     │  │  - Gate management    │  │  - Waypoint management│   │
│  │  - Route optimization  │  │  - Schedule management│  │  - Tracking operations│   │
│  │  - Path calculation    │  │  - Capacity planning  │  │  - ETA calculations   │   │
│  │  - Performance metrics│  │  - Access control     │  │  - Route monitoring   │   │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘   │
└─────────────────────────────────────────────────────────────────┘
┌─────────────────────────────────────────────────────────────────┐
│                         Core Layer                              │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐   │
│  │   RouteService     │  │   GateService       │  │ WaypointService     │   │
│  │  - Route logic     │  │  - Gate logic       │  │  - Tracking logic   │   │
│  │  - Optimization    │  │  - Schedule logic   │  │  - ETA calculations │   │
│  │  - Path algorithms │  │  - Capacity mgmt    │  │  - Location services│   │
│  │  - Performance    │  │  - Access control   │  │  - Monitoring       │   │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘   │
│                                                                 │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐   │
│  │     Route          │  │     Gate            │  │     Waypoint        │   │
│  │  - Id, Name        │  │  - Id, Name         │  │  - Id, RouteId      │   │
│  │  - Origin, Dest    │  │  - Location         │  │  - Location         │   │
│  │  - Distance, Time  │  │  - Type, Status     │  │  - Sequence         │   │
│  │  - Waypoints       │  │  - Capacity         │  │  - EstimatedTime    │   │
│  │  - Status          │  │  - Schedule         │  │  - Restrictions     │   │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘   │
└─────────────────────────────────────────────────────────────────┘
┌─────────────────────────────────────────────────────────────────┐
│                    Infrastructure Layer                         │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐   │
│  │  RouteRepository   │  │  GateRepository     │  │ WaypointRepository  │   │
│  │  - CRUD operations │  │  - CRUD operations  │  │  - CRUD operations  │   │
│  │  - Path queries    │  │  - Schedule queries │  │  - Tracking queries │   │
│  │  - Optimization    │  │  - Capacity queries │  │  - Location queries │   │
│  │  - Performance    │  │  - Access queries   │  │  - Sequence queries │   │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘   │
│                                                                 │
│  ┌─────────────────────────────────────────────────────────────┐  │
│  │                   RouteDbContext                             │  │
│  │  - Routes, Gates, Waypoints, Schedules Tables               │  │
│  │  - Entity configurations and relationships                  │  │
│  │  - Database migrations and seeding                          │  │
│  └─────────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
```

---

## 📊 **Entity Model**

### **Core Entities**

#### **Route Entity**
```csharp
public class Route : BaseEntity
{
    public string Name { get; set; }                    // "Mombasa Plant → Nairobi Warehouse"
    public string Code { get; set; }                    // "RT-MOB-NAI-001"
    public RouteType Type { get; set; }                 // PlantToPlant, PlantToWarehouse, PlantToCustomer
    public LocationPoint Origin { get; set; }
    public LocationPoint Destination { get; set; }
    public RouteDetails Details { get; set; }
    public List<Waypoint> Waypoints { get; set; }
    public RouteRestrictions Restrictions { get; set; }
    public RouteStatus Status { get; set; }             // Active, Inactive, Maintenance, Seasonal
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedBy { get; set; }
    public string UpdatedBy { get; set; }
}

public enum RouteType
{
    PlantToPlant,       // Between QaliTrack plants
    PlantToWarehouse,   // Plant to distribution warehouse
    PlantToCustomer,    // Direct plant to customer delivery
    WarehouseToCustomer, // Warehouse to customer delivery
    ReturnRoute,        // Return/backhaul route
    EmergencyRoute      // Emergency or backup route
}

public class LocationPoint
{
    public string Name { get; set; }                    // "Mombasa Plant"
    public string Code { get; set; }                    // "MOB-PLT-001"
    public string Address { get; set; }                 // "Industrial Area, Mombasa"
    public GeoCoordinate Coordinates { get; set; }
    public string ContactInfo { get; set; }             // "+254712345678"
    public List<string> AccessInstructions { get; set; } // ["Use Gate 2", "Report to security"]
}

public class GeoCoordinate
{
    public decimal Latitude { get; set; }               // -4.0435
    public decimal Longitude { get; set; }              // 39.6682
    public decimal? Altitude { get; set; }              // 16.0 (meters above sea level)
}

public class RouteDetails
{
    public decimal TotalDistance { get; set; }          // 485.5 (kilometers)
    public TimeSpan EstimatedTime { get; set; }         // 6 hours 30 minutes
    public decimal FuelConsumption { get; set; }        // 150.0 (liters per trip)
    public decimal TollCosts { get; set; }              // 800.0 (KES)
    public RoadConditions RoadConditions { get; set; }
    public TrafficPatterns TrafficPatterns { get; set; }
    public string PreferredTimeWindow { get; set; }     // "06:00-18:00"
    public List<string> AlternativeRoutes { get; set; } // ["RT-MOB-NAI-002", "RT-MOB-NAI-003"]
}

public class RoadConditions
{
    public string PrimaryRoadType { get; set; }         // "Highway"
    public string SurfaceCondition { get; set; }        // "Good"
    public decimal AverageSpeed { get; set; }           // 75.0 (km/h)
    public List<string> Hazards { get; set; }           // ["Construction at KM 120", "Steep hills"]
    public string WeatherConsiderations { get; set; }   // "Rainy season affects visibility"
}

public class TrafficPatterns
{
    public string PeakHours { get; set; }               // "07:00-09:00, 17:00-19:00"
    public string OptimalDeparture { get; set; }        // "05:00-06:00"
    public decimal TrafficDelayFactor { get; set; }     // 1.2 (20% additional time)
    public List<string> CongestionPoints { get; set; }  // ["Nairobi CBD", "Mombasa Bridge"]
}
```

#### **Gate Entity**
```csharp
public class Gate : BaseEntity
{
    public string Name { get; set; }                    // "Mombasa Plant Main Gate"
    public string Code { get; set; }                    // "MOB-PLT-GATE-01"
    public string LocationId { get; set; }              // References location (Plant/Warehouse)
    public GateType Type { get; set; }                  // Entry, Exit, Both
    public GeoCoordinate Coordinates { get; set; }
    public GateCapacity Capacity { get; set; }
    public OperatingHours Hours { get; set; }
    public List<GateRestriction> Restrictions { get; set; }
    public GateStatus Status { get; set; }              // Open, Closed, Maintenance, Restricted
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedBy { get; set; }
    public string UpdatedBy { get; set; }
}

public enum GateType
{
    Entry,              // Entry only
    Exit,               // Exit only
    Both,               // Bidirectional
    Checkpoint,         // Security checkpoint
    Weighbridge,        // Weighbridge gate
    Emergency          // Emergency access
}

public class GateCapacity
{
    public int MaxVehiclesPerHour { get; set; }         // 20
    public int MaxVehiclesQueued { get; set; }          // 10
    public decimal MaxVehicleWeight { get; set; }       // 50.0 (tons)
    public decimal MaxVehicleLength { get; set; }       // 18.0 (meters)
    public decimal MaxVehicleHeight { get; set; }       // 4.5 (meters)
    public List<string> AllowedVehicleTypes { get; set; } // ["Truck", "Trailer", "Tanker"]
}

public class OperatingHours
{
    public TimeSpan OpenTime { get; set; }              // 06:00
    public TimeSpan CloseTime { get; set; }             // 18:00
    public List<string> OperatingDays { get; set; }     // ["Monday", "Tuesday", ..., "Friday"]
    public List<string> ClosedDays { get; set; }        // ["Saturday", "Sunday"]
    public List<SpecialHours> SpecialSchedule { get; set; }
}

public class SpecialHours
{
    public DateTime Date { get; set; }                  // 2024-12-25
    public TimeSpan? OpenTime { get; set; }             // null = closed
    public TimeSpan? CloseTime { get; set; }            // null = closed
    public string Reason { get; set; }                  // "Christmas Holiday"
}

public class GateRestriction
{
    public string Type { get; set; }                    // "Vehicle", "Time", "Weight", "Documentation"
    public string Rule { get; set; }                    // "No entry without delivery note"
    public string Penalty { get; set; }                 // "Denied entry"
    public bool IsActive { get; set; }
}
```

#### **Waypoint Entity**
```csharp
public class Waypoint : BaseEntity
{
    public string RouteId { get; set; }                 // References Route
    public string Name { get; set; }                    // "Nakuru Bypass Junction"
    public string Code { get; set; }                    // "WP-NAK-001"
    public GeoCoordinate Coordinates { get; set; }
    public int Sequence { get; set; }                   // 1, 2, 3, ... (order in route)
    public WaypointType Type { get; set; }              // Checkpoint, RestStop, FuelStation, Gate
    public WaypointDetails Details { get; set; }
    public List<WaypointService> Services { get; set; }
    public bool IsOptional { get; set; }                // Can be skipped if needed
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedBy { get; set; }
    public string UpdatedBy { get; set; }
    
    // Navigation properties
    public Route Route { get; set; }
}

public enum WaypointType
{
    Checkpoint,         // Mandatory check-in point
    RestStop,           // Driver rest area
    FuelStation,        // Fuel/service station
    Gate,              // Entry/exit gate
    Weighbridge,       // Weight measurement point
    Customs,           // Customs/border point
    Maintenance,       // Maintenance facility
    Emergency          // Emergency service point
}

public class WaypointDetails
{
    public decimal DistanceFromOrigin { get; set; }     // 145.5 (km from route start)
    public TimeSpan EstimatedTime { get; set; }         // 2 hours from route start
    public decimal AverageStopDuration { get; set; }    // 15.0 (minutes)
    public string ContactInfo { get; set; }             // "+254723456789"
    public List<string> Instructions { get; set; }      // ["Stop at security", "Present documents"]
    public string AccessHours { get; set; }             // "24/7" or "06:00-22:00"
}

public class WaypointService
{
    public string Type { get; set; }                    // "Fuel", "Rest", "Maintenance", "Security"
    public string Provider { get; set; }                // "Total Kenya"
    public string Description { get; set; }             // "24/7 fuel station with truck parking"
    public decimal? Cost { get; set; }                  // 120.0 (KES per liter)
    public bool IsAvailable { get; set; }
}
```

#### **RouteSchedule Entity**
```csharp
public class RouteSchedule : BaseEntity
{
    public string RouteId { get; set; }                 // References Route
    public string TransporterId { get; set; }           // References Transporter Service
    public string VehicleId { get; set; }               // References Vehicle Service
    public string DriverId { get; set; }                // References Driver Service
    public DateTime ScheduledDeparture { get; set; }    // 2024-01-25T06:00:00Z
    public DateTime ScheduledArrival { get; set; }      // 2024-01-25T12:30:00Z
    public DateTime? ActualDeparture { get; set; }      // Actual departure time
    public DateTime? ActualArrival { get; set; }        // Actual arrival time
    public ScheduleStatus Status { get; set; }          // Scheduled, InProgress, Completed, Delayed, Cancelled
    public List<WaypointProgress> WaypointProgress { get; set; }
    public string OrderId { get; set; }                 // References Customer Service order
    public decimal LoadWeight { get; set; }             // 45.0 (tons)
    public string CargoType { get; set; }               // "Cement"
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedBy { get; set; }
    public string UpdatedBy { get; set; }
    
    // Navigation properties
    public Route Route { get; set; }
}

public enum ScheduleStatus
{
    Scheduled,          // Trip scheduled
    InProgress,         // Trip in progress
    Completed,          // Trip completed successfully
    Delayed,            // Trip delayed
    Cancelled,          // Trip cancelled
    Rescheduled        // Trip rescheduled
}

public class WaypointProgress
{
    public string WaypointId { get; set; }              // References Waypoint
    public DateTime? ScheduledTime { get; set; }        // Expected arrival time
    public DateTime? ActualTime { get; set; }           // Actual arrival time
    public TimeSpan? Duration { get; set; }             // Time spent at waypoint
    public string Status { get; set; }                  // "Reached", "Skipped", "Delayed"
    public string Notes { get; set; }                   // "Traffic delay at junction"
}
```

### **Entity Relationships**

```
┌─────────────────┐         ┌─────────────────┐         ┌─────────────────┐
│     Route       │◄────────┤    Waypoint     │         │     Gate        │
│                 │         │                 │         │                 │
│ • Id            │         │ • RouteId       │         │ • Id            │
│ • Name          │         │ • Name          │         │ • Name          │
│ • Origin        │         │ • Coordinates   │         │ • Type          │
│ • Destination   │         │ • Sequence      │         │ • Capacity      │
│ • Distance      │         │ • Type          │         │ • Hours         │
│ • Status        │         │ • IsOptional    │         │ • Status        │
└─────────────────┘         └─────────────────┘         └─────────────────┘
         │                                                        │
         │                                                        │
         ▼                                                        │
┌─────────────────┐                                              │
│  RouteSchedule  │                                              │
│                 │                                              │
│ • RouteId       │                                              │
│ • TransporterId │                                              │
│ • VehicleId     │                                              │
│ • DriverId      │                                              │
│ • ScheduledDep  │                                              │
│ • ScheduledArr  │                                              │
│ • Status        │                                              │
└─────────────────┘                                              │
                                                                 │
                                                                 ▼
                                                       ┌─────────────────┐
                                                       │  LocationPoint  │
                                                       │                 │
                                                       │ • Name          │
                                                       │ • Coordinates   │
                                                       │ • Address       │
                                                       │ • ContactInfo   │
                                                       └─────────────────┘
```

---

## 🔌 **API Endpoints**

### **Route Management**

#### **GET /api/routes**
**Purpose**: Retrieve paginated route list with filtering
```json
// Request
GET /api/routes?page=1&size=10&origin=Mombasa&destination=Nairobi&status=Active

// Response
{
  "data": [
    {
      "id": "route-001",
      "name": "Mombasa Plant → Nairobi Warehouse",
      "code": "RT-MOB-NAI-001",
      "type": "PlantToWarehouse",
      "origin": {
        "name": "Mombasa Plant",
        "code": "MOB-PLT-001",
        "address": "Industrial Area, Mombasa",
        "coordinates": {
          "latitude": -4.0435,
          "longitude": 39.6682,
          "altitude": 16.0
        },
        "contactInfo": "+254712345678"
      },
      "destination": {
        "name": "Nairobi Warehouse",
        "code": "NAI-WH-001",
        "address": "Industrial Area, Nairobi",
        "coordinates": {
          "latitude": -1.3194,
          "longitude": 36.8441,
          "altitude": 1661.0
        },
        "contactInfo": "+254723456789"
      },
      "details": {
        "totalDistance": 485.5,
        "estimatedTime": "06:30:00",
        "fuelConsumption": 150.0,
        "tollCosts": 800.0,
        "roadConditions": {
          "primaryRoadType": "Highway",
          "surfaceCondition": "Good",
          "averageSpeed": 75.0,
          "hazards": ["Construction at KM 120"],
          "weatherConsiderations": "Rainy season affects visibility"
        },
        "trafficPatterns": {
          "peakHours": "07:00-09:00, 17:00-19:00",
          "optimalDeparture": "05:00-06:00",
          "trafficDelayFactor": 1.2,
          "congestionPoints": ["Nairobi CBD"]
        },
        "preferredTimeWindow": "06:00-18:00",
        "alternativeRoutes": ["RT-MOB-NAI-002"]
      },
      "waypoints": [
        {
          "id": "wp-001",
          "name": "Nakuru Bypass Junction",
          "code": "WP-NAK-001",
          "coordinates": {
            "latitude": -0.3031,
            "longitude": 36.0800
          },
          "sequence": 1,
          "type": "Checkpoint",
          "details": {
            "distanceFromOrigin": 145.5,
            "estimatedTime": "02:00:00",
            "averageStopDuration": 15.0,
            "contactInfo": "+254734567890",
            "instructions": ["Stop at security checkpoint"],
            "accessHours": "24/7"
          },
          "services": [
            {
              "type": "Fuel",
              "provider": "Total Kenya",
              "description": "24/7 fuel station with truck parking",
              "cost": 120.0,
              "isAvailable": true
            }
          ],
          "isOptional": false,
          "isActive": true
        }
      ],
      "restrictions": {
        "maxWeight": 50.0,
        "maxLength": 18.0,
        "maxHeight": 4.5,
        "allowedVehicleTypes": ["Truck", "Trailer"],
        "timeRestrictions": "No night travel 20:00-06:00",
        "weatherRestrictions": "Closed during heavy rain",
        "seasonalRestrictions": "Reduced capacity during rainy season"
      },
      "status": "Active",
      "createdAt": "2024-01-15T10:30:00Z",
      "updatedAt": "2024-01-20T14:45:00Z"
    }
  ],
  "pagination": {
    "page": 1,
    "size": 10,
    "total": 25,
    "totalPages": 3
  }
}
```

#### **GET /api/routes/{id}**
**Purpose**: Retrieve specific route details
```json
// Request
GET /api/routes/route-001

// Response: Complete route object with all waypoints and details
```

#### **POST /api/routes**
**Purpose**: Create new route
```json
// Request
POST /api/routes
{
  "name": "Kisumu Plant → Nairobi Warehouse",
  "code": "RT-KIS-NAI-001",
  "type": "PlantToWarehouse",
  "origin": {
    "name": "Kisumu Plant",
    "code": "KIS-PLT-001",
    "address": "Industrial Area, Kisumu",
    "coordinates": {
      "latitude": -0.0917,
      "longitude": 34.7680,
      "altitude": 1131.0
    },
    "contactInfo": "+254745678901"
  },
  "destination": {
    "name": "Nairobi Warehouse",
    "code": "NAI-WH-001",
    "address": "Industrial Area, Nairobi",
    "coordinates": {
      "latitude": -1.3194,
      "longitude": 36.8441,
      "altitude": 1661.0
    },
    "contactInfo": "+254723456789"
  },
  "details": {
    "totalDistance": 350.0,
    "estimatedTime": "05:00:00",
    "fuelConsumption": 110.0,
    "tollCosts": 600.0,
    "roadConditions": {
      "primaryRoadType": "Highway",
      "surfaceCondition": "Good",
      "averageSpeed": 70.0,
      "hazards": [],
      "weatherConsiderations": "Clear roads year-round"
    },
    "trafficPatterns": {
      "peakHours": "07:00-09:00, 17:00-19:00",
      "optimalDeparture": "06:00-07:00",
      "trafficDelayFactor": 1.1,
      "congestionPoints": ["Nakuru Town"]
    },
    "preferredTimeWindow": "06:00-18:00",
    "alternativeRoutes": []
  },
  "restrictions": {
    "maxWeight": 50.0,
    "maxLength": 18.0,
    "maxHeight": 4.5,
    "allowedVehicleTypes": ["Truck", "Trailer"],
    "timeRestrictions": "No night travel 20:00-06:00",
    "weatherRestrictions": "None",
    "seasonalRestrictions": "None"
  },
  "status": "Active"
}

// Response: Created route object
```

#### **PUT /api/routes/{id}**
**Purpose**: Update existing route
```json
// Request: Updated route object
// Response: Updated route object
```

#### **DELETE /api/routes/{id}**
**Purpose**: Soft delete route (mark as inactive)
```json
// Request
DELETE /api/routes/route-001

// Response
{
  "message": "Route deactivated successfully",
  "routeId": "route-001"
}
```

### **Gate Management**

#### **GET /api/gates**
**Purpose**: Retrieve gate list with filtering
```json
// Request
GET /api/gates?location=Mombasa&type=Entry&status=Open

// Response
{
  "data": [
    {
      "id": "gate-001",
      "name": "Mombasa Plant Main Gate",
      "code": "MOB-PLT-GATE-01",
      "locationId": "MOB-PLT-001",
      "type": "Both",
      "coordinates": {
        "latitude": -4.0435,
        "longitude": 39.6682
      },
      "capacity": {
        "maxVehiclesPerHour": 20,
        "maxVehiclesQueued": 10,
        "maxVehicleWeight": 50.0,
        "maxVehicleLength": 18.0,
        "maxVehicleHeight": 4.5,
        "allowedVehicleTypes": ["Truck", "Trailer", "Tanker"]
      },
      "hours": {
        "openTime": "06:00:00",
        "closeTime": "18:00:00",
        "operatingDays": ["Monday", "Tuesday", "Wednesday", "Thursday", "Friday"],
        "closedDays": ["Saturday", "Sunday"],
        "specialSchedule": [
          {
            "date": "2024-12-25",
            "openTime": null,
            "closeTime": null,
            "reason": "Christmas Holiday"
          }
        ]
      },
      "restrictions": [
        {
          "type": "Documentation",
          "rule": "Must present delivery note",
          "penalty": "Entry denied",
          "isActive": true
        }
      ],
      "status": "Open",
      "createdAt": "2024-01-15T10:30:00Z",
      "updatedAt": "2024-01-20T14:45:00Z"
    }
  ]
}
```

#### **POST /api/gates**
**Purpose**: Create new gate
```json
// Request
POST /api/gates
{
  "name": "Nairobi Warehouse Side Gate",
  "code": "NAI-WH-GATE-02",
  "locationId": "NAI-WH-001",
  "type": "Exit",
  "coordinates": {
    "latitude": -1.3194,
    "longitude": 36.8441
  },
  "capacity": {
    "maxVehiclesPerHour": 15,
    "maxVehiclesQueued": 8,
    "maxVehicleWeight": 40.0,
    "maxVehicleLength": 16.0,
    "maxVehicleHeight": 4.0,
    "allowedVehicleTypes": ["Truck", "Trailer"]
  },
  "hours": {
    "openTime": "07:00:00",
    "closeTime": "17:00:00",
    "operatingDays": ["Monday", "Tuesday", "Wednesday", "Thursday", "Friday"],
    "closedDays": ["Saturday", "Sunday"],
    "specialSchedule": []
  },
  "restrictions": [
    {
      "type": "Time",
      "rule": "No exit after 17:00",
      "penalty": "Delayed until next day",
      "isActive": true
    }
  ],
  "status": "Open"
}

// Response: Created gate object
```

### **Route Scheduling**

#### **GET /api/routes/{id}/schedules**
**Purpose**: Retrieve route schedules
```json
// Request
GET /api/routes/route-001/schedules?date=2024-01-25&status=Scheduled

// Response
{
  "data": [
    {
      "id": "schedule-001",
      "routeId": "route-001",
      "transporterId": "trans-001",
      "vehicleId": "vehicle-001",
      "driverId": "driver-001",
      "scheduledDeparture": "2024-01-25T06:00:00Z",
      "scheduledArrival": "2024-01-25T12:30:00Z",
      "actualDeparture": null,
      "actualArrival": null,
      "status": "Scheduled",
      "waypointProgress": [
        {
          "waypointId": "wp-001",
          "scheduledTime": "2024-01-25T08:00:00Z",
          "actualTime": null,
          "duration": null,
          "status": "Pending",
          "notes": ""
        }
      ],
      "orderId": "order-001",
      "loadWeight": 45.0,
      "cargoType": "Cement",
      "createdAt": "2024-01-24T15:30:00Z",
      "updatedAt": "2024-01-24T15:30:00Z"
    }
  ]
}
```

#### **POST /api/routes/{id}/schedules**
**Purpose**: Create new route schedule
```json
// Request
POST /api/routes/route-001/schedules
{
  "transporterId": "trans-001",
  "vehicleId": "vehicle-001",
  "driverId": "driver-001",
  "scheduledDeparture": "2024-01-26T06:00:00Z",
  "scheduledArrival": "2024-01-26T12:30:00Z",
  "orderId": "order-002",
  "loadWeight": 50.0,
  "cargoType": "Cement"
}

// Response: Created schedule object
```

### **Business-Specific Endpoints**

#### **POST /api/routes/optimize**
**Purpose**: Optimize route between two points
```json
// Request
POST /api/routes/optimize
{
  "origin": {
    "name": "Mombasa Plant",
    "coordinates": {
      "latitude": -4.0435,
      "longitude": 39.6682
    }
  },
  "destination": {
    "name": "Nairobi Warehouse",
    "coordinates": {
      "latitude": -1.3194,
      "longitude": 36.8441
    }
  },
  "vehicleType": "Truck",
  "maxWeight": 45.0,
  "departureTime": "2024-01-25T06:00:00Z",
  "preferences": {
    "avoidTolls": false,
    "avoidTraffic": true,
    "preferHighways": true,
    "fuelEfficiency": true
  }
}

// Response
{
  "optimizedRoute": {
    "id": "route-001",
    "name": "Mombasa Plant → Nairobi Warehouse",
    "totalDistance": 485.5,
    "estimatedTime": "06:30:00",
    "estimatedFuelCost": 18000.0,
    "tollCosts": 800.0,
    "totalCost": 18800.0,
    "waypoints": [
      {
        "name": "Nakuru Bypass Junction",
        "coordinates": {
          "latitude": -0.3031,
          "longitude": 36.0800
        },
        "distanceFromOrigin": 145.5,
        "estimatedTime": "02:00:00"
      }
    ],
    "alternativeRoutes": [
      {
        "id": "route-002",
        "name": "Alternative via Nakuru Town",
        "totalDistance": 495.0,
        "estimatedTime": "07:00:00",
        "totalCost": 19500.0
      }
    ],
    "trafficAlerts": [
      {
        "location": "Nairobi CBD",
        "type": "Congestion",
        "severity": "Medium",
        "expectedDelay": "00:30:00"
      }
    ]
  }
}
```

#### **GET /api/routes/availability**
**Purpose**: Check route availability for specific time
```json
// Request
GET /api/routes/availability?origin=Mombasa&destination=Nairobi&date=2024-01-25&time=06:00

// Response
{
  "availableRoutes": [
    {
      "id": "route-001",
      "name": "Mombasa Plant → Nairobi Warehouse",
      "estimatedTime": "06:30:00",
      "capacity": {
        "maxVehicles": 10,
        "scheduledVehicles": 3,
        "availableSlots": 7
      },
      "gateStatus": {
        "originGate": "Open",
        "destinationGate": "Open",
        "waypointGates": ["Open", "Open"]
      },
      "weatherConditions": "Clear",
      "trafficConditions": "Light",
      "recommendedDeparture": "2024-01-25T06:00:00Z",
      "estimatedArrival": "2024-01-25T12:30:00Z"
    }
  ]
}
```

#### **PUT /api/schedules/{id}/update-progress**
**Purpose**: Update route progress in real-time
```json
// Request
PUT /api/schedules/schedule-001/update-progress
{
  "currentLocation": {
    "latitude": -0.3031,
    "longitude": 36.0800
  },
  "waypointId": "wp-001",
  "arrivalTime": "2024-01-25T08:15:00Z",
  "departureTime": "2024-01-25T08:30:00Z",
  "status": "Reached",
  "notes": "Slight delay due to fuel stop"
}

// Response
{
  "updated": true,
  "schedule": {
    "id": "schedule-001",
    "status": "InProgress",
    "currentWaypoint": "wp-001",
    "nextWaypoint": "wp-002",
    "estimatedArrival": "2024-01-25T13:00:00Z",
    "delay": "00:30:00"
  }
}
```

---

## 🔗 **Integration Points**

### **Service Dependencies**

#### **Customer Service Integration**
```
┌─────────────────┐         ┌─────────────────┐
│ Customer Service│────────►│  Route Service  │
│                 │         │                 │
│ • Orders        │         │ • Route Info    │
│ • RouteId       │         │ • Optimization  │
│ • Destinations  │         │ • Scheduling    │
└─────────────────┘         └─────────────────┘
```

#### **Transporter Service Integration**
```
┌─────────────────┐         ┌─────────────────┐
│Transporter Service│◄──────►│  Route Service  │
│                 │         │                 │
│ • Assignments   │         │ • Route Details │
│ • Capacity      │         │ • Schedules     │
│ • Performance   │         │ • Progress      │
└─────────────────┘         └─────────────────┘
```

#### **Vehicle Service Integration** (Future)
```
┌─────────────────┐         ┌─────────────────┐
│ Vehicle Service │◄───────►│  Route Service  │
│                 │         │                 │
│ • Vehicle Info  │         │ • Route Details │
│ • Restrictions  │         │ • Schedules     │
│ • Tracking      │         │ • Progress      │
└─────────────────┘         └─────────────────┘
```

### **Integration Patterns**

#### **Route Optimization Pattern**
```csharp
// Customer Service -> Route Service
public async Task<OptimizedRoute> OptimizeRouteAsync(RouteOptimizationRequest request)
{
    var optimizedRoute = await _routeService.OptimizeRouteAsync(new RouteOptimizationRequest
    {
        Origin = request.Origin,
        Destination = request.Destination,
        VehicleType = request.VehicleType,
        MaxWeight = request.MaxWeight,
        DepartureTime = request.DepartureTime,
        Preferences = request.Preferences
    });
    
    return optimizedRoute;
}
```

#### **Schedule Tracking Pattern**
```csharp
// Transporter Service -> Route Service
public async Task UpdateRouteProgressAsync(string scheduleId, RouteProgressUpdate update)
{
    await _routeService.UpdateScheduleProgressAsync(scheduleId, new ProgressUpdateRequest
    {
        CurrentLocation = update.CurrentLocation,
        WaypointId = update.WaypointId,
        ArrivalTime = update.ArrivalTime,
        Status = update.Status,
        Notes = update.Notes
    });
}
```

---

## 💼 **Business Use Cases**

### **Use Case 1: Route Planning**

**Scenario**: Operations manager plans optimal route for cement delivery
**Actors**: Operations Manager, System, Customer
**Flow**:
1. Manager receives delivery order from customer
2. System identifies origin (plant) and destination (customer site)
3. System analyzes available routes and current conditions
4. System calculates optimal route based on distance, time, and cost
5. System considers traffic patterns and gate operating hours
6. Optimal route is selected and assigned to delivery
7. Route details are shared with transporter and customer

**Business Value**: Reduces delivery costs and improves efficiency

### **Use Case 2: Real-Time Route Tracking**

**Scenario**: Customer tracks delivery progress in real-time
**Actors**: Customer, Driver, System, Operations Team
**Flow**:
1. Driver begins delivery journey following assigned route
2. System tracks vehicle location through GPS
3. Driver checks in at each waypoint along the route
4. System updates estimated arrival time based on progress
5. Customer receives real-time updates on delivery status
6. System alerts operations team if delays occur
7. Delivery completion is confirmed at destination

**Business Value**: Improves customer satisfaction and operational visibility

### **Use Case 3: Gate Management**

**Scenario**: Plant gate manages incoming and outgoing vehicles
**Actors**: Gate Operator, Driver, System, Security
**Flow**:
1. Driver arrives at plant gate with delivery documents
2. Gate operator scans delivery note and vehicle registration
3. System validates vehicle authorization and capacity
4. System checks gate capacity and operating hours
5. Vehicle is cleared for entry or queued if at capacity
6. Loading/unloading is completed at designated areas
7. Vehicle exits through designated gate with completion records

**Business Value**: Ensures security and manages facility capacity

### **Use Case 4: Route Performance Analysis**

**Scenario**: Operations team analyzes route performance for optimization
**Actors**: Operations Manager, System, Data Analyst
**Flow**:
1. System collects data from completed deliveries
2. Performance metrics are calculated for each route
3. Analysis identifies bottlenecks and improvement opportunities
4. Traffic patterns and seasonal variations are considered
5. Route modifications are proposed based on data
6. New waypoints or alternative routes are suggested
7. Route network is optimized for better performance

**Business Value**: Continuous improvement of logistics efficiency

---

## 🛣️ **Implementation Roadmap**

### **Phase 1: Foundation** (Month 3, Week 1-2)
- 🔄 **Project Setup**: Create Clean Architecture structure
- 🔄 **Core Entities**: Implement Route, Gate, Waypoint entities
- 🔄 **Database Layer**: Set up Entity Framework with SQLite
- 🔄 **Basic Repository**: Implement repository pattern for data access

### **Phase 2: Core Features** (Month 3, Week 3-4)
- 🔄 **Route Management**: Complete CRUD operations for routes
- 🔄 **Gate Management**: Gate configuration and capacity management
- 🔄 **Waypoint Management**: Waypoint creation and sequencing
- 🔄 **API Controllers**: REST API endpoints for all operations

### **Phase 3: Integration** (Month 4, Week 1-2)
- 🔄 **Customer Service Integration**: Route validation and assignment
- 🔄 **Transporter Service Integration**: Schedule management
- 🔄 **Route Optimization**: Basic optimization algorithms
- 🔄 **API Documentation**: Comprehensive OpenAPI documentation

### **Phase 4: Advanced Features** (Month 4, Week 3-4)
- 🔄 **Real-Time Tracking**: GPS integration and progress monitoring
- 🔄 **Performance Analytics**: Route performance metrics
- 🔄 **Traffic Integration**: Real-time traffic data integration
- 🔄 **Monitoring**: Health checks and performance metrics

### **Current Status**: 🔄 **READY FOR IMPLEMENTATION**
- **Priority**: MEDIUM (Third priority after Product and Transporter)
- **Dependencies**: Customer Service, Transporter Service
- **Estimated Timeline**: 1 month for complete implementation
- **Resource Requirements**: 1 developer, part-time

---

## 📊 **Success Metrics**

### **Technical Metrics**
- **API Response Time**: < 200ms for route optimization
- **Database Performance**: < 150ms for complex route queries
- **Service Availability**: 99.9% uptime
- **Integration Success**: 100% route validation accuracy

### **Business Metrics**
- **Route Network**: 15+ routes configured
- **Gate Coverage**: 10+ gates managed
- **Waypoint Density**: 50+ waypoints tracked
- **Schedule Accuracy**: 95% on-time performance

### **Quality Metrics**
- **Route Optimization**: 15% reduction in fuel costs
- **Delivery Accuracy**: 98% on-time deliveries
- **Customer Satisfaction**: 4.2+ average rating
- **System Reliability**: 0 critical routing failures

---

*Route Service serves as the central routing and scheduling hub, enabling optimized transportation planning, real-time tracking, and efficient gate management across the QaliTrack logistics network.*