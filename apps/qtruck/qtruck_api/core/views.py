from rest_framework import viewsets, status, permissions, serializers
from rest_framework.decorators import action, api_view, permission_classes
from rest_framework.response import Response
from rest_framework.views import APIView
from rest_framework_simplejwt.authentication import JWTAuthentication
from django.utils import timezone
from datetime import timedelta
from django.views.decorators.csrf import csrf_exempt
from django.utils.decorators import method_decorator
from django.conf import settings
from django.core.management import call_command
from drf_spectacular.utils import extend_schema, OpenApiParameter, extend_schema_view, inline_serializer
from drf_spectacular.types import OpenApiTypes

from .models import (
    CustomUser, UserProfile, SystemSettings, LicenseClass, Driver, Truck, Trip,
    Material, MaterialCost, Expense, Receipt, VehicleMileage, Feedback,
    DriverProfile, DriverActivity, DriverProfileChange, log_driver_activity
)
from .driver_models import DriverProfileStatus
from django.db import models
from .serializers import (
    UserRegistrationSerializer, CustomUserSerializer, UserProfileSerializer,
    SystemSettingsSerializer, DriverSerializer, TruckSerializer, TripSerializer,
    MaterialSerializer, MaterialCostSerializer, ExpenseSerializer,
    ReceiptSerializer, VehicleMileageSerializer, UserApprovalSerializer,
    FeedbackSerializer, FeedbackResponseSerializer,
    DriverProfileSerializer, DriverProfileCreateSerializer, DriverProfileUpdateSerializer, DriverProfileApprovalSerializer,
    DriverActivitySerializer, DriverEnhancedSerializer, DriverProfileChangeSerializer, LicenseClassSerializer
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
                user.is_approved = False
                user.is_active = False  # Mark as inactive so they show in rejected tab
                user.save()
                message = 'User rejected successfully'
            
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
        
        # Force admin type and add +admin alias if not present
        email = data.get('email', '')
        if '+' not in email:
            # Add +admin alias to comply with model requirements
            email_parts = email.split('@')
            if len(email_parts) == 2:
                email = f"{email_parts[0]}+admin@{email_parts[1]}"
            else:
                raise serializers.ValidationError({'email': 'Invalid email format.'})
        
        # Create admin user directly
        user = CustomUser.objects.create_user(
            username=email,
            email=email,
            password=data.get('password'),
            first_name=data.get('first_name', ''),
            last_name=data.get('last_name', ''),
            user_type='admin',
            is_approved=True  # Auto-approve admin-created admins
        )
        
        return Response({
            'message': 'Admin user created successfully',
            'user': CustomUserSerializer(user).data
        }, status=status.HTTP_201_CREATED)


class UserDeactivateView(APIView):
    """Deactivate a user account"""
    permission_classes = [IsAdmin]
    
    @extend_schema(
        tags=['User Management'],
        request=inline_serializer(
            name='DeactivateUserRequest',
            fields={
                'user_id': serializers.CharField(),
            }
        ),
        responses={200: inline_serializer(
            name='DeactivateUserResponse',
            fields={
                'message': serializers.CharField(),
                'user': CustomUserSerializer()
            }
        )}
    )
    def post(self, request):
        user_id = request.data.get('user_id')
        
        if not user_id:
            return Response({'error': 'user_id is required'}, status=status.HTTP_400_BAD_REQUEST)
        
        try:
            user = CustomUser.objects.get(id=user_id)
            user.is_active = False
            user.save()
            
            return Response({
                'message': 'User deactivated successfully',
                'user': CustomUserSerializer(user).data
            })
        except CustomUser.DoesNotExist:
            return Response({'error': 'User not found'}, status=status.HTTP_404_NOT_FOUND)


class UserReactivateView(APIView):
    """Reactivate a user account"""
    permission_classes = [IsAdmin]
    
    @extend_schema(
        tags=['User Management'],
        request=inline_serializer(
            name='ReactivateUserRequest',
            fields={
                'user_id': serializers.CharField(),
            }
        ),
        responses={200: inline_serializer(
            name='ReactivateUserResponse',
            fields={
                'message': serializers.CharField(),
                'user': CustomUserSerializer()
            }
        )}
    )
    def post(self, request):
        user_id = request.data.get('user_id')
        
        if not user_id:
            return Response({'error': 'user_id is required'}, status=status.HTTP_400_BAD_REQUEST)
        
        try:
            user = CustomUser.objects.get(id=user_id)
            user.is_active = True
            user.save()
            
            return Response({
                'message': 'User reactivated successfully',
                'user': CustomUserSerializer(user).data
            })
        except CustomUser.DoesNotExist:
            return Response({'error': 'User not found'}, status=status.HTTP_404_NOT_FOUND)


class UserDeleteView(APIView):
    """Permanently delete a user account (only for deactivated/rejected users)"""
    permission_classes = [IsAdmin]
    
    @extend_schema(
        tags=['User Management'],
        request=inline_serializer(
            name='DeleteUserRequest',
            fields={
                'user_id': serializers.CharField(),
            }
        ),
        responses={200: inline_serializer(
            name='DeleteUserResponse',
            fields={
                'message': serializers.CharField(),
            }
        )}
    )
    def post(self, request):
        user_id = request.data.get('user_id')
        
        if not user_id:
            return Response({'error': 'user_id is required'}, status=status.HTTP_400_BAD_REQUEST)
        
        try:
            user = CustomUser.objects.get(id=user_id)
            
            # Only allow deletion of deactivated users or rejected users
            if user.is_active and user.is_approved:
                return Response(
                    {'error': 'Cannot delete active and approved users. Deactivate first.'}, 
                    status=status.HTTP_400_BAD_REQUEST
                )
            
            # Prevent deleting the last admin
            if user.user_type == 'admin':
                active_admin_count = CustomUser.objects.filter(
                    user_type='admin', 
                    is_active=True, 
                    is_approved=True
                ).exclude(id=user_id).count()
                
                if active_admin_count == 0:
                    return Response(
                        {'error': 'Cannot delete the last admin user'}, 
                        status=status.HTTP_400_BAD_REQUEST
                    )
            
            user_email = user.email
            user.delete()
            
            return Response({
                'message': f'User {user_email} deleted successfully'
            })
        except CustomUser.DoesNotExist:
            return Response({'error': 'User not found'}, status=status.HTTP_404_NOT_FOUND)


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
        else:
            # Return trips grouped by status
            queryset = self.get_queryset()
            trips_by_status = {}
            for trip in queryset:
                status = trip.status or 'unknown'
                if status not in trips_by_status:
                    trips_by_status[status] = []
                trips_by_status[status].append(self.get_serializer(trip).data)
            return Response(trips_by_status)


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


# Driver Profile Management Views
@extend_schema_view(
    list=extend_schema(tags=['Driver Profiles']),
    retrieve=extend_schema(tags=['Driver Profiles']),
    create=extend_schema(tags=['Driver Profiles']),
    update=extend_schema(tags=['Driver Profiles']),
    partial_update=extend_schema(tags=['Driver Profiles']),
    destroy=extend_schema(tags=['Driver Profiles']),
    approve=extend_schema(tags=['Driver Profiles']),
    pending_profiles=extend_schema(tags=['Driver Profiles']),
    history=extend_schema(tags=['Driver Profiles']),
    activity=extend_schema(tags=['Driver Profiles']),
    heatmap=extend_schema(tags=['Driver Profiles']),
)
class DriverProfileViewSet(viewsets.ModelViewSet):
    queryset = DriverProfile.objects.all()
    permission_classes = [IsApproved, IsAdminOrDriver]
    
    def get_serializer_class(self):
        if self.action == 'create':
            return DriverProfileCreateSerializer
        elif self.action in ['update', 'partial_update']:
            return DriverProfileUpdateSerializer
        elif self.action == 'approve':
            return DriverProfileApprovalSerializer
        return DriverProfileSerializer
    
    def get_queryset(self):
        if self.request.user.user_type == 'admin':
            return DriverProfile.objects.all().select_related('driver', 'reviewed_by')
        elif self.request.user.user_type == 'driver':
            driver = getattr(self.request.user, 'driver_profile', None)
            if driver:
                return DriverProfile.objects.filter(driver=driver)
        return DriverProfile.objects.none()
    
    def perform_create(self, serializer):
        # Get or create driver profile for the user
        driver, created = Driver.objects.get_or_create(
            user=self.request.user,
            defaults={'name': self.request.user.get_full_name() or self.request.user.username}
        )
        
        profile = serializer.save(driver=driver)
        
        # Log activity
        log_driver_activity(
            driver=driver,
            activity_type='profile_update',
            activity_data={'action': 'profile_created', 'version': profile.version_number},
            request=self.request
        )
    
    @action(detail=True, methods=['post'], permission_classes=[IsAdmin])
    def approve(self, request, pk=None):
        """Approve, reject, or request changes for a driver profile"""
        profile = self.get_object()
        serializer = DriverProfileApprovalSerializer(data=request.data)
        
        if serializer.is_valid():
            action = serializer.validated_data['action']
            notes = serializer.validated_data.get('notes', '')
            
            if action == 'approve':
                profile.approve(request.user, notes)
                message = 'Profile approved successfully'
            elif action == 'reject':
                profile.reject(request.user, notes)
                message = 'Profile rejected'
            elif action == 'request_changes':
                profile.request_changes(request.user, notes)
                message = 'Changes requested'
            
            # Log activity
            log_driver_activity(
                driver=profile.driver,
                activity_type='profile_update',
                activity_data={'action': action, 'admin': request.user.username},
                request=request
            )
            
            return Response({'message': message})
        
        return Response(serializer.errors, status=status.HTTP_400_BAD_REQUEST)
    
    @action(detail=False, methods=['get'], permission_classes=[IsAdmin])
    def pending_profiles(self, request):
        """Get all pending driver profiles for admin approval"""
        pending = DriverProfile.objects.filter(status='pending').select_related('driver', 'driver__user')
        serializer = self.get_serializer(pending, many=True)
        return Response(serializer.data)
    
    @action(detail=True, methods=['get'])
    def history(self, request, pk=None):
        """Get profile change history"""
        profile = self.get_object()
        changes = DriverProfileChange.objects.filter(new_profile=profile)
        serializer = DriverProfileChangeSerializer(changes, many=True)
        return Response(serializer.data)
    
    @action(detail=False, methods=['get', 'patch'], permission_classes=[IsApproved, IsDriver])
    def me(self, request):
        """Get or update current driver's profile"""
        try:
            driver = Driver.objects.get(user=request.user)
        except Driver.DoesNotExist:
            # Auto-create Driver object if user is a driver but no Driver object exists
            if request.user.user_type == 'driver':
                driver = Driver.objects.create(
                    user=request.user,
                    name=f"{request.user.first_name} {request.user.last_name}".strip() or request.user.username,
                    phone='',
                    license_number=''
                )
            else:
                return Response({'detail': 'Driver not found'}, status=status.HTTP_404_NOT_FOUND)
        
        if request.method == 'GET':
            # Get the current approved profile (is_current=True)
            approved_profile = DriverProfile.objects.filter(driver=driver, is_current=True).first()
            
            # Check if there's a pending version
            pending_profile = DriverProfile.objects.filter(
                driver=driver, 
                status__in=[DriverProfileStatus.PENDING, DriverProfileStatus.DRAFT]
            ).order_by('-created_at').first()
            
            if approved_profile:
                serializer = DriverProfileSerializer(approved_profile)
                response_data = serializer.data
                
                # Add pending version info
                if pending_profile:
                    response_data['has_pending_version'] = True
                    response_data['pending_version_id'] = str(pending_profile.id)
                    response_data['pending_status'] = pending_profile.status
                else:
                    response_data['has_pending_version'] = False
                    response_data['pending_version_id'] = None
                    response_data['pending_status'] = None
                
                return Response(response_data)
            elif pending_profile:
                # No approved profile yet, return the pending one with metadata
                serializer = DriverProfileSerializer(pending_profile)
                response_data = serializer.data
                response_data['has_pending_version'] = True
                response_data['pending_version_id'] = str(pending_profile.id)
                response_data['pending_status'] = pending_profile.status
                response_data['is_first_profile'] = True
                return Response(response_data)
            else:
                # No profile exists - create an empty draft profile for the driver
                profile = DriverProfile.objects.create(
                    driver=driver,
                    full_name=driver.user.first_name + ' ' + driver.user.last_name if driver.user.first_name else '',
                    phone_number='',
                    id_number='',
                    license_number='',
                    license_expiry_date=timezone.now().date() + timedelta(days=365),
                    status=DriverProfileStatus.DRAFT,
                    is_current=False
                )
                
                # Log activity
                log_driver_activity(
                    driver=driver,
                    activity_type='profile_update',
                    activity_data={'action': 'empty_profile_created', 'version': 1},
                    request=request
                )
                
                serializer = DriverProfileSerializer(profile)
                response_data = serializer.data
                response_data['has_pending_version'] = True
                response_data['pending_version_id'] = str(profile.id)
                response_data['pending_status'] = profile.status
                response_data['is_first_profile'] = True
                response_data['needs_completion'] = True
                return Response(response_data, status=status.HTTP_201_CREATED)
        
        elif request.method == 'PATCH':
            # Check for existing pending profile - enforce only ONE pending version
            import logging
            from django.db import IntegrityError
            logger = logging.getLogger(__name__)
            logger.error(f"PATCH request for driver: {driver.id}")
            
            pending_profiles = DriverProfile.objects.filter(
                driver=driver, 
                status__in=[DriverProfileStatus.PENDING, DriverProfileStatus.DRAFT]
            ).order_by('-created_at')
            
            logger.error(f"Found {pending_profiles.count()} pending profiles")
            for profile in pending_profiles:
                logger.error(f"Pending profile: version={profile.version_number}, status={profile.status}, id={profile.id}")
            
            if pending_profiles.count() > 1:
                return Response(
                    {'error': 'Multiple pending versions detected. This should not happen. Please contact support.'}, 
                    status=status.HTTP_400_BAD_REQUEST
                )
            
            pending_profile = pending_profiles.first()
            
            if pending_profile:
                # Update existing pending profile
                serializer = DriverProfileUpdateSerializer(pending_profile, data=request.data, context={'request': request}, partial=True)
                if serializer.is_valid():
                    serializer.save()
                    
                    # Log activity
                    log_driver_activity(
                        driver=driver,
                        activity_type='profile_update',
                        activity_data={'action': 'pending_profile_updated', 'version': pending_profile.version_number},
                        request=request
                    )
                    
                    return Response(serializer.data)
                return Response(serializer.errors, status=status.HTTP_400_BAD_REQUEST)
            else:
                # Create new version based on current approved profile
                approved_profile = DriverProfile.objects.filter(driver=driver, is_current=True).first()
                
                if approved_profile:
                    # Create new profile with data from approved profile
                    new_profile_data = {}
                    excluded_fields = [
                        'id', 'driver', 'version_number', 'created_at', 'updated_at', 'status', 'is_current', 
                        'submitted_at', 'reviewed_at', 'reviewed_by', 'approval_notes', 'rejection_reason',
                        # Exclude file fields - they need special handling
                        'profile_photo', 'license_front_image', 'license_back_image', 'id_front_image', 'id_back_image'
                    ]
                    
                    for field in DriverProfile._meta.fields:
                        if field.name not in excluded_fields:
                            new_profile_data[field.name] = getattr(approved_profile, field.name)
                    
                    # Update with new data from request (excluding read-only fields)
                    new_profile_data.update(request.data)
                    
                    logger.error(f"About to create serializer with data: {new_profile_data}")
                    serializer = DriverProfileUpdateSerializer(data=new_profile_data, context={'request': request})
                    logger.error(f"Serializer created, checking validity...")
                    if serializer.is_valid():
                        logger.error(f"Serializer IS valid")
                        logger.error(f"Serializer is valid, about to save with driver: {driver.id}")
                        try:
                            new_profile = serializer.save(driver=driver)
                            logger.error(f"Successfully created profile: {new_profile.id}, version: {new_profile.version_number}")
                        except Exception as e:
                            logger.error(f"Error during save: {e}")
                            return Response({'error': 'Failed to create profile version'}, status=status.HTTP_500_INTERNAL_SERVER_ERROR)
                        
                        # Copy license classes from approved profile
                        new_profile.license_classes.set(approved_profile.license_classes.all())
                        
                        # Copy file field URLs from approved profile (if they exist and no new files provided)
                        file_fields = ['profile_photo', 'license_front_image', 'license_back_image', 'id_front_image', 'id_back_image']
                        for field_name in file_fields:
                            # Only copy if new profile doesn't have this field set and approved profile has it
                            approved_field = getattr(approved_profile, field_name)
                            new_field = getattr(new_profile, field_name)
                            if approved_field and not new_field:
                                try:
                                    # Copy the file URL/path (not the actual file object)
                                    setattr(new_profile, field_name, approved_field.name)
                                except (ValueError, AttributeError):
                                    # Skip if there's any issue with the file field
                                    pass
                        
                        new_profile.save()
                        
                        # Log activity
                        log_driver_activity(
                            driver=driver,
                            activity_type='profile_update',
                            activity_data={'action': 'new_version_created', 'version': new_profile.version_number},
                            request=request
                        )
                        
                        return Response(serializer.data, status=status.HTTP_201_CREATED)
                    else:
                        logger.error(f"Serializer is NOT valid, errors: {serializer.errors}")
                    return Response(serializer.errors, status=status.HTTP_400_BAD_REQUEST)
                    else:
                        # No approved profile, create first profile
                        serializer = DriverProfileCreateSerializer(data=request.data, context={'request': request})
                        if serializer.is_valid():
                            profile = serializer.save(driver=driver, version_number=1, status=DriverProfileStatus.DRAFT)
                            
                            # Log activity
                            log_driver_activity(
                                driver=driver,
                                activity_type='profile_update',
                                activity_data={'action': 'first_profile_created', 'version': 1},
                                request=request
                            )
                            
                            return Response(DriverProfileSerializer(profile).data, status=status.HTTP_201_CREATED)
                        return Response(serializer.errors, status=status.HTTP_400_BAD_REQUEST)
                    
        except Driver.DoesNotExist:
            return Response({'detail': 'Driver not found'}, status=status.HTTP_404_NOT_FOUND)
    
    @action(detail=False, methods=['get'], permission_classes=[IsApproved, IsDriver])
    def pending(self, request):
        """Get pending driver profile version if exists"""
        try:
            driver = Driver.objects.get(user=request.user)
            
            # Get pending version
            pending_profile = DriverProfile.objects.filter(
                driver=driver, 
                status__in=[DriverProfileStatus.PENDING, DriverProfileStatus.DRAFT]
            ).order_by('-created_at').first()
            
            if pending_profile:
                serializer = DriverProfileSerializer(pending_profile)
                response_data = serializer.data
                response_data['is_pending_version'] = True
                return Response(response_data)
            else:
                return Response({'detail': 'No pending version found'}, status=status.HTTP_404_NOT_FOUND)
                    
        except Driver.DoesNotExist:
            # Auto-create Driver object if user is a driver but no Driver object exists
            if request.user.user_type == 'driver':
                driver = Driver.objects.create(
                    user=request.user,
                    name=f"{request.user.first_name} {request.user.last_name}".strip() or request.user.username,
                    phone='',
                    license_number=''
                )
                # After creating driver, there will be no pending profile
                return Response({'detail': 'No pending version found'}, status=status.HTTP_404_NOT_FOUND)
            else:
                return Response({'detail': 'Driver not found'}, status=status.HTTP_404_NOT_FOUND)
    
    @action(detail=True, methods=['post'], permission_classes=[IsApproved, IsDriver])
    def submit(self, request, pk=None):
        """Submit a draft profile for approval"""
        profile = self.get_object()
        
        # Only allow submission of draft profiles by the owner
        if profile.driver.user != request.user:
            return Response({'detail': 'Permission denied'}, status=status.HTTP_403_FORBIDDEN)
        
        if profile.status != 'draft':
            return Response({'detail': 'Only draft profiles can be submitted'}, status=status.HTTP_400_BAD_REQUEST)
        
        profile.submit_for_approval()
        
        # Log activity
        log_driver_activity(
            driver=profile.driver,
            activity_type='profile_update',
            activity_data={'action': 'profile_submitted', 'version': profile.version_number},
            request=request
        )
        
        return Response({'detail': 'Profile submitted for approval'}, status=status.HTTP_200_OK)
    
    @action(detail=False, methods=['get'], permission_classes=[IsApproved, IsDriver])
    def my_changes(self, request):
        """Get current driver's profile changes history"""
        try:
            driver = Driver.objects.get(user=request.user)
            changes = DriverProfileChange.objects.filter(new_profile__driver=driver).order_by('-created_at')
            serializer = DriverProfileChangeSerializer(changes, many=True)
            return Response(serializer.data)
        except Driver.DoesNotExist:
            return Response([], status=status.HTTP_200_OK)


@extend_schema_view(
    list=extend_schema(tags=['Driver Management']),
    retrieve=extend_schema(tags=['Driver Management']),
    activity=extend_schema(tags=['Driver Management']),
    heatmap=extend_schema(tags=['Driver Management']),
    expiring_licenses=extend_schema(tags=['Driver Management']),
)
class DriverEnhancedViewSet(viewsets.ReadOnlyModelViewSet):
    """Enhanced driver viewset with profile and activity data"""
    queryset = Driver.objects.all().select_related('user').prefetch_related('profile_versions', 'activities')
    serializer_class = DriverEnhancedSerializer
    permission_classes = [IsApproved, IsAdmin]
    
    def get_queryset(self):
        return Driver.objects.all().select_related('user').prefetch_related('profile_versions', 'activities')
    
    @action(detail=True, methods=['get'])
    def activity(self, request, pk=None):
        """Get driver activity data"""
        driver = self.get_object()
        days = int(request.query_params.get('days', 30))
        
        activities = DriverActivity.objects.filter(driver=driver).order_by('-created_at')[:100]
        stats = driver.get_activity_stats(days)
        
        return Response({
            'recent_activities': DriverActivitySerializer(activities, many=True).data,
            'stats': stats
        })
    
    @action(detail=True, methods=['get'])
    def heatmap(self, request, pk=None):
        """Get driver activity heatmap data"""
        driver = self.get_object()
        year = request.query_params.get('year')
        if year:
            year = int(year)
        
        heatmap_data = driver.get_activity_heatmap(year)
        return Response(heatmap_data)
    
    @action(detail=False, methods=['get'])
    def expiring_licenses(self, request):
        """Get drivers with expiring licenses"""
        settings = SystemSettings.get_settings()
        warning_days = settings.license_expiry_warning_days
        
        from django.utils import timezone
        from datetime import timedelta
        
        warning_date = timezone.now().date() + timedelta(days=warning_days)
        
        expiring_profiles = DriverProfile.objects.filter(
            is_current=True,
            license_expiry_date__lte=warning_date
        ).select_related('driver')
        
        drivers_data = []
        for profile in expiring_profiles:
            driver_data = DriverEnhancedSerializer(profile.driver).data
            driver_data['license_expiry_info'] = {
                'expiry_date': profile.license_expiry_date,
                'days_until_expiry': profile.days_until_license_expiry(),
                'is_expired': profile.is_license_expired
            }
            drivers_data.append(driver_data)
        
        return Response(drivers_data)


@extend_schema_view(
    list=extend_schema(tags=['Driver Activity']),
    retrieve=extend_schema(tags=['Driver Activity']),
)
class DriverActivityViewSet(viewsets.ReadOnlyModelViewSet):
    queryset = DriverActivity.objects.all()
    serializer_class = DriverActivitySerializer
    permission_classes = [IsApproved, IsAdminOrDriver]
    
    def get_queryset(self):
        if self.request.user.user_type == 'admin':
            return DriverActivity.objects.all().select_related('driver')
        elif self.request.user.user_type == 'driver':
            driver = getattr(self.request.user, 'driver_profile', None)
            if driver:
                return DriverActivity.objects.filter(driver=driver)
        return DriverActivity.objects.none()
    
    @action(detail=False, methods=['get'], permission_classes=[IsApproved, IsDriver])
    def my_activity(self, request):
        """Get current driver's activity"""
        try:
            driver = Driver.objects.get(user=request.user)
            activities = DriverActivity.objects.filter(driver=driver).order_by('-created_at')[:50]
            stats = driver.get_activity_stats(days=30)
            
            return Response({
                'recent_activities': DriverActivitySerializer(activities, many=True).data,
                'stats': stats
            })
        except Driver.DoesNotExist:
            return Response({'recent_activities': [], 'stats': {}}, status=status.HTTP_200_OK)
    
    @action(detail=False, methods=['get'], permission_classes=[IsApproved, IsDriver])
    def my_heatmap(self, request):
        """Get current driver's activity heatmap"""
        try:
            driver = Driver.objects.get(user=request.user)
            year = request.query_params.get('year')
            if year:
                year = int(year)
            
            heatmap_data = driver.get_activity_heatmap(year)
            return Response(heatmap_data)
        except Driver.DoesNotExist:
            return Response({}, status=status.HTTP_200_OK)


@extend_schema_view(
    list=extend_schema(tags=['System Configuration']),
    retrieve=extend_schema(tags=['System Configuration']),
    create=extend_schema(tags=['System Configuration']),
    update=extend_schema(tags=['System Configuration']),
    destroy=extend_schema(tags=['System Configuration']),
)
class LicenseClassViewSet(viewsets.ModelViewSet):
    """License Class management for admins"""
    queryset = LicenseClass.objects.all()
    serializer_class = LicenseClassSerializer
    
    def get_permissions(self):
        """
        Instantiates and returns the list of permissions that this view requires.
        Allow authenticated users to read license classes, but only admins can modify them.
        """
        if self.action in ['list', 'retrieve']:
            permission_classes = [permissions.IsAuthenticated]
        else:
            permission_classes = [IsApproved, IsAdmin]
        return [permission() for permission in permission_classes]
    
    def get_queryset(self):
        """Return all license classes ordered alphabetically by name"""
        return LicenseClass.objects.all().order_by('name')