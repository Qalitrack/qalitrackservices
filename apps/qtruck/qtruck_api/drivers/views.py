"""
Driver-related views for the drivers app - Phase 1: ViewSet Consolidation

This module contains the consolidated DriverProfileViewSet that merges functionality from:
- DriverViewSet: Basic driver management
- DriverProfileViewSet: Driver profile with approval workflow  
- DriverEnhancedViewSet: Enhanced driver data with analytics
- DriverActivityViewSet: Driver activity tracking

All functionality has been preserved while providing a single, comprehensive API endpoint.
"""

from rest_framework import viewsets, status
from rest_framework.decorators import action
from rest_framework.response import Response
from django.utils import timezone
from datetime import timedelta
from drf_spectacular.utils import extend_schema_view, extend_schema, OpenApiParameter
from drf_spectacular.types import OpenApiTypes

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
    list=extend_schema(tags=["Drivers"]),
    retrieve=extend_schema(tags=["Drivers"]),
    create=extend_schema(tags=["Drivers"]),
    update=extend_schema(tags=["Drivers"]),
    partial_update=extend_schema(tags=["Drivers"]),
    destroy=extend_schema(tags=["Drivers"]),
    submit=extend_schema(tags=["Drivers"]),
    approve=extend_schema(tags=["Drivers"]),
    me=extend_schema(tags=["Drivers"]),
    history=extend_schema(tags=["Drivers"])
)
class DriverProfileViewSet(viewsets.ModelViewSet):
    """
    Consolidated driver profile management ViewSet.
    
    This ViewSet merges all driver-related functionality:
    - Basic CRUD operations (from DriverViewSet)
    - Profile approval workflow (from DriverProfileViewSet)
    - Enhanced analytics and activity tracking (from DriverEnhancedViewSet)
    - Driver-specific activity endpoints (from DriverActivityViewSet)
    
    Endpoints:
    - Standard CRUD: GET, POST, PUT, PATCH, DELETE /drivers/
    - Profile management: /drivers/me/ (GET, PATCH)
    - Activity tracking: /drivers/{id}/activity/
    - Heatmap data: /drivers/{id}/heatmap/
    - Admin approval: /drivers/{id}/approve/
    - Admin utilities: /drivers/pending/, /drivers/expiring_licenses/
    """
    queryset = DriverProfile.objects.all()
    permission_classes = [IsApproved, IsAdminOrDriver]
    
    def get_serializer_class(self):
        """Return appropriate serializer based on action"""
        if self.action == 'create':
            return DriverProfileCreateSerializer
        elif self.action in ['update', 'partial_update']:
            return DriverProfileUpdateSerializer
        elif self.action == 'approve':
            return DriverProfileApprovalSerializer
        return DriverProfileSerializer
    
    def get_permissions(self):
        """Apply action-specific permissions following existing patterns"""
        if self.action == 'approve':
            return [IsApproved(), IsAdmin()]
        elif self.action == 'me':
            return [IsApproved(), IsDriver()]
        elif self.action in ['update', 'partial_update', 'destroy']:
            return [IsApproved(), IsAdminOrDriverOrTester()]
        elif self.action == 'create':
            return [IsApproved()]
        return [IsApproved(), IsAdminOrDriver()]
    
    def get_queryset(self):
        """Filter queryset based on user permissions and action"""
        if self.request.user.user_type == 'admin':
            # Admin should only see profiles that have been submitted for approval (not unsubmitted drafts)
            base_queryset = DriverProfile.objects.exclude(status='draft', submitted_at__isnull=True).select_related('driver', 'reviewed_by', 'driver__user')
        elif self.request.user.user_type == 'driver':
            driver = getattr(self.request.user, 'driver_profile', None)
            if driver:
                base_queryset = DriverProfile.objects.filter(driver=driver)
            else:
                base_queryset = DriverProfile.objects.none()
        else:
            base_queryset = DriverProfile.objects.none()
        
        # For activity and heatmap actions, we need the related data
        if self.action in ['activity', 'heatmap']:
            base_queryset = base_queryset.prefetch_related('driver__activities')
            
        return base_queryset
    
    def perform_create(self, serializer):
        """Create driver profile with activity logging"""
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
    
    @extend_schema(
        parameters=[
            OpenApiParameter('status', OpenApiTypes.STR, description='Filter by status (admin only): pending, approved, draft, rejected'),
            OpenApiParameter('license_expiring', OpenApiTypes.INT, description='Filter by licenses expiring within N days (admin only)'),
            OpenApiParameter('license_expires_before', OpenApiTypes.DATE, description='Filter by licenses expiring before date (admin only)'),
            OpenApiParameter('has_pending_version', OpenApiTypes.BOOL, description='Filter by pending version status'),
            OpenApiParameter('include', OpenApiTypes.STR, description='Include enhanced data: stats, activity, heatmap (comma-separated)'),
            OpenApiParameter('days', OpenApiTypes.INT, description='Days for activity/heatmap data (default: 30)'),
        ]
    )
    def list(self, request, *args, **kwargs):
        """List driver profiles with query filters and enhanced data includes"""
        queryset = self.get_queryset()
        
        # Apply admin-only filters
        if request.user.user_type == 'admin':
            # Status filter
            status = request.query_params.get('status')
            if status:
                queryset = queryset.filter(status=status)
            
            # License expiring filter
            license_expiring = request.query_params.get('license_expiring')
            if license_expiring:
                try:
                    days = int(license_expiring)
                    warning_date = timezone.now().date() + timedelta(days=days)
                    queryset = queryset.filter(
                        is_current=True,
                        license_expiry_date__lte=warning_date
                    )
                except ValueError:
                    pass
            
            # License expires before filter
            license_expires_before = request.query_params.get('license_expires_before')
            if license_expires_before:
                try:
                    from datetime import datetime
                    expire_date = datetime.strptime(license_expires_before, '%Y-%m-%d').date()
                    queryset = queryset.filter(
                        is_current=True,
                        license_expiry_date__lte=expire_date
                    )
                except ValueError:
                    pass
        
        # Has pending version filter (available to all users)
        has_pending = request.query_params.get('has_pending_version')
        if has_pending is not None:
            has_pending_bool = has_pending.lower() in ['true', '1', 'yes']
            if has_pending_bool:
                # Get drivers that have pending versions
                drivers_with_pending = DriverProfile.objects.filter(
                    status__in=['pending', 'draft']
                ).values_list('driver', flat=True)
                queryset = queryset.filter(driver__in=drivers_with_pending)
            else:
                # Get drivers that don't have pending versions
                drivers_with_pending = DriverProfile.objects.filter(
                    status__in=['pending', 'draft']
                ).values_list('driver', flat=True)
                queryset = queryset.exclude(driver__in=drivers_with_pending)
        
        # Paginate the queryset
        page = self.paginate_queryset(queryset)
        if page is not None:
            serializer = self.get_serializer(page, many=True)
            response_data = serializer.data
        else:
            serializer = self.get_serializer(queryset, many=True)
            response_data = serializer.data
        
        # Add enhanced data based on include parameters
        include_params = request.query_params.get('include', '').split(',')
        include_params = [param.strip().lower() for param in include_params if param.strip()]
        days = int(request.query_params.get('days', 30))
        
        if include_params:
            for i, profile_data in enumerate(response_data):
                profile = page[i] if page else queryset[i]
                driver = profile.driver
                
                if 'stats' in include_params:
                    profile_data['stats'] = driver.get_activity_stats(days)
                
                if 'activity' in include_params:
                    activities = DriverActivity.objects.filter(driver=driver).order_by('-created_at')[:50]
                    profile_data['recent_activity'] = {
                        'activities': DriverActivitySerializer(activities, many=True).data,
                        'stats': driver.get_activity_stats(days)
                    }
                
                if 'heatmap' in include_params:
                    year = request.query_params.get('year')
                    if year:
                        year = int(year)
                    profile_data['heatmap_data'] = driver.get_activity_heatmap(year)
        
        if page is not None:
            return self.get_paginated_response(response_data)
        return Response(response_data)
    
    @action(detail=True, methods=['post'], permission_classes=[IsAdminOrDriverOrTester])
    @extend_schema(
        summary="Submit driver profile for approval (drivers only)",
        responses={200: {'description': 'Profile submitted for approval'}}
    )
    def submit(self, request, pk=None):
        """Submit driver profile for approval (drivers can only submit their own profiles)"""
        profile = self.get_object()
        
        # Ensure drivers can only submit their own profiles
        if request.user.user_type == 'driver':
            try:
                driver = Driver.objects.get(user=request.user)
                if profile.driver != driver:
                    return Response(
                        {'detail': 'You can only submit your own profile for approval'}, 
                        status=status.HTTP_403_FORBIDDEN
                    )
            except Driver.DoesNotExist:
                return Response(
                    {'detail': 'Driver profile not found'}, 
                    status=status.HTTP_404_NOT_FOUND
                )
        
        # Check if profile can be submitted (draft or changes_requested)
        if profile.status not in ['draft', 'changes_requested']:
            return Response(
                {'detail': 'Only draft or changes_requested profiles can be submitted for approval'}, 
                status=status.HTTP_400_BAD_REQUEST
            )
        
        # Submit for approval using the model method
        profile.submit_for_approval()
        
        # Log activity
        log_driver_activity(
            driver=profile.driver,
            activity_type='profile_update',
            activity_data={'action': 'submitted_for_approval', 'version': profile.version_number},
            request=request
        )
        
        return Response({'message': 'Profile submitted for approval successfully'})

    @action(detail=True, methods=['post'], permission_classes=[IsAdmin])
    @extend_schema(
        summary="Approve, reject, or request changes for a driver profile",
        request=DriverProfileApprovalSerializer,
        responses={200: {'description': 'Profile action completed'}}
    )
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
    
    
    @action(detail=True, methods=['get'])
    @extend_schema(
        summary="Get profile change history",
        responses={200: DriverProfileChangeSerializer(many=True)}
    )
    def history(self, request, pk=None):
        """Get profile change history"""
        profile = self.get_object()
        changes = DriverProfileChange.objects.filter(new_profile=profile)
        serializer = DriverProfileChangeSerializer(changes, many=True)
        return Response(serializer.data)
    
    
    
    
    @action(detail=False, methods=['get', 'patch'], permission_classes=[IsApproved, IsDriver])
    @extend_schema(
        summary="Get or update current driver's profile with enhanced data support",
        parameters=[
            OpenApiParameter(
                'include', 
                OpenApiTypes.STR, 
                description='Comma-separated list of additional data to include (activity, heatmap, stats)'
            ),
            OpenApiParameter('activity_days', OpenApiTypes.INT, description='Days for activity data (default: 30)'),
            OpenApiParameter('heatmap_days', OpenApiTypes.INT, description='Days for heatmap data (default: 90)'),
        ],
        responses={200: DriverProfileSerializer}
    )
    def me(self, request):
        """
        Enhanced /me/ endpoint with include parameter support.
        
        This consolidates functionality from DriverProfileViewSet.me() and 
        DriverActivityViewSet.my_activity() and DriverActivityViewSet.my_heatmap().
        
        GET: Returns current driver's profile with optional enhanced data
        PATCH: Updates current driver's profile
        
        Include parameters:
        - activity: Include recent activity data
        - heatmap: Include heatmap data for calendar visualization  
        - stats: Include driver statistics
        """
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
            
            # Check if there's a pending version (including changes_requested)
            pending_profile = DriverProfile.objects.filter(
                driver=driver, 
                status__in=[DriverProfileStatus.PENDING, DriverProfileStatus.DRAFT, DriverProfileStatus.CHANGES_REQUESTED]
            ).order_by('-created_at').first()
            
            profile_to_use = approved_profile or pending_profile
            
            if not profile_to_use:
                # No profile exists - return proper response for frontend to show "Create Profile"
                return Response({
                    'detail': 'No driver profile found',
                    'has_pending_version': False,
                    'pending_version_id': None,
                    'pending_status': None,
                    'is_first_profile': True,
                    'needs_creation': True
                }, status=status.HTTP_404_NOT_FOUND)
            
            # Base profile data
            serializer = DriverProfileSerializer(profile_to_use)
            response_data = serializer.data
            
            # Remove admin-sensitive information for driver privacy
            response_data.pop('reviewed_by', None)
            response_data.pop('reviewed_by_name', None)
            
            # Add profile status metadata
            response_data['has_pending_version'] = bool(pending_profile)
            response_data['pending_version_id'] = str(pending_profile.id) if pending_profile else None
            response_data['pending_status'] = pending_profile.status if pending_profile else None
            response_data['is_first_profile'] = not bool(approved_profile)
            
            # Parse include parameter for enhanced data
            include_params = request.query_params.get('include', '').split(',')
            include_params = [param.strip().lower() for param in include_params if param.strip()]
            
            # Add enhanced data based on include parameters
            if 'stats' in include_params:
                response_data['stats'] = driver.get_activity_stats(days=30)
            
            if 'activity' in include_params:
                activity_days = int(request.query_params.get('activity_days', 30))
                activities = DriverActivity.objects.filter(driver=driver).order_by('-created_at')[:50]
                response_data['recent_activity'] = {
                    'activities': DriverActivitySerializer(activities, many=True).data,
                    'stats': driver.get_activity_stats(activity_days)
                }
            
            if 'heatmap' in include_params:
                year = request.query_params.get('year')
                if year:
                    year = int(year)
                response_data['heatmap_data'] = driver.get_activity_heatmap(year)
            
            return Response(response_data)
        
        elif request.method == 'PATCH':
            # Update logic (preserved from original DriverProfileViewSet.me)
            current_profile = DriverProfile.objects.filter(driver=driver, is_current=True).first()
            pending_profile = DriverProfile.objects.filter(
                driver=driver,
                status__in=[DriverProfileStatus.PENDING, DriverProfileStatus.DRAFT, DriverProfileStatus.CHANGES_REQUESTED]
            ).order_by('-created_at').first()
            
            if pending_profile:
                # Use existing pending profile - DO NOT create new one even if changes_requested
                profile_to_update = pending_profile
                
                # If status is changes_requested, reset it to draft so user can resubmit
                if pending_profile.status == DriverProfileStatus.CHANGES_REQUESTED:
                    pending_profile.status = DriverProfileStatus.DRAFT
                    pending_profile.save()
            else:
                # No pending profile exists - create a new version based on current profile or create fresh
                if current_profile:
                    # Create new version based on current approved profile
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
                    # Create completely new profile (first time)
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
                response_data = response_serializer.data
                
                # Remove admin-sensitive information for driver privacy
                response_data.pop('reviewed_by', None)
                response_data.pop('reviewed_by_name', None)
                
                return Response(response_data)
            else:
                return Response(serializer.errors, status=status.HTTP_400_BAD_REQUEST)