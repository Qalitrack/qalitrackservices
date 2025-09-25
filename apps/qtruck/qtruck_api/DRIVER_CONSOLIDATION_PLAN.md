# Driver API Consolidation Plan

## Current State Analysis

### Existing ViewSets:
1. **DriverViewSet** - Basic driver CRUD (not exposed via router)
2. **DriverProfileViewSet** (`/profiles/`) - Profile management with approval workflow
3. **DriverEnhancedViewSet** (`/enhanced/`) - Enhanced driver data with analytics  
4. **DriverActivityViewSet** (`/activities/`) - Activity tracking

### Current Endpoints:
- `/api/drivers/profiles/` - Standard CRUD operations
- `/api/drivers/profiles/{id}/approve/` - Admin approval workflow
- `/api/drivers/profiles/pending/` - List pending profiles (admin only)
- `/api/drivers/profiles/me/` - Current driver's profile
- `/api/drivers/profiles/{id}/history/` - Profile change history
- `/api/drivers/enhanced/{id}/activity/` - Get driver activity data
- `/api/drivers/enhanced/{id}/heatmap/` - Activity heatmap data  
- `/api/drivers/enhanced/expiring_licenses/` - Drivers with expiring licenses
- `/api/drivers/activities/my_activity/` - Current driver's activity
- `/api/drivers/activities/my_heatmap/` - Current driver's heatmap

## Consolidation Strategy

### 1. Merge All Functionality into Single DriverProfileViewSet

**Remove ViewSets:**
- `DriverViewSet` (basic CRUD - redundant)
- `DriverEnhancedViewSet` (merge into DriverProfileViewSet)
- `DriverActivityViewSet` (merge into DriverProfileViewSet)

### 2. Replace Custom Endpoints with Query Filters

**Endpoints to Replace:**
- `/pending/` → `/profiles/?status=pending`
- `/expiring_licenses/` → `/profiles/?license_expiring=30` or `/profiles/?license_expires_before=YYYY-MM-DD`
- Activity data → `/profiles/{id}/?include=activity&days=30`
- Stats data → `/profiles/me/?include=stats,activity`

### 3. Final API Structure

```
/api/drivers/profiles/                    # List with filters + Create
/api/drivers/profiles/{id}/               # Profile CRUD operations
/api/drivers/profiles/{id}/approve/       # Admin approval workflow
/api/drivers/profiles/{id}/heatmap/       # Activity heatmap (complex data)
/api/drivers/profiles/{id}/history/       # Profile change history
/api/drivers/profiles/me/                 # Current driver profile shortcut
```

### 4. Complete Query Filter Examples

**Base URL:** `/api/drivers/`

#### Status Filters (Admin Only)
```http
# Filter by specific status - available: pending, approved, draft, rejected
GET /api/drivers/?status=pending
GET /api/drivers/?status=approved
GET /api/drivers/?status=draft
GET /api/drivers/?status=rejected
```

#### License-Related Filters (Admin Only)
```http
# Find drivers with licenses expiring within N days
GET /api/drivers/?license_expiring=30
GET /api/drivers/?license_expiring=7
GET /api/drivers/?license_expiring=90

# Find drivers with licenses expiring before specific date
GET /api/drivers/?license_expires_before=2024-12-31
GET /api/drivers/?license_expires_before=2025-01-15
```

#### Boolean Filters (All Users)
```http
# Filter by pending version status
GET /api/drivers/?has_pending_version=true
GET /api/drivers/?has_pending_version=false
```

#### Include Parameter Options (All Users)
```http
# Include enhanced data - single option
GET /api/drivers/?include=stats
GET /api/drivers/?include=activity
GET /api/drivers/?include=heatmap

# Include multiple enhanced data - comma-separated
GET /api/drivers/?include=stats,activity
GET /api/drivers/?include=activity,heatmap
GET /api/drivers/?include=stats,activity,heatmap
```

#### Enhanced Data Parameters
```http
# Control activity data timeframe (works with include=activity)
GET /api/drivers/?include=activity&days=30
GET /api/drivers/?include=activity&days=7
GET /api/drivers/?include=activity&days=90

# Control heatmap data timeframe (works with include=heatmap)
GET /api/drivers/?include=heatmap&days=90
GET /api/drivers/?include=heatmap&days=30
GET /api/drivers/?include=heatmap&days=180
```

