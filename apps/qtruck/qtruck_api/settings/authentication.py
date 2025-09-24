from rest_framework_simplejwt.serializers import TokenObtainPairSerializer
from rest_framework_simplejwt.views import TokenObtainPairView
from rest_framework import serializers
from django.contrib.auth import authenticate
from drf_spectacular.utils import extend_schema
from .models import CustomUser, SystemSettings


class EmailAliasTokenObtainPairSerializer(TokenObtainPairSerializer):
    email = serializers.EmailField(required=True)
    
    def __init__(self, *args, **kwargs):
        super().__init__(*args, **kwargs)
        # Remove username field and replace with email
        if 'username' in self.fields:
            del self.fields['username']
    
    def validate(self, attrs):
        email = attrs.get('email')  # Now use 'email' field directly
        password = attrs.get('password')
        
        if not email or not password:
            raise serializers.ValidationError('Email and password are required.')
        
        # Validate email format (must contain alias)
        if '@' not in email or '+' not in email.split('@')[0]:
            raise serializers.ValidationError({'email': 'Email must contain a role alias (e.g., user+admin@example.com, user+driver@example.com, or user+tester@example.com).'})
        
        # Extract user type from email alias for permission checks
        email_parts = email.split('@')
        parts = email_parts[0].split('+')
        role_part = parts[1] if len(parts) > 1 else ''
        
        if role_part == 'tester':
            settings = SystemSettings.get_settings()
            if not settings.tester_login_enabled:
                raise serializers.ValidationError('Tester login is currently disabled.')
        elif role_part not in ['admin', 'driver', 'tester']:
            raise serializers.ValidationError(f'Invalid role alias: {role_part}. Use +admin, +driver, or +tester.')
        
        # Find user by email (which includes the alias)
        try:
            user = CustomUser.objects.get(email=email)
        except CustomUser.DoesNotExist:
            raise serializers.ValidationError({'email': 'Invalid credentials.'})
        
        # Check if user is active (not deactivated)
        if not user.is_active:
            raise serializers.ValidationError({'email': 'Account has been deactivated. Please contact administrator.'})
        
        # Check if user is approved
        if not user.is_approved:
            raise serializers.ValidationError({'email': 'Account pending approval. Please contact administrator.'})
        
        # Verify password
        if not user.check_password(password):
            raise serializers.ValidationError({'password': 'Invalid credentials.'})
        
        # Set the username for parent class validation (SimpleJWT still expects username internally)
        attrs['username'] = user.username
        
        # Call parent validation
        data = super().validate(attrs)
        
        # Add custom claims
        data['user_type'] = user.user_type
        data['user_id'] = str(user.id)
        data['is_approved'] = user.is_approved
        
        return data


class EmailAliasTokenObtainPairView(TokenObtainPairView):
    serializer_class = EmailAliasTokenObtainPairSerializer
    
    @extend_schema(
        summary="Login with email alias",
        description="Login using email (with optional role alias like user+driver@domain.com)"
    )
    def post(self, request, *args, **kwargs):
        return super().post(request, *args, **kwargs)