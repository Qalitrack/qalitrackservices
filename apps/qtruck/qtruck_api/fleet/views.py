from rest_framework import viewsets, status, permissions
from rest_framework.decorators import action
from rest_framework.response import Response
from rest_framework_simplejwt.authentication import JWTAuthentication
from drf_spectacular.utils import extend_schema, extend_schema_view

from .models import Truck, Material, MaterialVariant, MaterialCost, MaterialPhoto, MaterialVariantPhoto
from .serializers import TruckSerializer, MaterialSerializer, MaterialVariantSerializer, MaterialCostSerializer, MaterialPhotoSerializer, MaterialVariantPhotoSerializer
from settings.permissions import (
    IsApproved, IsAdminOrReadOnly, IsAdminOrDriverOrTester
)


@extend_schema_view(
    list=extend_schema(tags=["Fleet"]),
    retrieve=extend_schema(tags=["Fleet"]),
    create=extend_schema(tags=["Fleet"]),
    update=extend_schema(tags=["Fleet"]),
    partial_update=extend_schema(tags=["Fleet"]),
    destroy=extend_schema(tags=["Fleet"])
)
class TruckViewSet(viewsets.ModelViewSet):
    queryset = Truck.objects.all()
    serializer_class = TruckSerializer
    permission_classes = [IsApproved, IsAdminOrReadOnly]  # Admin can modify, others read-only
    
    def get_queryset(self):
        # All approved users can see all trucks (read-only for non-admins)
        if self.request.user.user_type in ['admin', 'driver', 'tester']:
            return Truck.objects.all()
        return Truck.objects.none()


@extend_schema_view(
    list=extend_schema(tags=["Materials"]),
    retrieve=extend_schema(tags=["Materials"]),
    create=extend_schema(tags=["Materials"]),
    update=extend_schema(tags=["Materials"]),
    partial_update=extend_schema(tags=["Materials"]),
    destroy=extend_schema(tags=["Materials"])
)
class MaterialViewSet(viewsets.ModelViewSet):
    queryset = Material.objects.all()
    serializer_class = MaterialSerializer
    permission_classes = [IsApproved, IsAdminOrReadOnly]  # Admin can modify, others read-only
    
    def get_queryset(self):
        # All approved users can see all materials (read-only for non-admins)
        if self.request.user.user_type in ['admin', 'driver', 'tester']:
            return Material.objects.all()
        return Material.objects.none()
    
    @extend_schema(tags=["Materials"])
    @action(detail=True, methods=['get'])
    def variants(self, request, pk=None):
        """Get all variants for a specific material"""
        material = self.get_object()
        variants = material.variants.all()
        serializer = MaterialVariantSerializer(variants, many=True)
        return Response(serializer.data)


@extend_schema_view(
    list=extend_schema(tags=["Materials"]),
    retrieve=extend_schema(tags=["Materials"]),
    create=extend_schema(tags=["Materials"]),
    update=extend_schema(tags=["Materials"]),
    partial_update=extend_schema(tags=["Materials"]),
    destroy=extend_schema(tags=["Materials"])
)
class MaterialVariantViewSet(viewsets.ModelViewSet):
    queryset = MaterialVariant.objects.all()
    serializer_class = MaterialVariantSerializer
    permission_classes = [IsApproved, IsAdminOrReadOnly]
    
    # Explicitly define allowed actions
    http_method_names = ['get', 'post', 'put', 'patch', 'delete', 'head', 'options']
    
    def get_queryset(self):
        # All approved users can see all material variants (read-only for non-admins)
        if self.request.user.user_type in ['admin', 'driver', 'tester']:
            queryset = MaterialVariant.objects.all()
        else:
            queryset = MaterialVariant.objects.none()
        
        # Filter by material if provided
        material_id = self.request.query_params.get('material')
        if material_id:
            queryset = queryset.filter(material_id=material_id)
        
        return queryset


@extend_schema_view(
    list=extend_schema(tags=["Materials"]),
    retrieve=extend_schema(tags=["Materials"]),
    create=extend_schema(tags=["Materials"]),
    update=extend_schema(tags=["Materials"]),
    partial_update=extend_schema(tags=["Materials"]),
    destroy=extend_schema(tags=["Materials"])
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


@extend_schema_view(
    list=extend_schema(tags=["Materials"]),
    retrieve=extend_schema(tags=["Materials"]),
    create=extend_schema(tags=["Materials"]),
    update=extend_schema(tags=["Materials"]),
    partial_update=extend_schema(tags=["Materials"]),
    destroy=extend_schema(tags=["Materials"])
)
class MaterialPhotoViewSet(viewsets.ModelViewSet):
    queryset = MaterialPhoto.objects.all()
    serializer_class = MaterialPhotoSerializer
    permission_classes = [IsApproved, IsAdminOrReadOnly]
    
    def get_queryset(self):
        # All approved users can see material photos
        if self.request.user.user_type in ['admin', 'driver', 'tester']:
            queryset = MaterialPhoto.objects.all()
        else:
            queryset = MaterialPhoto.objects.none()
        
        # Filter by material if provided
        material_id = self.request.query_params.get('material')
        if material_id:
            queryset = queryset.filter(material_id=material_id)
        
        return queryset


@extend_schema_view(
    list=extend_schema(tags=["Materials"]),
    retrieve=extend_schema(tags=["Materials"]),
    create=extend_schema(tags=["Materials"]),
    update=extend_schema(tags=["Materials"]),
    partial_update=extend_schema(tags=["Materials"]),
    destroy=extend_schema(tags=["Materials"])
)
class MaterialVariantPhotoViewSet(viewsets.ModelViewSet):
    queryset = MaterialVariantPhoto.objects.all()
    serializer_class = MaterialVariantPhotoSerializer
    permission_classes = [IsApproved, IsAdminOrReadOnly]
    
    def get_queryset(self):
        # All approved users can see material variant photos
        if self.request.user.user_type in ['admin', 'driver', 'tester']:
            queryset = MaterialVariantPhoto.objects.all()
        else:
            queryset = MaterialVariantPhoto.objects.none()
        
        # Filter by material_variant if provided
        material_variant_id = self.request.query_params.get('material_variant')
        if material_variant_id:
            queryset = queryset.filter(material_variant_id=material_variant_id)
        
        return queryset