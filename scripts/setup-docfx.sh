#!/bin/bash

# DocFX Setup Script for QaliTrack Microservices
# Usage: ./setup-docfx.sh <service-name> <service-path>
# Example: ./setup-docfx.sh "Product Service" "packages/microservices/masterdata/product-service"

if [ $# -ne 2 ]; then
    echo "Usage: $0 <service-name> <service-path>"
    echo "Example: $0 'Product Service' 'packages/microservices/masterdata/product-service'"
    exit 1
fi

SERVICE_NAME="$1"
SERVICE_PATH="$2"
SERVICE_KEBAB=$(echo "$SERVICE_NAME" | tr '[:upper:]' '[:lower:]' | tr ' ' '-')
SERVICE_PASCAL=$(echo "$SERVICE_NAME" | sed 's/ //g')

# Check if service path exists
if [ ! -d "$SERVICE_PATH" ]; then
    echo "❌ Service path does not exist: $SERVICE_PATH"
    exit 1
fi

echo "🚀 Setting up DocFX for $SERVICE_NAME at $SERVICE_PATH"

# Create docfx.json
echo "📝 Creating docfx.json..."
cat > "$SERVICE_PATH/docfx.json" << EOF
{
  "\$schema": "https://raw.githubusercontent.com/dotnet/docfx/main/schemas/docfx.schema.json",
  "metadata": [
    {
      "src": [
        {
          "files": ["src/**/*.csproj"],
          "exclude": ["**/bin/**", "**/obj/**"]
        }
      ],
      "dest": "api",
      "includePrivateMembers": false,
      "disableGitFeatures": false,
      "disableDefaultFilter": false,
      "noRestore": false,
      "namespaceLayout": "flattened",
      "memberLayout": "SamePage",
      "allowCompilationErrors": true
    }
  ],
  "build": {
    "content": [
      {
        "files": ["api/**.yml", "api/index.md"]
      },
      {
        "files": ["docs/**.md", "*.md"],
        "exclude": ["**/bin/**", "**/obj/**"]
      }
    ],
    "resource": [
      {
        "files": ["images/**"]
      }
    ],
    "dest": "_site",
    "template": ["default", "modern"],
    "globalMetadata": {
      "_appTitle": "$SERVICE_NAME Documentation",
      "_appName": "$SERVICE_NAME",
      "_appFooter": "QaliTrack - $SERVICE_NAME",
      "_enableSearch": true,
      "_enableNewTab": true,
      "_disableContribution": false,
      "_gitContribute": {
        "repo": "https://github.com/adarlegendre/qalitrackservices",
        "branch": "main"
      }
    }
  }
}
EOF

# Create main index.md
echo "📄 Creating index.md..."
cat > "$SERVICE_PATH/index.md" << EOF
# $SERVICE_NAME

$SERVICE_NAME microservice for the QaliTrack platform.

## Service Type: Masterdata

This service handles core business operations following clean architecture principles.

## Features

- **Clean Architecture**: API, Core, and Infrastructure layers
- **Entity Framework**: SQLite database with EF Core
- **AutoMapper**: Object-to-object mapping
- **Swagger/OpenAPI**: API documentation
- **Logging**: Structured logging with Serilog
- **Health Checks**: Built-in monitoring

## Getting Started

### Prerequisites

- .NET 8.0 SDK
- SQLite (included)

### Running the Service

\`\`\`bash
# Build the service
dotnet build

# Run the service
dotnet run --project src/${SERVICE_PASCAL}.Api

# Access the API
# Swagger UI: http://localhost:5000
# Health Check: http://localhost:5000/health
\`\`\`

### Development

1. **Customize Entities**: Update entities in \`src/${SERVICE_PASCAL}.Core/Entities/\`
2. **Add Business Logic**: Implement services in \`src/${SERVICE_PASCAL}.Core/Services/\`
3. **Configure Database**: Modify \`src/${SERVICE_PASCAL}.Infrastructure/Data/${SERVICE_PASCAL}DbContext.cs\`
4. **Add Controllers**: Create API endpoints in \`src/${SERVICE_PASCAL}.Api/Controllers/\`

## Project Structure

\`\`\`
${SERVICE_PASCAL}/
├── src/
│   ├── ${SERVICE_PASCAL}.Api/           # Web API layer
│   ├── ${SERVICE_PASCAL}.Core/          # Business logic
│   └── ${SERVICE_PASCAL}.Infrastructure/ # Data access
├── tests/
│   └── ${SERVICE_PASCAL}.Tests/         # Unit & integration tests
├── Dockerfile                          # Container configuration
└── ${SERVICE_PASCAL}.sln               # Solution file
\`\`\`

## Testing

\`\`\`bash
# Run all tests
dotnet test

# Run specific test project
dotnet test tests/${SERVICE_PASCAL}.Tests
\`\`\`

## Docker

\`\`\`bash
# Build Docker image
docker build -t $SERVICE_KEBAB .

# Run container
docker run -p 5000:80 $SERVICE_KEBAB
\`\`\`

## Contributing

1. Follow clean architecture principles
2. Add comprehensive tests
3. Update documentation
4. Follow naming conventions

Generated with QaliTrack Service Template 🚀
EOF

# Create main toc.yml
echo "📋 Creating main toc.yml..."
cat > "$SERVICE_PATH/toc.yml" << EOF
- name: Docs
  href: docs/
- name: API
  href: api/
EOF

# Create docs directory if it doesn't exist
mkdir -p "$SERVICE_PATH/docs"

# Create docs/toc.yml
echo "📚 Creating docs/toc.yml..."
cat > "$SERVICE_PATH/docs/toc.yml" << EOF
- name: Introduction
  href: introduction.md

- name: API Overview
  href: api-overview.md

- name: Error Handling
  href: error-handling.md

- name: Security
  href: security.md
  
- name: Deployment
  href: deployment.md
EOF

# Create basic docs files if they don't exist
echo "📖 Creating documentation files..."

if [ ! -f "$SERVICE_PATH/docs/introduction.md" ]; then
cat > "$SERVICE_PATH/docs/introduction.md" << EOF
# Introduction

The **$SERVICE_NAME** is a robust microservice designed for managing core business operations within the QaliTrack platform.

### Key Technologies:
- **Backend:** ASP.NET Core (C#)
- **Authentication:** JWT (JSON Web Tokens)
- **Database:** SQLite with Entity Framework Core
- **API Documentation:** Swagger UI for interactive API documentation and testing

The service provides:
- RESTful API endpoints
- Comprehensive data validation
- Audit trail capabilities
- Health monitoring
- Clean architecture implementation

---
EOF
fi

if [ ! -f "$SERVICE_PATH/docs/api-overview.md" ]; then
cat > "$SERVICE_PATH/docs/api-overview.md" << EOF
# API Overview

The $SERVICE_NAME provides a RESTful API following OpenAPI 3.0 specifications.

## Base URL

\`\`\`
http://localhost:5000/api
\`\`\`

## Authentication

The API uses JWT (JSON Web Token) authentication. Include the token in the Authorization header:

\`\`\`
Authorization: Bearer <your-jwt-token>
\`\`\`

## Response Format

All API responses follow a consistent format:

\`\`\`json
{
  "data": {},
  "message": "Success message",
  "success": true,
  "timestamp": "2025-08-25T00:00:00Z"
}
\`\`\`

## Error Responses

Error responses include detailed information:

\`\`\`json
{
  "error": "Error description",
  "message": "Detailed error message",
  "success": false,
  "timestamp": "2025-08-25T00:00:00Z"
}
\`\`\`

## Content Types

- **Request**: \`application/json\`
- **Response**: \`application/json\`

## Rate Limiting

API endpoints are rate-limited to ensure service stability:
- 100 requests per minute per client
- 1000 requests per hour per client

## Interactive Documentation

Access the Swagger UI for interactive API testing:
\`http://localhost:5000\`
EOF
fi

if [ ! -f "$SERVICE_PATH/docs/error-handling.md" ]; then
cat > "$SERVICE_PATH/docs/error-handling.md" << EOF
# Error Handling

The $SERVICE_NAME implements comprehensive error handling to provide clear feedback and maintain system stability.

## HTTP Status Codes

| Code | Description | When Used |
|------|-------------|-----------|
| 200  | OK | Successful GET, PUT requests |
| 201  | Created | Successful POST requests |
| 204  | No Content | Successful DELETE requests |
| 400  | Bad Request | Invalid request data |
| 401  | Unauthorized | Authentication required |
| 403  | Forbidden | Insufficient permissions |
| 404  | Not Found | Resource doesn't exist |
| 409  | Conflict | Data conflict (e.g., duplicate) |
| 422  | Unprocessable Entity | Validation errors |
| 500  | Internal Server Error | Server-side errors |

## Error Response Structure

\`\`\`json
{
  "error": "ValidationError",
  "message": "The request contains invalid data",
  "details": {
    "field": ["Field is required"],
    "email": ["Email format is invalid"]
  },
  "success": false,
  "timestamp": "2025-08-25T00:00:00Z"
}
\`\`\`

## Common Error Types

### Validation Errors (422)
Returned when request data fails validation rules.

### Not Found Errors (404)
Returned when requested resources don't exist.

### Conflict Errors (409)
Returned when operations conflict with current state.

### Authentication Errors (401)
Returned when authentication is required or invalid.

## Logging

All errors are logged with:
- Error details
- Request context
- User information
- Timestamp
- Stack trace (in development)
EOF
fi

if [ ! -f "$SERVICE_PATH/docs/security.md" ]; then
cat > "$SERVICE_PATH/docs/security.md" << EOF
# Security

The $SERVICE_NAME implements multiple security layers to protect data and ensure secure operations.

## Authentication

### JWT Tokens
- Bearer token authentication
- Token expiration management
- Refresh token support
- Secure token generation

## Authorization

### Role-Based Access Control (RBAC)
- User roles and permissions
- Endpoint-level authorization
- Resource-based permissions
- Administrative controls

## Data Protection

### Input Validation
- Request data sanitization
- SQL injection prevention
- XSS attack prevention
- Input length limits

### Data Encryption
- Sensitive data encryption at rest
- TLS/HTTPS for data in transit
- Secure password hashing
- Token encryption

## Security Headers

The service implements security headers:
- X-Content-Type-Options
- X-Frame-Options
- X-XSS-Protection
- Strict-Transport-Security
- Content-Security-Policy

## Rate Limiting

Protection against abuse:
- Request rate limiting
- IP-based throttling
- User-based limits
- Burst protection

## Audit Logging

Security events are logged:
- Authentication attempts
- Authorization failures
- Data access patterns
- Administrative actions

## Best Practices

1. Always use HTTPS in production
2. Regularly rotate JWT secrets
3. Implement proper CORS policies
4. Monitor security logs
5. Keep dependencies updated
6. Use strong password policies
EOF
fi

if [ ! -f "$SERVICE_PATH/docs/deployment.md" ]; then
cat > "$SERVICE_PATH/docs/deployment.md" << EOF
# Deployment

This guide covers deploying the $SERVICE_NAME to various environments.

## Docker Deployment

### Build Image
\`\`\`bash
docker build -t $SERVICE_KEBAB .
\`\`\`

### Run Container
\`\`\`bash
docker run -d \\
  --name $SERVICE_KEBAB \\
  -p 5000:80 \\
  -e ASPNETCORE_ENVIRONMENT=Production \\
  $SERVICE_KEBAB
\`\`\`

## Docker Compose

\`\`\`yaml
version: '3.8'
services:
  $SERVICE_KEBAB:
    build: .
    ports:
      - "5000:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=Data Source=app.db
    volumes:
      - ./data:/app/data
\`\`\`

## Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| ASPNETCORE_ENVIRONMENT | Environment name | Development |
| ASPNETCORE_URLS | Binding URLs | http://+:80 |
| ConnectionStrings__DefaultConnection | Database connection | Data Source=app.db |

## Health Checks

The service provides health check endpoints:
- \`/health\` - Basic health status
- \`/health/ready\` - Readiness probe
- \`/health/live\` - Liveness probe

## Monitoring

### Metrics
- Request count and duration
- Error rates
- Database connection status
- Custom business metrics

### Logging
- Structured JSON logging
- Log levels: Debug, Info, Warning, Error
- Centralized log aggregation support
- Correlation IDs for tracing

## Production Checklist

- [ ] Environment variables configured
- [ ] Database migrations applied
- [ ] Health checks responding
- [ ] Monitoring configured
- [ ] Security headers enabled
- [ ] HTTPS certificates installed
- [ ] Backup procedures in place
EOF
fi

# Create Dockerfile-docfx for documentation serving
echo "🐳 Creating Dockerfile-docfx..."
cat > "$SERVICE_PATH/Dockerfile-docfx" << EOF
# Use the official Nginx image as the base image
FROM nginx:alpine

# Copy the generated DocFX site to the Nginx HTML folder
COPY _site /usr/share/nginx/html

# Expose port 80 for HTTP access
EXPOSE 80

# Run Nginx in the foreground
CMD ["nginx", "-g", "daemon off;"]
EOF

echo "✅ DocFX setup completed for $SERVICE_NAME!"
echo ""
echo "📋 Next Steps:"
echo "1. Install DocFX: dotnet tool install -g docfx"
echo "2. Generate documentation: cd $SERVICE_PATH && docfx"
echo "3. Serve locally: docfx serve _site"
echo "4. Build Docker docs: docker build -f Dockerfile-docfx -t $SERVICE_KEBAB-docs ."
echo "5. Customize docs in the docs/ folder"
echo ""
echo "📁 Files created:"
echo "  - docfx.json (DocFX configuration)"
echo "  - index.md (Landing page)"
echo "  - toc.yml (Main table of contents)"
echo "  - docs/toc.yml (Documentation TOC)"
echo "  - docs/*.md (Documentation pages)"
echo "  - Dockerfile-docfx (Documentation container)"