# 🚀 QaliTrack Multi-Environment Management

Complete multi-environment infrastructure management with automated deployment, backup/restore capabilities, and CI/CD integration.

## 📁 Directory Structure

```
/
├── environments/                    # Environment-specific configurations
│   ├── development/                # Development environment (standard ports)
│   │   ├── infrastructure/         # Generated infrastructure configs
│   │   ├── applications/           # Application service configs
│   │   ├── .env                   # Development environment variables
│   │   └── Makefile               # Development management commands
│   │
│   └── production/                 # Production environment (offset ports)
│       ├── infrastructure/         # Generated infrastructure configs
│       ├── applications/           # Application service configs  
│       ├── .env                   # Production environment variables
│       └── Makefile               # Production management commands
│
├── shared/                         # Shared templates and scripts
│   ├── templates/
│   │   └── infrastructure/         # Infrastructure templates
│   └── scripts/
│       └── generate-env.sh         # Environment generation script
│
├── infrastructure/                 # Legacy (can be removed)
├── Makefile                       # Root environment controller
└── README-ENVIRONMENTS.md         # This file
```

## 🛠️ Quick Start

### 1. Generate Environment Files
```bash
# Generate development environment
make dev generate

# Generate production environment  
make prod generate
```

### 2. Start Environment
```bash
# Development
make dev up               # Start complete development environment
make dev infra-up         # Start only infrastructure
make dev apps-up          # Start only applications

# Production
make prod up              # Start complete production environment (with safety prompts)
make prod infra-up        # Start only infrastructure
```

### 3. Monitor and Control
```bash
# Status and logs
make dev status           # Show service status
make dev logs             # Show all logs
make dev health           # Run health checks

# Individual services
make dev SERVICE=postgres logs-service    # View specific service logs
make dev SERVICE=vault up-service         # Start specific service
```

## 🌐 Environment Configuration

### Development Environment
- **Ports**: Standard (5432, 6379, 8200, etc.)
- **Network**: `172.20.0.0/16`
- **Project**: `qalitrack-dev`
- **Safety**: No confirmation prompts
- **Backups**: 7-day retention

### Production Environment  
- **Ports**: Offset by +10000 (15432, 16379, 18200, etc.)
- **Network**: `172.30.0.0/16`
- **Project**: `qalitrack-prod`
- **Safety**: Confirmation prompts for dangerous operations
- **Automation**: Use `FORCE=true` to bypass prompts for CI/CD
- **Backups**: 30-day retention, compressed

## 📋 Available Commands

### Root Level Commands
```bash
make help                 # Show all available commands
make dev [target]         # Run target in development
make prod [target]        # Run target in production
make ENV=development up   # Alternative syntax
```

### Environment Management
```bash
# Full Environment
up                       # Start complete environment
down                     # Stop all services  
restart                  # Restart all services
status                   # Show service status
logs                     # Show all logs
health                   # Run health checks
clean                    # Clean up volumes (DANGEROUS in prod)

# Infrastructure Only
infra-up                 # Start infrastructure services
infra-down               # Stop infrastructure services
infra-logs               # Show infrastructure logs
vault-init               # Initialize Vault secrets

# Applications Only  
apps-up                  # Start application services
apps-down                # Stop application services
apps-logs                # Show application logs

# Individual Services
SERVICE=name up-service   # Start specific service
SERVICE=name down-service # Stop specific service
SERVICE=name logs-service # Show service logs
```

### Backup & Restore
```bash
# Backup Operations
backup-db                # Backup PostgreSQL databases
backup-vault             # Export Vault secrets (encrypted)
backup-all               # Complete environment backup
list-backups             # List available backups

# Restore Operations (DANGEROUS)
restore-db FILE=backup.sql   # Restore database backup
```

## 🔒 Security Features

### Development Environment
- Standard security for local development
- Simple passwords for convenience
- No confirmation prompts for speed

### Production Environment
- **Safety Prompts**: Confirmation required for dangerous operations
- **Strong Passwords**: Must be changed from defaults
- **Operation Logging**: All commands logged
- **Backup Automation**: Automated daily backups
- **Access Control**: Separate network and ports

