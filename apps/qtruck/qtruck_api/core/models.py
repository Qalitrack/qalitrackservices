import uuid
from django.db import models
from django.contrib.auth.models import AbstractUser
from django.contrib.gis.db import models as gis_models
from django.db.models.signals import post_save, post_delete
from django.dispatch import receiver
from django.core.validators import FileExtensionValidator
from django.utils import timezone
from datetime import timedelta


class BaseModel(models.Model):
    id = models.UUIDField(primary_key=True, default=uuid.uuid4, editable=False)
    created_at = models.DateTimeField(auto_now_add=True)
    updated_at = models.DateTimeField(auto_now=True)

    class Meta:
        abstract = True


class CustomUser(AbstractUser):
    USER_TYPE_CHOICES = [
        ('admin', 'Admin'),
        ('driver', 'Driver'),
        ('tester', 'Tester'),
    ]
    
    id = models.UUIDField(primary_key=True, default=uuid.uuid4, editable=False)
    user_type = models.CharField(max_length=10, choices=USER_TYPE_CHOICES, default='driver')
    is_approved = models.BooleanField(default=False)
    created_at = models.DateTimeField(auto_now_add=True)
    updated_at = models.DateTimeField(auto_now=True)
    
    @property
    def base_email(self):
        """Extract base email without alias from the email field"""
        if '@' in self.email and '+' in self.email.split('@')[0]:
            email_parts = self.email.split('@')
            local_part = email_parts[0].split('+')[0]
            return f"{local_part}@{email_parts[1]}"
        return self.email

    def save(self, *args, **kwargs):
        # Validate that email contains + alias
        if not '+' in self.email:
            from django.core.exceptions import ValidationError
            raise ValidationError("Email must contain an alias (e.g., user+admin@example.com or user+driver@example.com)")
        
        # Extract user type from email alias and set base_email
        email_parts = self.email.split('@')
        if '+' in email_parts[0]:
            local_part = email_parts[0].split('+')
            base_part = local_part[0]
            alias_part = local_part[1].lower()
            
            
            # Set user_type based on alias
            if alias_part == 'admin':
                self.user_type = 'admin'
            elif alias_part == 'driver':
                self.user_type = 'driver'
            elif alias_part == 'tester':
                self.user_type = 'tester'
            else:
                from django.core.exceptions import ValidationError
                raise ValidationError("Email alias must be 'admin', 'driver', or 'tester' (e.g., user+admin@example.com)")
        
        # First user becomes admin and is auto-approved
        if not CustomUser.objects.exists():
            self.user_type = 'admin'
            self.is_approved = True
            self.is_staff = True
            self.is_superuser = True
        
        super().save(*args, **kwargs)


class UserProfile(BaseModel):
    user = models.OneToOneField(CustomUser, on_delete=models.CASCADE, related_name='profile')
    approval_date = models.DateTimeField(null=True, blank=True)
    approved_by = models.ForeignKey(CustomUser, on_delete=models.SET_NULL, null=True, blank=True, related_name='approved_users')


class LicenseClass(BaseModel):
    name = models.CharField(max_length=100, unique=True, help_text="License class name (e.g., Class A CDL, Class B CDL, Regular License)")
    description = models.TextField(blank=True, help_text="Description of what this license allows")
    
    class Meta:
        ordering = ['name']
        verbose_name = "License Class"
        verbose_name_plural = "License Classes"
    
    def __str__(self):
        return self.name


