# Django App Refactoring Plan: Core → Domain-Based Apps

## Objective
Refactor the monolithic `core` app into well-organized domain-based apps with proper RESTful URL structure and clear separation of concerns.

## Current Problems
- Everything is in a single `core` app (53KB views.py, 33KB serializers.py)
- No proper separation of concerns
- Poor maintainability and scalability
- All endpoints under `/api/` without domain separation
- Mixed user account management with business profile management

## Final App Structure & URLs

### 1. **Settings App** (renamed from core)
- **Purpose**: System-wide settings and shared configurations
- **Models**: `BaseModel`, `SystemSettings`, `LicenseClass`
- **URLs**:
  ```
  /settings/system-settings/
  /settings/license-classes/
  ```

### 2. **Auth App** 
- **Purpose**: Authentication only
- **URLs**:
  ```
  /auth/register/
  /auth/login/
  /auth/refresh/
  ```

### 3. **Users App**
- **Purpose**: User account management
- **Models**: `CustomUser`, `UserProfile`
- **URLs**:
  ```
  /users/
  /users/me/
  /users/profiles/
  /users/{id}/approve/      (approve specific USER account)
  /users/pending/           (list pending USER accounts)
  /users/create-admin/
  /users/{id}/deactivate/
  /users/{id}/activate/
  /users/{id}/delete/
  ```

### 4. **Drivers App**
- **Purpose**: Driver business profile management
- **Models**: `Driver`, `DriverProfile`, `DriverActivity`, `DriverProfileChange`
- **URLs**:
  ```
  /drivers/profiles/
  /drivers/profiles/me/
  /drivers/profiles/{id}/approve/   (approve specific DRIVER PROFILE)
  /drivers/profiles/pending/        (list pending DRIVER PROFILES)
  /drivers/activities/
  /drivers/enhanced/
  ```

### 5. **Admin App**
- **Purpose**: System administration only
- **URLs**:
  ```
  /admin/flush-database/
  ```

### 6. **Feedback App**
- **Purpose**: Feedback management
- **Models**: `Feedback`
- **URLs**:
  ```
  /feedback/
  /feedback/{id}/respond/
  /feedback/{id}/resolve/
  /feedback/{id}/reject/
  ```

### 7. **Fleet App**
- **Purpose**: Fleet management
- **Models**: `Truck`, `Material`, `MaterialPhoto`, `MaterialCost`
- **URLs**:
  ```
  /fleet/trucks/
  /fleet/materials/
  /fleet/material-costs/
  ```

### 8. **Trips App**
- **Purpose**: Trip operations and expenses
- **Models**: `Trip`, `Expense`, `Receipt`, `VehicleMileage`
- **URLs**:
  ```
  /fleet/trips/
  /fleet/trips/{id}/expenses/
  /fleet/trips/{id}/receipts/
  /fleet/trips/vehicle-mileage/
  ```

## Key Architectural Decisions

### User vs Driver Profile Separation
- **User Operations** (identity/account level): Creating accounts, user approval, activation/deactivation
- **Driver Profile Operations** (business data level): Profile data, profile approval, activity tracking

### Permission Flow
1. **User Registration** → Creates `CustomUser` (pending approval)
2. **Admin Approves User** → User can log in (`/users/{id}/approve/`)
3. **Driver Updates Profile** → Creates/updates `DriverProfile` (pending approval) 
4. **Admin Approves Driver Profile** → Profile becomes current/active (`/drivers/profiles/{id}/approve/`)

## Implementation Steps
1. **Save refactoring plan to REFACTORING_PLAN.md** ✅
2. **Rename core → settings**
3. **Create new Django apps**: auth, users, drivers, admin, feedback, fleet, trips
4. **Migrate models** to appropriate apps with data migrations
5. **Split views and serializers** by domain
6. **Update URL routing** with new RESTful structure
7. **Update imports** across codebase
8. **Update INSTALLED_APPS** in settings
9. **Update Swagger documentation** to reflect new structure
10. **Preserve all existing permissions and functionality**
11. **Test all endpoints**

## Migration Strategy
- Use Django data migrations to preserve existing data
- Create foreign key relationships between apps
- Maintain backward compatibility during transition
- Test each step thoroughly before proceeding

## Key Benefits
- **Clear Domain Separation**: Each app has focused responsibilities
- **RESTful URL Structure**: Resource-specific actions with proper HTTP methods
- **Better Maintainability**: Smaller, focused files instead of monolithic code
- **Scalability**: Easier to extend and maintain individual domains
- **Team Development**: Different teams can work on different apps
- **Organized Documentation**: Swagger docs grouped by business domain
- **Proper Separation**: User account management separate from business profiles

## URL Structure Summary
- `/settings/` - System configuration
- `/auth/` - Authentication endpoints
- `/users/` - User account management
- `/drivers/` - Driver business profiles
- `/admin/` - System administration
- `/feedback/` - Feedback management
- `/fleet/` - Fleet and materials management
- `/fleet/trips/` - Trip operations

This refactoring transforms the monolithic `core` app into a well-organized, maintainable Django project with proper domain separation and RESTful API design.