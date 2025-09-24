from rest_framework import serializers
from .models import CustomUser, UserProfile


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