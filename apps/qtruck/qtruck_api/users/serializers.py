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
    
    class Meta:
        model = CustomUser
        fields = ('id', 'email', 'base_email', 'first_name', 'last_name', 'full_name',
                 'user_type', 'status', 'is_active', 'profile', 'created_at', 'updated_at')
        read_only_fields = ('id', 'created_at', 'updated_at', 'full_name', 'profile')


