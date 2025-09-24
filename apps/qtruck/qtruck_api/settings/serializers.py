from rest_framework import serializers
from .models import SystemSettings, LicenseClass


class SystemSettingsSerializer(serializers.ModelSerializer):
    class Meta:
        model = SystemSettings
        fields = '__all__'


class LicenseClassSerializer(serializers.ModelSerializer):
    """Serializer for License Class configuration"""
    
    class Meta:
        model = LicenseClass
        fields = ['id', 'name', 'description']
        read_only_fields = ['id']