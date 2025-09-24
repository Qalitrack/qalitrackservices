import uuid
from django.db import models
from django.contrib.auth.models import AbstractUser
from django.db.models.signals import post_save
from django.dispatch import receiver
from django.utils import timezone


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
            name=f"{instance.first_name} {instance.last_name}".strip() or instance.username,
            phone='',
            license_number=''
        )