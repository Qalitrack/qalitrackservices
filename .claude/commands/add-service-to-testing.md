# Add Service to Testing Client

Enable a service in the testing client configuration for development and testing purposes.

## Service Name: $ARGUMENTS

## Service Addition Process

1. **Load Service Context**
   - Read and understand the service name: $ARGUMENTS
   - Analyze the current testing client configuration (configs/clients/testing.yml)
   - Identify the service in the available services list
   - Check current service status (enabled/disabled) in testing configuration
   - Understand service dependencies and prerequisites
   - Review port assignments and avoid conflicts

2. **ULTRATHINK**
   - Think hard before modifying configuration. Create a comprehensive service addition strategy
   - Break down service addition into manageable steps using your TodoWrite tool
   - Use the TodoWrite tool to create and track your service addition plan
   - Identify service dependencies that may need to be enabled first
   - Determine appropriate port assignment for the service in testing environment
   - Consider resource constraints in testing environment (single replica typically)
   - Plan for service-specific configuration requirements

3. **Execute Service Addition**
   - Enable the specified service in testing.yml configuration
   - Set appropriate testing environment values:
     - enabled: true
     - port: assign next available port in 7000+ range
     - image: "qalitrack/{service-name}:latest"
     - replicas: 1 (for testing environment)
     - test_mode: true (if applicable)
   - Check and enable any required service dependencies
   - Update service discovery configuration if needed
   - Regenerate testing deployment with updated configuration
   - Update testing documentation with new service availability

4. **Validate**
   - Validate that the service configuration is syntactically correct
   - Check that the assigned port doesn't conflict with existing services
   - Verify that all required dependencies are enabled
   - Regenerate testing docker-compose file to ensure it's valid
   - Test that the service can be started successfully in testing environment
   - Validate service health check and basic functionality
   - Fix any configuration issues or port conflicts
   - Re-validate until service starts successfully

5. **Complete**
   - Ensure service is properly configured and enabled in testing environment
   - Verify that regenerated testing deployment includes the new service
   - Update testing environment documentation with new service information
   - Generate updated make targets if needed for the new service
   - Test service integration with existing testing services (gateway, user-service)
   - Report service addition completion status with connection details
   - Confirm service is accessible and functional in testing environment

6. **Reference Service Requirements**
   - You can always reference the service implementation details if needed
   - Cross-reference service configuration with actual service capabilities
   - Ensure service configuration matches service requirements
   - Verify database and dependency requirements are met in testing environment

## Service Configuration Template

When adding a service, use this configuration template:

```yaml
{service-name}:
  enabled: true
  port: {next-available-port}
  image: "qalitrack/{service-name}:latest"
  replicas: 1
  test_mode: true
```

## Port Assignment Strategy

Testing environment port assignments (7000+ range):
- 7000: gateway (reserved)
- 7001: user-service (reserved)
- 7019: service-discovery (reserved)
- 7002+: Available for additional services

## Service Dependencies

Common service dependencies that may need to be enabled:
- **Core Services**: gateway, user-service (always required)
- **Master Data Services**: organization-service (often required by other services)
- **Operational Services**: May depend on specific master data services

## Testing Environment Considerations

**Resource Optimization for Testing:**
- Single replica (replicas: 1) for all services
- In-memory SQLite databases for faster testing
- Minimal logging and monitoring overhead
- Test mode enabled where applicable

**Service Integration:**
- Services should integrate with existing gateway routing
- Authentication through user-service
- Health checks enabled for service monitoring

## Configuration Validation

After adding service, validate:
```bash
# Validate YAML syntax
python -c "import yaml; yaml.safe_load(open('configs/clients/testing.yml'))"

# Regenerate testing deployment
python scripts/generate-deployment.py configs/clients/testing.yml

# Start testing environment with new service
make start-testing

# Validate service health
curl -s http://localhost:{port}/health
```

## Generated Make Target

When adding a service, also update the Makefile with appropriate test target:

```makefile
test-{service}:
	@echo "🔧 QaliTrack {Service} Service Tests"
	@echo "===================================="
	@echo ""
	@echo "Testing the following {service} management use cases:"
	@echo "  ✓ [Service-specific functionality]"
	@echo "  ✓ API Security and Role-Based Access Control"
	@echo "  ✓ Database Integration and Data Validation"
	@echo ""
	@echo "Running {number}+ comprehensive {service} tests..."
	@echo ""
	@cd packages/microservices/{category}/{service}-service/tests/{Service}Service.Tests && \
	dotnet test --logger "console;verbosity=normal" --configuration Release
	@echo ""
	@echo "✅ {Service} Service Tests Complete!"
```

## Usage Examples

```bash
# Add product service to testing environment
/add-service-to-testing product-service

# Add vehicle service to testing environment  
/add-service-to-testing vehicle-service

# Add weight data service to testing environment
/add-service-to-testing weight-data-service

# Add compliance service to testing environment
/add-service-to-testing compliance-service
```

## Example Service Additions

**Adding Product Service:**
```yaml
# Before
product-service:
  enabled: false
  test_mode: true

# After  
product-service:
  enabled: true
  port: 7005
  image: "qalitrack/product-service:latest"
  replicas: 1
  test_mode: true
```

**Adding Weight Data Service:**
```yaml
# Before
weight-data-service:
  enabled: false
  test_mode: true

# After
weight-data-service:
  enabled: true
  port: 7012
  image: "qalitrack/weight-data-service:latest"
  replicas: 1
  test_mode: true
```

## Post-Addition Testing

After adding a service to testing:

```bash
# Regenerate and start testing environment
python scripts/generate-deployment.py configs/clients/testing.yml
make start-testing

# Test service health
curl -s http://localhost:{port}/health

# Test service through gateway (if configured)
curl -s http://localhost:7000/api/{service}/health

# Run service-specific tests
make test-{service}
```

## Error Patterns & Solutions

Common issues when adding services:
- **Port Conflicts**: Check existing port assignments and use next available
- **Missing Dependencies**: Enable required dependent services first
- **Image Not Found**: Ensure service Docker image exists and is properly tagged
- **Service Won't Start**: Check service configuration and dependencies
- **Gateway Routing**: Update gateway configuration if service needs external access

## Completion Criteria

Service addition is complete when:
- [ ] Service is enabled in testing.yml with proper configuration
- [ ] Port assignment doesn't conflict with existing services
- [ ] Service dependencies are identified and enabled if needed
- [ ] Testing docker-compose file is regenerated successfully
- [ ] Service starts and responds to health checks
- [ ] Service integrates properly with existing testing services
- [ ] Testing documentation is updated with new service information
- [ ] Make target is added for testing the new service

This command streamlines the process of adding individual services to the testing environment for development and validation purposes.