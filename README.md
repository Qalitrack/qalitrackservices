# QaliTrack Microservices

A comprehensive weighbridge management system built with .NET 8 microservices architecture.

## 🏗️ Architecture Overview

QaliTrack consists of 19 microservices (so far) organized into two main categories:

## 🎯 Service Categories: WHO vs HOW

**Master Data Services** = **WHO, WHAT, WHERE** (The Entities)
- Defines the **foundational entities** and **static reference data**
- **WHO**: Drivers, Customers, Suppliers, Organizations
- **WHAT**: Products, Vehicles, Routes, Weighbridges  
- **WHERE**: Sites, Organizations, Routes
- Provides the **nouns** of the system - the things that exist

**DataManager Services** = **WHY, HOW, WHEN** (The Processes)
- Handles the **operational workflows** and **business processes**
- **WHY**: Compliance monitoring, Analytics for business insights
- **HOW**: Transaction processing, Weight data capture, Data synchronization
- **WHEN**: Real-time operations, Archive management, Operational scheduling
- Manages the **verbs/actions** of the system - the things that happen

### Master Data Services (11 services)
- **User Service** (Port 7001) - Authentication and user management
- **Organization Service** (Port 7002) - Multi-tenant organization management
- **Vehicle Service** (Port 7003) - Vehicle master data
- **Driver Service** (Port 7004) - Driver master data
- **Product Service** (Port 7005) - Product master data
- **Route Service** (Port 7006) - Route master data
- **Weighbridge Service** (Port 7007) - Weighbridge master data
- **Customer Service** (Port 7008) - Customer master data
- **Supplier Service** (Port 7009) - Supplier master data
- **Transporter Service** (Port 7010) - Transporter master data
- **Sacco Service** (Port 7011) - Sacco master data

### Data Management Services (8 services)
- **Weight Data Service** (Port 7012) - Weight measurements
- **Compliance Service** (Port 7013) - Compliance monitoring
- **Operational Data Service** (Port 7014) - Operational data management
- **Transaction Service** (Port 7015) - Transaction processing
- **Analytics Service** (Port 7016) - Analytics and reporting
- **Data Sync Service** (Port 7017) - Data synchronization
- **Archive Service** (Port 7018) - Data archival

### API Gateway
- **Gateway Service** (Port 7000) - Central entry point with authentication

## 🔐 Authorization System

QaliTrack implements a sophisticated role-based access control (RBAC) system that operates at the API Gateway level, providing centralized security enforcement across all microservices.

### Authorization Architecture

The system uses a **hybrid authorization model**:
- **Coarse-grained control** at the gateway (service-level access) eg: "User must be Operator+ to access /api/vehicles/*"
- **Fine-grained control** within services (endpoint-level permissions) eg: "User can only view vehicles assigned to their site" 
- **Role hierarchy** with inheritance (User < Operator < SiteManager < Admin < SuperAdmin)

### Role Hierarchy

```
SuperAdmin (Level 5) - System-wide administrative access
    ↓
Admin (Level 4) - Full organizational administrative access  
    ↓
SiteManager (Level 3) - Site-level management access
    ↓
Operator (Level 2) - Daily operational tasks
Auditor (Level 2) - Read-only compliance access
ClientAdmin (Level 2) - Organization-specific admin
    ↓
User (Level 1) - Basic authenticated access
```

### Authorization Configuration

The authorization system is managed through YAML configuration files that define:
- Service port mappings
- Role requirements for each service endpoint
- Public endpoints (no authentication required)
- Environment-specific overrides

#### Generate Gateway Authorization Configuration

```bash
# Generate authorization config from YAML rules
make auth-config-generate

# Apply configuration directly to gateway
make auth-config-apply

# Validate existing gateway configuration
make auth-config-validate

# Run authorization generator tests
make auth-config-test
```

#### Authorization Rules Example

