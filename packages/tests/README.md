# QaliTrack Testing Suite

This package contains testing tools and utilities for the QaliTrack system.

## Structure

```
tests/
├── business-flow-test/     # Interactive business flow testing interface
│   ├── index.html         # Main test interface
│   ├── assets/
│   │   ├── css/          # Stylesheets
│   │   └── js/           # JavaScript modules
│   └── data/             # Sample data definitions
├── api-tests/            # API endpoint tests
├── integration-tests/    # End-to-end integration tests
└── performance-tests/    # Load and performance tests
```

## Business Flow Test Interface

The business flow test interface provides a step-by-step testing environment for QaliTrack operations:

- **Location**: `business-flow-test/index.html`
- **Purpose**: Visual testing of the complete cement operations workflow
- **Features**: Sample data creation, dependency management, progress tracking

### Usage

1. Start both MasterData and DataManager services
2. Open `business-flow-test/index.html` in a web browser
3. Follow the step-by-step workflow from Level 0 to Level 3
4. Use the provided sample data or create your own test scenarios

### API Endpoints Tested

- **MasterData API** (Port 5004): Site Management, Business Entities, Fleet Management
- **DataManager API** (Port 5005): Orders, Transactions, Quality Control

## Test Data

Sample data follows Bamburi Cement's actual business requirements:
- Kenyan locations (Mombasa, Nairobi, coastal regions)
- Real product types (OPC 42.5N, Clinker, Raw Materials)
- Actual packaging formats (50kg bags, 25kg bags, bulk)
- Realistic vehicle registrations and driver information