class SystemSettings(BaseModel):
    tester_registration_enabled = models.BooleanField(default=True)
    tester_login_enabled = models.BooleanField(default=True)
    
    # Driver Profile Settings
    license_expiry_warning_days = models.PositiveIntegerField(
        default=30,
        help_text="Number of days before license expiry to show warning"
    )
    require_profile_photo = models.BooleanField(default=True)
    require_license_images = models.BooleanField(default=True)
    require_id_images = models.BooleanField(default=True)
    auto_approve_profile_updates = models.BooleanField(default=False)

    class Meta:
        verbose_name_plural = "System Settings"

    @classmethod
    def get_settings(cls):
        settings, created = cls.objects.get_or_create(pk=1)
        if created:
            # Create default license classes on first setup
            cls._create_default_license_classes()
        return settings
    
    @staticmethod
    def _create_default_license_classes():
        """Create default license classes"""
        default_classes = [
            ('Class A CDL', 'Heavy trucks, tractor-trailers'),
            ('Class B CDL', 'Large trucks, buses'),
            ('Class C License', 'Regular passenger vehicles'),
            ('Motorcycle License', 'Motorcycles'),
            ('Regular License', 'Standard passenger vehicles'),
            ('Commercial License', 'Commercial driving'),
        ]
        for name, desc in default_classes:
            LicenseClass.objects.get_or_create(
                name=name,
                defaults={'description': desc}
            )


class Driver(BaseModel):
    user = models.OneToOneField(CustomUser, on_delete=models.CASCADE, related_name='driver_profile')
    name = models.CharField(max_length=255)
    phone = models.CharField(max_length=20, null=True, blank=True)
    license_number = models.CharField(max_length=50, null=True, blank=True)

    def __str__(self):
        return self.name
    
    @property
    def current_profile(self):
        """Get the current approved driver profile"""
        return self.profile_versions.filter(is_current=True).first()
    
    @property
    def pending_profile(self):
        """Get pending profile if any"""
        return self.profile_versions.filter(status='pending').first()
    
    @property
    def latest_activity(self):
        """Get the latest activity"""
        return self.activities.first()
    
    def get_activity_stats(self, days=30):
        """Get activity statistics for the last N days"""
        return get_driver_activity_stats(self, days)
    
    def get_activity_heatmap(self, year=None):
        """Get activity heatmap data"""
        return get_driver_activity_heatmap(self, year)


# Import driver models from driver_models.py to avoid duplication
from .driver_models import (
    DriverActivityType,
    DriverActivity,
    DriverProfileStatus, 
    DriverProfile,
    DriverProfileChange
)


class Truck(BaseModel):
    license_plate = models.CharField(max_length=20, unique=True)
    model = models.CharField(max_length=100, null=True, blank=True)
    driver = models.ForeignKey(Driver, on_delete=models.SET_NULL, null=True, blank=True, related_name='trucks')

    def __str__(self):
        return self.license_plate


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
    start_mileage = models.FloatField(null=True, blank=True)
    end_mileage = models.FloatField(null=True, blank=True)
    proof_image = models.ImageField(upload_to='trip_images/', null=True, blank=True)
    proof_end_image = models.ImageField(upload_to='trip_images/', null=True, blank=True)
    total_mileage = models.FloatField(null=True, blank=True)
    status = models.CharField(max_length=20, choices=STATUS_CHOICES, default='pending')
    date = models.DateTimeField()
    total_cost = models.DecimalField(max_digits=10, decimal_places=2, default=0.00)

    def calculate_total_cost(self):
        expense_costs = sum(expense.amount for expense in self.expenses.all())
        self.total_cost = expense_costs
        return self.total_cost

    def save(self, *args, **kwargs):
        if self.start_mileage and self.end_mileage:
            self.total_mileage = self.end_mileage - self.start_mileage
        super().save(*args, **kwargs)

    def __str__(self):
        return f"Trip {self.id} - {self.truck.license_plate}"


class Material(BaseModel):
    name = models.CharField(max_length=255)
    description = models.TextField(null=True, blank=True)
    location = gis_models.PointField(null=True, blank=True, help_text="Geographic location where this material was recorded")

    def __str__(self):
        return self.name


class MaterialPhoto(BaseModel):
    material = models.ForeignKey(Material, on_delete=models.CASCADE, related_name='photos')
    photo = models.ImageField(upload_to='material_photos/')
    caption = models.CharField(max_length=255, null=True, blank=True)

    def __str__(self):
        return f"Photo for {self.material.name}"


class MaterialCost(BaseModel):
    material = models.ForeignKey(Material, on_delete=models.CASCADE, related_name='material_costs')
    cost = models.DecimalField(max_digits=10, decimal_places=2)
    location = models.CharField(max_length=255, null=True, blank=True)
    user_id = models.CharField(max_length=255, null=True, blank=True)
    synced = models.BooleanField(default=False)

    def __str__(self):
        return f"Cost for {self.material.name}: ${self.cost}"


