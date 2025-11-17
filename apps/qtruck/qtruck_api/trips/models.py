import uuid
from django.db import models
from django.contrib.gis.db import models as gis_models
from django.db.models.signals import post_save, post_delete
from django.dispatch import receiver
from django.core.validators import FileExtensionValidator
from django.core.exceptions import ValidationError
from django.utils import timezone

# Import MinIO storage classes
from storage import TripImageStorage, ExpenseReceiptStorage, ReceiptStorage, MaterialPhotoStorage

# Import from users app
from users.models import BaseModel
# Import related models from other apps
from drivers.models import Driver
from fleet.models import Truck, Material, MaterialVariant


class TripType(BaseModel):
    """
    Model to store different types of trips
    Admin can manage these, drivers can read and use custom types
    """
    name = models.CharField(max_length=100, unique=True, help_text="Name of the trip type")
    description = models.TextField(blank=True, help_text="Description of what this trip type entails")
    is_active = models.BooleanField(default=True, help_text="Whether this trip type is available for selection")
    category = models.CharField(
        max_length=50,
        choices=[
            ('loaded', 'Loaded Trip'),
            ('empty', 'Empty Trip'),
            ('maintenance', 'Maintenance'),
            ('other', 'Other'),
        ],
        default='other',
        help_text="Category of the trip type"
    )

    # For empty trips, we can have predefined options
    empty_trip_option = models.CharField(
        max_length=100,
        blank=True,
        help_text="For empty trips: e.g., carwash, starting day, ending day"
    )

    # Material requirement settings
    material_requirement = models.CharField(
        max_length=20,
        choices=[
            ('none', 'No Materials'),
            ('optional', 'Optional Materials'),
            ('mandatory', 'Mandatory Materials'),
        ],
        default='none',
        help_text="Whether this trip type requires materials"
    )

    created_by = models.ForeignKey(
        'users.CustomUser',
        on_delete=models.SET_NULL,
        null=True,
        blank=True,
        related_name='created_trip_types',
        help_text="Admin user who created this trip type"
    )

    class Meta:
        ordering = ['category', 'name']
        verbose_name = "Trip Type"
        verbose_name_plural = "Trip Types"

    def __str__(self):
        return f"{self.get_category_display()}: {self.name}"


class Trip(BaseModel):
    STATUS_CHOICES = [
        ('pending', 'Pending'),
        ('in_progress', 'In Progress'),
        ('completed', 'Completed'),
        ('cancelled', 'Cancelled'),
    ]
    
    truck = models.ForeignKey(Truck, on_delete=models.CASCADE, related_name='trips')
    driver = models.ForeignKey(Driver, on_delete=models.CASCADE, related_name='trips')

    # Trip type selection
    trip_type = models.ForeignKey(
        TripType,
        on_delete=models.SET_NULL,
        null=True,
        blank=True,
        related_name='trips',
        help_text="Type of trip - can be predefined or custom"
    )
    custom_trip_type = models.CharField(
        max_length=100,
        blank=True,
        help_text="Custom trip type if predefined types don't apply"
    )

    # Location fields
    start_location = models.CharField(max_length=255, null=True, blank=True)
    end_location = models.CharField(max_length=255, null=True, blank=True)
    start_location_coords = gis_models.PointField(null=True, blank=True, help_text="Geographic coordinates of start location")
    end_location_coords = gis_models.PointField(null=True, blank=True, help_text="Geographic coordinates of end location")

    # Truck location (where the truck currently is)
    truck_location_coords = gis_models.PointField(
        null=True,
        blank=True,
        help_text="Current GPS coordinates of the truck when trip is created"
    )
    start_mileage = models.FloatField(null=True, blank=True, help_text="Set when trip is started, not during creation")
    end_mileage = models.FloatField(null=True, blank=True, help_text="Set when trip is completed")
    proof_image = models.ImageField(upload_to='trip_images/', null=True, blank=True, help_text="Start mileage photo - uploaded when starting trip", storage=TripImageStorage)
    proof_end_image = models.ImageField(upload_to='trip_images/', null=True, blank=True, help_text="End mileage photo - uploaded when ending trip", storage=TripImageStorage)
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
        trip_material_costs = sum(material.total_cost for material in self.trip_materials.all())
        self.total_cost = expense_costs + material_cost + trip_material_costs
        return self.total_cost

    def save(self, *args, **kwargs):
        if self.start_mileage and self.end_mileage:
            self.total_mileage = self.end_mileage - self.start_mileage
        self.full_clean()  # Run validation before saving
        super().save(*args, **kwargs)

    def get_trip_type_display(self):
        """Get the display name for the trip type (either predefined or custom)"""
        if self.trip_type:
            return self.trip_type.name
        return self.custom_trip_type or 'Not specified'

    def __str__(self):
        trip_type_name = self.get_trip_type_display()
        return f"Trip {self.id} - {self.truck.license_plate} ({trip_type_name})"


