from rest_framework import viewsets, status, permissions
from rest_framework.decorators import action
from rest_framework.response import Response
from rest_framework_simplejwt.authentication import JWTAuthentication
from rest_framework import filters
from rest_framework.parsers import MultiPartParser, FormParser, JSONParser
from django_filters.rest_framework import DjangoFilterBackend
from django_filters import rest_framework as filters_rest
from drf_spectacular.utils import extend_schema, extend_schema_view
from django.utils import timezone

from .models import Trip, TripType, Expense, Receipt, VehicleMileage, TripMaterial
from .serializers import TripSerializer, TripTypeSerializer, ExpenseSerializer, ReceiptSerializer, VehicleMileageSerializer, TripMaterialSerializer
from settings.permissions import (
    IsApproved, IsAdminOrDriverOrTester
)


class TripFilterSet(filters_rest.FilterSet):
    date_from = filters_rest.DateFilter(field_name="date", lookup_expr='gte')
    date_to = filters_rest.DateFilter(field_name="date", lookup_expr='lte')
    
    class Meta:
        model = Trip
        fields = ['status', 'truck', 'driver', 'material', 'material_variant', 'date_from', 'date_to']


@extend_schema_view(
    list=extend_schema(tags=["Trips"]),
    retrieve=extend_schema(tags=["Trips"]),
    create=extend_schema(tags=["Trips"]),
    update=extend_schema(tags=["Trips"]),
    partial_update=extend_schema(tags=["Trips"]),
    destroy=extend_schema(tags=["Trips"]),
    calculate_cost=extend_schema(tags=["Trips"]),
    by_status=extend_schema(tags=["Trips"]),
    upload_photos=extend_schema(tags=["Trips"]),
    start_trip=extend_schema(tags=["Trips"]),
    end_trip=extend_schema(tags=["Trips"]),
)
class TripViewSet(viewsets.ModelViewSet):
    queryset = Trip.objects.all()
    serializer_class = TripSerializer
    permission_classes = [IsApproved, IsAdminOrDriverOrTester]
    filter_backends = [DjangoFilterBackend, filters.SearchFilter, filters.OrderingFilter]
    filterset_class = TripFilterSet
    search_fields = ['start_location', 'end_location', 'truck__license_plate', 'driver__name']
    ordering_fields = ['date', 'created_at', 'total_cost']
    ordering = ['-date']
    
    def get_queryset(self):
        if self.request.user.user_type == 'admin':
            return Trip.objects.all()
        elif self.request.user.user_type in ['driver', 'tester']:
            driver = getattr(self.request.user, 'driver_profile', None)
            if driver:
                return Trip.objects.filter(driver=driver)
        return Trip.objects.none()
    
    @action(detail=True, methods=['post'])
    def calculate_cost(self, request, pk=None):
        trip = self.get_object()
        total_cost = trip.calculate_total_cost()
        trip.save()
        return Response({'total_cost': total_cost})
    
    @action(detail=False, methods=['get'])
    def by_status(self, request):
        status_filter = request.query_params.get('status', None)
        if status_filter:
            queryset = self.get_queryset().filter(status=status_filter)
            serializer = self.get_serializer(queryset, many=True)
            return Response(serializer.data)
        else:
            # Return trips grouped by status
            queryset = self.get_queryset()
            trips_by_status = {}
            for trip in queryset:
                trip_status = trip.status or 'unknown'
                if trip_status not in trips_by_status:
                    trips_by_status[trip_status] = []
                trips_by_status[trip_status].append(self.get_serializer(trip).data)
            return Response(trips_by_status)
    
    @action(detail=True, methods=['post'], parser_classes=[MultiPartParser, FormParser])
    def start_trip(self, request, pk=None):
        """Start a trip with mileage and photo upload"""
        trip = self.get_object()
        
        if trip.status != 'pending':
            return Response(
                {'error': 'Trip must be in pending status to start'},
                status=status.HTTP_400_BAD_REQUEST
            )
        
        start_mileage = request.data.get('start_mileage')
        proof_image = request.FILES.get('proof_image')
        current_location_coords = request.data.get('current_location_coords')
        material_loading_photos = request.FILES.getlist('material_loading_photos')
        
        if not start_mileage:
            return Response(
                {'error': 'start_mileage is required'},
                status=status.HTTP_400_BAD_REQUEST
            )
        
        try:
            trip.start_mileage = float(start_mileage)
            if proof_image:
                trip.proof_image = proof_image
            if current_location_coords:
                # Parse coordinates if provided as string
                coords = current_location_coords.split(',') if isinstance(current_location_coords, str) else current_location_coords
                if len(coords) == 2:
                    from django.contrib.gis.geos import Point
                    trip.current_location_coords = Point(float(coords[0]), float(coords[1]))
            
            # Handle material loading photos
            if material_loading_photos:
                photo_urls = []
                from django.core.files.storage import default_storage
                import os
                for i, photo in enumerate(material_loading_photos):
                    file_extension = os.path.splitext(photo.name)[1]
                    filename = f'material_loading/{trip.id}_start_{i}{file_extension}'
                    file_path = default_storage.save(filename, photo)
                    photo_urls.append(default_storage.url(file_path))
                trip.material_loading_photos = photo_urls
            
            trip.status = 'in_progress'
            trip.save()
            
            return Response({
                'message': 'Trip started successfully',
                'trip_id': trip.id,
                'status': trip.status,
                'start_mileage': trip.start_mileage
            })
            
        except Exception as e:
            return Response(
                {'error': f'Failed to start trip: {str(e)}'},
                status=status.HTTP_500_INTERNAL_SERVER_ERROR
            )
    
    @action(detail=True, methods=['post'], parser_classes=[MultiPartParser, FormParser])
    def end_trip(self, request, pk=None):
        """End a trip with final mileage and photo upload"""
        trip = self.get_object()
        
        if trip.status != 'in_progress':
            return Response(
                {'error': 'Trip must be in progress to end'},
                status=status.HTTP_400_BAD_REQUEST
            )
        
        end_mileage = request.data.get('end_mileage')
        proof_end_image = request.FILES.get('proof_end_image')
        
        if not end_mileage:
            return Response(
                {'error': 'end_mileage is required'},
                status=status.HTTP_400_BAD_REQUEST
            )
        
        try:
            trip.end_mileage = float(end_mileage)
            if proof_end_image:
                trip.proof_end_image = proof_end_image
            
            # Calculate total mileage
            if trip.start_mileage:
                trip.total_mileage = trip.end_mileage - trip.start_mileage
            
            trip.status = 'completed'
            trip.save()
            
            return Response({
                'message': 'Trip completed successfully',
                'trip_id': trip.id,
                'status': trip.status,
                'end_mileage': trip.end_mileage,
                'total_mileage': trip.total_mileage
            })
            
        except Exception as e:
            return Response(
                {'error': f'Failed to end trip: {str(e)}'},
                status=status.HTTP_500_INTERNAL_SERVER_ERROR
            )

    @action(detail=True, methods=['post'], parser_classes=[MultiPartParser, FormParser])
    def upload_photos(self, request, pk=None):
        """Upload photos for a trip (start mileage, material loading, end mileage)"""
        trip = self.get_object()
        
        # Handle different types of photos
        photo_type = request.data.get('photo_type')  # 'start_mileage', 'material_loading', 'end_mileage'
        photo_file = request.FILES.get('photo')
        
        if not photo_type or not photo_file:
            return Response(
                {'error': 'Both photo_type and photo file are required'},
                status=status.HTTP_400_BAD_REQUEST
            )
        
        try:
            if photo_type == 'start_mileage':
                trip.proof_image = photo_file
                trip.save()
                return Response({
                    'message': 'Start mileage photo uploaded successfully',
                    'photo_url': trip.proof_image.url if trip.proof_image else None
                })
                
            elif photo_type == 'end_mileage':
                trip.proof_end_image = photo_file
                trip.save()
                return Response({
                    'message': 'End mileage photo uploaded successfully',
                    'photo_url': trip.proof_end_image.url if trip.proof_end_image else None
                })
                
            elif photo_type == 'material_loading':
                # Handle multiple material loading photos
                current_photos = trip.material_loading_photos or []
                
                # Save the file and add URL to the list
                # In a production environment, you'd save to cloud storage
                import os
                from django.conf import settings
                from django.core.files.storage import default_storage
                
                # Create unique filename
                file_extension = os.path.splitext(photo_file.name)[1]
                filename = f'material_loading/{trip.id}_{len(current_photos)}{file_extension}'
                
                # Save file
                file_path = default_storage.save(filename, photo_file)
                photo_url = default_storage.url(file_path)
                
                # Add to photos list
                current_photos.append(photo_url)
                trip.material_loading_photos = current_photos
                trip.save()
                
                return Response({
                    'message': 'Material loading photo uploaded successfully',
                    'photo_url': photo_url,
                    'total_photos': len(current_photos)
                })
                
            else:
                return Response(
                    {'error': 'Invalid photo_type. Must be start_mileage, material_loading, or end_mileage'},
                    status=status.HTTP_400_BAD_REQUEST
                )
                
        except Exception as e:
            return Response(
                {'error': f'Failed to upload photo: {str(e)}'},
                status=status.HTTP_500_INTERNAL_SERVER_ERROR
            )


