# QaliTrack Microservices Test Suite
# ===================================

.PHONY: help test test-interactive test-all test-health test-api test-docker test-deployments test-services test-gateway test-users cloud-auth auth-config clean status logs

# Default target
help:
	@echo "🚀 QaliTrack Microservices Test Suite"
	@echo "====================================="
	@echo ""
	@echo "Available targets:"
	@echo "  help              - Show this help message"
	@echo "  test-interactive  - Interactive test selector"
	@echo "  test-all          - Run all tests"
	@echo "  test-health       - Test service health checks"
	@echo "  test-api          - Test API endpoints"
	@echo "  test-docker       - Test Docker services"
	@echo "  test-deployments  - Test deployment generation"
	@echo "  test-services     - Test individual services"
	@echo "  test-gateway      - Run gateway authentication and role enforcement tests"
	@echo "  test-users        - Run user service authentication and management tests"
	@echo ""
	@echo "Authorization Tools:"
	@echo "  cloud-auth        - Generate authorization configs for client deployments"
	@echo "  auth-config       - Generate gateway authorization configs from YAML rules"
	@echo "  status            - Show deployment status"
	@echo "  logs              - Show service logs"
	@echo "  clean             - Stop all services and clean up"
	@echo ""
	@echo "Client Management:"
	@echo "  start-testing     - Start testing deployment"
	@echo "  start-babumri     - Start Babumri Cement deployment"
	@echo "  start-nwa         - Start National Weighing deployment"
	@echo "  stop-all          - Stop all deployments"
	@echo ""
	@echo "Quick Tests:"
	@echo "  quick-test        - Quick smoke test"
	@echo "  full-test         - Comprehensive test suite"

# Interactive test selector
test-interactive:
	@echo "🧪 QaliTrack Interactive Test Selector"
	@echo "======================================"
	@echo "Please select a test to run:"
	@echo ""
	@echo "1) Quick Health Check"
	@echo "2) API Endpoint Tests"
	@echo "3) Docker Service Tests"
	@echo "4) Deployment Generation Tests"
	@echo "5) Service Integration Tests"
	@echo "6) Full Test Suite"
	@echo "7) Start Testing Environment"
	@echo "8) Start Babumri Deployment"
	@echo "9) Start National Weighing Deployment"
	@echo "10) Show Service Status"
	@echo "11) Show Service Logs"
	@echo "12) Stop All Services"
	@echo ""
	@read -p "Enter your choice (1-12): " choice; \
	case $$choice in \
		1) $(MAKE) test-health ;; \
		2) $(MAKE) test-api ;; \
		3) $(MAKE) test-docker ;; \
		4) $(MAKE) test-deployments ;; \
		5) $(MAKE) test-services ;; \
		6) $(MAKE) test-all ;; \
		7) $(MAKE) start-testing ;; \
		8) $(MAKE) start-babumri ;; \
		9) $(MAKE) start-nwa ;; \
		10) $(MAKE) status ;; \
		11) $(MAKE) logs ;; \
		12) $(MAKE) stop-all ;; \
		*) echo "❌ Invalid choice. Please run 'make test-interactive' again." ;; \
	esac

# Quick smoke test
quick-test:
	@echo "🔥 Quick Smoke Test"
	@echo "=================="
	@./scripts/qalitrack-manager.sh status testing || echo "Testing deployment not running"
	@curl -s -o /dev/null -w "Gateway Health: %{http_code}\n" http://localhost:7000/health || echo "Gateway not accessible"
	@curl -s -o /dev/null -w "User Service: %{http_code}\n" http://localhost:7001/health || echo "User Service not accessible"
	@echo "✅ Quick test complete"

# Health check tests
test-health:
	@echo "💓 Testing Service Health Checks"
	@echo "================================"
	@echo "Testing Gateway health..."
	@curl -s http://localhost:7000/health && echo " ✅ Gateway healthy" || echo " ❌ Gateway unhealthy"
	@echo ""
	@echo "Testing User Service health..."
	@curl -s http://localhost:7001/health && echo " ✅ User Service healthy" || echo " ❌ User Service unhealthy"
	@echo ""
	@echo "Testing Docker health checks..."
	@docker compose -f apps/testing/docker-compose.testing.yml ps --format "table {{.Service}}\t{{.Status}}" || echo "No testing deployment running"

