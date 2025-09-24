from rest_framework import viewsets, status, permissions
from rest_framework.decorators import action
from rest_framework.response import Response
from django.utils import timezone
from drf_spectacular.utils import extend_schema, extend_schema_view

from .models import CustomUser
from .serializers import CustomUserSerializer
from settings.permissions import (
    IsAdmin, IsApproved, IsAdminOrOwner
)




@extend_schema_view(
    list=extend_schema(tags=["Users"]),
    retrieve=extend_schema(tags=["Users"]),
    create=extend_schema(tags=["Users"]),
    update=extend_schema(tags=["Users"]),
    partial_update=extend_schema(tags=["Users"]),
    destroy=extend_schema(tags=["Users"]),
    me=extend_schema(tags=["Users"])
)
class UserViewSet(viewsets.ModelViewSet):
    queryset = CustomUser.objects.all()
    serializer_class = CustomUserSerializer
    permission_classes = [IsAdminOrOwner]
    
    def get_queryset(self):
        # Handle filtering by status or user_type via query parameters
        queryset = CustomUser.objects.all()
        
        # For retrieve actions, don't filter queryset - let permissions handle access control
        if not self.request.user.user_type == 'admin' and self.action == 'list':
            # Non-admins can only see their own user record in list view
            return queryset.filter(id=self.request.user.id)
        
        # Admin filtering options
        status = self.request.query_params.get('status')
        user_type = self.request.query_params.get('user_type')
        
        if status == 'preapproval':
            queryset = queryset.filter(status='preapproval')
        elif status == 'rejected':
            queryset = queryset.filter(status='rejected')
        elif status == 'approved':
            queryset = queryset.filter(status='approved')
        elif status == 'inactive':
            queryset = queryset.filter(is_active=False)
            
        if user_type and user_type in ['admin', 'driver', 'tester']:
            queryset = queryset.filter(user_type=user_type)
            
        return queryset
    
    def get_permissions(self):
        """Set permissions based on action"""
        if self.action == 'create':
            # Only admins can create users via this endpoint
            permission_classes = [IsAdmin]
        elif self.action == 'list':
            # Only admins can list all users
            permission_classes = [IsAdmin]
        elif self.action in ['approve', 'destroy']:
            # Only admins can approve or delete users
            permission_classes = [IsAdmin]
        elif self.action in ['update', 'partial_update']:
            # Admins or owners can update
            permission_classes = [IsAdminOrOwner]
        else:
            # Default to admin or owner for other actions
            permission_classes = [IsAdminOrOwner]
        return [permission() for permission in permission_classes]
    
    
    def update(self, request, *args, **kwargs):
        """Handle user updates including activation/deactivation/approval"""
        partial = kwargs.pop('partial', False)
        instance = self.get_object()
        
        # Handle status transitions (admin only)
        if 'status' in request.data:
            if request.user.user_type != 'admin':
                return Response(
                    {'error': 'Only admins can change user status'}, 
                    status=status.HTTP_403_FORBIDDEN
                )
                
            new_status = request.data.get('status')
            current_status = instance.status
            
            # Validate status transitions
            valid_transitions = {
                'preapproval': ['approved', 'rejected'],
                'approved': ['rejected'],  # Can reject approved users
                'rejected': ['preapproval']  # Can move rejected back to preapproval
            }
            
            if new_status not in valid_transitions.get(current_status, []):
                return Response(
                    {'error': f'Invalid status transition from {current_status} to {new_status}'}, 
                    status=status.HTTP_400_BAD_REQUEST
                )
            
            instance.status = new_status
            instance.save()
            
            # Update profile if user is being approved
            if new_status == 'approved' and current_status != 'approved':
                from django.utils import timezone
                from .models import UserProfile
                
                profile, created = UserProfile.objects.get_or_create(user=instance)
                profile.approval_date = timezone.now()
                profile.approved_by = request.user
                profile.save()
        
        # Handle activation/deactivation logic (admin only)
        if 'is_active' in request.data and 'status' not in request.data:
            if request.user.user_type != 'admin':
                return Response(
                    {'error': 'Only admins can activate/deactivate users'}, 
                    status=status.HTTP_403_FORBIDDEN
                )
                
            is_active = request.data.get('is_active')
            
            if not is_active and instance.user_type == 'admin':
                # Prevent deactivating the last admin
                active_admin_count = CustomUser.objects.filter(
                    user_type='admin', is_active=True, status='approved'
                ).exclude(id=instance.id).count()
                
                if active_admin_count == 0:
                    return Response(
                        {'error': 'Cannot deactivate the last admin user'}, 
                        status=status.HTTP_400_BAD_REQUEST
                    )
        
        # Handle regular field updates
        serializer = self.get_serializer(instance, data=request.data, partial=partial)
        serializer.is_valid(raise_exception=True)
        self.perform_update(serializer)
        
        return Response(serializer.data)
    
    def destroy(self, request, *args, **kwargs):
        """Delete user (only deactivated/rejected users)"""
        instance = self.get_object()
        
        # Only allow deletion of deactivated users or rejected users
        if instance.is_active and instance.status == 'approved':
            return Response(
                {'error': 'Cannot delete active and approved users. Deactivate first.'}, 
                status=status.HTTP_400_BAD_REQUEST
            )
        
        # Prevent deleting the last admin
        if instance.user_type == 'admin':
            active_admin_count = CustomUser.objects.filter(
                user_type='admin', is_active=True, status='approved'
            ).exclude(id=instance.id).count()
            
            if active_admin_count == 0:
                return Response(
                    {'error': 'Cannot delete the last admin user'}, 
                    status=status.HTTP_400_BAD_REQUEST
                )
        
        user_email = instance.email
        self.perform_destroy(instance)
        
        return Response({
            'message': f'User {user_email} deleted successfully'
        })
    
    @action(detail=False, methods=['get'], permission_classes=[permissions.IsAuthenticated])
    def me(self, request):
        """Get current user information"""
        serializer = self.get_serializer(request.user)
        return Response(serializer.data)


