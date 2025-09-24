from rest_framework import viewsets, status, permissions
from rest_framework.decorators import action
from rest_framework.response import Response
from rest_framework_simplejwt.authentication import JWTAuthentication
from drf_spectacular.utils import extend_schema, extend_schema_view

from .models import Trip, Expense, Receipt, VehicleMileage
from .serializers import TripSerializer, ExpenseSerializer, ReceiptSerializer, VehicleMileageSerializer
from settings.permissions import (
    IsApproved, IsAdminOrDriverOrTester
)


@extend_schema_view(
    list=extend_schema(),
    retrieve=extend_schema(),
    create=extend_schema(),
    update=extend_schema(),
    partial_update=extend_schema(),
    destroy=extend_schema(),
    calculate_cost=extend_schema(),
    by_status=extend_schema(),
)
class TripViewSet(viewsets.ModelViewSet):
    queryset = Trip.objects.all()
    serializer_class = TripSerializer
    permission_classes = [IsApproved, IsAdminOrDriverOrTester]
    
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


@extend_schema_view(
    list=extend_schema(tags=["Trips"]),
    retrieve=extend_schema(tags=["Trips"]),
    create=extend_schema(tags=["Trips"]),
    update=extend_schema(tags=["Trips"]),
    partial_update=extend_schema(tags=["Trips"]),
    destroy=extend_schema(tags=["Trips"])
)
class ExpenseViewSet(viewsets.ModelViewSet):
    queryset = Expense.objects.all()
    serializer_class = ExpenseSerializer
    permission_classes = [IsApproved]
    
    def get_queryset(self):
        if self.request.user.user_type == 'admin':
            return Expense.objects.all()
        elif self.request.user.user_type in ['driver', 'tester']:
            driver = getattr(self.request.user, 'driver_profile', None)
            if driver:
                return Expense.objects.filter(trip__driver=driver)
        return Expense.objects.none()
    
    def get_permissions(self):
        if self.action in ['create', 'update', 'partial_update', 'destroy']:
            # Only admins, drivers, and testers can modify expenses
            return [IsApproved(), IsAdminOrDriverOrTester()]
        return [IsApproved()]
    
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
    list=extend_schema(),
    retrieve=extend_schema(),
    create=extend_schema(),
    update=extend_schema(),
    partial_update=extend_schema(),
    destroy=extend_schema(),
    extract_details=extend_schema(),
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