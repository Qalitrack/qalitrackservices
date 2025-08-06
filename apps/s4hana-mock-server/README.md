# S/4HANA Mock Server - Bamburi Cement Integration

A comprehensive SAP S/4HANA OData v4 mock server designed specifically for Bamburi Cement integration development. Built using SAP's official `@sap-ux/ui5-middleware-fe-mockserver` with realistic Kenyan business data and multi-plant operations.

## 🏗️ Architecture Overview

This mock server provides a complete S/4HANA API simulation environment without requiring production system access. It includes:

- **Multi-API Support**: Sales Order, Business Partner, Material Master, and Plant APIs
- **Bamburi-Specific Data**: 3 plants, 5 business partners, 5 products, realistic scenarios
- **Kenyan Localization**: KES currency, Kenyan addresses, local business practices
- **Docker Integration**: Seamless container deployment with existing infrastructure

## 🚀 Quick Start

### Prerequisites

- Node.js 18+ 
- Docker & Docker Compose
- npm 6+

### 1. Installation

```bash
# Navigate to the mock server directory
cd apps/s4hana-mock-server

# Install dependencies
npm install

# Start the mock server
./start-s4hana-mock.sh
```

### 2. Access APIs

Once running, the following APIs are available:

- **Sales Order API**: http://localhost:8080/sap/opu/odata4/sap/api_salesorder/srvd_a2x/sap/salesorder/0001
- **Business Partner API**: http://localhost:8080/sap/opu/odata/sap/API_BUSINESS_PARTNER  
- **Material API**: http://localhost:8080/sap/opu/odata/sap/API_MATERIAL
- **Plant API**: http://localhost:8080/sap/opu/odata/sap/API_PLANT

## 📊 Bamburi Master Data

### 🏭 Plants (3 Locations)

| Plant Code | Name | Location | Products |
|------------|------|----------|----------|
| 1000 | Bamburi Mombasa Plant | Industrial Area, Mombasa | Premium & General cement, Readymix |
| 1100 | Bamburi Athi River Plant | Athi River Industrial Area | General & Economy cement, Readymix |
| 1200 | Bamburi Nairobi Grinding Plant | Katani Quarry, Machakos | Economy cement, Concrete blocks |

### 👥 Business Partners

#### Customers (3 Segments)
- **Large Contractors**: China Road & Bridge Corporation (CRBC)
  - Credit: 50M KES, Terms: 60 days
- **Regional Dealers**: Nairobi Hardware Dealers Ltd (NHWD)
  - Credit: 5M KES, Terms: 30 days  
- **Individual Customers**: John Mwangi Construction (JMWNG)
  - Credit: 100K KES, Terms: 14 days

#### Vendors
- **Transport**: Mombasa Transport SACCO Ltd (MTSACCO)
- **Utilities**: Kenya Power & Lighting Co. (KPLC)

### 🏗️ Products (5 Items)

| Material | Description | Price (KES) | Category |
|----------|-------------|-------------|----------|
| PWRMAX50 | PowerMax Premium Cement 50kg | 1,250/BAG | Premium |
| NGUVU50 | Nguvu General Purpose Cement 50kg | 835/BAG | General |
| FUNDI50 | Fundi Affordable Cement 50kg | 750/BAG | Economy |
| BBLOX01 | BamburiBlox Paving Blocks | 45/PC | Concrete |
| RMX-M25 | Readymix Concrete Grade M25 | 8,500/M3 | Concrete |

### 📋 Sample Sales Orders

| Order | Customer | Value (KES) | Scenario |
|-------|----------|-------------|----------|
| 0000000001 | CRBC | 16,675,000 | Large Infrastructure Project |
| 0000000002 | NHWD | 2,560,000 | Regional Dealer Multi-Product |  
| 0000000003 | John Mwangi | 75,000 | Individual Small Order |

## 🔧 Configuration

### Environment Variables

```bash
# Docker environment variables
NODE_ENV=development
UI5_SERVE_PORT=8080
CLIENT_CODE=bamburi
BAMBURI_COMPANY_CODE=1710
BAMBURI_SALES_ORG=1710
DEFAULT_CURRENCY=KES
MOCK_DATA_REFRESH_INTERVAL=300000
LOG_LEVEL=info
```

### UI5 Configuration

The mock server uses three main configuration files:

- **ui5.yaml**: Basic UI5 tooling configuration
- **ui5-mock.yaml**: Mock server middleware configuration with all API endpoints
- **manifest.json**: SAP Fiori app manifest with data source definitions

## 📁 Project Structure

```
apps/s4hana-mock-server/
├── package.json                        # SAP middleware dependencies
├── ui5.yaml                            # Basic UI5 configuration
├── ui5-mock.yaml                       # Mock server configuration
├── Dockerfile                          # Container configuration
├── docker-compose.s4hana-mock.yml      # Service orchestration
├── start-s4hana-mock.sh               # Startup script
├── webapp/
│   ├── manifest.json                   # Fiori app manifest
│   ├── localService/
│   │   ├── metadata/                   # OData metadata files
│   │   └── data/                       # Mock data JSON files
│   │       ├── BusinessPartners.json  # Customer/vendor data
│   │       ├── Materials.json         # Product catalog
│   │       ├── Plants.json            # Plant locations
│   │       ├── SalesOrders.json       # Order headers
│   │       └── SalesOrderItems.json   # Order line items
│   └── annotations/
│       └── annotations.xml             # OData annotations
└── data/
    └── bamburi-master-data/            # Additional configuration
```

