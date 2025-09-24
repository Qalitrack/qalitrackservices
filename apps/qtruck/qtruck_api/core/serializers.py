from rest_framework import serializers
from django.contrib.auth.password_validation import validate_password
from django.core.exceptions import ValidationError as DjangoValidationError
from datetime import datetime
from .models import (
    CustomUser, UserProfile, SystemSettings, LicenseClass, Driver, Truck, Trip,
    Material, MaterialPhoto, MaterialCost, Expense, Receipt, VehicleMileage, Feedback,
    DriverProfile, DriverActivity, DriverProfileChange
)


class FlexibleDateField(serializers.DateField):
    """Custom date field that accepts multiple date formats"""
    
    def __init__(self, **kwargs):
        # Define multiple input formats that we want to accept
        self.input_formats = [
            '%Y-%m-%d',      # 2025-12-31 (ISO format)
            '%m/%d/%Y',      # 12/31/2025 (US format)
            '%d/%m/%Y',      # 31/12/2025 (European format)
            '%Y/%m/%d',      # 2025/12/31 (Alternative format)
            '%m-%d-%Y',      # 12-31-2025
            '%d-%m-%Y',      # 31-12-2025
        ]
        super().__init__(**kwargs)
    
    def to_internal_value(self, value):
        """Convert various date formats to a date object"""
        import logging
        logger = logging.getLogger(__name__)
        
        # Log the original value for debugging
        logger.error(f"FlexibleDateField received value: {repr(value)}, type: {type(value)}")
        
        original_value = value
        
        # Handle list input from multipart form data (Django QueryDict)
        if isinstance(value, (list, tuple)) and len(value) > 0:
            # Take the first element from the list
            value = value[0]
            logger.error(f"Extracted first element from list: {repr(value)}, type: {type(value)}")
        elif isinstance(value, (list, tuple)) and len(value) == 0:
            # Empty list
            logger.error(f"Empty list detected, returning None")
            return None
        
        # Handle multipart form data edge cases
        if hasattr(value, 'read'):
            # If it's a file-like object, read its content
            try:
                value.seek(0)
                content = value.read()
                if isinstance(content, bytes):
                    value = content.decode('utf-8')
                else:
                    value = str(content)
                logger.error(f"File-like object converted to: {repr(value)}")
            except Exception as e:
                logger.error(f"Error reading file-like object: {e}")
                pass
        elif isinstance(value, bytes):
            # Handle bytes data directly
            try:
                value = value.decode('utf-8')
                logger.error(f"Bytes decoded to: {repr(value)}")
            except UnicodeDecodeError as e:
                logger.error(f"Unicode decode error: {e}")
                # If we can't decode it, let the parent handle the error
                return super().to_internal_value(value)
        
        if not value or (isinstance(value, str) and not value.strip()):
            logger.error(f"Empty value detected, returning None")
            return None
            
        if isinstance(value, str):
            # Clean the value more thoroughly for multipart form data
            value_before_clean = value
            value = value.strip()
            # Remove any carriage returns, newlines, or null characters
            value = value.replace('\r', '').replace('\n', '').replace('\x00', '')
            
            logger.error(f"Value before cleaning: {repr(value_before_clean)}")
            logger.error(f"Value after cleaning: {repr(value)}")
            
            if not value:  # Check again after cleaning
                logger.error(f"Value empty after cleaning, returning None")
                return None
            
            # Try each format until one works
            for i, date_format in enumerate(self.input_formats):
                try:
                    parsed_date = datetime.strptime(value, date_format)
                    logger.error(f"Successfully parsed '{value}' with format '{date_format}' (format #{i+1})")
                    return parsed_date.date()
                except ValueError as e:
                    logger.error(f"Format '{date_format}' failed for '{value}': {e}")
                    continue
            
            logger.error(f"All formats failed for value: {repr(value)}")
            # If none of the formats worked, let the parent handle it
            # This will raise the appropriate validation error
        else:
            logger.error(f"Value is not a string after processing: {repr(value)}, type: {type(value)}")
        
        logger.error(f"Falling back to parent validation for: {repr(original_value)}")
        return super().to_internal_value(original_value)