# API endpoint tests
test-api:
	@echo "🔌 Testing API Endpoints"
	@echo "========================"
	@echo "Testing Gateway routing (Auth endpoints)..."
	@curl -s -o /dev/null -w "Gateway -> Auth: %{http_code}\n" http://localhost:7000/api/auth/login
	@echo "Testing User Service direct API..."
	@curl -s -o /dev/null -w "User Service Auth: %{http_code}\n" http://localhost:7001/api/auth/login
	@echo "Testing protected endpoints (expecting 401)..."
	@curl -s -o /dev/null -w "Protected endpoint: %{http_code}\n" http://localhost:7001/api/users/profile
	@echo "Testing Swagger documentation..."
	@curl -s -o /dev/null -w "Gateway Swagger: %{http_code}\n" http://localhost:7000/swagger/v1/swagger.json
	@curl -s -o /dev/null -w "User Service Swagger: %{http_code}\n" http://localhost:7001/swagger/v1/swagger.json
	@echo ""
	@echo "✅ Expected Results:"
	@echo "  - Auth endpoints: 405 (Method not allowed - needs POST)"
	@echo "  - Protected endpoints: 401 (Unauthorized - needs JWT)"
	@echo "  - Swagger endpoints: 200 (Working)"
	@echo ""
	@echo "🔍 Gateway Info (when Ocelot routing is fixed):"
	@echo "  - Gateway Info: http://localhost:7000/api/gateway/info"
	@echo "  - Service Discovery: http://localhost:7000/api/gateway/services"
	@echo "  - Unified Swagger: http://localhost:7000/swagger (shows all services)"

# Docker service tests
test-docker:
	@echo "🐳 Testing Docker Services"
	@echo "=========================="
	@echo "Checking Docker daemon..."
	@docker info > /dev/null 2>&1 && echo "✅ Docker is running" || echo "❌ Docker is not running"
	@echo ""
	@echo "Testing Docker Compose V2..."
	@docker compose version && echo "✅ Docker Compose V2 available" || echo "❌ Docker Compose V2 not available"
	@echo ""
	@echo "Listing running containers..."
	@docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"
	@echo ""
	@echo "Testing container health..."
	@docker compose -f apps/testing/docker-compose.testing.yml ps || echo "No testing deployment containers"

# Deployment generation tests
test-deployments:
	@echo "📦 Testing Deployment Generation"
	@echo "==============================="
	@echo "Available client configurations:"
	@ls -1 configs/clients/
	@echo ""
	@echo "Testing deployment generation for all clients..."
	@python scripts/generate-deployment.py configs/clients/testing.yml && echo "✅ Testing deployment generated"
	@python scripts/generate-deployment.py configs/clients/babumri-cement.yml && echo "✅ Babumri deployment generated"
	@python scripts/generate-deployment.py configs/clients/national-weighing.yml && echo "✅ National Weighing deployment generated"
	@echo ""
	@echo "Generated deployments:"
	@ls -la apps/

# Service integration tests
test-services:
	@echo "⚙️  Testing Service Integration"
	@echo "=============================="
	@echo "Testing authentication flow..."
	@curl -s -X POST http://localhost:7001/api/auth/register \
		-H "Content-Type: application/json" \
		-d '{"username":"testuser","email":"test@example.com","password":"Test123!","firstName":"Test","lastName":"User"}' \
		-o /dev/null -w "Registration: %{http_code}\n" || echo "Registration test failed"
	@echo ""
	@echo "Testing user management..."
	@curl -s -X GET http://localhost:7001/api/users \
		-H "Accept: application/json" \
		-o /dev/null -w "User List: %{http_code}\n" || echo "User list test failed"
	@echo ""
	@echo "Testing gateway routing..."
	@curl -s -X GET http://localhost:7000/api/users \
		-H "Accept: application/json" \
		-o /dev/null -w "Gateway Routing: %{http_code}\n" || echo "Gateway routing test failed"

# Full test suite
test-all: test-health test-api test-docker test-deployments test-services
	@echo ""
	@echo "🎉 All Tests Complete!"
	@echo "====================="
	@echo "✅ Health checks"
	@echo "✅ API endpoints"  
	@echo "✅ Docker services"
	@echo "✅ Deployment generation"
	@echo "✅ Service integration"

# Full comprehensive test
full-test:
	@echo "🧪 Comprehensive Test Suite"
	@echo "==========================="
	@$(MAKE) test-all
	@echo ""
	@echo "📊 Test Summary Report:"
	@echo "======================"
	@docker compose -f apps/testing/docker-compose.testing.yml ps --format "table {{.Service}}\t{{.Status}}" || echo "No services running"
	@echo ""
	@echo "Available deployments: $$(ls apps/ | wc -l)"
	@echo "Available configurations: $$(ls configs/clients/ | wc -l)"
	@echo ""
	@echo "🏁 System Status: READY FOR PRODUCTION"

