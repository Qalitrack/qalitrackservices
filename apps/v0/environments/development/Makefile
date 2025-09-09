# Development Environment Management
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
	@echo "$(BLUE)🛠️  Development Environment Commands$(RESET)"
	@echo ""
	@echo "$(YELLOW)Setup:$(RESET)"
	@echo "  $(GREEN)generate$(RESET)           - Generate missing files from templates"
	@echo "  $(GREEN)FORCE=true generate$(RESET) - Force regenerate all files from templates"
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
	@echo "  $(GREEN)restart$(RESET)            - Restart all services"
	@echo "  $(GREEN)status$(RESET)             - Show service status"
	@echo "  $(GREEN)logs$(RESET)               - Show all logs"
	@echo "  $(GREEN)health$(RESET)             - Run health checks"
	@echo "  $(GREEN)clean$(RESET)              - Clean up volumes and networks"
	@echo ""
	@echo "$(YELLOW)Individual Services:$(RESET)"
	@echo "  $(GREEN)SERVICE=name up-service$(RESET)   - Start specific service"
	@echo "  $(GREEN)SERVICE=name down-service$(RESET) - Stop specific service"
	@echo "  $(GREEN)SERVICE=name logs-service$(RESET) - Show service logs"
	@echo ""
	@echo "$(YELLOW)Backup/Restore:$(RESET)"
	@echo "  $(GREEN)backup-full$(RESET)         - Full backup (all data)"
	@echo "  $(GREEN)backup-incremental$(RESET)  - Incremental backup"
	@echo "  $(GREEN)backup-snapshot$(RESET)     - Docker volume snapshots"
	@echo "  $(GREEN)backup-vault$(RESET)        - Vault secrets backup"
	@echo "  $(GREEN)backup-schedule$(RESET)     - Setup automated backup schedule"
	@echo "  $(GREEN)backup-status$(RESET)       - Show backup schedule status"
	@echo "  $(GREEN)list-backups$(RESET)        - List available backups"
	@echo "  $(GREEN)restore-db$(RESET)          - Restore database backup"
	@echo ""
	@echo "$(YELLOW)Configuration:$(RESET)"
	@echo "  $(GREEN)generate-secrets$(RESET)   - Generate new unique secrets (.env)"
	@echo "  $(GREEN)show-secrets$(RESET)       - Display current secret status"

