# User Service Documentation (Administration Module)

## Overview
This service provides comprehensive administration functionalities for the system, focusing on user management, access control, auditing, and system maintenance.

## Features
- **User Authentication:** Secure sign-in with username/email and password
- **User Access Creation:** Admin-controlled account creation and role assignment
- **Permission Management:** Role-based and individual permission assignment
- **Password Management:** Secure password changes and policy enforcement
- **Audit Trails:** Comprehensive logging of user activities
- **User & Role Management:** Complete CRUD operations for users and roles
- **User Deactivation:** Safe user access restriction without data loss
- **Shift Assignment:** Operational shift management for users
- **User Exit:** Secure logout and session management
- **Reporting:** User lists and activity reports with export capabilities
- **System Backup:** Full and incremental backup with restore functionality

## Architecture

### Service Components
- **Authentication Service:** JWT-based user sign-in and session control
- **Authorization Service:** Role-based access control (RBAC) and permissions
- **User Service:** Complete user lifecycle management
- **Role Service:** Role definition and permission mapping
- **Audit Service:** Immutable activity logging for compliance
- **Shift Service:** User shift assignment and management
- **Reporting Service:** Administrative reports and user lists
- **Backup Service:** Database backup and restoration

### Database Schema (PostgreSQL)

#### Core Tables
- **Users:** User profiles, credentials, status
- **Roles:** Role definitions and hierarchies
- **Permissions:** Granular permission definitions
- **User_Roles:** Many-to-many user-role assignments
- **Role_Permissions:** Many-to-many role-permission mappings
- **Audit_Logs:** Immutable activity tracking
- **Shifts:** User shift assignments with time periods
- **Backups:** Backup metadata and restoration points

## Security Features
- **Password Security:** bcrypt/Argon2 encryption
- **Authentication:** JWT token-based authentication
- **Authorization:** Role-based access control (RBAC)
- **Audit Logging:** Secure, immutable activity logs
- **Session Management:** Secure logout and session invalidation
- **Data Protection:** Compliance with data protection regulations

## API Endpoints

### Authentication
- `POST /auth/login` - User authentication
- `POST /auth/logout` - Secure logout
- `POST /auth/change-password` - Password management

### User Management
- `POST /users` - Create new user
- `GET /users` - List users with filtering
- `PUT /users/{id}` - Update user details
- `DELETE /users/{id}` - Deactivate user

### Role & Permission Management
- `GET /roles` - List available roles
- `POST /roles` - Create new role
- `PUT /users/{id}/roles` - Assign roles to user
- `PUT /roles/{id}/permissions` - Assign permissions to role

### Shift Management
- `POST /shifts` - Create shift assignment
- `GET /shifts/user/{id}` - Get user shifts
- `PUT /shifts/{id}` - Update shift details

### Reporting & Auditing
- `GET /reports/users` - Generate user reports
- `GET /audit-logs` - Retrieve audit trails
- `GET /reports/export` - Export reports (CSV, PDF)

### Backup & Restore
- `POST /backup/create` - Trigger backup
- `GET /backup/status` - Check backup status
- `POST /backup/restore` - Restore from backup

## Technology Stack
- **Backend:** C#/.NET
- **Database:** PostgreSQL
- **Authentication:** JWT
- **Password Hashing:** bcrypt
- **Logging:** ELK Stack
- **Containerization:** Docker
- **Testing:** Unit and integration testing
- **Documentation:** Swagger/OpenAPI

## Development Timeline
- **Week 1:** Core backend setup, authentication, basic user management
- **Week 2:** Advanced features (shifts, audit logging, reporting, backup)
- **Week 3:** Testing, security validation, deployment, documentation

## Compliance & Extensibility
- Data protection regulation compliance
- Modular service architecture for easy feature addition
- API-first design for system integration
- Comprehensive audit trails for regulatory requirements