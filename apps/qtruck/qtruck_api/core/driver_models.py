"""
Enhanced Driver Profile Models with Activity Tracking and Approval System
"""
import uuid
from django.db import models
from django.contrib.auth import get_user_model
from django.core.validators import FileExtensionValidator
from django.utils import timezone
from datetime import timedelta

User = get_user_model()


class DriverActivityType(models.TextChoices):
    """Types of driver activities to track for heatmap and analytics"""
    LOGIN = 'login', 'Login'
    TRIP_CREATE = 'trip_create', 'Trip Created'
    TRIP_START = 'trip_start', 'Trip Started'
    TRIP_COMPLETE = 'trip_complete', 'Trip Completed'
    EXPENSE_ADD = 'expense_add', 'Expense Added'
    RECEIPT_UPLOAD = 'receipt_upload', 'Receipt Uploaded'
    PROFILE_UPDATE = 'profile_update', 'Profile Updated'
    MILEAGE_RECORD = 'mileage_record', 'Mileage Recorded'
    LOCATION_UPDATE = 'location_update', 'Location Updated'
    APP_OPEN = 'app_open', 'App Opened'
    DOCUMENT_UPLOAD = 'document_upload', 'Document Uploaded'


class DriverActivity(models.Model):
    """Track all driver activities for heatmap and analytics"""
    id = models.UUIDField(primary_key=True, default=uuid.uuid4, editable=False)
    driver = models.ForeignKey('Driver', on_delete=models.CASCADE, related_name='activities')
    activity_type = models.CharField(max_length=20, choices=DriverActivityType.choices)
    created_at = models.DateTimeField(auto_now_add=True)
    activity_data = models.JSONField(default=dict, blank=True)  # Store additional activity context
    ip_address = models.GenericIPAddressField(null=True, blank=True)
    user_agent = models.TextField(null=True, blank=True)
    
    class Meta:
        ordering = ['-created_at']
        indexes = [
            models.Index(fields=['driver', 'created_at']),
            models.Index(fields=['activity_type', 'created_at']),
        ]
    
    def __str__(self):
        return f"{self.driver.name} - {self.get_activity_type_display()} on {self.created_at.strftime('%Y-%m-%d %H:%M')}"


class DriverProfileStatus(models.TextChoices):
    """Driver profile approval status"""
    DRAFT = 'draft', 'Draft'
    PENDING = 'pending', 'Pending Approval'
    APPROVED = 'approved', 'Approved'
    REJECTED = 'rejected', 'Rejected'
    CHANGES_REQUESTED = 'changes_requested', 'Changes Requested'