class FlexibleCharField(serializers.CharField):
    """Custom char field that handles list inputs from multipart form data"""
    
    def to_internal_value(self, value):
        import logging
        logger = logging.getLogger(__name__)
        
        logger.error(f"FlexibleCharField received value: {repr(value)}, type: {type(value)}")
        
        # Handle list input from multipart form data (Django QueryDict)
        if isinstance(value, (list, tuple)) and len(value) > 0:
            # Take the first element from the list
            value = value[0]
            logger.error(f"Extracted first element from list: {repr(value)}, type: {type(value)}")
        elif isinstance(value, (list, tuple)) and len(value) == 0:
            # Empty list
            logger.error(f"Empty list detected, returning empty string")
            return ""
        
        # Handle bytes input (might come from some form parsers)
        if isinstance(value, bytes):
            value = value.decode('utf-8')
            logger.error(f"Decoded bytes to string: {repr(value)}")
        
        # Handle file-like objects
        if hasattr(value, 'read'):
            try:
                value.seek(0)
                content = value.read()
                if isinstance(content, bytes):
                    value = content.decode('utf-8')
                else:
                    value = str(content)
                logger.error(f"Read from file-like object: {repr(value)}")
            except Exception as e:
                logger.error(f"Error reading file-like object: {e}")
        
        # Clean whitespace and control characters
        if isinstance(value, str):
            original_value = value
            value = value.strip().replace('\r', '').replace('\n', '').replace('\x00', '')
            logger.error(f"Value before cleaning: {repr(original_value)}")
            logger.error(f"Value after cleaning: {repr(value)}")
        
        # Call parent to handle validation
        return super().to_internal_value(value)


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


class UserProfileSerializer(serializers.ModelSerializer):
    user = serializers.StringRelatedField(read_only=True)
    approved_by = serializers.StringRelatedField(read_only=True)
    
    class Meta:
        model = UserProfile
        fields = '__all__'


class CustomUserSerializer(serializers.ModelSerializer):
    profile = UserProfileSerializer(read_only=True)
    
    class Meta:
        model = CustomUser
        fields = ('id', 'username', 'email', 'base_email', 'first_name', 'last_name', 
                 'user_type', 'is_approved', 'is_active', 'created_at', 'updated_at', 'profile')
        read_only_fields = ('id', 'created_at', 'updated_at')


class SystemSettingsSerializer(serializers.ModelSerializer):
    class Meta:
        model = SystemSettings
        fields = '__all__'


class DriverSerializer(serializers.ModelSerializer):
    user = CustomUserSerializer(read_only=True)
    
    class Meta:
        model = Driver
        fields = '__all__'


class TruckSerializer(serializers.ModelSerializer):
    driver = DriverSerializer(read_only=True)
    driver_id = serializers.UUIDField(write_only=True, required=False, allow_null=True)
    
    class Meta:
        model = Truck
        fields = '__all__'


class MaterialCostSerializer(serializers.ModelSerializer):
    class Meta:
        model = MaterialCost
        fields = '__all__'


class MaterialPhotoSerializer(serializers.ModelSerializer):
    class Meta:
        model = MaterialPhoto
        fields = '__all__'


class MaterialSerializer(serializers.ModelSerializer):
    photos = MaterialPhotoSerializer(many=True, read_only=True)
    
    class Meta:
        model = Material
        fields = ['id', 'name', 'description', 'photos', 'created_at', 'updated_at']
    
    def create(self, validated_data):
        # Create the material instance
        material = Material.objects.create(**validated_data)
        
        # Handle photo uploads from request.FILES
        request = self.context.get('request')
        if request and 'photos' in request.FILES:
            photos = request.FILES.getlist('photos')
            for photo in photos:
                MaterialPhoto.objects.create(material=material, photo=photo)
        
        return material


class ReceiptSerializer(serializers.ModelSerializer):
    class Meta:
        model = Receipt
        fields = '__all__'


class ExpenseSerializer(serializers.ModelSerializer):
    receipts = ReceiptSerializer(many=True, read_only=True)
    trip_id = serializers.UUIDField(write_only=True)
    driver = serializers.SerializerMethodField(read_only=True)
    
    class Meta:
        model = Expense
        fields = '__all__'
        extra_kwargs = {
            'trip': {'read_only': True}
        }
    
    def get_driver(self, obj):
        if obj.trip and obj.trip.driver:
            return DriverSerializer(obj.trip.driver).data
        return None
    
    def create(self, validated_data):
        trip_id = validated_data.pop('trip_id')
        trip = Trip.objects.get(id=trip_id)
        validated_data['trip'] = trip
        return Expense.objects.create(**validated_data)


