# QaliTrack System Architecture Overview

## System Architecture

The QaliTrack weighbridge management system follows a three-layer architecture pattern with centralized authentication and authorization.

### Architecture Layers

```
┌─────────────────────────────────────────────────────────────────┐
│                        Client Applications                       │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐  │
│  │   Web Portal    │  │   Mobile App    │  │   Admin Panel   │  │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
┌─────────────────────────────────────────────────────────────────┐
│                         Gateway Layer                           │
│  ┌─────────────────────────────────────────────────────────────┐ │
│  │                    API Gateway                              │ │
│  │  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────┐  │ │
│  │  │  Authentication │  │   Rate Limiting │  │   Request   │  │ │
│  │  │   Middleware    │  │   & Monitoring  │  │   Routing   │  │ │
│  │  └─────────────────┘  └─────────────────┘  └─────────────┘  │ │
│  └─────────────────────────────────────────────────────────────┘ │
│                                    │                            │
│                                    ▼                            │
│  ┌─────────────────────────────────────────────────────────────┐ │
│  │              User Service (Authentication)                  │ │
│  │              ⚠️  DEPENDENCY REQUIRED                        │ │
│  │  • User Authentication & Authorization                      │ │
│  │  • Role-Based Access Control (RBAC)                        │ │
│  │  • JWT Token Management                                    │ │
│  │  • Permission Validation                                   │ │
│  └─────────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
┌─────────────────────────────────────────────────────────────────┐
│                         Core Services                           │
│  ┌─────────────────────────────────────────────────────────────┐ │
│  │                   Generic Services                          │ │
│  │  (Representing all microservices in the ecosystem)         │ │
│  │                                                             │ │
│  │  • Data Management Services                                │ │
│  │  • Business Logic Services                                 │ │
│  │  • Integration Services                                    │ │
│  │  • Reporting Services                                      │ │
│  │  • Monitoring Services                                     │ │
│  └─────────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────┘
```

### Authentication Dependencies

#### Gateway Layer Authentication Requirements

**🔐 All Gateway Endpoints Require Authentication**
- **No Anonymous Access**: All API endpoints require valid authentication
- **JWT Token Required**: Every request must include a valid JWT token
- **Role-Based Authorization**: Endpoints are protected by role-based access control

#### Required User Roles for Gateway Access

| Access Level | Role Required | Permissions |
|--------------|---------------|-------------|
| **System Operations** | `ADMIN`, `SUPER_ADMIN` | Full system access, configuration management |
| **Site Management** | `SITE_MANAGER`, `ADMIN` | Site-specific operations, reporting |
| **Daily Operations** | `OPERATOR`, `SITE_MANAGER` | Weighbridge operations, basic reporting |
| **Compliance & Audit** | `AUDITOR`, `COMPLIANCE_OFFICER` | Read-only access to compliance reports |
| **Client Access** | `CLIENT_ADMIN`, `CLIENT_USER` | Organization-specific data access |

#### Authentication Flow Dependencies

1. **Client Authentication**
   - Client applications must authenticate with User Service via Gateway
   - Invalid credentials result in `401 Unauthorized`

2. **Gateway Authorization**
   - Gateway validates JWT tokens with User Service
   - Expired or invalid tokens result in `401 Unauthorized`
   - Insufficient permissions result in `403 Forbidden`

3. **Service Access**
   - Core services receive pre-validated user context from Gateway
   - Services trust Gateway authentication (no additional auth required)
   - User context passed via headers: `X-User-ID`, `X-User-Roles`, `X-User-Permissions`

### Security Enforcement Points

#### Gateway Layer Security
- **Authentication Middleware**: Validates JWT tokens for all requests
- **Authorization Middleware**: Checks user roles against endpoint requirements
- **Rate Limiting**: Prevents abuse and ensures service availability
- **Audit Logging**: Records all authentication and authorization events

#### User Service Integration
- **Credential Validation**: Authenticates users against user database
- **Role Resolution**: Provides user roles and permissions to Gateway
- **Token Management**: Issues, validates, and refreshes JWT tokens
- **Session Management**: Handles user sessions and logout

### Architecture Benefits

**Security**
- Single point of authentication reduces attack surface
- Centralized authorization ensures consistent security policies
- Comprehensive audit trail for all access attempts

**Performance**
- Gateway-level caching of user permissions
- Reduced authentication overhead for core services
- Efficient token-based authentication

**Maintainability**
- Centralized security logic in Gateway layer
- Core services focus on business logic
- Simplified security updates and patches

**Scalability**
- Stateless JWT authentication
- Horizontal scaling of Gateway instances
- Independent scaling of core services

---

*This architecture ensures that all system access is properly authenticated and authorized through the Gateway layer, with the User Service providing the foundational authentication and authorization capabilities required for the entire system to function securely.*