class DriverProfile(models.Model):
    """Enhanced Driver Profile with approval workflow"""
    id = models.UUIDField(primary_key=True, default=uuid.uuid4, editable=False)
    driver = models.ForeignKey('Driver', on_delete=models.CASCADE, related_name='profile_versions')
    
    # Basic Information
    full_name = models.CharField(max_length=255)
    phone_number = models.CharField(max_length=20)
    id_number = models.CharField(max_length=50, help_text="National ID or equivalent")
    
    # License Information
    license_number = models.CharField(max_length=50)
    license_expiry_date = models.DateField()
    license_classes = models.ManyToManyField('LicenseClass', blank=True, help_text="Driver's license classes")
    
    # Document Uploads
    profile_photo = models.ImageField(
        upload_to='driver_profiles/photos/',
        validators=[FileExtensionValidator(allowed_extensions=['jpg', 'jpeg', 'png'])],
        null=True, blank=True
    )
    license_front_image = models.ImageField(
        upload_to='driver_profiles/licenses/',
        validators=[FileExtensionValidator(allowed_extensions=['jpg', 'jpeg', 'png', 'pdf'])],
        null=True, blank=True
    )
    license_back_image = models.ImageField(
        upload_to='driver_profiles/licenses/',
        validators=[FileExtensionValidator(allowed_extensions=['jpg', 'jpeg', 'png', 'pdf'])],
        null=True, blank=True
    )
    id_front_image = models.ImageField(
        upload_to='driver_profiles/ids/',
        validators=[FileExtensionValidator(allowed_extensions=['jpg', 'jpeg', 'png', 'pdf'])],
        null=True, blank=True
    )
    id_back_image = models.ImageField(
        upload_to='driver_profiles/ids/',
        validators=[FileExtensionValidator(allowed_extensions=['jpg', 'jpeg', 'png', 'pdf'])],
        null=True, blank=True
    )
    
    # Approval Information
    status = models.CharField(max_length=20, choices=DriverProfileStatus.choices, default=DriverProfileStatus.DRAFT)
    is_current = models.BooleanField(default=False)  # Only one current profile per driver
    version_number = models.PositiveIntegerField(default=1)
    
    # Approval workflow
    submitted_at = models.DateTimeField(null=True, blank=True)
    reviewed_at = models.DateTimeField(null=True, blank=True)
    reviewed_by = models.ForeignKey(User, on_delete=models.SET_NULL, null=True, blank=True, related_name='reviewed_driver_profiles')
    approval_notes = models.TextField(blank=True)
    rejection_reason = models.TextField(blank=True)
    
    # Timestamps
    created_at = models.DateTimeField(auto_now_add=True)
    updated_at = models.DateTimeField(auto_now=True)
    
    class Meta:
        ordering = ['-version_number', '-created_at']
        unique_together = ['driver', 'version_number']
        indexes = [
            models.Index(fields=['driver', 'is_current']),
            models.Index(fields=['status']),
            models.Index(fields=['license_expiry_date']),
        ]
    
    def __str__(self):
        return f"{self.full_name} - Profile v{self.version_number} ({self.get_status_display()})"
    
    @property
    def is_license_expired(self):
        """Check if license is expired"""
        return self.license_expiry_date < timezone.now().date()
    
    def is_license_expiring_soon(self, days_ahead=30):
        """Check if license is expiring within specified days"""
        warning_date = timezone.now().date() + timedelta(days=days_ahead)
        return self.license_expiry_date <= warning_date
    
    @property
    def days_until_license_expiry(self):
        """
        Calculate days until license expiry with smart warning threshold.
        Returns -1 if not expiring soon, or actual days if within warning threshold.
        """
        from .models import SystemSettings
        
        delta = self.license_expiry_date - timezone.now().date()
        actual_days = delta.days
        
        # Get the warning threshold from system settings
        settings = SystemSettings.get_settings()
        warning_threshold = settings.license_expiry_warning_days
        
        # Return actual days if within warning threshold, otherwise -1
        if actual_days <= warning_threshold:
            return actual_days
        else:
            return -1
    
    def submit_for_approval(self):
        """Submit profile for approval"""
        self.status = DriverProfileStatus.PENDING
        self.submitted_at = timezone.now()
        self.save()
    
    def approve(self, approved_by, notes=""):
        """Approve the profile"""
        self.status = DriverProfileStatus.APPROVED
        self.reviewed_by = approved_by
        self.reviewed_at = timezone.now()
        self.approval_notes = notes
        
        # Set as current profile and deactivate others
        DriverProfile.objects.filter(driver=self.driver, is_current=True).update(is_current=False)
        self.is_current = True
        self.save()
    
    def reject(self, rejected_by, reason):
        """Reject the profile"""
        self.status = DriverProfileStatus.REJECTED
        self.reviewed_by = rejected_by
        self.reviewed_at = timezone.now()
        self.rejection_reason = reason
        self.save()
    
    def request_changes(self, requested_by, notes):
        """Request changes to the profile"""
        self.status = DriverProfileStatus.CHANGES_REQUESTED
        self.reviewed_by = requested_by
        self.reviewed_at = timezone.now()
        self.approval_notes = notes
        self.save()
    
    def clean(self):
        """Validate the model before saving - with logging to debug IntegrityError"""
        import logging
        logger = logging.getLogger(__name__)
        
        logger.error(f"=== DriverProfile.clean() called ===")
        logger.error(f"Instance ID: {self.id}")
        logger.error(f"Driver ID: {self.driver_id}")
        logger.error(f"Version Number: {self.version_number}")
        logger.error(f"Status: {self.status}")
        logger.error(f"Is Current: {self.is_current}")
        logger.error(f"Full Name: {self.full_name}")
        logger.error(f"Phone: {self.phone_number}")
        logger.error(f"License Number: {self.license_number}")
        logger.error(f"License Expiry: {self.license_expiry_date}")
        
        # Check if there's already a profile with this driver_id and version_number
        if self.driver_id and self.version_number:
            existing_profiles = DriverProfile.objects.filter(
                driver_id=self.driver_id,
                version_number=self.version_number
            ).exclude(id=self.id)  # Exclude self when updating
            
            logger.error(f"Checking for existing profiles with driver_id={self.driver_id}, version_number={self.version_number}")
            logger.error(f"Found {existing_profiles.count()} existing profiles")
            
            for profile in existing_profiles:
                logger.error(f"Existing profile: ID={profile.id}, driver_id={profile.driver_id}, version={profile.version_number}, status={profile.status}")
        
        # Call parent clean
        super().clean()
    
    def save(self, *args, **kwargs):
        """Custom save method with logging and automatic version number assignment"""
        import logging
        logger = logging.getLogger(__name__)
        
        # Check if this is a new instance
        is_new = self._state.adding
        
        # Auto-assign version number for new instances
        if is_new and self.driver_id:
            from django.db import models as django_models
            # Get the maximum version number for this driver
            max_version = DriverProfile.objects.filter(
                driver=self.driver
            ).aggregate(max_version=django_models.Max('version_number'))['max_version']
            
            # Set the new version number
            new_version = (max_version or 0) + 1
            self.version_number = new_version
            
            logger.error(f"Auto-assigned version number {new_version} for new DriverProfile")
        
        logger.error(f"=== DriverProfile.save() called ===")
        logger.error(f"Instance ID: {self.id}")
        logger.error(f"Driver ID: {self.driver_id}")
        logger.error(f"Version Number: {self.version_number}")
        logger.error(f"Status: {self.status}")
        logger.error(f"Is Current: {self.is_current}")
        logger.error(f"Args: {args}")
        logger.error(f"Kwargs: {kwargs}")
        logger.error(f"Is new instance: {is_new}")
        
        # Check for existing profiles with same driver_id and version_number
        if self.driver_id and self.version_number:
            existing_profiles = DriverProfile.objects.filter(
                driver_id=self.driver_id,
                version_number=self.version_number
            )
            if not is_new:
                existing_profiles = existing_profiles.exclude(id=self.id)
            
            logger.error(f"Pre-save check: Found {existing_profiles.count()} existing profiles with driver_id={self.driver_id}, version_number={self.version_number}")
            
            for profile in existing_profiles:
                logger.error(f"Conflicting profile: ID={profile.id}, driver_id={profile.driver_id}, version={profile.version_number}, status={profile.status}")
        
        try:
            # Call parent save
            result = super().save(*args, **kwargs)
            logger.error(f"DriverProfile.save() completed successfully")
            return result
        except Exception as e:
            logger.error(f"DriverProfile.save() failed with error: {e}")
            logger.error(f"Error type: {type(e)}")
            
            # If it's an IntegrityError, let's check the database state again
            if 'UNIQUE constraint failed' in str(e):
                logger.error(f"=== IntegrityError detected - checking database state ===")
                
                post_error_profiles = DriverProfile.objects.filter(
                    driver_id=self.driver_id,
                    version_number=self.version_number
                )
                logger.error(f"Post-error check: Found {post_error_profiles.count()} profiles with driver_id={self.driver_id}, version_number={self.version_number}")
                
                for profile in post_error_profiles:
                    logger.error(f"Existing profile after error: ID={profile.id}, driver_id={profile.driver_id}, version={profile.version_number}, status={profile.status}")
            
            raise


