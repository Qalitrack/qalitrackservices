from rest_framework import viewsets, status, permissions
from rest_framework.decorators import action
from rest_framework.response import Response
from rest_framework_simplejwt.authentication import JWTAuthentication
from drf_spectacular.utils import extend_schema, extend_schema_view

from .models import Truck, Material, MaterialCost
from .serializers import TruckSerializer, MaterialSerializer, MaterialCostSerializer
from settings.permissions import (
    IsApproved, IsAdminOrReadOnly, IsAdminOrDriverOrTester
)


@extend_schema_view(
    list=extend_schema(),
    retrieve=extend_schema(),
    create=extend_schema(),
    update=extend_schema(),
    partial_update=extend_schema(),
    destroy=extend_schema(),
)
class TruckViewSet(viewsets.ModelViewSet):
    queryset = Truck.objects.all()
    serializer_class = TruckSerializer
    permission_classes = [IsApproved, IsAdminOrReadOnly]  # Admin can modify, others read-only
    
    def get_queryset(self):
        if self.request.user.user_type == 'admin':
            return Truck.objects.all()
        elif self.request.user.user_type in ['driver', 'tester']:
            driver = getattr(self.request.user, 'driver_profile', None)
            if driver:
                return Truck.objects.filter(driver=driver)
        return Truck.objects.none()


@extend_schema_view(
    list=extend_schema(),
    retrieve=extend_schema(),
    create=extend_schema(),
    update=extend_schema(),
    partial_update=extend_schema(),
    destroy=extend_schema(),
)
class MaterialViewSet(viewsets.ModelViewSet):
    queryset = Material.objects.all()
    serializer_class = MaterialSerializer
    permission_classes = [IsApproved, IsAdminOrReadOnly]  # Admin can modify, others read-only
    
    def get_queryset(self):
        if self.request.user.user_type == 'admin':
            return Material.objects.all()
        elif self.request.user.user_type in ['driver', 'tester']:
            return Material.objects.all()  # All users can see materials
        return Material.objects.none()


@extend_schema_view(
    list=extend_schema(),
    retrieve=extend_schema(),
    create=extend_schema(),
    update=extend_schema(),
    partial_update=extend_schema(),
    destroy=extend_schema(),
)
class MaterialCostViewSet(viewsets.ModelViewSet):
    queryset = MaterialCost.objects.all()
    serializer_class = MaterialCostSerializer
    permission_classes = [IsApproved, IsAdminOrDriverOrTester]
    
    def get_queryset(self):
        if self.request.user.user_type == 'admin':
            return MaterialCost.objects.all()
        elif self.request.user.user_type in ['driver', 'tester']:
            # Users can see all material costs for reference
            return MaterialCost.objects.all()
        return MaterialCost.objects.none()