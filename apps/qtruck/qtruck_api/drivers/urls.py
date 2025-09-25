"""
Driver URLs for the drivers app - Phase 1: ViewSet Consolidation

This module now contains a single consolidated DriverProfileViewSet that provides
all driver-related functionality through one comprehensive endpoint.

Consolidated URL structure:
- /profiles/ -> All driver profile operations with enhanced functionality
- /profiles/me/ -> Current driver's profile with include parameter support  
- /profiles/{id}/approve/ -> Admin approval workflow
- /profiles/pending/ -> Pending profiles for admin review
- /profiles/{id}/activity/ -> Driver activity tracking
- /profiles/{id}/heatmap/ -> Activity heatmap data
- /profiles/expiring_licenses/ -> Expiring license alerts
"""

from django.urls import path, include
from rest_framework.routers import DefaultRouter

from .views import DriverProfileViewSet

# Create router for the consolidated viewset
router = DefaultRouter()
router.register(r'profiles', DriverProfileViewSet, basename='driverprofile')

urlpatterns = [
    # Include all the consolidated router URLs
    path('', include(router.urls)),
]

# The consolidated router generates these URLs with all functionality:
# 
# Standard CRUD:
# - GET /profiles/ -> List all driver profiles (admin) or own profiles (driver)
# - POST /profiles/ -> Create new driver profile
# - GET /profiles/{id}/ -> Retrieve specific driver profile
# - PUT /profiles/{id}/ -> Update driver profile
# - PATCH /profiles/{id}/ -> Partial update driver profile  
# - DELETE /profiles/{id}/ -> Delete driver profile
#
# Enhanced Actions:
# - GET /profiles/me/ -> Get current driver's profile
# - PATCH /profiles/me/ -> Update current driver's profile
# - GET /profiles/me/?include=activity,stats,heatmap -> Enhanced profile data
# - POST /profiles/{id}/approve/ -> Approve/reject/request changes
# - GET /profiles/pending/ -> List pending profiles (admin)
# - GET /profiles/{id}/history/ -> Get profile change history
# - GET /profiles/{id}/activity/ -> Get driver activity data
# - GET /profiles/{id}/heatmap/ -> Get activity heatmap
# - GET /profiles/expiring_licenses/ -> Get drivers with expiring licenses