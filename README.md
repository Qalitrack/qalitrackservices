# QaliTrack Services Monorepo

A Turborepo-powered monorepo for QaliTrack microservices and applications, providing a comprehensive platform for industrial weighbridge operations, logistics management, and regulatory compliance.

## What is QaliTrack?

QaliTrack is an integrated platform that manages:
- **Master Data Management**: Centralized data for vehicles, suppliers, drivers, products, routes, and weighbridges
- **User Administration**: Comprehensive user management, authentication, and role-based access control
- **Operational Workflows**: End-to-end logistics and compliance processes
- **Regulatory Compliance**: Audit trails, reporting, and regulatory data management

## Technology Stack

- **Monorepo**: Turborepo for build orchestration and caching
- **Package Manager**: pnpm with workspaces
- **Backend Services**: C#/.NET microservices
- **Database**: PostgreSQL
- **Authentication**: JWT-based security
- **Frontend**: React/Next.js applications
- **Containerization**: Docker
- **Documentation**: Comprehensive docs with technical and user guides

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