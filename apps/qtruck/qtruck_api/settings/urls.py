from django.urls import path, include
from rest_framework.routers import DefaultRouter

from .views import SystemSettingsViewSet, LicenseClassViewSet

router = DefaultRouter()
router.register(r'system-settings', SystemSettingsViewSet)
router.register(r'license-classes', LicenseClassViewSet)

urlpatterns = [
    # Settings endpoints
    path('', include(router.urls)),
]