class TripSerializer(serializers.ModelSerializer):
    truck = TruckSerializer(read_only=True)
    driver = DriverSerializer(read_only=True)
    materials = MaterialSerializer(many=True, read_only=True)
    expenses = ExpenseSerializer(many=True, read_only=True)
    truck_id = serializers.UUIDField(write_only=True)
    driver_id = serializers.UUIDField(write_only=True)
    
    class Meta:
        model = Trip
        fields = '__all__'
        read_only_fields = ('total_cost', 'total_mileage')


class VehicleMileageSerializer(serializers.ModelSerializer):
    truck = TruckSerializer(read_only=True)
    driver = DriverSerializer(read_only=True)
    truck_id = serializers.UUIDField(write_only=True)
    driver_id = serializers.UUIDField(write_only=True)
    
    class Meta:
        model = VehicleMileage
        fields = '__all__'
        read_only_fields = ('mileage',)


class UserApprovalSerializer(serializers.Serializer):
    user_id = serializers.UUIDField()
    action = serializers.ChoiceField(choices=['approve', 'reject'])
    
    def validate_user_id(self, value):
        try:
            user = CustomUser.objects.get(id=value)
            if user.is_approved:
                raise serializers.ValidationError('User is already approved.')
            return value
        except CustomUser.DoesNotExist:
            raise serializers.ValidationError('User not found.')


class FeedbackSerializer(serializers.ModelSerializer):
    user = CustomUserSerializer(read_only=True)
    responded_by = CustomUserSerializer(read_only=True)
    
    class Meta:
        model = Feedback
        fields = '__all__'
        read_only_fields = ('user', 'responded_by', 'response_date', 'status')


class FeedbackResponseSerializer(serializers.Serializer):
    feedback_id = serializers.UUIDField()
    status = serializers.ChoiceField(choices=Feedback.STATUS_CHOICES)
    admin_response = serializers.CharField(required=False, allow_blank=True)
    
    def validate_feedback_id(self, value):
        try:
            feedback = Feedback.objects.get(id=value)
            return value
        except Feedback.DoesNotExist:
            raise serializers.ValidationError('Feedback not found.')


# Driver Profile Serializers
class DriverActivitySerializer(serializers.ModelSerializer):
    activity_type_display = serializers.CharField(source='get_activity_type_display', read_only=True)
    
    class Meta:
        model = DriverActivity
        fields = [
            'id', 'activity_type', 'activity_type_display', 'created_at',
            'activity_data', 'ip_address'
        ]
        read_only_fields = ['id', 'created_at', 'ip_address']


class DriverProfileChangeSerializer(serializers.ModelSerializer):
    change_type_display = serializers.CharField(source='get_change_type_display', read_only=True)
    
    class Meta:
        model = DriverProfileChange
        fields = [
            'id', 'field_name', 'old_value', 'new_value', 
            'change_type', 'change_type_display', 'created_at'
        ]
        read_only_fields = ['id', 'created_at']


