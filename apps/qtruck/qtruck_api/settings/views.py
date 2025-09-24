from rest_framework import viewsets, permissions
from rest_framework_simplejwt.authentication import JWTAuthentication
from drf_spectacular.utils import extend_schema_view, extend_schema

from .models import SystemSettings, LicenseClass
from .serializers import SystemSettingsSerializer, LicenseClassSerializer
from .permissions import IsAdmin


@extend_schema_view(
    list=extend_schema(),
    retrieve=extend_schema(),
    create=extend_schema(),
    update=extend_schema(),
    partial_update=extend_schema(),
    destroy=extend_schema(),
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
    list=extend_schema(),
    retrieve=extend_schema(),
    create=extend_schema(),
    update=extend_schema(),
    partial_update=extend_schema(),
    destroy=extend_schema(),
)
class LicenseClassViewSet(viewsets.ModelViewSet):
    """
    ViewSet for managing license classes.
    Only admins can modify license classes.
    """
    queryset = LicenseClass.objects.all()
    serializer_class = LicenseClassSerializer
    authentication_classes = [JWTAuthentication]
    permission_classes = [IsAdmin]