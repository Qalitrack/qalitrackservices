# Routes Module

The Routes module manages route planning, logistics, and transportation path optimization within the QaliTrack system.

## Overview

This module provides comprehensive route management capabilities including:
- Route definition and planning
- Waypoint management
- Schedule coordination
- Route optimization
- Historical tracking
- Performance analytics

## Key Features

- **Route Planning**: Define origin, destination, and waypoints
- **Schedule Management**: Time-based route scheduling
- **Waypoint Tracking**: Detailed stop and checkpoint management
- **Route Optimization**: Distance and time optimization
- **Historical Data**: Route performance and usage history
- **Multi-Modal Support**: Different transportation modes
- **Real-Time Updates**: Dynamic route modifications

## API Endpoints

### Routes
- `GET /api/masterdata/routes` - List all routes
- `GET /api/masterdata/routes/{id}` - Get specific route
- `POST /api/masterdata/routes` - Create new route
- `PUT /api/masterdata/routes/{id}` - Update route information
- `PATCH /api/masterdata/routes/{id}` - Partial route update
- `DELETE /api/masterdata/routes/{id}` - Remove route

### Route Waypoints
- `GET /api/masterdata/routes/{id}/waypoints` - Get route waypoints
- `POST /api/masterdata/routes/{id}/waypoints` - Add waypoint
- `PUT /api/masterdata/routes/{id}/waypoints/{waypointId}` - Update waypoint
- `DELETE /api/masterdata/routes/{id}/waypoints/{waypointId}` - Remove waypoint

### Route Schedules
- `GET /api/masterdata/routes/{id}/schedules` - Get route schedules
- `POST /api/masterdata/routes/{id}/schedules` - Create schedule
- `PUT /api/masterdata/routes/{id}/schedules/{scheduleId}` - Update schedule
- `DELETE /api/masterdata/routes/{id}/schedules/{scheduleId}` - Remove schedule

### Route History
- `GET /api/masterdata/routes/{id}/history` - Get route usage history
- `POST /api/masterdata/routes/{id}/history` - Add history record
- `PUT /api/masterdata/routes/{id}/history/{historyId}` - Update history
- `DELETE /api/masterdata/routes/{id}/history/{historyId}` - Remove history

## Data Models

### Route
Main route entity containing path and logistics information.

**Key Properties:**
- Route name and description
- Origin and destination
- Total distance and duration
- Route type and status
- Optimization settings

### RouteWaypoint
Individual stops and checkpoints along the route.

**Key Properties:**
- Location coordinates
- Waypoint type (pickup, delivery, checkpoint)
- Scheduled arrival/departure times
- Instructions and notes
- Sequence order

### RouteSchedule
Time-based scheduling for route execution.

**Key Properties:**
- Schedule type (daily, weekly, one-time)
- Start and end dates
- Time windows
- Frequency settings
- Active status

### RouteHistory
Historical record of route usage and performance.

**Key Properties:**
- Execution date and time
- Actual vs planned duration
- Distance covered
- Delays and issues
- Performance metrics

## Route Types

- **Delivery Route**: Package/goods delivery
- **Pickup Route**: Collection and pickup
- **Service Route**: Maintenance and service calls
- **Transfer Route**: Inter-facility transfers
- **Return Route**: Round-trip routes

## Route Status

- **Active**: Currently in use
- **Inactive**: Not currently used
- **Planned**: Under development
- **Archived**: Historical/retired
- **Suspended**: Temporarily disabled

## Waypoint Types

- **Origin**: Starting point
- **Destination**: End point
- **Pickup**: Collection point
- **Delivery**: Drop-off point
- **Checkpoint**: Monitoring point
- **Rest Stop**: Driver break location
- **Fuel Stop**: Refueling location

## Schedule Types

- **Daily**: Repeats daily
- **Weekly**: Repeats weekly
- **Monthly**: Repeats monthly
- **One-Time**: Single execution
- **On-Demand**: As-needed basis

## Business Rules

### Route Planning
- Routes must have at least origin and destination
- Waypoints must be in logical sequence
- Total route distance cannot exceed vehicle capacity

### Scheduling
- Schedule times must be realistic
- No overlapping schedules for same resources
- Buffer time required between routes

### Optimization
- Consider traffic patterns
- Account for loading/unloading times
- Factor in driver break requirements
- Optimize for fuel efficiency

## Integration Points

### Vehicle Assignment
- Route-vehicle compatibility
- Vehicle capacity constraints
- Fuel range limitations

### Driver Assignment
- Driver route familiarity
- Driver schedule availability
- Commercial license requirements

### Weighbridge Integration
- Mandatory weighbridge stops
- Route-weighbridge associations
- Compliance checkpoints

### Real-Time Tracking
- GPS tracking integration
- Route deviation alerts
- Progress monitoring

## Performance Metrics

- **On-Time Performance**: Adherence to schedule
- **Route Efficiency**: Time vs planned duration
- **Fuel Efficiency**: Fuel consumption per mile
- **Customer Satisfaction**: Delivery performance
- **Cost Per Mile**: Route operational costs