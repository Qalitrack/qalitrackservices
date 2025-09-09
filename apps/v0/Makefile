# QaliTrack Multi-Environment Management
# Usage: make [ENV=env] [target]

ENV ?= development
VALID_ENVS := development production

# Colors for output
RED    := \033[31m
GREEN  := \033[32m
YELLOW := \033[33m
BLUE   := \033[34m
RESET  := \033[0m

.PHONY: help dev prod validate-env

help:
	@echo "$(BLUE)🚀 QaliTrack Multi-Environment Management$(RESET)"
	@echo ""
	@echo "$(YELLOW)Environments:$(RESET) development, production"
	@echo ""
	@echo "$(YELLOW)Quick Commands:$(RESET)"
	@echo "  $(GREEN)make dev [target]$(RESET)     - Run target in development environment"
	@echo "  $(GREEN)make prod [target]$(RESET)    - Run target in production environment"
	@echo ""
	@echo "$(YELLOW)Infrastructure:$(RESET)"
	@echo "  $(GREEN)infra-up$(RESET)             - Start infrastructure services only"
	@echo "  $(GREEN)infra-down$(RESET)           - Stop infrastructure services"
	@echo "  $(GREEN)vault-init$(RESET)           - Initialize Vault secrets"
	@echo ""
	@echo "$(YELLOW)Applications:$(RESET)"
	@echo "  $(GREEN)apps-up$(RESET)              - Start application services"
	@echo "  $(GREEN)apps-down$(RESET)            - Stop application services"
	@echo ""
	@echo "$(YELLOW)Full Environment:$(RESET)"
	@echo "  $(GREEN)up$(RESET)                   - Start complete environment"
	@echo "  $(GREEN)down$(RESET)                 - Stop all services"
	@echo "  $(GREEN)restart$(RESET)              - Restart all services"
	@echo "  $(GREEN)status$(RESET)               - Show service status"
	@echo "  $(GREEN)logs$(RESET)                 - Show all logs"
	@echo "  $(GREEN)health$(RESET)               - Run health checks"
	@echo "  $(GREEN)clean$(RESET)                - Clean up volumes and networks"
	@echo ""
	@echo "$(YELLOW)Individual Services:$(RESET)"
	@echo "  $(GREEN)SERVICE=name up-service$(RESET)   - Start specific service"
	@echo "  $(GREEN)SERVICE=name logs-service$(RESET) - Show service logs"
	@echo ""
	@echo "$(YELLOW)Backup/Restore:$(RESET)"
	@echo "  $(GREEN)backup-db$(RESET)            - Backup PostgreSQL databases"
	@echo "  $(GREEN)backup-vault$(RESET)         - Export Vault secrets"
	@echo "  $(GREEN)backup-all$(RESET)           - Complete environment backup"
	@echo "  $(GREEN)restore-db$(RESET)           - Restore database backup"
	@echo ""
	@echo "$(YELLOW)Examples:$(RESET)"
	@echo "  $(GREEN)make dev up$(RESET)                    - Start development environment"
	@echo "  $(GREEN)make prod infra-up$(RESET)             - Start production infrastructure (with prompt)"
	@echo "  $(GREEN)make FORCE=true prod infra-up$(RESET)  - Start production (bypass prompt for CI/CD)"
	@echo "  $(GREEN)make ENV=production status$(RESET)     - Check production status"
	@echo "  $(GREEN)make dev SERVICE=postgres logs-service$(RESET) - View dev postgres logs"

# Environment shortcuts
dev:
	@$(MAKE) ENV=development $(filter-out $@,$(MAKECMDGOALS))

prod:
	@$(MAKE) ENV=production FORCE=$(FORCE) $(filter-out $@,$(MAKECMDGOALS))

# Validate environment
validate-env:
	@if ! echo "$(VALID_ENVS)" | grep -q "$(ENV)"; then \
		echo "$(RED)❌ Invalid environment: $(ENV)$(RESET)"; \
		echo "$(YELLOW)Valid environments: $(VALID_ENVS)$(RESET)"; \
		exit 1; \
	fi
	@if [ ! -d "environments/$(ENV)" ]; then \
		echo "$(RED)❌ Environment directory not found: environments/$(ENV)$(RESET)"; \
		exit 1; \
	fi

# Show current environment info
info: validate-env
	@echo "$(BLUE)📋 Environment Information$(RESET)"
	@echo "Environment: $(YELLOW)$(ENV)$(RESET)"
	@echo "Directory: $(YELLOW)environments/$(ENV)$(RESET)"
	@cd environments/$(ENV) && $(MAKE) info

# Delegate all other targets to environment-specific Makefile
%: validate-env
	@echo "$(BLUE)🎯 Running target '$(YELLOW)$@$(RESET)$(BLUE)' in $(YELLOW)$(ENV)$(RESET)$(BLUE) environment...$(RESET)"
	@cd environments/$(ENV) && $(MAKE) $@

# Allow arguments to be passed to environment makefiles
%:
	@: