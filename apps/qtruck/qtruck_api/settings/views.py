from rest_framework import viewsets, permissions
from rest_framework_simplejwt.authentication import JWTAuthentication
from drf_spectacular.utils import extend_schema_view, extend_schema

from .models import SystemSettings, LicenseClass
from .serializers import SystemSettingsSerializer, LicenseClassSerializer
from .permissions import IsAdmin, IsAdminOrReadOnly


@extend_schema_view(
    list=extend_schema(tags=["Settings"]),
    retrieve=extend_schema(tags=["Settings"]),
    create=extend_schema(tags=["Settings"]),
    update=extend_schema(tags=["Settings"]),
    partial_update=extend_schema(tags=["Settings"]),
    destroy=extend_schema(tags=["Settings"]),
)
class SystemSettingsViewSet(viewsets.ModelViewSet):
    """
    ViewSet for managing system settings.
    Only admins can modify settings.
    """
    queryset = SystemSettings.objects.all()
    serializer_class = SystemSettingsSerializer
    authentication_classes = [JWTAuthentication]
    permission_classes = [IsAdmin]


@extend_schema_view(
    list=extend_schema(tags=["Settings"]),
    retrieve=extend_schema(tags=["Settings"]),
    create=extend_schema(tags=["Settings"]),
    update=extend_schema(tags=["Settings"]),
    partial_update=extend_schema(tags=["Settings"]),
    destroy=extend_schema(tags=["Settings"]),
)
class LicenseClassViewSet(viewsets.ModelViewSet):
    """
    ViewSet for managing license classes.
    Only admins can modify license classes, but all approved users can read them.
    """
    queryset = LicenseClass.objects.all()
    serializer_class = LicenseClassSerializer
    authentication_classes = [JWTAuthentication]
    permission_classes = [IsAdminOrReadOnly]