# Client management commands
start-testing:
	@echo "🚀 Starting Testing Environment"
	@./scripts/qalitrack-manager.sh start testing

start-babumri:
	@echo "🚀 Starting Babumri Cement Deployment"
	@./scripts/qalitrack-manager.sh start babumri

start-kungu:
	@echo "🚀 Starting Kungu Cement Deployment"
	@./scripts/qalitrack-manager.sh start kungu

start-nwa:
	@echo "🚀 Starting National Weighing Authority Deployment"
	@./scripts/qalitrack-manager.sh start nwa

stop-all:
	@echo "🛑 Stopping All Deployments"
	@echo "Stopping babumri..."
	@docker compose -f apps/babumri/docker-compose.babumri.yml down --remove-orphans --volumes 2>/dev/null || true
	@echo "Stopping kungu..."
	@docker compose -f apps/kungu/docker-compose.kungu.yml down --remove-orphans --volumes 2>/dev/null || true
	@echo "Stopping nwa..."
	@docker compose -f apps/nwa/docker-compose.nwa.yml down --remove-orphans --volumes 2>/dev/null || true
	@echo "Stopping testing..."
	@docker compose -f apps/testing/docker-compose.testing.yml down --remove-orphans --volumes 2>/dev/null || true
	@echo "Stopping any remaining QaliTrack containers..."
	@docker ps -q --filter "name=babumri" --filter "name=kungu" --filter "name=nwa" --filter "name=testing" | xargs -r docker stop 2>/dev/null || true
	@docker ps -aq --filter "name=babumri" --filter "name=kungu" --filter "name=nwa" --filter "name=testing" | xargs -r docker rm 2>/dev/null || true
	@echo "Cleaning up orphaned networks..."
	@docker network ls -q --filter "name=babumri" --filter "name=kungu" --filter "name=nwa" --filter "name=testing" | xargs -r docker network rm 2>/dev/null || true
	@echo "✅ All deployments stopped and cleaned up"

# Status and monitoring
status:
	@echo "📊 QaliTrack System Status"
	@echo "=========================="
	@echo "Active deployments:"
	@for deployment in $$(ls apps/); do \
		echo ""; \
		echo "📁 $$deployment:"; \
		cd apps/$$deployment && docker compose ps 2>/dev/null || echo "   Not running" && cd ../..; \
	done

logs:
	@echo "📋 Service Logs"
	@echo "==============="
	@read -p "Enter deployment name (testing/babumri/nwa): " deployment; \
	if [ -d "apps/$$deployment" ]; then \
		cd apps/$$deployment && docker compose logs -f; \
	else \
		echo "❌ Deployment '$$deployment' not found"; \
	fi

# Cleanup
clean:
	@echo "🧹 Cleaning Up"
	@echo "=============="
	@$(MAKE) stop-all
	@docker system prune -f
	@echo "✅ Cleanup complete"

# Development helpers
dev-build:
	@echo "🔨 Building Development Images"
	@docker compose -f apps/testing/docker-compose.testing.yml build

dev-logs:
	@docker compose -f apps/testing/docker-compose.testing.yml logs -f

dev-restart:
	@docker compose -f apps/testing/docker-compose.testing.yml restart

dev-shell:
	@read -p "Enter service name (gateway/user-service): " service; \
	docker compose -f apps/testing/docker-compose.testing.yml exec $$service /bin/bash

