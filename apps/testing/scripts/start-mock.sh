#!/bin/bash

# Start testing environment with mock services
echo "🚀 Starting testing environment with MOCK services..."
echo "📝 Using:"
echo "  - Mock User Service (JWT: MockUserService)"
echo "  - Mock authentication endpoints"
echo "  - Role-based authorization testing"

# Set environment for mock services
export USE_MOCK_SERVICES=true
export JWT_ISSUER=MockUserService
export JWT_AUDIENCE=MockUserService

# Start with mock overlay
docker compose -f docker-compose.testing.yml -f docker-compose.mock.yml up -d

echo "✅ Mock environment started!"
echo "🔗 Services available at:"
echo "  - Gateway: http://localhost:7000"
echo "  - Mock User Service: http://localhost:7001"
echo "  - Product Service: http://localhost:7005"
echo "  - Customer Service: http://localhost:7003"
echo ""
echo "🧪 Test mock login:"
echo "curl -X POST http://localhost:7001/api/MockAuth/mock-login -H 'Content-Type: application/json' -d '{\"username\": \"testuser\", \"role\": \"User\"}'"