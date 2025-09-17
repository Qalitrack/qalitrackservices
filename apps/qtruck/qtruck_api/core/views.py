from rest_framework import viewsets, status, permissions, serializers
from rest_framework.decorators import action, api_view, permission_classes
from rest_framework.response import Response
from rest_framework.views import APIView
from rest_framework_simplejwt.authentication import JWTAuthentication
from django.utils import timezone
from django.views.decorators.csrf import csrf_exempt
from django.utils.decorators import method_decorator
from django.conf import settings
from django.core.management import call_command
from drf_spectacular.utils import extend_schema, OpenApiParameter, extend_schema_view, inline_serializer
from drf_spectacular.types import OpenApiTypes

from .models import (
    CustomUser, UserProfile, SystemSettings, Driver, Truck, Trip,
    Material, MaterialCost, Expense, Receipt, VehicleMileage, Feedback
)
from .serializers import (
    UserRegistrationSerializer, CustomUserSerializer, UserProfileSerializer,
    SystemSettingsSerializer, DriverSerializer, TruckSerializer, TripSerializer,
    MaterialSerializer, MaterialCostSerializer, ExpenseSerializer,
    ReceiptSerializer, VehicleMileageSerializer, UserApprovalSerializer,
    FeedbackSerializer, FeedbackResponseSerializer
)
from .permissions import (
    IsAdmin, IsDriver, IsTester, IsAdminOrDriver, IsAdminOrOwner, IsApproved,
    IsAdminOrReadOnly, IsDriverOrTester, IsAdminOrDriverOrTester
)
from .authentication import EmailAliasTokenObtainPairView


class UserRegistrationView(APIView):
    permission_classes = [permissions.AllowAny]
    
    @extend_schema(
        tags=['Authentication'],
        request=UserRegistrationSerializer,
        responses={201: CustomUserSerializer}
    )
    def post(self, request):
        serializer = UserRegistrationSerializer(data=request.data)
        if serializer.is_valid():
            user = serializer.save()
            return Response(
                CustomUserSerializer(user).data, 
                status=status.HTTP_201_CREATED
            )
        return Response(serializer.errors, status=status.HTTP_400_BAD_REQUEST)


class UserApprovalView(APIView):
    authentication_classes = [JWTAuthentication]
    permission_classes = [IsAdmin]
    
    @extend_schema(
        tags=['User Management'],
        request=UserApprovalSerializer,
        responses={200: CustomUserSerializer}
    )
    def post(self, request):
        serializer = UserApprovalSerializer(data=request.data)
        if serializer.is_valid():
            user_id = serializer.validated_data['user_id']
            action = serializer.validated_data['action']
            
            try:
                user = CustomUser.objects.get(id=user_id)
            except CustomUser.DoesNotExist:
                return Response({'error': 'User not found'}, status=status.HTTP_404_NOT_FOUND)
            
            if action == 'approve':
                user.is_approved = True
                user.save()
                
                # Create or get user profile
                profile, created = UserProfile.objects.get_or_create(user=user)
                profile.approval_date = timezone.now()
                profile.approved_by = request.user
                profile.save()
                
                message = 'User approved successfully'
            else:  # reject
                user.delete()
                return Response({'message': 'User rejected and deleted'}, status=status.HTTP_200_OK)
            
            return Response({
                'message': message,
                'user': CustomUserSerializer(user).data
            }, status=status.HTTP_200_OK)
        
        return Response(serializer.errors, status=status.HTTP_400_BAD_REQUEST)


class PendingUsersView(APIView):
    permission_classes = [IsAdmin]
    
    @extend_schema(
        tags=['User Management'],
        responses={200: CustomUserSerializer(many=True)}
    )
    def get(self, request):
        pending_users = CustomUser.objects.filter(is_approved=False)
        serializer = CustomUserSerializer(pending_users, many=True)
        return Response(serializer.data)