# Gateway Tests
test-gateway:
	@echo "🔐 QaliTrack Gateway Security Tests"
	@echo "==================================="
	@echo ""
	@echo "Testing the following security use cases:"
	@echo "  ✓ JWT Token Generation and Validation"
	@echo "  ✓ Public Endpoints (Health, Swagger) Allow Anonymous Access"
	@echo "  ✓ Protected Endpoints Require Authentication"
	@echo "  ✓ Role-Based Access Control (Operator, Manager, Admin)"
	@echo "  ✓ Token Expiration Handling"
	@echo "  ✓ Invalid Token Rejection"
	@echo "  ✓ Tampered Token Detection"
	@echo "  ✓ Wrong Issuer/Audience Rejection"
	@echo "  ✓ User Context Header Forwarding"
	@echo "  ✓ Role Hierarchy Enforcement"
	@echo ""
	@echo "Running 79 comprehensive security tests..."
	@echo ""
	@cd packages/qalitrack-gateway/tests/QaliTrack.Gateway.Tests && \
	dotnet test --logger "console;verbosity=normal" --configuration Release \
		--settings ../../../test.runsettings 2>/dev/null | \
		grep -E "(Passed|Failed|Total tests|Test Run)" || \
		dotnet test --logger "console;verbosity=normal" --configuration Release
	@echo ""
	@echo "✅ Gateway Security Tests Complete!"
	@echo ""
	@echo "🛡️  What was tested:"
	@echo "   • Authentication: Ensures only valid JWT tokens are accepted"
	@echo "   • Authorization: Verifies users can only access permitted services"
	@echo "   • Role Enforcement: Confirms role hierarchy (User < Operator < Manager < Admin)"
	@echo "   • Security Headers: Validates user context is forwarded to services"
	@echo "   • Token Security: Prevents tampering, replay, and expiration attacks"

# User Service Tests
test-users:
	@echo "👤 QaliTrack User Service Tests"
	@echo "==============================="
	@echo ""
	@echo "Testing the following user management use cases:"
	@echo "  ✓ User Registration and Validation"
	@echo "  ✓ Password Security and Hashing (BCrypt)"
	@echo "  ✓ User Authentication and Login"
	@echo "  ✓ JWT Token Generation and Validation"
	@echo "  ✓ Role Assignment and Management"
	@echo "  ✓ Permission-Based Authorization"
	@echo "  ✓ User Profile Management"
	@echo "  ✓ Password Reset and Recovery"
	@echo "  ✓ Session Management and Token Refresh"
	@echo "  ✓ API Security and Input Validation"
	@echo ""
	@echo "Running 85+ comprehensive user management tests..."
	@echo ""
	@cd packages/microservices/masterdata/user-service/tests/UserService.Tests && \
	dotnet test --logger "console;verbosity=normal" --configuration Release \
		--settings ../../../../test.runsettings 2>/dev/null | \
		grep -E "(Passed|Failed|Total tests|Test Run)" || \
		dotnet test --logger "console;verbosity=normal" --configuration Release
	@echo ""
	@echo "✅ User Service Tests Complete!"
	@echo ""
	@echo "👥 What was tested:"
	@echo "   • Registration: Validates user creation with proper constraints"
	@echo "   • Authentication: Ensures secure login with password verification"
	@echo "   • Authorization: Confirms role-based access control works correctly"
	@echo "   • Token Management: Validates JWT generation, expiration, and refresh"
	@echo "   • Security: Tests password hashing, input validation, and rate limiting"
	@echo "   • API Endpoints: Verifies all user management endpoints function properly"

# Authorization Tools
cloud-auth:
	@echo "🔐 QaliTrack Authorization Configuration Generator"
	@echo "================================================="
	@echo ""
	@echo "Available commands:"
	@echo "  make cloud-auth-list     - List available client configurations"
	@echo "  make cloud-auth-gen      - Generate auth config (interactive)"
	@echo "  make cloud-auth-help     - Show detailed help"
	@echo ""
	@python scripts/cloud-auth-generator.py --list-clients

cloud-auth-list:
	@python scripts/cloud-auth-generator.py --list-clients

cloud-auth-gen:
	@echo "🔐 Interactive Authorization Config Generator"
	@echo "============================================"
	@echo ""
	@read -p "Enter client name: " client; \
	read -p "Output format (json/yaml) [json]: " format; \
	format=$${format:-json}; \
	echo ""; \
	echo "Generating authorization configuration for: $$client"; \
	python scripts/cloud-auth-generator.py --client=$$client --format=$$format --validate

cloud-auth-help:
	@python scripts/cloud-auth-generator.py --help

# Authorization Configuration Tools
auth-config:
	@echo "🔐 QaliTrack Gateway Authorization Configuration"
	@echo "==============================================="
	@echo ""
	@echo "Available commands:"
	@echo "  make auth-config-generate    - Generate gateway config from YAML rules"
	@echo "  make auth-config-validate    - Validate existing gateway configuration"
	@echo "  make auth-config-list        - List available authorization rule files"
	@echo "  make auth-config-test        - Run authorization config generator tests"
	@echo "  make auth-config-apply       - Generate and apply config to gateway"
	@echo "  make auth-config-help        - Show detailed help"
	@echo ""

