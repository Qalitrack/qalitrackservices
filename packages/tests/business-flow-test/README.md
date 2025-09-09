# QaliTrack Business Flow Test Interface

A comprehensive testing interface for QaliTrack's cement operations workflow, designed to test the complete business process from foundation data setup to weighing transactions.

## 🏭 Overview

This interface provides step-by-step testing of Bamburi Cement's operations workflow following the exact Entity Relationship diagram dependencies:

- **Level 0**: Foundation Data (Pure Reference Tables)
- **Level 1**: Business Configuration 
- **Level 2**: Operational Configuration
- **Level 3**: Business Orders & Transactions

## ⚙️ Features

- **🔧 Configurable API Endpoints**: Set MasterData and DataManager API URLs from the UI
- **📊 Visual Progress Tracking**: Real-time progress bar and status indicators
- **🔗 Dependency Management**: Clear indicators of what needs to be created first
- **📱 Responsive Design**: Works on desktop and mobile devices  
- **🎯 Sample Data**: Realistic Bamburi Cement data for Kenya operations
- **💾 Persistent Configuration**: API settings saved to browser localStorage
- **🔄 Bulk Operations**: Create all sample data or load all existing data with one click

## 🚀 Quick Start

### Option 1: Python Server (Recommended)
```bash
# Start with default port 8080
./run.sh

# Or specify custom port
./run.sh 3000
```

### Option 2: Python Direct
```bash
python3 serve.py        # Port 8080
python3 serve.py 3000   # Port 3000
```

### Option 3: Node.js (Alternative)
```bash
npx http-server . -p 8080 -c-1
```

### Option 4: Other HTTP Servers
```bash
# PHP
php -S localhost:8080

# Ruby
ruby -run -e httpd . -p 8080
```

## 📋 Prerequisites

1. **QaliTrack Services Running**:
   - MasterData API: http://localhost:5004 (default)
   - DataManager API: http://localhost:5005 (default)

2. **Web Server**: Python 3, Node.js, PHP, or any HTTP server

## 🔧 Configuration

1. **Open the interface** in your browser
2. **Click "⚙️ Configure"** in the API Configuration section
3. **Set your API endpoints**:
   - MasterData API Base URL (e.g., http://localhost:5004)
   - DataManager API Base URL (e.g., http://localhost:5005)
4. **Click "Save Configuration"**
5. **Test connections** using the "Test" buttons

## 📈 Workflow Steps

### Level 0: Foundation Data
1. **Location Types**: Plant, Warehouse, Terminal, Branch
2. **Zones**: Coastal, Eastern, Central regions
3. **Product Categories**: Cement → OPC/PPC, Clinker, Raw Materials
4. **Packaging Types**: 50kg Bags, 25kg Bags, Bulk, Container

### Level 1: Business Configuration  
5. **Sites**: Mombasa Plant, Nairobi Grinding Station, BSP Plant
6. **Business Entities**: Customers, Suppliers, Transporters
7. **Vehicles & Drivers**: Fleet with Kenyan registration numbers

### Level 2: Operational Configuration
8. **Weighbridges**: Scale configurations at each site

### Level 3: Business Orders & Transactions
9. **Customer Orders**: Sales orders from construction companies
10. **Purchase Orders**: Raw material procurement
11. **Weighing Transactions**: Actual weighing operations

## 🎯 Sample Data

All sample data reflects real Bamburi Cement operations:

- **Locations**: Mombasa, Nairobi, Coastal/Eastern regions
- **Products**: OPC 42.5N, PPC 32.5R, Clinker Grade A
- **Vehicles**: Kenyan registration (KCA 123A, KBZ 456B)
- **Customers**: ABC Construction Ltd, Sunrise Builders
- **Suppliers**: Kenya Limestone Quarries, Coastal Gypsum

## 🔄 Usage Instructions

### Individual Steps
1. **Follow the order**: Start with Level 0, then Level 1, etc.
2. **Check dependencies**: Red warnings show what needs to be created first
3. **Create or Load**: Use "Create Sample Data" for new data, "Load Existing" to view current data
4. **Monitor status**: Green ✅ = success, Red ❌ = error, Gray ⚪ = pending

### Bulk Operations
- **🚀 Create All Sample Data**: Creates complete workflow data in correct order
- **📊 Load All Existing Data**: Loads all current data from both APIs
- **🧹 Clear All Data**: Would clear all data (requires DELETE endpoints)

## 🔍 Troubleshooting

### Connection Issues
- Ensure MasterData and DataManager services are running
- Check API URLs in configuration (don't include /api in the URL)
- Verify no CORS issues (use proper HTTP server, not file:// protocol)

### API Errors
- Check browser console for detailed error messages
- Verify API endpoint implementations
- Some endpoints may not be fully implemented yet

### Sample Data Issues  
- Follow the dependency order (Level 0 → 1 → 2 → 3)
- If creation fails, check the response area for specific error details
- Clear browser localStorage if configuration gets corrupted

## 🏗️ File Structure

```
business-flow-test/
├── index.html              # Main interface
├── assets/
│   ├── css/
│   │   └── styles.css      # Modern responsive styles  
│   └── js/
│       ├── api-client.js   # HTTP client with error handling
│       ├── workflow-manager.js    # UI state and configuration
│       └── business-flow-functions.js  # Workflow step functions
├── data/
│   └── sample-data.js      # Bamburi Cement sample data
├── serve.py                # Python HTTP server
├── run.sh                  # Launch script
└── README.md              # This file
```

## 🔗 API Integration

The interface integrates with:

- **MasterData API** (`/api/SiteManagement`, `/api/BusinessEntity`, `/api/Vehicle`, `/api/Driver`)
- **DataManager API** (`/api/Orders`, `/api/Transactions`, `/api/Quality`)

API responses are displayed in formatted JSON in expandable response areas.

## 🎨 UI Features

- **Modern Design**: Gradient backgrounds, smooth animations, hover effects
- **Status Indicators**: Color-coded status dots with glowing effects
- **Progress Tracking**: Animated progress bar with shine effects
- **Responsive Layout**: Mobile-friendly design
- **Notifications**: Toast-style notifications for important events
- **Modal Configuration**: Clean modal for API endpoint configuration

## 🔐 Browser Storage

- API configuration saved to `localStorage`
- No sensitive data stored
- Configuration persists across browser sessions

## 🤝 Contributing

To extend the interface:

1. Add new sample data to `data/sample-data.js`
2. Add API client methods to `assets/js/api-client.js`
3. Add workflow functions to `assets/js/business-flow-functions.js`
4. Update the HTML structure in `index.html`

---

**🏭 Built for Bamburi Cement operations workflow testing**