```yaml
# configs/auth/auth-rules-template.yml
authorization_rules:
  user-service:
    rules:
      - path: "/api/users/{everything}"
        roles: ["User"]
        description: "Basic user operations"
      - path: "/api/users/admin/{everything}"
        roles: ["Admin"]
        description: "Administrative user operations"
```

### Adding Authorization Rules for New Microservices

When creating a new microservice, add its authorization rules to the master template:

#### 1. Add Rules to Template
Edit `configs/auth/auth-rules-template.yml`:

```yaml
authorization_rules:
  # Existing services...
  user-service:
    rules:
      - path: "/api/users/{everything}"
        roles: ["User"]
  
  # Add your new service:
  inventory-service:
    rules:
      - path: "/api/inventory/{everything}"
        roles: ["Operator"]
      - path: "/api/inventory/admin/{everything}"
        roles: ["Admin"]
    port: 7019
    description: "Inventory management service"
```

#### 2. Automatic Client-Specific Configuration Generation

**New Enhanced Process (Automatic)**: Authorization configurations are now **automatically generated** during deployment creation:

```bash
# Authorization configs are automatically created when generating deployments
python scripts/generate-deployment.py configs/clients/testing.yml
# → Creates configs/auth/testing-ocelot.json automatically

# Or use the qalitrack manager (recommended)
./scripts/qalitrack-manager.sh generate testing
# → Automatically generates both deployment AND auth config
```

**What happens automatically**:
1. Client-specific Ocelot configuration generated (`configs/auth/{client}-ocelot.json`)
2. Only includes services enabled for that specific client
3. Docker compose configured to mount auth config as volume
4. Gateway uses client-specific authorization rules

**Manual Generation (if needed)**:
```bash
# Generate auth config for specific client (manual)
make cloud-auth-gen CLIENT=testing

# List available client configurations
make cloud-auth-list
```

**Important**: Authorization generation is now automatic during deployment creation. No manual steps required!

### Security Features

- **JWT Token Authentication** - Secure token-based authentication
- **Role-Based Access Control** - Hierarchical role enforcement
- **Gateway-Level Security** - Centralized authorization before routing
- **User Context Forwarding** - User information passed to downstream services
- **Environment-Specific Rules** - Different access levels per environment
- **Automatic Configuration** - Generate gateway configs from YAML rules

## 🚀 Quick Start

### Prerequisites
- Docker and Docker Compose
- .NET 8 SDK (for development)
- Git

### Option 1: Docker Compose (Recommended)

```bash
# Clone the repository
git clone <repository-url>
cd qalitrackservices

# Start all services
./scripts/start-services.sh

# Access the API Gateway
open https://localhost:7000

# View API documentation
open https://localhost:7000/swagger
```

### Option 2: Manual Build and Run

```bash
# Build all services
./scripts/build-all.sh

# Or run individual services
cd apps/masterdata/user-service
dotnet run --project src/UserService.Api
```

## 📊 Service URLs

| Service | URL | Documentation |
|---------|-----|---------------|
| API Gateway | https://localhost:7000 | https://localhost:7000/swagger |
| User Service | https://localhost:7001 | https://localhost:7001/swagger |
| Organization Service | https://localhost:7002 | https://localhost:7002/swagger |
| Vehicle Service | https://localhost:7003 | https://localhost:7003/swagger |
| Driver Service | https://localhost:7004 | https://localhost:7004/swagger |
| Product Service | https://localhost:7005 | https://localhost:7005/swagger |
| Route Service | https://localhost:7006 | https://localhost:7006/swagger |
| Weighbridge Service | https://localhost:7007 | https://localhost:7007/swagger |
| Customer Service | https://localhost:7008 | https://localhost:7008/swagger |
| Supplier Service | https://localhost:7009 | https://localhost:7009/swagger |
| Transporter Service | https://localhost:7010 | https://localhost:7010/swagger |
| Sacco Service | https://localhost:7011 | https://localhost:7011/swagger |
| Weight Data Service | https://localhost:7012 | https://localhost:7012/swagger |
| Compliance Service | https://localhost:7013 | https://localhost:7013/swagger |
| Operational Data Service | https://localhost:7014 | https://localhost:7014/swagger |
| Transaction Service | https://localhost:7015 | https://localhost:7015/swagger |
| Analytics Service | https://localhost:7016 | https://localhost:7016/swagger |
| Data Sync Service | https://localhost:7017 | https://localhost:7017/swagger |
| Archive Service | https://localhost:7018 | https://localhost:7018/swagger |

