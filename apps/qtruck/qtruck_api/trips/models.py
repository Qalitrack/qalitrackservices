import uuid
from django.db import models
from django.contrib.gis.db import models as gis_models
from django.db.models.signals import post_save, post_delete
from django.dispatch import receiver
from django.core.validators import FileExtensionValidator
from django.core.exceptions import ValidationError
from django.utils import timezone

# Import from users app
from users.models import BaseModel
# Import related models from other apps
from drivers.models import Driver
from fleet.models import Truck, Material, MaterialVariant


class Trip(BaseModel):
    STATUS_CHOICES = [
        ('pending', 'Pending'),
        ('in_progress', 'In Progress'),
        ('completed', 'Completed'),
        ('cancelled', 'Cancelled'),
    ]
    
    truck = models.ForeignKey(Truck, on_delete=models.CASCADE, related_name='trips')
    driver = models.ForeignKey(Driver, on_delete=models.CASCADE, related_name='trips')
    start_location = models.CharField(max_length=255, null=True, blank=True)
    end_location = models.CharField(max_length=255, null=True, blank=True)
    start_location_coords = gis_models.PointField(null=True, blank=True, help_text="Geographic coordinates of start location")
    end_location_coords = gis_models.PointField(null=True, blank=True, help_text="Geographic coordinates of end location")
    start_mileage = models.FloatField(null=True, blank=True, help_text="Set when trip is started, not during creation")
    end_mileage = models.FloatField(null=True, blank=True, help_text="Set when trip is completed")
    proof_image = models.ImageField(upload_to='trip_images/', null=True, blank=True, help_text="Start mileage photo - uploaded when starting trip")
    proof_end_image = models.ImageField(upload_to='trip_images/', null=True, blank=True, help_text="End mileage photo - uploaded when ending trip")
    total_mileage = models.FloatField(null=True, blank=True)
    status = models.CharField(max_length=20, choices=STATUS_CHOICES, default='pending')
    date = models.DateTimeField(default=timezone.now)
    total_cost = models.DecimalField(max_digits=10, decimal_places=2, default=0.00)
    
    # Material loading confirmation photos (different from start/end proof images)
    material_loading_photos = models.JSONField(default=list, blank=True, help_text="URLs to material loading confirmation photos")
    
    # Current location when trip starts (for verification)
    current_location_coords = gis_models.PointField(null=True, blank=True, help_text="Current GPS coordinates when trip starts")
    
    # Material fields
    material = models.ForeignKey(Material, on_delete=models.SET_NULL, null=True, blank=True, help_text="Type of material transported")
    material_variant = models.ForeignKey(MaterialVariant, on_delete=models.SET_NULL, null=True, blank=True, help_text="Specific variant of the material")
    material_cost = models.DecimalField(max_digits=10, decimal_places=2, null=True, blank=True, help_text="Cost to purchase the material for this trip")

    def clean(self):
        # Validate that material_variant belongs to the selected material
        if self.material_variant and self.material:
            if self.material_variant.material != self.material:
                raise ValidationError('Material variant must belong to the selected material.')
        elif self.material_variant and not self.material:
            raise ValidationError('Material must be selected when specifying a material variant.')

    def calculate_total_cost(self):
        expense_costs = sum(expense.amount for expense in self.expenses.all())
        material_cost = self.material_cost or 0
        self.total_cost = expense_costs + material_cost
        return self.total_cost

    def save(self, *args, **kwargs):
        if self.start_mileage and self.end_mileage:
            self.total_mileage = self.end_mileage - self.start_mileage
        self.full_clean()  # Run validation before saving
        super().save(*args, **kwargs)

    def __str__(self):
        return f"Trip {self.id} - {self.truck.license_plate}"


class Expense(BaseModel):
    trip = models.ForeignKey(Trip, on_delete=models.CASCADE, related_name='expenses')
    description = models.CharField(max_length=255)
    amount = models.DecimalField(max_digits=10, decimal_places=2, help_text="Amount in KSh")
    receipt_photo = models.ImageField(upload_to='expense_receipts/', null=True, blank=True, help_text="Optional receipt photo")
    user_id = models.CharField(max_length=255, null=True, blank=True)
    synced = models.BooleanField(default=False)
    
    @property
    def driver(self):
        return self.trip.driver if self.trip else None

    def __str__(self):
        return f"{self.description}: KSh {self.amount}"


class Receipt(BaseModel):
    expense = models.ForeignKey(Expense, on_delete=models.CASCADE, related_name='receipts')
    image = models.ImageField(upload_to='receipts/')
    note = models.TextField(null=True, blank=True)
    receipt_details = models.JSONField(default=dict, blank=True)
    user_id = models.CharField(max_length=255, null=True, blank=True)
    synced = models.BooleanField(default=False)

    def __str__(self):
        return f"Receipt for {self.expense.description}"


class VehicleMileage(BaseModel):
    truck = models.ForeignKey(Truck, on_delete=models.CASCADE, related_name='mileage_records')
    driver = models.ForeignKey(Driver, on_delete=models.CASCADE, related_name='mileage_records')
    start_mileage = models.FloatField()
    end_mileage = models.FloatField()
    mileage = models.FloatField(null=True, blank=True)
    proof_image = models.ImageField(upload_to='mileage_images/', null=True, blank=True)
    proof_end_image = models.ImageField(upload_to='mileage_images/', null=True, blank=True)
    date = models.DateTimeField()
    user_id = models.CharField(max_length=255, null=True, blank=True)
    synced = models.BooleanField(default=False)

    def save(self, *args, **kwargs):
        if self.start_mileage and self.end_mileage:
            self.mileage = self.end_mileage - self.start_mileage
        super().save(*args, **kwargs)

    def __str__(self):
        return f"Mileage for {self.truck.license_plate}: {self.mileage}"


# Signals for automatic trip cost calculation
@receiver([post_save, post_delete], sender=Expense)
def update_trip_cost_on_expense_change(sender, instance, **kwargs):
    if instance.trip:
        trip = instance.trip
        trip.calculate_total_cost()
        trip.save()