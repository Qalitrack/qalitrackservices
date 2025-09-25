# Feedback API Consolidation Plan

## Current State Analysis

### Existing ViewSet:
1. **FeedbackViewSet** (`/feedback/`) - Complete feedback management with workflow actions

### Current Endpoints:
- `/api/feedback/` - Standard CRUD operations
- `/api/feedback/{id}/respond/` - Admin response workflow
- `/api/feedback/{id}/resolve/` - Admin resolution workflow
- `/api/feedback/{id}/reject/` - Admin rejection workflow
- `/api/feedback/pending/` - List pending feedback (admin only)
- `/api/feedback/my_feedback/` - Current user's feedback
- `/api/feedback/by_status/` - Filter feedback by status (admin only)

## Consolidation Strategy

### 1. Replace Custom Endpoints with Query Filters

**Endpoints to Replace:**
- `/pending/` → `/feedback/?status=pending`
- `/by_status/?status=X` → `/feedback/?status=X`
- Keep `/my_feedback/` as user-friendly shortcut

### 2. Add Advanced Filtering Capabilities

**New Filter Options:**
- Status filtering: `/?status=pending,reviewed,resolved`
- Type filtering: `/?feedback_type=bug_report,feature_request`
- Date filtering: `/?created_after=2024-01-01&created_before=2024-12-31`
- Response filtering: `/?has_response=true&responded_by=admin_id`
- Search functionality: `/?search=keyword` (across subject and description)

### 3. Add Include Parameter Support

**Include Options:**
- Response details: `/?include=response_details`
- User profile: `/?include=user_profile`
- Combined: `/?include=response_details,user_profile`

### 4. Final API Structure

```
/api/feedback/                          # List with filters + Create
/api/feedback/{id}/                     # Feedback CRUD operations
/api/feedback/{id}/respond/             # Admin response workflow
/api/feedback/{id}/resolve/             # Admin resolution workflow
/api/feedback/{id}/reject/              # Admin rejection workflow
/api/feedback/me/                       # Current user feedback shortcut
```

### 5. Complete Query Filter Examples

**Base URL:** `/api/feedback/`

#### Status Filters (Admin Only)
```http
# Filter by specific status - available: pending, reviewed, in_progress, resolved, rejected
GET /api/feedback/?status=pending
GET /api/feedback/?status=resolved
GET /api/feedback/?status=in_progress
GET /api/feedback/?status=rejected

# Multiple statuses (comma-separated)
GET /api/feedback/?status=pending,reviewed
```

#### Type Filters (All Users)
```http
# Filter by feedback type
GET /api/feedback/?feedback_type=bug_report
GET /api/feedback/?feedback_type=feature_request
GET /api/feedback/?feedback_type=general

# Multiple types
GET /api/feedback/?feedback_type=bug_report,complaint
```

#### Date Filters (All Users)
```http
# Filter by creation date
GET /api/feedback/?created_after=2024-01-01
GET /api/feedback/?created_before=2024-12-31
GET /api/feedback/?created_after=2024-01-01&created_before=2024-12-31

# Filter by response date
GET /api/feedback/?responded_after=2024-01-01
GET /api/feedback/?responded_before=2024-12-31
```

#### Response Filters (Admin Only)
```http
# Filter by response status
GET /api/feedback/?has_response=true
GET /api/feedback/?has_response=false

# Filter by who responded
GET /api/feedback/?responded_by=admin_user_id
```

#### Search Filters (All Users)
```http
# Search across subject and description
GET /api/feedback/?search=login
GET /api/feedback/?search=bug
GET /api/feedback/?search=feature request
```

#### Include Parameter Options (All Users)
```http
# Include additional data - single option
GET /api/feedback/?include=response_details
GET /api/feedback/?include=user_profile

# Include multiple data - comma-separated
GET /api/feedback/?include=response_details,user_profile
```