class DriverProfileSerializer(serializers.ModelSerializer):
    status_display = serializers.CharField(source='get_status_display', read_only=True)
    reviewed_by_name = serializers.CharField(source='reviewed_by.get_full_name', read_only=True)
    is_license_expired = serializers.BooleanField(read_only=True)
    days_until_license_expiry = serializers.IntegerField(read_only=True)
    license_status = serializers.SerializerMethodField()
    changes_to = DriverProfileChangeSerializer(many=True, read_only=True)
    driver_name = serializers.CharField(source='driver.name', read_only=True)
    
    def get_full_image_url(self, field_value):
        """Convert relative image URLs to full URLs"""
        if field_value:
            request = self.context.get('request')
            if request:
                return request.build_absolute_uri(field_value.url)
            else:
                # Fallback using Django settings if no request context
                from django.conf import settings
                base_url = getattr(settings, 'BACKEND_BASE_URL', 'http://164.68.116.82:8000')
                return f"{base_url}{field_value.url}"
        return None
    
    def to_representation(self, instance):
        """Override to convert image URLs to full URLs"""
        data = super().to_representation(instance)
        
        # Convert image fields to full URLs
        image_fields = ['profile_photo', 'license_front_image', 'license_back_image', 'id_front_image', 'id_back_image']
        for field in image_fields:
            if data.get(field):
                field_value = getattr(instance, field, None)
                data[field] = self.get_full_image_url(field_value)
        
        return data
    
    def get_license_classes(self, obj):
        """Get license classes as a list of objects"""
        return [{'id': lc.id, 'name': lc.name, 'description': lc.description} for lc in obj.license_classes.all()]
    
    def get_license_status(self, obj):
        """Get license status information for this profile"""
        from .models import SystemSettings
        from django.utils import timezone
        
        if not obj.license_expiry_date:
            return 'valid'
            
        if obj.is_license_expired:
            return 'expired'
        
        days_until_expiry = obj.days_until_license_expiry
        
        # If days_until_expiry is -1, it means not expiring soon
        if days_until_expiry == -1:
            return 'valid'
        else:
            return 'expiring_soon'
    
    license_classes = serializers.SerializerMethodField()
    
    class Meta:
        model = DriverProfile
        fields = [
            'id', 'driver', 'driver_name', 'full_name', 'phone_number', 'id_number',
            'license_number', 'license_expiry_date', 'license_classes',
            'profile_photo', 'license_front_image', 'license_back_image',
            'id_front_image', 'id_back_image',
            'status', 'status_display', 'is_current', 'version_number',
            'submitted_at', 'reviewed_at', 'reviewed_by', 'reviewed_by_name',
            'approval_notes', 'rejection_reason', 'created_at', 'updated_at',
            'is_license_expired', 'days_until_license_expiry', 'license_status', 'changes_to'
        ]
        read_only_fields = [
            'id', 'driver', 'version_number', 'submitted_at', 'reviewed_at',
            'reviewed_by', 'created_at', 'updated_at', 'is_current'
        ]
    
    def validate_license_expiry_date(self, value):
        from django.utils import timezone
        if value <= timezone.now().date():
            raise serializers.ValidationError("License expiry date must be in the future.")
        return value


class DriverProfileCreateSerializer(serializers.ModelSerializer):
    """Serializer for creating new driver profiles"""
    
    class Meta:
        model = DriverProfile
        fields = [
            'full_name', 'phone_number', 'id_number',
            'license_number', 'license_expiry_date', 'license_classes',
            'profile_photo', 'license_front_image', 'license_back_image',
            'id_front_image', 'id_back_image'
        ]
    
    def create(self, validated_data):
        # Handle many-to-many field
        license_classes_data = validated_data.pop('license_classes', [])
        
        profile = super().create(validated_data)
        if license_classes_data:
            profile.license_classes.set(license_classes_data)
        return profile
    
    def validate_license_expiry_date(self, value):
        from django.utils import timezone
        if value <= timezone.now().date():
            raise serializers.ValidationError("License expiry date must be in the future.")
        return value


class DriverProfileApprovalSerializer(serializers.Serializer):
    """Serializer for profile approval actions"""
    action = serializers.ChoiceField(choices=['approve', 'reject', 'request_changes'])
    notes = serializers.CharField(required=False, allow_blank=True, max_length=1000)
    
    def validate(self, attrs):
        action = attrs.get('action')
        notes = attrs.get('notes', '')
        
        if action == 'reject' and not notes:
            raise serializers.ValidationError({'notes': 'Rejection reason is required when rejecting a profile.'})
        
        if action == 'request_changes' and not notes:
            raise serializers.ValidationError({'notes': 'Notes are required when requesting changes.'})
        
        return attrs


