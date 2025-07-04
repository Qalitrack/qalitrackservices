# Create Service Test Suite

Generate comprehensive test suites for QaliTrack microservices with authentication, authorization, and integration testing.

## Service Name: $ARGUMENTS

## Test Suite Generation Process

1. **Load Service Context**
   - Read and understand the service name: $ARGUMENTS
   - Analyze the existing service codebase structure and architecture
   - Identify controllers, services, repositories, DTOs, and data models
   - Map API endpoints and their authentication/authorization requirements
   - Understand existing patterns and conventions in the QaliTrack service
   - Explore similar test implementations in other QaliTrack services for consistency

2. **ULTRATHINK**
   - Think hard before generating tests. Create a comprehensive test strategy addressing all service aspects
   - Break down testing into manageable categories using your TodoWrite tool
   - Use the TodoWrite tool to create and track your test generation plan
   - Identify existing test patterns from QaliTrack codebase to follow
   - Map service functionality to specific test scenarios (CRUD, auth, validation, edge cases)
   - Consider security implications, error conditions, and performance requirements
   - Plan for unit, integration, security, and repository test coverage (85+ tests minimum)

3. **Execute Test Generation**
   - Generate complete test suite following QaliTrack testing patterns and conventions
   - Create Unit Tests (controllers, services, repositories, validators)
   - Create Integration Tests (API endpoints, database integration, authentication flows)
   - Create Security Tests (authorization, input validation, JWT handling)
   - Create Repository Tests (data access, CRUD operations, query testing)
   - Generate realistic test data factories and mock implementations
   - Create business-friendly make target with descriptive test explanations (like test-gateway/test-users)
   - Generate test descriptions showing what business capabilities are being validated
   - Ensure proper project structure and dependencies

4. **Validate**
   - Run each generated test class to ensure compilation and basic functionality
   - Execute `make test-[service]` to run full test suite
   - Validate test coverage meets QaliTrack standards (>80% coverage)
   - Check that all API endpoints have corresponding test methods
   - Verify authentication and authorization scenarios are properly covered
   - Test mock implementations and test data factories
   - Fix any test failures, compilation issues, or dependency problems
   - Re-run until all tests pass and coverage is achieved

5. **Complete**
   - Ensure all test categories are implemented (Unit, Integration, Security, Repository)
   - Verify make targets are properly integrated into main Makefile
   - Run final comprehensive test validation suite
   - Check that tests follow QaliTrack coding conventions and patterns
   - Validate test project structure matches QaliTrack standards
   - Update any related documentation if needed
   - Report test generation completion status with metrics
   - Confirm service now has comprehensive test coverage (85+ tests)

6. **Reference Service Requirements**
   - You can always reference the service code again if needed during generation
   - Cross-reference test scenarios with actual service functionality
   - Ensure no critical business logic paths are left untested
   - Verify all public methods and endpoints have test coverage

## Generated Test Structure

```
tests/{ServiceName}.Tests/
├── Controllers/
│   ├── {Entity}ControllerTests.cs
│   └── HealthControllerTests.cs
├── Services/ 
│   ├── {Entity}ServiceTests.cs
│   └── ValidationServiceTests.cs
├── Integration/
│   ├── {ServiceName}IntegrationTests.cs
│   └── ApiEndpointTests.cs
├── Repositories/
│   └── {Entity}RepositoryTests.cs
├── Security/
│   ├── AuthorizationTests.cs
│   └── AuthenticationTests.cs
├── Helpers/
│   ├── TestDataFactory.cs
│   ├── MockFactories.cs
│   └── TestWebApplicationFactory.cs
└── {ServiceName}.Tests.csproj
```

## Testing Standards

All generated tests follow QaliTrack patterns:
- **xUnit** testing framework
- **FluentAssertions** for readable test assertions
- **Moq** for mocking dependencies and external services
- **WebApplicationFactory** for integration testing
- **AutoFixture** for realistic test data generation
- **Category** attributes for test filtering and organization

## Generated Make Target Example

The command creates a business-friendly make target similar to test-gateway and test-users:

```makefile
test-vehicles:
	@echo "🚗 QaliTrack Vehicle Service Tests"
	@echo "=================================="
	@echo ""
	@echo "Testing the following vehicle management use cases:"
	@echo "  ✓ Vehicle Registration and Validation"
	@echo "  ✓ Fleet Management Operations"
	@echo "  ✓ Vehicle Assignment and Tracking"
	@echo "  ✓ License Plate Validation and Uniqueness"
	@echo "  ✓ Vehicle Type and Capacity Management"
	@echo "  ✓ Ownership and Assignment Validation"
	@echo "  ✓ Vehicle Status Management (Active/Inactive)"
	@echo "  ✓ API Security and Authentication"
	@echo "  ✓ Role-Based Access Control (Operator+ required)"
	@echo "  ✓ Database Integration and Data Validation"
	@echo ""
	@echo "Running 85+ comprehensive vehicle management tests..."
	@echo ""
	@cd packages/microservices/masterdata/vehicle-service/tests/VehicleService.Tests && \
	dotnet test --logger "console;verbosity=normal" --configuration Release
	@echo ""
	@echo "✅ Vehicle Service Tests Complete!"
	@echo ""
	@echo "🚗 What was tested:"
	@echo "   • Registration: Validates vehicle creation with proper constraints"
	@echo "   • Fleet Management: Ensures comprehensive vehicle lifecycle operations"
	@echo "   • Security: Tests authentication and role-based access control"
	@echo "   • Data Integrity: Verifies database operations and business rules"
	@echo "   • API Endpoints: Confirms all vehicle management endpoints work correctly"
```