#### Combined Filter Examples
```http
# Admin combining multiple filters
GET /api/feedback/?status=pending&feedback_type=bug_report&search=login
GET /api/feedback/?has_response=false&created_after=2024-01-01&include=user_profile
GET /api/feedback/?status=resolved&responded_by=admin_id&include=response_details

# User with enhanced data
GET /api/feedback/?feedback_type=feature_request&search=mobile&include=response_details
GET /api/feedback/me/?include=response_details&created_after=2024-01-01
```

#### Access Control
**Admin-Only Filters:** `status` (for filtering others), `has_response`, `responded_by`
**User-Accessible Filters:** `feedback_type`, `created_after`, `created_before`, `search`, `include`
**User Auto-Filtering:** Non-admins automatically see only their own feedback

## Implementation Tasks

### Phase 1: Add FilterSet and Search Backend
1. **Create FeedbackFilterSet**
   - `status` filter (admin only)
   - `feedback_type` filter (all users)
   - `created_after`/`created_before` date filters
   - `has_response` boolean filter (admin only)
   - `responded_by` filter (admin only)

2. **Add Search Backend**
   - Enable search across `subject` and `description` fields
   - Add SearchFilter to filter_backends

### Phase 2: Add Include Parameter Support
1. **Modify FeedbackSerializer**
   - Add conditional fields for response details
   - Add conditional user profile inclusion
   - Handle include parameter parsing

2. **Update ViewSet**
   - Add include parameter processing
   - Modify get_serializer_context for conditional data

### Phase 3: Update Permissions and Filtering
1. **Enhance get_queryset()**
   - Apply admin-only filters with permission checks
   - Maintain user-specific filtering for non-admins
   - Add performance optimizations with select_related/prefetch_related

### Phase 4: Remove Deprecated Endpoints
1. **Update views.py**
   - Remove `pending()` action method
   - Remove `by_status()` action method
   - Keep `my_feedback()` as user-friendly shortcut

2. **Update URL patterns**
   - Remove deprecated endpoint documentation
   - Update comments with new filter examples

### Phase 5: Permission Pattern Consistency
**Follow existing permission patterns from driver consolidation:**

- Use permission classes like `IsAdmin`, `IsDriverOrTester`, `IsApproved`
- Apply permissions at filter level using permission checks
- Follow pattern: authenticated + approved + role-based access

**Example Permission Patterns:**
```python
# Standard CRUD - approved users only, with role-based queryset filtering
permission_classes = [IsApproved, IsDriverOrTester]

# Admin-only filters in get_queryset()
def get_queryset(self):
    queryset = super().get_queryset()
    
    # Admin-only filters
    if not self.request.user.user_type == 'admin':
        # Remove admin-only filter parameters
        if 'status' in self.request.query_params:
            # Non-admins can only see their own feedback regardless of status filter
            pass
    
    return queryset

# Workflow actions remain admin-only
@action(detail=True, methods=['post'], permission_classes=[IsAdmin])
def respond(self, request, pk=None):
```

## Success Criteria

1. ✅ Custom endpoints replaced with query filters where appropriate
2. ✅ Enhanced filtering capabilities added (date, type, search, response status)
3. ✅ Include parameter support for flexible data inclusion
4. ✅ Permission-based filter access maintained
5. ✅ User shortcut endpoint preserved for UX
6. ✅ Performance optimized with proper query optimization
7. ✅ API responses maintain same data structure
8. ✅ No breaking changes for existing API consumers

## Benefits

- **Consistent with driver API pattern** - Same filtering approach across APIs
- **More flexible filtering** - Admin can combine multiple filters
- **Enhanced search capabilities** - Full-text search across feedback content
- **Better performance** - Optimized queries with selective data inclusion
- **Improved discoverability** - Standard query parameters are more intuitive
- **Reduced endpoint complexity** - Fewer custom endpoints to maintain

## Risk Assessment: **LOW RISK** 🟢

- Simple consolidation (only 1 ViewSet vs driver's 4 ViewSets)
- Minimal breaking changes (keeping user shortcuts)
- Clear upgrade path for API consumers
- Well-defined permission boundaries
- Backward compatibility maintained