class DriverProfileUpdateSerializer(serializers.ModelSerializer):
    """Serializer for updating driver profiles"""
    
    # Use our custom fields that handle multipart form data
    license_expiry_date = FlexibleDateField()
    full_name = FlexibleCharField()
    phone_number = FlexibleCharField()
    id_number = FlexibleCharField()
    license_number = FlexibleCharField()
    
    class Meta:
        model = DriverProfile
        fields = [
            'full_name', 'phone_number', 'id_number',
            'license_number', 'license_expiry_date', 'license_classes',
            'profile_photo', 'license_front_image', 'license_back_image',
            'id_front_image', 'id_back_image', 'version_number', 'status', 'is_current'
        ]
        read_only_fields = ('version_number', 'status', 'is_current')
    
    def to_internal_value(self, data):
        """Handle form data conversion for multipart/form-data requests"""
        import io
        import logging
        from django.core.files.uploadedfile import InMemoryUploadedFile
        
        logger = logging.getLogger(__name__)
        logger.error(f"DriverProfileUpdateSerializer received data: {dict(data) if hasattr(data, 'items') else data}")
        
        # Log license_expiry_date specifically
        if 'license_expiry_date' in data:
            led_value = data['license_expiry_date']
            logger.error(f"Raw license_expiry_date value: {repr(led_value)}, type: {type(led_value)}")
            if hasattr(led_value, 'read'):
                logger.error(f"license_expiry_date is file-like object")
            elif isinstance(led_value, bytes):
                logger.error(f"license_expiry_date is bytes")
            else:
                logger.error(f"license_expiry_date is string/other: {repr(led_value)}")
        
        # Make a mutable copy of the data
        if hasattr(data, '_mutable'):
            data._mutable = True
        else:
            data = data.copy()
        
        # Handle string fields that might come as bytes or file-like objects in multipart form data
        # Note: license_expiry_date is handled by FlexibleDateField, so we exclude it here
        string_fields = ['full_name', 'phone_number', 'id_number', 'license_number', 'license_classes']
        
        for field in string_fields:
            if field in data:
                value = data[field]
                
                # If it's an InMemoryUploadedFile (Django sometimes treats form fields as files)
                if isinstance(value, InMemoryUploadedFile):
                    # Read the content as string
                    value.seek(0)  # Reset file pointer
                    content = value.read()
                    if isinstance(content, bytes):
                        data[field] = content.decode('utf-8').strip()
                    else:
                        data[field] = str(content).strip()
                
                # If it's bytes, decode to string
                elif isinstance(value, bytes):
                    data[field] = value.decode('utf-8').strip()
                
                # If it's already a string, ensure it's clean
                elif isinstance(value, str):
                    data[field] = value.strip()
                
                # Handle other types by converting to string
                elif value is not None:
                    data[field] = str(value).strip()
        
        # Special handling for license_classes - convert to list if it's a string
        if 'license_classes' in data:
            license_classes_value = data['license_classes']
            
            # Handle different formats that license_classes might come in
            if isinstance(license_classes_value, str):
                # Remove any surrounding brackets or quotes that might be added by form serialization
                license_classes_str = license_classes_value.strip()
                if license_classes_str.startswith('[') and license_classes_str.endswith(']'):
                    # Handle case like "['uuid1', 'uuid2']" or "['uuid1']"
                    license_classes_str = license_classes_str[1:-1]  # Remove brackets
                    # Remove quotes around individual UUIDs
                    license_classes_str = license_classes_str.replace("'", "").replace('"', '')
                
                # Split by comma if multiple values, otherwise make it a single-item list
                if ',' in license_classes_str:
                    data['license_classes'] = [lc.strip() for lc in license_classes_str.split(',') if lc.strip()]
                else:
                    data['license_classes'] = [license_classes_str.strip()] if license_classes_str.strip() else []
            elif isinstance(license_classes_value, list):
                # Already a list, just ensure each item is a string
                data['license_classes'] = [str(item).strip() for item in license_classes_value if item]
        
        # Note: license_expiry_date is now handled by FlexibleDateField automatically
        
        return super().to_internal_value(data)
    
    def to_representation(self, instance):
        """Return license classes as objects for consistency with read serializer"""
        data = super().to_representation(instance)
        if instance.license_classes.exists():
            data['license_classes'] = [
                {'id': str(lc.id), 'name': lc.name, 'description': lc.description} 
                for lc in instance.license_classes.all()
            ]
        return data
    
    def update(self, instance, validated_data):
        from .driver_models import DriverProfileStatus, DriverProfileChange, DriverProfile
        from django.utils import timezone
        from django.db import models
        
        # Handle many-to-many field
        license_classes_data = validated_data.pop('license_classes', [])
        
        # Create a new profile version instead of updating the current one
        current_profile = instance
        
        # Get the next version number
        max_version = current_profile.driver.profile_versions.aggregate(
            max_version=models.Max('version_number')
        )['max_version'] or 0
        next_version = max_version + 1
        
        # Create new profile version with updated data
        new_profile = DriverProfile.objects.create(
            driver=current_profile.driver,
            version_number=next_version,
            status=DriverProfileStatus.DRAFT,
            is_current=False,  # New version starts as non-current until approved
            **validated_data
        )
        
        # Set license classes
        if license_classes_data:
            new_profile.license_classes.set(license_classes_data)
        
        # Track changes between versions
        fields_to_track = [
            'full_name', 'phone_number', 'id_number', 
            'license_number', 'license_expiry_date', 'license_classes'
        ]
        
        for field in fields_to_track:
            old_value = getattr(current_profile, field, None)
            new_value = getattr(new_profile, field, None)
            
            if old_value != new_value:
                DriverProfileChange.objects.create(
                    old_profile=current_profile,
                    new_profile=new_profile,
                    field_name=field,
                    old_value=str(old_value) if old_value else '',
                    new_value=str(new_value) if new_value else '',
                    change_type='modified' if old_value else 'added'
                )
        
        # Check if auto-approval is enabled
        settings = SystemSettings.get_settings()
        if settings.auto_approve_profile_updates:
            new_profile.approve(user, "Auto-approved profile update")
        
        return new_profile
    
    def validate_license_expiry_date(self, value):
        from django.utils import timezone
        if value <= timezone.now().date():
            raise serializers.ValidationError("License expiry date must be in the future.")
        return value


