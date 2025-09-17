from rest_framework import serializers
from django.contrib.auth.password_validation import validate_password
from django.core.exceptions import ValidationError as DjangoValidationError
from .models import (
    CustomUser, UserProfile, SystemSettings, Driver, Truck, Trip,
    Material, MaterialCost, Expense, Receipt, VehicleMileage, Feedback
)


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
        
        # Check if user with same base_email and user_type already exists
        if CustomUser.objects.filter(base_email=base_email, user_type=user_type).exists():
            raise serializers.ValidationError(f'User with this email and role ({user_type}) already exists.')
        
        attrs['user_type'] = user_type
        attrs['base_email'] = base_email
        
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
            user_type=validated_data['user_type'],
            base_email=validated_data['base_email']
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
                 'user_type', 'is_approved', 'created_at', 'updated_at', 'profile')
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


class MaterialSerializer(serializers.ModelSerializer):
    material_costs = MaterialCostSerializer(many=True, read_only=True)
    
    class Meta:
        model = Material
        fields = '__all__'


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