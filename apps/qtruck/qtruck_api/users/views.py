from rest_framework import viewsets, status, permissions, serializers
from rest_framework.decorators import api_view, permission_classes
from rest_framework.response import Response
from rest_framework.views import APIView
from rest_framework_simplejwt.authentication import JWTAuthentication
from django.utils import timezone
from drf_spectacular.utils import extend_schema, OpenApiParameter, inline_serializer

from .models import CustomUser, UserProfile
from .serializers import CustomUserSerializer, UserProfileSerializer, UserApprovalSerializer
from settings.permissions import (
    IsAdmin, IsApproved, IsAdminOrOwner
)


class UserApprovalView(APIView):
    authentication_classes = [JWTAuthentication]
    permission_classes = [IsAdmin]
    
    @extend_schema(
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
        responses={200: CustomUserSerializer(many=True)}
    )
    def get(self, request):
        pending_users = CustomUser.objects.filter(is_approved=False)
        serializer = CustomUserSerializer(pending_users, many=True)
        return Response(serializer.data)


class UsersByTypeView(APIView):
    permission_classes = [IsAdmin]
    
    @extend_schema(
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


class UserProfileViewSet(viewsets.ReadOnlyModelViewSet):
    queryset = UserProfile.objects.all()
    serializer_class = UserProfileSerializer
    permission_classes = [IsApproved, IsAdminOrOwner]
    
    def get_queryset(self):
        if self.request.user.user_type == 'admin':
            return UserProfile.objects.all()
        else:
            return UserProfile.objects.filter(user=self.request.user)


@api_view(['GET'])
@permission_classes([permissions.IsAuthenticated])
def user_info(request):
    """Get current user information"""
    serializer = CustomUserSerializer(request.user)
    return Response(serializer.data)