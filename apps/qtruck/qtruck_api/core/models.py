import uuid
from django.db import models
from django.contrib.auth.models import AbstractUser
from django.db.models.signals import post_save, post_delete
from django.dispatch import receiver


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
    base_email = models.EmailField(unique=True)
    user_type = models.CharField(max_length=10, choices=USER_TYPE_CHOICES, default='driver')
    is_approved = models.BooleanField(default=False)
    created_at = models.DateTimeField(auto_now_add=True)
    updated_at = models.DateTimeField(auto_now=True)

    def save(self, *args, **kwargs):
        if not self.base_email:
            email_parts = self.email.split('@')
            if '+' in email_parts[0]:
                base_part = email_parts[0].split('+')[0]
                self.base_email = f"{base_part}@{email_parts[1]}"
            else:
                self.base_email = self.email
        
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


class SystemSettings(BaseModel):
    tester_registration_enabled = models.BooleanField(default=True)
    tester_login_enabled = models.BooleanField(default=True)

    class Meta:
        verbose_name_plural = "System Settings"

    @classmethod
    def get_settings(cls):
        settings, created = cls.objects.get_or_create(pk=1)
        return settings


class Driver(BaseModel):
    user = models.OneToOneField(CustomUser, on_delete=models.CASCADE, related_name='driver_profile')
    name = models.CharField(max_length=255)
    phone = models.CharField(max_length=20, null=True, blank=True)
    license_number = models.CharField(max_length=50, null=True, blank=True)

    def __str__(self):
        return self.name


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
    start_mileage = models.FloatField(null=True, blank=True)
    end_mileage = models.FloatField(null=True, blank=True)
    proof_image = models.ImageField(upload_to='trip_images/', null=True, blank=True)
    proof_end_image = models.ImageField(upload_to='trip_images/', null=True, blank=True)
    total_mileage = models.FloatField(null=True, blank=True)
    status = models.CharField(max_length=20, choices=STATUS_CHOICES, default='pending')
    date = models.DateTimeField()
    total_cost = models.DecimalField(max_digits=10, decimal_places=2, default=0.00)

    def calculate_total_cost(self):
        material_costs = sum(mc.cost for material in self.materials.all() for mc in material.material_costs.all())
        expense_costs = sum(expense.amount for expense in self.expenses.all())
        self.total_cost = material_costs + expense_costs
        return self.total_cost

    def save(self, *args, **kwargs):
        if self.start_mileage and self.end_mileage:
            self.total_mileage = self.end_mileage - self.start_mileage
        super().save(*args, **kwargs)

    def __str__(self):
        return f"Trip {self.id} - {self.truck.license_plate}"


class Material(BaseModel):
    trip = models.ForeignKey(Trip, on_delete=models.CASCADE, related_name='materials', null=True, blank=True)
    name = models.CharField(max_length=255)
    quantity = models.FloatField(null=True, blank=True)

    def __str__(self):
        return f"{self.name} - {self.trip.id if self.trip else 'Base Material'}"


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
@receiver([post_save, post_delete], sender=MaterialCost)
def update_trip_cost_on_material_cost_change(sender, instance, **kwargs):
    if instance.material and instance.material.trip:
        trip = instance.material.trip
        trip.calculate_total_cost()
        trip.save()


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