## 🧪 API Testing

### Sales Order Creation Example

```bash
# Create a new sales order
curl -X POST http://localhost:8080/sap/opu/odata4/sap/api_salesorder/srvd_a2x/sap/salesorder/0001/SalesOrder \
  -H "Content-Type: application/json" \
  -d '{
    "SalesOrderType": "OR",
    "SalesOrganization": "1710",
    "DistributionChannel": "10",
    "Division": "00",
    "SoldToParty": "17100001",
    "_Item": [{
      "SalesOrderItem": "10",
      "Material": "PWRMAX50",
      "RequestedQuantity": "100",
      "RequestedQuantityUnit": "BAG",
      "Plant": "1000"
    }]
  }'
```

### Business Partner Retrieval

```bash
# Get all business partners
curl http://localhost:8080/sap/opu/odata/sap/API_BUSINESS_PARTNER/A_BusinessPartner

# Get specific customer
curl http://localhost:8080/sap/opu/odata/sap/API_BUSINESS_PARTNER/A_BusinessPartner('17100001')
```

### Material Master Query

```bash
# Get all materials
curl http://localhost:8080/sap/opu/odata/sap/API_MATERIAL/A_Product

# Get cement products only
curl "http://localhost:8080/sap/opu/odata/sap/API_MATERIAL/A_Product?\$filter=MaterialGroup eq 'CEMENT'"
```

## 🐳 Docker Operations

### Start Services
```bash
# Using startup script (recommended)
./start-s4hana-mock.sh

# Or manually
docker-compose -f docker-compose.s4hana-mock.yml up -d
```

### Monitor Services
```bash
# View logs
docker-compose -f docker-compose.s4hana-mock.yml logs -f

# Check health
curl http://localhost:8080/health

# View container status
docker-compose -f docker-compose.s4hana-mock.yml ps
```

### Stop Services
```bash
docker-compose -f docker-compose.s4hana-mock.yml down
```

## 🔧 Development

### Adding New Mock Data

1. **Update JSON Files**: Modify data files in `webapp/localService/data/`
2. **Restart Container**: `docker-compose restart s4hana-mock-server`
3. **Verify Changes**: Test API endpoints to confirm data updates

### Configuration Changes

1. **Modify ui5-mock.yaml**: Update service configurations
2. **Rebuild Image**: `docker build -t qalitrack/s4hana-mock-server:latest .`
3. **Restart Services**: `docker-compose down && docker-compose up -d`

## 🚦 Health Monitoring

The mock server includes comprehensive health checks:

- **Container Health**: Docker health check on port 8080
- **Service Health**: REST endpoint at `/health`
- **Redis Health**: Cache service monitoring
- **Log Monitoring**: Structured logging with configurable levels

## 🔄 Integration with Existing Infrastructure

This mock server is designed to integrate seamlessly with the existing QaliTrack testing infrastructure:

### Update Testing Configuration

Add to `apps/testing/docker-compose.mock.yml`:

```yaml
services:
  s4hana-mock:
    extends:
      file: ../s4hana-mock-server/docker-compose.s4hana-mock.yml
      service: s4hana-mock-server
    networks:
      - testing_network
```

### Environment Switching

Configure your application to switch between mock and production:

```javascript
const S4HANA_BASE_URL = process.env.NODE_ENV === 'production' 
  ? 'https://bamburi.s4hana.ondemand.com'
  : 'http://localhost:8080';
```

## 📈 Performance Considerations

- **Memory Usage**: ~256MB RAM allocated to Redis cache
- **Response Time**: <100ms for typical API calls
- **Concurrent Users**: Supports 100+ concurrent connections
- **Data Volume**: Optimized for Bamburi's expected transaction volumes

## 🛡️ Security Features

- **Non-root Container**: Runs as dedicated `ui5user` 
- **Read-only Data**: Mock data mounted as read-only volumes
- **Network Isolation**: Runs in dedicated Docker network
- **Health Monitoring**: Automated health checks and restart policies

## 🔗 References

- [SAP Fiori Tools Samples](https://github.com/SAP-samples/fiori-tools-samples)
- [SAP UX UI5 Middleware FE Mockserver](https://www.npmjs.com/package/@sap-ux/ui5-middleware-fe-mockserver)  
- [Bamburi Master Data Specification (Issue #15)](https://github.com/adarlegendre/qalitrackservices/issues/15)
- [Implementation Plan (Issue #16)](https://github.com/adarlegendre/qalitrackservices/issues/16)
- [UI5 Tooling Documentation](https://sap.github.io/ui5-tooling/)

## 🤝 Contributing

This mock server is part of the QaliTrack-Bamburi integration project. For contributions:

1. Follow the development phases outlined in [Issue #16](https://github.com/adarlegendre/qalitrackservices/issues/16)
2. Ensure all new mock data follows Bamburi specifications
3. Test thoroughly with Docker container deployment
4. Update documentation for any API changes

## 📄 License

This project is proprietary to QaliTrack Systems for Bamburi Cement integration development.

---

**Built with ❤️ by QaliTrack Systems for Bamburi Cement S/4HANA Integration**