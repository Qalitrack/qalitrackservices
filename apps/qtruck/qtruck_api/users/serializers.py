from rest_framework import serializers
from .models import CustomUser, UserProfile


class UserProfileSerializer(serializers.ModelSerializer):
    class Meta:
        model = UserProfile
        fields = ('approval_date', 'approved_by')
        read_only_fields = ('approval_date', 'approved_by')


class CustomUserSerializer(serializers.ModelSerializer):
    full_name = serializers.ReadOnlyField()
    profile = UserProfileSerializer(read_only=True)
    password = serializers.CharField(write_only=True, min_length=8)
    
    class Meta:
        model = CustomUser
        fields = ('id', 'email', 'base_email', 'first_name', 'last_name', 'full_name',
                 'user_type', 'status', 'is_active', 'profile', 'created_at', 'updated_at', 'password')
        read_only_fields = ('id', 'created_at', 'updated_at', 'full_name', 'profile')
        extra_kwargs = {
            'password': {'write_only': True}
        }
    
    def create(self, validated_data):
        """Create user with properly hashed password"""
        password = validated_data.pop('password')
        user = CustomUser.objects.create_user(
            password=password,
            **validated_data
        )
        return user


