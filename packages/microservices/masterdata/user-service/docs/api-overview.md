
### Architecture Overview

The **User Service Microservice** follows a **microservice architecture** that emphasizes modularity, scalability, and separation of concerns. Below is an overview of the different components of the architecture:

#### 1. **Presentation Layer (API Layer):**
   - **ASP.NET Core Web API**: This is the primary entry point for all client requests. It handles HTTP requests and responses.
   - **Swagger UI**: An interactive tool integrated into the API that allows users and developers to explore the API and test its endpoints directly from the browser.

#### 2. **Authentication & Authorization Layer:**
   - **JWT Authentication**: Secures access to protected resources by generating JSON Web Tokens upon successful login. These tokens are used to authenticate subsequent requests.
   - **RBAC (Role-Based Access Control)**: Manages access control by assigning roles to users and associating permissions with those roles. This allows flexible and secure management of access across the system.

#### 3. **Business Logic Layer (Service Layer):**
   - **RoleService**, **PermissionService**, and **ShiftService**: These core services handle the business logic for user management, role assignments, permission validation, and shift scheduling. Each service performs the necessary actions related to its domain and interacts with the data layer.
The User Service Microservice follows a microservice architecture with clear separation of concerns to ensure scalability and maintainability. Below is the breakdown of the architecture:

1. **Presentation Layer (API Layer):**
   - ASP.NET Core Web API: Handles incoming HTTP requests.
   - Swagger UI: Interactive interface for testing API endpoints.

2. **Authentication & Authorization Layer:**
   - JWT Authentication: Ensures secure access to protected resources.
   - Permission-Based Authorization: Uses dynamic policies (via PermissionPolicyProvider) to check user permissions.

3. **Business Logic Layer (Service Layer):**
   - Core services like RoleService, PermissionService, and ShiftService process requests related to user management, role assignments, permission checks, and shift handling.

4. **Data Layer (Persistence Layer):**
   - PostgreSQL Database stores users, roles, permissions, and shifts.
   - Entity Framework Core is used for data access.

5. **Security:**
   - JWT-based Authentication and Authorization Handlers ensure users are authenticated and authorized to access certain resources based on their roles/permissions.

6. **Error Handling & Logging:**
   - Serilog is used for logging, providing structured logs for better traceability.
   - Standardized error responses ensure consistent error handling across the API.



#### 4. **Data Layer (Persistence Layer):**
   - **SQLite Database**: Stores data related to users, roles, permissions, and shifts. SQLite is used as a lightweight, embedded relational database for local development and testing. In production, this could be replaced with more robust systems such as PostgreSQL or MySQL.
   - **Entity Framework Core**: Used for data access, Entity Framework Core (EF Core) facilitates communication between the application and the SQLite database, making CRUD operations easier.

#### 5. **Security:**
   - **JWT-based Authentication and Authorization Handlers**: Ensure users are authenticated with valid JWT tokens and authorized to access specific resources based on their roles and permissions.
   - **Permission-Based Authorization**: Implements dynamic policies through the `PermissionPolicyProvider`, validating permissions in real-time to control access to resources.

#### 6. **Error Handling & Logging:**
   - **Serilog**: A structured logging tool that helps track application behavior, errors, and user actions. Logs are generated in a structured format for easy analysis and troubleshooting.
   - **Standardized Error Responses**: Consistent error handling across all endpoints ensures that all API responses follow the same structure for easier debugging and clarity.

---

This architecture allows the **User Service Microservice** to handle user authentication, role management, shift assignments, and reporting effectively while maintaining security, scalability, and flexibility.

---

### **Next Steps**  
The **User Service Microservice** is designed for easy extension and can be modified to integrate with other services and systems, depending on business requirements.