class UsersByTypeView(APIView):
    permission_classes = [IsAdmin]
    
    @extend_schema(
        tags=['User Management'],
        parameters=[
            OpenApiParameter(
                name='user_type',
                description='Filter by user type',
                required=False,
                type=str,
                enum=['admin', 'driver', 'tester']
            ),
        ],
        responses={200: CustomUserSerializer(many=True)}
    )
    def get(self, request):
        user_type = request.query_params.get('user_type')
        
        if user_type and user_type in ['admin', 'driver', 'tester']:
            users = CustomUser.objects.filter(user_type=user_type)
        else:
            users = CustomUser.objects.all()
        
        serializer = CustomUserSerializer(users, many=True)
        return Response(serializer.data)


class CreateAdminView(APIView):
    permission_classes = [IsAdmin]
    
    @extend_schema(
        tags=['User Management'],
        request=inline_serializer(
            name='CreateAdminRequest',
            fields={
                'email': serializers.EmailField(),
                'password': serializers.CharField(),
                'first_name': serializers.CharField(required=False),
                'last_name': serializers.CharField(required=False),
            }
        ),
        responses={201: CustomUserSerializer}
    )
    def post(self, request):
        data = request.data.copy()
        data['password_confirm'] = data.get('password')  # Auto-confirm for admin creation
        
        # Force admin type
        email = data.get('email', '')
        if '+' in email:
            raise serializers.ValidationError('Admin accounts should not use email aliases.')
        
        # Create admin user directly
        user = CustomUser.objects.create_user(
            username=email,
            email=email,
            password=data.get('password'),
            first_name=data.get('first_name', ''),
            last_name=data.get('last_name', ''),
            user_type='admin',
            base_email=email,
            is_approved=True  # Auto-approve admin-created admins
        )
        
        return Response({
            'message': 'Admin user created successfully',
            'user': CustomUserSerializer(user).data
        }, status=status.HTTP_201_CREATED)


@extend_schema_view(
    list=extend_schema(tags=['System Settings']),
    retrieve=extend_schema(tags=['System Settings']),
    create=extend_schema(tags=['System Settings']),
    update=extend_schema(tags=['System Settings']),
    partial_update=extend_schema(tags=['System Settings']),
    destroy=extend_schema(tags=['System Settings']),
)
class SystemSettingsViewSet(viewsets.ModelViewSet):
    queryset = SystemSettings.objects.all()
    serializer_class = SystemSettingsSerializer
    permission_classes = [IsAdmin]
    
    def get_object(self):
        return SystemSettings.get_settings()
    
    def list(self, request, *args, **kwargs):
        settings = self.get_object()
        serializer = self.get_serializer(settings)
        return Response(serializer.data)


@extend_schema_view(
    list=extend_schema(tags=['User Profiles']),
    retrieve=extend_schema(tags=['User Profiles']),
    create=extend_schema(tags=['User Profiles']),
    update=extend_schema(tags=['User Profiles']),
    partial_update=extend_schema(tags=['User Profiles']),
    destroy=extend_schema(tags=['User Profiles']),
)
class DriverViewSet(viewsets.ModelViewSet):
    queryset = Driver.objects.all()
    serializer_class = DriverSerializer
    permission_classes = [IsApproved]  # Admin sees all, drivers/testers see own
    
    def get_permissions(self):
        if self.action in ['update', 'partial_update', 'destroy']:
            # Only admins, drivers, and testers can modify existing driver profiles
            return [IsApproved(), IsAdminOrDriverOrTester()]
        elif self.action == 'create':
            # Admins, drivers, and testers can create driver profiles (testers for testing purposes)
            return [IsApproved()]
        return [IsApproved()]
    
    def get_queryset(self):
        if self.request.user.user_type == 'admin':
            return Driver.objects.all()
        elif self.request.user.user_type in ['driver', 'tester']:
            return Driver.objects.filter(user=self.request.user)
        return Driver.objects.none()
    
    def perform_create(self, serializer):
        serializer.save(user=self.request.user)


