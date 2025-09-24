"""
Driver-related serializers for the drivers app

This module contains all driver-related serializers migrated from the settings app:
- DriverSerializer: Basic driver serializer
- DriverActivitySerializer: Driver activity tracking
- DriverProfileSerializer: Enhanced driver profile with approval workflow
- DriverEnhancedSerializer: Enhanced driver with profile and activity data
"""

from rest_framework import serializers
from datetime import datetime
from django.conf import settings

# Import models
from .models import Driver, DriverActivity, DriverProfile, DriverProfileChange
from users.models import CustomUser
from settings.models import SystemSettings, LicenseClass


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


class CustomUserSerializer(serializers.ModelSerializer):
    """Basic user serializer for driver relations"""
    class Meta:
        model = CustomUser
        fields = ['id', 'username', 'email', 'first_name', 'last_name', 'user_type', 'is_active']
        read_only_fields = ['id', 'username', 'email', 'user_type']


class DriverSerializer(serializers.ModelSerializer):
    """Basic driver serializer"""
    user = CustomUserSerializer(read_only=True)
    
    class Meta:
        model = Driver
        fields = '__all__'


class DriverActivitySerializer(serializers.ModelSerializer):
    """Serializer for driver activity tracking"""
    activity_type_display = serializers.CharField(source='get_activity_type_display', read_only=True)
    
    class Meta:
        model = DriverActivity
        fields = [
            'id', 'activity_type', 'activity_type_display', 'created_at',
            'activity_data', 'ip_address'
        ]
        read_only_fields = ['id', 'created_at', 'ip_address']


class DriverProfileChangeSerializer(serializers.ModelSerializer):
    """Serializer for driver profile changes"""
    change_type_display = serializers.CharField(source='get_change_type_display', read_only=True)
    
    class Meta:
        model = DriverProfileChange
        fields = [
            'id', 'field_name', 'old_value', 'new_value', 
            'change_type', 'change_type_display', 'created_at'
        ]
        read_only_fields = ['id', 'created_at']


class DriverProfileSerializer(serializers.ModelSerializer):
    """Enhanced driver profile serializer with approval workflow"""
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
        
        # Handle license_classes conversion (from list of strings to list of LicenseClass objects)
        if 'license_classes' in data:
            license_classes_data = data.get('license_classes')
            logger.error(f"Raw license_classes data: {repr(license_classes_data)}, type: {type(license_classes_data)}")
            
            if isinstance(license_classes_data, (list, tuple)):
                # Convert string UUIDs to LicenseClass objects
                license_class_objects = []
                for lc_id in license_classes_data:
                    if isinstance(lc_id, str):
                        try:
                            lc_obj = LicenseClass.objects.get(id=lc_id)
                            license_class_objects.append(lc_obj)
                            logger.error(f"Found LicenseClass: {lc_obj.name} ({lc_obj.id})")
                        except LicenseClass.DoesNotExist:
                            logger.error(f"LicenseClass with id {lc_id} not found")
                    elif hasattr(lc_id, 'id'):
                        # It's already a LicenseClass object
                        license_class_objects.append(lc_id)
                
                # Update the data with the converted objects
                data['license_classes'] = license_class_objects
                logger.error(f"Converted license_classes to objects: {[lc.name for lc in license_class_objects]}")
        
        return super().to_internal_value(data)
    
    def update(self, instance, validated_data):
        """Custom update method to handle license classes"""
        import logging
        logger = logging.getLogger(__name__)
        
        # Extract license_classes from validated_data
        license_classes_data = validated_data.pop('license_classes', None)
        
        # Update other fields
        for field, value in validated_data.items():
            setattr(instance, field, value)
        
        # Save the instance
        instance.save()
        
        # Handle license_classes separately after saving
        if license_classes_data is not None:
            logger.error(f"Setting license_classes: {[lc.name if hasattr(lc, 'name') else str(lc) for lc in license_classes_data]}")
            instance.license_classes.set(license_classes_data)
        
        return instance
    
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