@extend_schema_view(
    list=extend_schema(tags=["Trips"]),
    retrieve=extend_schema(tags=["Trips"]),
    create=extend_schema(tags=["Trips"]),
    update=extend_schema(tags=["Trips"]),
    partial_update=extend_schema(tags=["Trips"]),
    destroy=extend_schema(tags=["Trips"]),
    by_driver=extend_schema(tags=["Trips"]),
    by_truck=extend_schema(tags=["Trips"])
)
class ExpenseViewSet(viewsets.ModelViewSet):
    queryset = Expense.objects.all()
    serializer_class = ExpenseSerializer
    permission_classes = [IsApproved]
    parser_classes = [JSONParser, MultiPartParser, FormParser]
    
    def get_queryset(self):
        if self.request.user.user_type == 'admin':
            return Expense.objects.all()
        elif self.request.user.user_type in ['driver', 'tester']:
            driver = getattr(self.request.user, 'driver_profile', None)
            if driver:
                return Expense.objects.filter(trip__driver=driver)
        return Expense.objects.none()
    
    def get_permissions(self):
        # Simplified permissions for debugging
        return [IsApproved(), IsAdminOrDriverOrTester()]
    
    @action(detail=False, methods=['get'])
    def by_driver(self, request):
        driver_id = request.query_params.get('driver_id', None)
        if driver_id and self.request.user.user_type == 'admin':
            queryset = Expense.objects.filter(trip__driver__id=driver_id)
            serializer = self.get_serializer(queryset, many=True)
            return Response(serializer.data)
        return Response({'error': 'Driver ID parameter required and admin access needed'}, status=400)
    
    @action(detail=False, methods=['get'])
    def by_truck(self, request):
        truck_id = request.query_params.get('truck_id', None)
        if truck_id and self.request.user.user_type == 'admin':
            queryset = Expense.objects.filter(trip__truck__id=truck_id)
            serializer = self.get_serializer(queryset, many=True)
            return Response(serializer.data)
        return Response({'error': 'Truck ID parameter required and admin access needed'}, status=400)


