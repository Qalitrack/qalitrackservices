"""
Driver-related views for the drivers app

This module contains all driver-related views migrated from the settings app:
- DriverViewSet: Basic driver management
- DriverProfileViewSet: Driver profile with approval workflow
- DriverEnhancedViewSet: Enhanced driver data with analytics
- DriverActivityViewSet: Driver activity tracking
"""

from rest_framework import viewsets, status
from rest_framework.decorators import action
from rest_framework.response import Response
from django.utils import timezone
from datetime import timedelta
from drf_spectacular.utils import extend_schema_view, extend_schema

# Import models and serializers
from .models import (
    Driver, DriverProfile, DriverActivity, DriverProfileChange, 
    DriverProfileStatus, log_driver_activity
)
from .serializers import (
    DriverSerializer, DriverProfileSerializer, DriverProfileCreateSerializer,
    DriverProfileUpdateSerializer, DriverProfileApprovalSerializer,
    DriverEnhancedSerializer, DriverActivitySerializer, DriverProfileChangeSerializer
)

# Import permissions from settings app (until we create a shared permissions module)
from settings.permissions import (
    IsApproved, IsAdmin, IsDriver, IsAdminOrDriver, IsAdminOrDriverOrTester
)

# Import SystemSettings from settings app
from settings.models import SystemSettings


@extend_schema_view(
    list=extend_schema(),
    retrieve=extend_schema(),
    create=extend_schema(),
    update=extend_schema(),
    partial_update=extend_schema(),
    destroy=extend_schema(),
)
class DriverViewSet(viewsets.ModelViewSet):
    """Basic driver management viewset"""
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
    list=extend_schema(),
    retrieve=extend_schema(),
    create=extend_schema(),
    update=extend_schema(),
    partial_update=extend_schema(),
    destroy=extend_schema(),
    approve=extend_schema(),
    pending_profiles=extend_schema(),
    me=extend_schema(),
    history=extend_schema(),
)
class DriverProfileViewSet(viewsets.ModelViewSet):
    """Driver profile management with approval workflow"""
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
    def pending(self, request):
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
                
                serializer = DriverProfileSerializer(profile)
                response_data = serializer.data
                response_data['has_pending_version'] = True
                response_data['pending_version_id'] = str(profile.id)
                response_data['pending_status'] = profile.status
                response_data['is_first_profile'] = True
                return Response(response_data)
        
        elif request.method == 'PATCH':
            # Get or create a draft/pending profile to update
            current_profile = DriverProfile.objects.filter(driver=driver, is_current=True).first()
            pending_profile = DriverProfile.objects.filter(
                driver=driver,
                status__in=[DriverProfileStatus.PENDING, DriverProfileStatus.DRAFT]
            ).order_by('-created_at').first()
            
            if pending_profile:
                # Update the existing pending profile
                profile_to_update = pending_profile
            else:
                # Create a new version based on the current profile or create fresh
                if current_profile:
                    # Create new version based on current
                    profile_data = {
                        'full_name': current_profile.full_name,
                        'phone_number': current_profile.phone_number,
                        'id_number': current_profile.id_number,
                        'license_number': current_profile.license_number,
                        'license_expiry_date': current_profile.license_expiry_date,
                        'profile_photo': current_profile.profile_photo,
                        'license_front_image': current_profile.license_front_image,
                        'license_back_image': current_profile.license_back_image,
                        'id_front_image': current_profile.id_front_image,
                        'id_back_image': current_profile.id_back_image,
                        'status': DriverProfileStatus.DRAFT
                    }
                    profile_to_update = DriverProfile.objects.create(driver=driver, **profile_data)
                    # Copy license classes
                    profile_to_update.license_classes.set(current_profile.license_classes.all())
                else:
                    # Create completely new profile
                    profile_to_update = DriverProfile.objects.create(
                        driver=driver,
                        full_name=driver.user.get_full_name() or '',
                        phone_number='',
                        id_number='',
                        license_number='',
                        license_expiry_date=timezone.now().date() + timedelta(days=365),
                        status=DriverProfileStatus.DRAFT
                    )
            
            # Update the profile
            serializer = DriverProfileUpdateSerializer(
                profile_to_update, 
                data=request.data, 
                partial=True,
                context={'request': request}
            )
            
            if serializer.is_valid():
                updated_profile = serializer.save()
                
                # Log activity
                log_driver_activity(
                    driver=driver,
                    activity_type='profile_update',
                    activity_data={'action': 'profile_updated', 'version': updated_profile.version_number},
                    request=request
                )
                
                response_serializer = DriverProfileSerializer(updated_profile, context={'request': request})
                return Response(response_serializer.data)
            else:
                return Response(serializer.errors, status=status.HTTP_400_BAD_REQUEST)


@extend_schema_view(
    list=extend_schema(),
    retrieve=extend_schema(),
    activity=extend_schema(),
    heatmap=extend_schema(),
    expiring_licenses=extend_schema(),
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
    list=extend_schema(),
    retrieve=extend_schema(),
    my_activity=extend_schema(),
    my_heatmap=extend_schema(),
)
class DriverActivityViewSet(viewsets.ReadOnlyModelViewSet):
    """Driver activity tracking and analytics"""
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