## Validation Commands

```bash
# Run full test suite for the service (with business descriptions)
make test-[service]

# Run specific test categories
dotnet test --filter Category=Unit
dotnet test --filter Category=Integration
dotnet test --filter Category=Security

# Check test coverage
dotnet test --collect:"XPlat Code Coverage"

# Validate test compilation
dotnet build tests/{ServiceName}.Tests/
```

## Error Patterns & Solutions

Common issues during test generation and solutions:
- **Compilation errors**: Check service dependencies and project references
- **Authentication test failures**: Ensure JWT configuration matches service setup
- **Database test issues**: Verify test database setup and migration scripts
- **Missing mock dependencies**: Add appropriate mock configurations for external services
- **Integration test failures**: Check service startup and dependency injection configuration

## Completion Criteria

Test generation is complete when:
- [ ] All test files compile successfully without errors
- [ ] Full test suite runs and passes (85+ tests minimum)
- [ ] Test coverage exceeds 80% for business logic
- [ ] All public API endpoints have corresponding test methods
- [ ] Authentication and authorization scenarios are covered
- [ ] Security edge cases and validation are tested
- [ ] Business-friendly make target created with descriptive test explanations
- [ ] Make target shows what business capabilities are being validated
- [ ] Make target follows the pattern of test-gateway and test-users
- [ ] Test data factories provide realistic test scenarios

## Usage Examples

```bash
# Generate comprehensive tests for vehicle service
/create-service-test vehicle-service

# Generate tests for weight data service
/create-service-test weight-data-service

# Generate tests for compliance monitoring service
/create-service-test compliance-service

# Generate tests for customer management service
/create-service-test customer-service
```

## More Make Target Examples

**Weight Data Service:**
```makefile
test-weight-data:
	@echo "⚖️  QaliTrack Weight Data Service Tests"
	@echo "======================================"
	@echo ""
	@echo "Testing the following weight measurement use cases:"
	@echo "  ✓ Weight Measurement Capture and Validation"
	@echo "  ✓ Measurement Accuracy and Calibration"
	@echo "  ✓ Weight Correction and Adjustment Procedures"
	@echo "  ✓ Data Integrity and Audit Trails"
	@echo "  ✓ Real-time Weight Data Processing"
	@echo "  ✓ Integration with Weighbridge Hardware"
	@echo "  ✓ Measurement History and Analytics"
	@echo "  ✓ API Security and Role-Based Access (Operator+ required)"
	@echo "  ✓ Database Operations and Performance"
	@echo ""
	@echo "Running 80+ comprehensive weight management tests..."
	@echo ""
	@echo "⚖️  What was tested:"
	@echo "   • Measurement: Validates accurate weight capture and processing"
	@echo "   • Calibration: Ensures weighbridge accuracy and corrections"
	@echo "   • Data Integrity: Verifies audit trails and measurement history"
	@echo "   • Performance: Tests real-time processing and database operations"
```

**Compliance Service:**
```makefile
test-compliance:
	@echo "📋 QaliTrack Compliance Service Tests"
	@echo "====================================="
	@echo ""
	@echo "Testing the following compliance monitoring use cases:"
	@echo "  ✓ Regulatory Rule Engine and Validation"
	@echo "  ✓ Compliance Violation Detection"
	@echo "  ✓ Audit Trail Generation and Management"
	@echo "  ✓ Regulatory Reporting and Documentation"
	@echo "  ✓ Weight Limit and Route Restriction Checking"
	@echo "  ✓ License and Permit Validation"
	@echo "  ✓ Real-time Compliance Monitoring"
	@echo "  ✓ API Security and Role-Based Access (Auditor+ required)"
	@echo "  ✓ Integration with Regulatory Systems"
	@echo ""
	@echo "Running 75+ comprehensive compliance tests..."
	@echo ""
	@echo "📋 What was tested:"
	@echo "   • Rule Engine: Validates regulatory compliance checking"
	@echo "   • Violation Detection: Ensures accurate compliance monitoring"
	@echo "   • Audit Trails: Verifies comprehensive audit documentation"
	@echo "   • Reporting: Tests regulatory report generation and accuracy"
```

## Post-Generation Validation

After successful test generation:
1. Run the full test suite to ensure 100% pass rate
2. Check test coverage reports and identify any gaps
3. Verify integration with CI/CD pipeline
4. Document any service-specific testing considerations
5. Update team on new test patterns for future reference
6. Ensure tests run successfully in different environments

Note: If generation fails, analyze error patterns and retry. The generated tests integrate with existing QaliTrack testing infrastructure and follow established conventions for consistency and maintainability.