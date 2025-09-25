import uuid
from django.db import models
from django.contrib.gis.db import models as gis_models
from django.core.validators import FileExtensionValidator

# Import from users app
from users.models import BaseModel
# Import Driver model from drivers app
from drivers.models import Driver


class Truck(BaseModel):
    license_plate = models.CharField(max_length=20, unique=True)
    model = models.CharField(max_length=100, null=True, blank=True)
    driver = models.ForeignKey(Driver, on_delete=models.SET_NULL, null=True, blank=True, related_name='trucks')

    def __str__(self):
        return self.license_plate


class Material(BaseModel):
    name = models.CharField(max_length=255, help_text="Base material name (e.g., Sand, Stone, Cement)")
    description = models.TextField(null=True, blank=True)

    def __str__(self):
        return self.name


class MaterialVariant(BaseModel):
    material = models.ForeignKey(Material, on_delete=models.CASCADE, related_name='variants')
    name = models.CharField(max_length=255, help_text="Variant name (e.g., Darugo, Kajido, River Sand)")
    description = models.TextField(null=True, blank=True)
    
    class Meta:
        unique_together = ['material', 'name']
    
    def __str__(self):
        return f"{self.material.name} - {self.name}"


class MaterialPhoto(BaseModel):
    material = models.ForeignKey(Material, on_delete=models.CASCADE, related_name='photos')
    photo = models.ImageField(upload_to='material_photos/')
    caption = models.CharField(max_length=255, null=True, blank=True)

    def __str__(self):
        return f"Photo for {self.material.name}"


class MaterialVariantPhoto(BaseModel):
    material_variant = models.ForeignKey(MaterialVariant, on_delete=models.CASCADE, related_name='photos')
    photo = models.ImageField(upload_to='material_variant_photos/')
    caption = models.CharField(max_length=255, null=True, blank=True)

    def __str__(self):
        return f"Photo for {self.material_variant.name}"


class MaterialCost(BaseModel):
    material = models.ForeignKey(Material, on_delete=models.CASCADE, related_name='material_costs')
    cost = models.DecimalField(max_digits=10, decimal_places=2)
    location = models.CharField(max_length=255, null=True, blank=True)
    user_id = models.CharField(max_length=255, null=True, blank=True)
    synced = models.BooleanField(default=False)

    def __str__(self):
        return f"Cost for {self.material.name}: ${self.cost}"