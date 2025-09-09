# Production Environment Management
include .env
export

# Colors for output
RED    := \033[31m
GREEN  := \033[32m
YELLOW := \033[33m
BLUE   := \033[34m
RESET  := \033[0m

# Docker Compose file groups
COMPOSE_FILES = -f infrastructure/00-volumes-networks.yml \
                -f infrastructure/01-databases.yml \
                -f infrastructure/02-cache-messaging.yml \
                -f infrastructure/03-storage-search.yml \
                -f infrastructure/04-secrets.yml \
                -f infrastructure/05-email.yml \
                -f infrastructure/06-nginx.yml \
                -f infrastructure/07-microservices-control-hub.yml

APP_FILES = -f applications/qalitrack-masterdata.yml \
            -f applications/transaction-service.yml \
            -f applications/weight-data-service.yml \
            -f applications/backup-service.yml

ALL_FILES = $(COMPOSE_FILES) $(APP_FILES)
INIT_FILES = $(COMPOSE_FILES) -f infrastructure/99-secrets-init.yml

.PHONY: help info generate up down restart status logs clean health

help:
	@echo "$(BLUE)🏭 Production Environment Commands$(RESET)"
	@echo ""
	@echo "$(RED)⚠️  PRODUCTION ENVIRONMENT - Use with caution!$(RESET)"
	@echo ""
	@echo "$(YELLOW)Setup:$(RESET)"
	@echo "  $(GREEN)generate$(RESET)           - Generate files from templates"
	@echo "  $(GREEN)info$(RESET)               - Show environment information"
	@echo ""
	@echo "$(YELLOW)Infrastructure:$(RESET)"
	@echo "  $(GREEN)infra-up$(RESET)           - Start infrastructure services only"
	@echo "  $(GREEN)infra-down$(RESET)         - Stop infrastructure services"
	@echo "  $(GREEN)infra-logs$(RESET)         - Show infrastructure logs"
	@echo "  $(GREEN)vault-init$(RESET)         - Initialize Vault secrets"
	@echo ""
	@echo "$(YELLOW)Applications:$(RESET)"
	@echo "  $(GREEN)apps-up$(RESET)            - Start application services"
	@echo "  $(GREEN)apps-down$(RESET)          - Stop application services"
	@echo "  $(GREEN)apps-logs$(RESET)          - Show application logs"
	@echo ""
	@echo "$(YELLOW)Full Environment:$(RESET)"
	@echo "  $(GREEN)up$(RESET)                 - Start complete environment"
	@echo "  $(GREEN)down$(RESET)               - Stop all services"
	@echo "  $(GREEN)restart$(RESET)            - Restart all services ($(RED)DANGEROUS$(RESET))"
	@echo "  $(GREEN)status$(RESET)             - Show service status"
	@echo "  $(GREEN)logs$(RESET)               - Show all logs"
	@echo "  $(GREEN)health$(RESET)             - Run health checks"
	@echo "  $(GREEN)clean$(RESET)              - Clean up volumes ($(RED)DANGEROUS$(RESET))"
	@echo ""
	@echo "$(YELLOW)Individual Services:$(RESET)"
	@echo "  $(GREEN)SERVICE=name up-service$(RESET)   - Start specific service"
	@echo "  $(GREEN)SERVICE=name down-service$(RESET) - Stop specific service"
	@echo "  $(GREEN)SERVICE=name logs-service$(RESET) - Show service logs"
	@echo ""
	@echo "$(YELLOW)Backup/Restore (Critical for Production):$(RESET)"
	@echo "  $(GREEN)backup-full$(RESET)         - Full backup (encrypted, external)"
	@echo "  $(GREEN)backup-incremental$(RESET)  - Incremental backup (external)"
	@echo "  $(GREEN)backup-snapshot$(RESET)     - Docker volume snapshots"
	@echo "  $(GREEN)backup-vault$(RESET)        - Vault secrets backup (encrypted)"
	@echo "  $(GREEN)backup-schedule$(RESET)     - Setup automated backup schedule"
	@echo "  $(GREEN)backup-status$(RESET)       - Show backup schedule status"
	@echo "  $(GREEN)list-backups$(RESET)        - List available backups"
	@echo "  $(GREEN)restore-db$(RESET)          - Restore database backup ($(RED)DANGEROUS$(RESET))"
	@echo ""
	@echo "$(YELLOW)Configuration:$(RESET)"
	@echo "  $(GREEN)generate-secrets$(RESET)   - Generate new unique secrets (.env)"
	@echo "  $(GREEN)show-secrets$(RESET)       - Display current secret status"
	@echo ""
	@echo "$(YELLOW)Automation Support:$(RESET)"
	@echo "  $(GREEN)FORCE=true [target]$(RESET)     - Bypass safety prompts for CI/CD"
	@echo "  Example: $(GREEN)make FORCE=true infra-up$(RESET)"

info:
	@echo "$(BLUE)📋 Production Environment Information$(RESET)"
	@echo "Environment: $(RED)$(ENVIRONMENT)$(RESET)"
	@echo "Project Name: $(YELLOW)$(COMPOSE_PROJECT_NAME)$(RESET)"
	@echo "Network: $(YELLOW)$(NETWORK_SUBNET)$(RESET)"
	@echo ""
	@echo "$(YELLOW)🌐 Service Ports (Production Offset):$(RESET)"
	@echo "  PostgreSQL Main: $(GREEN)$(POSTGRES_PORT)$(RESET)"
	@echo "  PostgreSQL Backup: $(GREEN)$(POSTGRES_BACKUP_PORT)$(RESET)"
	@echo "  Redis: $(GREEN)$(REDIS_PORT)$(RESET)"
	@echo "  RabbitMQ: $(GREEN)$(RABBITMQ_PORT)$(RESET) / Management: $(GREEN)$(RABBITMQ_MGMT_PORT)$(RESET)"
	@echo "  Vault: $(GREEN)$(VAULT_PORT)$(RESET)"
	@echo "  MinIO: $(GREEN)$(MINIO_PORT)$(RESET) / Console: $(GREEN)$(MINIO_CONSOLE_PORT)$(RESET)"
	@echo "  Mailpit: $(GREEN)$(MAILPIT_WEB_PORT)$(RESET) / SMTP: $(GREEN)$(MAILPIT_SMTP_PORT)$(RESET)"
	@echo "  ZincSearch: $(GREEN)$(ZINCSEARCH_PORT)$(RESET)"
	@echo ""
	@echo "$(YELLOW)🚀 Application Ports:$(RESET)"
	@echo "  MasterData: $(GREEN)$(MASTERDATA_PORT)$(RESET)"
	@echo "  Transaction: $(GREEN)$(TRANSACTION_PORT)$(RESET)"
	@echo "  Weight Data: $(GREEN)$(WEIGHT_DATA_PORT)$(RESET)"
	@echo "  Backup Service: $(GREEN)$(BACKUP_SERVICE_PORT)$(RESET)"

generate:
	@echo "$(BLUE)🏗️  Generating production environment configuration...$(RESET)"
	@if [ ! -f .env ]; then \
		echo "$(YELLOW)📋 Generating unique secrets for production environment...$(RESET)"; \
		../../shared/scripts/generate-secrets.sh production; \
	else \
		echo "$(GREEN)✓$(RESET) Environment secrets already exist"; \
	fi
	@mkdir -p infrastructure applications
	@echo "$(BLUE)🏗️  Generating production infrastructure files from templates...$(RESET)"
	@for template in ../../shared/templates/infrastructure/*.yml; do \
		filename=$$(basename $$template); \
		echo "  $(GREEN)✓$(RESET) Generating infrastructure/$$filename"; \
		envsubst < $$template > infrastructure/$$filename; \
	done
	@echo "$(GREEN)✅ Production infrastructure files generated successfully!$(RESET)"

# Production safety checks
production-warning:
	@if [ "$(FORCE)" = "true" ]; then \
		echo "$(YELLOW)⚠️  PRODUCTION OPERATION - Safety check bypassed with FORCE=true$(RESET)"; \
	else \
		echo "$(RED)⚠️  PRODUCTION ENVIRONMENT OPERATION$(RESET)"; \
		echo "$(YELLOW)This will affect the production environment.$(RESET)"; \
		echo "$(YELLOW)Use FORCE=true to bypass this prompt in automated environments.$(RESET)"; \
		echo -n "$(YELLOW)Are you sure you want to continue? [y/N]: $(RESET)" && read ans && [ $${ans:-N} = y ]; \
	fi

# Infrastructure commands  
infra-up: production-warning generate
	@echo "$(BLUE)🚀 Starting production infrastructure services...$(RESET)"
	docker compose $(COMPOSE_FILES) up -d

infra-down: production-warning
	@echo "$(BLUE)🛑 Stopping production infrastructure services...$(RESET)"
	docker compose $(COMPOSE_FILES) down

infra-logs:
	@echo "$(BLUE)📋 Showing production infrastructure logs...$(RESET)"
	docker compose $(COMPOSE_FILES) logs -f

vault-init: production-warning
	@echo "$(BLUE)🔐 Initializing production Vault secrets...$(RESET)"
	@sleep 5  # Wait for Vault to be ready
	docker compose $(INIT_FILES) up vault-init

# Application commands
apps-up: infra-up
	@echo "$(BLUE)🚀 Starting production application services...$(RESET)"
	@echo "$(YELLOW)⏳ Waiting 15 seconds for infrastructure to be ready...$(RESET)"
	@sleep 15
	docker compose $(ALL_FILES) up -d

apps-down: production-warning
	@echo "$(BLUE)🛑 Stopping production application services...$(RESET)"
	docker compose $(APP_FILES) down

apps-logs:
	@echo "$(BLUE)📋 Showing production application logs...$(RESET)"
	docker compose $(APP_FILES) logs -f

# Full environment commands
up: infra-up vault-init apps-up
	@echo "$(GREEN)✅ Production environment started successfully!$(RESET)"
	@echo ""
	@$(MAKE) info

down: production-warning
	@echo "$(BLUE)🛑 Stopping complete production environment...$(RESET)"
	docker compose $(ALL_FILES) -f infrastructure/99-secrets-init.yml down

restart: production-warning down up

status:
	@echo "$(BLUE)📊 Production Environment Status$(RESET)"
	@docker compose $(ALL_FILES) ps --format table

logs:
	@echo "$(BLUE)📋 Showing all production logs...$(RESET)"
	docker compose $(ALL_FILES) logs -f --tail=100

clean: production-warning
	@echo "$(RED)⚠️  This will DELETE all production data!$(RESET)"
	@echo -n "$(RED)Type 'DELETE' to confirm: $(RESET)" && read ans && [ "$$ans" = "DELETE" ]
	@echo "$(BLUE)🧹 Cleaning up production environment...$(RESET)"
	docker compose $(ALL_FILES) -f infrastructure/99-secrets-init.yml down -v
	docker system prune -f

# Individual service commands
up-service: production-warning
	@if [ -z "$(SERVICE)" ]; then echo "$(RED)❌ Usage: make SERVICE=name up-service$(RESET)"; exit 1; fi
	@echo "$(BLUE)🚀 Starting production service: $(YELLOW)$(SERVICE)$(RESET)"
	docker compose $(ALL_FILES) up -d $(SERVICE)

down-service: production-warning
	@if [ -z "$(SERVICE)" ]; then echo "$(RED)❌ Usage: make SERVICE=name down-service$(RESET)"; exit 1; fi
	@echo "$(BLUE)🛑 Stopping production service: $(YELLOW)$(SERVICE)$(RESET)"
	docker compose $(ALL_FILES) stop $(SERVICE)

logs-service:
	@if [ -z "$(SERVICE)" ]; then echo "$(RED)❌ Usage: make SERVICE=name logs-service$(RESET)"; exit 1; fi
	@echo "$(BLUE)📋 Showing logs for production service: $(YELLOW)$(SERVICE)$(RESET)"
	docker compose $(ALL_FILES) logs -f $(SERVICE)

# Health checks
health:
	@echo "$(BLUE)🏥 Production Environment Health Check$(RESET)"
	@echo ""
	@echo -n "Vault: "
	@if curl -s http://localhost:$(VAULT_PORT)/v1/sys/health | grep -q '"initialized":true'; then echo "$(GREEN)✅ Healthy$(RESET)"; else echo "$(RED)❌ Unhealthy$(RESET)"; fi
	@echo -n "Mailpit: "
	@if curl -s http://localhost:$(MAILPIT_WEB_PORT)/api/v1/messages > /dev/null 2>&1; then echo "$(GREEN)✅ Healthy$(RESET)"; else echo "$(RED)❌ Unhealthy$(RESET)"; fi
	@echo -n "PostgreSQL Main: "
	@if docker compose $(COMPOSE_FILES) exec -T postgres pg_isready -U $(POSTGRES_ADMIN_USER) > /dev/null 2>&1; then echo "$(GREEN)✅ Healthy$(RESET)"; else echo "$(RED)❌ Unhealthy$(RESET)"; fi
	@echo ""
	@echo "$(YELLOW)📊 Container Status:$(RESET)"
	@docker compose $(ALL_FILES) ps --format table

# Comprehensive Backup System (Production-Grade)
backup-full:
	@echo "$(BLUE)💾 Creating full production backup (encrypted, external)...$(RESET)"
	@../../shared/scripts/backup-system.sh production full --compress --encrypt --external --verify

backup-incremental:
	@echo "$(BLUE)💾 Creating incremental production backup (external)...$(RESET)"
	@../../shared/scripts/backup-system.sh production incremental --compress --external

backup-snapshot:
	@echo "$(BLUE)💾 Creating production volume snapshots (external)...$(RESET)"
	@../../shared/scripts/backup-system.sh production snapshot --compress --external

backup-vault:
	@echo "$(BLUE)💾 Backing up production Vault secrets (encrypted, external)...$(RESET)"
	@../../shared/scripts/backup-system.sh production vault --encrypt --external

backup-schedule:
	@echo "$(BLUE)⏰ Setting up automated production backup schedule...$(RESET)"
	@../../shared/scripts/setup-backup-schedule.sh production

backup-status:
	@echo "$(BLUE)📊 Showing production backup schedule status...$(RESET)"
	@../../shared/scripts/setup-backup-schedule.sh production --status

restore-db: production-warning
	@if [ -z "$(FILE)" ]; then echo "$(RED)❌ Usage: make restore-db FILE=backup.sql$(RESET)"; exit 1; fi
	@echo "$(RED)⚠️  PRODUCTION DATABASE RESTORE$(RESET)"
	@echo "$(RED)This will OVERWRITE production data with: $(YELLOW)$(FILE)$(RESET)"
	@echo -n "$(RED)Type 'RESTORE' to confirm: $(RESET)" && read ans && [ "$$ans" = "RESTORE" ]
	@docker compose $(COMPOSE_FILES) exec -T postgres psql -U $(POSTGRES_ADMIN_USER) microservices < $(FILE)
	@echo "$(GREEN)✅ Production database restored$(RESET)"

list-backups:
	@echo "$(BLUE)📋 Available production backups:$(RESET)"
	@if [ -d "$(BACKUP_PATH)" ]; then ls -la $(BACKUP_PATH)/; else echo "$(YELLOW)No backups found$(RESET)"; fi

# Configuration management (with production safety)
generate-secrets:
	@if [ "$(FORCE)" = "true" ]; then \
		echo "$(BLUE)🔐 Generating new unique secrets for production environment...$(RESET)"; \
		echo "$(YELLOW)⚠️  FORCE=true - Bypassing confirmation$(RESET)"; \
		../../shared/scripts/generate-secrets.sh production --force; \
	elif [ -f .env ]; then \
		echo "$(RED)⚠️  PRODUCTION ENVIRONMENT OPERATION$(RESET)"; \
		echo "$(YELLOW)This will overwrite existing production secrets in .env$(RESET)"; \
		echo "$(RED)⚠️  All services will need to be restarted with new credentials$(RESET)"; \
		echo -n "$(YELLOW)Are you sure you want to continue? [y/N]: $(RESET)" && read ans && [ $${ans:-N} = y ]; \
		echo "$(BLUE)🔐 Regenerating production secrets...$(RESET)"; \
		../../shared/scripts/generate-secrets.sh production --force; \
	else \
		echo "$(BLUE)🔐 Generating initial secrets for production environment...$(RESET)"; \
		../../shared/scripts/generate-secrets.sh production; \
	fi
	@echo "$(GREEN)✅ Production secrets generated.$(RESET)"
	@echo "$(YELLOW)⚠️  Restart all services to apply new credentials$(RESET)"

show-secrets:
	@echo "$(BLUE)📋 Production Environment Secret Status$(RESET)"
	@if [ -f .env ]; then \
		echo "$(GREEN)✓$(RESET) .env file exists"; \
		echo "$(YELLOW)Created:$(RESET) $$(stat -c %y .env 2>/dev/null || stat -f %Sm .env 2>/dev/null || echo 'Unknown')"; \
		echo "$(YELLOW)Vault Token:$(RESET) $$(grep VAULT_ROOT_TOKEN .env | cut -d'=' -f2 | head -c 8)..."; \
		echo "$(YELLOW)Secret Count:$(RESET) $$(grep -c 'PASSWORD\|TOKEN' .env) credentials"; \
		echo "$(RED)⚠️  Production secrets - handle with care$(RESET)"; \
	else \
		echo "$(RED)❌ .env file not found$(RESET)"; \
		echo "$(YELLOW)Run 'make generate' to create environment configuration$(RESET)"; \
	fi