#### Combined Filter Examples
```http
# Admin combining multiple filters
GET /api/drivers/?status=approved&license_expiring=30&include=stats
GET /api/drivers/?status=pending&has_pending_version=true
GET /api/drivers/?license_expires_before=2024-12-31&include=activity,stats&days=60

# Non-admin user with enhanced data
GET /api/drivers/?has_pending_version=false&include=stats,activity,heatmap&days=45
```

#### Individual Driver Actions
```http
# Get current user's driver profile with enhanced data
GET /api/drivers/me/
GET /api/drivers/me/?include=stats
GET /api/drivers/me/?include=activity&days=30
GET /api/drivers/me/?include=heatmap&days=90
GET /api/drivers/me/?include=stats,activity,heatmap

# Get specific driver activity
GET /api/drivers/{driver_id}/activity/
GET /api/drivers/{driver_id}/activity/?days=30

# Get specific driver heatmap data
GET /api/drivers/{driver_id}/heatmap/
GET /api/drivers/{driver_id}/heatmap/?days=90
```

#### Access Control
**Admin-Only Filters:** `status`, `license_expiring`, `license_expires_before`
**User-Accessible Filters:** `has_pending_version`, `include`, `days`

## Implementation Tasks

### Phase 1: Merge ViewSets
1. **Move Enhanced ViewSet methods to DriverProfileViewSet**
   - Move `activity()` method → `@action(detail=True)` in DriverProfileViewSet
   - Move `heatmap()` method → `@action(detail=True)` in DriverProfileViewSet  
   - Move `expiring_licenses()` method → implement as query filter

2. **Move Activity ViewSet methods to DriverProfileViewSet**
   - Move `my_activity()` functionality → add to `/me/` endpoint with `?include=activity`
   - Move `my_heatmap()` functionality → add to `/me/` endpoint with `?include=heatmap`

3. **Remove Basic DriverViewSet**
   - Ensure no functionality is lost
   - Remove from imports and URL registration

### Phase 2: Implement Query Filters
1. **Add FilterSet class for DriverProfileViewSet**
   - `status` filter (pending, approved, draft, rejected)
   - `license_expiring` filter (days until expiry)
   - `license_expires_before` filter (date)
   - `has_pending_version` filter (boolean)

2. **Add `include` parameter support**
   - Modify serializers to conditionally include activity data
   - Add query parameter parsing in ViewSet
   - Include stats, activity data based on request

### Phase 3: Update URLs and Remove Old Code
1. **Update drivers/urls.py**
   - Remove DriverEnhancedViewSet registration
   - Remove DriverActivityViewSet registration
   - Keep only DriverProfileViewSet registration

2. **Remove old ViewSet files**
   - Delete or comment out unused ViewSet classes
   - Clean up imports

3. **Update serializers if needed**
   - Ensure DriverProfileSerializer can include enhanced data
   - Add conditional fields for activity/stats

### Phase 4: Permission Pattern Consistency
**Follow existing permission patterns from authentication, settings, and users apps:**

- Use permission classes like `IsAdmin`, `IsDriver`, `IsApproved`
- Apply permissions at action level using `permission_classes` parameter
- Use `get_permissions()` method for conditional permissions
- Follow pattern: authenticated + approved + role-based access

**Example Permission Patterns:**
```python
# Standard CRUD - approved users only, with role-based queryset filtering
permission_classes = [IsApproved]

# Admin-only actions
@action(detail=False, permission_classes=[IsAdmin])
def some_admin_action(self, request):

# Driver-only actions  
@action(detail=False, permission_classes=[IsApproved, IsDriver])
def driver_specific_action(self, request):

# Conditional permissions in get_permissions()
def get_permissions(self):
    if self.action in ['create', 'update']:
        return [IsApproved(), IsAdminOrDriver()]
    return [IsApproved()]
```

## Success Criteria

1. ✅ Reduced from 4 ViewSets to 1 comprehensive DriverProfileViewSet
2. ✅ All functionality preserved and accessible
3. ✅ Custom endpoints replaced with query filters where appropriate
4. ✅ URL structure simplified and consistent
5. ✅ Old code completely removed
6. ✅ Permission patterns consistent with other apps
7. ✅ API responses maintain same data structure
8. ✅ No breaking changes for existing API consumers

## Benefits

- **Simplified API surface** - 6 endpoints instead of 10+
- **Consistent filtering** - Standard query parameters across all list endpoints
- **Reduced maintenance** - Single ViewSet to maintain instead of 4
- **Better discoverability** - All driver functionality in one logical place
- **Flexible data inclusion** - Clients can request exactly the data they need