@extend_schema_view(
    list=extend_schema(tags=["Trips"]),
    retrieve=extend_schema(tags=["Trips"]),
    create=extend_schema(tags=["Trips"]),
    update=extend_schema(tags=["Trips"]),
    partial_update=extend_schema(tags=["Trips"]),
    destroy=extend_schema(tags=["Trips"]),
    extract_details=extend_schema(tags=["Trips"]),
)
class ReceiptViewSet(viewsets.ModelViewSet):
    queryset = Receipt.objects.all()
    serializer_class = ReceiptSerializer
    permission_classes = [IsApproved, IsAdminOrDriverOrTester]
    
    def get_queryset(self):
        if self.request.user.user_type == 'admin':
            return Receipt.objects.all()
        elif self.request.user.user_type in ['driver', 'tester']:
            driver = getattr(self.request.user, 'driver_profile', None)
            if driver:
                return Receipt.objects.filter(expense__trip__driver=driver)
        return Receipt.objects.none()
    
    @action(detail=True, methods=['post'])
    def extract_details(self, request, pk=None):
        receipt = self.get_object()
        # Placeholder for OCR integration
        # This would integrate with an OCR service to extract receipt details
        extracted_data = {
            'vendor': 'Example Vendor',
            'amount': '25.99',
            'date': '2023-01-01',
            'items': ['Item 1', 'Item 2']
        }
        receipt.receipt_details = extracted_data
        receipt.save()
        return Response(extracted_data)