@extend_schema_view(
    list=extend_schema(tags=['Fleet Management']),
    retrieve=extend_schema(tags=['Fleet Management']),
    create=extend_schema(tags=['Fleet Management']),
    update=extend_schema(tags=['Fleet Management']),
    partial_update=extend_schema(tags=['Fleet Management']),
    destroy=extend_schema(tags=['Fleet Management']),
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
    list=extend_schema(tags=['Trip Management']),
    retrieve=extend_schema(tags=['Trip Management']),
    create=extend_schema(tags=['Trip Management']),
    update=extend_schema(tags=['Trip Management']),
    partial_update=extend_schema(tags=['Trip Management']),
    destroy=extend_schema(tags=['Trip Management']),
    calculate_cost=extend_schema(tags=['Trip Management']),
    by_status=extend_schema(tags=['Trip Management']),
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
        return Response({'error': 'Status parameter is required'}, status=400)


@extend_schema_view(
    list=extend_schema(tags=['Fleet Management']),
    retrieve=extend_schema(tags=['Fleet Management']),
    create=extend_schema(tags=['Fleet Management']),
    update=extend_schema(tags=['Fleet Management']),
    partial_update=extend_schema(tags=['Fleet Management']),
    destroy=extend_schema(tags=['Fleet Management']),
)
class MaterialViewSet(viewsets.ModelViewSet):
    queryset = Material.objects.all()
    serializer_class = MaterialSerializer
    permission_classes = [IsApproved, IsAdminOrReadOnly]  # Admin can modify, others read-only
    
    def get_queryset(self):
        if self.request.user.user_type == 'admin':
            return Material.objects.all()
        elif self.request.user.user_type in ['driver', 'tester']:
            driver = getattr(self.request.user, 'driver_profile', None)
            if driver:
                return Material.objects.filter(trip__driver=driver)
        return Material.objects.none()


@extend_schema_view(
    list=extend_schema(tags=['Trip Expenses']),
    retrieve=extend_schema(tags=['Trip Expenses']),
    create=extend_schema(tags=['Trip Expenses']),
    update=extend_schema(tags=['Trip Expenses']),
    partial_update=extend_schema(tags=['Trip Expenses']),
    destroy=extend_schema(tags=['Trip Expenses']),
)
class MaterialCostViewSet(viewsets.ModelViewSet):
    queryset = MaterialCost.objects.all()
    serializer_class = MaterialCostSerializer
    permission_classes = [IsApproved, IsAdminOrDriverOrTester]
    
    def get_queryset(self):
        if self.request.user.user_type == 'admin':
            return MaterialCost.objects.all()
        elif self.request.user.user_type in ['driver', 'tester']:
            driver = getattr(self.request.user, 'driver_profile', None)
            if driver:
                return MaterialCost.objects.filter(material__trip__driver=driver)
        return MaterialCost.objects.none()


@extend_schema_view(
    list=extend_schema(tags=['Trip Expenses']),
    retrieve=extend_schema(tags=['Trip Expenses']),
    create=extend_schema(tags=['Trip Expenses']),
    update=extend_schema(tags=['Trip Expenses']),
    partial_update=extend_schema(tags=['Trip Expenses']),
    destroy=extend_schema(tags=['Trip Expenses']),
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
    list=extend_schema(tags=['Trip Expenses']),
    retrieve=extend_schema(tags=['Trip Expenses']),
    create=extend_schema(tags=['Trip Expenses']),
    update=extend_schema(tags=['Trip Expenses']),
    partial_update=extend_schema(tags=['Trip Expenses']),
    destroy=extend_schema(tags=['Trip Expenses']),
    extract_details=extend_schema(tags=['Trip Expenses']),
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
                return Receipt.objects.filter(expense__driver=driver)
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
    list=extend_schema(tags=['Vehicle Tracking']),
    retrieve=extend_schema(tags=['Vehicle Tracking']),
    create=extend_schema(tags=['Vehicle Tracking']),
    update=extend_schema(tags=['Vehicle Tracking']),
    partial_update=extend_schema(tags=['Vehicle Tracking']),
    destroy=extend_schema(tags=['Vehicle Tracking']),
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


@extend_schema_view(
    list=extend_schema(tags=['User Profiles']),
    retrieve=extend_schema(tags=['User Profiles']),
)
class UserProfileViewSet(viewsets.ReadOnlyModelViewSet):
    queryset = UserProfile.objects.all()
    serializer_class = UserProfileSerializer
    permission_classes = [IsApproved, IsAdminOrOwner]
    
    def get_queryset(self):
        if self.request.user.user_type == 'admin':
            return UserProfile.objects.all()
        else:
            return UserProfile.objects.filter(user=self.request.user)


@extend_schema_view(
    list=extend_schema(tags=['Feedback System']),
    retrieve=extend_schema(tags=['Feedback System']),
    create=extend_schema(tags=['Feedback System']),
    update=extend_schema(tags=['Feedback System']),
    partial_update=extend_schema(tags=['Feedback System']),
    destroy=extend_schema(tags=['Feedback System']),
    respond=extend_schema(tags=['Feedback System']),
    pending=extend_schema(tags=['Feedback System']),
)
class FeedbackViewSet(viewsets.ModelViewSet):
    queryset = Feedback.objects.all()
    serializer_class = FeedbackSerializer
    permission_classes = [IsApproved, IsDriverOrTester]
    
    def get_queryset(self):
        if self.request.user.user_type == 'admin':
            return Feedback.objects.all()
        else:
            return Feedback.objects.filter(user=self.request.user)
    
    def perform_create(self, serializer):
        serializer.save(user=self.request.user)
    
    @action(detail=True, methods=['post'], permission_classes=[IsAdmin])
    def respond(self, request, pk=None):
        feedback = self.get_object()
        serializer = FeedbackResponseSerializer(data=request.data)
        
        if serializer.is_valid():
            feedback.status = serializer.validated_data['status']
            feedback.admin_response = serializer.validated_data.get('admin_response', '')
            feedback.responded_by = request.user
            feedback.response_date = timezone.now()
            feedback.save()
            
            return Response({'message': 'Feedback response saved'})
        
        return Response(serializer.errors, status=status.HTTP_400_BAD_REQUEST)
    
    @action(detail=False, methods=['get'], permission_classes=[IsAdmin])
    def pending(self, request):
        """Get all pending feedback for admin"""
        pending_feedback = Feedback.objects.filter(status='pending')
        serializer = self.get_serializer(pending_feedback, many=True)
        return Response(serializer.data)


@api_view(['GET'])
@permission_classes([permissions.IsAuthenticated])
def user_info(request):
    """Get current user information"""
    serializer = CustomUserSerializer(request.user)
    return Response(serializer.data)


@extend_schema(
    tags=['Database Operations'],
    summary="Flush Database",
    description="Flush all data from the database and run migrations. Only available when DATABASE_FLUSH_ENABLED=True in environment.",
    responses={
        200: inline_serializer(
            name='DatabaseFlushResponse',
            fields={
                'message': serializers.CharField(),
                'details': serializers.CharField(),
            }
        ),
        403: inline_serializer(
            name='DatabaseFlushDisabledResponse',
            fields={
                'error': serializers.CharField(),
            }
        ),
    }
)
@api_view(['POST'])
@permission_classes([IsAdmin])
def flush_database(request):
    """
    Flush all data from the database and run migrations.
    Only available when DATABASE_FLUSH_ENABLED=True in environment.
    This is typically used for testing purposes.
    """
    if not getattr(settings, 'DATABASE_FLUSH_ENABLED', False):
        return Response(
            {'error': 'Database flush is not enabled. Set DATABASE_FLUSH_ENABLED=True in environment to enable this endpoint.'},
            status=status.HTTP_403_FORBIDDEN
        )
    
    try:
        # Flush the database
        call_command('flush', '--noinput')
        
        # Run migrations to ensure database is properly set up
        call_command('migrate', '--noinput')
        
        return Response({
            'message': 'Database flushed successfully',
            'details': 'All data has been removed and migrations have been applied.'
        }, status=status.HTTP_200_OK)
        
    except Exception as e:
        return Response({
            'error': f'Failed to flush database: {str(e)}'
        }, status=status.HTTP_500_INTERNAL_SERVER_ERROR)