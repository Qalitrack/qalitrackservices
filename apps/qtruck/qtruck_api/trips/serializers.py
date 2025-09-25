from rest_framework import serializers
from .models import Trip, Expense, Receipt, VehicleMileage
from fleet.serializers import TruckSerializer, MaterialSerializer, MaterialVariantSerializer
from drivers.serializers import DriverSerializer


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
    material = MaterialSerializer(read_only=True)
    material_variant = MaterialVariantSerializer(read_only=True)
    expenses = ExpenseSerializer(many=True, read_only=True)
    truck_id = serializers.UUIDField(write_only=True)
    driver_id = serializers.UUIDField(write_only=True)
    material_id = serializers.UUIDField(write_only=True, required=False, allow_null=True)
    material_variant_id = serializers.UUIDField(write_only=True, required=False, allow_null=True)
    
    # Photo upload fields
    proof_image_file = serializers.ImageField(write_only=True, required=False, allow_null=True)
    proof_end_image_file = serializers.ImageField(write_only=True, required=False, allow_null=True)
    material_loading_photos_files = serializers.ListField(
        child=serializers.ImageField(),
        write_only=True,
        required=False,
        allow_empty=True
    )
    
    class Meta:
        model = Trip
        fields = '__all__'
        read_only_fields = ('total_cost', 'total_mileage', 'date')
    
    def validate(self, data):
        # Validate that material_variant belongs to material if both are provided
        material_id = data.get('material_id')
        material_variant_id = data.get('material_variant_id')
        
        if material_variant_id and not material_id:
            raise serializers.ValidationError("Material must be selected when specifying a material variant.")
        
        if material_variant_id and material_id:
            from fleet.models import MaterialVariant
            try:
                variant = MaterialVariant.objects.get(id=material_variant_id)
                if str(variant.material.id) != str(material_id):
                    raise serializers.ValidationError("Material variant must belong to the selected material.")
            except MaterialVariant.DoesNotExist:
                raise serializers.ValidationError("Invalid material variant.")
        
        return data
    
    def create(self, validated_data):
        # Extract photo files
        proof_image_file = validated_data.pop('proof_image_file', None)
        proof_end_image_file = validated_data.pop('proof_end_image_file', None)
        material_loading_photos_files = validated_data.pop('material_loading_photos_files', [])
        
        # Get related objects
        truck_id = validated_data.pop('truck_id')
        driver_id = validated_data.pop('driver_id')
        material_id = validated_data.pop('material_id', None)
        material_variant_id = validated_data.pop('material_variant_id', None)
        
        from fleet.models import Truck, Material, MaterialVariant
        from drivers.models import Driver
        
        validated_data['truck'] = Truck.objects.get(id=truck_id)
        validated_data['driver'] = Driver.objects.get(id=driver_id)
        if material_id:
            validated_data['material'] = Material.objects.get(id=material_id)
        if material_variant_id:
            validated_data['material_variant'] = MaterialVariant.objects.get(id=material_variant_id)
        
        # Create trip instance
        trip = Trip.objects.create(**validated_data)
        
        # Handle photo uploads
        if proof_image_file:
            trip.proof_image = proof_image_file
        if proof_end_image_file:
            trip.proof_end_image = proof_end_image_file
            
        # Handle material loading photos (store as URLs in JSON array)
        if material_loading_photos_files:
            photo_urls = []
            # In a real implementation, you would upload these to a storage service
            # For now, we'll just reference them by filename
            for i, photo_file in enumerate(material_loading_photos_files):
                # This would be replaced with actual file upload logic
                photo_urls.append(f'/media/material_loading/{trip.id}_{i}.jpg')
            trip.material_loading_photos = photo_urls
            
        trip.save()
        return trip
    
    def update(self, instance, validated_data):
        # Extract photo files
        proof_image_file = validated_data.pop('proof_image_file', None)
        proof_end_image_file = validated_data.pop('proof_end_image_file', None)
        material_loading_photos_files = validated_data.pop('material_loading_photos_files', [])
        
        # Handle regular field updates
        for attr, value in validated_data.items():
            if attr in ['truck_id', 'driver_id', 'material_id', 'material_variant_id']:
                # Handle foreign key updates
                if attr == 'truck_id' and value:
                    from fleet.models import Truck
                    instance.truck = Truck.objects.get(id=value)
                elif attr == 'driver_id' and value:
                    from drivers.models import Driver
                    instance.driver = Driver.objects.get(id=value)
                elif attr == 'material_id' and value:
                    from fleet.models import Material
                    instance.material = Material.objects.get(id=value)
                elif attr == 'material_variant_id' and value:
                    from fleet.models import MaterialVariant
                    instance.material_variant = MaterialVariant.objects.get(id=value)
            else:
                setattr(instance, attr, value)
        
        # Handle photo updates
        if proof_image_file:
            instance.proof_image = proof_image_file
        if proof_end_image_file:
            instance.proof_end_image = proof_end_image_file
            
        # Handle material loading photos update
        if material_loading_photos_files:
            photo_urls = []
            for i, photo_file in enumerate(material_loading_photos_files):
                photo_urls.append(f'/media/material_loading/{instance.id}_{i}.jpg')
            instance.material_loading_photos = photo_urls
            
        instance.save()
        return instance


class VehicleMileageSerializer(serializers.ModelSerializer):
    truck = TruckSerializer(read_only=True)
    driver = DriverSerializer(read_only=True)
    truck_id = serializers.UUIDField(write_only=True)
    driver_id = serializers.UUIDField(write_only=True)
    
    class Meta:
        model = VehicleMileage
        fields = '__all__'
        read_only_fields = ('mileage',)