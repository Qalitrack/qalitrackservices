from rest_framework import serializers
from .models import Truck, Material, MaterialVariant, MaterialPhoto, MaterialVariantPhoto, MaterialCost
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


class MaterialVariantPhotoSerializer(serializers.ModelSerializer):
    class Meta:
        model = MaterialVariantPhoto
        fields = '__all__'


class MaterialVariantSerializer(serializers.ModelSerializer):
    material_name = serializers.CharField(source='material.name', read_only=True)
    photos = MaterialVariantPhotoSerializer(many=True, read_only=True)
    
    class Meta:
        model = MaterialVariant
        fields = ['id', 'material', 'material_name', 'name', 'description', 'photos', 'created_at', 'updated_at']
    
    def create(self, validated_data):
        # Create the material variant instance
        variant = MaterialVariant.objects.create(**validated_data)
        
        # Handle photo uploads from request.FILES
        request = self.context.get('request')
        if request and 'photos' in request.FILES:
            photos = request.FILES.getlist('photos')
            for photo in photos:
                MaterialVariantPhoto.objects.create(material_variant=variant, photo=photo)
        
        return variant
    
    def update(self, instance, validated_data):
        # Update the material variant instance
        instance = super().update(instance, validated_data)
        
        # Handle photo uploads from request.FILES
        request = self.context.get('request')
        if request and 'photos' in request.FILES:
            # Optionally clear existing photos if new ones are provided
            # instance.photos.all().delete()  # Uncomment if you want to replace all photos
            
            photos = request.FILES.getlist('photos')
            for photo in photos:
                MaterialVariantPhoto.objects.create(material_variant=instance, photo=photo)
        
        return instance


class MaterialSerializer(serializers.ModelSerializer):
    variants = MaterialVariantSerializer(many=True, read_only=True)
    photos = MaterialPhotoSerializer(many=True, read_only=True)
    
    class Meta:
        model = Material
        fields = ['id', 'name', 'description', 'variants', 'photos', 'created_at', 'updated_at']
    
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
    
    def update(self, instance, validated_data):
        # Update the material instance
        instance = super().update(instance, validated_data)
        
        # Handle photo uploads from request.FILES
        request = self.context.get('request')
        if request and 'photos' in request.FILES:
            # Optionally clear existing photos if new ones are provided
            # instance.photos.all().delete()  # Uncomment if you want to replace all photos
            
            photos = request.FILES.getlist('photos')
            for photo in photos:
                MaterialPhoto.objects.create(material=instance, photo=photo)
        
        return instance


class MaterialCostSerializer(serializers.ModelSerializer):
    class Meta:
        model = MaterialCost
        fields = '__all__'