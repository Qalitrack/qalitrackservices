# Generate Service Documentation

Automatically generate comprehensive user and technical documentation for any QaliTrack microservice.

## Service Name: $ARGUMENTS

## Documentation Generation Process

1. **Load Service Context**
   - Read and understand the service name: $ARGUMENTS
   - Analyze the existing service codebase structure and architecture
   - Examine controllers, DTOs, services, repositories, and data models
   - Map API endpoints, authentication requirements, and business logic
   - Understand service dependencies and integration points
   - Review existing documentation patterns from other QaliTrack services

2. **ULTRATHINK**
   - Think hard before generating documentation. Create a comprehensive documentation strategy
   - Break down documentation into manageable sections using your TodoWrite tool
   - Use the TodoWrite tool to create and track your documentation generation plan
   - Identify existing documentation patterns from QaliTrack codebase to follow
   - Map service functionality to specific documentation sections (API, User Guide, Architecture)
   - Consider user personas (developers, end-users, system administrators)
   - Plan for comprehensive coverage including business context and technical details

3. **Execute Documentation Generation**
   - Generate README.md with overview and quick start guide
   - Create user-guide.md for business users and system administrators
   - Generate api-reference.md with complete endpoint documentation and examples
   - Create technical-architecture.md with implementation details and diagrams
   - Generate database-schema.md with ERD and table definitions
   - Create integration-guide.md for service-to-service communication
   - Generate testing-guide.md with test strategies and examples
   - Create troubleshooting.md with common issues and solutions
   - Generate performance-tuning.md with optimization guidelines
   - Create FAQ.md with frequently asked questions

4. **Validate**
   - Validate that all documentation files are created and properly formatted
   - Check that all API endpoints are documented with examples
   - Verify code examples compile and work correctly
   - Validate that business context is clearly explained
   - Test all curl examples and request/response samples
   - Check that architecture diagrams accurately represent the service
   - Verify cross-references to other services are correct
   - Fix any documentation gaps or inaccuracies
   - Re-validate until all documentation sections are complete

5. **Complete**
   - Ensure all documentation sections are generated and comprehensive
   - Verify documentation follows QaliTrack standards and formatting
   - Check that documentation integrates with existing QaliTrack documentation structure
   - Validate that business value and technical implementation are both covered
   - Generate final documentation package with proper cross-references
   - Report generation completion status with metrics
   - Confirm service now has complete documentation coverage
   - Update main documentation index with new service documentation

6. **Reference Service Requirements**
   - You can always reference the service code again if needed during generation
   - Cross-reference documentation with actual service implementation
   - Ensure no API endpoints or features are left undocumented
   - Verify business use cases align with technical implementation

## Generated Documentation Structure

```
docs/packages/{service-name}/
├── README.md                    # Overview and quick start
├── user-guide.md               # End-user documentation
├── api-reference.md            # Complete API documentation
├── technical-architecture.md   # Technical implementation details
├── database-schema.md          # Database design and relationships
├── integration-guide.md        # How to integrate with the service
├── testing-guide.md           # Testing documentation
├── troubleshooting.md         # Common issues and solutions
├── performance-tuning.md      # Optimization guidelines
└── faq.md                     # Frequently asked questions
```

## Documentation Features

**User Documentation**
- Clear API examples with real-world usage scenarios
- Business context explaining service purpose and value
- Step-by-step workflows and business processes
- User-friendly error explanations and solutions

**Technical Documentation** 
- Architecture diagrams and visual representations
- Complete database schema with ERD and relationships
- Configuration guides with environment variables
- Integration patterns for service-to-service communication
- Security implementation details for authentication and authorization

**API Reference**
- Complete endpoint coverage with request/response examples
- Real JSON examples for every API endpoint
- HTTP status code reference with explanations
- Authentication and authorization requirements per endpoint
- Rate limiting and usage guidelines

## Auto-Generated Content Analysis

The system automatically generates documentation by:
1. **Code Analysis**: Examines controllers, DTOs, and service interfaces
2. **Database Inspection**: Analyzes Entity Framework models and relationships
3. **Configuration Parsing**: Extracts settings and environment variables
4. **Test Analysis**: Uses existing tests to generate usage examples
5. **OpenAPI Integration**: Leverages Swagger/OpenAPI definitions

## Validation Commands

```bash
# Validate documentation completeness
ls -la docs/packages/[service-name]/

# Check API examples work
curl -X GET http://localhost:700X/api/[endpoint]

# Verify documentation formatting
markdown-lint docs/packages/[service-name]/*.md

# Test code examples
dotnet test --filter Category=Documentation
```

## Error Patterns & Solutions

Common issues during documentation generation:
- **Missing API endpoints**: Ensure all controllers are properly documented
- **Incomplete examples**: Verify all DTOs have example data
- **Broken cross-references**: Check that referenced services exist
- **Formatting issues**: Follow QaliTrack markdown standards
- **Missing business context**: Add business value explanations

## Completion Criteria

Documentation generation is complete when:
- [ ] All 10 documentation files are created
- [ ] Every API endpoint is documented with examples
- [ ] Business context is clearly explained
- [ ] Technical architecture is accurately represented
- [ ] All code examples are tested and working
- [ ] Cross-references to other services are correct
- [ ] Documentation follows QaliTrack formatting standards
- [ ] Integration examples are comprehensive

## Usage Examples

```bash
# Generate complete documentation for vehicle service
/generate-docs vehicle-service

# Generate docs for user authentication service  
/generate-docs user-service

# Generate docs for weight data management service
/generate-docs weight-data-service

# Generate docs for compliance monitoring service
/generate-docs compliance-service
```

## Example Generated Content

**API Endpoint Documentation**:
```markdown
### POST /api/vehicles
Register a new vehicle in the system.

**Authentication Required**: Yes (Operator+ role)

**Request Body**:
```json
{
  "licensePlate": "ABC-123",
  "vehicleType": "Truck", 
  "capacity": 5000,
  "ownerId": "uuid-here"
}
```

**Response (201 Created)**:
```json
{
  "success": true,
  "data": {
    "id": "vehicle-uuid",
    "licensePlate": "ABC-123",
    "status": "Active",
    "createdAt": "2024-07-04T10:30:00Z"
  }
}
```

**Business Use Case**: Register new vehicles for weighbridge operations
**Error Handling**: Returns 409 if license plate already exists
```

## Post-Generation Validation

After successful documentation generation:
1. Review all generated documentation files for accuracy
2. Test all API examples and code snippets
3. Verify business context aligns with service functionality
4. Check cross-references to other services work correctly
5. Validate architecture diagrams represent current implementation
6. Update main documentation index with new service links
7. Share documentation with team for review and feedback

Note: If generation fails, analyze error patterns and retry. Generated documentation integrates with existing QaliTrack documentation infrastructure and follows established formatting standards for consistency and maintainability.