@extend_schema_view(
    list=extend_schema(tags=["Trips"]),
    retrieve=extend_schema(tags=["Trips"]),
    create=extend_schema(tags=["Trips"]),
    update=extend_schema(tags=["Trips"]),
    partial_update=extend_schema(tags=["Trips"]),
    destroy=extend_schema(tags=["Trips"])
)
class VehicleMileageViewSet(viewsets.ModelViewSet):
    queryset = VehicleMileage.objects.all()
    serializer_class = VehicleMileageSerializer
    permission_classes = [IsApproved, IsAdminOrDriverOrTester]
    
    def get_queryset(self):
        if self.request.user.user_type == 'admin':
            return VehicleMileage.objects.all()
        elif self.request.user.user_type in ['driver', 'tester']:
            driver = getattr(self.request.user, 'driver_profile', None)
            if driver:
                return VehicleMileage.objects.filter(driver=driver)
        return VehicleMileage.objects.none()


class TripTypeFilterSet(filters_rest.FilterSet):
    """Filter set for TripType"""
    category = filters_rest.CharFilter(field_name="category")
    is_active = filters_rest.BooleanFilter(field_name="is_active")

    class Meta:
        model = TripType
        fields = ['category', 'is_active']


@extend_schema_view(
    list=extend_schema(tags=["Trip Types"]),
    retrieve=extend_schema(tags=["Trip Types"]),
    create=extend_schema(tags=["Trip Types"]),
    update=extend_schema(tags=["Trip Types"]),
    partial_update=extend_schema(tags=["Trip Types"]),
    destroy=extend_schema(tags=["Trip Types"]),
    active=extend_schema(tags=["Trip Types"]),
    by_category=extend_schema(tags=["Trip Types"]),
)
class TripTypeViewSet(viewsets.ModelViewSet):
    """
    ViewSet for managing Trip Types
    - Admin users have full CRUD access
    - Driver users can only read (list/retrieve)
    """
    queryset = TripType.objects.all()
    serializer_class = TripTypeSerializer
    filter_backends = [DjangoFilterBackend, filters.SearchFilter, filters.OrderingFilter]
    filterset_class = TripTypeFilterSet
    search_fields = ['name', 'description', 'empty_trip_option']
    ordering_fields = ['name', 'category', 'created_at']
    ordering = ['category', 'name']

    def get_permissions(self):
        """
        Set permissions based on action:
        - Admin users: full CRUD access
        - Driver users: read-only access
        """
        if self.action in ['list', 'retrieve', 'active', 'by_category']:
            # All authenticated users can read trip types
            self.permission_classes = [IsApproved]
        else:
            # Only admin users can create/update/delete trip types
            self.permission_classes = [IsApproved]
            # Check if user is admin
            if not (self.request.user.is_authenticated and self.request.user.user_type == 'admin'):
                from rest_framework.exceptions import PermissionDenied
                raise PermissionDenied("Only admin users can perform this action.")

        return [permission() for permission in self.permission_classes]

    def get_queryset(self):
        """
        Filter queryset based on user role:
        - Admin: see all trip types
        - Drivers: see only active trip types
        """
        if self.request.user.user_type == 'admin':
            return TripType.objects.all()
        elif self.request.user.user_type in ['driver', 'tester']:
            return TripType.objects.filter(is_active=True)
        return TripType.objects.none()

    @action(detail=False, methods=['get'])
    def active(self, request):
        """Get all active trip types"""
        queryset = self.get_queryset().filter(is_active=True)
        serializer = self.get_serializer(queryset, many=True)
        return Response(serializer.data)

    @action(detail=False, methods=['get'])
    def by_category(self, request):
        """Get trip types grouped by category"""
        category = request.query_params.get('category', None)
        if category:
            queryset = self.get_queryset().filter(category=category)
            serializer = self.get_serializer(queryset, many=True)
            return Response(serializer.data)
        else:
            # Return trip types grouped by category
            queryset = self.get_queryset()
            trip_types_by_category = {}
            for trip_type in queryset:
                cat = trip_type.category or 'other'
                if cat not in trip_types_by_category:
                    trip_types_by_category[cat] = []
                trip_types_by_category[cat].append(self.get_serializer(trip_type).data)
            return Response(trip_types_by_category)