class DriverProfileChange(models.Model):
    """Track changes between profile versions for comparison"""
    id = models.UUIDField(primary_key=True, default=uuid.uuid4, editable=False)
    old_profile = models.ForeignKey(DriverProfile, on_delete=models.CASCADE, related_name='changes_from', null=True, blank=True)
    new_profile = models.ForeignKey(DriverProfile, on_delete=models.CASCADE, related_name='changes_to')
    field_name = models.CharField(max_length=100)
    old_value = models.TextField(null=True, blank=True)
    new_value = models.TextField(null=True, blank=True)
    change_type = models.CharField(max_length=20, choices=[
        ('added', 'Added'),
        ('modified', 'Modified'),
        ('removed', 'Removed'),
    ])
    created_at = models.DateTimeField(auto_now_add=True)
    
    class Meta:
        ordering = ['-created_at']
    
    def __str__(self):
        return f"{self.new_profile.driver.name} - {self.field_name} {self.change_type}"


# Activity tracking helper functions
def log_driver_activity(driver, activity_type, activity_data=None, request=None):
    """Helper function to log driver activities"""
    ip_address = None
    user_agent = None
    
    if request:
        ip_address = request.META.get('REMOTE_ADDR')
        user_agent = request.META.get('HTTP_USER_AGENT')
    
    DriverActivity.objects.create(
        driver=driver,
        activity_type=activity_type,
        activity_data=activity_data or {},
        ip_address=ip_address,
        user_agent=user_agent
    )


def get_driver_activity_heatmap(driver, year=None):
    """Generate heatmap data for driver activities"""
    if year is None:
        year = timezone.now().year
    
    activities = DriverActivity.objects.filter(
        driver=driver,
        created_at__year=year
    ).extra(
        select={'date': 'DATE(created_at)'}
    ).values('date').annotate(
        count=models.Count('id')
    ).order_by('date')
    
    # Convert to format suitable for frontend heatmap
    heatmap_data = {}
    for activity in activities:
        # activity['date'] is already a string from DATE(created_at) SQL function
        date_str = str(activity['date']) if activity['date'] else ''
        if date_str:
            heatmap_data[date_str] = activity['count']
    
    return heatmap_data


def get_driver_activity_stats(driver, days=30):
    """Get driver activity statistics for the last N days"""
    start_date = timezone.now() - timedelta(days=days)
    
    activities = DriverActivity.objects.filter(
        driver=driver,
        created_at__gte=start_date
    )
    
    total_activities = activities.count()
    
    # Group by activity type
    activity_breakdown = activities.values('activity_type').annotate(
        count=models.Count('id')
    ).order_by('-count')
    
    # Get last activity
    last_activity = activities.first()
    
    # Calculate daily average
    daily_average = total_activities / days if days > 0 else 0
    
    return {
        'total_activities': total_activities,
        'daily_average': round(daily_average, 2),
        'activity_breakdown': list(activity_breakdown),
        'last_activity': last_activity.created_at if last_activity else None,
        'period_days': days
    }