## 🔒 Authentication

The system uses JWT-based authentication through the User Service. To access protected endpoints:

1. Register a user: `POST /api/auth/register`
2. Login: `POST /api/auth/login`
3. Use the returned JWT token in the Authorization header: `Bearer <token>`

## 📋 Management Commands

### Multi-Client Manager

```bash
# List available client configurations
./scripts/qalitrack-manager.sh list

# Generate deployment for a client
./scripts/qalitrack-manager.sh generate testing

# Start services for a client
./scripts/qalitrack-manager.sh start testing

# Stop services (preserve data)
./scripts/qalitrack-manager.sh stop testing

# Stop services and remove data
./scripts/qalitrack-manager.sh stop testing --remove-data

# View service status
./scripts/qalitrack-manager.sh status testing

# View logs
./scripts/qalitrack-manager.sh logs testing
./scripts/qalitrack-manager.sh logs testing user-service
```

### Docker Commands

```bash
# Build and start services
docker compose -f apps/testing/docker-compose.testing.yml up --build

# View logs for all services
docker compose -f apps/testing/docker-compose.testing.yml logs -f

# View logs for specific service
docker compose -f apps/testing/docker-compose.testing.yml logs -f user-service

# Restart a specific service
docker compose -f apps/testing/docker-compose.testing.yml restart user-service
```

## 🏥 Health Monitoring

- **Gateway Health**: http://localhost:7000/health
- **Aggregated Swagger**: http://localhost:7000/api/swagger (integrated into gateway)
- **Individual Service Health**: http://localhost:700X/health (where X is service port)
- **Service Discovery**: http://localhost:7000/api/gateway/services
- **Available Services**: http://localhost:7000/api/swagger/services

## 🛠️ Development

### Technology Stack

- **.NET 8** - Framework
- **Entity Framework Core** - ORM
- **SQLite** - Development database
- **JWT** - Authentication
- **AutoMapper** - Object mapping
- **FluentValidation** - Input validation
- **Serilog** - Logging
- **Swagger/OpenAPI** - API documentation
- **Docker** - Containerization
- **Ocelot** - API Gateway
- **YamlDotNet** - Configuration management

## 🏗️ Project Structure

```
qalitrackservices/
├── packages/                      # Reusable Components & Services
│   ├── qalitrack-gateway/         # API Gateway with integrated Swagger aggregator
│   └── microservices/             # All microservice implementations
│       ├── masterdata/            # Master data services
│       │   ├── user-service/      # Authentication and user management
│       │   ├── organization-service/ # Multi-tenant organizations
│       │   ├── vehicle-service/   # Vehicle master data
│       │   ├── driver-service/    # Driver master data
│       │   ├── product-service/   # Product catalog
│       │   ├── route-service/     # Route management
│       │   ├── weighbridge-service/ # Weighbridge configuration
│       │   ├── customer-service/  # Customer management
│       │   ├── supplier-service/  # Supplier management
│       │   ├── transporter-service/ # Transporter management
│       │   └── sacco-service/     # SACCO management
│       └── datamanager/           # Data management services
│           ├── weight-data-service/ # Weight measurements
│           ├── compliance-service/ # Regulatory compliance
│           ├── operational-data-service/ # Operational metrics
│           ├── transaction-service/ # Business transactions
│           ├── analytics-service/ # Analytics and reporting
│           ├── data-sync-service/ # Data synchronization
│           └── archive-service/   # Data archival
├── apps/                          # Client Application Deployments
│   ├── testing/                   # Testing deployment
│   ├── babumri/                   # Babumri Cement deployment
│   ├── kungu/                     # Kungu Cement deployment
│   └── national-weighing/         # National Weighing Authority deployment
├── configs/                       # Configuration Management
│   └── clients/                   # Client-specific configurations
│       ├── testing.yml            # Testing environment
│       ├── babumri-cement.yml     # Babumri Cement factory
│       ├── kungu-cement.yml       # Kungu Cement factory
│       └── national-weighing.yml  # National Weighing Authority
├── scripts/                       # Management & Build Scripts
│   ├── generate-deployment.py     # Deployment generator
│   ├── qalitrack-manager.sh       # Multi-client manager
│   ├── build-all.sh              # Build all services
│   ├── create-dockerfiles.sh     # Generate Dockerfiles
│   ├── start-services.sh         # Start all services
│   └── stop-services.sh          # Stop services
├── testing/                       # Testing Resources
│   └── gateway-user-test.http     # REST client tests
└── docs/                         # Documentation
    ├── IMPLEMENTATION_SUMMARY.md
    └── ...
```