class Expense(BaseModel):
    trip = models.ForeignKey(Trip, on_delete=models.CASCADE, related_name='expenses')
    description = models.CharField(max_length=255)
    amount = models.DecimalField(max_digits=10, decimal_places=2, help_text="Amount in KSh")
    receipt_photo = models.ImageField(upload_to='expense_receipts/', null=True, blank=True, help_text="Optional receipt photo", storage=ExpenseReceiptStorage)
    user_id = models.CharField(max_length=255, null=True, blank=True)
    synced = models.BooleanField(default=False)
    
    @property
    def driver(self):
        return self.trip.driver if self.trip else None

    def __str__(self):
        return f"{self.description}: KSh {self.amount}"


class Receipt(BaseModel):
    expense = models.ForeignKey(Expense, on_delete=models.CASCADE, related_name='receipts')
    image = models.ImageField(upload_to='receipts/', storage=ReceiptStorage)
    note = models.TextField(null=True, blank=True)
    receipt_details = models.JSONField(default=dict, blank=True)
    user_id = models.CharField(max_length=255, null=True, blank=True)
    synced = models.BooleanField(default=False)

    def __str__(self):
        return f"Receipt for {self.expense.description}"


class TripMaterial(BaseModel):
    """
    Track materials associated with a trip
    Allows adding materials during trip creation and while trip is in progress
    """
    trip = models.ForeignKey(Trip, on_delete=models.CASCADE, related_name='trip_materials')
    material = models.ForeignKey(Material, on_delete=models.CASCADE, related_name='trip_materials')
    material_variant = models.ForeignKey(
        MaterialVariant,
        on_delete=models.SET_NULL,
        null=True,
        blank=True,
        related_name='trip_materials',
        help_text="Specific variant of the material"
    )
    quantity = models.DecimalField(
        max_digits=10,
        decimal_places=2,
        null=True,
        blank=True,
        help_text="Quantity of material (if applicable)"
    )
    unit_cost = models.DecimalField(
        max_digits=10,
        decimal_places=2,
        null=True,
        blank=True,
        help_text="Cost per unit for this material in this trip"
    )
    total_cost = models.DecimalField(
        max_digits=10,
        decimal_places=2,
        default=0.00,
        help_text="Total cost for this material (quantity * unit_cost)"
    )
    added_at = models.DateTimeField(
        auto_now_add=True,
        help_text="When this material was added to the trip"
    )
    added_by = models.ForeignKey(
        'users.CustomUser',
        on_delete=models.SET_NULL,
        null=True,
        blank=True,
        help_text="User who added this material to the trip"
    )
    notes = models.TextField(
        blank=True,
        help_text="Additional notes about this material for the trip"
    )

    class Meta:
        ordering = ['added_at']
        unique_together = ['trip', 'material', 'material_variant']
        verbose_name = "Trip Material"
        verbose_name_plural = "Trip Materials"

    def save(self, *args, **kwargs):
        # Calculate total cost if quantity and unit cost are provided
        if self.quantity is not None and self.unit_cost is not None:
            self.total_cost = self.quantity * self.unit_cost
        super().save(*args, **kwargs)

    def __str__(self):
        material_name = f"{self.material.name}"
        if self.material_variant:
            material_name += f" - {self.material_variant.name}"
        return f"{self.trip.id}: {material_name}"


class VehicleMileage(BaseModel):
    truck = models.ForeignKey(Truck, on_delete=models.CASCADE, related_name='mileage_records')
    driver = models.ForeignKey(Driver, on_delete=models.CASCADE, related_name='mileage_records')
    start_mileage = models.FloatField()
    end_mileage = models.FloatField()
    mileage = models.FloatField(null=True, blank=True)
    proof_image = models.ImageField(upload_to='trip_images/', null=True, blank=True, storage=TripImageStorage)
    proof_end_image = models.ImageField(upload_to='trip_images/', null=True, blank=True, storage=TripImageStorage)
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

@receiver([post_save, post_delete], sender=TripMaterial)
def update_trip_cost_on_material_change(sender, instance, **kwargs):
    if instance.trip:
        trip = instance.trip
        trip.calculate_total_cost()
        trip.save()