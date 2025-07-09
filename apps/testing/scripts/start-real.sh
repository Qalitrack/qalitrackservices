#!/bin/bash

# Start testing environment with real services
echo "🚀 Starting testing environment with REAL services..."
echo "📝 Using:"
echo "  - Real User Service (JWT: UserService)"
echo "  - Database-backed authentication"
echo "  - Full user management features"

# Set environment for real services
export USE_MOCK_SERVICES=false
export JWT_ISSUER=UserService
export JWT_AUDIENCE=UserService

# Start with real overlay
docker compose -f docker-compose.testing.yml -f docker-compose.real.yml up -d

echo "✅ Real environment started!"
echo "🔗 Services available at:"
echo "  - Gateway: http://localhost:7000"
echo "  - User Service: http://localhost:7001"
echo "  - Product Service: http://localhost:7005"
echo "  - Customer Service: http://localhost:7003"
echo ""
echo "🧪 Test real authentication:"
echo "curl -X POST http://localhost:7001/api/auth/login -H 'Content-Type: application/json' -d '{\"username\": \"admin\", \"password\": \"password\"}'"