class Expense(BaseModel):
    trip = models.ForeignKey(Trip, on_delete=models.CASCADE, related_name='expenses')
    description = models.CharField(max_length=255)
    amount = models.DecimalField(max_digits=10, decimal_places=2)
    user_id = models.CharField(max_length=255, null=True, blank=True)
    synced = models.BooleanField(default=False)
    
    @property
    def driver(self):
        return self.trip.driver if self.trip else None

    def __str__(self):
        return f"{self.description}: ${self.amount}"


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


class Feedback(BaseModel):
    FEEDBACK_TYPE_CHOICES = [
        ('bug_report', 'Bug Report'),
        ('feature_request', 'Feature Request'),
        ('general', 'General Feedback'),
        ('complaint', 'Complaint'),
        ('suggestion', 'Suggestion'),
    ]
    
    STATUS_CHOICES = [
        ('pending', 'Pending'),
        ('reviewed', 'Reviewed'),
        ('in_progress', 'In Progress'),
        ('resolved', 'Resolved'),
        ('rejected', 'Rejected'),
    ]
    
    user = models.ForeignKey(CustomUser, on_delete=models.CASCADE, related_name='feedbacks')
    feedback_type = models.CharField(max_length=20, choices=FEEDBACK_TYPE_CHOICES, default='general')
    subject = models.CharField(max_length=255)
    description = models.TextField()
    status = models.CharField(max_length=20, choices=STATUS_CHOICES, default='pending')
    admin_response = models.TextField(null=True, blank=True)
    responded_by = models.ForeignKey(CustomUser, on_delete=models.SET_NULL, null=True, blank=True, related_name='feedback_responses')
    response_date = models.DateTimeField(null=True, blank=True)

    def __str__(self):
        return f"{self.feedback_type}: {self.subject} by {self.user.username}"

    class Meta:
        ordering = ['-created_at']


# Signals for automatic trip cost calculation
# Note: MaterialCost signals removed since materials are no longer trip-specific


@receiver([post_save, post_delete], sender=Expense)
def update_trip_cost_on_expense_change(sender, instance, **kwargs):
    if instance.trip:
        trip = instance.trip
        trip.calculate_total_cost()
        trip.save()


# Auto-create user profile
@receiver(post_save, sender=CustomUser)
def create_user_profile(sender, instance, created, **kwargs):
    if created:
        UserProfile.objects.create(user=instance)


# Auto-create driver object for driver users
@receiver(post_save, sender=CustomUser)
def create_driver_object(sender, instance, created, **kwargs):
    if created and instance.user_type == 'driver':
        Driver.objects.create(
            user=instance,
            name=f"{instance.first_name} {instance.last_name}".strip() or instance.username,
            phone='',
            license_number=''
        )


# Helper functions are now imported from driver_models.py
from .driver_models import (
    log_driver_activity,
    get_driver_activity_heatmap,
    get_driver_activity_stats
)


# Signal to track profile changes
@receiver(post_save, sender=DriverProfile)
def track_profile_changes(sender, instance, created, **kwargs):
    """Track changes between driver profile versions"""
    if not created and instance.version_number > 1:
        # Get previous version
        previous_profile = DriverProfile.objects.filter(
            driver=instance.driver,
            version_number=instance.version_number - 1
        ).first()
        
        if previous_profile:
            # Compare fields and track changes
            fields_to_track = [
                'full_name', 'phone_number', 'id_number', 
                'license_number', 'license_expiry_date', 'license_class'
            ]
            
            for field in fields_to_track:
                old_value = getattr(previous_profile, field, None)
                new_value = getattr(instance, field, None)
                
                if old_value != new_value:
                    DriverProfileChange.objects.create(
                        old_profile=previous_profile,
                        new_profile=instance,
                        field_name=field,
                        old_value=str(old_value) if old_value else '',
                        new_value=str(new_value) if new_value else '',
                        change_type='modified' if old_value else 'added'
                    )