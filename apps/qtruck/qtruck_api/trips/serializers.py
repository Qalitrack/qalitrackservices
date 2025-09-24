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
    
    class Meta:
        model = Trip
        fields = '__all__'
        read_only_fields = ('total_cost', 'total_mileage')
    
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


class VehicleMileageSerializer(serializers.ModelSerializer):
    truck = TruckSerializer(read_only=True)
    driver = DriverSerializer(read_only=True)
    truck_id = serializers.UUIDField(write_only=True)
    driver_id = serializers.UUIDField(write_only=True)
    
    class Meta:
        model = VehicleMileage
        fields = '__all__'
        read_only_fields = ('mileage',)