class TripMaterialFilterSet(filters_rest.FilterSet):
    """Filter set for TripMaterial"""
    material = filters_rest.ModelChoiceFilter(queryset=Material.objects.all())
    added_at_from = filters_rest.DateFilter(field_name="added_at", lookup_expr='gte')
    added_at_to = filters_rest.DateFilter(field_name="added_at", lookup_expr='lte')

    class Meta:
        model = TripMaterial
        fields = ['material', 'added_at_from', 'added_at_to']


@extend_schema_view(
    list=extend_schema(tags=["Trip Materials"]),
    retrieve=extend_schema(tags=["Trip Materials"]),
    create=extend_schema(tags=["Trip Materials"]),
    update=extend_schema(tags=["Trip Materials"]),
    partial_update=extend_schema(tags=["Trip Materials"]),
    destroy=extend_schema(tags=["Trip Materials"]),
)
class TripMaterialViewSet(viewsets.ModelViewSet):
    """
    ViewSet for managing Trip Materials
    - Admin users have full CRUD access
    - Driver users can add materials to their own trips
    """
    queryset = TripMaterial.objects.all()
    serializer_class = TripMaterialSerializer
    filter_backends = [DjangoFilterBackend, filters.OrderingFilter]
    filterset_class = TripMaterialFilterSet
    ordering_fields = ['added_at', 'material__name', 'total_cost']
    ordering = ['-added_at']

    def get_permissions(self):
        """
        Set permissions based on action:
        - Admin users: full CRUD access
        - Driver users: can add materials to their own trips
        """
        if self.action in ['create']:
            # Drivers can add materials to their own trips
            self.permission_classes = [IsApproved]
            # Check if trip belongs to the driver
            if not (self.request.user.is_authenticated and self.request.user.user_type in ['driver', 'tester']):
                from rest_framework.exceptions import PermissionDenied
                raise PermissionDenied("Only authenticated users can add trip materials.")
        else:
            # For other actions, use default permissions
            self.permission_classes = [IsApproved, IsAdminOrDriverOrTester]

        return [permission() for permission in self.permission_classes]

    def get_queryset(self):
        """
        Filter queryset based on user role:
        - Admin: see all trip materials
        - Drivers: see materials for their own trips
        """
        if self.request.user.user_type == 'admin':
            return TripMaterial.objects.all()
        elif self.request.user.user_type in ['driver', 'tester']:
            driver = getattr(self.request.user, 'driver_profile', None)
            if driver:
                return TripMaterial.objects.filter(trip__driver=driver)
        return TripMaterial.objects.none()

    def perform_create(self, serializer):
        """
        Override to check if trip belongs to driver (for non-admin users)
        and set appropriate permissions
        """
        if self.request.user.user_type not in ['admin']:
            # Check if trip belongs to the driver
            trip = serializer.validated_data['trip']
            driver = getattr(self.request.user, 'driver_profile', None)
            if not driver or trip.driver != driver:
                from rest_framework.exceptions import PermissionDenied
                raise PermissionDenied("You can only add materials to your own trips.")

        serializer.save()

    @action(detail=True, methods=['post'])
    def calculate_total(self, request, pk=None):
        """Recalculate total cost for a specific material"""
        trip_material = self.get_object()

        if trip_material.quantity and trip_material.unit_cost:
            trip_material.total_cost = trip_material.quantity * trip_material.unit_cost
            trip_material.save()

            # Also recalculate trip total cost
            trip_material.trip.calculate_total_cost()
            trip_material.trip.save()

        return Response({
            'material_id': trip_material.id,
            'total_cost': str(trip_material.total_cost)
        })