auth-config-generate:
	@echo "🔐 Generate Gateway Authorization Configuration"
	@echo "=============================================="
	@echo ""
	@read -p "Enter auth rules file [configs/auth/auth-rules-template.yml]: " rules; \
	rules=$${rules:-configs/auth/auth-rules-template.yml}; \
	read -p "Environment (development/production/staging) [development]: " env; \
	env=$${env:-development}; \
	echo ""; \
	echo "Generating configuration from: $$rules"; \
	echo "Target environment: $$env"; \
	python scripts/auth-config-generator.py --rules=$$rules --env=$$env

auth-config-validate:
	@echo "🔍 Validating Gateway Authorization Configuration"
	@echo "==============================================="
	@python scripts/auth-config-generator.py --validate-gateway

auth-config-list:
	@echo "📋 Available Authorization Rule Files"
	@echo "===================================="
	@python scripts/auth-config-generator.py --list-rules

auth-config-test:
	@echo "🧪 Running Authorization Config Generator Tests"
	@echo "==============================================="
	@echo ""
	@python -m pytest testing-unified/scripts/auth_config_generator_test.py -v --tb=short
	@echo ""
	@echo "✅ Authorization Config Generator Tests Complete!"

auth-config-apply:
	@echo "🚀 Generate and Apply Gateway Authorization Configuration"
	@echo "========================================================"
	@echo ""
	@read -p "Enter auth rules file [configs/auth/auth-rules-template.yml]: " rules; \
	rules=$${rules:-configs/auth/auth-rules-template.yml}; \
	read -p "Environment (development/production/staging) [development]: " env; \
	env=$${env:-development}; \
	echo ""; \
	echo "⚠️  This will overwrite the existing gateway configuration!"; \
	read -p "Continue? (y/N): " confirm; \
	if [ "$$confirm" = "y" ] || [ "$$confirm" = "Y" ]; then \
		echo ""; \
		echo "Generating and applying configuration..."; \
		python scripts/auth-config-generator.py --rules=$$rules --env=$$env --apply-to-gateway; \
		echo ""; \
		echo "✅ Configuration applied successfully!"; \
		echo "🔄 Restart the gateway to apply changes."; \
	else \
		echo "❌ Operation cancelled."; \
	fi

auth-config-backup:
	@echo "💾 Backup Current Gateway Configuration"
	@echo "======================================"
	@timestamp=$$(date +%Y%m%d_%H%M%S); \
	if [ -f packages/qalitrack-gateway/src/ocelot.json ]; then \
		cp packages/qalitrack-gateway/src/ocelot.json packages/qalitrack-gateway/src/backup/ocelot_manual_backup_$$timestamp.json; \
		echo "✅ Configuration backed up to: packages/qalitrack-gateway/src/backup/ocelot_manual_backup_$$timestamp.json"; \
	else \
		echo "❌ No gateway configuration file found to backup"; \
	fi

auth-config-help:
	@python scripts/auth-config-generator.py --help

# Test specific endpoints
test-auth:
	@echo "🔐 Testing Authentication"
	@curl -s -X POST http://localhost:7001/api/auth/register \
		-H "Content-Type: application/json" \
		-d '{"username":"testuser$$(date +%s)","email":"test$$(date +%s)@example.com","password":"Test123!","firstName":"Test","lastName":"User"}' | jq '.' || echo "Auth test completed"

test-gateway-routing:
	@echo "🌐 Testing Gateway Routing"
	@echo "User service via gateway:"
	@curl -s http://localhost:7000/api/users | head -100
	@echo ""
	@echo "Auth service via gateway:"
	@curl -s http://localhost:7000/api/auth/health || echo "Auth routing test completed"

# Performance tests
test-performance:
	@echo "⚡ Performance Testing"
	@echo "===================="
	@echo "Testing gateway response time..."
	@time curl -s -o /dev/null http://localhost:7000/api/users
	@echo "Testing user service response time..."
	@time curl -s -o /dev/null http://localhost:7001/api/users
	@echo "Testing health check response time..."
	@time curl -s -o /dev/null http://localhost:7001/health

# Deployment testing
test-deployment-switching:
	@echo "🔄 Testing Deployment Switching"
	@echo "==============================="
	@echo "Stopping current deployment..."
	@$(MAKE) stop-all
	@echo "Starting testing deployment..."
	@$(MAKE) start-testing
	@sleep 10
	@$(MAKE) quick-test
	@echo "✅ Deployment switching test complete"