## 🌟 Multi-Client Configuration

QaliTrack supports different client configurations with varying service compositions:

### Configuration Examples

**Testing Environment** (2 services):
- Gateway + User Service only
- Perfect for development

**Babumri Cement** (15+ services):
- Full factory operations
- Advanced analytics and compliance

**Kungu Cement** (8 services):
- Essential weighbridge operations
- Cost-effective deployment

**National Weighing Authority** (Regulatory):
- Compliance monitoring focus
- Read-only operational access

## Repository Structure

```
├── apps/                    # Applications and Services
│   ├── docs/               # Documentation site (Next.js)
│   ├── web/                # Main web application (Next.js)
│   ├── masterdataservice/  # Master Data Service (.NET/C#)
│   └── userservice/        # User Service (.NET/C#)
├── packages/               # Shared Libraries & Tooling
│   ├── eslint-config/      # Shared ESLint configuration
│   ├── typescript-config/  # Shared TypeScript configuration
│   └── ui/                 # Shared UI components
├── docs/                   # Comprehensive Documentation
│   ├── requirements/       # Business Requirements (BRD)
│   │   ├── masterdataservice/  # Master Data Service requirements
│   │   └── userservice/        # User Service requirements
│   ├── apps/               # Application documentation
│   │   ├── technical/      # Developer documentation
│   │   └── end-users/      # User guides and tutorials
├── turbo.json             # Turborepo configuration
└── pnpm-workspace.yaml    # pnpm workspace configuration
```

## Development Process

### 1. Requirements Phase
- **Location**: `/docs/requirements/`
- **Process**: Business Requirements Documents (BRDs) define functional and non-functional requirements
- **Deliverables**: User stories, acceptance criteria, technical specifications

### 2. Design Phase
- **Location**: `/docs/apps/technical/`
- **Process**: System architecture, API design, database schemas
- **Deliverables**: Architecture diagrams, API specifications, data models

### 3. Development Phase
- **Services**: Built in `/apps/` using C#/.NET (e.g., `/apps/userservice/`)
- **Frontend**: Built in `/apps/web/` using Next.js/React
- **Shared Code**: Utilities and components in `/packages/`

### 4. Documentation Phase
- **Technical Docs**: `/docs/apps/technical/` for developers
- **User Docs**: `/docs/apps/end-users/` for application users
- **Service Docs**: `/docs/services/` for microservice documentation

## Getting Started

### Prerequisites

- **Node.js** >= 18 (for frontend and tooling)
- **pnpm** >= 8 (package manager)
- **.NET SDK** >= 8.0 (for backend services)
- **PostgreSQL** >= 14 (database)
- **Docker** (for containerization)

### Installation

```bash
# Install all dependencies
pnpm install

# Install .NET dependencies (if applicable)
dotnet restore
```

### Development Commands

```bash
# Start all applications in development mode
pnpm dev

# Build all packages and applications
pnpm build

# Run linting across all packages
pnpm lint

# Run type checking
pnpm type-check

# Format code
pnpm format
```

## Turborepo Usage

### Running Specific Apps/Services

```bash
# Start only the web application
pnpm dev --filter=web

# Build only the documentation site
pnpm build --filter=docs

# Run tests for a specific service
pnpm test --filter=userservice
```

### Turborepo Benefits

- **Incremental Builds**: Only rebuild what changed
- **Intelligent Caching**: Cache build results for faster subsequent builds
- **Parallel Execution**: Run tasks across multiple packages simultaneously
- **Dependency-Aware**: Understands package dependencies and build order

### Pipeline Configuration

The `turbo.json` file defines the build pipeline:

```json
{
  "pipeline": {
    "build": {
      "dependsOn": ["^build"],
      "outputs": [".next/**", "dist/**"]
    },
    "dev": {
      "cache": false,
      "persistent": true
    },
    "lint": {
      "dependsOn": ["^lint"]
    }
  }
}
```

## Workspace Management

### Adding New Microservices

```bash
# Create new service directory
mkdir apps/new-service
cd apps/new-service

# Initialize .NET project
dotnet new webapi
dotnet add package Microsoft.EntityFrameworkCore.PostgreSQL

# Add to workspace (automatic due to pnpm-workspace.yaml)
```

### Adding New Frontend Applications

```bash
# Create new Next.js app
mkdir apps/new-app
cd apps/new-app
pnpm create next-app . --typescript --tailwind --eslint

# Configure to use shared packages
pnpm add @qalitrack/ui @qalitrack/eslint-config
```

### Adding Shared Packages

```bash
# Create new shared package
mkdir packages/new-package
cd packages/new-package
pnpm init

# Add dependencies and configure package.json
```

## Documentation Structure

### Requirements Documentation (`/docs/requirements/`)
- [Master Data Service BRD](./docs/requirements/masterdataservice/README.md) - Comprehensive master data management requirements
- [User Service BRD](./docs/requirements/userservice/README.md) - User administration and security requirements
- Business context and problem statements
- Functional and non-functional requirements
- Technical specifications and constraints

### Technical Documentation (`/docs/apps/technical/`)
- System architecture
- Integration guides
- Development procedures
- API documentation

### User Documentation (`/docs/apps/end-users/`)
- User guides and tutorials
- Feature documentation
- FAQ and troubleshooting
- Getting started guides

### Requirements (`/docs/requirements/`)
- [Master Data Service BRD](./docs/requirements/masterdataservice/README.md) - Master data management requirements
- [User Service BRD](./docs/requirements/userservice/README.md) - User administration and security requirements
- Business Requirements Documents (BRD) for all applications
- Functional specifications and user stories
- Technical requirements and acceptance criteria

## Development Standards

### Code Quality
- **ESLint**: Shared configuration in `packages/eslint-config`
- **TypeScript**: Shared configuration in `packages/typescript-config`
- **Prettier**: Consistent code formatting
- **Testing**: Unit and integration tests for all services

### Git Workflow
- Feature branches for new development
- Pull requests for code review
- Automated testing in CI/CD pipeline
- Semantic versioning for releases

### Documentation Standards
- Keep documentation current with code changes
- Include code examples and diagrams
- Provide step-by-step procedures
- Update README files for new packages

## Deployment

### Docker Support
Each service includes Dockerfile for containerization:

```bash
# Build service container
docker build -t qalitrack/userservice ./apps/userservice

# Run with docker-compose
docker-compose up -d
```

### Environment Configuration
- Development: `.env.local`
- Staging: `.env.staging`
- Production: `.env.production`

## Support and Contribution

### Getting Help
- Check `/docs/` for comprehensive documentation
- Review service-specific README files
- Consult API documentation for integration details

### Contributing
1. Review requirements in `/docs/requirements/`
2. Follow development standards and conventions
3. Update documentation for new features
4. Ensure tests pass before submitting pull requests

---

*This monorepo provides a scalable foundation for QaliTrack's microservices architecture, enabling efficient development, testing, and deployment of industrial logistics and compliance solutions.*