## 🏥 Health Monitoring

### Automated Health Checks
```bash
make dev health          # Check all development services
make prod health         # Check all production services
```

**Health Check Coverage:**
- ✅ Vault API accessibility and initialization status
- ✅ PostgreSQL database connectivity
- ✅ Redis cache connectivity  
- ✅ Mailpit email service
- ✅ Container status and resource usage

### Manual Health Verification
```bash
# Development URLs
curl http://localhost:8200/v1/sys/health     # Vault
curl http://localhost:8025/api/v1/messages   # Mailpit

# Production URLs (offset ports)
curl http://localhost:18200/v1/sys/health    # Vault
curl http://localhost:18025/api/v1/messages  # Mailpit
```

## 🤖 CI/CD Integration

### GitHub Actions Example
```yaml
name: Deploy QaliTrack
on:
  push:
    branches: [main]

jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Deploy to Development
        run: |
          make dev down
          make dev up
          make dev health
          
      - name: Run Tests
        run: make dev test
        
      - name: Backup Production
        if: github.ref == 'refs/heads/main'
        run: make FORCE=true prod backup-all
        
      - name: Deploy to Production
        if: github.ref == 'refs/heads/main'
        run: |
          make FORCE=true prod down
          make FORCE=true prod up
          make prod health
```

### Docker Compose Integration
Each environment generates its own isolated Docker Compose setup:
- **Networks**: Separate networks per environment
- **Volumes**: Environment-prefixed volume names
- **Containers**: Environment-prefixed container names
- **Ports**: Non-conflicting port assignments

## 📦 Service Ports

### Development (Standard)
| Service | Port | Management |
|---------|------|------------|
| PostgreSQL Main | 5432 | - |
| PostgreSQL Backup | 5435 | - |
| Redis | 6379 | - |
| RabbitMQ | 5672 | 15672 |
| Vault | 8200 | - |
| MinIO | 9000 | 9001 |
| Mailpit | 1025 | 8025 |
| ZincSearch | 4080 | - |

### Production (+10000 Offset)
| Service | Port | Management |
|---------|------|------------|
| PostgreSQL Main | 15432 | - |
| PostgreSQL Backup | 15435 | - |
| Redis | 16379 | - |
| RabbitMQ | 15672 | 25672 |
| Vault | 18200 | - |
| MinIO | 19000 | 19001 |
| Mailpit | 11025 | 18025 |
| ZincSearch | 14080 | - |

## 🔧 Customization

### Adding New Environments
1. Create new environment directory: `environments/staging/`
2. Copy `.env` from existing environment
3. Update ports and configuration
4. Copy Makefile and update prompts/settings
5. Add environment to root `Makefile` validation

### Creating New Templates
1. Add template to `shared/templates/infrastructure/`
2. Use environment variables: `${VARIABLE_NAME}`
3. Test with: `./shared/scripts/generate-env.sh development`

### Custom Backup Strategies
Edit environment-specific `Makefile` backup functions:
- `backup-db`: Database backup logic
- `backup-vault`: Vault export logic  
- `backup-all`: Combined backup orchestration

## 🚨 Important Notes

### Production Safety
- **NEVER** run `make prod clean` unless you want to DELETE ALL DATA
- Always run `make prod backup-all` before major operations
- Production operations require confirmation prompts
- Change default passwords in `production/.env`

### Development Speed
- Development has no safety prompts for faster iteration
- Use `make dev clean` freely to reset state
- Infrastructure files are regenerated on each `make dev up`

### Network Isolation
- Each environment uses separate Docker networks
- No port conflicts between environments
- Can run development and production simultaneously

## 📈 Monitoring Integration

### Prometheus Metrics (Future)
- Container resource usage
- Service health status
- Backup success/failure rates
- Environment-specific dashboards

### Log Aggregation (Future)
- Centralized logging per environment
- Log retention policies
- Alert integration

---

**🎉 Your QaliTrack infrastructure is now ready for multi-environment deployment!**

Start with: `make dev up` for development or `make prod up` for production.