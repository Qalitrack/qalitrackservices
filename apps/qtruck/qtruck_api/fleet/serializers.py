from rest_framework import serializers
from .models import Truck, Material, MaterialPhoto, MaterialCost
from drivers.serializers import DriverSerializer


class TruckSerializer(serializers.ModelSerializer):
    driver = DriverSerializer(read_only=True)
    driver_id = serializers.UUIDField(write_only=True, required=False, allow_null=True)
    
    class Meta:
        model = Truck
        fields = '__all__'


class MaterialPhotoSerializer(serializers.ModelSerializer):
    class Meta:
        model = MaterialPhoto
        fields = '__all__'


class MaterialSerializer(serializers.ModelSerializer):
    photos = MaterialPhotoSerializer(many=True, read_only=True)
    
    class Meta:
        model = Material
        fields = ['id', 'name', 'description', 'location', 'photos', 'created_at', 'updated_at']
    
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


class MaterialCostSerializer(serializers.ModelSerializer):
    class Meta:
        model = MaterialCost
        fields = '__all__'