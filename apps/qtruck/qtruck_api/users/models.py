import uuid
from django.db import models
from django.contrib.auth.models import AbstractUser, BaseUserManager
from django.db.models.signals import post_save
from django.dispatch import receiver
from django.utils import timezone


class BaseModel(models.Model):
    id = models.UUIDField(primary_key=True, default=uuid.uuid4, editable=False)
    created_at = models.DateTimeField(auto_now_add=True)
    updated_at = models.DateTimeField(auto_now=True)

    class Meta:
        abstract = True


class CustomUserManager(BaseUserManager):
    """Custom user manager for email-based authentication"""
    
    def create_user(self, email, password=None, **extra_fields):
        """Create and save a regular user with the given email and password"""
        if not email:
            raise ValueError('The Email field must be set')
        email = self.normalize_email(email)
        
        # Handle first user logic here instead of in save()
        if not self.model.objects.exists():
            # First user becomes admin and approved
            extra_fields['user_type'] = 'admin'
            extra_fields['status'] = 'approved' 
            extra_fields['is_staff'] = True
            extra_fields['is_superuser'] = True
        else:
            # For subsequent users, check auto-approval settings
            # Extract user type from email alias
            email_parts = email.split('@')
            if '+' in email_parts[0]:
                alias_part = email_parts[0].split('+')[1].lower()
                if alias_part in ['admin', 'driver', 'tester']:
                    user_type = alias_part
                    
                    try:
                        from settings.models import SystemSettings
                        # Try to get any existing settings first, then fall back to get_settings()
                        settings = SystemSettings.objects.first()
                        if not settings:
                            settings = SystemSettings.get_settings()
                        auto_approve_types = settings.auto_approve_user_types if settings else []
                        
                        if user_type in auto_approve_types:
                            extra_fields['status'] = 'approved'
                        else:
                            extra_fields['status'] = 'preapproval'
                    except Exception:
                        if user_type == 'admin':
                            extra_fields['status'] = 'approved'
                        else:
                            extra_fields['status'] = 'preapproval'
        
        user = self.model(email=email, **extra_fields)
        # Set flags to indicate this was handled by the manager
        user._user_type_set_by_manager = True
        user._status_set_by_manager = True
        user.set_password(password)
        user.save(using=self._db)
        return user
    
    def create_superuser(self, email, password=None, **extra_fields):
        """Create and save a superuser with the given email and password"""
        extra_fields.setdefault('is_staff', True)
        extra_fields.setdefault('is_superuser', True)
        extra_fields.setdefault('status', 'approved')
        
        if extra_fields.get('is_staff') is not True:
            raise ValueError('Superuser must have is_staff=True.')
        if extra_fields.get('is_superuser') is not True:
            raise ValueError('Superuser must have is_superuser=True.')
        
        return self.create_user(email, password, **extra_fields)


class CustomUser(AbstractUser):
    USER_TYPE_CHOICES = [
        ('admin', 'Admin'),
        ('driver', 'Driver'),
        ('tester', 'Tester'),
    ]
    
    STATUS_CHOICES = [
        ('preapproval', 'Pre-approval'),
        ('approved', 'Approved'),
        ('rejected', 'Rejected'),
    ]
    
    id = models.UUIDField(primary_key=True, default=uuid.uuid4, editable=False)
    email = models.EmailField(unique=True)
    user_type = models.CharField(max_length=10, choices=USER_TYPE_CHOICES, default='driver')
    status = models.CharField(max_length=15, choices=STATUS_CHOICES, default='preapproval')
    created_at = models.DateTimeField(auto_now_add=True)
    updated_at = models.DateTimeField(auto_now=True)
    
    # Remove username requirement - use email as primary identifier
    username = None  # Remove username field
    USERNAME_FIELD = 'email'
    REQUIRED_FIELDS = []  # Remove username from required fields
    
    # Use custom manager
    objects = CustomUserManager()
    
    @property
    def full_name(self):
        """Return user's full name"""
        if self.first_name or self.last_name:
            return f"{self.first_name} {self.last_name}".strip()
        # Fallback to base email (without alias)
        return self.base_email.split('@')[0]
    
    @property
    def is_approved(self):
        """Backward compatibility property"""
        return self.status == 'approved'
    
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
        
        # Extract user type from email alias (if not already set by create_user)
        if not hasattr(self, '_user_type_set_by_manager'):
            email_parts = self.email.split('@')
            if '+' in email_parts[0]:
                local_part = email_parts[0].split('+')
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
        
            # For new non-first users, check auto-approval settings
            is_new_user = self.pk is None
            if is_new_user and not hasattr(self, '_status_set_by_manager'):
                try:
                    from settings.models import SystemSettings
                    settings = SystemSettings.get_settings()
                    auto_approve_types = settings.auto_approve_user_types if settings else []
                    
                    if self.user_type in auto_approve_types:
                        self.status = 'approved'
                    else:
                        self.status = 'preapproval'
                except Exception:
                    if self.user_type == 'admin':
                        self.status = 'approved'
                    else:
                        self.status = 'preapproval'
        
        super().save(*args, **kwargs)


class UserProfile(BaseModel):
    user = models.OneToOneField(CustomUser, on_delete=models.CASCADE, related_name='profile')
    approval_date = models.DateTimeField(null=True, blank=True)
    approved_by = models.ForeignKey(CustomUser, on_delete=models.SET_NULL, null=True, blank=True, related_name='approved_users')




# Auto-create user profile
@receiver(post_save, sender=CustomUser)
def create_user_profile(sender, instance, created, **kwargs):
    if created:
        UserProfile.objects.create(user=instance)


# Auto-create driver object for driver users
@receiver(post_save, sender=CustomUser)
def create_driver_object(sender, instance, created, **kwargs):
    if created and instance.user_type == 'driver':
        # Import here to avoid circular imports
        from drivers.models import Driver
        Driver.objects.create(
            user=instance,
            name=instance.full_name,
            phone='',
            license_number=''
        )