# QaliTrack Testing Suite

*Comprehensive testing infrastructure for the QaliTrack microservices ecosystem*

## 📁 Directory Structure

```
testing-unified/
├── docs/
│   └── TESTING.md              # Complete testing guide and documentation
├── http-tests/
│   └── gateway-user-test.http  # HTTP API test scenarios
├── scripts/
│   └── auth_config_generator_test.py  # Authorization config generator tests
└── README.md                   # This file
```

## 🧪 Testing Components

### Documentation (`docs/`)
- **TESTING.md**: Comprehensive testing guide covering architecture, standards, execution, and troubleshooting
- Testing strategies for all service types
- Business-friendly make command documentation
- Claude command integration for test generation

### HTTP API Tests (`http-tests/`)
- **gateway-user-test.http**: REST Client test scenarios for gateway and user service integration
- Authentication flow testing through gateway
- Direct service vs gateway routing comparison
- Error handling and edge case validation

### Test Scripts (`scripts/`)
- **auth_config_generator_test.py**: Python test suite for authorization configuration generator
- Validates YAML rule processing, Ocelot config generation, and gateway integration
- 85+ comprehensive tests covering all authorization scenarios

## 🚀 Quick Start

### Run All Tests
```bash
# Execute complete test suite
make test-all

# Interactive test selection
make test-interactive
```

### Service-Specific Tests
```bash
# Gateway security tests
make test-gateway

# User service tests  
make test-users

# Authorization config tests
make auth-config-test
```

### HTTP API Testing
```bash
# Use REST Client extension in VS Code to run gateway-user-test.http
# Or use curl/httpie with the endpoints defined in the file
```

## 📊 Test Coverage

### Current Implementation
- **Gateway Tests**: 79+ security and routing tests
- **User Service Tests**: 85+ authentication and management tests
- **Authorization Tests**: 85+ configuration generator tests
- **Integration Tests**: HTTP API flow validation

### Testing Standards
- xUnit framework for .NET services
- FluentAssertions for readable test assertions
- Moq for mocking dependencies
- WebApplicationFactory for integration testing
- Business-friendly make targets with descriptive output

## 🔧 Test Generation

Use Claude commands to generate comprehensive test suites:

```bash
# Generate tests for any service
/create-service-test vehicle-service

# Test all services with detailed reporting
/test-all-services

# Generate documentation for services
/generate-docs weight-data-service
```

## 📈 Integration with CI/CD

Tests are designed for continuous integration:
- Exit codes indicate test success/failure
- Detailed reporting for build pipelines
- Coverage reporting integration
- Performance benchmark validation

## 🛠️ Tools and Frameworks

- **.NET Testing**: xUnit, FluentAssertions, Moq, WebApplicationFactory
- **Python Testing**: pytest for configuration scripts
- **HTTP Testing**: REST Client files for API validation
- **Make Integration**: Business-friendly test execution commands
- **Docker Health Checks**: Container-level health validation

## 📚 Related Documentation

- [Main Testing Guide](docs/TESTING.md) - Complete testing documentation
- [Gateway Documentation](../docs/packages/gateway/) - Gateway-specific testing
- [User Service Guide](../docs/packages/user/) - User service testing
- [Implementation Gaps](../IMPLEMENTATION_GAPS.md) - Known limitations and gaps

---

*This unified testing suite ensures comprehensive validation of the QaliTrack ecosystem while providing clear guidance for developers and stakeholders.*