class DriverEnhancedSerializer(serializers.ModelSerializer):
    """Enhanced driver serializer with profile and activity data"""
    user_email = serializers.CharField(source='user.email', read_only=True)
    user_base_email = serializers.CharField(source='user.base_email', read_only=True)
    user_is_active = serializers.BooleanField(source='user.is_active', read_only=True)
    current_profile = serializers.SerializerMethodField()
    pending_profile = serializers.SerializerMethodField()
    latest_activity = serializers.SerializerMethodField()
    activity_stats = serializers.SerializerMethodField()
    license_status = serializers.SerializerMethodField()
    
    class Meta:
        model = Driver
        fields = [
            'id', 'name', 'phone', 'license_number', 'created_at', 'updated_at',
            'user_email', 'user_base_email', 'user_is_active',
            'current_profile', 'pending_profile', 'latest_activity',
            'activity_stats', 'license_status'
        ]
        read_only_fields = ['id', 'created_at', 'updated_at']
    
    def get_current_profile(self, obj):
        """Get current driver profile"""
        profile = obj.current_profile
        return DriverProfileSerializer(profile, context=self.context).data if profile else None
    
    def get_pending_profile(self, obj):
        """Get pending driver profile"""
        profile = obj.pending_profile
        return DriverProfileSerializer(profile, context=self.context).data if profile else None
    
    def get_latest_activity(self, obj):
        """Get latest driver activity"""
        activity = obj.latest_activity
        return DriverActivitySerializer(activity).data if activity else None
    
    def get_activity_stats(self, obj):
        """Get driver activity statistics"""
        return obj.get_activity_stats(days=30)
    
    def get_license_status(self, obj):
        """Get license status information"""
        current_profile = obj.current_profile
        if not current_profile:
            return {'status': 'no_profile', 'message': 'No approved profile'}
        
        if current_profile.is_license_expired:
            return {
                'status': 'expired',
                'message': f'License expired {abs(current_profile.days_until_license_expiry)} days ago',
                'days_overdue': abs(current_profile.days_until_license_expiry)
            }
        
        days_until_expiry = current_profile.days_until_license_expiry
        settings = SystemSettings.get_settings()
        
        if current_profile.is_license_expiring_soon(settings.license_expiry_warning_days):
            return {
                'status': 'expiring_soon',
                'message': f'License expires in {days_until_expiry} days',
                'days_until_expiry': days_until_expiry
            }
        
        return {
            'status': 'valid',
            'message': f'License valid for {days_until_expiry} days',
            'days_until_expiry': days_until_expiry
        }


class LicenseClassSerializer(serializers.ModelSerializer):
    """Serializer for License Class configuration"""
    
    class Meta:
        model = LicenseClass
        fields = ['id', 'name', 'description']
        read_only_fields = ['id']