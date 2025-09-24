"""
Driver URLs for the drivers app

Target URL structure:
- /drivers/profiles/
- /drivers/profiles/me/
- /drivers/profiles/{id}/approve/ (approve specific DRIVER PROFILE)
- /drivers/profiles/pending/ (list pending DRIVER PROFILES)
- /drivers/activities/
- /drivers/enhanced/
"""

from django.urls import path, include
from rest_framework.routers import DefaultRouter

from .views import (
    DriverViewSet, 
    DriverProfileViewSet, 
    DriverEnhancedViewSet, 
    DriverActivityViewSet
)

# Create router for standard viewsets
router = DefaultRouter()
router.register(r'profiles', DriverProfileViewSet, basename='driverprofile')
router.register(r'activities', DriverActivityViewSet, basename='driveractivity') 
router.register(r'enhanced', DriverEnhancedViewSet, basename='driverenhanced')

# Note: We don't register DriverViewSet with router since we want cleaner URLs
# The basic driver endpoints will be handled by other viewsets or separately

urlpatterns = [
    # Include all the router URLs
    path('', include(router.urls)),
]

# The router will generate these URLs:
# - /profiles/ -> DriverProfileViewSet (list, create)
# - /profiles/{id}/ -> DriverProfileViewSet (retrieve, update, destroy)
# - /profiles/{id}/approve/ -> DriverProfileViewSet.approve (custom action)
# - /profiles/pending/ -> DriverProfileViewSet.pending (custom action)
# - /profiles/me/ -> DriverProfileViewSet.me (custom action)
# - /profiles/{id}/history/ -> DriverProfileViewSet.history (custom action)
# - /activities/ -> DriverActivityViewSet (list, retrieve)
# - /activities/my_activity/ -> DriverActivityViewSet.my_activity (custom action)
# - /activities/my_heatmap/ -> DriverActivityViewSet.my_heatmap (custom action)
# - /enhanced/ -> DriverEnhancedViewSet (list, retrieve)
# - /enhanced/{id}/activity/ -> DriverEnhancedViewSet.activity (custom action)
# - /enhanced/{id}/heatmap/ -> DriverEnhancedViewSet.heatmap (custom action)
# - /enhanced/expiring_licenses/ -> DriverEnhancedViewSet.expiring_licenses (custom action)