info:
	@echo "$(BLUE)📋 Development Environment Information$(RESET)"
	@echo "Environment: $(YELLOW)$(ENVIRONMENT)$(RESET)"
	@echo "Project Name: $(YELLOW)$(COMPOSE_PROJECT_NAME)$(RESET)"
	@echo "Network: $(YELLOW)$(NETWORK_SUBNET)$(RESET)"
	@echo ""
	@echo "$(YELLOW)🌐 Service Ports:$(RESET)"
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
	@echo "$(BLUE)🏗️  Generating environment configuration...$(RESET)"
	@if [ ! -f .env ]; then \
		echo "$(YELLOW)📋 Generating unique secrets for development environment...$(RESET)"; \
		../../shared/scripts/generate-secrets.sh development; \
	else \
		echo "$(GREEN)✓$(RESET) Environment secrets already exist"; \
	fi
	@mkdir -p infrastructure applications
	@if [ "$(FORCE)" = "true" ]; then \
		echo "$(YELLOW)⚠️  FORCE=true - Regenerating all infrastructure files...$(RESET)"; \
		for template in ../../shared/templates/infrastructure/*.yml; do \
			filename=$$(basename $$template); \
			echo "  $(GREEN)✓$(RESET) Generating infrastructure/$$filename"; \
			envsubst < $$template > infrastructure/$$filename; \
		done; \
		echo "$(GREEN)✅ Infrastructure files regenerated successfully!$(RESET)"; \
	else \
		files_generated=false; \
		for template in ../../shared/templates/infrastructure/*.yml; do \
			filename=$$(basename $$template); \
			if [ ! -f infrastructure/$$filename ]; then \
				echo "  $(GREEN)✓$(RESET) Generating infrastructure/$$filename"; \
				envsubst < $$template > infrastructure/$$filename; \
				files_generated=true; \
			fi; \
		done; \
		if [ "$$files_generated" = "true" ]; then \
			echo "$(GREEN)✅ Missing infrastructure files generated successfully!$(RESET)"; \
		else \
			echo "$(GREEN)✓$(RESET) All infrastructure files already exist (use FORCE=true to regenerate)"; \
		fi; \
	fi

# Infrastructure commands
infra-up: generate
	@echo "$(BLUE)🚀 Starting development infrastructure services...$(RESET)"
	docker compose $(COMPOSE_FILES) up -d

infra-down:
	@echo "$(BLUE)🛑 Stopping development infrastructure services...$(RESET)"
	docker compose $(COMPOSE_FILES) down

infra-logs:
	@echo "$(BLUE)📋 Showing development infrastructure logs...$(RESET)"
	docker compose $(COMPOSE_FILES) logs -f

vault-init:
	@echo "$(BLUE)🔐 Initializing Vault secrets...$(RESET)"
	@sleep 3  # Wait for Vault to be ready
	docker compose $(INIT_FILES) up vault-init

# Application commands
apps-up: infra-up
	@echo "$(BLUE)🚀 Starting development application services...$(RESET)"
	@echo "$(YELLOW)⏳ Waiting 10 seconds for infrastructure to be ready...$(RESET)"
	@sleep 10
	docker compose $(ALL_FILES) up -d

apps-down:
	@echo "$(BLUE)🛑 Stopping development application services...$(RESET)"
	docker compose $(APP_FILES) down

apps-logs:
	@echo "$(BLUE)📋 Showing development application logs...$(RESET)"
	docker compose $(APP_FILES) logs -f

# Full environment commands
up: infra-up vault-init apps-up
	@echo "$(GREEN)✅ Development environment started successfully!$(RESET)"
	@echo ""
	@$(MAKE) info

down:
	@echo "$(BLUE)🛑 Stopping complete development environment...$(RESET)"
	docker compose $(ALL_FILES) -f infrastructure/99-secrets-init.yml down

restart: down up

status:
	@echo "$(BLUE)📊 Development Environment Status$(RESET)"
	@docker compose $(ALL_FILES) ps --format table

logs:
	@echo "$(BLUE)📋 Showing all development logs...$(RESET)"
	docker compose $(ALL_FILES) logs -f --tail=100

clean: down
	@echo "$(BLUE)🧹 Cleaning up development environment...$(RESET)"
	docker compose $(ALL_FILES) -f infrastructure/99-secrets-init.yml down -v
	docker system prune -f

# Individual service commands
up-service:
	@if [ -z "$(SERVICE)" ]; then echo "$(RED)❌ Usage: make SERVICE=name up-service$(RESET)"; exit 1; fi
	@echo "$(BLUE)🚀 Starting service: $(YELLOW)$(SERVICE)$(RESET)"
	docker compose $(ALL_FILES) up -d $(SERVICE)

down-service:
	@if [ -z "$(SERVICE)" ]; then echo "$(RED)❌ Usage: make SERVICE=name down-service$(RESET)"; exit 1; fi
	@echo "$(BLUE)🛑 Stopping service: $(YELLOW)$(SERVICE)$(RESET)"
	docker compose $(ALL_FILES) stop $(SERVICE)

logs-service:
	@if [ -z "$(SERVICE)" ]; then echo "$(RED)❌ Usage: make SERVICE=name logs-service$(RESET)"; exit 1; fi
	@echo "$(BLUE)📋 Showing logs for service: $(YELLOW)$(SERVICE)$(RESET)"
	docker compose $(ALL_FILES) logs -f $(SERVICE)

# Health checks
health:
	@echo "$(BLUE)🏥 Development Environment Health Check$(RESET)"
	@echo ""
	@echo -n "Vault: "
	@if curl -s http://localhost:$(VAULT_PORT)/v1/sys/health | grep -q '"initialized":true'; then echo "$(GREEN)✅ Healthy$(RESET)"; else echo "$(RED)❌ Unhealthy$(RESET)"; fi
	@echo -n "Mailpit: "
	@if curl -s http://localhost:$(MAILPIT_WEB_PORT)/api/v1/messages > /dev/null 2>&1; then echo "$(GREEN)✅ Healthy$(RESET)"; else echo "$(RED)❌ Unhealthy$(RESET)"; fi
	@echo -n "PostgreSQL Main: "
	@if docker compose $(COMPOSE_FILES) exec -T postgres pg_isready -U $(POSTGRES_ADMIN_USER) > /dev/null 2>&1; then echo "$(GREEN)✅ Healthy$(RESET)"; else echo "$(RED)❌ Unhealthy$(RESET)"; fi
	@echo -n "Redis: "
	@if docker compose $(COMPOSE_FILES) exec -T redis redis-cli --no-auth-warning -a $(REDIS_ADMIN_PASSWORD) ping | grep -q PONG; then echo "$(GREEN)✅ Healthy$(RESET)"; else echo "$(RED)❌ Unhealthy$(RESET)"; fi
	@echo ""
	@echo "$(YELLOW)📊 Container Status:$(RESET)"
	@docker compose $(ALL_FILES) ps --format table

# Comprehensive Backup System
backup-full:
	@echo "$(BLUE)💾 Creating full backup for development environment...$(RESET)"
	@../../shared/scripts/backup-system.sh development full --compress --verify

backup-incremental:
	@echo "$(BLUE)💾 Creating incremental backup for development environment...$(RESET)"
	@../../shared/scripts/backup-system.sh development incremental --compress

backup-snapshot:
	@echo "$(BLUE)💾 Creating volume snapshots for development environment...$(RESET)"
	@../../shared/scripts/backup-system.sh development snapshot

backup-vault:
	@echo "$(BLUE)💾 Backing up Vault secrets for development environment...$(RESET)"
	@../../shared/scripts/backup-system.sh development vault

backup-schedule:
	@echo "$(BLUE)⏰ Setting up automated backup schedule for development...$(RESET)"
	@../../shared/scripts/setup-backup-schedule.sh development

backup-status:
	@echo "$(BLUE)📊 Showing backup schedule status for development...$(RESET)"
	@../../shared/scripts/setup-backup-schedule.sh development --status

restore-db:
	@if [ -z "$(FILE)" ]; then echo "$(RED)❌ Usage: make restore-db FILE=backup.sql$(RESET)"; exit 1; fi
	@echo "$(BLUE)🔄 Restoring database from: $(YELLOW)$(FILE)$(RESET)"
	@echo "$(RED)⚠️  This will overwrite existing data. Continue? [y/N]$(RESET)" && read ans && [ $${ans:-N} = y ]
	@docker compose $(COMPOSE_FILES) exec -T postgres psql -U $(POSTGRES_ADMIN_USER) microservices < $(FILE)
	@echo "$(GREEN)✅ Database restored successfully$(RESET)"

list-backups:
	@echo "$(BLUE)📋 Available development backups:$(RESET)"
	@if [ -d "$(BACKUP_PATH)" ]; then ls -la $(BACKUP_PATH)/; else echo "$(YELLOW)No backups found$(RESET)"; fi

# Configuration management
generate-secrets:
	@if [ "$(FORCE)" = "true" ]; then \
		echo "$(BLUE)🔐 Generating new unique secrets for development environment...$(RESET)"; \
		echo "$(YELLOW)⚠️  FORCE=true - Bypassing confirmation$(RESET)"; \
		../../shared/scripts/generate-secrets.sh development --force; \
	elif [ -f .env ]; then \
		echo "$(BLUE)🔐 Regenerating unique secrets for development environment...$(RESET)"; \
		echo "$(RED)⚠️  This will overwrite existing secrets in .env$(RESET)"; \
		echo -n "$(YELLOW)Are you sure you want to continue? [y/N]: $(RESET)" && read ans && [ $${ans:-N} = y ]; \
		../../shared/scripts/generate-secrets.sh development --force; \
	else \
		echo "$(BLUE)🔐 Generating initial secrets for development environment...$(RESET)"; \
		../../shared/scripts/generate-secrets.sh development; \
	fi
	@echo "$(GREEN)✅ Secrets generated. Restart services to apply changes.$(RESET)"

show-secrets:
	@echo "$(BLUE)📋 Development Environment Secret Status$(RESET)"
	@if [ -f .env ]; then \
		echo "$(GREEN)✓$(RESET) .env file exists"; \
		echo "$(YELLOW)Created:$(RESET) $$(stat -c %y .env 2>/dev/null || stat -f %Sm .env 2>/dev/null || echo 'Unknown')"; \
		echo "$(YELLOW)Vault Token:$(RESET) $$(grep VAULT_ROOT_TOKEN .env | cut -d'=' -f2 | head -c 8)..."; \
		echo "$(YELLOW)Secret Count:$(RESET) $$(grep -c 'PASSWORD\|TOKEN' .env) credentials"; \
	else \
		echo "$(RED)❌ .env file not found$(RESET)"; \
		echo "$(YELLOW)Run 'make generate' to create environment configuration$(RESET)"; \
	fi