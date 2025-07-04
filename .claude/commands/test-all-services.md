# Test All Services

Run comprehensive test suites across all QaliTrack microservices with detailed reporting and validation.

## Complete Test Suite Execution Process

1. **Load System Context**
   - Discover all QaliTrack microservices in the ecosystem
   - Analyze service dependencies and test execution order
   - Identify available test suites and their coverage
   - Map service relationships and integration points
   - Understand current system state and health
   - Validate test environment configuration and readiness

2. **ULTRATHINK**
   - Think hard before executing tests. Create a comprehensive test execution strategy
   - Break down testing into manageable service groups using your TodoWrite tool
   - Use the TodoWrite tool to create and track your test execution plan
   - Identify optimal test execution order based on service dependencies
   - Plan parallel vs sequential execution for maximum efficiency
   - Consider resource constraints and test environment limitations
   - Plan for comprehensive reporting and failure analysis across all services

3. **Execute Complete Test Suite**
   - Run Gateway & Authentication tests (JWT, routing, security - 79+ tests)
   - Execute Master Data Services tests (User, Organization, Vehicle, Driver, etc.)
   - Run Operational Services tests (Weight Data, Transaction, Compliance)
   - Execute Analytics & Reporting tests (Analytics, Archive, Data Sync)
   - Run Integration tests (service-to-service communication, end-to-end workflows)
   - Execute Security tests (authentication, authorization, input validation)
   - Run Performance tests (response times, load testing, resource usage)
   - Generate comprehensive test reports and metrics

4. **Validate**
   - Validate that all services have been tested successfully
   - Check overall test coverage meets QaliTrack standards (>85%)
   - Verify no critical test failures that would block deployment
   - Validate security test results meet compliance requirements
   - Check performance benchmarks are within acceptable ranges
   - Analyze integration test results for service communication issues
   - Fix any test failures and re-run affected test suites
   - Re-validate until all critical tests pass

5. **Complete**
   - Ensure all service test suites have been executed
   - Generate final comprehensive test report with business value summary
   - Validate overall system health and readiness for deployment
   - Check that all test categories passed (Security, API, Business Logic, Integration)
   - Report completion status with detailed metrics and coverage
   - Provide business-focused summary of validated capabilities
   - Document any test failures or issues for team review
   - Confirm entire QaliTrack ecosystem is properly validated

6. **Reference System Requirements**
   - You can always reference individual service tests if issues arise
   - Cross-reference test results with business requirements
   - Ensure all critical business capabilities are validated
   - Verify compliance with security and regulatory requirements

## Test Execution Categories

**Gateway & Authentication**
- API Gateway security and routing (79+ tests)
- User Service authentication and management (85+ tests)
- JWT token validation and role enforcement
- Cross-service authentication integration

**Master Data Services**  
- User Service: Authentication, authorization, user management
- Organization Service: Multi-tenant organization management
- Vehicle Service: Vehicle registration and fleet management
- Driver Service: Driver certification and compliance
- Customer/Supplier Services: Business relationship management

**Operational Services**
- Weight Data Service: Measurement capture and validation
- Transaction Service: Business transaction processing
- Compliance Service: Regulatory compliance checking
- Operational Data Service: Daily operations management

**Analytics & Reporting**
- Analytics Service: Data analysis and insights
- Archive Service: Data retention and retrieval
- Data Sync Service: Multi-site synchronization

### Output Format

```
🧪 QaliTrack Complete Test Suite
================================

🔐 Security & Authentication Tests
  ✅ Gateway Security (79/79 passed)
  ✅ User Service (85/85 passed)
  
📊 Master Data Services  
  ✅ Organization Service (67/67 passed)
  ✅ Vehicle Service (73/73 passed)
  ✅ Driver Service (69/69 passed)
  
⚖️  Operational Services
  ✅ Weight Data Service (81/81 passed)
  ✅ Transaction Service (76/76 passed)
  ✅ Compliance Service (84/84 passed)
  
📈 Analytics & Reporting
  ✅ Analytics Service (72/72 passed)
  ✅ Archive Service (68/68 passed)
  
🔄 Integration Services
  ✅ Data Sync Service (75/75 passed)

================================
🎉 OVERALL RESULTS
Total Tests: 849
Passed: 849 
Failed: 0
Success Rate: 100%

Time: 8m 32s
Coverage: 94.2%
================================
```

### Business Value Reporting

The command also provides business-focused summaries:

```
🏢 Business Capability Validation
=================================

✅ User Management: Complete authentication and authorization
✅ Vehicle Operations: Full fleet management capabilities  
✅ Weight Management: Accurate measurement and compliance
✅ Transaction Processing: Reliable business transaction handling
✅ Compliance Monitoring: Regulatory requirement validation
✅ Data Analytics: Business intelligence and reporting
✅ System Integration: Seamless multi-site operations

🛡️  Security Validation
======================
✅ All endpoints properly secured
✅ Role-based access control functional
✅ Data encryption and validation working
✅ Audit trails properly captured

📊 Performance Metrics  
=====================
✅ Average response time: < 200ms
✅ Database operations: < 50ms
✅ Memory usage: Optimal
✅ Error rate: 0.0%
```

### Prerequisites

- All microservices must be discoverable in `packages/microservices/`
- Services must have test projects generated (use `/create-service-test` for missing ones)
- .NET 8.0 SDK and test dependencies must be available
- Test database connections must be configured

### Configuration

The test runner can be configured via environment variables:

```bash
# Parallel execution (default: true)
QALITRACK_PARALLEL_TESTS=true

# Maximum parallel services (default: 4)  
QALITRACK_MAX_PARALLEL=4

# Test timeout in minutes (default: 10)
QALITRACK_TEST_TIMEOUT=10

# Detailed output (default: false)
QALITRACK_VERBOSE_OUTPUT=false
```

### Integration with CI/CD

This command is designed to integrate with continuous integration pipelines:

```bash
# Exit code 0 = all tests passed
# Exit code 1 = test failures detected
# Exit code 2 = configuration/setup errors

# Example CI usage
/test-all-services
if [ $? -eq 0 ]; then
  echo "✅ All tests passed - deployment ready"
else
  echo "❌ Test failures detected - blocking deployment"
  exit 1
fi
```

### Troubleshooting

Common issues and solutions:

- **Service Not Found**: Use `/create-service-test <service>` to generate missing tests
- **Database Connection**: Ensure test databases are configured and accessible  
- **Authentication Failures**: Verify JWT configuration across all services
- **Timeout Issues**: Increase `QALITRACK_TEST_TIMEOUT` for slower environments

This comprehensive testing approach ensures the entire QaliTrack ecosystem functions correctly as an integrated system.