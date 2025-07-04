# Authentication Architecture Design

## Overview
This document outlines the authentication and authorization architecture for the QaliTrack weighbridge management system, designed to support multiple deployment contexts including national weighing (KENHA) and factory operations.

## Architecture Decision: API Gateway Authentication

### Recommended Approach: Centralized Authentication at API Gateway

**Architecture Pattern:**
```
Client → API Gateway (Auth) → Microservice (Business Logic)
```

**Key Components:**
- **API Gateway:** Single authentication point, token validation, request routing
- **User Service:** User management, role definitions, permission matrices
- **Microservices:** Business logic execution with trusted user context

### Technical Benefits

**1. Centralized Security Management**
- Single point for authentication policies and token management
- Consistent security implementation across all services
- Simplified security updates and patches

**2. Performance Optimization**
- Services avoid duplicating authentication validation logic
- Reduced latency through elimination of multiple auth calls
- Gateway-level caching of user permissions

**3. Development Efficiency**
- Microservices focus purely on business logic
- Reduced code duplication across service boundaries
- Simplified service development and testing

**4. Operational Excellence**
- Centralized access logging and security metrics
- Unified monitoring and alerting for authentication events
- Easier debugging and troubleshooting

### Alternative Approaches Considered

**1. Microservice-Level Authentication**
- ❌ Code duplication across services
- ❌ Inconsistent security implementations
- ❌ Higher latency from multiple auth validations
- ✅ More granular control per service

**2. Hybrid Authentication/Authorization**
- Gateway handles authentication (who you are)
- Services handle authorization (what you can do)
- ❌ Increased complexity and debugging difficulty
- ✅ Fine-grained permission control

## Role-Based Access Control (RBAC) Integration

### UserService Integration Pattern

**UserService Responsibilities:**
- Store user profiles, roles, and permissions
- Define role hierarchies and permission matrices
- Provide role validation and permission lookup endpoints
- Manage organizational contexts and site-specific access

**Integration Flow:**
1. **Authentication:** API Gateway validates credentials with UserService
2. **Role Resolution:** Gateway fetches user roles and permissions from UserService
3. **Token Enrichment:** Gateway adds role context to JWT tokens and request headers
4. **Service Authorization:** Individual services check permissions for specific operations

### Permission Model

**Role Hierarchy:**
```
Super Admin: System-wide access, all operations
├── Site Manager: All operations for assigned sites
├── Operator: Operational tasks for assigned shifts
├── Auditor: Read-only access to compliance reports
└── Client Admin: Organization-specific access only
```

**Permission Matrix:**
- **Vehicle Logs:** OPERATOR+ (Operator and above)
- **Weight Compliance Reports:** AUDITOR+ (Auditor and above)
- **Revenue Reports:** MANAGER+ (Manager and above)
- **System Health Monitoring:** ADMIN+ (Admin only)

### Technical Implementation

**API Gateway Enhancements:**
- Implements `/auth/login` endpoint for credential validation
- Caches user permissions with 5-minute TTL for performance
- Forwards enriched headers: `X-User-ID`, `X-User-Roles`, `X-User-Permissions`
- Handles token refresh and session management

**UserService API:**
```
GET /users/{id}/permissions     # Fetch user permissions
GET /users/{id}/roles          # Get user role assignments
POST /auth/validate            # Validate credentials
POST /auth/refresh             # Refresh access tokens
```

**Service-Level Authorization:**
- Services receive pre-validated user context in headers
- Fine-grained permission checking at operation level
- Audit logging for all permission checks and access attempts

## Security Considerations

**Token Management:**
- JWT tokens with short expiration (15 minutes)
- Refresh tokens with longer validity (7 days)
- Secure token storage and transmission

**Access Control:**
- Role-based permissions with organizational boundaries
- Site-specific access controls for multi-tenant deployments
- Audit trails for all authentication and authorization events

**Performance Optimization:**
- Gateway-level permission caching
- Efficient database queries for role resolution
- Minimal service-to-service authentication overhead

## Deployment Contexts

**National Weighing (KENHA):**
- Government inspector roles with compliance-focused permissions
- Site-specific access for different weighbridge locations
- Integration with national vehicle registry systems

**Factory Operations:**
- Company-specific user hierarchies and permissions
- Integration with existing factory management systems
- Supplier and contractor access management

**QalibratedSystems Internal:**
- Multi-tenant access across customer deployments
- Technical support and maintenance roles
- System administration and monitoring capabilities

---

## Gateway Authentication Dependencies

### Visual Architecture Representations

The QaliTrack system architecture is documented with comprehensive visual diagrams:

**🏗️ System Architecture Overview**
- **File**: `system-architecture.png` / `system-architecture.mmd`
- **Shows**: Three-layer architecture (Client → Gateway → Core Services)
- **Highlights**: Critical authentication dependencies at gateway layer
- **Demonstrates**: How User Service is required for all gateway endpoints

**🔄 Gateway Authentication Flow**
- **File**: `gateway-auth-flow.png` / `gateway-auth-flow.mmd` 
- **Shows**: Detailed authentication flow through gateway endpoints
- **Highlights**: Role-based access control for different endpoint types
- **Demonstrates**: How JWT tokens and user context flow through the system

### Gateway Endpoint Authentication Matrix

All API Gateway endpoints require authentication except for specific public endpoints:

| Endpoint Pattern | Access Level | Required Role | Authentication |
|------------------|--------------|---------------|----------------|
| `/api/auth/*` | 🔓 **PUBLIC** | None | No JWT required |
| `/health` | 🔓 **PUBLIC** | None | No JWT required |
| `/api/swagger` | 🔓 **PUBLIC** | None | No JWT required |
| `/api/users/*` | 🔒 **AUTHENTICATED** | Any valid user | JWT required |
| `/api/vehicles/*` | 🔒 **OPERATOR+** | Operator or above | JWT + Role check |
| `/api/weight/*` | 🔒 **OPERATOR+** | Operator or above | JWT + Role check |
| `/api/reports/*` | 🔒 **AUDITOR+** | Auditor or above | JWT + Role check |
| `/api/compliance/*` | 🔒 **AUDITOR+** | Auditor or above | JWT + Role check |
| `/api/analytics/*` | 🔒 **MANAGER+** | Manager or above | JWT + Role check |
| `/api/organizations/*` | 🔒 **ADMIN+** | Admin or above | JWT + Role check |
| `/api/archive/*` | 🔒 **ADMIN+** | Admin or above | JWT + Role check |

### Critical System Dependencies

**⚠️ User Service is a Critical Dependency**
- **All authenticated endpoints** depend on the User Service being operational
- **Gateway cannot function** without User Service for authentication
- **No fallback authentication** - User Service is the single source of truth
- **Service startup order** must ensure User Service is available before Gateway

**Authentication Flow Requirements**
1. **Client Authentication**: All requests must include valid JWT tokens (except public endpoints)
2. **Gateway Validation**: Gateway validates tokens with User Service before forwarding
3. **Role Enforcement**: Gateway checks user roles against endpoint requirements
4. **Context Forwarding**: Gateway adds user context headers to downstream service requests

### Service Port Configuration

The gateway routes to 19 microservices with the following authentication requirements:

- **Port 7001**: User Service (Authentication hub - always required)
- **Ports 7002-7018**: Business services (receive pre-authenticated context)
- **Port 7000**: Gateway (validates all authentication)

*This architecture provides a scalable, secure, and maintainable authentication system suitable for diverse weighbridge management scenarios while maintaining consistent security standards across all deployment contexts.*