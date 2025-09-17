from rest_framework_simplejwt.serializers import TokenObtainPairSerializer
from rest_framework_simplejwt.views import TokenObtainPairView
from rest_framework import serializers
from django.contrib.auth import authenticate
from .models import CustomUser, SystemSettings


class EmailAliasTokenObtainPairSerializer(TokenObtainPairSerializer):
    def validate(self, attrs):
        email = attrs.get('username')  # DRF uses 'username' field for email
        password = attrs.get('password')
        
        if not email or not password:
            raise serializers.ValidationError('Email and password are required.')
        
        # Parse email for role detection
        user_type = 'admin'  # default when no alias
        base_email = email
        
        if '@' in email:
            email_parts = email.split('@')
            if '+' in email_parts[0]:
                parts = email_parts[0].split('+')
                base_part = parts[0]
                role_part = parts[1] if len(parts) > 1 else ''
                base_email = f"{base_part}@{email_parts[1]}"
                
                if role_part == 'admin':
                    user_type = 'admin'
                elif role_part == 'driver':
                    user_type = 'driver'
                elif role_part == 'tester':
                    user_type = 'tester'
                else:
                    raise serializers.ValidationError(f'Invalid role alias: {role_part}. Use +admin, +driver, or +tester.')
            # If no alias, user_type remains 'admin'
        
        # Check tester permissions
        if user_type == 'tester':
            settings = SystemSettings.get_settings()
            if not settings.tester_login_enabled:
                raise serializers.ValidationError('Tester login is currently disabled.')
        
        # Find user by base_email and user_type
        try:
            user = CustomUser.objects.get(base_email=base_email, user_type=user_type)
        except CustomUser.DoesNotExist:
            raise serializers.ValidationError('Invalid credentials.')
        
        # Check if user is approved
        if not user.is_approved:
            raise serializers.ValidationError('Account pending approval. Please contact administrator.')
        
        # Verify password
        if not user.check_password(password):
            raise serializers.ValidationError('Invalid credentials.')
        
        # Set the user for token generation
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