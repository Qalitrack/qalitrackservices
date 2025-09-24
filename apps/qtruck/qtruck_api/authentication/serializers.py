from rest_framework_simplejwt.serializers import TokenObtainPairSerializer
from rest_framework_simplejwt.views import TokenObtainPairView
from rest_framework import serializers
from django.contrib.auth import authenticate
from django.contrib.auth.password_validation import validate_password
from drf_spectacular.utils import extend_schema
from users.models import CustomUser
from settings.models import SystemSettings


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


class UserRegistrationSerializer(serializers.ModelSerializer):
    password = serializers.CharField(write_only=True, validators=[validate_password])
    password_confirm = serializers.CharField(write_only=True)
    
    class Meta:
        model = CustomUser
        fields = ('email', 'password', 'password_confirm', 'first_name', 'last_name')
    
    def validate(self, attrs):
        if attrs['password'] != attrs['password_confirm']:
            raise serializers.ValidationError("Passwords don't match.")
        
        email = attrs['email']
        
        # Parse email for role detection - require + alias
        if '@' not in email:
            raise serializers.ValidationError({'email': 'Invalid email format.'})
            
        email_parts = email.split('@')
        if '+' not in email_parts[0]:
            raise serializers.ValidationError({'email': 'Email must contain a role alias (e.g., user+admin@example.com, user+driver@example.com, or user+tester@example.com).'})
        
        parts = email_parts[0].split('+')
        if len(parts) != 2 or not parts[1]:
            raise serializers.ValidationError({'email': 'Email must contain a valid role alias (e.g., user+admin@example.com).'})
            
        base_part = parts[0]
        role_part = parts[1].lower()
        base_email = f"{base_part}@{email_parts[1]}"
        
        if role_part == 'admin':
            user_type = 'admin'
        elif role_part == 'driver':
            user_type = 'driver'
        elif role_part == 'tester':
            user_type = 'tester'
        else:
            raise serializers.ValidationError({'email': f'Invalid role alias: {role_part}. Use +admin, +driver, or +tester.'})
        
        # Check if trying to register as admin when admins already exist
        if user_type == 'admin':
            existing_admins = CustomUser.objects.filter(user_type='admin').count()
            if existing_admins > 0:
                raise serializers.ValidationError({'email': 'Admin accounts can only be created by existing admins after the first user. Please contact an administrator or use +driver/+tester.'})
        
        # Check tester registration permissions
        if user_type == 'tester':
            settings = SystemSettings.get_settings()
            if not settings.tester_registration_enabled:
                raise serializers.ValidationError('Tester registration is currently disabled.')
        
        # Check if user with same email already exists (email includes alias)
        if CustomUser.objects.filter(email=email).exists():
            raise serializers.ValidationError({'email': 'User with this email already exists.'})
        
        attrs['user_type'] = user_type
        
        return attrs
    
    def create(self, validated_data):
        validated_data.pop('password_confirm')
        password = validated_data.pop('password')
        
        user = CustomUser.objects.create_user(
            username=validated_data['email'],  # Use email as username
            email=validated_data['email'],
            password=password,
            first_name=validated_data.get('first_name', ''),
            last_name=validated_data.get('last_name', ''),
            user_type=validated_data['user_type']
        )
        
        return user